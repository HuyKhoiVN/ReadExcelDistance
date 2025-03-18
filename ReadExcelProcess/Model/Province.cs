using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class Province
    {
        public int Id { get; set; }
        public string ProvinceName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
    }
}
