using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireflyApi.Controllers
{
    [ApiController]
    [Route("api/load_test")]
    [Authorize]
    public class LoadTestController : ControllerBase
    {
        [HttpGet("root_folders")]
        public ActionResult<object> GetRootFolders()
        {
            // Return empty list for now - will implement actual functionality later
            return Ok(new object[] { });
        }
    }
}