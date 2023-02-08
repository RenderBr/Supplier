using Auxiliary;
using CSF;
using CSF.TShock;
using MongoDB.Driver;
using Supplier.Api;
using Supplier.Models;
using Supplier.Extensions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using static TShockAPI.GetDataHandlers;

namespace Supplier
{
    [ApiVersion(2, 1)]
    public class Supplier : TerrariaPlugin
    {
        #region Plugin Metadata
        public override string Name => "Supplier";
        public override string Description => "An infinite-chests successor, allows chests to be filled with items";
        public override string Author => "Average";
        public override Version Version => new Version(1, 1);
        #endregion
        private readonly TSCommandFramework _fx;

        public SupplierApi core;

        #region Plugin Initialization
        public Supplier(Main game) : base(game)
        {
            _fx = new(new CommandConfiguration()
            {
                DoAsynchronousExecution = false
            });
        }

        public async override void Initialize()
        {
            // build command modules
            await _fx.BuildModulesAsync(typeof(Supplier).Assembly);

            // initialize api
            core = new SupplierApi();

            //register hooks
            ChestOpen += OnChestOpen;
            ChestItemChange += OnItemChange;
            TerrariaApi.Server.ServerApi.Hooks.GameInitialize.Register(this, OnWorldLoad);
        }

        #endregion

        #region onWorldLoad
        private void OnWorldLoad(EventArgs args)
        {
            //retrieve world name, send to api
            core.WorldName = "" + Main.worldID;
        }
        #endregion

        #region Chest Item Change Event
        private async void OnItemChange(object sender, ChestItemEventArgs e) // called when an item within a chest is changed
        {
            // get the chest object
            Chest chest = Main.chest[e.ID];
            if (chest == null) // <--- if the chest for whatever reason cannot be found, return
                return;

            // attempt to retrieve the Infinite Chest from database, based on position and the world name
            InfiniteChest? entity = await core.RetrieveChest(chest.x, chest.y);
            if (entity == null) // <---- this will occur when the chest is NOT an infinite chest, so return
                return;

            // if the chest doesn't have a world assigned to it, assign the current world to it
            // (for migrating v1.0 -> 1.1, populates potentially missing data)
            if (string.IsNullOrEmpty(entity.World))
                entity.World = core.WorldName;

            // loop through each item slot in the chest
            for (var i = 0; i < entity.Items.Count; i++)
            {
                // create a tempItem based on the data WE have collected for each slot
                Item tempItem = new Item();
                tempItem.SetDefaults(entity.Items[i].type);
                tempItem.stack = entity.Items[i].stack;
                tempItem.prefix = (byte)entity.Items[i].prefixID;

                // set the chest slot to our new temp item
                chest.item[i] = tempItem;

                // send a packet to the user, updating the chest slot in real time
                TSPlayer.All.SendData(PacketTypes.ChestItem, "", e.ID, i, tempItem.stack, tempItem.prefix, tempItem.type);

            }

            // tell the server that WE have handled this event, letting it know the server does not need to do anything further
            e.Handled = true;
        }
        #endregion

        #region Chest Open Event
        private async void OnChestOpen(object sender, ChestOpenEventArgs e) // called when a player opens a chest
        {
            // retrieve player
            var player = e.Player;
            if (player == null) // <---- if the player was somehow not available, return
                return;

            var plrState = player.GetPlayerOperationState();

            #region Add Infinite Chest
            if (plrState.InfChestAdd || plrState.InfChestAddBulk) // <----- if the player just executed /infchest add or addbulk
            {
                // find the chest the player opened
                Chest? chest = Main.chest.FirstOrDefault(x => x.x == e.X && x.y == e.Y, null);
                if (chest == null) // <----- if the chest is somehow null or not available, return
                    return;

                // set addinf to false, unless bulk operation
                if (!plrState.InfChestAddBulk)
                {
                    plrState.InfChestAdd = false;
                }

                // check if chest already is infinite
                var entity = await core.RetrieveChest(chest.x,chest.y);

                //if already infinite,
                if (entity != null) { player.SendErrorMessage("This chest is already infinite."); return; } // <---- inform the player and return

                // make an attempt to add the chest
                try
                {
                    core.AddChest(chest);
                }
                catch (Exception ex)  // inform the player if for some reason it doesn't work (and send error to console)
                {
                    Console.WriteLine("[SUPPLIER] - SOMETHING WENT WRONG!", ConsoleColor.Red);
                    Console.WriteLine(ex);
                    player.SendErrorMessage("Something went wrong! Chest could not be made infinite!");
                    return;
                }

                // success!
                player.SendSuccessMessage("Chest saved as infinite.");
            }
            #endregion
            #region Delete Inf Chest
            else if (plrState.InfChestDelete)
            {
                {
                    // retrieve opened chest
                    Chest? chest = Main.chest.FirstOrDefault(x => x.x == e.X && x.y == e.Y, null);
                    if (chest == null) // if it is somehow null, return
                        return;

                    plrState.InfChestDelete = false; // set delinf to false, to prevent further accidental deletion

                    // check to see if the chest already exists
                    var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == chest.x && x.Y == chest.y && x.World == core.WorldName));

                    // if it doesn't, tell the player
                    if (entity == null) { player.SendInfoMessage("This chest is not infinite already, are you selecting the correct chest?"); return; }

                    // attempt to delete the chest
                    try
                    {
                        core.RemoveChest(chest.x, chest.y);
                    }
                    catch (Exception ex)  // inform the player if for some reason it doesn't work (and send error to console)
                    {
                        Console.WriteLine("[SUPPLIER] - SOMETHING WENT WRONG!", ConsoleColor.Red);
                        Console.WriteLine(ex);
                        player.SendErrorMessage("Something went wrong! Chest could not be deleted.");
                        return;
                    }

                    // success!
                    player.SendSuccessMessage("Chest removed from infinite.");
                }
            }
            #endregion
        }
        #endregion
    }
}