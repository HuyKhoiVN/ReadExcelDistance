namespace ReadExcelProcess.Service
{
    public interface IOfficerImportService
    {
        public Task<List<int>> ImportOfficerFromExcel(IFormFile file);
    }
}
