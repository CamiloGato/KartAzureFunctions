namespace Nekundo.DemoPlayFab.Mappers;

public static class EntityMapper
{
    public static PlayFab.EconomyModels.EntityKey ToPlayFabEntityKey(this PlayFab.ProfilesModels.EntityKey entityKey)
    {
        return new PlayFab.EconomyModels.EntityKey
        {
            Id = entityKey.Id,
            Type = entityKey.Type
        };
    }
}