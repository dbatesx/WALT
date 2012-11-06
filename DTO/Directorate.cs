using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Directorate : Object
    {
        public string Name { get; set; }
        public List<Team> Teams { get; set; }
        public List<Profile> Admins { get; set; }
        public List<Profile> Managers { get; set; }
        public List<Barrier> Barriers { get; set; }
        public List<TaskType> TaskTypes { get; set; }
        public List<UnplannedCode> UnplannedCodes { get; set; }
        public List<string> OrgCodes { get; set; }

        public Directorate()
        {
            Teams = new List<Team>();
            Admins = new List<Profile>();
            Managers = new List<Profile>();
            Barriers = new List<Barrier>();
            TaskTypes = new List<TaskType>();
            UnplannedCodes = new List<UnplannedCode>();
            OrgCodes = new List<string>();
        }
    }
}
