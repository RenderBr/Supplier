using CSF;
using CSF.TShock;

namespace Supplier
{
    [RequirePermission("tbc.admin")]
    public class AdminCommands : TSModuleBase<TSCommandContext>
    {
        #region /infchest <subcmd> - manage infinite chests - tbc.admin
        [Command("infchest")]
        [Description("Command used to manage infinite chest.")]
        public IResult InfChest(string sub = "")
        {
            // switch for sub commands
            switch (sub)
            {
                // if /infchest add was executed
                case "add":
                    {
                        Context.Player.SetData<bool>("addinf", true);
                        return Success("Open a chest to make it infinite. Type /cancel to cancel.");
                    }
                // if /infchest del was executed
                case "del":
                    {
                        Context.Player.SetData<bool>("delinf", true);
                        return Success("Open a chest to delete it from Supplier. Type /cancel to cancel.");
                    }
                // if no sub command was executed, if it was invalid, or if they entered /infchest help
                case "help":
                default:
                    {
                        Info("Help commands for /infchest:");
                        Info("/infchest add - allows the user to create an infinite chest");
                        Info("/infchest del - deletes an infinite chest");
                        return Info("/infchest help - shows this help message");
                    }
            }
        }
        #endregion

        #region /cancel - cancels infinite chest selection - tbc.admin
        [Command("cancel")]
        [Description("Cancels infinite chest actions.")]
        public IResult InfChest()
        {
            // set all data to false and tell player if either selection action is enabled
            if (Context.Player.GetData<bool>("addinf") || Context.Player.GetData<bool>("delinf"))
            {
                Context.Player.SetData<bool>("addinf", false);
                Context.Player.SetData<bool>("delinf", false);
                return Success("Cancelled.");

            }
            // tell the player they weren't selecting anything
            return Error("You weren't selecting anything, so nothing was cancelled");

        }
        #endregion
    }
}
