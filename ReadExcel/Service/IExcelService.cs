﻿using ReadExcel.Models;

namespace ReadExcel.Service
{
    public interface IExcelService
    {
        (List<double> MaintenanceTimes, List<List<double>> TravelTimes) GetExcelData(IFormFile file);

        Task<byte[]> GetFile(IFormFile file);

        Task<byte[]> Export(List<RepairPerson> model);
    }
}