using Newtonsoft.Json;
using OfficeOpenXml;
using ReadExcel.DTO;
using ReadExcel.Models;

namespace ReadExcel.Service
{
    public class ExcelService : IExcelService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAssignmentService _assignmentService;

        public ExcelService(
            IHttpClientFactory httpClientFactory,
            IAssignmentService assignmentService)
        {
            _httpClientFactory = httpClientFactory;
            _assignmentService = assignmentService;
        }

        public (List<double> MaintenanceTimes, List<List<double>> TravelTimes) GetExcelData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or not provided.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Invalid file format. Please upload an Excel file (.xlsx).");

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
                work_times = workTimes,
                travel_times = travelArray,
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

                worksheet.Cells[1, 1].Value = "Location";
                worksheet.Cells[1, 2].Value = "RepairTime";

                int row = 2;
                if (repairPerson.assignments != null)
                {
                    foreach (var assignment in repairPerson.assignments)
                    {
                        worksheet.Cells[row, 1].Value = assignment.Location;
                        worksheet.Cells[row, 2].Value = assignment.RepairTime;
                        row++;
                    }
                }
                worksheet.Cells[row + 1, 1].Value = "TotalWorkTime";
                worksheet.Cells[row + 1, 2].Value = repairPerson.TotalWorkTime;
            }
            return package.GetAsByteArray();
        }
    }
}