namespace ReadExcelProcess.Service
{
    public interface IContractImportService
    {
        Task<List<int>> ImportContractsAsync(IFormFile file);
    }
}
