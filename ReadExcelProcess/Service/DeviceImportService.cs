using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReadExcelProcess.Constant;
using ReadExcelProcess.Model;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public class DeviceImportService : IDeviceImportService
    {
        private readonly SysDbContext _dbContext;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IDistanceMatrixService _distanceMatrixService;
        private readonly ILogger<DeviceImportService> _logger;

        public DeviceImportService(SysDbContext dbContext, IGeoCodingService geoCodingService, IDistanceMatrixService distanceMatrixService, ILogger<DeviceImportService> logger)
        {
            _dbContext = dbContext;
            _geoCodingService = geoCodingService;
            _distanceMatrixService = distanceMatrixService;
            _logger = logger;
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

            var worksheet = package.Workbook.Worksheets["ATM"];
            if (worksheet == null)
                throw new ArgumentException("Sheet ATM is missing from the file.");

            int rowCount = worksheet.Dimension.Rows;
            var newDevices = new List<Device>();

            var serialNumbers = worksheet.Cells[2, 4, rowCount, 4]
                .Select(cell => cell.Text.Trim())
                .Where(sn => !string.IsNullOrWhiteSpace(sn))
                .Distinct()
                .ToList();

            var existingDevices = await _dbContext.Devices
                .Where(d => serialNumbers.Contains(d.SerialNumber))
                .ToDictionaryAsync(d => d.SerialNumber);

            // Import devices and set coordinates to (0, 0)
            foreach (int row in Enumerable.Range(2, rowCount - 1))
            {
                try
                {
                    var serialNumber = worksheet.Cells[row, 4].Text;
                    if (string.IsNullOrWhiteSpace(serialNumber)) continue;

                    string provinceName = worksheet.Cells[row, 9].Text.Trim();

                    var device = new Device
                    {
                        Name = worksheet.Cells[row, 1].Text,
                        Class = worksheet.Cells[row, 2].Text,
                        Family = worksheet.Cells[row, 3].Text,
                        SerialNumber = serialNumber,
                        Contact = worksheet.Cells[row, 5].Text,
                        DeviceIdNumber = worksheet.Cells[row, 6].Text.Trim(),
                        Address = worksheet.Cells[row, 7].Text.Trim(),
                        Province = provinceName,
                        ProvinceCode = CommonFunction.ConvertToCode(provinceName),
                        Area = worksheet.Cells[row, 8].Text,
                        Zone = worksheet.Cells[row, 10].Text,
                        Support1 = worksheet.Cells[row, 11].Text,
                        Support2 = worksheet.Cells[row, 12].Text,
                        DeviceStatus = worksheet.Cells[row, 13].Text,
                        LastChange = CommonFunction.ConvertToDateTime(worksheet.Cells[row, 14].Text.Trim(), row),
                        ContractNumber = worksheet.Cells[row, 15].Text,
                        SubContractNumber = worksheet.Cells[row, 16].Text,    
                        Latitude = 0,
                        Longitude = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = "SystemAdmin",
                        CreatedDate = DateTime.Now
                    };

                    if (existingDevices.TryGetValue(serialNumber, out var existingDevice))
                    {
                        continue;
                    }
                    else
                    {
                        newDevices.Add(device);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi dòng {row}: {ex.Message}");
                    continue;
                }
            }
            // Calculate travel times

            await _dbContext.AddRangeAsync(newDevices);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Hoàn tất import và xử lý tọa độ!");
            return newDevices.Select(x => x.Id).ToList();
        }

        public async Task<List<int>> ImportCoordinateAndTravelTime(string supportCode)
        {
            List<Device> devices = await _dbContext.Devices.Where(d => d.Support1 == supportCode).ToListAsync();

            if(devices.Any())
            {
                await UpdateDeviceCoordinates(devices);
                await CalculateTravelTimes(devices);
            }

            return devices.Select(x => x.Id).ToList();
        }

        public async Task<List<int>> ImportTravelTimeProvince(List<string> supportCodes, int provinceId)
        {
            // Lấy danh sách thiết bị có Support1 khớp 100% với mã trong danh sách supportCodes
            List<Device> devices = await _dbContext.Devices
                .Where(d => supportCodes.Any(code => d.Support1.Trim() == code))
                .ToListAsync();

            // Lấy thông tin tỉnh
            var province = await _dbContext.Provinces.FirstOrDefaultAsync(x => x.Id == provinceId);

            if (devices.Count < 1 || province == null)
            {
                return new List<int>();
            }

            var origin = new Location { Latitude = province.Latitude, Longitude = province.Longitude };
            var destinations = devices
                .Select(d => new Location { Latitude = d.Latitude ?? 0, Longitude = d.Longitude ?? 0 })
                .ToList();

            // Gọi API tính thời gian di chuyển
            var distanceMatrix = await _distanceMatrixService.GetTravelTime(origin, destinations);
            await Task.Delay(250);

            var travelTimes = new List<ProvinceTravelTime>();
            if (distanceMatrix?.Rows?.Count > 0)
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    var travelTime = distanceMatrix.Rows[0].Elements[i].Status == "OK"
                        ? distanceMatrix.Rows[0].Elements[i].Duration.Value / 3600.0
                        : double.MaxValue;

                    travelTimes.Add(new ProvinceTravelTime
                    {
                        ProvinceId = province.Id,
                        DeviceId = device.Id,
                        TravelTime = (decimal)travelTime,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = "SystemAdmin",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // Lưu dữ liệu vào DB
            if (travelTimes.Any())
            {
                await _dbContext.ProvinceTravelTimes.AddRangeAsync(travelTimes);
                await _dbContext.SaveChangesAsync();
            }
            return travelTimes.Select(x => x.Id).ToList();
        }


        private async Task<List<int>> UpdateDeviceCoordinates(List<Device> devices)
        {
            var deviceFail = new List<Device>();
            foreach (var device in devices)
            {
                try
                {
                    var location = await _geoCodingService.GetCoordinatesAsync(device.Address) ?? new Location { Latitude = 0, Longitude = 0 };
                    device.Latitude = location.Latitude;
                    device.Longitude = location.Longitude;
                    await _dbContext.SaveChangesAsync();
                    await Task.Delay(250);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Lỗi cập nhật tọa độ cho thiết bị: {device.SerialNumber}");
                    deviceFail.Add(device);
                }
            }
            return deviceFail.Select(x => x.Id).ToList();
        }

        private async Task CalculateTravelTimes(List<Device> devicesWithCoordinates)
        {          
            var devicePairs = new HashSet<(int, int)>();
            foreach (var origin in devicesWithCoordinates)
            {
                var destinations = devicesWithCoordinates.Where(d => d.Id != origin.Id).ToList();
                var distanceMatrix = await _distanceMatrixService.GetTravelTime(
                    new Location { Latitude = origin.Latitude ?? 0, Longitude = origin.Longitude ?? 0 },
                    destinations.Select(d => new Location { Latitude = d.Latitude ?? 0, Longitude = d.Longitude ?? 0 }).ToList()
                );
                await Task.Delay(250);

                for (int j = 0; j < destinations.Count; j++)
                {
                    var destination = destinations[j];
                    var (id1, id2) = origin.Id < destination.Id ? (origin.Id, destination.Id) : (destination.Id, origin.Id);
                    if (devicePairs.Contains((id1, id2))) continue;

                    devicePairs.Add((id1, id2));

                    var durationInHours = distanceMatrix.Rows[0].Elements[j].Status == "OK"
                        ? distanceMatrix.Rows[0].Elements[j].Duration.Value / 3600.0
                        : 0.001;

                    var travelTime = new DeviceTravelTime
                    {
                        DeviceId1 = id1,
                        DeviceId2 = id2,
                        TravelTime = (decimal)durationInHours,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    };

                    await _dbContext.DeviceTravelTimes.AddAsync(travelTime);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}