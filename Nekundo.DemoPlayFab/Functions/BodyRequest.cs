using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Nekundo.DemoPlayFab.Functions;

public class BodyRequest(ILogger<BodyRequest> logger)
{
    [Function("BodyRequest")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        string body = await new StreamReader(req.Body).ReadToEndAsync();
        return new OkObjectResult(body);
    }

}