using OfficeOpenXml;

namespace ReadExcel.Service
{
    public class ExcelService
    {
        public (List<double> MaintenanceTimes, List<List<double>> TravelTimes) GetExcelData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or not provided.");

            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                throw new ArgumentException("Invalid file format. Please upload an Excel file (.xlsx).");

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(stream);

            var sheet1 = package.Workbook.Worksheets[0];
            if (sheet1 == null)
                throw new ArgumentException("Sheet1 is missing from the file.");

            int rowCount = sheet1.Dimension.Rows;
            List<double> maintenanceTimes = new();
            for (int i = 1; i <= rowCount; i++)
            {
                double value = sheet1.Cells[i, 2].GetValue<double>();
                maintenanceTimes.Add(Math.Round(value, 2));
            }

            var sheet2 = package.Workbook.Worksheets[1];
            if (sheet2 == null)
                throw new ArgumentException("Sheet2 is missing from the file.");

            int size = sheet2.Dimension.Rows;
            List<List<double>> travelTimes = new();
            for (int i = 0; i < size; i++)
            {
                List<double> row = new();
                for (int j = 0; j < size; j++)
                {
                    double value = sheet2.Cells[i + 1, j + 1].GetValue<double>();
                    row.Add(Math.Round(value, 2));
                }
                travelTimes.Add(row);
            }

            return (maintenanceTimes, travelTimes);
        }
    }
}
