using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Algorithm
{
    internal class SetSet<T> : HashSet<HashSet<T>>
    {
        public SetSet() : base() { }

        public SetSet<T> SetsCopy()
        {
            SetSet<T> copy = new SetSet<T>();
            foreach (HashSet<T> set in this)
            {
                copy.Add(new HashSet<T>(set));
            }
            return copy;
        }

        public bool ContainsInSet(T element)
        {
            foreach (HashSet<T> set in this)
            {
                if (set.Contains(element)) return true;
            }

            return false;
        }

        public HashSet<T> FindSet(T element)
        {
            foreach (HashSet<T> set in this)
            {
                if (set.Contains(element)) return set;
            }

            return new HashSet<T>();
        }

        public int SetsTotalCount()
        {
            int count = 0;
            foreach (HashSet<T> set in this)
            {
                count += set.Count;
            }
            return count;
        }
    }
}
