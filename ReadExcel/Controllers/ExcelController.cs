using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReadExcel.Service;

namespace ReadExcel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly ExcelService _excelService;

        public ExcelController()
        {
            _excelService = new ExcelService();
        }

        [HttpPost("data")]
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
    }
}
