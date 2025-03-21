using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ReadExcelProcess.DTO;
using ReadExcelProcess.Models;
using System.Drawing;

namespace ReadExcelProcess.Service
{
    public class ExcelService : IExcelService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistanceMatrixService _distanceMatrixService;

        public ExcelService(
            IHttpClientFactory httpClientFactory,
            IDistanceMatrixService distanceMatrixService)
        {
            _httpClientFactory = httpClientFactory;
            _distanceMatrixService = distanceMatrixService;
        }

      
        public async Task<byte[]> ExportDivisionDay(InputData inputData)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return await Task.Run(() =>
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Data");
                    worksheet.Cells[1, 1].Value = "Ngày";
                    worksheet.Cells[1, 2].Value = "Tên Nhân Viên";
                    worksheet.Cells[1, 3].Value = "Địa Chỉ";
                    worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

                    worksheet.Cells[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    worksheet.Cells[1, 1, 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells.AutoFitColumns();

                    int row = 2;
                    foreach (var division in inputData.divisionDays)
                    {
                        foreach (var task in division.emplTasks)
                        {
                            foreach (var location in task.taskLocations)
                            {
                                worksheet.Cells[row, 1].Value = division.date.ToString("dd-MM-yyyy");
                                worksheet.Cells[row, 2].Value = task.emplName;
                                worksheet.Cells[row, 3].Value = location;
                                row++;
                            }
                        }
                    }
                    return package.GetAsByteArray();
                }
            });
        }
    }
}