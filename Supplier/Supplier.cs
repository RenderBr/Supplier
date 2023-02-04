using Auxiliary;
using CSF;
using CSF.TShock;
using MongoDB.Driver;
using NuGet.Protocol.Plugins;
using Supplier.Models;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using static TShockAPI.GetDataHandlers;

namespace Supplier
{
    [ApiVersion(2,1)]
    public class Supplier : TerrariaPlugin
    {
        private readonly TSCommandFramework _fx;
        public override string Name => "Supplier";
        public override string Description => "An infinite-chests successor, allows chests to be filled with items";

        public override string Author => "Average";

        public override Version Version => new Version(1, 0);
        
        public Supplier(Main game) : base(game)
        {
            _fx = new(new CommandConfiguration()
            {
                DoAsynchronousExecution = false
            });
        }

        public async override void Initialize()
        {
            await _fx.BuildModulesAsync(typeof(Supplier).Assembly);
            ChestOpen += OnChestOpen;
            ChestItemChange += OnItemChange;
        }

        private async void OnItemChange(object sender, ChestItemEventArgs e)
        {
            Chest chest = Main.chest[e.ID];
            if (chest == null)
                return;

            var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == chest.x && x.Y == chest.y));
            if (entity == null)
                return;

            for (var i = 0; i < entity.Items.Count; i++)
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(entity.Items[i].type);
                tempItem.stack = entity.Items[i].stack;
                tempItem.prefix = (byte)entity.Items[i].prefixID;

                chest.item[i] = tempItem;
                TSPlayer.All.SendData(PacketTypes.ChestItem, "", e.ID, i, tempItem.stack, tempItem.prefix, tempItem.type);

            }

            e.Handled = true;


        }

        private async void OnChestOpen(object sender, ChestOpenEventArgs e)
        {
            var player = e.Player;
            if (player == null)
                return;

            if (player.GetData<bool>("addinf"))
            {
                Chest? chest = Main.chest.FirstOrDefault(x => x.x == e.X && x.y == e.Y, null);
                if (chest == null)
                    return;

                player.SetData<bool>("addinf", false);

                await IModel.CreateAsync(CreateRequest.Bson<InfiniteChest>(x =>
                {
                    x.Index = Main.chest.ToList().IndexOf(chest);
                    x.X = chest.x;
                    x.Y = chest.y;
                    List <ChestItem> Items = new List<ChestItem>();

                    foreach(Item item in chest.item)
                    {
                        Items.Add(new ChestItem(item.type, item.stack, item.prefix));
                    }
                    x.Items = Items;
                }));
            
                player.SendSuccessMessage("Chest saved as infinite.");
            }
            else if (player.GetData<bool>("delinf"))
            {
                {
                    Chest? chest = Main.chest.FirstOrDefault(x => x.x == e.X && x.y == e.Y, null);
                    if (chest == null)
                        return;

                    player.SetData<bool>("delinf", false);

                    var entity = await IModel.GetAsync(GetRequest.Bson<InfiniteChest>(x => x.X == chest.x && x.Y == chest.y));
                    if (entity == null)
                        return;

                    await StorageProvider.GetMongoCollection<InfiniteChest>("InfiniteChests").FindOneAndDeleteAsync(x => x.X == e.X && x.Y == e.Y);

                    player.SendSuccessMessage("Chest removed from infinite.");
                }
            }
        }
    }
}