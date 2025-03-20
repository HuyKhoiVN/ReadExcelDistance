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
        private readonly IAssignmentService _assignmentService;
        private readonly IDistanceMatrixService _distanceMatrixService;

        public ExcelService(
            IHttpClientFactory httpClientFactory,
            IAssignmentService assignmentService,
            IDistanceMatrixService distanceMatrixService)
        {
            _httpClientFactory = httpClientFactory;
            _assignmentService = assignmentService;
            _distanceMatrixService = distanceMatrixService;
        }

        /// <summary>
        /// Lấy data excel trả về list thời gian sửa chữa và mảng 2 chiều thời gian di chuyển
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public (List<double> MaintenanceTimes, List<List<double>> TravelTimes) GetExcelData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx).");

            _assignmentService.LoadAssignmentsFromExcel(file);

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);

            var sheet1 = package.Workbook.Worksheets[0];
            if (sheet1 == null)
                throw new ArgumentException("Sheet1 is missing from the file.");

            int rowCount = sheet1.Dimension.Rows;
            List<double> maintenanceTimes = new();
            for (int i = 1; i <= rowCount; i++)
            {
                double value = sheet1.Cells[i, 2].GetValue<double>();
                maintenanceTimes.Add(Math.Round(value, 2));
            }

            var sheet2 = package.Workbook.Worksheets[1];
            if (sheet2 == null)
                throw new ArgumentException("Sheet2 is missing from the file.");

            int size = sheet2.Dimension.Rows;
            List<List<double>> travelTimes = new();
            for (int i = 0; i < size; i++)
            {
                List<double> row = new();
                for (int j = 0; j < size; j++)
                {
                    double value = sheet2.Cells[i + 1, j + 1].GetValue<double>();
                    row.Add(Math.Round(value, 2));
                }
                travelTimes.Add(row);
            }

            return (maintenanceTimes, travelTimes);
        }

        private (List<string> Addresses, List<double> MaintenanceTimes) ExtractDataFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx). ");

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];
            if (sheet == null)
                throw new ArgumentException("Sheet is missing from the file.");

            int rowCount = sheet.Dimension.Rows;
            List<string> addresses = new();
            List<double> maintenanceTimes = new();

            for (int i = 1; i <= rowCount; i++)
            {
                string address = sheet.Cells[i, 1].GetValue<string>();
                double time = sheet.Cells[i, 2].GetValue<double>();

                addresses.Add(address);
                maintenanceTimes.Add(Math.Round(time, 2));
            }

            return (addresses, maintenanceTimes);
        }

        public async Task<string> GenerateTravelTimeExcel(IFormFile file)
        {
            var (addressList, _) = ExtractDataFromExcel(file);

            double[,] travelTimeMatrix = await _distanceMatrixService.GetTravelTimeMatrix(addressList);
            string fileName = $"{Guid.NewGuid().ToString("N")}.xlsx";
            string filePath = Path.Combine("wwwroot", fileName);

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("TravelTimes");

            int size = addressList.Count;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    sheet.Cells[i + 1, j + 1].Value = Math.Round(travelTimeMatrix[i, j], 4);
                }
            }

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            package.SaveAs(fileStream);

            return fileName;
        }

        public async Task<byte[]> GetFile(IFormFile file)
        {
            var (maintenanceTimes, travelTimes) = GetExcelData(file);
            List<int> workTimes = maintenanceTimes.Select(x => (int)Math.Round(x)).ToList();

            int rows = travelTimes.Count;
            int cols = rows > 0 ? travelTimes[0].Count : 0;
            int[,] travelArray = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    travelArray[i, j] = (int)Math.Round(travelTimes[i][j]);
                }
            }
            InputTest inputTest = new InputTest
            {
                work_times = maintenanceTimes.ToArray(),
                travel_times = travelTimes.Select(x => x.ToArray()).ToArray(),
                num_workers = 3,
                delta = 1
            };
            var client = _httpClientFactory.CreateClient();
            var requestUri = "http://10.14.117.15:8000/optimize";
            var payload = JsonConvert.SerializeObject(inputTest);
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var optimizationResult = JsonConvert.DeserializeObject<OptimizationResultDto>(json);

            if (optimizationResult == null || optimizationResult.assignments == null)
                return null;

            var list = new List<RepairPerson>();
            foreach (var scheduled in optimizationResult.assignments)
            {
                var repairPerson = new RepairPerson
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Worker {scheduled.worker}",
                    TotalWorkTime = scheduled.total_time,
                    assignments = new List<Assignment>()
                };

                foreach (var taskId in scheduled.tasks)
                {
                    var assignmentDetail = _assignmentService.GetAssignmentByTaskId(taskId);
                    if (assignmentDetail != null)
                    {
                        var assignment = new Assignment
                        {
                            Id = Guid.NewGuid().ToString(),
                            Location = assignmentDetail.Location,
                            RepairTime = assignmentDetail.RepairTime,
                            RepairPersonId = repairPerson.Id
                        };
                        repairPerson.assignments.Add(assignment);
                    }
                }
                list.Add(repairPerson);
            }

            return await Export(list);
        }

        public async Task<byte[]> Export(List<RepairPerson> model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            foreach (var repairPerson in model)
            {
                string sheetName = repairPerson.Name;
                if (sheetName.Length > 31)
                    sheetName = sheetName.Substring(0, 31);

                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                worksheet.Cells[1, 1].Value = "Vị trí";
                worksheet.Cells[1, 2].Value = "Thời gian sửa chữa";
                int row = 2;
                decimal totalRepairTime = 0;
                if (repairPerson.assignments != null && repairPerson.assignments.Count > 0)
                {
                    foreach (var assignment in repairPerson.assignments)
                    {
                        worksheet.Cells[row, 1].Value = assignment.Location;
                        worksheet.Cells[row, 2].Value = assignment.RepairTime;
                        totalRepairTime += assignment.RepairTime;
                        row++;
                    }
                }

                worksheet.Cells[row + 1, 1].Value = "Tổng thời gian di chuyển";
                decimal totalTravelTime = repairPerson.TotalWorkTime - totalRepairTime;
                worksheet.Cells[row + 1, 2].Value = totalTravelTime;
                worksheet.Cells[row + 2, 1].Value = "Tổng thời gian làm việc";
                worksheet.Cells[row + 2, 2].Value = repairPerson.TotalWorkTime;

                worksheet.Cells.AutoFitColumns();
            }
            return package.GetAsByteArray();
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