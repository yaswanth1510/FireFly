using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireflyApi.Controllers
{
    [ApiController]
    [Route("api/script")]
    [Authorize]
    public class ScriptController : ControllerBase
    {
        [HttpGet("root_folders")]
        public ActionResult<object> GetRootFolders()
        {
            // Return empty list for now - will implement actual functionality later
            return Ok(new object[] { });
        }
    }
}