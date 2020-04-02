using System;
using System.Collections.Generic;

namespace Caching_Update
{
    public interface IDataManager<T>
    {
        Dictionary<string, ICacheData<T>> Cache { get; set; }

        void Initialize(int minutesobjectisvalid);
        bool IsValid();
        bool IsInCache(string input, out ICacheData<T> outputdata);
        void AddToCache(string input, T obj);
        void RemoveFirstIndex();
        void UpdateExpiration(string key);
    }

    public interface ICacheManager<T>
    {
        IDataManager<T> DataManager { get; }

        /// <summary>
        /// Initializes database.
        /// </summary>
        /// <param name="datamanager">Wrapper for data persistence.</param>
        /// <param name="expiredscaninterval">Interval (seconds) at which database will be scan and cleanup expired objects.</param>
        /// <param name="maxobjectsinmemory">Maximum number of objects to maintain in database. Note: Oldest object will be cleaned up when maximum is reached.</param>
        /// <param name="timeobjectvalid">Duration (seconds) of how long an object will remain in the database before expiration. </param>
        void Initialize(IDataManager<T> datamanager, int expiredscaninterval, int maxobjectsinmemory, int timeobjectvalid);

        void Stop();
        void Start();
        bool IsValid();
        void TriggerCleanup();
        bool IsAtMaxCount();
    }

    public interface ICacheData<T>
    {
        string ObjectMD5 { get; set; }
        DateTime TimeToExpire { get; set; }
        T CachedObject { get; set; }
    }

    public interface ICacheMessage
    {
        MessageLevel Level { get; set; }
        string Message { get; set; }
    }
}
