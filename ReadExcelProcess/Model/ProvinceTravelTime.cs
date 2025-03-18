using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class ProvinceTravelTime
    {
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public int DeviceId { get; set; }
        public decimal TravelTime { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
