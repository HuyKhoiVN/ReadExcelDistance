namespace ReadExcelProcess.DTO
{
    public class DistanceMatrixResponse
    {
        public List<Row> Rows { get; set; }
    }

    public class Row
    {
        public List<Element> Elements { get; set; }
    }

    public class Element
    {
        public string Status { get; set; }
        public Duration Duration { get; set; }
        public Distance Distance { get; set; }
    }

    public class Duration
    {
        public string Text { get; set; }
        public int Value { get; set; } // Thời gian di chuyển (giây)
    }

    public class Distance
    {
        public string Text { get; set; }
        public int Value { get; set; } // Khoảng cách - mét
    }

}
