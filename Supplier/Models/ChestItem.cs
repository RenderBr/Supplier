using Auxiliary;

namespace Supplier.Models
{
    public class ChestItem : BsonModel
    {
        public int type;
        public int stack;
        public byte prefixID;

        public ChestItem(int _type, int _stack, byte prefix)
        {
            type = _type;
            stack = _stack;
            prefixID = prefix;
        }
    }
}
