using System.ComponentModel.DataAnnotations;

namespace ReadExcel.Models
{
    public class Assignment
    {
        [Key]
        public string Id { get; set; }
        public string Location { get; set; }
        public decimal RepairTime { get; set; } 
        public string RepairPersonId { get; set; }
    }
}