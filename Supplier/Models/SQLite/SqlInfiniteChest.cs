using Auxiliary;
using Microsoft.Xna.Framework;
using SQLite;
using Supplier.Api;
using Supplier.Models.SQLite;
using Terraria;

namespace Supplier.Models.Auxiliary
{
    public class SQLInfiniteChest : IInfiniteChest
    {
        [PrimaryKey, AutoIncrement] 
        public int Index { get; set; }
        public int ChestID { get; set; }

        [Indexed]
        public string World { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2 RetrievePosition() => new Vector2(X, Y);
        public int Delay { get; set; }


    }
}
