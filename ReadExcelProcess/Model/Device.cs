using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Class { get; set; } = null!;
        public string Family { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string Contact { get; set; } = null!;
        public string? DeviceIdNumber { get; set; }
        public string? Address { get; set; }
        public string Province { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string Zone { get; set; } = null!;
        public string? Support1 { get; set; }
        public string? Support2 { get; set; }
        public string DeviceStatus { get; set; } = null!;
        public DateTime? LastChange { get; set; }
        public string? ContractNumber { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ProvinceCode { get; set; } = null!;
        public string SubContractNumber { get; set; } = null!;
    }
}
