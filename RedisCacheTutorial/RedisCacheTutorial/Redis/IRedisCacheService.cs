using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Data.Common;

namespace RedisCacheTutorial.Redis
{
    public interface IRedisCacheService
    {
        public IDatabase GetDatabase();


        public void SetValue(string key, string value, int? exMinute);


        public string GetValue(string key);

        public string GetExpireTime(string key);

        public bool SetExpireTime(string key, int exMinute,bool isAdding);
        public bool KeyExists(string key);


        public void RemoveKey(string key);


        public List<string> GetAllKeys();

        public bool RemoveAllKeys();

        public void PublishMessage(string channel, string message);

        public void Subscribe(string channelName, Action<string, string> handleMessage);

        public void Unsubscribe(string channel);

        public void SetObject(string key, object value);
        public T GetObject<T>(string key);


        public void HashSet(string key, Dictionary<string, string> values);

        public Dictionary<string, string> HashGetAll(string key);


        public void ListRightPush(string key, string value);

        public string ListLeftPop(string key);

        public List<RedisValue> GetList(string key);
        public void SortedSetAdd(string key, string value, double score);


        public string SortedSetRangeByScore(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity);
       

    }
}
