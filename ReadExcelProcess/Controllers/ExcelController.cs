using Microsoft.AspNetCore.Mvc;
using ReadExcelProcess.Service;

namespace ReadExcelProcess.Controllers
{
    [Route("[controller]")]
    public class ExcelController : Controller
    {
        private readonly IExcelService _excelService;
        private readonly IDistanceMatrixService _distanceMatrixService;
        private readonly IDeviceImportService _deviceImportService;
        private readonly IProvinceService _provinceService;
        private readonly IOfficerImportService _officerImportService;

        public ExcelController(IExcelService excelService, IDistanceMatrixService distanceMatrixService, IDeviceImportService deviceImportService,
            IProvinceService provinceService, IOfficerImportService officerImportService)
        {
            _distanceMatrixService = distanceMatrixService;
            _excelService = excelService;
            _deviceImportService = deviceImportService;
            _provinceService = provinceService;
            _officerImportService = officerImportService;
        }

        [HttpGet("home")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("time")]
        public IActionResult TimeWork()
        {
            return View();
        }

        [HttpPost("api/data")]
        public IActionResult GetExcelData(IFormFile file)
        {
            try
            {
                var (maintenanceTimes, travelTimes) = _excelService.GetExcelData(file);
                return Ok(new { MaintenanceTimes = maintenanceTimes, TravelTimes = travelTimes });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("api/import")]
        public async Task<IActionResult> ImportOptimizationData(IFormFile file)
        {
            var data = await _excelService.GetFile(file);
            return File(
                    data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Report.xlsx"
                );
        }

        [HttpPost("api/generate-travel-time-matrix")]
        public async Task<IActionResult> GenerateTravelTimeMatrix(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            // Gọi function để tạo file Excel mới chứa ma trận tọa độ
            string generatedFileName = await _excelService.GenerateTravelTimeExcel(file);

            string filePath = Path.Combine("wwwroot", generatedFileName);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            // Đặt tên file để AJAX có thể lấy
            Response.Headers["X-File-Name"] = generatedFileName;

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", generatedFileName);
        }

        [HttpPost("api/GetDistanceMatrix")]
        public async Task<IActionResult> GetDistanceMatrix([FromBody] List<string> addressList)
        {
            if (addressList == null || addressList.Count < 2)
                return BadRequest("Cần ít nhất 2 điểm để tính khoảng cách.");

            var matrix = await _distanceMatrixService.GetTravelTimeMatrix(addressList);

            // Chuyển mảng 2D sang danh sách List<List<double>>
            var result = ConvertMatrixToList(matrix);

            return Ok(result);
        }

        [HttpPost("api/import-devices")]
        public async Task<IActionResult> ImportDevicesFromExcel(IFormFile file)
        {
            try
            {
                var result = await _deviceImportService.ImportDevicesFromExcel(file);

                return Ok(new
                {
                    Status = "Success",
                    Total = result.Count,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }

        [HttpPost("api/import-officer")]
        public async Task<IActionResult> ImportOfficerFromExcel(IFormFile file)
        {
            try
            {
                var result = await _officerImportService.ImportOfficerFromExcel(file);

                return Ok(new
                {
                    Status = "Success",
                    Total = result.Count,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }

        [HttpPost("api/import-provinces")]
        public async Task<IActionResult> ImportProvinceFromExcel(IFormFile file)
        {
            try
            {
                var result = await _provinceService.ImportProvincesFromExcelAsync(file);

                return Ok(new
                {
                    Status = "Success",
                    Total = result.Count,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }

        private List<List<double>> ConvertMatrixToList(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var list = new List<List<double>>(rows);

            for (int i = 0; i < rows; i++)
            {
                var row = new List<double>(cols);
                for (int j = 0; j < cols; j++)
                {
                    row.Add(matrix[i, j]);
                }
                list.Add(row);
            }

            return list;
        }
    }
}