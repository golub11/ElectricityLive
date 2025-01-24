 namespace nigo.Models
{
    public class TimeInterval
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }

        public TimeInterval(DateTime from, DateTime to)
        {
            this.from = from;
            this.to = to;
        }

        public static TimeInterval FromString(string from, string to){
            DateTime dateFrom = DateTime.ParseExact(from, "yyyy-MM-dd", null);
            DateTime dateTo = DateTime.ParseExact(to, "yyyy-MM-dd", null);

            var timeInterval = new TimeInterval
            (
             dateFrom,
             dateTo
            );

            return timeInterval;
        }

        public static TimeInterval FromUTCString(string from, string to)
        {
            DateTime dateFrom = DateTime.ParseExact(from, "yyyy-MM-ddTHH:mmZ", null);
            DateTime dateTo = DateTime.ParseExact(to, "yyyy-MM-ddTHH:mmZ", null);

            var timeInterval = new TimeInterval
            (
             dateFrom,
             dateTo
            );

            return timeInterval;
        }

    }
}