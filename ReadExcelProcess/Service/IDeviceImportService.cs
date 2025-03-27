namespace ReadExcelProcess.Service
{
    public interface IDeviceImportService
    {
        Task<List<int>> ImportDevicesFromExcel(IFormFile file);
        Task<List<int>> ImportCoordinateAndTravelTime(string supportCode);
        Task<List<int>> ImportTravelTimeProvince(List<string> supportCodes, int provinceId);
    }
}
