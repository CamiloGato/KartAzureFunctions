using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nekundo.DemoPlayFab.Mappers;
using Nekundo.DemoPlayFab.Models;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.EconomyModels;
using PlayFab.ServerModels;

namespace Nekundo.DemoPlayFab;

public class SimpleReward(ILogger<SimpleReward> logger)
{
    private static string DeveloperKey => 
        Environment.GetEnvironmentVariable("PlayFab_DeveloperKey") ?? string.Empty;

    private static readonly string CoinsId = 
        Environment.GetEnvironmentVariable("PlayFab_CoinsId") ?? string.Empty;
    
    [Function("SimpleReward")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req
        )
    {
        logger.LogError("C# HTTP trigger function processed a request.");
        string body = await new StreamReader(req.Body).ReadToEndAsync();
        FunctionExecutionContext<dynamic>? executionContext = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
        
        if (executionContext == null)
        {
            return new BadRequestObjectResult("Invalid request body");
        }
        
        PlayFabApiSettings apiSettings = new PlayFabApiSettings()
        {
            TitleId = executionContext.TitleAuthenticationContext.Id,
            DeveloperSecretKey =  DeveloperKey,
        };
        
        PlayFabAuthenticationContext authenticationContext = new PlayFabAuthenticationContext()
        {
            EntityType = executionContext.CallerEntityProfile.Entity.Type,
            EntityId = executionContext.CallerEntityProfile.Entity.Id,
            EntityToken = executionContext.TitleAuthenticationContext.EntityToken
        };
        
        PlayFabEconomyInstanceAPI economyApi = new PlayFabEconomyInstanceAPI(apiSettings, authenticationContext);
        PlayFabServerInstanceAPI serverInstanceApi = new PlayFabServerInstanceAPI(apiSettings, authenticationContext);

        string overrideLabel;
        
        try
        {
            overrideLabel = executionContext.FunctionArgument.ServerLabel;
        }
        catch
        {
            return new BadRequestObjectResult("Invalid request body");
        }

        GetTitleDataRequest titleDataRequest = new GetTitleDataRequest()
        {
            OverrideLabel = overrideLabel
        };
        
        PlayFabResult<GetTitleDataResult>? titleDataResult = await serverInstanceApi.GetTitleDataAsync(titleDataRequest);
        
        if (titleDataResult.Error != null)
        {
            return new BadRequestObjectResult(titleDataResult.Error.ErrorMessage);
        }
        
        titleDataResult.Result.Data.TryGetValue("Reward", out string? titleCurrencyReward);

        if (!int.TryParse(titleCurrencyReward, out int value))
        {
            return new BadRequestObjectResult("Error from server: Invalid reward value");
        }
        
        int currencyValue = value;
        
        GetInventoryItemsRequest getInventoryItemsRequest = new GetInventoryItemsRequest()
        {
            Entity = executionContext.CallerEntityProfile.Entity.ToPlayFabEntityKey()
        };
        
        PlayFabResult<GetInventoryItemsResponse> inventoryItemsResult = await economyApi.GetInventoryItemsAsync(getInventoryItemsRequest);

        if (inventoryItemsResult.Result.Items.Any(x => x.Id == CoinsId && x.Amount != 0))
        {
            return new OkObjectResult("Already rewarded");
        }
        
        AddInventoryItemsRequest addInventoryItemsRequest = new AddInventoryItemsRequest()
        {
            Entity = executionContext.CallerEntityProfile.Entity.ToPlayFabEntityKey(),
            Item = new InventoryItemReference()
            {
                Id = CoinsId
            },
            Amount = currencyValue
        };
        
        PlayFabResult<AddInventoryItemsResponse> addInventoryResult = await economyApi.AddInventoryItemsAsync(addInventoryItemsRequest);
        
        if (addInventoryResult.Error != null)
        {
            return new BadRequestObjectResult(addInventoryResult.Error.ErrorMessage);
        }
        
        return new OkObjectResult($"Added new reward to {currencyValue}");
    }

}