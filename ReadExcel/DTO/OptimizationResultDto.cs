namespace ReadExcel.DTO
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
}
