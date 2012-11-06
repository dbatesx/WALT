using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.BLL
{
    /// <summary>
    ///Unplanned 
    ///  i) Total Unplanned Hours
    ///  ii) % Unplanned Hours
    ///  iii) Total Hours Worked
    ///  iv) Hours for each Unplanned Code 
    /// 
    /// Efficiency Barrier
    ///  i) Total Identified Efficiency Barrier Time 
    ///  ii) % Efficiency Barrier Time
    ///  iii) Total Hours Worked
    ///  iv) Hours for each Efficiency Barrier Code 
    ///  
    /// Delay Barrier
    ///   i) Total Identified Delay Barrier Time 
    ///   ii) % Delay Barrier Time
    ///   iii) Total Hours Worked
    ///   iv) Hours for each Delay Barrier Code 
    /// </summary>
    public class ParetoReportTable
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime WeekEnding { get; set; }

        /// <summary>
        /// The title of the barrier / unplanned task.
        /// </summary>
        public string BarrierTitle { get; set; }

        /// <summary>
        /// The number of hours spent on the barrier / unplanned code
        /// </summary>
        public double BarrierHours { get; set; }

        /// <summary>
        /// The total number of hours worked
        /// </summary>
        public double TotalHoursWorked { get; set; }

        /// <summary>
        /// The Task Title as defined by the Task DTO ojbect.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// The Individual Contributor's Display Name as defined by the Profile DTO ojbject.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The user entered comment on this barrier.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// If this barrier has an Information System Trouble Ticket
        /// associated with it, this will be the unique ID
        /// used by the IS Help Desk.
        /// </summary>
        public string IsTicketNumber { get; set; }

        /// <summary>
        /// When exporting this class to a csv file using OperatingReportLoadToCsvList(),
        /// this method will return the header row as a string.
        /// </summary>
        /// <param name="type">The type (OperatingReportTypeEnum) of report being generated.</param>
        /// <returns>A comma separated string representing the header row for each class member.</returns>
        public string ParetoReportTableCsvHeader(DTO.Report.TypeEnum type)
        {
            StringBuilder result = new StringBuilder();

            result.Append("Week Ending"); result.Append("\t");

            switch (type)
            {
                case DTO.Report.TypeEnum.PARETO_UNPLANNED_RAW:
                case DTO.Report.TypeEnum.PARETO_UNPLANNED_GRAPH:
                case DTO.Report.TypeEnum.PARETO_UNPLANNED_TABLE:
                    result.Append("Unplanned Task Title"); result.Append("\t");
                    result.Append("Total Unplanned Hours"); result.Append("\t");
                    break;

                case DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_RAW:
                case DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_GRAPH:
                case DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_TABLE:
                    result.Append("Efficiency Barrier Title"); result.Append("\t");
                    result.Append("Total Identified Efficiency Barrier Time"); result.Append("\t");
                    break;

                case DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW:
                case DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_GRAPH:
                case DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_TABLE:
                    result.Append("Delay Barrier Title"); result.Append("\t");
                    result.Append("Total Identified Delay Barrier Time"); result.Append("\t");
                    break;

                default:
                    result.Append("Title"); result.Append("\t");
                    result.Append("Barrier Hours"); result.Append("\t");
                    break;
            }

            result.Append("Total Hours Worked"); result.Append("\t");
            result.AppendLine("Comment");

            return result.ToString();
        }

        /// <summary>
        /// When exporting this class to a csv file, this function will
        /// return a list of strings with each list entry being a row
        /// for the csv file. Each row will contain WeekEnding, BarrierTitle,
        /// BarrierHours, and TotalHoursWorked
        /// </summary>
        /// <returns>A list of strings where each item in the list is a week consisting
        /// of WeekEnding, BarrierTitle, BarrierHours, and TotalHoursWorked</returns>
        public List<string> ParetoReportTableToCsvList()
        {
            List<string> classAsStringList = new List<string>();
            StringBuilder result = new StringBuilder();

            // Ensure the date appears as MM/DD/YYYY
            result.Append(WeekEnding.ToShortDateString()); result.Append("\t");

            // Since we are making tab delimited file,
            // replace any tab with space.
            result.Append(BarrierTitle.Replace("\t", " ")); result.Append("\t");

            result.Append(BarrierHours); result.Append("\t");
            result.Append(TotalHoursWorked.ToString()); result.Append("\t");
            
            // Since we are making tab delimited file,
            // replace any tab with space.
            result.AppendLine(string.IsNullOrEmpty(Comment) ? String.Empty : Comment.Replace("\t", " ")); //result.Append("\n");
            classAsStringList.Add(result.ToString());

            return classAsStringList;
        }
    }

    /// <summary>
    /// Part of Report1, contains the Pareto Unplanned Code
    /// titles for the time period, and the hours spent.
    /// </summary>
    public class ParetoReportUnplanned_codeList
    {
        /// <summary>
        /// 
        /// </summary>
        public double totalHoursWorked { get; set; }

        /// <summary>
        /// The title of the unplanned code.
        /// </summary>
        public string unplannedCodeTitle { get; set; }

        /// <summary>
        /// The number of hours spent on the unplanned code
        /// </summary>
        public double unplannedCodeHours { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime WeekEnding { get; set; }
    }


    /// <summary>
    /// Part of Report2, contains the Pareto Efficiency Barrier
    /// titles for the time period, and the hours spent.
    /// </summary>
    public class ParetoReportBarriers_codeList
    {
        /// <summary>
        /// 
        /// </summary>
        public double totalHoursWorked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BarrierTitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double BarrierHours { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime WeekEnding { get; set; }
    }

    /// <summary>
    /// This class handles all Operating Report Tables.
    /// 
    /// Load
    ///   i)	Total Hours Planned (summary) (Dividend)
    ///   ii)	Total Hours Worked (summary)  (Divisor)
    ///   iii)	%Load = Total Hours Planned / Total Hours Worked
    ///   iv)	5-week or Quarter to Date Rolling Average Load 
    ///   v)	%Base
    ///   vi)	%Goal 
    ///   vii)	Variance from %Base
    ///
    /// Efficiency Barrier
    ///   i)	Total Efficiency Barrier Time (summary) (Dividend)
    ///   ii)	Total Hours Worked (summary)            (Divisor)
    ///   iii)	% Efficiency Barrier Time = Total Efficiency Barrier Time / Total Hours Worked
    ///   iv)	5-week or Quarter to Date Rolling Average Efficiency Barrier Time
    ///   v)	%Base 
    ///   vi)	%Goal 
    ///   vii)	Variance from %Base 
    ///
    /// Plan Adherence
    ///  i) Total Completed Planned Tasks (summary) (Dividend)
    ///  ii) Total Planned Tasks (summary)          (Divisor)
    ///  iii) % Plan Adherence = Total Completed Planned Tasks / Total Planned Tasks
    ///  iv) 5-week or Quarter to Date Rolling Average Plan Adherence
    ///  v) %Base 
    ///  vi) %Goal 
    ///  vii) Variance from %Base
    ///  
    /// Plan Attainment
    ///  i) Total Completed Tasks (summary) (Dividend)
    ///  ii) Total Planned Tasks (summary) (Divisor)
    ///  iii) % Plan Attainment = Total Completed Tasks / Total Planned Tasks
    ///  iv) 5-week or Quarter to Date Rolling Average Plan Attainment
    ///  v) %Base 
    ///  vi) %Goal 
    ///  vii) Variance from %Base
    ///  
    /// Productivity
    ///  i) Total Earned Hours (summary)   (Dividend)
    ///  ii) Total Hours Worked (summary) (Divisor)
    ///  iii) % Productivity = Total Earned Hours / Total Hours Worked
    ///  iv) 5-week or Quarter to Date Rolling Average Productivity
    ///  v) %Base 
    ///  vi) %Goal 
    ///  vii) Variance from %Base
    ///  
    /// Unplanned
    ///  i) Total Unplanned Hours (summary) (Dividend)
    ///  ii) Total Hours Worked (summary)  (Divisor)
    ///  iii) % Unplanned = Total Unplanned Hours / Total Hours Worked
    ///  iv) 5-week or Quarter to Date Rolling Average Unplanned
    ///  v) %Base 
    ///  vi) %Goal 
    ///  vii) Variance from %Base
    /// </summary>
    public class OperatingReportTable
    {
        /// <summary>
        /// This class is generic for all Operating Report Tables,
        /// therefore Dividend and Divisor may become meaningless.
        /// Therefore, this enumeration will be used for generating the header
        /// line when exporting to CSV.
        /// </summary>

        private DTO.Report.TypeEnum _type;

        /// <summary>
        /// 
        /// </summary>
        public string Type
        {
            get
            {
                return _type.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PercentageTitle
        {
            get
            {
                if (_type == DTO.Report.TypeEnum.OPERATING_LOAD_TABLE)
                {
                    return "% LOAD";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE)
                {
                    return "% BARRIER TIME";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE)
                {
                    return "% PLAN ADHERENCE";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE)
                {
                    return "% PLAN ATTAINMENT";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE)
                {
                    return "% PRODUCTIVITY";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE)
                {
                    return "% UNPLANNED";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DividendTitle
        {
            get
            {
                if (_type == DTO.Report.TypeEnum.OPERATING_LOAD_TABLE)
                {
                    return "Total Hours Planned";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE)
                {
                    return "Total Id'd Barrier Time";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE)
                {
                    return "Ttl Completed Planned";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE)
                {
                    return "Ttl Completed Tasks";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE)
                {
                    return "Total Earned Hours";
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE)
                {
                    return "Total Unplanned";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DivisorTitle
        {
            get
            {
                if (_type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE ||
                    _type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE)
                {
                    return "Ttl Planned Tasks";
                }
                else
                {
                    return "Total Hours Worked";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReportOrder
        {
            get
            {
                if (_type == DTO.Report.TypeEnum.OPERATING_LOAD_TABLE)
                {
                    return 0;
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE)
                {
                    return 1;
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE)
                {
                    return 2;
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE)
                {
                    return 3;
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE)
                {
                    return 4;
                }
                else if (_type == DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE)
                {
                    return 5;
                }

                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string group1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string group2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime WeekEnding { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double dividend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double dividend2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double divisor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double divisor2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PercentBase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PercentGoal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OperatingReportTable()
        {
        }

        /// <summary>
        /// Constructor for the OperatingReportTable Object sets the six week endings.
        /// </summary>
        /// <param name="weekEnding">The date of the most recent week ending requested. WeekEndings 2-5 set one week prior each.</param>
        public OperatingReportTable(DateTime weekEnding)
        {
            WeekEnding = weekEnding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="weekEnding"></param>
        /// <param name="percentBase"></param>
        /// <param name="percentGoal"></param>
        public OperatingReportTable(DTO.Report.TypeEnum type, DateTime weekEnding, int percentBase, int percentGoal)
        {
            _type = type;
            WeekEnding = weekEnding;
            PercentBase = percentBase;
            PercentGoal = percentGoal;
        }

        /// <summary>
        /// Constructor for the OperatingReportTable Object sets the six week endings.
        /// </summary>
        /// <param name="weekEnding">The date of the most recent week ending requested. WeekEndings 2-5 set one week prior each.</param>
        /// <param name="IsMonthly"></param>
        public OperatingReportTable(DateTime weekEnding, bool IsMonthly)
        {
            WeekEnding = WeekEnding;
        }

        /// <summary>
        /// When exporting this class to a csv file using OperatingReportLoadToCsvList(),
        /// this method will return the header row as a string.
        /// </summary>
        /// <returns>A comma separated string representing the header row for each class member.</returns>
        public string OperatingReportTableCsvHeader()
        {
            StringBuilder result = new StringBuilder();

            result.Append("Week Ending"); result.Append(", ");
            result.Append("Display Name"); result.Append(", ");
            result.Append("Team Name"); result.Append(", ");

            switch (_type)
            {
                case DTO.Report.TypeEnum.OPERATING_LOAD_TABLE:
                    result.Append("Total Hours Planned"); result.Append(", ");
                    result.AppendLine("Total Hours Worked");
                    break;

                case DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE:
                    result.Append("Total Efficiency Barrier Time"); result.Append(", ");
                    result.AppendLine("Total Hours Worked");
                    break;
                case DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE:
                    result.Append("Total Completed Planned Tasks "); result.Append(", ");
                    result.AppendLine("Total Planned Tasks");
                    break;
                case DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE:
                    result.Append("Total Completed Tass"); result.Append(", ");
                    result.AppendLine("Total Planned Tasks");
                    break;
                case DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE:
                    result.Append("Total Earned Hours"); result.Append(", ");
                    result.AppendLine("Total Hours Worked");
                    break;
                default:
                    result.Append("Dividend"); result.Append(", ");
                    result.AppendLine("Divisor");
                    break;
            }

            return result.ToString();
        }

        /// <summary>
        /// When exporting this class to a csv file, this function will
        /// return a list of strings with each list entry being a row
        /// for the csv file. Each row will contain WeekEnding,
        /// ProfileDisplayName, TeamName, dividend, and 
        /// divisor.
        /// </summary>
        /// <returns>A list of strings where each item in the list is a week consisting
        /// of WeekEnding, ProfileDisplayName, TeamName, dividend, and 
        /// divisor.</returns>
        public List<string> OperatingReportTableToCsvList()
        {
            List<string> classAsStringList = new List<string>();
            StringBuilder result = new StringBuilder();

            result.Append(WeekEnding); result.Append(", ");
            result.Append(group2); result.Append(", ");
            result.Append(group1); result.Append(", ");
            result.Append(dividend); result.Append(", ");
            result.AppendLine(divisor.ToString());
            classAsStringList.Add(result.ToString());

            return classAsStringList;
        }
    }

    /// <summary>
    /// This class handles all Operating Report Graphs.
    /// 
    /// Load
    /// b) Graph (only applies to weekly OR):
    ///  i) Axes:
    ///   (1) X Axis: weeks
    ///   (2) Y Axis: %
    ///  ii) Plotted Area:
    ///   (1) %Load (Line chart)
    ///   (2) 5-week Rolling Average Load (Line chart)
    ///   (3) %Base (Line chart)
    ///   (4) %Goal (Line chart)
    ///
    /// Efficiency Barrier 
    /// b) Graph:
    ///   i) Axes:
    ///    (1) X Axis: weeks or months
    ///    (2) Y Axis: %
    ///   ii) Plotted Area:
    ///    (1) % Efficiency Barrier Time (Line chart)
    ///    (2) 5-week Rolling Average Efficiency Barrier Time (Line chart)
    ///    (3) %Base (Line chart)
    ///    (4) %Goal (Line chart)
    /// 
    /// Plan Adherence
    /// b) Graph:
    ///  i) Axes:
    ///   (1) X Axis: weeks or months
    ///   (2) Y Axis: %
    ///  ii) Plotted Area:
    ///   (1) % Plan Adherence (Line chart)
    ///   (2) 5-week Rolling Average Plan Adherence (Line chart)
    ///   (3) %Base (Line chart)
    ///   (4) %Goal (Line chart)
    ///  
    /// Plan Attainment
    /// b) Graph:
    ///   i) Axes:
    ///    (1) X Axis: weeks or months
    ///    (2) Y Axis: %
    ///   ii) Plotted Area:
    ///    (1) % Plan Attainment (Line chart)
    ///    (2) 5-week Rolling Average Plan Attainment (Line chart)
    ///    (3) %Base (Line chart)
    ///    (4) %Goal (Line chart)
    ///    
    /// Productivity
    /// b) Graph:
    ///   i) Axes:
    ///    (1) X Axis: weeks or months
    ///    (2) Y Axis: %
    ///   ii) Plotted Area:
    ///    (1) % Productivity (Line chart)
    ///    (2) 5-week Rolling Average Productivity (Line chart)
    ///    (3) %Base (Line chart)
    ///    (4) %Goal (Line chart)
    /// 
    /// Unplanned
    /// b) Graph:
    ///   i) Axes:
    ///    (1) X Axis: weeks or months
    ///    (2) Y Axis: %
    ///   ii) Plotted Area:
    ///    (1) % Unplanned (Line chart)
    ///    (2) 5-week Rolling Average Unplanned (Line chart)
    ///    (3) %Base (Line chart)
    ///    (4) %Goal (Line chart)
    /// </summary>
    public class OperatingReportGraph
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime WeekEnding { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double dividend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double divisor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double weekAverage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double percentBase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double percentGoal { get; set; }
    }

    /// <summary>
    /// 5.4.2.4.1 [W2100] The system shall provide the following Summary
    /// report types with corresponding report data:
    /// 
    /// 1) Efficiency Barrier Summary
    ///   a) Data Table
    ///     i) Assignee
    ///     ii) Efficiency Barrier Code
    ///     iii) Efficiency Barrier Hours
    ///     iv) Date
    ///     v) Program
    ///     vi) Task
    ///     vii) Barrier Comments
    /// 2) Delay Barrier Summary 
    ///   a) Data Table
    ///     i) Assignee
    ///     ii) Delay Barrier Code
    ///     iii) Delay Barrier Hours
    ///     iv) Date
    ///     v) Program
    ///     vi) Task
    ///     vii) Barrier Comments
    /// 3) Unplanned Summary 
    ///   a) Data Table
    ///     i) Assignee
    ///     ii) Unplanned Code
    ///     iii) Unplanned Hours
    ///     iv) Date 
    ///     v) Program
    ///     vi) Task
    ///     vii) Assignee Comments
    /// </summary>
    public class SummaryReportTable
    {
        /// <summary>
        /// Type of report, "Efficiency" or "Delay" or "Unplanned"
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string assignee { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double hours { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string program { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string task { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string comment { get; set; }

    }

    /// <summary>
    /// 5.4.2.3.1 [W2090] The system shall provide the following Participation report types with corresponding report data.
    /// 1) Activity Log Participation
    ///  a) Data Table: 
    ///   i) Total number of ICs
    ///   ii) Total number of ICs required to use activity log planning (as defined by DA – those not exempt from planning/logging)
    ///   iii) Names of ICs exempt from activity log planning
    ///   iv) Total number of ICs completing weekly activity planning/logging (when a weekly activity log is ALM approved)
    ///   v) Names of ICs required to use activity log planning but not completing weekly activity plan/log
    ///   vi) % planned participation: (Total ICs required to use AL)/(Total ICs)
    ///   vii) % actual planned participation: (ICs completing AL)/ (ICs required to use AL)
    /// </summary>
    public class ParticipationReport
    {
        /// <summary>
        /// The total number of Individual Contributors (ICs).
        /// </summary>
        public int totalIc { get; set; }

        /// <summary>
        /// Total number of ICs required to use activity log planning
        /// (as defined by DA – those not exempt from planning/logging)
        /// </summary>
        public int totalNonExemptIc { get; set; }

        /// <summary>
        /// Total number of IC's completing all weekly plans in the given
        /// range of weeks.
        /// </summary>
        public int totalNotCompleting { get; set; }

        /// <summary>
        /// The profile name associated with this weekly plan.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool exempt { get; set; }

        /// <summary>
        /// Set to the week ending value of this weekly plan.
        /// </summary>
        public DateTime weekEnding { get; set; }

        /// <summary>
        /// Set true when this week ending's weekly plan has been completed.
        /// </summary>
        public bool completed { get; set; }

    }

    /// <summary>
    /// For the display of WALT teams and their members, Directorate,
    /// Owner (Activity Log Manager) and Backup Activity Log Managers.
    /// </summary>
    public class waltTeamInformation
    {
        /// <summary>
        /// The team to which the display name is associated.
        /// </summary>
        public string teamName { get; set; }

        /// <summary>
        /// AL Manager
        /// Backup AL Managers
        /// Directorate
        /// Members
        /// R/E Type
        /// Selected Barriers
        /// Selected Unplanned Codes
        /// Task Types
        /// </summary>
        public string dataTitle { get; set; }   

        /// <summary>
        /// The text to display with the dataTitle.
        /// </summary>
        public string data { get; set; }

        /// <summary>
        /// The Complexity data "Title (Hours)" for display
        /// next to 'Task Type' data rows.
        /// </summary>
        public string taskTypeComplexity { get; set; }
    }
}
