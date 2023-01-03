using Microsoft.AspNetCore.Mvc;

namespace MK94.Assert.Example.PizzaApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PizzaApi : ControllerBase
    {
        private readonly ILogger<PizzaApi> _logger;

        public PizzaApi(ILogger<PizzaApi> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {

        }
    }
}