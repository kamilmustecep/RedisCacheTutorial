using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RedisCacheTutorial.Models;
using RedisCacheTutorial.Redis;
using StackExchange.Redis;
using System.Diagnostics;

namespace RedisCacheTutorial.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IRedisCacheService _redisService;



        public HomeController(ILogger<HomeController> logger, IRedisCacheService redisService)
        {
            _logger = logger;
            _redisService = redisService;
        }


        public IActionResult Index()
        {
            var sessionId = HttpContext.Request.Cookies[".SampleApp.Session"];
            string jsonData = _redisService.GetValue(sessionId);
            UserModel user = JsonConvert.DeserializeObject<UserModel>(jsonData);

            return View(user);
        }

    }
}