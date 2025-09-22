using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireflyApi.Controllers
{
    [ApiController]
    [Route("api/autotest")]
    [Authorize]
    public class AutoTestController : ControllerBase
    {
        [HttpGet("root_folders")]
        public ActionResult<object> GetRootFolders()
        {
            // Return empty list for now - will implement actual functionality later
            return Ok(new object[] { });
        }

        [HttpGet("test_runs")]
        public ActionResult<object> GetTestRuns([FromQuery] string? root_folder = null, [FromQuery] string? env = null)
        {
            // Return empty list for now - will implement actual functionality later
            return Ok(new object[] { });
        }
    }
}