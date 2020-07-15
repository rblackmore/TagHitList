using ecom.TagHitList.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecom.TagHitList
{
    public static class ObservableCollectionExtensions
    {
        public static bool ContainsSerial(this ObservableCollection<TagRead> collection, string serialNumber)
        {
            foreach (TagRead tag in collection)
            {
                if (tag.SerialNumber == serialNumber)
                    return true;
            }

            return false;
        }

        public static void Sort<T>(this ObservableCollection<T> observable) where T : IComparable<T>, IEquatable<T>
        {
            List<T> sorted = observable.OrderBy(x => x).ToList();

            int ptr = 0;
            while (ptr < sorted.Count)
            {
                if (!observable[ptr].Equals(sorted[ptr]))
                {
                    T t = observable[ptr];
                    observable.RemoveAt(ptr);
                    observable.Insert(sorted.IndexOf(t), t);
                } else
                {
                    ptr++;
                }
            }
        }
    }
}
