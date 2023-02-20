using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RedisCacheTutorial.Redis;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


#region OTURUM Y�NET�M� ���N RED�S KULLANIMI

// Redis Cache ekleme
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Redis sunucusunun adresi ve portu
    options.InstanceName = "TutorialApp_"; // �nek, benzersiz bir isim olmal�
});
// Oturum y�netimi ekleme
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TutorialApp.Session"; // Oturum �erezi ad�
    options.IdleTimeout = TimeSpan.FromSeconds(3600); // Oturum s�resi (saniye cinsinden)
    options.Cookie.IsEssential = true;
});
// Controller'lar�n kullanabilece�i yetkilendirme servisini ekleme
builder.Services.AddAuthorization();

#endregion


builder.Services.AddControllersWithViews();


builder.Services.AddSingleton<IRedisCacheService, RedisCacheManager>();



builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger Example API", Description = "This project is test api project.", Version = "v1" });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Swagger Example API", Description = "This project is test api project.", Version = "v2" });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger Example API EndPoint");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Swagger Example API EndPoint");
    //options.RoutePrefix = "swagger";
    options.RoutePrefix = String.Empty;

});



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
