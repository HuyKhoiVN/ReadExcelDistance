using ReadExcelProcess.Constant;
using ReadExcelProcess.Models;
using static ReadExcelProcess.DTO.Geocoding;

namespace ReadExcelProcess.Service
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly HttpClient _httpClient;

        public GeoCodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Location> GetCoordinatesAsync(string address)
        {
            var requestUrl = $"{ReadExcelConstant.APIURL}geocode?address={Uri.EscapeDataString(address)}&api_key={ReadExcelConstant.APIKEY}";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var goongResponse = await response.Content.ReadFromJsonAsync<GoongGeocodingResponse>();
            if (goongResponse == null || goongResponse.Results == null || goongResponse.Results.Count == 0)
            {
                return null;
            }
            var result = goongResponse.Results[0];
            return new Location
            {
                Address = address,
                Latitude = result.Geometry.Location.Lat,
                Longitude = result.Geometry.Location.Lng
            };
        }
    }
}