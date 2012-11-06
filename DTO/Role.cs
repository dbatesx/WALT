using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Role : Object
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Action> Actions { get; set; }
        public bool Active { get; set; }

        public Role()
        {
            Actions = new List<Action>();
            Active = true;
        }

        public int GetSize()
        {
            int size = sizeof(long);
            size += Title.Length;
            size += Description.Length;
            size += Actions.Count();

            return size;
        }
    }
}
