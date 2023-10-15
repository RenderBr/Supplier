using Auxiliary;
using Microsoft.Xna.Framework;

namespace Supplier.Models.Auxiliary
{
    public class InfiniteChest : BsonModel, IInfiniteChest
    {
        private int _chestID;
        public int ChestID
        {
            get
              => _chestID;
            set
            {
                _ = this.SaveAsync(x => x.ChestID, value);
                _chestID = value;
            }
        }

        private string? _world;
        public string? World
        {
            get
              => _world;
            set
            {
                _ = this.SaveAsync(x => x.World, value);
                _world = value;
            }
        }

        private List<ChestItem> _items = new List<ChestItem>();
        public List<ChestItem> Items
        {
            get
              => _items;
            set
            {
                _ = this.SaveAsync(x => x.Items, value);
                _items = value;
            }
        }

        private int _x;

        public int X
        {
            get
              => _x;
            set
            {
                _ = this.SaveAsync(x => x.X, value);
                _x = value;
            }
        }

        private int _y;
        public int Y
        {
            get
              => _y;
            set
            {
                _ = this.SaveAsync(x => x.Y, value);
                _y = value;
            }
        }

        private int delay = 0;
        public int Delay
        {
            get
              => delay;
            set
            {
                _ = this.SaveAsync(x => x.Delay, value);
                delay = value;
            }
        }

        Vector2 IInfiniteChest.RetrievePosition() => new Vector2(X,Y);

        List<ChestItem> RetrieveItems() => Items;
    }
}
