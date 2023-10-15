using Microsoft.Xna.Framework;
using Supplier.Models.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier.Models
{
    public interface IInfiniteChest
    {
        public abstract Vector2 RetrievePosition();
        public int Delay { get; set; }
        public int ChestID { get; set; }
        public string World { get; set; }
    }
}
