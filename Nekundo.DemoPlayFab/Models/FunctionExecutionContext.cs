using PlayFab.ProfilesModels;

namespace Nekundo.DemoPlayFab.Models;

public class FunctionExecutionContext<TArgument>(
    EntityProfileBody callerEntityProfile,
    TitleAuthenticationContext titleAuthenticationContext,
    bool? generatePlayStreamEvent
    )
{
    public EntityProfileBody CallerEntityProfile { get; set; } = callerEntityProfile;
    public TitleAuthenticationContext TitleAuthenticationContext { get; set; } = titleAuthenticationContext;
    public bool? GeneratePlayStreamEvent { get; set; } = generatePlayStreamEvent;
    public TArgument FunctionArgument { get; set; }
}