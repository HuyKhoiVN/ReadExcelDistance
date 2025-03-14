using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReadExcelProcess.Model;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public class DeviceImportService : IDeviceImportService
    {
        private readonly SysDbContext _dbContext;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IDistanceMatrixService _distanceMatrixService;

        public DeviceImportService(SysDbContext dbContext, IGeoCodingService geoCodingService, IDistanceMatrixService distanceMatrixService)
        {
            _dbContext = dbContext;
            _geoCodingService = geoCodingService;
            _distanceMatrixService = distanceMatrixService;
        }

        public async Task<List<int>> ImportDevicesFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx).");

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
                throw new ArgumentException("Sheet1 is missing from the file.");

            int rowCount = worksheet.Dimension.Rows;
            var newDevices = new List<Device>();

            var serialNumbers = worksheet.Cells[2, 9, rowCount, 9]
                .Select(cell => cell.Text.Trim())
                .Where(sn => !string.IsNullOrWhiteSpace(sn))
                .Distinct()
                .ToList();

            // Lấy toàn bộ thiết bị đã tồn tại
            var existingDevices = await _dbContext.Devices
                .Where(d => serialNumbers.Contains(d.SerialNumber))
                .ToDictionaryAsync(d => d.SerialNumber);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var serialNumber = worksheet.Cells[row, 9].Text;
                        if (string.IsNullOrWhiteSpace(serialNumber)) continue; // Bỏ qua nếu không có SerialNumber

                        var startDateCell = worksheet.Cells[row, 16].Text.Trim();
                        var endDateCell = worksheet.Cells[row, 17].Text.Trim();
                        var address = worksheet.Cells[row, 6].Text;

                        Location location;
                        try
                        {
                            location = await _geoCodingService.GetCoordinatesAsync(address) ?? new Location { Latitude = 0, Longitude = 0 };
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Lỗi lấy tọa độ dòng {row}");
                            continue; // Bỏ qua dòng lỗi
                        }

                        var device = new Device
                        {
                            SerialNumber = serialNumber,
                            Customer = worksheet.Cells[row, 2].Text,
                            ContractNumber = string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text) ? null : worksheet.Cells[row, 3].Text,
                            SubContractNumber = string.IsNullOrWhiteSpace(worksheet.Cells[row, 4].Text) ? null : worksheet.Cells[row, 4].Text,
                            ManagementBranch = worksheet.Cells[row, 5].Text,
                            Address = address,
                            Province = worksheet.Cells[row, 7].Text,
                            Area = worksheet.Cells[row, 8].Text,
                            Model = worksheet.Cells[row, 11].Text,
                            Type = worksheet.Cells[row, 12].Text,
                            Manufacturer = worksheet.Cells[row, 13].Text,
                            DeviceStatus = worksheet.Cells[row, 14].Text,
                            MaintenanceCycle = worksheet.Cells[row, 15].Text,
                            TimeMaintenance = 2,
                            MaintenanceStartDate = ParseDateFromExcel(startDateCell, row),
                            MaintenanceEndDate = ParseDateFromExcel(endDateCell, row),
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = "SystemAdmin",
                            CreatedDate = DateTime.Now
                        };

                        // Kiểm tra xem SerialNumber đã tồn tại chưa
                        if (existingDevices.TryGetValue(serialNumber, out var existingDevice))
                        {
                            if (existingDevice.IsDeleted) continue;

                            // Cập nhật thông tin cần thiết
                            existingDevice.Address = device.Address;
                            existingDevice.Latitude = device.Latitude;
                            existingDevice.Longitude = device.Longitude;
                            existingDevice.UpdatedBy = "SystemAdmin";
                            existingDevice.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            await _dbContext.Devices.AddAsync(device);
                        }
                        newDevices.Add(device);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi dòng {row}: {ex.Message}");
                        continue;
                    }
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Lỗi import thiết bị: {ex.Message}");
                throw;
            }


            return newDevices.Select(x => x.Id).ToList();
        }

        public async Task ImportTravelTimeDevice(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx).");

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
                throw new ArgumentException("Sheet1 is missing from the file.");

            int rowCount = worksheet.Dimension.Rows;
            var newDevices = new List<Device>();

            var serialNumbers = worksheet.Cells[2, 9, rowCount, 9]
                .Select(cell => cell.Text.Trim())
                .Where(sn => !string.IsNullOrWhiteSpace(sn))
                .Distinct()
                .ToList();

            // Lấy toàn bộ thiết bị đã tồn tại
            var devices = await _dbContext.Devices
                                .Where(d => serialNumbers.Contains(d.SerialNumber) && !d.IsDeleted)
                                .ToListAsync();

            var locations = devices.Select(d => new Location
            {
                Latitude = d.Latitude ?? 0,
                Longitude = d.Longitude ?? 0
            }).ToList();

            int n = locations.Count;
            double[,] matrix = new double[n, n];
            List<DeviceTravelTime> deviceTravels = new List<DeviceTravelTime>();

            for (int i = 0; i < n; i++)
            {
                matrix[i, i] = 0;

                if (i < n - 1)
                {
                    var origin = locations[i];
                    var destinations = locations.Skip(i + 1).ToList();

                    try
                    {
                        var distanceMatrix = await _distanceMatrixService.GetTravelTime(origin, destinations);
                        List<double> travelTimes = new();

                        if (distanceMatrix?.Rows?.Count > 0)
                        {
                            foreach (var element in distanceMatrix.Rows[0].Elements)
                            {
                                double durationInHours = element.Status == "OK" ? element.Duration.Value / 3600.0 : double.MaxValue;
                                travelTimes.Add(durationInHours);
                            }
                        }

                        for (int j = 0; j < destinations.Count; j++)
                        {
                            int destIndex = i + 1 + j;
                            matrix[i, destIndex] = matrix[destIndex, i] = travelTimes[j];

                            // Tạo DeviceTravelTime
                            var travelTimeEntry = new DeviceTravelTime
                            {
                                DeviceId1 = devices[i].Id,
                                DeviceId2 = devices[destIndex].Id,
                                TravelTime = (decimal)travelTimes[j],
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = "System",
                                CreatedDate = DateTime.UtcNow
                            };
                            deviceTravels.Add(travelTimeEntry);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi gọi API lấy thời gian từ {origin.Latitude},{origin.Longitude}: {ex.Message}");
                        for (int j = i + 1; j < n; j++)
                        {
                            matrix[i, j] = matrix[j, i] = double.MaxValue;
                        }
                    }
                    await Task.Delay(500);
                }
            }

            // Thêm vào DB
            if (deviceTravels.Count > 0)
            {
                await _dbContext.DeviceTravelTimes.AddRangeAsync(deviceTravels);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Parse date excel from text d/m/y to datetime in sql
        /// </summary>
        /// <param name="dateText">date value in string</param>
        /// <param name="row">row need to change</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private DateTime ParseDateFromExcel(string dateText, int row)
        {
            var parts = dateText.Split('/');
            if (parts.Length != 3 ||
                !int.TryParse(parts[0], out int day) ||
                !int.TryParse(parts[1], out int month) ||
                !int.TryParse(parts[2], out int year))
            {
                throw new ArgumentException($"Ngày không hợp lệ tại dòng {row}:: {dateText}");
            }

            try
            {
                return new DateTime(year, month, day);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Ngày không tồn tại tại dòng {row}: {dateText}");
            }
        }
    }
}
