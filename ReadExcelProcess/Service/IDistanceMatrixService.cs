using ReadExcelProcess.DTO;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public interface IDistanceMatrixService
    {
        Task<double[,]> GetTravelTimeMatrix(List<string> addressList);
        Task<DistanceMatrixResponse> GetTravelTime(Location origin, List<Location> destinations);
    }
}