using Microsoft.AspNetCore.Mvc;
using RedisCacheTutorial.Models;
using RedisCacheTutorial.Redis;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisCacheTutorial.Controllers
{
    public class LoginController : Controller
    {
        IRedisCacheService _redisService;
        public LoginController(IRedisCacheService redisService)
        {
            _redisService = redisService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            // Kullanıcı doğrulaması yapılacak
            if (username=="admin" && password=="12345a*")
            {
                // Kullanıcı oturum verilerini sakla
                UserModel user = new UserModel();
                user.userName = username;
                user.isAdmin = true;
                user.LastLoginTime = DateTime.Now;  


                var jsonData = JsonSerializer.Serialize(user);
                var sessionId = Guid.NewGuid().ToString();

                _redisService.SetValue(sessionId, jsonData, 60);

                // Oturum kimliğini çereze ekle
                HttpContext.Response.Cookies.Append(".TutorialApp.Session", sessionId,
                    new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                        Secure = true
                    });

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            
        }


        public IActionResult Logout()
        {
            // Oturum kimliğini çerezden al
            var sessionId = HttpContext.Request.Cookies[".SampleApp.Session"];

            // Oturum verilerini Redis'ten sil
            _redisService.RemoveKey(sessionId);

            // Oturum kimliğini çerezden sil
            HttpContext.Response.Cookies.Delete(".SampleApp.Session");

            return RedirectToAction("Index", "Home");
        }
    }
}
