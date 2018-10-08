using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCast.Core.Helpers
{
    public static class Collection
    {
        public static IEnumerable<IList<T>> Spliter<T>(ref IEnumerable<T> collection, int splitNumber)
        {
            ICollection<IList<T>> _splitedCollection = new List<IList<T>>();
            int start = 0, end = splitNumber;
            int collectionLength = collection.Count();

            int splitCount = (collectionLength / splitNumber);
            int splitOverflow = (collectionLength % splitNumber);

            for (int x = 0; x < splitCount; ++x)
            {
                IList<T> _list = new List<T>();

                for (int y = start; y < end; ++y)
                    _list.Add(collection.ElementAt(y));

                _splitedCollection.Add(_list);
                start = end;
                end += splitNumber;
            }

            if (splitOverflow > 0)
            {
                IList<T> _list = new List<T>();
                for (int x = start; x < collectionLength; ++x)
                    _list.Add(collection.ElementAt(x));

                _splitedCollection.Add(_list);

            }

            start = end;
            start = 0;
            return _splitedCollection;
        }
    }
}
