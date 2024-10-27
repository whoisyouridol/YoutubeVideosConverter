namespace YoutubeVideoConverter.Infrastructure.SQL.Models
{
    public class UserRequestResponse
    {
        public int Id { get; private set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime RequestDate { get; set; }
        public bool RequestSucceeded { get; set; }
        public List<ConvertLog>? ConvertLogs { get; set; } = [];
    }
}