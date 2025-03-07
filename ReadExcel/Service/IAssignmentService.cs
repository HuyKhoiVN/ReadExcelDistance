using ReadExcel.Models;

namespace ReadExcel.Service
{
    public interface IAssignmentService
    {
        Assignment GetAssignmentByTaskId(int taskId);
    }
}
