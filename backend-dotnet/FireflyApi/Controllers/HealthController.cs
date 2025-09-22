using Microsoft.AspNetCore.Mvc;

namespace FireflyApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("health")]
        public ActionResult<object> Health()
        {
            return Ok(new { status = "healthy" });
        }

        [HttpGet("version")]
        public ActionResult<object> Version()
        {
            var version = _configuration["PROJECT_VERSION"] ?? "1.0.0";
            var projectName = _configuration["PROJECT_NAME"] ?? "Firefly API";
            
            return Ok(new 
            { 
                version = version,
                project_name = projectName
            });
        }
    }
}