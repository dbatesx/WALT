using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
    public class Favorite : Object
    {
        public Profile Profile { get; set; }
        public string Title { get; set; }
        public TaskType TaskType { get; set; }
        public Program Program { get; set; }
        public Complexity Complexity { get; set; }
	    public double Hours { get; set; }
	    public double Estimate { get; set; }
        public bool Template { get; set; }
        public Dictionary<int, double> PlanHours { get; set; }

        public Favorite()
        {
            PlanHours = new Dictionary<int, double>();
        }
    }
}
