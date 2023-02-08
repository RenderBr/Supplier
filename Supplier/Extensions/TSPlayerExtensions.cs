using TShockAPI;
namespace Supplier.Extensions;
public static class TSPlayerExtensions
{
    public static PlayerOperationState GetPlayerOperationState(this TSPlayer tsplayer)
    {
        if (!tsplayer.ContainsData("Supplier.PlayerState"))
            tsplayer.SetData("Supplier.PlayerState", new PlayerOperationState());

        return tsplayer.GetData<PlayerOperationState>("Supplier.PlayerState");
    }
}