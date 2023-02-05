using Auxiliary;
using MongoDB.Driver;
using Supplier.Models;
using Terraria;

namespace Supplier.Api
{
    public class SupplierApi
    {
        public string? WorldName { get; set; }

        public async void AddChest(Chest chest)
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

        public async Task<InfiniteChest> RetrieveChest(int _x, int _y)
        {
            // retrieve the chest from the coordinates
            var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == _x && x.Y == _y && (x.World == WorldName || string.IsNullOrEmpty(x.World))));
            // return the chest
            return entity;
        }

        public async void RemoveChest(int _x, int _y)
        {
            StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests").Find(x => x.X == _x && x.Y == _y && (x.World == WorldName || string.IsNullOrEmpty(x.World))).First().DeleteAsync();        
        }
    }
}
