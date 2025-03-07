using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReadExcel.Service;

namespace ReadExcel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairerController : ControllerBase
    {
        private readonly IOptimizationService _optimizationService;
        private readonly IExportService _exportService;

        public RepairerController(IOptimizationService optimizationService, IExportService exportService)
        {
            _optimizationService = optimizationService;
            _exportService = exportService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportOptimizationData()
        {
            await _optimizationService.ProcessOptimizationDataAsync();
            return Ok(new { message = "Dữ liệu từ API optimize đã được import và lưu trữ thành công." });
        }
        [HttpGet("export")]
        public IActionResult ExportRepairData()
        {
            var filePath = _exportService.ExportRepairDataToExcel();
            return Ok(new { message = "Export thành công", filePath });
        }
    }
}
