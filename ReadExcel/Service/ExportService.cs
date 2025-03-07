using OfficeOpenXml;
using ReadExcel.Repositories;

namespace ReadExcel.Service
{
    public class ExportService : IExportService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IRepairPersonRepository _repairPersonRepository;

        public ExportService(IWebHostEnvironment env, IRepairPersonRepository repairPersonRepository)
        {
            _env = env;
            _repairPersonRepository = repairPersonRepository;
        }

        public string ExportRepairDataToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var repairPersons = _repairPersonRepository.GetRepairPersons();
                foreach (var repairPerson in repairPersons)
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

                string exportFolder = Path.Combine(_env.WebRootPath, "Export");
                if (!Directory.Exists(exportFolder))
                {
                    Directory.CreateDirectory(exportFolder);
                }

                string fileName = $"RepairData_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(exportFolder, fileName);

                package.SaveAs(new FileInfo(filePath));
                return filePath;
            }
        }
    }
}
