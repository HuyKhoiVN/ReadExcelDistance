using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class Device
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = null!;
        public string Customer { get; set; } = null!;
        public string? ContractNumber { get; set; }
        public string? SubContractNumber { get; set; }
        public string ManagementBranch { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Manufacturer { get; set; } = null!;
        public string DeviceStatus { get; set; } = null!;
        public string MaintenanceCycle { get; set; } = null!;
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
