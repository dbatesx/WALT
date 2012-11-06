using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
    [Serializable()]
    public class ADEntry
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string EmployeeID { get; set; }
        public string OrgCode { get; set; }
        public string Manager { get; set; }
    }
}
