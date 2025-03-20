namespace ReadExcelProcess.DTO
{
    public class InputData
    {
        public List<DivisionDay> divisionDays { get; set; }
    }
    public class DivisionDay
    {
        public DateTime date { get; set; }
        public List<EmplTask> emplTasks { get; set; }
    }

    public class EmplTask
    {
        public int emplId { get; set; }
        public string emplName { get; set; }
        public List<string> taskLocations { get; set; }
    }
}
