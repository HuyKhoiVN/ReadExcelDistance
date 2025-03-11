using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ReadExcelProcess.Constant;
using ReadExcelProcess.DTO;
using ReadExcelProcess.Models;
using System.Globalization;
using System.Text.Json;

namespace ReadExcelProcess.Service
{
    public class DistanceMatrixService : IDistanceMatrixService
    {
        private readonly HttpClient _httpClient;
        private readonly IGeoCodingService _geoCodingService;

        public DistanceMatrixService(HttpClient httpClient, IGeoCodingService geoCodingService)
        {
            _httpClient = httpClient;
            _geoCodingService = geoCodingService;
        }

        public async Task<List<Location>> GetListLocation(List<string> addressList)
        {
            List<Location> locations = new List<Location>();
            foreach (var add in addressList)
            {
                Location location = await _geoCodingService.GetCoordinatesAsync(add);
                locations.Add(location);
            }
            return locations;
        }

        public async Task<double[,]> GetTravelTimeMatrix(List<string> addressList)
        {
            var locations = await GetListLocation(addressList);

            int n = locations.Count;
            double[,] matrix = new double[n, n];

            for(int i = 0; i < n; i++)
            {
                matrix[i, i] = 0;

                if(i < n -1)
                {
                    var origin = locations[i];
                    var destinations = locations.Skip(i + 1).ToList();

                    var travelTimes = await GetTravelTime(origin, destinations);

                    for(int j = 0; j < destinations.Count; j++)
                    {
                        int destIndex = i + 1 + j;
                        matrix[i, destIndex] = matrix[destIndex, i] = travelTimes[j];
                    }
                }
            }

            return matrix;
        }

        private async Task<List<double>> GetTravelTime(Location origin, List<Location> destinations)
        {
            string originParam = $"{origin.Latitude.ToString(CultureInfo.InvariantCulture)},{origin.Longitude.ToString(CultureInfo.InvariantCulture)}";
            string destinationsParam = string.Join("|", destinations.Select(d => $"{d.Latitude.ToString(CultureInfo.InvariantCulture)},{d.Longitude.ToString(CultureInfo.InvariantCulture)}"));

            string url = $"{ReadExcelConstant.APIURL}distancematrix?origins={originParam}&destinations={destinationsParam}&vehicle=car&api_key={ReadExcelConstant.APIKEY}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<double>(new double[destinations.Count]);

            string json = await response.Content.ReadAsStringAsync();
            // map obj
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<DistanceMatrixResponse>(json, options);


            List<double> travelTimes = new();
            if (result?.Rows?.Count > 0)
            {
                foreach (var element in result.Rows[0].Elements)
                {
                    double durationInHours = element.Status == "OK" ? element.Duration.Value / 3600.0 : double.MaxValue;
                    travelTimes.Add(durationInHours);
                }
            }

            return travelTimes;
        }
    }
}
