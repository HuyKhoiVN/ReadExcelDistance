using OfficeOpenXml;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public class AssignmentService : IAssignmentService
    {
        private readonly Dictionary<int, Assignment> _assignmentMapping;

        public AssignmentService()
        {
            _assignmentMapping = new Dictionary<int, Assignment>();
        }

        public void LoadAssignmentsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File Excel không hợp lệ hoặc rỗng.");
            }

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("Không tìm thấy worksheet nào trong file Excel.");

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

        public Assignment GetAssignmentByTaskId(int taskId)
        {
            return _assignmentMapping.TryGetValue(taskId, out var assignment) ? assignment : null;
        }
    }

}
