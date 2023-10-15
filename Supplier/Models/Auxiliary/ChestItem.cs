using Auxiliary;

namespace Supplier.Models.Auxiliary
{
    public class ChestItem : BsonModel
    {
        public int type;
        public int stack;
        public byte prefixID;
        public int slot;

        public ChestItem(int _type, int _stack, byte prefix, int _slot)
        {
            type = _type;
            stack = _stack;
            prefixID = prefix;
            slot = _slot;
        }
    }
}
