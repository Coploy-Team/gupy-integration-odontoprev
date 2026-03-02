using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using GupyIntegration.Models;

namespace GupyIntegration.Attributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class ApiKeyAttribute : Attribute, IAsyncActionFilter
  {
    private const string ApiKeyHeaderName = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      var potentialApiKey = context.HttpContext.Request.Headers[ApiKeyHeaderName].FirstOrDefault()
          ?? context.HttpContext.Request.Query["apiKey"].FirstOrDefault();

      if (string.IsNullOrEmpty(potentialApiKey))
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      var configuration = context.HttpContext.RequestServices.GetRequiredService<IOptions<ApiKeySettings>>();
      var apiKey = configuration.Value.Key;

      if (!apiKey.Equals(potentialApiKey))
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      await next();
    }
  }
}