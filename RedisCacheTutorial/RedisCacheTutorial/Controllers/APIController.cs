using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RedisCacheTutorial.Redis;
using System.Text.Json;

namespace RedisCacheTutorial.Controllers
{
    [Route("api")]
    [ApiController]
    public class APIController : ControllerBase
    {
        IRedisCacheService _redisService;

        public APIController(IRedisCacheService redisService)
        {
            _redisService = redisService;
        }


        [HttpGet("getValue")]
        public IActionResult Get(string key)
        {
            var value = _redisService.GetValue(key);
            return Content(value);
        }

        [HttpPost("setKey")]
        public IActionResult Set(string key,string value,int exMinute)
        {
            _redisService.SetValue(key, value, exMinute);
            return Content("1");
        }

        [HttpDelete("deleteKey")]
        public IActionResult Delete(string key)
        {
            _redisService.RemoveKey(key);
            return Content("1");
        }


        [HttpGet("getExTime")]
        public IActionResult GetExpireTime(string key)
        {
            var value = _redisService.GetExpireTime(key);
            return Content(value);
        }

        [HttpPost("setExTime")]
        public IActionResult SetExpireTime(string key,int minute,bool isAdding)
        {
            var value = _redisService.SetExpireTime(key, minute,isAdding);
            return Content(value.ToString());
        }


        [HttpGet("getAllKey")]
        public IActionResult GetAllKey()
        {
            var result = JsonConvert.SerializeObject(_redisService.GetAllKeys());  
            return Content(result);
        }

        [HttpGet("deleteAllKey")]
        public IActionResult DeleteAllKey()
        {
            var result = _redisService.RemoveAllKeys();
            return Content(result.ToString());
        }


        [HttpPost("setSortedWithScore")]
        public IActionResult SetSortedWithScore(string key,string value,double score)
        {
            _redisService.SortedSetAdd(key, value, score);
            return Ok();
        }


        [HttpGet("getSortedWithScore")]
        public IActionResult SortedSetRangeByScore(string key)
        {
            return Content(_redisService.SortedSetRangeByScore(key));
        }






        [HttpGet("subscribe")]
        public async Task<IActionResult> Subscribe(string channelName)
        {
            var messageReceived = false;
            var message = string.Empty;
            _redisService.Subscribe(channelName, (channel, msg) =>
            {
                message = msg;
                messageReceived = true;
            });

            while (!messageReceived)
            {
                await Task.Delay(100);
            }

            return Ok(message);
        }


        [HttpPost("publish")]
        public IActionResult Publish(string channelName,string messageSend)
        {
            _redisService.PublishMessage(channelName, messageSend);
            return Ok();
        }
    }
}
