namespace CustomerService.API.Utils
{
    public class DayMonth
    {
        public int Day { get; set; }
        public int Month { get; set; }

        public override string ToString()
        {
            return $"{Day:D2}/{Month:D2}";
        }
        public static DayMonth Parse(string value)
        {
            var parts = value.Split('/');
            return new DayMonth
            {
                Day = int.Parse(parts[0]),
                Month = int.Parse(parts[1])
            };
        }
    }
}