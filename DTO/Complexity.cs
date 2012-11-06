using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
    [Serializable()]
    public class Complexity : Object
    {
        public string Title { get; set; }
        public double Hours { get; set; }
        public bool Active { get; set; }
        public TaskType TaskType { get; set; }
        public int SortOrder { get; set; }

        public int GetSize()
        {
            int size = sizeof(long);
            size += Title.Length;
            size += sizeof(double);
            size += sizeof(bool);

            return size;
        }
    }
}
