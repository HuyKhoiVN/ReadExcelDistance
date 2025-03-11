﻿using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ReadExcelProcess.Constant;
using ReadExcelProcess.DTO;
using ReadExcelProcess.Models;
using System.Text.Json;

namespace ReadExcelProcess.Service
{
    public class DistanceMatrixService : IDistanceMatrixService
    {
        private readonly HttpClient _httpClient;

        public DistanceMatrixService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Location>> GetListLocation(List<string> addressList)
        {
            List<Location> locations = new List<Location>();
            foreach (var add in addressList)
            {
                Location location = new Location();
                location.Address = add;
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
            string originParam = $"{origin.Latitude},{origin.Longitude}";
            string destinationsParam = string.Join("%7C", destinations.Select(d => $"{d.Latitude},{d.Longitude}"));

            string url = $"{ReadExcelConstant.APIURL}?origins={originParam}&destinations={destinationsParam}&vehicle=car&api_key={ReadExcelConstant.APIKEY}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<double>(new double[destinations.Count]);

            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DistanceMatrixResponse>(json);

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
