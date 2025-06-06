using System;
using System.Text;

namespace Little_Hafiz
{
    public static class Base64Converter
    {
        public static string StringToBase64(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public static string Base64ToString(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return string.Empty;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
