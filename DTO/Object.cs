using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public abstract class Object
    {
        public long Id;
        public long ParentId;
    }
}
