using System;
using System.Collections.Generic;

namespace ReadExcelProcess.Model
{
    public partial class Officer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Title { get; set; }
        public string? Cccd { get; set; }
        public DateTime? DateOfIssue { get; set; }
        public string? PlaceOfIssue { get; set; }
        public string? Account { get; set; }
        public string? Branch { get; set; }
        public string? Region { get; set; }
        public string? ProvinceCode { get; set; }
    }
}
