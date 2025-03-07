using OfficeOpenXml;
using ReadExcel.Models;

namespace ReadExcel.Service
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IWebHostEnvironment _env;
        private readonly Dictionary<int, Assignment> _assignmentMapping;

        public AssignmentService(IWebHostEnvironment env)
        {
            _env = env;
            _assignmentMapping = new Dictionary<int, Assignment>();
            LoadAssignmentsFromExcel();
        }

        private void LoadAssignmentsFromExcel()
        {
            var filePath = Path.Combine(_env.WebRootPath, "Danh_sach_ATM_no_headers.xlsx");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Không tìm thấy file Danh_sach_ATM_no_headers.xlsx trong wwwroot");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new Exception("Không tìm thấy worksheet nào trong file Excel");

                int row = 1;
                while (worksheet.Cells[row, 1].Value != null)
                {
                    try
                    {
                        string location = worksheet.Cells[row, 1].Value.ToString();
                        decimal repairTime = decimal.Parse(worksheet.Cells[row, 2].Value.ToString());

                        var assignment = new Assignment
                        {
                            Id = Guid.NewGuid().ToString(),
                            Location = location,
                            RepairTime = repairTime,
                            RepairPersonId = null  
                        };

                        _assignmentMapping[row] = assignment;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi tại dòng {row}: {ex.Message}");
                    }
                    row++;
                }
            }
        }


        public Assignment GetAssignmentByTaskId(int taskId)
        {
            _assignmentMapping.TryGetValue(taskId, out var assignment);
            return assignment;
        }
    }
}
