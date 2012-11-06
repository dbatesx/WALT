using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO // DTO: Data Transfer Object
{
     [Serializable()]
    public class Profile : Object
    {
        public string Username { get; set; }
        public bool ExemptPlan { get; set; }
        public bool ExemptTask { get; set; }
        public bool CanTask { get; set; }
        public string DisplayName { get; set; }
        public string EmployeeID { get; set; }
        public List<DTO.Role> Roles { get; set; }
        public bool Active { get; set; }
        public string OrgCode { get; set; }
        public Dictionary<string, string> Preferences { get; set; }
        public Profile Manager { get; set; }
        public Directorate Directorate { get; set; }

        public long ID
        {
            get { return Id; }
        }

        public Profile() : this(string.Empty)
        {
            //Roles = new List<Role>();
            //Preferences = new Dictionary<string, string>();
            //Active = true;
        }

        public Profile(string username)
        {
            Roles = new List<Role>();
            Preferences = new Dictionary<string, string>();
            Active = true;
            Username = username;
        }

        public int GetSize()
        {
            int size = sizeof(long);
            size += Username.Length;
            size += sizeof(bool);
            size += sizeof(bool);
            size += sizeof(bool);
            size += DisplayName.Length;
            size += EmployeeID != null ? EmployeeID.Length : 0;

            foreach (Role r in Roles)
            {
                size += r.GetSize();
            }

            size += sizeof(bool);
            size += OrgCode != null ? OrgCode.Length : 0;

            foreach (string k in Preferences.Keys)
            {
                size += k.Length;
                size += Preferences[k].Length;
            }

            if (Manager != null)
            {
                size += Manager.GetSize();
            }

            return size;
        }
    }
}
