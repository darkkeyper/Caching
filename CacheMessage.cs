namespace Caching_Update
{
    public class CacheMessage : ICacheMessage
    {
        public MessageLevel Level { get; set; }
        public string Message { get; set; }

        public CacheMessage(MessageLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }
}
