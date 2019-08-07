using System;

namespace DaemonApp.Libs
{
    public static class StringExtensions
    {
        public static string AppendDate(this string value)
        {
            if (value == null)
            {
                return value;
            }
            return string.Format("{0} {1:yyyy-MM-dd HH:mm:ss:fff}", value, DateTime.Now);
        }
    }
}