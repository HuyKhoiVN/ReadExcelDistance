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

                    try
                    {
                        var distanceMatrix = await GetTravelTime(origin, destinations);
                        List<double> travelTimes = new();

                        if (distanceMatrix?.Rows?.Count > 0)
                        {
                            foreach (var element in distanceMatrix.Rows[0].Elements)
                            {
                                double durationInHours = element.Status == "OK" ? element.Duration.Value / 3600.0 : double.MaxValue;
                                travelTimes.Add(durationInHours);
                            }
                        }

                        for (int j = 0; j < destinations.Count; j++)
                        {
                            int destIndex = i + 1 + j;
                            matrix[i, destIndex] = matrix[destIndex, i] = travelTimes[j];
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi gọi API lấy thời gian từ {origin.Latitude},{origin.Longitude}: {ex.Message}");
                        for (int j = i + 1; j < n; j++)
                        {
                            matrix[i, j] = matrix[j, i] = double.MaxValue;
                        }
                    }
                    await Task.Delay(500);
                }
            }

            return matrix;
        }

        private async Task<List<Location>> GetListLocation(List<string> addressList)
        {
            List<Location> locations = new List<Location>();
            foreach (var add in addressList)
            {
                Location location = await _geoCodingService.GetCoordinatesAsync(add); // gọi tới api geoCoding để lấy được toạ độ
                locations.Add(location);

                await Task.Delay(500);
            }
            return locations;
        }

        public async Task<DistanceMatrixResponse> GetTravelTime(Location origin, List<Location> destinations)
        {
            try
            {
                string originParam = $"{origin.Latitude.ToString(CultureInfo.InvariantCulture)},{origin.Longitude.ToString(CultureInfo.InvariantCulture)}";
                string destinationsParam = string.Join("|", destinations.Select(d => $"{d.Latitude.ToString(CultureInfo.InvariantCulture)},{d.Longitude.ToString(CultureInfo.InvariantCulture)}"));

                string url = $"{ReadExcelConstant.APIURL}distancematrix?origins={originParam}&destinations={destinationsParam}&vehicle=car&api_key={ReadExcelConstant.APIKEY}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"API trả về lỗi: {response.StatusCode}");

                string json = await response.Content.ReadAsStringAsync();
                // map obj
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                return JsonSerializer.Deserialize<DistanceMatrixResponse>(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API Goong: {ex.Message}");
                return null;
            }
        }
    }
}
