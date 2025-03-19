using System.Globalization;
using System.Text;

namespace ReadExcelProcess.Constant
{
    public static class CommonFunction
    {
        public static string ConvertToCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Chuẩn hóa về chữ thường
            input = input.ToLower();

            // Loại bỏ dấu tiếng Việt
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Loại bỏ khoảng trắng và trả về chuỗi không dấu
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(" ", "");
        }


        public static DateTime? ConvertToDateTime(string dateString, int row)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Định dạng ngày tháng phổ biến trong Excel (dd/MM/yyyy HH:mm)
            string[] formats = { "dd/MM/yyyy HH:mm", "d/M/yyyy H:m", "dd/MM/yyyy H:m", "d/M/yyyy HH:mm" };

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }

            Console.WriteLine($"Không thể chuyển đổi: {dateString} tại {row}");
            return DateTime.Now;
        }
    }
}
