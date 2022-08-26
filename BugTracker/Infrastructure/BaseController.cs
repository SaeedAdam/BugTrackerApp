using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Infrastructure
{
    [Route("[controller]/[action]", Name = "[controller]_[action]")]
    public abstract class BaseController : Controller
    {
    }
}
