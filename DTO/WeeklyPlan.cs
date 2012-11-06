using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WALT.DTO
{
     [Serializable()]
    public class WeeklyPlan : Object 
    {
        public Profile Profile { get; set; }
        public Profile PlanApprovedBy { get; set; }
        public Profile LogApprovedBy { get; set; }
        public Team Team { get; set; }
        public DateTime WeekEnding { get; set; }
        public DateTime? PlanSubmitted { get; set; }
        public DateTime? LogSubmitted { get; set; }

        public enum StatusEnum { NEW, PLAN_READY, PLAN_APPROVED, LOG_READY, LOG_APPROVED }
        public StatusEnum State { get; set; }

        public DateTime Modified { get; set; }
        public List<WeeklyTask> WeeklyTasks { get; set; }

        public bool? LeavePlanned;
        public Dictionary<int, double> LeavePlanHours;
        public Dictionary<int, double> LeaveActualHours;        

        /// <summary>
        /// Constructor instantiates a new object for each collection.
        /// </summary>
        public WeeklyPlan()
        {
            WeeklyTasks = new List<WeeklyTask>();
            LeavePlanHours = new Dictionary<int, double>();
            LeaveActualHours = new Dictionary<int, double>();
        }
    }
}
