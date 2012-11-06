using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class WeeklyBarrier : Object
    {
        public enum BarriersEnum { EFFICIENCY, DELAY };

        public Barrier Barrier { get; set; }
        public string Comment { get; set; }
        public BarriersEnum BarrierType { get; set; }
        public string Ticket { get; set; }
        public long WeeklyTaskId { get; set; }
        public Dictionary<int, double> Hours;

        public WeeklyBarrier()
        {
            Hours = new Dictionary<int, double>();
        }
    }
}
