﻿using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class Province
    {
        public int Id { get; set; }
        public string ProvinceName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
