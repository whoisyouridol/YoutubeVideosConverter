namespace YoutubeVideosConverter.Application.Models
{
    public class CustomFileStream : FileStream
    {
        private readonly string _filePath;
        public CustomFileStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
            _filePath = path;
        }
        public override ValueTask DisposeAsync()
        {
            Dispose();
            if (File.Exists(_filePath)) File.Delete(_filePath);
            return base.DisposeAsync();
        }
    }
}