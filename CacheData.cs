using System;

namespace Caching
{
    public class CacheData<T> : ICacheData<T>
    {
        public string ObjectMD5 { get; set; }
        public DateTime TimeToExpire { get; set; }
        public T CachedObject { get; set; }

        public CacheData(DateTime timetoexpire, T objecttocache)
        {
            TimeToExpire = timetoexpire;
            CachedObject = objecttocache;
        }
    }
}
