using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nekundo.DemoPlayFab.Models;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;

namespace Nekundo.DemoPlayFab;

public class SimpleReward(ILogger<SimpleReward> logger)
{
    [Function("SimpleReward")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req
        )
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        string body = await new StreamReader(req.Body).ReadToEndAsync();
        FunctionExecutionContext? executionContext = JsonConvert.DeserializeObject<FunctionExecutionContext>(body);
        
        if (executionContext == null)
        {
            return new BadRequestObjectResult("Invalid request body");
        }

        PlayFabApiSettings apiSettings = new PlayFabApiSettings()
        {
            TitleId = executionContext.TitleAuthenticationContext.Id,
            DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_SECRET_KEY"),
        };

        PlayFabAuthenticationContext authenticationContext = new PlayFabAuthenticationContext()
        {
            EntityToken = executionContext.TitleAuthenticationContext.EntityToken
        };

        PlayFabServerInstanceAPI instanceApi = new PlayFabServerInstanceAPI(apiSettings, authenticationContext);
        
        GetTitleDataRequest titleDataRequest = new GetTitleDataRequest()
        {
            Keys = ["Reward"]
        };
        
        PlayFabResult<GetTitleDataResult>? titleDataResult = await instanceApi.GetTitleDataAsync(titleDataRequest);
        titleDataResult.Result.Data.TryGetValue("Reward", out string? value);

        if (value == null)
        {
            return new BadRequestObjectResult("Reward not found");
        }
        
        return new OkObjectResult(value);
    }

}