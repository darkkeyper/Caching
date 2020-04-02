using System;
using System.Collections.Generic;
using System.Linq;

namespace Caching_Update.DataManagers
{
    public class DataManager_Mem<T> : IDataManager<T>
    {
        private object _syncObject = new object();
        
        public Dictionary<string, ICacheData<T>> Cache { get; set; } = new Dictionary<string, ICacheData<T>>();

        private int _minutesCacheObjectValid;

        public void Initialize(int minutesobjectisvalid)
        {
            _minutesCacheObjectValid = minutesobjectisvalid;
        }

        public bool IsValid()
        {
            if (_minutesCacheObjectValid <= 1)
                return false;
            else
                return true;
        }

        public bool IsInCache(string input, out ICacheData<T> outputdata)
        {
            if (Cache.TryGetValue(input, out outputdata))
                return true;
            return false;
        }

        public void AddToCache(string input, T obj)
        {
            lock (_syncObject)
            {
                Cache.Add(input, new CacheData<T>(DateTime.Now.AddMinutes(_minutesCacheObjectValid), obj)); 
            }
        }

        public void RemoveFirstIndex()
        {
            lock (_syncObject)
            {
                KeyValuePair<string, ICacheData<T>> objtoremove = Cache.ElementAt(0);
                Cache.Remove(objtoremove.Key); 
            }
        }

        public void UpdateExpiration(string key)
        {
            lock (_syncObject)
            {
                KeyValuePair<string, ICacheData<T>> objtoupdate = Cache.ElementAt(0);
                objtoupdate.Value.TimeToExpire = DateTime.Now.AddMinutes(_minutesCacheObjectValid); 
            }
        }
    }
}
