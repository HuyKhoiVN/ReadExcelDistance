using ReadExcelProcess.Models;

namespace ReadExcelProcess.Service
{
    public interface IAssignmentService
    {
        Assignment GetAssignmentByTaskId(int taskId);
    }
}
