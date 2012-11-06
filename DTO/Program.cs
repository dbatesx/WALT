using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Program : Object
    {
        public string Title { get; set; }
        public bool Active { get; set; }

        public Program()
        {
            Active = true;
        }

        public int GetSize()
        {
            int size = sizeof(long);
            size += Title.Length;
            size += sizeof(bool);

            return size;
        }
    }
}
