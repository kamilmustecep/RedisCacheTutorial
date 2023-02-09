using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Data.Common;
using System.Text.Json;

namespace RedisCacheTutorial.Redis
{
    public class RedisCacheManager : IRedisCacheService
    {
        private static ConnectionMultiplexer _connection;
        private static IServer _server;
        private static ISubscriber _subscriber;
        private static readonly object _lock = new object();


        public RedisCacheManager()
        {
            Connect();
            _server = _connection.GetServer("localhost:6379");
            _subscriber = _connection.GetSubscriber();
        }

        private static void Connect()
        {
            if (_connection != null && _connection.IsConnected) return;

            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected) return;


                //flushdb için allow admin true yapılır.
                var options = ConfigurationOptions.Parse("localhost:6379");
                options.ConnectRetry = 5;
                options.AllowAdmin = true;

                _connection = ConnectionMultiplexer.Connect(options);
            }
        }

        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }


        public void SetValue(string key, string value, int? exMinute)
        {
            TimeSpan? duration = (exMinute != null && exMinute!=0) ? TimeSpan.FromMinutes((int)exMinute) : (TimeSpan?)null;


            #region When komutları
            /*
            When.Always: Her zaman değer ataması yapılır.

            When.Exists: Anahtar zaten var ise değer ataması yapılır.

            When.NotExists: Anahtar yok ise değer ataması yapılır.
            */
            #endregion


            #region CommandFlags komutları
            /* CommandFlags enumeration tipinde şu seçenekler bulunmaktadir:

            None: Varsayılan değerdir ve işlem normal olarak yürütülür ve bir sonuç döndürülür.

            FireAndForget: Bu işaret, sunucuya gönderilen işlemi yürütmek için gerekli kaynakları kullanmasına izin verir, ancak sunucu işlem tamamlandıktan sonra bir sonuç döndürmez.

            DemandMaster: Bu işaret, işlemi sadece Redis Master sunucusunda yürütmesini ister. Eğer Master sunucu mevcutsa, işlem orada yürütülür, değilse hata döndürülür.

            PreferMaster: Bu işaret, işlemi öncelikle Master sunucuda yürütmeyi ister, ancak eğer Master sunucu mevcut değilse Slave sunucuda yürütmeyi dener.
            
            DemandSlave: Bu işaret, işlemi sadece Redis Slave sunucularında yürütmesini ister. Eğer bir Slave sunucu mevcutsa, işlem orada yürütülür, değilse hata döndürülür.

            PreferSlave: Bu işaret, işlemi öncelikle Slave sunucularında yürütmeyi ister, ancak eğer bir Slave sunucu mevcut değilse Master sunucuda yürütmeyi dener.

            NoRedirect: Bu işaret, sunucunun işlemi bir başka sunucuda yürütmeyi isteyeceği durumlarda işlemi yine de yürütmesini ister.

            NoAck: Bu işaret, sunucunun yapılan işlemin onayını vermemesini ister. Bu, sunucuda gerçekleştirilen işlemlerin hızlı bir şekilde yürütülmesine olanak tanır, ancak verilerin sunucuda kaydedilmesi veya güncellenmesi gibi işlemlerin güvenliği konusunda endişeler oluşabilir.
             
             */
            #endregion


            GetDatabase().StringSet(key, value, duration, When.Always, CommandFlags.None);
        }
        public string GetValue(string key)
        {
            return GetDatabase().StringGet(key);
        }
        public void RemoveKey(string key)
        {
            GetDatabase().KeyDelete(key);
        }
        public bool KeyExists(string key)
        {
            return GetDatabase().KeyExists(key);
        }


        public string GetExpireTime(string key)
        {
            TimeSpan? ttl = GetDatabase().KeyTimeToLive(key);

            if (ttl.HasValue)
            {
                return "Key '" + key + "' will expire in " + ttl.Value.TotalMinutes + " minute.";
            }
            else
            {
                return "Key '" + key + "' has no expiry time.";
            }

        }
        public bool SetExpireTime(string key, int exMinute, bool isAdding)
        {
            // Key exists, refresh the expiration time
            if (KeyExists(key))
            {

                // isAdding true, prev expiration time added to new exMinute
                TimeSpan ttl = isAdding
                ? (TimeSpan)(TimeSpan.FromMinutes(exMinute) + GetDatabase().KeyTimeToLive(key))
                : TimeSpan.FromMinutes(exMinute);

                GetDatabase().KeyExpire(key, ttl);
                return true;
            }
            else
            {
                // Key does not exist, add the key-value pair with an expiration time
                return false;
            }

        }
        public List<string> GetAllKeys()
        {
            return _server.Keys().Select(key => key.ToString()).ToList();
        }
        public bool RemoveAllKeys()
        {
            //admin izni ile çalışan bir metoddur
            _server.FlushDatabase();
            return true;
        }


        public void SetObject(string key, object value)
        {
            GetDatabase().StringSet(key, JsonConvert.SerializeObject(value));
        }
        public T GetObject<T>(string key)
        {
            var value = GetDatabase().StringGet(key);
            return value.HasValue ? JsonConvert.DeserializeObject<T>(value) : default(T);
        }



        //OTHER OPERATİONS

        public void HashSet(string key, Dictionary<string, string> values)
        {
            GetDatabase().HashSet(key, values.Select(x => new HashEntry(x.Key, x.Value)).ToArray());
        }
        public Dictionary<string, string> HashGetAll(string key)
        {
            return GetDatabase().HashGetAll(key).ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        }
        public  void ListRightPush(string key, string value)
        {
            GetDatabase().ListRightPush(key, value);
        }
        public string ListLeftPop(string key)
        {
            return GetDatabase().ListLeftPop(key);
        }
        public void SortedSetAdd(string key, string value, double score)
        {
            GetDatabase().SortedSetAdd(key, value, score);
        }
        public List<string> SortedSetRangeByScore(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity)
        {
            return GetDatabase().SortedSetRangeByScore(key, start, stop).Select(x => x.ToString()).ToList();
        }


        //PUB-SUB OPERATİONS

        public void PublishMessage(string channel, string message)
        {
            _subscriber.Publish(channel, message);
        }
        public void Subscribe(string channelName, Action<string, string> handleMessage)
        {
            var subscriber = _connection.GetSubscriber();
            subscriber.Subscribe(channelName, (channel, message) =>
            {
                handleMessage(channel, message);
            });
        }
        public void Unsubscribe(string channel)
        {
            _subscriber.Unsubscribe(channel);
        }


    }
}
