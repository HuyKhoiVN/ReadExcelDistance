using ReadExcelProcess.DTO;
using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public interface IExcelService
    {
        Task<byte[]> ExportDivisionDay(InputData inputData);
    }
}