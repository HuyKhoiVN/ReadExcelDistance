using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class DeviceTravelTime
    {
        public int Id { get; set; }
        public int DeviceId1 { get; set; }
        public int DeviceId2 { get; set; }
        public decimal TravelTime { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
