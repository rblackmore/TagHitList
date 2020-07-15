using ecom.TagHitList.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecom.TagHitList.Framework
{
    public class TagReadEventArgs : EventArgs
    {
        public IEnumerable<TagRead> Tags { get; set; }

        public TagReadEventArgs(IEnumerable<TagRead> tags)
        {
            Tags = tags;
        }
    }
}
