namespace ReadExcelProcess.DTO
{
    public class Geocoding
    {
        public class GoongGeocodingResponse
        {
            public List<GeocodingResult> Results { get; set; }
        }

        public class GeocodingResult
        {
            public Geometry Geometry { get; set; }
        }

        public class Geometry
        {
            public LocationCoordinates Location { get; set; }
        }

        public class LocationCoordinates
        {
            public decimal Lat { get; set; }
            public decimal Lng { get; set; }
        }
    }
}