using Common.Types;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers
{
    [Route("/teams")]
    [ApiController]
    public class SystemInfoController() : ControllerBase
    {
        [HttpGet]
        [Route("version")]
        public IActionResult GetVersion()
        {
            return Ok(new
            {
                version = VersionInfo.Version,
                buildDate = VersionInfo.BuildDate
            });
        }
    }
}
