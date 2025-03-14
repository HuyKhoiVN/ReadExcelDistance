using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReadExcelProcess.Model;

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

        public async Task<(int totalAdded, List<int> addedIds)> ImportDevicesFromExcel(IFormFile file)
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
            int totalAdded = 0;
            List<int> addedIds = new List<int>();

            // Lấy danh sách SerialNumber từ file
            var serialNumbers = Enumerable.Range(2, rowCount - 1)
                .Select(row => worksheet.Cells[row, 9].Text)
                .Where(sn => !string.IsNullOrWhiteSpace(sn))
                .ToList();

            // Lấy toàn bộ thiết bị có SerialNumber tương ứng
            var existingDevices = await _dbContext.Devices
                .Where(d => serialNumbers.Contains(d.SerialNumber))
                .ToDictionaryAsync(d => d.SerialNumber);

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var serialNumber = worksheet.Cells[row, 9].Text;
                    if (string.IsNullOrWhiteSpace(serialNumber)) continue; // Bỏ qua nếu không có SerialNumber

                    var startDateCell = worksheet.Cells[row, 16].Text.Trim();
                    var endDateCell = worksheet.Cells[row, 17].Text.Trim();

                    var device = new Device
                    {
                        SerialNumber = serialNumber,
                        Customer = worksheet.Cells[row, 2].Text,
                        ContractNumber = string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text) ? null : worksheet.Cells[row, 3].Text,
                        SubContractNumber = string.IsNullOrWhiteSpace(worksheet.Cells[row, 4].Text) ? null : worksheet.Cells[row, 4].Text,
                        ManagementBranch = worksheet.Cells[row, 5].Text,
                        Address = worksheet.Cells[row, 6].Text,
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
                        Latitude = null,
                        Longitude = null,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = "SystemAdmin",
                        CreatedDate = DateTime.Now
                    };

                    // Kiểm tra xem SerialNumber đã tồn tại chưa
                    if (!existingDevices.TryGetValue(serialNumber, out var existingDevice))
                    {
                        await _dbContext.Devices.AddAsync(device);
                        totalAdded++;
                        addedIds.Add(device.Id); // Lưu luôn Id sau khi thêm mới
                    }
                    else
                    {
                        // Cập nhật thông tin cần thiết
                        existingDevice.Customer = device.Customer;
                        existingDevice.Address = device.Address;
                        existingDevice.UpdatedBy = "SystemAdmin";
                        existingDevice.UpdatedDate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi dòng {row}: {ex.Message}");
                    continue;
                }
            }

            await _dbContext.SaveChangesAsync();

            return (totalAdded, addedIds);
        }

        public async Task UpdateDeviceCoordinatesFromExcelAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var serialNumbers = new List<string>();
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var serialNumber = worksheet.Cells[row, 9].Text.Trim();
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    serialNumbers.Add(serialNumber);
                }
            }

            var devicesToUpdate = _dbContext.Devices
                .Where(d => serialNumbers.Contains(d.SerialNumber))
                .ToList();

            foreach (var device in devicesToUpdate)
            {
                var location = await _geoCodingService.GetCoordinatesAsync(device.Address);
                if (location != null)
                {
                    device.Latitude = location.Latitude;
                    device.Longitude = location.Longitude;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

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
