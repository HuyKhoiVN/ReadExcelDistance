using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReadExcelProcess.Model;

namespace ReadExcelProcess.Service
{
    public class OfficerImportService : IOfficerImportService
    {
        private readonly SysDbContext _dbContext;

        public OfficerImportService(SysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<int>> ImportOfficerFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Vui lòng tải lên file Excel (.xlsx).");

            var newOfficers = new List<Officer>();
            using var stream = file.OpenReadStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
                throw new ArgumentException("Sheet is missing from the file.");
            int rowCount = worksheet.Dimension.Rows;
            var cccdList = worksheet.Cells[2, 4, rowCount, 4]
                .Select(cell => cell.Text.Trim())
                .Where(cccd => !string.IsNullOrWhiteSpace(cccd))
                .Distinct()
                .ToList();
            var existingOfficers = await _dbContext.Officers
                .Where(o => cccdList.Contains(o.Cccd))
                .ToDictionaryAsync(o => o.Cccd);
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                string currentRegion = null;
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var firstCell = worksheet.Cells[row, 1].Text.Trim();
                        if (!int.TryParse(firstCell, out _))
                        {
                            if (!string.IsNullOrWhiteSpace(firstCell) &&
                                firstCell.StartsWith("Miền", StringComparison.OrdinalIgnoreCase))
                            {
                                currentRegion = firstCell;
                            }
                            continue;
                        }

                        string fullName = worksheet.Cells[row, 2].Text.Trim();
                        if (string.IsNullOrWhiteSpace(fullName))
                            continue;

                        string title = worksheet.Cells[row, 3].Text.Trim();
                        string cccd;
                        var cccdCell = worksheet.Cells[row, 4];
                        if (cccdCell.Value is double numericValue)
                        {
                            cccd = numericValue.ToString("F0");
                        }
                        else
                        {
                            cccd = cccdCell.Text.Trim();
                        }
                        string dateOfIssueCell = worksheet.Cells[row, 5].Text.Trim();
                        string placeOfIssue = worksheet.Cells[row, 6].Text.Trim();
                        string account = worksheet.Cells[row, 7].Text.Trim();
                        string branch = worksheet.Cells[row, 8].Text.Trim();

                        DateTime? dateOfIssue = null;
                        if (DateTime.TryParse(dateOfIssueCell, out DateTime parsedDate))
                            dateOfIssue = parsedDate;

                        var officer = new Officer
                        {
                            FullName = fullName,
                            Title = title,
                            Cccd = cccd,
                            DateOfIssue = dateOfIssue,
                            PlaceOfIssue = placeOfIssue,
                            Account = account,
                            Branch = branch,
                            Region = currentRegion
                        };

                        if (existingOfficers.TryGetValue(cccd, out var existingOfficer))
                        {
                            existingOfficer.FullName = fullName;
                            existingOfficer.Title = title;
                            existingOfficer.DateOfIssue = dateOfIssue;
                            existingOfficer.PlaceOfIssue = placeOfIssue;
                            existingOfficer.Account = account;
                            existingOfficer.Branch = branch;
                            existingOfficer.Region = currentRegion;
                            officer = existingOfficer;
                        }
                        else
                        {
                            await _dbContext.Officers.AddAsync(officer);
                        }

                        newOfficers.Add(officer);
                        await Task.Delay(100);
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
                Console.WriteLine($"Lỗi import officer: {ex.Message}");
                throw;
            }
            return newOfficers.Select(o => o.Id).ToList();
        }
    }
}