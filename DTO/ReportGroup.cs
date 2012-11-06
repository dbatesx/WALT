using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
    public class ReportGroup : Object
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Profile Owner { get; set; }
        public Boolean Public { get; set; }
        public List<Profile> Profiles { get; set; }
        public List<Team> Teams { get; set; }
        public List<Directorate> Directorates { get; set; }
        public List<ReportGroup> Groups { get; set; }

        public ReportGroup()
        {
            Profiles = new List<Profile>();
            Teams = new List<Team>();
            Directorates = new List<Directorate>();
            Groups = new List<ReportGroup>();
        }
    }
}
