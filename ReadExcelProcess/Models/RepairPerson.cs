using System.ComponentModel.DataAnnotations;

namespace ReadExcelProcess.Models
{
    public class RepairPerson
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Assignment> assignments { get; set; }
        public decimal TotalWorkTime { get; set; }
    }
}
