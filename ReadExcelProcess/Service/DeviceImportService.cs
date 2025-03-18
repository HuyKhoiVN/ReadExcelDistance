using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReadExcelProcess.Model;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    //public class DeviceImportService : IDeviceImportService
    //{
    //    private readonly SysDbContext _dbContext;
    //    private readonly IGeoCodingService _geoCodingService;
    //    private readonly IDistanceMatrixService _distanceMatrixService;
    //    private readonly ILogger<DeviceImportService> _logger;

    //    public DeviceImportService(SysDbContext dbContext, IGeoCodingService geoCodingService, IDistanceMatrixService distanceMatrixService, ILogger<DeviceImportService> logger)
    //    {
    //        _dbContext = dbContext;
    //        _geoCodingService = geoCodingService;
    //        _distanceMatrixService = distanceMatrixService;
    //        _logger = logger;
    //    }

    //    public async Task<List<int>> ImportDevicesFromExcel(IFormFile file)
    //    {
    //        if (file == null || file.Length == 0)
    //            throw new ArgumentException("File không hợp lệ.");

    //        if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
    //            throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx).");

    //        using var stream = new MemoryStream();
    //        file.CopyTo(stream);
    //        stream.Position = 0;

    //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    //        using var package = new ExcelPackage(stream);

    //        var worksheet = package.Workbook.Worksheets[0];
    //        if (worksheet == null)
    //            throw new ArgumentException("Sheet1 is missing from the file.");

    //        int rowCount = worksheet.Dimension.Rows;
    //        var newDevices = new List<Device>();

    //        var serialNumbers = worksheet.Cells[2, 9, rowCount, 9]
    //            .Select(cell => cell.Text.Trim())
    //            .Where(sn => !string.IsNullOrWhiteSpace(sn))
    //            .Distinct()
    //            .ToList();

    //        // Lấy toàn bộ thiết bị đã tồn tại
    //        var existingDevices = await _dbContext.Devices
    //            .Where(d => serialNumbers.Contains(d.SerialNumber))
    //            .ToDictionaryAsync(d => d.SerialNumber);

    //        using var transaction = await _dbContext.Database.BeginTransactionAsync();
    //        try
    //        {
    //            for (int row = 2; row <= rowCount; row++)
    //            {
    //                try
    //                {
    //                    var serialNumber = worksheet.Cells[row, 3].Text;
    //                    if (string.IsNullOrWhiteSpace(serialNumber)) continue; // Bỏ qua nếu không có SerialNumber

    //                    var startDateCell = worksheet.Cells[row, 16].Text.Trim();
    //                    var endDateCell = worksheet.Cells[row, 17].Text.Trim();
    //                    var address = worksheet.Cells[row, 6].Text;

    //                    Location location;
    //                    try
    //                    {
    //                        location = await _geoCodingService.GetCoordinatesAsync(address) ?? new Location { Latitude = 0, Longitude = 0 };
    //                        await Task.Delay(250);
    //                    }
    //                    catch (Exception)
    //                    {
    //                        Console.WriteLine($"Lỗi lấy tọa độ dòng {row}");
    //                        continue; // Bỏ qua dòng lỗi
    //                    }

    //                    var device = new Device
    //                    {
    //                        SerialNumber = serialNumber,
    //                        ContractNumber = string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text) ? null : worksheet.Cells[row, 3].Text,
    //                        Address = address,
    //                        Province = worksheet.Cells[row, 7].Text,
    //                        Area = worksheet.Cells[row, 8].Text,
    //                        Model = worksheet.Cells[row, 11].Text,
    //                        Type = worksheet.Cells[row, 12].Text,
    //                        Manufacturer = worksheet.Cells[row, 13].Text,
    //                        DeviceStatus = worksheet.Cells[row, 14].Text,
    //                        MaintenanceCycle = worksheet.Cells[row, 15].Text,
    //                        TimeMaintenance = 2,
    //                        MaintenanceStartDate = ParseDateFromExcel(startDateCell, row),
    //                        MaintenanceEndDate = ParseDateFromExcel(endDateCell, row),
    //                        Latitude = location.Latitude,
    //                        Longitude = location.Longitude,
    //                        IsActive = true,
    //                        IsDeleted = false,
    //                        CreatedBy = "SystemAdmin",
    //                        CreatedDate = DateTime.Now
    //                    };

    //                    // Kiểm tra xem SerialNumber đã tồn tại chưa
    //                    if (existingDevices.TryGetValue(serialNumber, out var existingDevice))
    //                    {
    //                        if (existingDevice.IsDeleted) continue;

    //                        // Cập nhật thông tin cần thiết
    //                        existingDevice.Address = device.Address;
    //                        existingDevice.Latitude = device.Latitude;
    //                        existingDevice.Longitude = device.Longitude;
    //                        existingDevice.UpdatedBy = "SystemAdmin";
    //                        existingDevice.UpdatedDate = DateTime.Now;
    //                    }
    //                    else
    //                    {
    //                        await _dbContext.Devices.AddAsync(device);
    //                    }

    //                    newDevices.Add(device);
    //                    await Task.Delay(250);

    //                    await _dbContext.SaveChangesAsync();

    //                    int frequencyMonths = 3;
    //                    if (!string.IsNullOrWhiteSpace(device.MaintenanceCycle))
    //                    {
    //                        if (!int.TryParse(device.MaintenanceCycle, out frequencyMonths))
    //                        {
    //                            frequencyMonths = 3;
    //                        }
    //                    }

    //                    DateTime currentStart = device.MaintenanceStartDate;
    //                    DateTime currentEnd = device.MaintenanceEndDate;
    //                    DateTime limitDate = new DateTime(currentStart.Year, 12, 31);

    //                    int maintenanceTimes = 1;
    //                    while (currentStart <= limitDate)
    //                    {
    //                        var schedule = new DeviceMaintenanceSchedule
    //                        {
    //                            DeviceId = device.Id,
    //                            MaintenanceStartDate = currentStart,
    //                            MaintenanceEndDate = currentEnd,
    //                            MaintenanceTimes = maintenanceTimes,
    //                        };

    //                        await _dbContext.DeviceMaintenanceSchedules.AddAsync(schedule);

    //                        maintenanceTimes++;
    //                        currentStart = currentStart.AddMonths(frequencyMonths);
    //                        currentEnd = currentEnd.AddMonths(frequencyMonths);
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine($"Lỗi dòng {row}: {ex.Message}");
    //                    continue;
    //                }
    //            }
    //            await _dbContext.SaveChangesAsync();

    //            // GỌI HÀM REIMPORT NẾU CÒN THIẾU TOẠ ĐỘ
    //            if (newDevices.Any(d => d.Latitude == 0 || d.Longitude == 0))
    //            {
    //                _logger.LogInformation("Bắt đầu quá trình retry lấy tọa độ...");
    //                await ReImportDeviceCoordinate(newDevices);
    //            }

    //            _logger.LogInformation("Hoàn tất import và xử lý tọa độ!");

    //            // Lấy tất cả thiết bị có tọa độ từ DB (bao gồm cả thiết bị mới)
    //            var devicesWithCoordinates = await _dbContext.Devices
    //                .Where(d => d.Latitude != 0 && d.Longitude != 0 && !d.IsDeleted)
    //                .ToListAsync();

    //            var deviceTravels = new List<DeviceTravelTime>();

    //            // Nhóm thiết bị theo tỉnh (không phân biệt hoa thường)
    //            var devicesByProvince = devicesWithCoordinates
    //                .GroupBy(d => d.Province.ToLower().Trim())
    //                .ToList();

    //            foreach (var provinceGroup in devicesByProvince)
    //            {
    //                var devicesInProvince = provinceGroup.ToList();
    //                int n = devicesInProvince.Count;
    //                if (n < 2) continue;

    //                // Chỉ tính thời gian di chuyển giữa các thiết bị trong cùng tỉnh
    //                for (int i = 0; i < n; i++)
    //                {
    //                    var origin = devicesInProvince[i];
    //                    var destinations = devicesInProvince.Skip(i + 1).ToList();

    //                    // Gọi API lấy thời gian di chuyển từ thiết bị "origin" tới tất cả thiết bị còn lại trong cùng tỉnh
    //                    var distanceMatrix = await _distanceMatrixService.GetTravelTime(
    //                        new Location { Latitude = origin.Latitude ?? 0, Longitude = origin.Longitude ?? 0 },
    //                        destinations.Select(d => new Location { Latitude = d.Latitude ?? 0, Longitude = d.Longitude ?? 0 }).ToList()
    //                    );

    //                    await Task.Delay(250); // Delay tránh vượt giới hạn API

    //                    if (distanceMatrix?.Rows?.Count > 0)
    //                    {
    //                        for (int j = 0; j < destinations.Count; j++)
    //                        {
    //                            var destinationDevice = destinations[j];

    //                            var durationInHours = distanceMatrix.Rows[0].Elements[j].Status == "OK"
    //                                ? distanceMatrix.Rows[0].Elements[j].Duration.Value / 3600.0
    //                                : double.MaxValue;

    //                            // Sắp xếp id để tránh trùng lặp cặp (id1, id2) và (id2, id1)
    //                            var (id1, id2) = origin.Id < destinationDevice.Id
    //                                            ? (origin.Id, destinationDevice.Id)
    //                                            : (destinationDevice.Id, origin.Id);

    //                            // Kiểm tra xem cặp này đã tồn tại trong DB chưa
    //                            var isExistInDb = await _dbContext.DeviceTravelTimes
    //                                .AnyAsync(dt => dt.DeviceId1 == id1 && dt.DeviceId2 == id2);

    //                            if (!isExistInDb)
    //                            {
    //                                deviceTravels.Add(new DeviceTravelTime
    //                                {
    //                                    DeviceId1 = id1,
    //                                    DeviceId2 = id2,
    //                                    TravelTime = (decimal)durationInHours,
    //                                    IsActive = true,
    //                                    IsDeleted = false,
    //                                    CreatedBy = "System",
    //                                    CreatedDate = DateTime.UtcNow
    //                                });
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            // Lưu dữ liệu nếu có chuyến đi mới
    //            if (deviceTravels.Count > 0)
    //            {
    //                await _dbContext.DeviceTravelTimes.AddRangeAsync(deviceTravels);
    //                await _dbContext.SaveChangesAsync();
    //            }

    //            await transaction.CommitAsync();

    //        }
    //        catch (Exception ex)
    //        {
    //            await transaction.RollbackAsync();
    //            Console.WriteLine($"Lỗi import thiết bị: {ex.Message}");
    //            throw;
    //        }

    //        return newDevices.Select(x => x.Id).ToList();
    //    }

    //    /// <summary>
    //    /// Re check if device coordinate is null or == 0
    //    /// </summary>
    //    /// <param name="devices">list device need to check</param>
    //    /// <returns></returns>
    //    private async Task ReImportDeviceCoordinate(List<Device> devices)
    //    {
    //        int maxRetries = 3;

    //        for (int attempt = 1; attempt <= maxRetries; attempt++)
    //        {
    //            var devicesNeedRetry = devices.Where(d => d.Latitude == 0 || d.Longitude == 0).ToList();
    //            if (devicesNeedRetry.Count == 0) break; 

    //            _logger.LogWarning($"Retry lần {attempt} - Thiết bị cần lấy lại tọa độ: {devicesNeedRetry.Count}");

    //            foreach (var device in devicesNeedRetry)
    //            {
    //                try
    //                {
    //                    var updatedLocation = await _geoCodingService.GetCoordinatesAsync(device.Address);
    //                    if (updatedLocation != null && updatedLocation.Latitude != 0 && updatedLocation.Longitude != 0)
    //                    {
    //                        device.Latitude = updatedLocation.Latitude;
    //                        device.Longitude = updatedLocation.Longitude;
    //                        _logger.LogInformation($" Đã cập nhật tọa độ cho thiết bị {device.SerialNumber}");
    //                    }
    //                    else
    //                    {
    //                        _logger.LogWarning($" Không lấy được tọa độ mới cho thiết bị {device.SerialNumber}");
    //                    }

    //                    // Delay tránh vượt giới hạn API
    //                    await Task.Delay(250);
    //                }
    //                catch (Exception ex)
    //                {
    //                    _logger.LogError($"Lỗi khi retry lấy tọa độ cho thiết bị {device.SerialNumber}: {ex.Message}");
    //                }
    //            }

    //            // Lưu DB sau mỗi lần retry
    //            await _dbContext.SaveChangesAsync();

    //            if (devicesNeedRetry.Count == 0)
    //            {
    //                _logger.LogInformation("Tất cả thiết bị đã cập nhật đủ tọa độ!");
    //                break;
    //            }
    //        }

    //        // Nếu retry 3 lần vẫn còn thiết bị lỗi
    //        if (devices.Any(d => d.Latitude == 0 || d.Longitude == 0))
    //        {
    //            _logger.LogError("Sau 3 lần retry, vẫn còn thiết bị chưa lấy được tọa độ!");
    //        }
    //    }

    //    /// <summary>
    //    /// Parse date excel from text d/m/y to datetime in sql
    //    /// </summary>
    //    /// <param name="dateText">date value in string</param>
    //    /// <param name="row">row need to change</param>
    //    /// <returns></returns>
    //    /// <exception cref="ArgumentException"></exception>
    //    private DateTime ParseDateFromExcel(string dateText, int row)
    //    {
    //        var parts = dateText.Split('/');
    //        if (parts.Length != 3 ||
    //            !int.TryParse(parts[0], out int day) ||
    //            !int.TryParse(parts[1], out int month) ||
    //            !int.TryParse(parts[2], out int year))
    //        {
    //            throw new ArgumentException($"Ngày không hợp lệ tại dòng {row}:: {dateText}");
    //        }

    //        try
    //        {
    //            return new DateTime(year, month, day);
    //        }
    //        catch (Exception)
    //        {
    //            throw new ArgumentException($"Ngày không tồn tại tại dòng {row}: {dateText}");
    //        }
    //    }
    //}
}