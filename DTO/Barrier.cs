using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Barrier : Object 
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Barrier> Children { get; set; }
        public bool Active { get; set; }

        public Barrier()
        {
            Children = new List<Barrier>();
        }
    }
}
