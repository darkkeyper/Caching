using System;
using System.Collections.Generic;

namespace Caching_Update
{
    public static class CacheSystem
    {
        private static Md5System _md5System { get; set; } = new Md5System();
        private static Dictionary<object, object> _cacheDatabases { get; set; } = new Dictionary<object, object>();

        private static object _syncObject_Create = new object();
        private static object _syncObject_add = new object();

        public static bool CreateDatabase<T>(IDataManager<T> datamanager, int expiredobjectscaninterval, int maxobjectsinmemory, int minutesobjectisvalid, out ICacheMessage message)
        {
            bool databasecreated = false;
            Type objecttype = typeof(T);
            message = null;

            lock (_syncObject_Create)
            {
                if (!_cacheDatabases.ContainsKey(objecttype))
                {
                    datamanager.Initialize(minutesobjectisvalid);

                    if (datamanager.IsValid())
                    {
                        CacheManager<T> database = new CacheManager<T>();

                        database.Initialize(datamanager, expiredobjectscaninterval, maxobjectsinmemory, minutesobjectisvalid);

                        if (database.IsValid())
                        {
                            _cacheDatabases.Add(objecttype, database);

                            database.Start();

                            databasecreated = true;
                        }
                        else
                            message = new CacheMessage(MessageLevel.Error, "Database input requires valid input, using minutesobjectisvalid variable.");
                    }
                    else
                        message = new CacheMessage(MessageLevel.Error, "Data manager input requires valid input, using minutesobjectisvalid variable.");
                }
                else
                {
                    databasecreated = true;
                    message = new CacheMessage(MessageLevel.Warning, "Database already existed.");
                }
            }

            return databasecreated;
        }

        public static T Check<T>(string input, Func<T> delegatefunction)
        {
            bool functionexceptionthrown = false;

            ICacheData<T> cachedata = null;
            ICacheManager<T> manager = null;

            try
            {
                if (TryGetManager<T>(out manager))
                {
                    string hash = _md5System.GetMd5Hash(input);
                    if (manager.DataManager.IsInCache(hash, out cachedata))
                    {
                        manager.DataManager.UpdateExpiration(hash);
                        return cachedata.CachedObject;
                    }
                    else
                    {
                        if (manager.IsAtMaxCount())
                        {
                            manager.DataManager.RemoveFirstIndex();
                        }

                        T freshobject = delegatefunction();
                        manager.DataManager.AddToCache(hash, freshobject);
                        return freshobject;
                    }
                }
                else
                {
                    try
                    {
                        return delegatefunction();
                    }
                    catch
                    {
                        functionexceptionthrown = true;
                        throw;
                    }
                }
            }
            catch
            {
                if (functionexceptionthrown)
                    throw;
                else
                    return delegatefunction();
            }
        }

        public static List<T> GetAllItemsInCache<T>()
        {
            ICacheManager<T> manager = null;
            List<T> output = new List<T>();

            if (TryGetManager<T>(out manager))
            {
                foreach (string key in manager.DataManager.Cache.Keys)
                {
                    output.Add(manager.DataManager.Cache[key].CachedObject);
                }
            }

            return output;
        }

        public static void MassLoadCache<T>(Dictionary<string, T> items)
        {
            ICacheManager<T> manager = null;

            if (TryGetManager<T>(out manager))
            {
                foreach (string key in items.Keys)
                {
                    if (manager.IsAtMaxCount())
                    {
                        manager.DataManager.RemoveFirstIndex();
                    }

                    string hash = _md5System.GetMd5Hash(key);
                    manager.DataManager.AddToCache(key, items[key]);
                }
            }
        }

        private static bool TryGetManager<T>(out ICacheManager<T> cachedatabase)
        {
            cachedatabase = null;

            Type key = typeof(T);

            if (_cacheDatabases.ContainsKey(key))
            {
                cachedatabase = (_cacheDatabases[key] as ICacheManager<T>);
                return true;
            }
            return false;
        }
    }
}
