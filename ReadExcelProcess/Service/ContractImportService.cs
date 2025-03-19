using OfficeOpenXml;
using ReadExcelProcess.Model;

namespace ReadExcelProcess.Service
{
    public class ContractImportService : IContractImportService
    {
        private readonly SysDbContext _dbContext;
        public ContractImportService(SysDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
        public async Task<List<int>> ImportContractsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx).");

            var newContracts = new List<Contract>();

            using var stream = file.OpenReadStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0]; 
            if (worksheet == null)
                throw new ArgumentException("Sheet is missing from the file.");

            int rowCount = worksheet.Dimension.Rows; 

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string customerName = worksheet.Cells[row, 1].Text.Trim();
                        if (string.IsNullOrWhiteSpace(customerName))
                        {
                            continue;
                        }

                        string contractNumberParent = worksheet.Cells[row, 2].Text.Trim();

                        string contractNumberChildren = worksheet.Cells[row, 3].Text.Trim();

                        string timeMaintenanceCell = worksheet.Cells[row, 4].Text.Trim();
                        int? timeMaintenance = null;
                        if (!string.IsNullOrEmpty(timeMaintenanceCell)
                            && int.TryParse(timeMaintenanceCell, out int tm))
                        {
                            timeMaintenance = tm;
                        }

                        string startDateCell = worksheet.Cells[row, 5].Text.Trim();
                        DateTime? startDate = null;
                        if (!string.IsNullOrEmpty(startDateCell)
                            && DateTime.TryParse(startDateCell, out DateTime sd))
                        {
                            startDate = sd;
                        }
                        string endDateCell = worksheet.Cells[row, 6].Text.Trim();
                        DateTime? endDate = null;
                        if (!string.IsNullOrEmpty(endDateCell)
                            && DateTime.TryParse(endDateCell, out DateTime ed))
                        {
                            endDate = ed;
                        }
                        var contract = new Contract
                        {
                            CustomerName = customerName,
                            ContractNumberParent = contractNumberParent,
                            ContractNumberChildren = contractNumberChildren,
                            TimeMaintenance = timeMaintenance,
                            StartDate = startDate,
                            EndDate = endDate
                        };

                        await _dbContext.Contracts.AddAsync(contract);
                        newContracts.Add(contract);


                        await Task.Delay(150); 
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi dòng {row}: {ex.Message}");
                        continue;
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Lỗi import contracts: {ex.Message}");
                throw;
            }
            return newContracts.Select(c => c.Id).ToList();
        }
    }
}
