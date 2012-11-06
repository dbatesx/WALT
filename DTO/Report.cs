using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Report : Object
    {
        public enum TypeEnum
        {
            PARETO_UNPLANNED_TABLE,
            PARETO_UNPLANNED_GRAPH,
            PARETO_UNPLANNED_RAW,
            PARETO_EFFICIENCY_BARRIERS_TABLE,
            PARETO_EFFICIENCY_BARRIERS_GRAPH,
            PARETO_EFFICIENCY_BARRIERS_RAW,
            PARETO_DELAY_BARRIERS_TABLE,
            PARETO_DELAY_BARRIERS_GRAPH,
            PARETO_DELAY_BARRIERS_RAW,
            OPERATING_SUMMARY,
            OPERATING_LOAD_TABLE,
            OPERATING_LOAD_GRAPH,
            OPERATING_BARRIER_TABLE,
            OPERATING_BARRIER_GRAPH,
            OPERATING_ADHERENCE_TABLE,
            OPERATING_ADHERENCE_GRAPH,
            OPERATING_ATTAINMENT_TABLE,
            OPERATING_ATTAINMENT_GRAPH,
            OPERATING_PRODUCTIVITY_TABLE,
            OPERATING_PRODUCTIVITY_GRAPH,
            OPERATING_UNPLANNED_TABLE,
            OPERATING_UNPLANNED_GRAPH,
            LOG_PARTICIPATION,
            SUMMARY_EFFICIENCY_BARRIER,
            SUMMARY_DELAY_BARRIER,
            SUMMARY_UNPLANNED,
            TEAM_INFO
        };

        public Profile Owner { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Boolean Public { get; set; }
        public ReportGroup Group { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TypeEnum Type { get; set; }
        public string Attributes { get; set; }

        /// <summary>
        /// Used on Operating Reports.
        /// Default to 0% for Unplanned and Efficiency Barriers.
        /// Otherwise 100%.
        /// </summary>
        public int PercentBase { get; set; }

        /// <summary>
        /// Used on Operating Reports.
        /// Default to 0% for Unplanned and Efficiency Barriers.
        /// Otherwise 100%.
        /// </summary>
        public int PercentGoal { get; set; }

        /// <summary>
        /// Set true when a montly operating report is requested.
        /// </summary>
        public bool IsMonthly { get; set; }

        public int LoadBase;
        public int LoadGoal;
        public int BarrierBase;
        public int BarrierGoal;
        public int AdherenceBase;
        public int AdherenceGoal;
        public int AttainmentBase;
        public int AttainmentGoal;
        public int ProductivityBase;
        public int ProductivityGoal;
        public int UnplannedBase;
        public int UnplannedGoal;

        public static string GetReportName(TypeEnum type)
        {
            if (type == TypeEnum.PARETO_UNPLANNED_TABLE)
            {
                return "Pareto - Unplanned Tasks - Table";
            }
            else if (type == TypeEnum.PARETO_UNPLANNED_GRAPH)
            {
                return "Pareto - Unplanned Tasks - Graph";
            }
            else if (type == TypeEnum.PARETO_UNPLANNED_RAW)
            {
                return "Pareto - Unplanned Tasks - Raw";
            }
            else if (type == TypeEnum.PARETO_EFFICIENCY_BARRIERS_TABLE)
            {
                return "Pareto - Task Efficiency Barriers - Table";
            }
            else if (type == TypeEnum.PARETO_EFFICIENCY_BARRIERS_GRAPH)
            {
                return "Pareto - Task Efficiency Barriers - Graph";
            }
            else if (type == TypeEnum.PARETO_EFFICIENCY_BARRIERS_RAW)
            {
                return "Pareto - Task Efficiency Barriers - Raw";
            }
            else if (type == TypeEnum.PARETO_DELAY_BARRIERS_TABLE)
            {
                return "Pareto - Task Delay Barriers - Table";
            }
            else if (type == TypeEnum.PARETO_DELAY_BARRIERS_GRAPH)
            {
                return "Pareto - Task Delay Barriers - Graph";
            }
            else if (type == TypeEnum.PARETO_DELAY_BARRIERS_RAW)
            {
                return "Pareto - Task Delay Barriers - Raw";
            }
            else if (type == TypeEnum.OPERATING_SUMMARY)
            {
                return "Operating Report Summary";
            }
            else if (type == TypeEnum.OPERATING_LOAD_TABLE)
            {
                return "Operating Report - Load - Table";
            }
            else if (type == TypeEnum.OPERATING_LOAD_GRAPH)
            {
                return "Operating Report - Load - Graph";
            }
            else if (type == TypeEnum.OPERATING_BARRIER_TABLE)
            {
                return "Operating Report - Efficiency Barrier Time - Table";
            }
            else if (type == TypeEnum.OPERATING_BARRIER_GRAPH)
            {
                return "Operating Report - Efficiency Barrier Time - Graph";
            }
            else if (type == TypeEnum.OPERATING_ADHERENCE_TABLE)
            {
                return "Operating Report - Plan Adherence - Table";
            }
            else if (type == TypeEnum.OPERATING_ADHERENCE_GRAPH)
            {
                return "Operating Report - Plan Adherence - Graph";
            }
            else if (type == TypeEnum.OPERATING_ATTAINMENT_TABLE)
            {
                return "Operating Report - Plan Attainment - Table";
            }
            else if (type == TypeEnum.OPERATING_ATTAINMENT_GRAPH)
            {
                return "Operating Report - Plan Attainment - Graph";
            }
            else if (type == TypeEnum.OPERATING_PRODUCTIVITY_TABLE)
            {
                return "Operating Report - Productivity - Table";
            }
            else if (type == TypeEnum.OPERATING_PRODUCTIVITY_GRAPH)
            {
                return "Operating Report - Productivity - Graph";
            }
            else if (type == TypeEnum.OPERATING_UNPLANNED_TABLE)
            {
                return "Operating Report - Unplanned - Table";
            }
            else if (type == TypeEnum.OPERATING_UNPLANNED_GRAPH)
            {
                return "Operating Report - Unplanned - Graph";
            }
            else if (type == TypeEnum.LOG_PARTICIPATION)
            {
                return "Auditing Report - Log Participation - Table";
            }
            else if (type == TypeEnum.SUMMARY_EFFICIENCY_BARRIER)
            {
                return "Summary Report - Efficiency Barrier - Table";
            }
            else if (type == TypeEnum.SUMMARY_DELAY_BARRIER)
            {
                return "Summary Report - Delay Barrier - Table";
            }
            else if (type == TypeEnum.SUMMARY_UNPLANNED)
            {
                return "Summary Report - Unplanned - Table";
            }
            else if (type == TypeEnum.TEAM_INFO)
            {
                return "WALT Team Information Report";
            }

            return string.Empty;
        }
    }
}
