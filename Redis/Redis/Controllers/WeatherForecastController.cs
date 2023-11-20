using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Redis.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<List<WeatherForecast>> SetAsync()
        {
            var value = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            var serialize = JsonSerializer.Serialize(value);

            await _distributedCache.SetStringAsync("Key", serialize, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

            List<WeatherForecast> result = JsonSerializer.Deserialize<List<WeatherForecast>>(serialize).ToList();

            return result;
        }

        [HttpGet]
        public async ValueTask<IActionResult> GetAsync()
        {
            var result = await _distributedCache.GetStringAsync("Key");

            return Ok(result);
        }
        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    var caheValue = _memoryCache.Get("key");

        //    if (caheValue == null)
        //    {
        //        var value = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //        {
        //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //            TemperatureC = Random.Shared.Next(-20, 55),
        //            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //        }).ToArray();

        //        _memoryCache.Set(
        //            key: "key",
        //            value: value,
        //            options: new MemoryCacheEntryOptions()
        //            {
        //                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
        //                SlidingExpiration = TimeSpan.FromSeconds(20)
        //            });

        //        return value;
        //    }

        //    return _memoryCache.Get("key") as IEnumerable<WeatherForecast>;
        //}
    }   
}