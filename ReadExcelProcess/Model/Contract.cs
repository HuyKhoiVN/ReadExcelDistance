using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class Contract
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? ContractNumberParent { get; set; }
        public string? ContractNumberChildren { get; set; }
        public int? TimeMaintenance { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
