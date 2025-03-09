namespace shopFlow.Utils
{
    public static class DateUtils
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek=DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
