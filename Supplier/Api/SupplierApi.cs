using Auxiliary;
using Auxiliary.Configuration;
using MongoDB.Driver;
using SQLite;
using Supplier.Models;
using Supplier.Models.Auxiliary;
using Supplier.Models.SQLite;
using Terraria;
using TShockAPI;

namespace Supplier.Api
{
    public class SupplierApi
    {
        public SupplierApi() {}
        private string dbPath = Path.Combine(TShock.SavePath, "supplier.sqlite");
        public SupplierSettings Config { get; set; }
        public static SQLiteAsyncConnection? SQL {get; set;}

        #region Add Chest
        public async Task AddChest(Chest chest, int delay)
        {
            int chestId = Main.chest.ToList().IndexOf(chest);

            if (Config.DatabaseType != "sqlite")
            {
                // add the chest to our database, marking it as infinite
                await IModel.CreateAsync(CreateRequest.Bson<InfiniteChest>(x =>
                {
                    x.ChestID = chestId;
                    x.X = chest.x;
                    x.Y = chest.y;
                    x.Delay = delay;
                    x.World = Main.worldName;

                    // Create a list storing all the item slots and the respective item slot data
                    var items = chest.item.Select((item, index) =>
                        new ChestItem(item.type, item.stack, item.prefix, index)).ToList();

                    x.Items = items;
                }));
            }
            else
            {
                SQLInfiniteChest x = new SQLInfiniteChest()
                {
                    ChestID = chestId,
                    X = chest.x,
                    Y = chest.y,
                    Delay = delay,
                    World = Main.worldName
                };

                // Create a list storing all the item slots and the respective item slot data
                var sqlChestItems = chest.item.Select((item, index) =>
                    new SQLChestItem(chestId, item.type, item.stack, item.prefix, index)).ToList();

                await SQL.InsertAllAsync(sqlChestItems);
                await SQL.InsertAsync(x);
            }
        }

        #endregion

        #region Retrieve Chest
        public async Task<IInfiniteChest>? RetrieveChest(int _x, int _y)
        {
            IInfiniteChest? chest;
            // retrieve the chest from the coordinates
            if(Config.DatabaseType != "sqlite")
                chest = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == _x && x.Y == _y && (x.World == Main.worldName || string.IsNullOrEmpty(x.World))));
            else
            {
                var chestQuery = SQL.Table<SQLInfiniteChest>().Where(x => x.X == _x && x.Y == _y && (x.World == Main.worldName || string.IsNullOrEmpty(x.World)));
                if (await chestQuery.CountAsync() > 0)
                {
                    chest = await chestQuery.FirstAsync();
                }
                else
                {
                    chest = null; // No matching data found
                }
            }

            // return the chest
            return chest;
        }
        #endregion

        #region Retrieve Chest Items
        public async Task<List<ChestItem>> RetrieveChestItems(int chestID)
        {
            List<ChestItem> chests = new List<ChestItem>();

            if (Config.DatabaseType != "sqlite")
            {
                var infiniteChest = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.ChestID == chestID && (x.World == Main.worldName || string.IsNullOrEmpty(x.World))));
                if (infiniteChest != null)
                {
                    chests = infiniteChest.Items;
                }
            }
            else
            {
                var chestItems = await SQL.Table<SQLChestItem>().Where(x => x.ChestId == chestID).ToListAsync();

                if (chestItems.Count > 0)
                {
                    chests.AddRange(chestItems.Select(ci => new ChestItem(ci.Type, ci.Stack, ci.PrefixID, ci.Slot)));
                }
            }

            return chests;
        }
        #endregion

        #region Remove Chest
        public async void RemoveChest(int _x, int _y)
        {
            if (Config.DatabaseType != "sqlite")
                await StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests").Find(x => x.X == _x && x.Y == _y && (x.World == Main.worldName || string.IsNullOrEmpty(x.World))).First().DeleteAsync();
            else
            {
                var chestID = (await RetrieveChest(_x, _y)).ChestID;
                await SQL.Table<SQLChestItem>().DeleteAsync(x=>x.ChestId==chestID);
                await SQL.Table<SQLInfiniteChest>().DeleteAsync(x => x.X == _x && x.Y == _y && (x.World == Main.worldName || string.IsNullOrEmpty(x.World)));
            }

        }
        #endregion

        #region Reload Config
        public async Task ReloadConfig() {
            Config = Configuration<SupplierSettings>.Load(nameof(Supplier));

            switch(Config.DatabaseType)
            {
                case "sqlite":
                    {
                        SQL = new SQLiteAsyncConnection(dbPath);
                        await SQL.CreateTableAsync<SQLInfiniteChest>();
                        await SQL.CreateTableAsync<SQLChestItem>();
                        Console.WriteLine("SupplierAPI has connected to the database successfully (via SQLite)");
                        break;

                    }
                case "mongodb":
                default:
                    {
                        StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests");
                        Console.WriteLine("SupplierAPI has connected to the database successfully (via Auxiliary)");
                        break;
                    }
            }
        }
        
        #endregion
    }
}
