namespace ReadExcelProcess.Service
{
    public interface IProvinceService
    {
        Task<List<int>> ImportProvincesFromExcelAsync(IFormFile file);
    }
}
