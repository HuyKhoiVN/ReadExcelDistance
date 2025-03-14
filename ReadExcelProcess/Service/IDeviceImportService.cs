namespace ReadExcelProcess.Service
{
    public interface IDeviceImportService
    {
        Task<(int totalAdded, List<int> addedIds)> ImportDevicesFromExcel(IFormFile file);
    }
}
