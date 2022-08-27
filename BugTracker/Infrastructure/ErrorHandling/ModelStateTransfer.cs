using Microsoft.AspNetCore.Mvc.Filters;

namespace BugTracker.Infrastructure.ErrorHandling
{
    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }
}