using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections; 

namespace WALT.DTO
{
     [Serializable()]
   public class WeeklyTask : Object 
    {
       public Task Task { get; set; }
       public string Comment { get; set; }
       public UnplannedCode UnplannedCode { get; set; }
       public int PlanDayComplete { get; set; }
       public int ActualDayComplete { get; set; }
       public long WeeklyPlanId { get; set; }
       
       public Dictionary<int, double> PlanHours;
       public Dictionary<int, double> ActualHours;
       public List<WeeklyBarrier> Barriers;    

       /// <summary>
       /// Constructor instantiates a new object for each collection.
       /// </summary>
       public WeeklyTask()
       {
           PlanHours = new Dictionary<int, double>();
           ActualHours = new Dictionary<int, double>();
           Barriers = new List<WeeklyBarrier>();
       }       
    }
}
