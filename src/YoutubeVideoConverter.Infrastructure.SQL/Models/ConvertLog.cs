using YoutubeVideoConverter.Infrastructure.SQL.Enums;

namespace YoutubeVideoConverter.Infrastructure.SQL.Models
{
    public class ConvertLog
    {
        public int Id { get; private set; }
        public string Url { get; set; }
        public string VideoName { get; set; }
        public bool ConvertedSuccessfully { get; set; }
        public TimeSpan ConversionTime { get; set; }
        public ConvertType ConvertTo { get; set; }
        public Destination ConversionDestination { get; set; }
        public UserRequestResponse UserRequest { get; set; }
    }
}