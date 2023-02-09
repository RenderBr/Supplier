using Auxiliary;
using MongoDB.Driver;
using Supplier.Models;
using Terraria;

namespace Supplier.Api
{
    public class SupplierApi
    {
        public SupplierApi()
        {
            // this is done to see if a connection can even be made to auxiliary
            // it will also force Auxiliary to auto-gen a config
            StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests");
            Console.WriteLine("SupplierAPI has connected to the database successfully.");
        }

        public string? WorldName { get; set; }

        #region Add Chest
        public async void AddChest(Chest chest, int delay)
        {
            // add the chest to our database, marking it as infinite
            await IModel.CreateAsync(CreateRequest.Bson<InfiniteChest>(x =>
            {
                // retrieve chest world index
                x.Index = Main.chest.ToList().IndexOf(chest);

                // retrieve chest coordinates
                x.X = chest.x;
                x.Y = chest.y;

                // set the delay, will be in miliseconds
                x.Delay = delay;

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
        #endregion

        #region Retrieve Chest
        public async Task<InfiniteChest> RetrieveChest(int _x, int _y)
        {
            // retrieve the chest from the coordinates
            var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == _x && x.Y == _y && (x.World == WorldName || string.IsNullOrEmpty(x.World))));
            // return the chest
            return entity;
        }
        #endregion

        #region Remove Chest
        public async void RemoveChest(int _x, int _y)
        {
            await StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests").Find(x => x.X == _x && x.Y == _y && (x.World == WorldName || string.IsNullOrEmpty(x.World))).First().DeleteAsync();
        }
        #endregion
    }
}
