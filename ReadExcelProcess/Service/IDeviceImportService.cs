namespace ReadExcelProcess.Service
{
    public interface IDeviceImportService
    {
        Task<List<int>> ImportDevicesFromExcel(IFormFile file);
        Task<List<int>> ImportCoordinateAndTravelTime(string provinceCode);
    }
}
