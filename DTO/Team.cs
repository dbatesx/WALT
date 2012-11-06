using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Team : Object
    {
        public enum TypeEnum { ORG, DIRECTORATE, TEAM };

        public string Name { get; set; }
        public Profile Owner { get; set; }
        public List<Profile> Members { get; set; }
        public List<Profile> Admins { get; set; }
        public bool Active { get; set; }
        public bool ComplexityBased { get; set; }
        public TypeEnum Type { get; set; }
        public List<string> OrgCodes { get; set; }
        public List<Barrier> SelectedBarriers { get; set; }
        public List<UnplannedCode> SelectedUnplannedCodes { get; set; }
        public List<TaskType> SelectedTaskTypes { get; set; }

        public Team()
        {
            Members = new List<Profile>();
            Admins = new List<Profile>();
            OrgCodes = new List<string>();
            SelectedBarriers = new List<Barrier>();
            SelectedUnplannedCodes = new List<UnplannedCode>();
            SelectedTaskTypes = new List<TaskType>();
            Active = true;
            ComplexityBased = false;
            Type = TypeEnum.TEAM;
        }
    }
}
