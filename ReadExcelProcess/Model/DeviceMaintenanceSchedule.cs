using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class DeviceMaintenanceSchedule
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public DateTime MaintenanceStartDate { get; set; }
        public DateTime MaintenanceEndDate { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int MaintenanceTimes { get; set; }
        public string? ContractNumber { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
