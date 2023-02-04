using Auxiliary;
using CSF;
using CSF.TShock;
using MongoDB.Driver;
using Supplier.Models;
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
        private string WorldName { get; set; }

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

            //register hooks
            ChestOpen += OnChestOpen;
            ChestItemChange += OnItemChange;
            TerrariaApi.Server.ServerApi.Hooks.GameInitialize.Register(this, OnWorldLoad);
        }

        #endregion

        #region onWorldLoad
        private void OnWorldLoad(EventArgs args)
        {
            //retrieve world name
            WorldName = Main.worldName;
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
            var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == chest.x && x.Y == chest.y && (x.World == WorldName || string.IsNullOrEmpty(x.World))));
            if (entity == null) // <---- this will occur when the chest is NOT an infinite chest, so return
                return;

            // if the chest doesn't have a world assigned to it, assign the current world to it
            // (for migrating v1.0 -> 1.1, populates potentially missing data)
            if (string.IsNullOrEmpty(entity.World))
                entity.World = WorldName;

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

            #region Add Infinite Chest
            if (player.GetData<bool>("addinf")) // <----- if the player just executed /infchest add
            {
                // find the chest the player opened
                Chest? chest = Main.chest.FirstOrDefault(x => x.x == e.X && x.y == e.Y, null);
                if (chest == null) // <----- if the chest is somehow null or not available, return
                    return;

                // set addinf to false
                // this is done so the user doesn't set any more chests to infinite accidentally
                player.SetData<bool>("addinf", false);

                // check if chest already is infinite
                var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == chest.x && x.Y == chest.y && (x.World == WorldName || string.IsNullOrEmpty(x.World))));

                //if already infinite,
                if (entity != null) { player.SendErrorMessage("This chest is already infinite."); return; } // <---- inform the player and return

                // make an attempt to add the chest
                try
                {
                    // add the chest to our database, marking it as infinite
                    await IModel.CreateAsync(CreateRequest.Bson<InfiniteChest>(x =>
                    {
                        // retrieve chest world index
                        x.Index = Main.chest.ToList().IndexOf(chest);

                        // retrieve chest coordinates
                        x.X = chest.x;
                        x.Y = chest.y;

                        // assign chests world
                        x.World = WorldName;

                        // create a list storing all the item slots, and the respective item slot data
                        List<ChestItem> Items = new List<ChestItem>();

                        // loop through each item in the existing chest
                        foreach (Item item in chest.item)
                        {
                            // and add each item to the list
                            Items.Add(new ChestItem(item.type, item.stack, item.prefix));
                        }

                        // add our new list to the database
                        x.Items = Items;
                    }));
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
            else if (player.GetData<bool>("delinf"))
            {
                {
                    // retrieve opened chest
                    Chest? chest = Main.chest.FirstOrDefault(x => x.x == e.X && x.y == e.Y, null);
                    if (chest == null) // if it is somehow null, return
                        return;

                    player.SetData<bool>("delinf", false); // set delinf to false, to prevent further accidental deletion

                    // check to see if the chest already exists
                    var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == chest.x && x.Y == chest.y && x.World == WorldName));

                    // if it doesn't, tell the player
                    if (entity == null) { player.SendInfoMessage("This chest is not infinite already, are you selecting the correct chest?"); return; }

                    // attempt to delete the chest
                    try
                    {
                        await StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests").FindOneAndDeleteAsync(x => x.X == e.X && x.Y == e.Y);
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