namespace Nekundo.DemoPlayFab.Models;

public class TitleAuthenticationContext(string id, string entityToken)
{
    public string Id { get; set; } = id;
    public string EntityToken { get; set; } = entityToken;
}