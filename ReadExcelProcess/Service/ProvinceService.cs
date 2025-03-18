using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReadExcelProcess.Model;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public class ProvinceService : IProvinceService
    {
        private readonly SysDbContext _dbContext;
        private readonly IDistanceMatrixService _distanceMatrixService;
        private readonly IGeoCodingService _geoCodingService;

        public ProvinceService(SysDbContext dbContext, IDistanceMatrixService distanceMatrixService, IGeoCodingService geoCodingService)
        {
            _dbContext = dbContext;
            _distanceMatrixService = distanceMatrixService;
            _geoCodingService = geoCodingService;
        }

        public async Task<List<int>> ImportProvincesFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("File không hợp lệ hoặc không đúng định dạng (.xlsx).");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
                throw new ArgumentException("Sheet1 is missing from the file.");

            int rowCount = worksheet.Dimension.Rows;
            List<Province> provinces = new();

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Bước 1: Đọc và lưu dữ liệu từ file Excel
                for (int row = 4; row <= rowCount; row += 5)
                {
                    try
                    {
                        string provinceName = worksheet.Cells[row, 1].Text?.Trim();
                        string address = worksheet.Cells[row + 1, 2].Text?.Trim();
                        string phone = worksheet.Cells[row + 2, 2].Text?.Trim();
                        string fax = worksheet.Cells[row + 3, 2].Text?.Trim();

                        if (string.IsNullOrEmpty(provinceName)) continue;

                        // Gọi API lấy tọa độ
                        Location location = await _geoCodingService.GetCoordinatesAsync(address) ?? new Location { Latitude = 0, Longitude = 0 };
                        await Task.Delay(250);

                        var province = new Province
                        {
                            ProvinceName = provinceName,
                            Address = address,
                            Phone = phone,
                            Fax = fax,
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = "SystemAdmin",
                            CreatedDate = DateTime.Now
                        };

                        provinces.Add(province);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi tại dòng {row}: {ex.Message}");
                        continue;
                    }
                }

                await _dbContext.Provinces.AddRangeAsync(provinces);
                await _dbContext.SaveChangesAsync();

                // Bước 2: Tính thời gian di chuyển giữa Province và Device
                foreach (var province in provinces)
                {
                    var provinceVariations = new List<string> { province.ProvinceName.ToLower() };
                    if (province.ProvinceName.ToLower() == "hà nội")
                        provinceVariations.Add("hn");
                    if (province.ProvinceName.ToLower() == "hcm")
                        provinceVariations.AddRange(new[] { "hồ chí minh", "tp hcm", "tp hồ chí minh" });

                    var devicesInProvince = await _dbContext.Devices
                        .Where(d => provinceVariations.Contains(d.Province.ToLower()) && !d.IsDeleted)
                        .ToListAsync();

                    if (!devicesInProvince.Any()) continue;

                    var origin = new Location { Latitude = province.Latitude, Longitude = province.Longitude };
                    var destinations = devicesInProvince
                        .Select(d => new Location { Latitude = d.Latitude ?? 0, Longitude = d.Longitude ?? 0 })
                        .ToList();

                    // Gọi API tính thời gian di chuyển
                    var distanceMatrix = await _distanceMatrixService.GetTravelTime(origin, destinations);
                    await Task.Delay(250);

                    var travelTimes = new List<ProvinceTravelTime>();
                    if (distanceMatrix?.Rows?.Count > 0)
                    {
                        for (int i = 0; i < devicesInProvince.Count; i++)
                        {
                            var device = devicesInProvince[i];
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

                    if (travelTimes.Any())
                    {
                        await _dbContext.ProvinceTravelTimes.AddRangeAsync(travelTimes);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                Console.WriteLine($"Đã nhập thành công {provinces.Count} tỉnh và tính thời gian di chuyển.");

                // Lấy danh sách ID Province vừa thêm
                var provinceIds = await _dbContext.Provinces
                    .Where(p => provinces.Select(x => x.ProvinceName).Contains(p.ProvinceName))
                    .Select(p => p.Id)
                    .ToListAsync();

                return provinceIds;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Lỗi khi xử lý file: {ex.Message}");
                throw;
            }
        }

    }
}
