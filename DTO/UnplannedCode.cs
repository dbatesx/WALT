using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class UnplannedCode : Object
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<UnplannedCode> Children { get; set; }
        public bool Active { get; set; }

        public UnplannedCode()
        {
            Children = new List<UnplannedCode>();
        }
    }
}
