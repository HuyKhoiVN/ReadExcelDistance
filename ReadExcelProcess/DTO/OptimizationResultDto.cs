namespace ReadExcelProcess.DTO
{
    public class OptimizationResultDto
    {
        public string status { get; set; }
        public List<ScheduledAssignmentDto> assignments { get; set; }
    }

    public class ScheduledAssignmentDto
    {
        public int worker { get; set; }
        public List<int> tasks { get; set; }
        public decimal total_time { get; set; }
    }

    public class InputTest
    {
        public List<int> work_times { get; set; }
        public int[,] travel_times { get; set; }
        public int num_workers { get; set; }
        public int delta { get; set; }
    }
}