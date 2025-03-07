using System.ComponentModel.DataAnnotations;

namespace ReadExcel.Models
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
