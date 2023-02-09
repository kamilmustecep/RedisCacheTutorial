using Microsoft.AspNetCore.Hosting.Server;
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


    }
}
