using CSF;
using CSF.TShock;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier
{
    [RequirePermission("tbc.admin")]
    public class AdminCommands : TSModuleBase<TSCommandContext>
    {
        [Command("infchest")]
        [Description("Command used to manage infinite chest.")]
        public IResult InfChest(string sub = "") // optional parameter getIndex, if someone would execute /who true it will resolve successfully.
        {
            switch (sub)
            {
                case "add":
                    {
                        Context.Player.SetData<bool>("addinf", true);
                        return Success("Open a chest to make it infinite. Type /cancel to cancel.");
                    }
                case "del":
                    {
                        Context.Player.SetData<bool>("delinf", true);
                        return Success("Open a chest to delete it from Supplier. Type /cancel to cancel.");
                    }

                case "help":
                default:
                    {
                        Info("Help commands for /infchest:");
                        Info("/infchest add - allows the user to create an infinite chest");
                        return Info("/infchest del - deletes an infinite chest");
                    }
            }
        }
        [Command("cancel")]
        [Description("Cancels infinite chest actions.")]
        public IResult InfChest() // optional parameter getIndex, if someone would execute /who true it will resolve successfully.
        {
            Context.Player.SetData<bool>("addinf", false);
            Context.Player.SetData<bool>("delinf", false);
            return Success("Cancelled.");
        }
    }
}
