﻿using Microsoft.AspNetCore.Mvc;
using ReadExcelProcess.DTO;
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
        private readonly IContractImportService _contractImportService;
        private readonly IDeviceMaintenanceService _deviceMaintenanceService;

        public ExcelController(IExcelService excelService, IDistanceMatrixService distanceMatrixService, IDeviceImportService deviceImportService,
            IProvinceService provinceService, IOfficerImportService officerImportService, IContractImportService contractImportService, IDeviceMaintenanceService deviceMaintenanceService)
        {
            _distanceMatrixService = distanceMatrixService;
            _excelService = excelService;
            _deviceImportService = deviceImportService;
            _provinceService = provinceService;
            _officerImportService = officerImportService;
            _contractImportService = contractImportService;
            _deviceMaintenanceService = deviceMaintenanceService;
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

        [HttpPost("api/import-travel-time")]
        public async Task<IActionResult> ImportCoordinateAndTravelTime(string supportCode)
        {
            try
            {
                var result = await _deviceImportService.ImportCoordinateAndTravelTime(supportCode);

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

        [HttpPost("api/import-province-travel-time")]
        public async Task<IActionResult> ImportProvinceTravel([FromBody] ProvinceTravelDto model)
        {
            if (model == null || model.SupportCodes == null || model.SupportCodes.Count == 0)
            {
                return BadRequest("Danh sách SupportCode không hợp lệ");
            }

            var result = await _deviceImportService.ImportTravelTimeProvince(model.SupportCodes, model.ProvinceId);

            if (result.Count == 0)
            {
                return NotFound("Không tìm thấy thiết bị hoặc tỉnh thành phù hợp");
            }

            return Ok(new { Message = "Import thành công " + model.ProvinceId.ToString(), Total = result.Count(), Data = result });
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

        [HttpPost("api/import-contracts")]
        public async Task<IActionResult> ImportContractFromExcel(IFormFile file)
        {
            try
            {
                var result = await _contractImportService.ImportContractsAsync(file);

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

        [HttpPost("generate-date")]
        public async Task<IActionResult> GenerateMaintenanceSchedules()
        {
            try
            {
                await _deviceMaintenanceService.GenerateMaintenanceSchedulesAsync();
                return Ok("Lịch bảo trì đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPost("api/export-divisionDay")]
        public async Task<IActionResult> ExportExcel([FromBody] InputData inputData)
        {
            var fileContent = await _excelService.ExportDivisionDay(inputData);

            string fileName = $"DivisionDay_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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