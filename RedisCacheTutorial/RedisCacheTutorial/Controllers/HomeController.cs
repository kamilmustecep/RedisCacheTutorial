using Microsoft.AspNetCore.Mvc;
using RedisCacheTutorial.Models;
using RedisCacheTutorial.Redis;
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
            return View();
        }

    }
}