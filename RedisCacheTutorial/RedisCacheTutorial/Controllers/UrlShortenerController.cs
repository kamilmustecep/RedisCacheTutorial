using Microsoft.AspNetCore.Mvc;
using RedisCacheTutorial.Redis;
using System.Text;
using System;
using System.Web;

namespace RedisCacheTutorial.Controllers
{
    public class UrlShortenerController : Controller
    {
        IRedisCacheService _redisService;
        public UrlShortenerController(IRedisCacheService redisService)
        {
            _redisService = redisService;
        }

        [HttpGet("getShortUrl")]
        public ActionResult Index(string shortUrl)
        {
            var longUrl = _redisService.GetValue(shortUrl);
            return Content(longUrl);
        }

        [HttpGet("shortUrl")]
        public ActionResult ShortUrl(string longUrl)
        {
            string shortUrl = ShortenUrl(longUrl);
            return Content(shortUrl);
        }


        public string GenerateShortUrl()
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var shortUrl = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                shortUrl.Append(chars[random.Next(chars.Length)]);
            }
            return shortUrl.ToString();
        }

        public string ShortenUrl(string longUrl)
        {
            if (_redisService.KeyExists(longUrl))
            {
                return _redisService.GetValue(longUrl);
            }
            else
            {
                var shortUrl = GenerateShortUrl();
                _redisService.SetValue(longUrl, shortUrl,null);
                _redisService.SetValue(shortUrl, longUrl,null);
                return shortUrl;
            }
        }

    }
}
