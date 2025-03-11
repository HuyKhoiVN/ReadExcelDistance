using Microsoft.AspNetCore.Mvc;
using ReadExcelProcess.Models;
using ReadExcelProcess.Service;

namespace ReadExcelProcess.Controllers
{
    [Route("[controller]")]
    public class ExcelController : Controller
    {
        private readonly IExcelService _excelService;
        private readonly IDistanceMatrixService _distanceMatrixService;

        public ExcelController(IExcelService excelService)
        {
            _excelService = excelService;
        }

        [HttpGet("home")]
        public IActionResult Index()
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

        [HttpGet]
        public async Task<IActionResult> GetDistanceMatrix([FromQuery] List<string> addressList)
        {
            if (addressList == null || addressList.Count < 2)
                return BadRequest("Cần ít nhất 2 điểm để tính khoảng cách.");

            var matrix = await _distanceMatrixService.GetTravelTimeMatrix(addressList);

            // Chuyển mảng 2D sang danh sách List<List<double>>
            var result = ConvertMatrixToList(matrix);

            return Ok(result);
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