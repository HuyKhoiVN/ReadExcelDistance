using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public interface IGeoCodingService
    {
        Task<Location> GetCoordinatesAsync(string address);
    }
}