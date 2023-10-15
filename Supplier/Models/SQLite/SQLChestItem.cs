using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier.Models.SQLite
{
    public class SQLChestItem
    {
        public SQLChestItem() { }
        public SQLChestItem(int _cid, int _type, int _stack, byte prefix, int slot)
        {
            ChestId = _cid;
            Type = _type;
            Stack = _stack;
            PrefixID = prefix;
            Slot = slot;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey("ChestId")]
        public int ChestId { get; set; }


        [Indexed]
        public int Type { get; set; }
        public int Stack { get; set; }
        public byte PrefixID { get; set; }

        public int Slot { get; set; }

    }
}
