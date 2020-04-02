using System;
using System.Collections.Generic;

namespace Caching_Update
{
    public class CacheManager<T> : TimedThread, ICacheManager<T>
    {
        /// <summary>
        /// Wrapper for data persistence.
        /// </summary>
        public IDataManager<T> DataManager { get; private set; }
        /// <summary>
        /// Interval (seconds) at which database will be scan and cleanup expired objects.
        /// </summary>
        public int ExpiredScanInterval { get; private set; }
        /// <summary>
        /// Maximum number of objects to maintain in database. Note: Oldest object will be cleaned up when maximum is reached.
        /// </summary>
        public int MaxObjectsInMemory { get; private set; }
        /// <summary>
        /// Duration (seconds) of how long an object will remain in the database before expiration. 
        /// </summary>
        public int TimeObjectValid { get; private set; }

        private HashSet<string> _updateExpirationQueue { get; set; }
        private Md5System MD5System { get; set; }
        public bool _initialized { get; set; }

        public CacheManager()
        {
            MD5System = new Md5System();
            _updateExpirationQueue = new HashSet<string>();
        }

        public void Initialize(IDataManager<T> datamanager, int expiredscaninterval, int maxobjectsinmemory, int timeobjectvalid)
        {
            try
            {
                ValidateInput(expiredscaninterval, timeobjectvalid, maxobjectsinmemory);

                DataManager = datamanager;
                MaxObjectsInMemory = maxobjectsinmemory;
                TimeObjectValid = timeobjectvalid;

                InitializeBase(expiredscaninterval);

                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }

        public bool IsValid()
        {
            if (_initialized)
                return true;
            return false;
        }

        public void TriggerCleanup()
        {
            RemovedExpiredItems();
        }

        public bool IsAtMaxCount()
        {
            if (DataManager.Cache.Count >= MaxObjectsInMemory)
                return true;
            else
                return false;
        }

        protected override bool DoWork()
        {
            UpdateExpiration();
            RemovedExpiredItems();
            return false;
        }

        private void UpdateExpiration()
        {
            lock (DataManager.Cache)
            {
                foreach (string key in _updateExpirationQueue)
                    if (DataManager.Cache.ContainsKey(key))
                        DataManager.UpdateExpiration(key);
            }
            _updateExpirationQueue.Clear();
        }

        private void RemovedExpiredItems()
        {
            
            lock (DataManager.Cache)
            {
                List<string> itemsmarkedforremoval = new List<string>();

                foreach (KeyValuePair<string, ICacheData<T>> kvp in DataManager.Cache)
                {
                    int result = DateTime.Compare(DateTime.Now, kvp.Value.TimeToExpire);

                    if (result == 2)
                        itemsmarkedforremoval.Add(kvp.Key);
                } 

                foreach (string item in itemsmarkedforremoval)
                {
                    DataManager.Cache.Remove(item);
                }
            }
        }

        private void ValidateInput(int expirecheckinterval, int minutesobjectvalid, int maxmemoryentries)
        {
            if (maxmemoryentries <= 0)
                throw new Exception("Max memory objects must be 1+ integer.");
            if (expirecheckinterval <= 0)
                throw new Exception("Expire check interval must be 1+ integer.");
            if (minutesobjectvalid <= 0)
                throw new Exception("Minutes object valid must be 1+ integer.");
        }
    }
}
