﻿namespace ReadExcelProcess.Service
{
    public interface IDeviceImportService
    {
        Task<List<int>> ImportDevicesFromExcel(IFormFile file);
        Task ImportTravelTimeDevice(IFormFile file);
    }
}
