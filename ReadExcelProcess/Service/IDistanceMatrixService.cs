using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public interface IDistanceMatrixService
    {
        Task<double[,]> GetTravelTimeMatrix(List<string> addressList);
    }
}