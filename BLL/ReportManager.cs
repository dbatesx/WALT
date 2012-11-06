using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WALT.BLL
{
    /// <summary>
    /// 
    /// </summary>
    public struct ReportCondition
    {
        /// <summary>
        /// 
        /// </summary>
        public string field;

        /// <summary>
        /// 
        /// </summary>
        public string op;

        /// <summary>
        /// 
        /// </summary>
        public string val;

        /// <summary>
        /// 
        /// </summary>
        public ReportFilter? filter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rf"></param>
        public ReportCondition(ReportFilter? rf)
        {
            this.field = string.Empty;
            this.op = string.Empty;
            this.val = string.Empty;
            this.filter = rf;
        }
    };

    /// <summary>
    /// 
    /// </summary>
    public struct ReportFilter
    {
        /// <summary>
        /// 
        /// </summary>
        public string name;

        /// <summary>
        /// 
        /// </summary>
        public string type;

        /// <summary>
        /// 
        /// </summary>
        public string relation;

        /// <summary>
        /// 
        /// </summary>
        public List<ReportCondition> conditions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="init"></param>
        public ReportFilter(bool init)
        {
            this.name = string.Empty;
            this.type = string.Empty;
            this.relation = string.Empty;
            this.conditions = new List<ReportCondition>();
        }
    };

    /// <summary>
    /// The ReportManager class inherits from the Manager class, and adds
    /// functionality to pull the necessary data from the database
    /// for built-in report generation.
    /// </summary>
    public class ReportManager : Manager
    {
        private static string _id = typeof(ReportManager).ToString();
        private List<DTO.WeeklyPlan> _weeklyPlans;
        private List<string> _groupTitles1;
        private List<string> _groupTitles2;
        private List<long> _teamProfiles;
        private List<DateTime> _weekEnding;
        private Dictionary<long, bool> _teamAllowed;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ReportManager GetInstance()
        {
            ReportManager m = (ReportManager)GetSessionValue(_id);

            if (m == null)
            {
                m = new ReportManager();
                SetSessionValue(_id, m);
            }

            return m;
        }

        /// <summary>
        /// Contructor for ReportManager
        /// </summary>
        public ReportManager()
            : base()
        {
            _weeklyPlans = null;
            _groupTitles1 = null;
            _groupTitles2 = null;
            _teamProfiles = null;
            _weekEnding = null;
            _teamAllowed = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (_weeklyPlans != null)
            {
                _weeklyPlans.Clear();
                _weeklyPlans = null;
            }

            if (_groupTitles1 != null)
            {
                _groupTitles1.Clear();
                _groupTitles1 = null;
            }

            if (_groupTitles2 != null)
            {
                _groupTitles2.Clear();
                _groupTitles2 = null;
            }

            if (_teamProfiles != null)
            {
                _teamProfiles.Clear();
                _teamProfiles = null;
            }

            if (_weekEnding != null)
            {
                _weekEnding.Clear();
                _weekEnding = null;
            }

            if (_teamAllowed != null)
            {
                _teamAllowed.Clear();
                _teamAllowed = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        public List<DTO.Team> GetReportTeams(DTO.ReportGroup group)
        {
            List<DTO.Team> teams = new List<DTO.Team>();

            try
            {
                LoadTeams(group, teams);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return teams;
        }

        private void LoadTeams(DTO.ReportGroup group, List<DTO.Team> teams)
        {
            List<long> teamIDs = teams.Select(x => x.Id).ToList();

            // Add each team from each directorate in the report.
            foreach (DTO.Directorate d in group.Directorates)
            {
                List<DTO.Team> dirTeams = BLL.AdminManager.GetInstance().GetTeamsByParent(d.Id, true);
                dirTeams = dirTeams.Where(x => !teamIDs.Contains(x.Id)).ToList();
                teams.AddRange(dirTeams);
                teamIDs.AddRange(dirTeams.Select(x => x.Id));
            }

            // Add each team in the report
            foreach (DTO.Team team in group.Teams)
            {
                if (!teamIDs.Contains(team.Id))
                {
                    teams.Add(BLL.AdminManager.GetInstance().GetTeam(team.Id, true));
                    teamIDs.Add(team.Id);
                }
            }

            foreach (DTO.Profile p in group.Profiles)
            {
                DTO.Team team = _dalMediator.GetAdminProcessor().GetTeam(p);

                if (!teamIDs.Contains(team.Id))
                {
                    teams.Add(BLL.AdminManager.GetInstance().GetTeam(team.Id, true));
                    teamIDs.Add(team.Id);
                }
            }

            foreach (DTO.ReportGroup g in group.Groups)
            {
                LoadTeams(g, teams);
            }
        }

        /// <summary>
        /// Returns the "Hours for each Unplanned Code" part
        /// of the Unplanned Pareto.
        /// </summary>
        /// <returns></returns>
        public List<ParetoReportUnplanned_codeList> GetParetoReportUnplannedCodeList(DTO.Report spec)
        {
            List<ParetoReportUnplanned_codeList> codeList = new List<ParetoReportUnplanned_codeList>();

            try
            {
                ParetoReportUnplanned_codeList report;
                SetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    foreach (DTO.WeeklyTask weeklyTask in wp.WeeklyTasks)
                    {
                        report = new ParetoReportUnplanned_codeList();
                        report.WeekEnding = wp.WeekEnding;
                        report.totalHoursWorked = weeklyTask.ActualHours.Values.Sum();

                        if (weeklyTask.UnplannedCode != null)
                        {
                            report.unplannedCodeTitle = weeklyTask.UnplannedCode.Code + " >> " + weeklyTask.UnplannedCode.Title;
                            report.unplannedCodeHours = report.totalHoursWorked;
                        }
                        else
                        {
                            report.unplannedCodeTitle = string.Empty;
                            report.unplannedCodeHours = 0;
                        }

                        codeList.Add(report);
                    }
                }

                // Need a row for every week
                List<DateTime> codeWeeks = codeList.Select(x => x.WeekEnding).Distinct().ToList();

                // If there aren't as many weeks in the codeList as
                // the user requested, we need some dummy weeks added.
                if (codeWeeks.Count < _weekEnding.Count)
                {
                    foreach (DateTime d in _weekEnding)
                    {
                        if (!codeWeeks.Contains(d))
                        {
                            report = new ParetoReportUnplanned_codeList();
                            report.WeekEnding = d;
                            report.totalHoursWorked = 0;
                            report.unplannedCodeHours = 0;
                            report.unplannedCodeTitle = string.Empty;
                            codeList.Add(report);
                        }
                    }
                }

                if (spec.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_GRAPH)
                {
                    List<string> codes = codeList.Where(x => !string.IsNullOrEmpty(x.unplannedCodeTitle)).Select(
                        x => x.unplannedCodeTitle).Distinct().ToList();

                    if (codes.Count > 10)
                    {
                        Dictionary<string, double> codeHours = new Dictionary<string, double>();

                        foreach (string code in codes)
                        {
                            codeHours.Add(code, codeList.Where(x => x.unplannedCodeTitle == code).Sum(x => x.unplannedCodeHours));
                        }

                        List<string> removeCodes = codeHours.OrderByDescending(x => x.Value).Select(x => x.Key).Skip(10).ToList();

                        foreach (ParetoReportUnplanned_codeList cl in codeList)
                        {
                            if (removeCodes.Contains(cl.unplannedCodeTitle))
                            {
                                cl.unplannedCodeTitle = string.Empty;
                            }
                        }
                    }
                    else if (codes.Count < 10)
                    {
                        // Pareto report needs top 10. If less, pad.
                        for (int i = codes.Count; i < 10; i++)
                        {
                            report = new ParetoReportUnplanned_codeList();

                            // Each title must be unique or the report will
                            // group them together. Use up to 10 spaces.
                            for (int j = 0; j < i; j++)
                            {
                                report.unplannedCodeTitle += " ";
                            }

                            report.totalHoursWorked = 0;
                            report.unplannedCodeHours = 0;
                            report.WeekEnding = _weekEnding.First();
                            codeList.Add(report);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            // Order by WeekEnding for Last()/First() calls in report.
            return codeList.OrderBy(x => x.WeekEnding).ToList();
        }

        /// <summary>
        /// Returns the "Hours for each Unplanned Code" part
        /// of the Unplanned Pareto.
        /// 
        /// This function is used for the Download functionality for
        /// Pareto Report - Unplanned Code.
        /// </summary>
        /// <returns>List of raw data for the report.</returns>
        /// <param name="spec">The data for the report to be generated.</param>
        /// <returns></returns>
        public List<ParetoReportTable> DownloadParetoReportUnplannedCode(DTO.Report spec)
        {
            List<ParetoReportTable> report = new List<ParetoReportTable>();

            try
            {
                ParetoReportTable reportItem;
                SetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    foreach (DTO.WeeklyTask weeklyTask in wp.WeeklyTasks)
                    {
                        // Add up the total hours worked on the weekly task
                        double totalHoursWorked = weeklyTask.ActualHours.Values.Sum();

                        for (int i = 0; i < weeklyTask.ActualHours.Count - 1; i++)
                        {
                            if (weeklyTask.ActualHours[i] > 0)
                            {
                                reportItem = new ParetoReportTable();
                                reportItem.BarrierHours = weeklyTask.ActualHours[i];
                                reportItem.TotalHoursWorked = totalHoursWorked;
                                reportItem.WeekEnding = wp.WeekEnding.AddDays(i - 6);
                                reportItem.BarrierTitle = weeklyTask.UnplannedCode.Code + " >> " + weeklyTask.UnplannedCode.Title;
                                reportItem.Comment = weeklyTask.Comment;

                                // Hide the Individual Contributor from those
                                // that don't have permission to see it.
                                reportItem.DisplayName = IsAllowedViewIcs(wp.Team) ? wp.Profile.DisplayName : "Individual Contributor";
                                reportItem.TaskTitle = weeklyTask.Task.Title;
                                reportItem.IsTicketNumber = String.Empty;
                                report.Add(reportItem);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return report;
        }

         /// <summary>
        /// Returns the "Hours for each Efficiency Barrier" part
        /// of the Efficiency Barrier Pareto.
        /// </summary>
        /// <returns></returns>
        public List<ParetoReportBarriers_codeList> GetParetoReportBarriers(DTO.Report spec)
        {
            List<ParetoReportBarriers_codeList> codeList = new List<ParetoReportBarriers_codeList>();
            
            try
            {
                ParetoReportBarriers_codeList report;
                SetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    foreach (DTO.WeeklyTask weeklyTask in wp.WeeklyTasks)
                    {
                        report = new ParetoReportBarriers_codeList();
                        report.WeekEnding = wp.WeekEnding;
                        report.totalHoursWorked = weeklyTask.ActualHours.Values.Sum();
                        report.BarrierHours = 0;
                        report.BarrierTitle = string.Empty;
                        codeList.Add(report);

                        foreach (DTO.WeeklyBarrier wb in weeklyTask.Barriers)
                        {
                            report = new ParetoReportBarriers_codeList();
                            report.WeekEnding = wp.WeekEnding;
                            report.totalHoursWorked = 0;
                            report.BarrierTitle = wb.Barrier.Code + " >> " + wb.Barrier.Title;
                            report.BarrierHours = wb.Hours.Values.Sum();
                            codeList.Add(report);
                        }
                    }
                }

                // Need a row for every week
                List<DateTime> codeWeeks = codeList.Select(x => x.WeekEnding).Distinct().ToList();

                // If there aren't as many weeks in the codeList as
                // the user requested, we need some dummy weeks added.
                if (codeWeeks.Count < _weekEnding.Count)
                {
                    foreach (DateTime d in _weekEnding)
                    {
                        if (!codeWeeks.Contains(d))
                        {
                            report = new ParetoReportBarriers_codeList();
                            report.WeekEnding = d;
                            report.totalHoursWorked = 0;
                            report.BarrierHours = 0;
                            report.BarrierTitle = string.Empty;
                            codeList.Add(report);
                        }
                    }
                }

                if (spec.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_GRAPH ||
                    spec.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_GRAPH)
                {
                    List<string> codes = codeList.Where(x => !string.IsNullOrEmpty(x.BarrierTitle)).Select(
                        x => x.BarrierTitle).Distinct().ToList();

                    if (codes.Count > 10)
                    {
                        Dictionary<string, double> codeHours = new Dictionary<string, double>();

                        foreach (string code in codes)
                        {
                            codeHours.Add(code, codeList.Where(x => x.BarrierTitle == code).Sum(x => x.BarrierHours));
                        }

                        List<string> removeCodes = codeHours.OrderByDescending(x => x.Value).Select(x => x.Key).Skip(10).ToList();

                        foreach (ParetoReportBarriers_codeList cl in codeList)
                        {
                            if (removeCodes.Contains(cl.BarrierTitle))
                            {
                                cl.BarrierTitle = string.Empty;
                            }
                        }
                    }
                    else if (codes.Count < 10)
                    {
                        // Pareto report needs top 10. If less, pad.
                        for (int i = codes.Count; i < 10; i++)
                        {
                            report = new ParetoReportBarriers_codeList();

                            // Each title must be unique or the report will
                            // group them together. Use up to 10 spaces.
                            for (int j = 0; j < i; j++)
                            {
                                report.BarrierTitle += " ";
                            }

                            report.totalHoursWorked = 0;
                            report.BarrierHours = 0;
                            report.WeekEnding = _weekEnding.First();
                            codeList.Add(report);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            // Order by WeekEnding for Last()/First() calls in report.
            return codeList.OrderBy(x => x.WeekEnding).ToList();
        }

        /// <summary>
        /// Returns the "Hours for each Efficiency Barrier" part
        /// of the Efficiency Barrier Pareto.
        /// 
        /// This function is used for the Download functionality for
        /// Pareto Report - Efficiency Barrier.
        /// </summary>
        /// <returns></returns>
        public List<ParetoReportTable> DownloadParetoReportBarriers(DTO.Report spec)
        {
            List<ParetoReportTable> report = new List<ParetoReportTable>();

            try
            {
                ParetoReportTable reportItem;
                SetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    foreach (DTO.WeeklyTask weeklyTask in wp.WeeklyTasks)
                    {
                        // Add up the total hours worked on the weekly task
                        double totalHoursWorked = weeklyTask.ActualHours.Values.Sum();

                        foreach (DTO.WeeklyBarrier wb in weeklyTask.Barriers)
                        {
                            for (int i = 0; i < wb.Hours.Count - 1; i++)
                            {
                                if (wb.Hours[i] > 0)
                                {
                                    reportItem = new ParetoReportTable();
                                    reportItem.TotalHoursWorked = totalHoursWorked;
                                    reportItem.WeekEnding = wp.WeekEnding.AddDays(i - 6);
                                    reportItem.BarrierTitle = wb.Barrier.Code + " >> " + wb.Barrier.Title;
                                    reportItem.Comment = wb.Comment;
                                    reportItem.BarrierHours = wb.Hours[i];
                                    reportItem.DisplayName = IsAllowedViewIcs(wp.Team) ? wp.Profile.DisplayName : "Individual Contributor";
                                    reportItem.TaskTitle = weeklyTask.Task.Title;
                                    reportItem.IsTicketNumber = wb.Ticket;
                                    report.Add(reportItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return report;
        }

        /// <summary>
        /// Check if the current user is able to see the
        /// Individual Contributors in the given team.
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        private bool IsAllowedViewIcs(DTO.Team team)
        {
            /* If you own the team, you can see the IC's.
             * DMs shall be able to see all ICs in the ALTs in their directorate
             * ALMs shall be able to see all ICs in their ALTs (as owner or backup)
             * 
             * Helpful note: The parent_id of the team is the directorate team.
             */

            bool allowed;

            if (_teamAllowed == null)
            {
                _teamAllowed = new Dictionary<long, bool>();
            }

            if (!_teamAllowed.ContainsKey(team.Id))
            {
                allowed = (_profile.Id == team.Owner.Id ||
                    (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_MANAGE) &&
                     IsDirectorateManager(team.ParentId)) ||
                    (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TEAM_MANAGE) &&
                     IsActivityLogManager(team.Id)));

                _teamAllowed.Add(team.Id, allowed);
            }
            else
            {
                allowed = _teamAllowed[team.Id];
            }

            return allowed;
        }

        /// <summary>
        /// Check if the current user is a
        /// Directorate Manager for the given Directorate team.
        /// </summary>
        /// <param name="directorateTeamId">Check if the current user is the Directorate Manager for this Directorate team.</param>
        /// <returns>True if the current user is the Directorate Manager for the given team.</returns>
        private bool IsDirectorateManager(long directorateTeamId)
        {
            return _dalMediator.GetReportProcessor().IsDirectorateManager(directorateTeamId, _profile.Id);
        }

        /// <summary>
        /// Check if the current user is an
        /// Activity Log Manager for the given team.
        /// </summary>
        /// <param name="team_id">Check if the current user is the Activity Log Manager for this team.</param>
        /// <returns>True if the current user is the Activity Log Manager for the given team.</returns>
        private bool IsActivityLogManager(long team_id)
        {
            return _dalMediator.GetReportProcessor().IsActivityLogManager(team_id, _profile.Id);
        }

        /// <summary>
        /// Given a team.Id, return the profile id of the owner.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns>The profile Id of the owner.</returns>
        private long GetTeamOwner(long teamId)
        {
            return _dalMediator.GetReportProcessor().GetTeamOwner(teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public List<OperatingReportTable> GetOperatingReportSummary(DTO.Report spec)
        {
            GetWeekEndingList(spec);
            LoadReportData(spec);

            spec.PercentBase = spec.LoadBase;
            spec.PercentGoal = spec.LoadGoal;
            spec.Type = DTO.Report.TypeEnum.OPERATING_LOAD_TABLE;
            List<OperatingReportTable> reports = GetOperatingReport(spec);

            spec.PercentBase = spec.BarrierBase;
            spec.PercentGoal = spec.BarrierGoal;
            spec.Type = DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE;
            reports.AddRange(GetOperatingReport(spec));

            spec.PercentBase = spec.AdherenceBase;
            spec.PercentGoal = spec.AdherenceGoal;
            spec.Type = DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE;
            reports.AddRange(GetOperatingReport(spec));

            spec.PercentBase = spec.AttainmentBase;
            spec.PercentGoal = spec.AttainmentGoal;
            spec.Type = DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE;
            reports.AddRange(GetOperatingReport(spec));

            spec.PercentBase = spec.ProductivityBase;
            spec.PercentGoal = spec.ProductivityGoal;
            spec.Type = DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE;
            reports.AddRange(GetOperatingReport(spec));

            spec.PercentBase = spec.UnplannedBase;
            spec.PercentGoal = spec.UnplannedGoal;
            spec.Type = DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE;
            reports.AddRange(GetOperatingReport(spec));

            return reports;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spec"></param>
        private void LoadReportData(DTO.Report spec)
        {
            if (_weeklyPlans == null)
            {
                bool loadPlanned = false;
                bool loadUnplanned = false;
                bool loadUnplannedCodes = false;
                bool loadTaskInfo = false;
                bool loadTaskPrograms = false;
                bool loadTaskHours = false;
                bool loadEffBarrier = false;
                bool loadDelayBarrier = false;
                bool loadBarrierInfo = false;
                bool loadBarrierHours = false;

                if (spec.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadUnplannedCodes = true;
                    loadTaskHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_RAW)
                {
                    loadUnplanned = true;
                    loadUnplannedCodes = true;
                    loadTaskInfo = true;
                    loadTaskHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskHours = true;
                    loadEffBarrier = true;
                    loadBarrierInfo = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_RAW)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskInfo = true;
                    loadTaskHours = true;
                    loadEffBarrier = true;
                    loadBarrierInfo = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskHours = true;
                    loadDelayBarrier = true;
                    loadBarrierInfo = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskInfo = true;
                    loadTaskHours = true;
                    loadDelayBarrier = true;
                    loadBarrierInfo = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_SUMMARY)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskInfo = true;
                    loadTaskHours = true;
                    loadEffBarrier = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_LOAD_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.OPERATING_LOAD_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.OPERATING_BARRIER_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskHours = true;
                    loadEffBarrier = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_GRAPH)
                {
                    loadPlanned = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskInfo = true;
                    loadTaskHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE ||
                    spec.Type == DTO.Report.TypeEnum.OPERATING_UNPLANNED_GRAPH)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.SUMMARY_EFFICIENCY_BARRIER)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskInfo = true;
                    loadTaskPrograms = true;
                    loadEffBarrier = true;
                    loadBarrierInfo = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.SUMMARY_DELAY_BARRIER)
                {
                    loadPlanned = true;
                    loadUnplanned = true;
                    loadTaskInfo = true;
                    loadTaskPrograms = true;
                    loadDelayBarrier = true;
                    loadBarrierInfo = true;
                    loadBarrierHours = true;
                }
                else if (spec.Type == DTO.Report.TypeEnum.SUMMARY_UNPLANNED)
                {
                    loadUnplanned = true;
                    loadUnplannedCodes = true;
                    loadTaskInfo = true;
                    loadTaskPrograms = true;
                    loadTaskHours = true;
                }

                _groupTitles1 = new List<string>();
                _groupTitles2 = new List<string>();
                _weeklyPlans = _dalMediator.GetReportProcessor().GetReportData(
                    spec.Group, _weekEnding.Last(), _weekEnding[0], _groupTitles1, _groupTitles2,
                    loadPlanned, loadUnplanned, loadUnplannedCodes, loadTaskInfo, loadTaskPrograms, loadTaskHours,
                    loadEffBarrier, loadDelayBarrier, loadBarrierInfo, loadBarrierHours);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        /// <param name="wp"></param>
        /// <param name="reportType"></param>
        /// <param name="dupe"></param>
        private void CalculateOperatingReport(
            OperatingReportTable report,
            DTO.WeeklyPlan wp,
            DTO.Report.TypeEnum reportType,
            bool dupe
        )
        {
            CalculateOperatingReport(report, null, wp, reportType, dupe);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        /// <param name="wp"></param>
        /// <param name="reportType"></param>
        private void CalculateOperatingReport(
            OperatingReportGraph report,
            DTO.WeeklyPlan wp,
            DTO.Report.TypeEnum reportType
        )
        {
            CalculateOperatingReport(null, report, wp, reportType, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportTable"></param>
        /// <param name="reportGraph"></param>
        /// <param name="wp"></param>
        /// <param name="reportType"></param>
        /// <param name="dupe"></param>
        private void CalculateOperatingReport(
            OperatingReportTable reportTable,
            OperatingReportGraph reportGraph,
            DTO.WeeklyPlan wp,
            DTO.Report.TypeEnum reportType,
            bool dupe
        )
        {
            double dividend = 0;
            double divisor = 0;
            double percentBase = 1; // 100%
            double percentGoal = 1; // 100%

            if (reportType == DTO.Report.TypeEnum.OPERATING_LOAD_TABLE ||
                reportType == DTO.Report.TypeEnum.OPERATING_LOAD_GRAPH)
            {
                dividend = wp.WeeklyTasks.Sum(x => x.PlanHours.Values.Sum());
                divisor = wp.WeeklyTasks.Sum(x => x.ActualHours.Values.Sum());
            }
            else if (reportType == DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE ||
                reportType == DTO.Report.TypeEnum.OPERATING_BARRIER_GRAPH)
            {
                dividend = wp.WeeklyTasks.Sum(t => t.Barriers.Where(
                    b => b.BarrierType == DTO.WeeklyBarrier.BarriersEnum.EFFICIENCY).Sum(x => x.Hours.Values.Sum()));

                divisor = wp.WeeklyTasks.Sum(x => x.ActualHours.Values.Sum());

                percentBase = 0; // 0 %
                percentGoal = 0; // 0 %
            }
            else if (reportType == DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE ||
                reportType == DTO.Report.TypeEnum.OPERATING_ADHERENCE_GRAPH)
            {
                dividend = wp.WeeklyTasks.Where(x => x.UnplannedCode == null).Count(x => x.ActualDayComplete != -1);
                divisor = wp.WeeklyTasks.Count(x => x.PlanDayComplete != -1);
            }
            else if (reportType == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE ||
                reportType == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_GRAPH)
            {
                dividend = wp.WeeklyTasks.Count(x => x.ActualDayComplete != -1);
                divisor = wp.WeeklyTasks.Count(x => x.PlanDayComplete != -1);
            }
            else if (reportType == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE ||
                reportType == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_GRAPH)
            {
                dividend = wp.WeeklyTasks.Where(x => x.ActualDayComplete != -1).Sum(x => x.Task.Estimate);
                divisor = wp.WeeklyTasks.Sum(x => x.ActualHours.Values.Sum());
            }
            else if (reportType == DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE ||
                reportType == DTO.Report.TypeEnum.OPERATING_UNPLANNED_GRAPH)
            {
                dividend = wp.WeeklyTasks.Where(x => x.UnplannedCode != null).Sum(x => x.ActualHours.Values.Sum());
                divisor = wp.WeeklyTasks.Sum(x => x.ActualHours.Values.Sum());

                percentBase = 0; // 0 %
                percentGoal = 0; // 0 %
            }

            if (reportTable != null)
            {
                reportTable.dividend += dividend;
                reportTable.divisor += divisor;

                if (!dupe)
                {
                    reportTable.dividend2 += dividend;
                    reportTable.divisor2 += divisor;
                }
            }

            if (reportGraph != null)
            {
                reportGraph.percentBase = percentBase;
                reportGraph.percentGoal = percentGoal;
                reportGraph.dividend = dividend;
                reportGraph.divisor = divisor;
                reportGraph.weekAverage = divisor != 0 ? dividend / divisor : 0;
            }
        }

        /// <summary>
        /// Gather the necessary data for the Operating Report - Efficiency Barrier Time.
        /// 
        /// a) Data Table: 
        ///   i)   Total Efficiency Barrier Time (summary) 
        ///   ii)  Total Hours Worked (summary)
        ///   iii) % Efficiency Barrier Time = Total Efficiency Barrier Time / Total Hours Worked
        ///   iv)  5-week or Quarter to Date Rolling Average Efficiency Barrier Time
        ///   v)   %Base 
        ///   vi)  %Goal 
        ///   vii) Variance from %Base
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public List<OperatingReportTable> GetOperatingReport(DTO.Report spec)
        {
            List<OperatingReportTable> plans = new List<OperatingReportTable>();

            try
            {
                OperatingReportTable report = null;
                GetWeekEndingList(spec);
                LoadReportData(spec);

                long profileID = -1;
                DateTime previousDateTracker = new DateTime(1900, 1, 1);
                List<OperatingReportTable> profilePlans = new List<OperatingReportTable>();
                List<long> planIDs = new List<long>();

                for (int i = 0; i < _weeklyPlans.Count; i++)
                {
                    DTO.WeeklyPlan wp = _weeklyPlans[i];

                    if (profileID != wp.Profile.Id ||
                        (spec.IsMonthly && wp.WeekEnding.Month != previousDateTracker.Month && wp.WeekEnding.Year != previousDateTracker.Year) ||
                        (!spec.IsMonthly && wp.WeekEnding != previousDateTracker))
                    {
                        if (profileID != wp.Profile.Id && profilePlans.Count > 0)
                        {
                            AddMissingWeeks(_weekEnding, profilePlans, spec);
                            plans.AddRange(profilePlans);
                            profilePlans = new List<OperatingReportTable>();
                        }

                        report = new OperatingReportTable(spec.Type, _weekEnding[0],  spec.PercentBase, spec.PercentGoal);
                        report.dividend = 0;
                        report.divisor = 0;
                        report.WeekEnding = spec.IsMonthly ?
                            new DateTime(wp.WeekEnding.Year, wp.WeekEnding.Month, 1) : wp.WeekEnding;

                        if (_groupTitles1[i] != wp.Profile.DisplayName)
                        {
                            report.group1 = _groupTitles1[i];
                        }
                        else if (IsAllowedViewIcs(wp.Team))
                        {
                            report.group1 = wp.Profile.DisplayName;
                        }
                        else
                        {
                            report.group1 = "Individuals";
                        }

                        if (_groupTitles2[i] != wp.Profile.DisplayName)
                        {
                            report.group2 = _groupTitles2[i];
                        }
                        else if (IsAllowedViewIcs(wp.Team))
                        {
                            report.group2 = wp.Profile.DisplayName;
                        }
                        else
                        {
                            report.group2 = "Individual Contributors";
                        }

                        
                        profilePlans.Add(report);

                        profileID = wp.Profile.Id;
                        previousDateTracker = wp.WeekEnding;
                    }

                    bool dupe = planIDs.Contains(wp.Id);
                    if (!dupe) planIDs.Add(wp.Id);

                    CalculateOperatingReport(report, wp, spec.Type, dupe);
                }

                if (profilePlans.Count > 0)
                {
                    AddMissingWeeks(_weekEnding, profilePlans, spec);
                    plans.AddRange(profilePlans);
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return plans;
        }

        /// <summary>
        /// b) Graph:
        ///   i) Axes:
        ///    (1) X Axis: weeks or months
        ///    (2) Y Axis: %
        ///   ii) Plotted Area:
        ///    (1) % Efficiency Barrier Time (Line chart)
        ///    (2) 5-week Rolling Average Efficiency Barrier Time (Line chart)
        ///    (3) %Base (Line chart)
        ///    (4) %Goal (Line chart)
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public List<OperatingReportGraph> GetOperatingReportGraph(DTO.Report spec)
        {
            List<OperatingReportGraph> plans = new List<OperatingReportGraph>();

            try
            {
                List<long> wpIDs = new List<long>();
                GetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    if (!wpIDs.Contains(wp.Id))
                    {
                        OperatingReportGraph report = new OperatingReportGraph();
                        report.WeekEnding = spec.IsMonthly ?
                            new DateTime(wp.WeekEnding.Year, wp.WeekEnding.Month, 1) : wp.WeekEnding;

                        CalculateOperatingReport(report, wp, spec.Type);

                        wpIDs.Add(wp.Id);
                        plans.Add(report);
                    }
                }

                // Ensure all weeks found in Week Ending are accounted for
                // in the data placed into profilePlans.
                AddMissingWeeks(_weekEnding, plans, spec);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return plans;
        }

        /// <summary>
        /// 1) Efficiency Barrier Summary
        ///   a) Data Table
        ///     i) Assignee
        ///     ii) Efficiency Barrier Code
        ///     iii) Efficiency Barrier Hours
        ///     iv) Date
        ///     v) Program
        ///     vi) Task
        ///     vii) Barrier Comments
        /// 
        /// 2) Delay Barrier Summary
        ///   a) Data Table
        ///     i) Assignee
        ///     ii) Delay Barrier Code
        ///     iii) Delay Barrier Hours
        ///     iv) Date
        ///     v) Program
        ///     vi) Task
        ///     vii) Barrier Comments
        /// </summary>
        /// <param name="spec">The report information from ReportSelect.aspx</param>
        /// <returns></returns>
        public List<SummaryReportTable> GetSummaryReportBarrier(DTO.Report spec)
        {
            List<SummaryReportTable> report = new List<SummaryReportTable>();

            try
            {
                string type = spec.Type == DTO.Report.TypeEnum.SUMMARY_EFFICIENCY_BARRIER ?
                    "Efficiency" : "Delay";

                SummaryReportTable listItem;
                SetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    foreach (DTO.WeeklyTask weeklyTask in wp.WeeklyTasks)
                    {
                        foreach (DTO.WeeklyBarrier wb in weeklyTask.Barriers)
                        {
                            for (int i = 0; i < wb.Hours.Count(); i++)
                            {
                                // Don't add this as a barrier if no hours
                                // are logged for this day.
                                if (wb.Hours[i] > 0)
                                {
                                    listItem = new SummaryReportTable();
                                    listItem.type = type + " Barrier";
                                    listItem.assignee = IsAllowedViewIcs(wp.Team) ? wp.Profile.DisplayName : "Individual Contributor";
                                    listItem.code = wb.Barrier.Code + " >> " + wb.Barrier.Title;
                                    listItem.comment = wb.Comment;
                                    listItem.date = wp.WeekEnding.AddDays(i - 6);
                                    listItem.program = weeklyTask.Task.Program.Title;
                                    listItem.task = weeklyTask.Task.Title;
                                    listItem.hours = wb.Hours[i];
                                    report.Add(listItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return report;
        }

        /// <summary>
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
        /// <param name="spec">The report information from ReportSelect.aspx</param>
        /// <returns></returns>
        public List<SummaryReportTable> GetSummaryReportUnplanned(DTO.Report spec)
        {
            List<SummaryReportTable> report = new List<SummaryReportTable>();

            try
            {
                SummaryReportTable listItem;
                SetWeekEndingList(spec);
                LoadReportData(spec);

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    foreach (DTO.WeeklyTask weeklyTask in wp.WeeklyTasks)
                    {
                        // Add all the hours spent on this unplanned task
                        for (int i = 0; i < weeklyTask.ActualHours.Count - 1; i++)
                        {
                            if (weeklyTask.ActualHours[i] > 0.0)
                            {
                                listItem = new SummaryReportTable();
                                listItem.type = "Unplanned";
                                listItem.assignee = IsAllowedViewIcs(wp.Team) ? wp.Profile.DisplayName : "Individual Contributor";
                                listItem.code = weeklyTask.UnplannedCode.Code + " >> " + weeklyTask.UnplannedCode.Title;
                                listItem.comment = weeklyTask.Comment;
                                listItem.date = wp.WeekEnding.AddDays(i - 6);
                                listItem.program = weeklyTask.Task.Program.Title;
                                listItem.task = weeklyTask.Task.Title;
                                listItem.hours = weeklyTask.ActualHours[i];
                                report.Add(listItem);
                            }
                        }
                    }
                }                
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return report;
        }

        private void SetupParticipationGroup(
            DTO.ReportGroup group,
            List<DTO.Profile> profiles,
            List<long> profileIDs,
            List<long> teamIDs,
            List<long> checkTeam
        )
        {
            profileIDs.AddRange(group.Profiles.Where(x => !profileIDs.Contains(x.Id)).Select(x => x.Id).ToList());

            foreach (DTO.Directorate dir in group.Directorates)
            {
                List<DTO.Profile> dirProfiles = AdminManager.GetInstance().GetDirectorateMembersByOrgCodes(dir, true);
                dirProfiles = dirProfiles.Where(x => !profileIDs.Contains(x.Id)).ToList();
                group.Profiles.AddRange(dirProfiles);
                profileIDs.AddRange(dirProfiles.Select(x => x.Id));

                List<DTO.Team> dirTeams = BLL.AdminManager.GetInstance().GetTeamsByParent(dir.Id, false);
                group.Teams.AddRange(dirTeams.Where(x => !teamIDs.Contains(x.Id)).ToList());
            }

            group.Directorates.Clear();

            foreach (DTO.Team team in group.Teams)
            {
                DTO.Team fullTeam = BLL.AdminManager.GetInstance().GetTeam(team.Id, true);
                List<DTO.Profile> teamProfiles = fullTeam.Members.Where(x => !profileIDs.Contains(x.Id)).ToList();
                group.Profiles.AddRange(teamProfiles);
                profileIDs.AddRange(teamProfiles.Select(x => x.Id));
                checkTeam.AddRange(teamProfiles.Select(x => x.Id));

                if (!teamIDs.Contains(team.Id))
                {
                    teamIDs.Add(team.Id);
                }
            }

            profiles.AddRange(group.Profiles.Where(x => !profiles.Select(p => p.Id).ToList().Contains(x.Id)).ToList());

            foreach (DTO.ReportGroup child in group.Groups)
            {
                SetupParticipationGroup(child, profiles, profileIDs, teamIDs, checkTeam);
            }
        }

        /// <summary>
        /// This report is run for only one week.
        /// 
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
        public List<ParticipationReport> GetParticipationReport(DTO.Report spec)
        {
            try
            {
                List<DTO.Profile> profiles = new List<DTO.Profile>();
                List<long> profileIDs = new List<long>();
                List<long> teamIDs = new List<long>();
                List<long> checkTeam = new List<long>();
                List<long> wpIDs = new List<long>();

                SetupParticipationGroup(spec.Group, profiles, profileIDs, teamIDs, checkTeam);
                _weekEnding = new List<DateTime>();
                _weekEnding.Add(GetWeekEnding(spec.ToDate));
                
                LoadReportData(spec);

                int totalIC = 0;
                int totalNonExemptIc = 0;
                int totalIncompletePlans = 0; // Used to count the number of a IC's not completing all weekly plans.
                List<ParticipationReport> report = new List<ParticipationReport>();

                foreach (DTO.WeeklyPlan wp in _weeklyPlans)
                {
                    if (!wpIDs.Contains(wp.Id))
                    {
                        if (!checkTeam.Contains(wp.Profile.Id) || teamIDs.Contains(wp.Team.Id))
                        {
                            ParticipationReport reportItem = new ParticipationReport();
                            reportItem.displayName = wp.Profile.DisplayName;
                            reportItem.exempt = wp.Profile.ExemptPlan;
                            reportItem.weekEnding = wp.WeekEnding;
                            reportItem.completed = wp.State == DTO.WeeklyPlan.StatusEnum.LOG_APPROVED;
                            report.Add(reportItem);

                            totalIC++;

                            if (!wp.Profile.ExemptPlan)
                            {
                                totalNonExemptIc++;

                                if (!reportItem.completed)
                                {
                                    totalIncompletePlans++;
                                }
                            }
                        }

                        DTO.Profile remove = profiles.SingleOrDefault(x => x.Id == wp.Profile.Id);

                        if (remove != null)
                        {
                            profiles.Remove(remove);
                        }

                        wpIDs.Add(wp.Id);
                    }
                }

                // profiles with no plan found
                foreach (DTO.Profile p in profiles)
                {
                    ParticipationReport reportItem = new ParticipationReport();
                    reportItem.displayName = p.DisplayName;
                    reportItem.exempt = p.ExemptPlan;
                    reportItem.weekEnding = _weekEnding[0];
                    reportItem.completed = false;
                    report.Add(reportItem);

                    totalIC++;

                    if (!p.ExemptPlan)
                    {
                        totalNonExemptIc++;
                        totalIncompletePlans++;
                    }
                }

                ParticipationReport first;

                if (report.Count > 0)
                {
                    first = report.First();
                }
                else
                {
                    first = new ParticipationReport();
                    report.Add(first);
                }

                first.totalNonExemptIc = totalNonExemptIc;
                first.totalIc = totalIC; // Total number of Individual Contributors (distinct # of profiles).
                first.totalNotCompleting = totalIncompletePlans; // The number of a IC's not completing all weekly plans.

                return report;
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return null;
        }

        /// <summary>
        /// Display teams and their members, Directorate, Owner (Activity Log Manager) and Backup Activity Log Managers.
        /// </summary>
        /// <param name="spec">The report information from ReportSelect.aspx</param>
        /// <returns></returns>
        public List<waltTeamInformation> GetWaltTeamInformation(DTO.Report spec)
        {
            try
            {
                List<waltTeamInformation> report = new List<waltTeamInformation>();
                waltTeamInformation listItem;
                StringBuilder sb = new StringBuilder();

                // Exapand each team to a fully qualified DTO.Team.
                foreach (DTO.Team team in GetReportTeams(spec.Group))
                {
                    // Only display active teams.
                    if (team.Active)
                    {
                        // Set the owner information.
                        listItem = new waltTeamInformation();
                        report.Add(listItem);

                        listItem.teamName = team.Name;

                        sb = new StringBuilder();
                        sb.Append(team.Owner != null ? team.Owner.DisplayName : String.Empty);
                        listItem.data = sb.ToString();

                        sb = new StringBuilder();
                        sb.Append("AL Manager");
                        listItem.dataTitle = sb.ToString();

                        // Set the Directorate information.
                        listItem = new waltTeamInformation();
                        report.Add(listItem);

                        listItem.teamName = team.Name;

                        sb = new StringBuilder();
                        sb.Append(BLL.AdminManager.GetInstance().GetTeam(team.ParentId, false).Name);
                        listItem.data = sb.ToString();

                        sb = new StringBuilder();
                        sb.Append("Directorate");
                        listItem.dataTitle = sb.ToString();


                        // Set the complexity information
                        listItem = new waltTeamInformation();
                        report.Add(listItem);

                        listItem.teamName = team.Name;

                        sb = new StringBuilder();
                        if (team.ComplexityBased)
                        {
                            sb.Append("Complexity Based");
                        }
                        else
                        {
                            sb.Append("Hand Entered");
                        }

                        listItem.data = sb.ToString();

                        sb = new StringBuilder();
                        sb.Append("R/E Type");
                        listItem.dataTitle = sb.ToString();

                        foreach (DTO.Profile member in team.Members)
                        {
                            listItem = new waltTeamInformation();
                            report.Add(listItem);

                            listItem.teamName = team.Name;

                            listItem.data = member.DisplayName;

                            sb = new StringBuilder();
                            sb.Append("Members");
                            listItem.dataTitle = sb.ToString();
                        }

                        foreach (DTO.Profile admin in team.Admins)
                        {
                            listItem = new waltTeamInformation();
                            report.Add(listItem);

                            listItem.teamName = team.Name;

                            listItem.data = admin.DisplayName;

                            sb = new StringBuilder();
                            sb.Append("Backup AL Managers");
                            listItem.dataTitle = sb.ToString();
                        }

                        foreach (DTO.Barrier b in _dalMediator.GetAdminProcessor().GetBarrierList(team, true, true))
                        {
                            if (b.Active)
                            {
                                listItem = new waltTeamInformation();
                                report.Add(listItem);

                                listItem.teamName = team.Name;
                                listItem.data = b.Code + " >> " + b.Title;
                                listItem.dataTitle = "Selected Barriers";

                                foreach (DTO.Barrier child in b.Children)
                                {
                                    listItem = new waltTeamInformation();
                                    report.Add(listItem);

                                    listItem.teamName = team.Name;
                                    listItem.data = child.Code + " >> " + child.Title;
                                    listItem.dataTitle = "Selected Barriers";

                                    foreach (DTO.Barrier child2 in child.Children)
                                    {
                                        listItem = new waltTeamInformation();
                                        report.Add(listItem);

                                        listItem.teamName = team.Name;
                                        listItem.data = child2.Code + " >> " + child2.Title;
                                        listItem.dataTitle = "Selected Barriers";
                                    }
                                }
                            }
                        }

                        foreach (DTO.UnplannedCode uc in _dalMediator.GetAdminProcessor().GetUnplannedCodeList(team, true, true))
                        {
                            if (uc.Active)
                            {
                                listItem = new waltTeamInformation();
                                report.Add(listItem);

                                listItem.teamName = team.Name;
                                listItem.data = uc.Code + " >> " + uc.Title;
                                listItem.dataTitle = "Selected Unplanned Codes";

                                foreach (DTO.UnplannedCode child in uc.Children)
                                {
                                    listItem = new waltTeamInformation();
                                    report.Add(listItem);

                                    listItem.teamName = team.Name;
                                    listItem.data = child.Code + " >> " + child.Title;
                                    listItem.dataTitle = "Selected Unplanned Codes";

                                    foreach (DTO.UnplannedCode child2 in child.Children)
                                    {
                                        listItem = new waltTeamInformation();
                                        report.Add(listItem);

                                        listItem.teamName = team.Name;
                                        listItem.data = child2.Code + " >> " + child2.Title;
                                        listItem.dataTitle = "Selected Unplanned Codes";
                                    }
                                }
                            }
                        }

                        foreach (DTO.TaskType taskType in _dalMediator.GetAdminProcessor().GetTaskTypeList(team, true, true, true))
                        {
                            listItem = new waltTeamInformation();
                            report.Add(listItem);

                            listItem.teamName = team.Name;
                            listItem.dataTitle = "Task Types";
                            listItem.data = taskType.Title;

                            foreach (DTO.TaskType childTT in taskType.Children)
                            {
                                listItem = new waltTeamInformation();
                                report.Add(listItem);

                                listItem.teamName = team.Name;
                                listItem.dataTitle = "Task Types";
                                listItem.data = "    " + childTT.Title;
                                listItem.taskTypeComplexity = String.Empty;

                                foreach (DTO.Complexity comp in childTT.Complexities)
                                {
                                    listItem.taskTypeComplexity += comp.Title + " (" + comp.Hours.ToString() +
                                        (comp.Hours == 1 ? " hour)\n" : " hours)\n");
                                }
                            }
                        }
                    }
                }

                return report;
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return null;
        }

        /// <summary>
        /// From given date (of, if null, today's date) get last Sunday's date.
        /// 
        /// Example:
        ///  Given today is monday, and this function is given today's date,
        ///  it will return the date for yesterday.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public DateTime GetWeekEnding(DateTime? dateTime)
        {
            DateTime weekEnding;
            int diff = 0;

            if (dateTime.HasValue)
            {
                weekEnding = dateTime.Value.Date;
            }
            else
            {
                weekEnding = DateTime.Now.Date;
                diff = -7;
            }

            switch (weekEnding.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    weekEnding = weekEnding.AddDays(6 + diff);
                    break;
                case DayOfWeek.Tuesday:
                    weekEnding = weekEnding.AddDays(5 + diff);
                    break;
                case DayOfWeek.Wednesday:
                    weekEnding = weekEnding.AddDays(4 + diff);
                    break;
                case DayOfWeek.Thursday:
                    weekEnding = weekEnding.AddDays(3 + diff);
                    break;
                case DayOfWeek.Friday:
                    weekEnding = weekEnding.AddDays(2 + diff);
                    break;
                case DayOfWeek.Saturday:
                    weekEnding = weekEnding.AddDays(1 + diff);
                    break;
                case DayOfWeek.Sunday:
                    break;
                default:
                    break;
            }

            return weekEnding;
        }

        /// <summary>
        /// Generate a list of weekly plans ever assigned to the given team by
        /// searching all weekly_plans for a teamId match, and ordering the
        /// results by profile, then weekending.
        /// 
        /// The weekly plans returned will fall between the given start and
        /// end dates passed in.
        /// </summary>
        /// <param name="teamId">The foreign key id of the team to be gathered for.</param>
        /// <param name="start">Return only weekly plans with week endings after this date.</param>
        /// <param name="end">Return only weekly plans with week endings before this date.</param>
        /// <returns>A list of DTO.Profiles that ever were assigned to teaId.</returns>
        /// <returns>List of weekly plans for the given team between start and end dates.</returns>
        private List<DTO.WeeklyPlan> GetWeeklyPlanListByTeam(long teamId, DateTime start, DateTime end)
        {
            List<DTO.WeeklyPlan> weeklyPlanList = new List<DTO.WeeklyPlan>();

            try
            {
                weeklyPlanList = _dalMediator.GetReportProcessor().GetWeeklyPlanListByTeam(teamId, start, end);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return weeklyPlanList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private List<DTO.WeeklyPlan> GetWeeklyPlanListByProfile(long p, DateTime start, DateTime end)
        {
            List<DTO.WeeklyPlan> weeklyPlanList = new List<DTO.WeeklyPlan>();

            try
            {
                weeklyPlanList = _dalMediator.GetReportProcessor().GetWeeklyPlanListByProfile(p, start, end);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return weeklyPlanList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<DTO.Profile> GetExemptProfileListByTeam(long t)
        {
            List<DTO.Profile> profileList = new List<DTO.Profile>();

            try
            {
                profileList = _dalMediator.GetReportProcessor().GetExemptProfileListByTeam(t);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profileList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private List<DTO.Profile> GetExemptProfileListByProfile(long p)
        {
            List<DTO.Profile> profileList = new List<DTO.Profile>();

            try
            {
                profileList = _dalMediator.GetReportProcessor().GetExemptProfileListByProfile(p);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profileList;
        }

        /// <summary>
        /// Convert a weekEnding string, 03/18/2012, to a list of five
        /// DateTime objects going back five weeks.
        /// </summary>
        private List<DateTime> GetWeekEndingList(DTO.Report spec)
        {
            if (_weekEnding == null)
            {
                DateTime dateTime = GetWeekEnding(spec.ToDate);
                _weekEnding = new List<DateTime>();

                if (!spec.IsMonthly)
                {
                    int weeks = spec.Type.ToString().EndsWith("GRAPH") ? 10 : 5;

                    for (int i = 0; i < weeks; i++)
                    {
                        _weekEnding.Add(dateTime.AddDays(i * -7));
                    }
                }
                else // Monthly
                {
                    for (int i = 0; i < 6; i++)
                    {
                        _weekEnding.Add(dateTime.AddMonths(-i));
                    }
                }
            }

            return _weekEnding;
        }
        
        /// <summary>
        /// Create a list of week endings given a start and finish week.
        /// </summary>
        private List<DateTime> SetWeekEndingList(DTO.Report spec)
        {
            if (_weekEnding == null)
            {
                _weekEnding = new List<DateTime>();

                // The parsing could fail. Try to protect from failure.
                try
                {
                    DateTime ToDateTime = GetWeekEnding(spec.ToDate);
                    DateTime FromDateTime = GetWeekEnding(spec.FromDate);
                    _weekEnding.Add(FromDateTime);

                    while (FromDateTime != ToDateTime)
                    {
                        FromDateTime = FromDateTime.AddDays(7);
                        _weekEnding.Add(FromDateTime);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("SetWeekEndingList(): Unable to create date list. Returning null.");
                    LogError(e.ToString());
                }
            }

            return _weekEnding;
        }

        /// <summary>
        /// Get all public and private report groups.
        /// </summary>
        /// <returns></returns>
        public List<DTO.ReportGroup> GetReportGroups()
        {
            return _dalMediator.GetReportProcessor().GetReportGroups(_profile);
        }

        /// <summary>
        /// Get all public report groups.
        /// </summary>
        /// <returns></returns>
        public List<DTO.ReportGroup> GetPublicReportGroups()
        {
            return _dalMediator.GetReportProcessor().GetPublicReportGroups(_profile);
        }

        /// <summary>
        /// Get all private report groups.
        /// </summary>
        /// <returns></returns>
        public List<DTO.ReportGroup> GetPrivateReportGroups()
        {
            return _dalMediator.GetReportProcessor().GetPrivateReportGroups(_profile);
        }

        /// <summary>
        /// Get all Directorates for display at User Filter Type when
        /// Directorate is selected.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDirectorates()
        {
            List<string> directorateList = new List<string>();

            directorateList.AddRange(BLL.AdminManager.GetInstance().GetDirectorateNameList());

            // Put a blank selection at the top.
            directorateList.Insert(0, String.Empty);

            return directorateList;
        }

        /// <summary>
        /// Get all Activity Log Teams as a list DTO.Team objects.
        /// </summary>
        /// <returns>Returns a list of Teams of type TEAM as a List of DTO.Team ojects.</returns>
        public List<DTO.Team> GetALTs()
        {
            List<DTO.Team> altList = new List<DTO.Team>();

            altList.AddRange(BLL.AdminManager.GetInstance().GetTeamList());

            return altList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string FilterToXML(ReportFilter filter)
        {
            string xml = "<expression><name>" + filter.name +
                "</name><type>" + filter.type + "</type><relation>" +
                filter.relation + "</relation>";

            try
            {
                foreach (ReportCondition cond in filter.conditions)
                {
                    if (cond.filter != null)
                    {
                        xml += FilterToXML(cond.filter.Value);
                    }
                    else
                    {
                        xml += "<condition><field>" + cond.field +
                            "</field><op>" + cond.op +
                            "</op><val>" + cond.val + "</val></condition>";
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            xml += "</expression>";
            return xml;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public ReportFilter XMLtoFilter(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            return BuildFilter(xmlDoc.FirstChild);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ReportFilter BuildFilter(XmlNode node)
        {
            ReportFilter filter = new ReportFilter(true);

            try
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.LocalName == "name")
                    {
                        filter.name = child.InnerText;
                    }
                    else if (child.LocalName == "type")
                    {
                        filter.type = child.InnerText;
                    }
                    else if (child.LocalName == "relation")
                    {
                        filter.relation = child.InnerText;
                    }
                    else if (child.LocalName == "condition")
                    {
                        ReportCondition condition = new ReportCondition(null);

                        foreach (XmlNode condNode in child)
                        {
                            if (condNode.LocalName == "field")
                            {
                                condition.field = condNode.InnerText;
                            }
                            else if (condNode.LocalName == "op")
                            {
                                condition.op = condNode.InnerText;
                            }
                            else if (condNode.LocalName == "val")
                            {
                                condition.val = condNode.InnerText;
                            }
                        }

                        filter.conditions.Add(condition);
                    }
                    else if (child.LocalName == "expression")
                    {
                        ReportCondition condition = new ReportCondition(BuildFilter(child));
                        filter.conditions.Add(condition);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public string XMLtoSQL(string xml)
        {
            return FilterToSQL(XMLtoFilter(xml));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string FilterToSQL(ReportFilter filter)
        {
            int i = 1;
            string sql = "(";

            try
            {
                foreach (ReportCondition cond in filter.conditions)
                {
                    if (cond.filter != null)
                    {
                        sql += FilterToSQL(cond.filter.Value);
                    }
                    else
                    {
                        sql += cond.field + " " + cond.op + " " + cond.val;
                    }

                    if (i < filter.conditions.Count)
                    {
                        sql += " " + filter.relation + " ";
                    }

                    i++;
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            sql += ")";
            return sql;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        public void SaveReportGroup(WALT.DTO.ReportGroup g)
        {
            // If the group is public, check to see if they have permission to save it.
            if (((true == IsReportGroupPublic(g.Id)) || (true == g.Public))
                && (false == IsAllowedSavePublicGroupFilter()))
            {
                throw new Exception("You do not have permission to save Public Group Filters.");
            }

            if (g.Name.Length == 0)
            {
                throw new Exception("Group name is blank");
            }

            if (g.Profiles.Count == 0 && g.Teams.Count == 0 && g.Directorates.Count == 0 && g.Groups.Count == 0)
            {
                throw new Exception("Group must contain at least one individual, or team, or directorate");
            }

            _dalMediator.GetReportProcessor().SaveReportGroup(g);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        public void DeleteFilterGroup(DTO.ReportGroup g)
        {
            if (g == null || g.Id == 0)
            {
                throw new Exception("Report group is not defined");
            }

            // If the group is public, check to see if they have permission to delete it.
            if ((true == IsReportGroupPublic(g.Id)) && (false == IsAllowedDeletePublicReportGroupFilter()))
            {
                throw new Exception("You do not have permission to delete Public Group Filters.");
            }

            if (g.Owner.Id != _profile.Id)
            {
                throw new Exception("You do not own this report group");
            }

            _dalMediator.GetReportProcessor().DeleteReportGroup(g);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Report> GetReportList()
        {
            return _dalMediator.GetReportProcessor().GetReportList(_profile, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Report> GetReportList(bool loadGroup)
        {
            return _dalMediator.GetReportProcessor().GetReportList(_profile, loadGroup);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, string> GetReportDictionary()
        {
            Dictionary<long, string> reports = new Dictionary<long, string>();

            try
            {
                reports = _dalMediator.GetReportProcessor().GetReportDictionary(_profile);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return reports;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        public void SaveReport(DTO.Report report)
        {
            if (report.Group == null || report.Group.Id == 0)
            {
                throw new Exception("Report must have a group defined");
            }

            // If this report is marked for public view either in the database,
            // or by the user checking the public check box,
            // ensure this user has permission to save it.
            if ((IsReportPublic(report.Id) || report.Public) && !IsAllowedSavePublicReport())
            {
                throw new Exception("You do not have permission to save public reports");
            }

            // Stop user from saving public report with private filter.
            if ((IsReportPublic(report.Id) || report.Public) && !IsReportGroupPublic(report.Group.Id))
            {
                throw new Exception("Please select a Public Group Filter for this Public Report");
            }

            if (!BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.REPORT_MANAGE))
            {
                throw new Exception("You do not have permission to save reports");
            }

            if (report.Title.Length == 0)
            {
                throw new Exception("Report title is blank");
            }

            _dalMediator.GetReportProcessor().SaveReport(report);
        }

        /// <summary>
        /// Checks the database to see if the given report is marked public.
        /// This stops the user from unchecking the public box and hitting save
        /// when they don't have permissions to save public reports
        /// </summary>
        /// <returns>True if the report is public in the database.</returns>
        private bool IsReportPublic(long id)
        {
            return _dalMediator.GetReportProcessor().IsReportPublic(id);
        }

        /// <summary>
        /// Checks the database to see if the given report group is
        /// contained in another report group.
        /// </summary>
        /// <returns>True if the report group is contained in another group.</returns>
        public bool IsReportGroupInAnotherGroup(long id)
        {
            return _dalMediator.GetReportProcessor().IsReportGroupInAnotherGroup(id);
        }

        /// <summary>
        /// Checks the database to see if the given report group is marked public.
        /// This stops the user from unchecking the public box and hitting delete
        /// when they don't have permissions to delete public reports groups
        /// </summary>
        /// <returns>True if the report group is public in the database.</returns>
        private bool IsReportGroupPublic(long id)
        {
            return _dalMediator.GetReportProcessor().IsReportGroupPublic(id);
        }

        /// <summary>
        /// Checks to see if the current user is able to save Public Group Filters.
        /// </summary>
        /// <returns>True if the current user is able to save Public Group Filters.</returns>
        public bool IsAllowedSavePublicGroupFilter()
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the current user is able to save a Report marked Public.
        /// </summary>
        /// <returns>True if the current user is able to save Public Report.</returns>
        public bool IsAllowedSavePublicReport()
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the current user is able to delete a Report marked Public.
        /// </summary>
        /// <returns>True if the current user is able to delete Public Report.</returns>
        public bool IsAllowedDeletePublicReport()
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the current user is able to delete a Report Group marked Public.
        /// </summary>
        /// <returns>True if the current user is able to delete Public Report Group.</returns>
        public bool IsAllowedDeletePublicReportGroupFilter()
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        public void DeleteReport(DTO.Report report)
        {
            if (report == null || report.Id == 0)
            {
                throw new Exception("Report is not defined");
            }

            // If this report is marked for public view in the database
            // ensure this user has permission to delete it.
            if ((true == IsReportPublic(report.Id))
                && (false == IsAllowedDeletePublicReport()))
            {
                throw new Exception("You do not have permission to delete public reports");
            }

            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.REPORT_MANAGE) == false)
            {
                throw new Exception("You do not have permission to delete reports");
            }

            if (report.Owner.Id != _profile.Id)
            {
                throw new Exception("You are not the report owner");
            }

            _dalMediator.GetReportProcessor().DeleteReport(report);
        }

        /// <summary>
        /// 5.4.1.3.1	[W1750] The system shall allow RMs to specify the user
        /// group from one of the following:
        ///   1) Individual Contributor (IC)
        ///   2) AL team
        ///   3) Directorate
        ///   4) Public Group
        ///   5) Private Group 
        /// </summary>
        /// <returns></returns>
        public List<string> GetUserGroupTypes()
        {
            List<String> groupTypes = new List<String>();

            groupTypes.Add("Private Group");
            groupTypes.Add("Public Group");
            groupTypes.Add("Directorate");
            groupTypes.Add("Activity Log Team");
            groupTypes.Add("Individual Contributor");

            return groupTypes;
        }

        /// <summary>
        /// Reports are 5-weeks or 6-months. Ensure any weekEnding
        /// not found in profilePlans gets added so that the report
        /// will display to the user with all the dates they asked for.
        /// </summary>
        /// <param name="weekEnding">The list of week endings to be included in the report.</param>
        /// <param name="profilePlans">The list of OperatingReport data built from the profiles.</param>
        /// <param name="spec">The details of the report the user asked for.</param>
        private void AddMissingWeeks(List<DateTime> weekEnding, List<OperatingReportGraph> profilePlans, DTO.Report spec)
        {
            // For each week ending that is to end up in the report,
            // ensure this user had one.
            foreach (DateTime we in weekEnding)
            {
                bool found = false;

                // Check month and year for Monthly reports.
                // Check entire date for Weekly reports.
                foreach (OperatingReportGraph plan in profilePlans)
                {
                    if ((spec.IsMonthly && we.Year == plan.WeekEnding.Year && we.Month == plan.WeekEnding.Month) ||
                        (!spec.IsMonthly && we == plan.WeekEnding))
                    {
                        found = true;
                        break;
                    }
                }

                // If this date was not found in the list, add it.
                if (false == found)
                {
                    OperatingReportGraph report = new OperatingReportGraph();
                    profilePlans.Add(report);

                    report.WeekEnding = spec.IsMonthly ?
                        new DateTime(we.Year, we.Month, 1) :
                        report.WeekEnding = we;
                }
            }

            return;
        }

        /// <summary>
        /// Reports are 5-weeks or 6-months. Ensure any weekEnding
        /// not found in profilePlans gets added so that the report
        /// will display to the user with all the dates they asked for.
        /// </summary>
        /// <param name="weekEnding">The list of week endings to be included in the report.</param>
        /// <param name="profilePlans">The list of OperatingReport data built from the profiles.</param>
        /// <param name="spec">The details of the report the user asked for.</param>
        private void AddMissingWeeks(
            List<DateTime> weekEnding,
            List<OperatingReportTable> profilePlans,
            DTO.Report spec
        )
        {
            // For each week ending that is to end up in the report,
            // ensure this user had one.
            foreach (DateTime we in weekEnding)
            {
                bool found = false;

                // Check month and year for Monthly reports.
                // Check entire date for Weekly reports.
                foreach (OperatingReportTable plan in profilePlans)
                {
                    if ((spec.IsMonthly && we.Year == plan.WeekEnding.Year && we.Month == plan.WeekEnding.Month) ||
                        (!spec.IsMonthly && we == plan.WeekEnding))
                    {
                        found = true;
                        break;
                    }
                }

                // If this date was not found in the list, add it.
                if (!found)
                {
                    OperatingReportTable report = new OperatingReportTable(
                        spec.Type, spec.IsMonthly ? new DateTime(we.Year, we.Month, 1) : we, spec.PercentBase, spec.PercentGoal);                    

                    report.group1 = profilePlans[0].group1;
                    report.group2 = profilePlans[0].group2;
                    profilePlans.Add(report);
                }
            }

            return;
        }

        /// <summary>
        /// Used to create a string containing the directorate/team/individual
        /// names included in the ReportGroup.
        /// </summary>
        /// <param name="reportGroup">The group data include Directorates, 
        /// Activity Log teams, and Individual Contributors.</param>
        /// <returns>A string containing the directorate/team/individual 
        /// names included in the ReportGroup.</returns>
        public string GetFilterGroupAsString(DTO.ReportGroup reportGroup)
        {
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < reportGroup.Directorates.Count(); i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                
                sb.Append(reportGroup.Directorates[i].Name);                
            }

            for (int i = 0; i < reportGroup.Teams.Count(); i++)
            {
                if ((sb.ToString() != String.Empty) || (i > 0))
                {
                    sb.Append(", ");
                }

                sb.Append(reportGroup.Teams[i].Name);
            }

            for (int i = 0; i < reportGroup.Profiles.Count(); i++)
            {
                if ((sb.ToString() != String.Empty) || (i > 0))
                {
                    sb.Append(", ");
                }

                sb.Append(reportGroup.Profiles[i].DisplayName);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get a DTO.Report object by Id.
        /// </summary>
        /// <param name="id">The Id of the DTO.Report ojbect to return.</param>
        /// <returns>A DTO.Report object with Id equal to given id.</returns>
        public DTO.Report GetReport(long id)
        {
            return GetReport(id, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loadGroup"></param>
        /// <returns></returns>
        public DTO.Report GetReport(long id, bool loadGroup)
        {
            DTO.Report report = new DTO.Report();

            try
            {
                report = _dalMediator.GetReportProcessor().GetReport(id, loadGroup);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return report;
        }

        /// <summary>
        /// Get a DTO.ReportGroup object by Id.
        /// </summary>
        /// <param name="id">The Id of the DTO.ReportGroup ojbect to return.</param>
        /// <returns>A DTO.ReportGroup object with Id equal to given id.</returns>
        public DTO.ReportGroup GetReportGroup(long id)
        {
            DTO.ReportGroup rg = new DTO.ReportGroup();

            try
            {
                rg = _dalMediator.GetReportProcessor().GetReportGroup(id);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return rg;
        }
    }
}
