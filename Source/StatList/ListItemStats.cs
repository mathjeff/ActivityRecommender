using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatLists
{
    public class ListItemStats<KeyType, ValueType>
    {
        public ListItemStats(KeyType startingKey, ValueType startingValue)
        {
            this.key = startingKey;
            this.value = startingValue;
            //this.index = startingIndex;
        }
        public KeyType Key { get { return this.key; } }
        public ValueType Value { get { return this.value; } }
        //public int Index { get { return this.index; } }

        private KeyType key;
        private ValueType value;
        //private int index;
    }
}
