using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DAL
{
    public class ReportProcessor : Processor
    {
        public ReportProcessor(Mediator mediator)
            : base(mediator)
        {
        }

        /// <summary>
        /// Queries the database to generate a list of weekly plans
        /// ever assigned to the given team by searching all weekly_plans
        /// for a teamId match, and ordering the results by
        /// profile, then weekending.
        /// 
        /// The weekly plans returned will fall between the given start and
        /// end dates passed in.
        /// </summary>
        /// <param name="teamId">The foreign key id of the team to be gathered for.</param>
        /// <param name="start">Return only weekly plans with week endings after this date.</param>
        /// <param name="end">Return only weekly plans with week endings before this date.</param>
        /// <returns>A list of DTO.Profiles that ever were assigned to teaId.</returns>
        public List<DTO.WeeklyPlan> GetWeeklyPlanListByTeam(long teamId, DateTime start, DateTime end)
        {
            List<DTO.WeeklyPlan> weeklyPlanList = new List<DTO.WeeklyPlan>();

            // Go through the weekly plans to find the profiles
            // that were part of the given team that week.
            var query = from item in _db.GetContext().weekly_plans
                        where item.team_id == teamId && item.week_ending >= start && item.week_ending <= end
                        orderby item.profile_id ascending, item.week_ending descending
                        select item;

            foreach (var rec in query)
            {
                weeklyPlanList.Add(_mediator.GetTaskProcessor().CreateWeeklyPlanDTO(rec, true));
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
        public List<DTO.WeeklyPlan> GetWeeklyPlanListByProfile(long p, DateTime start, DateTime end)
        {
            List<DTO.WeeklyPlan> weeklyPlanList = new List<DTO.WeeklyPlan>();

            // Go through the weekly plans to find the profiles
            // that were part of the given team that week.
            var query = from item in _db.GetContext().weekly_plans
                        where item.profile_id == p && item.week_ending >= start && item.week_ending <= end
                        orderby item.week_ending descending
                        select item;

            foreach (var rec in query)
            {
                weeklyPlanList.Add(_mediator.GetTaskProcessor().CreateWeeklyPlanDTO(rec, true));
            }

            return weeklyPlanList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<DTO.Profile> GetExemptProfileListByTeam(long t)
        {
            List<DTO.Profile> profileList = new List<DTO.Profile>();

            // Go through the profiles to find the profiles
            // that were part of the given team and exempt.
            var query = from item1 in _db.GetContext().team_profiles
                        where item1.team_id == t
                        join item2 in _db.GetContext().profiles on item1.profile_id equals item2.id
                        where item2.exempt_plan == true
                        select item2;
                
            foreach (var rec in query)
            {
                profileList.Add(_mediator.GetProfileProcessor().CreateProfileDTO(rec));
            }

            return profileList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<DTO.Profile> GetExemptProfileListByProfile(long p)
        {
            List<DTO.Profile> profileList = new List<DTO.Profile>();

            // Go through the profiles to find the profiles
            // that were part of the given team and exempt.
            var query = from item1 in _db.GetContext().profiles
                        where item1.id == p && item1.exempt_plan == true
                        select item1;

            foreach (var rec in query)
            {
                profileList.Add(_mediator.GetProfileProcessor().CreateProfileDTO(rec));
            }

            return profileList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        public void SaveReportGroup(DTO.ReportGroup group)
        {
            long id = group.Id;

            _db.BeginTransaction();

            try
            {
                report_group g = null;

                if (id > 0)
                {
                    g = _db.GetContext().report_groups.SingleOrDefault(x => x.id == id);
                }

                if (g == null)
                {
                    g = new report_group();
                    _db.GetContext().report_groups.InsertOnSubmit(g);
                }
                else
                {
                    _db.GetContext().report_group_profiles.DeleteAllOnSubmit(
                         from item in _db.GetContext().report_group_profiles
                         where item.report_group_id == g.id
                         select item);

                    _db.GetContext().report_group_teams.DeleteAllOnSubmit(
                        from item in _db.GetContext().report_group_teams
                        where item.report_group_id == g.id
                        select item);

                    _db.GetContext().report_group_groups.DeleteAllOnSubmit(
                        from item in _db.GetContext().report_group_groups
                        where item.parent_group_id == g.id
                        select item);

                    _db.SubmitChanges();
                }

                g.title = group.Name;
                g.description = group.Description;
                g.public_flag = group.Public;
                g.profile = _db.GetContext().profiles.SingleOrDefault(x => x.id == group.Owner.Id);

                _db.SubmitChanges();

                group.Id = g.id;

                foreach (DTO.Profile p in group.Profiles)
                {
                    report_group_profile rec = new report_group_profile();
                    rec.profile_id = p.Id;
                    rec.report_group_id = g.id;
                    _db.GetContext().report_group_profiles.InsertOnSubmit(rec);
                }

                foreach (DTO.Team t in group.Teams)
                {
                    report_group_team rec = new report_group_team();
                    rec.team_id = t.Id;
                    rec.report_group_id = g.id;
                    _db.GetContext().report_group_teams.InsertOnSubmit(rec);
                }

                foreach (DTO.Directorate d in group.Directorates)
                {
                    report_group_team rec = new report_group_team();
                    rec.team_id = d.Id; // Directorate is a team in the db.
                    rec.report_group_id = g.id;
                    _db.GetContext().report_group_teams.InsertOnSubmit(rec);
                }

                foreach (DTO.ReportGroup rg in group.Groups)
                {
                    report_group_group rec = new report_group_group();
                    rec.parent_group_id = g.id;
                    rec.child_group_id = rg.Id;
                    _db.GetContext().report_group_groups.InsertOnSubmit(rec);
                }

                _db.SubmitChanges();
                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// Returns a list of report groups.
        /// If public_flag is sent as true, this function shall return
        ///  all reports that are public.
        /// If public_flag is sent as false, this function shall return
        ///  all reports that are owned by you and private.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="public_flag">True to get public reports. False to get private reports.</param>
        /// <returns></returns>
        private List<WALT.DTO.ReportGroup> GetReportGroups(DTO.Profile profile, bool public_flag)
        {
            List<DTO.ReportGroup> groups = new List<WALT.DTO.ReportGroup>();

            if (true == public_flag)
            {
                var query = from item in _db.GetContext().report_groups
                            where (item.public_flag == public_flag) && !(item.title.StartsWith("__"))
                            select item;

                foreach (var rec in query)
                {
                    groups.Add(CreateReportGroupDTO(rec));
                }
            }
            else
            {
                var query = from item in _db.GetContext().report_groups
                            where (item.profile_id == profile.Id) && (item.public_flag == public_flag)
                            select item;

                foreach (var rec in query)
                {
                    groups.Add(CreateReportGroupDTO(rec));
                }
            }

            return groups;
        }

        /// <summary>
        /// Return the public report groups
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<WALT.DTO.ReportGroup> GetPublicReportGroups(DTO.Profile profile)
        {
            return GetReportGroups(profile, true);
        }

        /// <summary>
        /// Return the private report groups
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<WALT.DTO.ReportGroup> GetPrivateReportGroups(DTO.Profile profile)
        {
            List<WALT.DTO.ReportGroup> privateReports = GetReportGroups(profile, false);

            /* Remove any report with "__" at beginning of title.
             * These reports are special case groups for User Filter Type
             * (IC, ALT, and Directorate).
             */
            privateReports.RemoveAll(item => true == item.Name.StartsWith("__"));

            return privateReports;
        }


        /// <summary>
        /// Return all public and private report groups
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<WALT.DTO.ReportGroup> GetReportGroups(DTO.Profile profile)
        {
            List<WALT.DTO.ReportGroup> allReports = new List<WALT.DTO.ReportGroup>();

            allReports.AddRange(GetPublicReportGroups(profile));
            allReports.AddRange(GetPrivateReportGroups(profile));

            return allReports;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        public void DeleteReportGroup(DTO.ReportGroup g)
        {
            if (_db.GetContext().reports.Count(x => x.report_group_id == g.Id) > 0)
            {
                throw new Exception("Report Group is still referenced in a report definition");
            }
            else if (_db.GetContext().report_group_groups.Count(x => x.child_group_id == g.Id) > 0)
            {
                throw new Exception("Report Group is still referenced in other report groups");
            }

            report_group rg = _db.GetContext().report_groups.SingleOrDefault(x => x.id == g.Id);

            if (rg != null)
            {
                _db.BeginTransaction();

                try
                {
                    _db.GetContext().report_group_profiles.DeleteAllOnSubmit(
                        _db.GetContext().report_group_profiles.Where(x => x.report_group_id == g.Id));

                    _db.GetContext().report_group_teams.DeleteAllOnSubmit(
                        _db.GetContext().report_group_teams.Where(x => x.report_group_id == g.Id));

                    _db.GetContext().report_group_groups.DeleteAllOnSubmit(
                        _db.GetContext().report_group_groups.Where(x => x.parent_group_id == g.Id));

                    _db.GetContext().report_groups.DeleteOnSubmit(rg);

                    _db.SubmitChanges();
                    _db.CommitTransaction();
                }
                catch
                {
                    _db.CancelTransaction();
                    throw;
                }
            }
        }

        DTO.Report CreateReportDTO(report rec)
        {
            return CreateReportDTO(rec, true);
        }

        /// <summary>
        /// Given a report record, create a DTO.Report object.
        /// </summary>
        /// <param name="g"></param>
        DTO.Report CreateReportDTO(report rec, bool loadGroup)
        {
            DTO.Report r = new DTO.Report();

            r.Id = rec.id;
            r.Title = rec.title;
            r.Owner = _mediator.GetProfileProcessor().GetProfile(rec.profile_id, false, false);
            r.Description = rec.description ?? string.Empty;
            r.Public = rec.public_flag;
            r.FromDate = rec.from_date;
            r.ToDate = rec.to_date;
            r.Type = (DTO.Report.TypeEnum)Enum.Parse(typeof(DTO.Report.TypeEnum), rec.type, true);
            r.PercentBase = rec.percent_base.GetValueOrDefault();
            r.PercentGoal = rec.percent_goal.GetValueOrDefault();
            r.Attributes = rec.attributes;

            if (loadGroup)
            {
                try
                {
                    r.Group = CreateReportGroupDTO(_db.GetContext().report_groups.Single(x => x.id == rec.report_group_id));
                }
                catch
                {
                    r.Group = new DTO.ReportGroup();
                }
            }
            else
            {
                r.Group = new DTO.ReportGroup();
            }
            
            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        DTO.ReportGroup CreateReportGroupDTO(report_group rec)
        {
            return CreateReportGroupDTO(rec, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="isRecursive">Set true when this funciton calls itself</param>
        /// <returns></returns>
        DTO.ReportGroup CreateReportGroupDTO(report_group rec, bool isRecursive)
        {
            DTO.ReportGroup g = new WALT.DTO.ReportGroup();
            g.Id = rec.id;
            g.Name = rec.title;
            g.Public = rec.public_flag;
            g.Owner = _mediator.GetProfileProcessor().CreateProfileDTO(rec.profile, false, false);
            g.Description = rec.description ?? string.Empty;

            var getProfiles = from rgp in _db.GetContext().report_group_profiles
                              join p in _db.GetContext().profiles on rgp.profile_id equals p.id
                              where rgp.report_group_id == rec.id
                              select p;

            foreach (profile p in getProfiles)
            {
                g.Profiles.Add(_mediator.GetProfileProcessor().CreateProfileDTO(p, false, false));
            }

            var getTeams = from rgt in _db.GetContext().report_group_teams
                           join t in _db.GetContext().teams on rgt.team_id equals t.id
                           where rgt.report_group_id == rec.id
                           select t;

            foreach (team t in getTeams)
            {
                if (t.type == DTO.Team.TypeEnum.TEAM.ToString())
                {
                    g.Teams.Add(_mediator.GetAdminProcessor().CreateTeamDTO(t, false));
                }
                else if (t.type == DTO.Team.TypeEnum.DIRECTORATE.ToString())
                {
                    g.Directorates.Add(_mediator.GetAdminProcessor().CreateDirectorateDTO(t, false));
                }
                else
                {
                    // Should not get a team that is not a TEAM or a DIRECTORATE.
                }
            }

            // Do not allow the user to get in trouble when a group has another group in it,
            // and another, and another ...
            if (isRecursive == false)
            {
                var getGroups = from rg in _db.GetContext().report_groups
                                join rgg in _db.GetContext().report_group_groups on rg.id equals rgg.child_group_id
                                where rgg.parent_group_id == rec.id
                                select rg;

                foreach (report_group rg in getGroups)
                {
                    g.Groups.Add(CreateReportGroupDTO(rg, true));
                }
            }

            return g;
        }

        /// <summary>
        /// Get a DTO.Report object by Id.
        /// </summary>
        /// <param name="id">The Id of the DTO.Report ojbect to return.</param>
        /// <returns>A DTO.Report object with Id equal to given id.</returns>
        public DTO.Report GetReport(long id, bool loadGroup)
        {
            DTO.Report report;

            report r = _db.GetContext().reports.SingleOrDefault(x => x.id == id);

            if (r != null)
            {
                try
                {
                    report = CreateReportDTO(r, loadGroup);
                }
                catch
                {
                    throw new Exception("GetReport: Failed to create report DTO.");
                }
            }
            else
            {
                report = new DTO.Report();
            }

            return report;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<DTO.Report> GetReportList(DTO.Profile p, bool loadGroup)
        {
            List<DTO.Report> reports = new List<DTO.Report>();

            var query = from item in _db.GetContext().reports
                        where item.public_flag == true || item.profile_id == p.Id
                        orderby item.public_flag descending, item.title
                        select item;

            foreach (var rec in query)
            {
                reports.Add(CreateReportDTO(rec, loadGroup));
            }

            return reports;
        }

        /// <summary>
        /// Returns a dictionary of all reports.
        /// Key is the id.
        /// Value is the title.
        /// </summary>
        /// <param name="p">The profile for which to look up private reports.</param>
        /// <returns>Returns a Dictionary of all reports where Key is the report Id and Value is the report Title.</returns>
        public Dictionary<long, string> GetReportDictionary(DTO.Profile p)
        {
            var query = (from item in _db.GetContext().reports
                        where item.public_flag == true || item.profile_id == p.Id
                        orderby item.public_flag descending, item.title
                        select item).ToDictionary(item => item.id, item => item.title);

            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        public void SaveReport(DTO.Report report)
        {
            report r = null;

            if (report.Id > 0)
            {
                r = _db.GetContext().reports.SingleOrDefault(x => x.id == report.Id);
            }            

            if (r == null)
            {
                r = new report();
                _db.GetContext().reports.InsertOnSubmit(r);
            }

            r.title = report.Title;
            r.type = report.Type.ToString();
            r.from_date = report.FromDate;
            r.to_date = report.ToDate;
            r.description = report.Description;
            r.public_flag = report.Public;
            r.profile_id = report.Owner.Id;
            r.report_group_id = report.Group.Id;
            r.percent_base = report.PercentBase;
            r.percent_goal = report.PercentGoal;
            r.attributes = report.Attributes;

            _db.SubmitChanges();

            report.Id = r.id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        public void DeleteReport(DTO.Report report)
        {
            var query = from item in _db.GetContext().reports
                        where item.id == report.Id
                        select item;

            if (query.Count() > 0)
            {
                _db.GetContext().reports.DeleteOnSubmit(query.First());
                _db.SubmitChanges();
            }
        }

        /// <summary>
        /// Checks the database to see if the given report is marked public.
        /// This stops the user from unchecking the public box and hitting save
        /// when they don't have permissions to save public reports
        /// </summary>
        /// <returns>True if the report is public in the database.</returns>
        public bool IsReportPublic(long id)
        {
            var query = from item in _db.GetContext().reports
                        where item.id == id && item.public_flag == true
                        select item;

            if (query.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the database to see if the given report group is marked public.
        /// This stops the user from unchecking the public box and hitting delete
        /// when they don't have permissions to delete public report groups
        /// </summary>
        /// <returns>True if the report group is public in the database.</returns>
        public bool IsReportGroupPublic(long id)
        {
            var query = from item in _db.GetContext().report_groups
                        where item.id == id && item.public_flag == true
                        select item;

            if (query.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the database to see if the given report group is
        /// contained in another report group.
        /// </summary>
        /// <returns>True if the report group is contained in another group.</returns>
        public bool IsReportGroupInAnotherGroup(long id)
        {
            var query = from item in _db.GetContext().report_group_groups
                        where item.child_group_id == id
                        select item;

            if (query.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Given a team, return the owner_id.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public long GetTeamOwner(long teamId)
        {
            var query = from item in _db.GetContext().teams
                        where item.id == teamId
                        select item.owner_id;

            if (query.Count() > 0)
            {
                return query.First().HasValue ? query.First().Value : -1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if the given profile_id is a
        /// Directorate Manager for the given Directorate team id.
        /// </summary>
        /// <param name="directorateTeamId">The directorate team id to be checked.</param>
        /// <param name="profile_id">The profile_id of to be checked for permission.</param>
        /// <returns>True if the profile_id matches team_profiles.id, directorateTeam matches profile_id, and team_profile.role == MANAGER.</returns>
        public bool IsDirectorateManager(long directorateTeamId, long profile_id)
        {
            var query = from item in _db.GetContext().team_profiles
                        where item.profile_id == profile_id
                              && item.team_id == directorateTeamId
                              && item.role == "MANAGER"
                        select item;

            return query.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Check if the given profile_id is an
        /// Activity Log Manager for the given team_id.
        /// </summary>
        /// <param name="team_id">The team id to be checked.</param>
        /// <param name="profile_id">The profile_id of to be checked for permission.</param>
        /// <returns>True if the profile_id matches team_profiles.profile_id, team_id matches team_profiles.id, and team_profile.role == ADMIN.</returns>
        public bool IsActivityLogManager(long team_id, long profile_id)
        {
            var query = from item in _db.GetContext().team_profiles
                        where item.profile_id == profile_id
                              && item.team_id == team_id
                              && item.role == "ADMIN"
                        select item;

            return query.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Get a DTO.ReportGroup object by Id.
        /// </summary>
        /// <param name="id">The Id of the DTO.ReportGroup ojbect to return.</param>
        /// <returns>A DTO.ReportGroup object with Id equal to given id.</returns>
        public DTO.ReportGroup GetReportGroup(long id)
        {
            DTO.ReportGroup rg = new DTO.ReportGroup();
            report_group group = _db.GetContext().report_groups.SingleOrDefault(x => x.id == id);

            if (group != null)
            {
                try
                {
                    rg = CreateReportGroupDTO(group);
                }
                catch
                {

                }
            }

            return rg;
        }

        public List<DTO.WeeklyPlan> GetReportData(
            DTO.ReportGroup group,
            DateTime weekStart,
            DateTime weekEnd,
            List<string> groupTitles1,
            List<string> groupTitles2,
            bool loadPlanned,
            bool loadUnplanned,
            bool loadUnplannedCodes,
            bool loadTaskInfo,
            bool loadTaskPrograms,
            bool loadTaskHours,
            bool loadEffBarrier,
            bool loadDelayBarrier,
            bool loadBarrierInfo,
            bool loadBarrierHours
        )
        {
            List<DTO.WeeklyPlan> plans = new List<DTO.WeeklyPlan>();
            Dictionary<DTO.WeeklyPlan, string> groupMap1 = new Dictionary<DTO.WeeklyPlan, string>();
            Dictionary<DTO.WeeklyPlan, string> groupMap2 = new Dictionary<DTO.WeeklyPlan, string>();

            GetReportData(group, weekStart, weekEnd, plans, groupMap1, groupMap2,
                loadPlanned, loadUnplanned, loadUnplannedCodes, loadTaskInfo, loadTaskPrograms, loadTaskHours,
                loadEffBarrier, loadDelayBarrier, loadBarrierInfo, loadBarrierHours, null);

            plans = plans.OrderBy(x => x.Profile.DisplayName).ThenBy(x => x.WeekEnding).ToList();

            if (groupTitles1 != null && groupTitles2 != null)
            {
                foreach (DTO.WeeklyPlan wp in plans)
                {
                    groupTitles1.Add(groupMap1[wp]);
                    groupTitles2.Add(groupMap2[wp]);
                }
            }

            return plans;
        }

        private void GetReportData(
            DTO.ReportGroup group,
            DateTime weekStart,
            DateTime weekEnd,
            List<DTO.WeeklyPlan> plans,
            Dictionary<DTO.WeeklyPlan, string> groupMap1,
            Dictionary<DTO.WeeklyPlan, string> groupMap2,
            bool loadPlanned,
            bool loadUnplanned,
            bool loadUnplannedCodes,
            bool loadTaskInfo,
            bool loadTaskPrograms,
            bool loadTaskHours,
            bool loadEffBarrier,
            bool loadDelayBarrier,
            bool loadBarrierInfo,
            bool loadBarrierHours,
            string groupBy
        )
        {
            int dirs = group.Directorates.Count;
            int teams = group.Teams.Count;
            int profiles = group.Profiles.Count;
            int groups = groupBy == null ? group.Groups.Count : 0;
            string group1 = groupBy;
            string group2 = string.Empty;
            Dictionary<long, string> dirMap = new Dictionary<long, string>();
            IQueryable<weekly_plan> wps = null;

            if (dirs > 0)
            {
                List<long> dirIDs = new List<long>();

                foreach (DTO.Directorate dir in group.Directorates)
                {
                    dirMap[dir.Id] = dir.Name;
                    dirIDs.Add(dir.Id);
                }

                wps = from wp in _db.GetContext().weekly_plans
                      join t in _db.GetContext().teams on wp.team_id equals t.id
                      where t.parent_id.HasValue && dirIDs.Contains(t.parent_id.Value) && wp.week_ending >= weekStart && wp.week_ending <= weekEnd
                      select wp;

                if (dirs + teams + profiles + groups == 1)
                {
                    if (group1 == null)
                    {
                        group1 = "team";
                        group2 = "profile";
                    }
                    else
                    {
                        group2 = "team";
                    }
                }
                else if (group1 == null)
                {
                    group1 = "dir";
                    group2 = "team";
                }
                else
                {
                    group2 = "dir";
                }

                dirs = 0;
            }
            else if (teams > 0)
            {
                List<long> teamIDs = group.Teams.Select(x => x.Id).ToList();
                wps = _db.GetContext().weekly_plans.Where(
                    x => teamIDs.Contains(x.team_id) && x.week_ending >= weekStart && x.week_ending <= weekEnd);

                if (group1 == null)
                {
                    group1 = "team";
                    group2 = "profile";
                }
                else
                {
                    group2 = "team";
                }

                teams = 0;
            }
            else if (profiles > 0)
            {
                List<long> profileIDs = group.Profiles.Select(x => x.Id).ToList();
                wps = _db.GetContext().weekly_plans.Where(
                    x => profileIDs.Contains(x.profile_id) && x.week_ending >= weekStart && x.week_ending <= weekEnd);

                if (group1 == null)
                {
                    group1 = "Individuals";
                }

                group2 = "profile";
                profiles = 0;
            }

            if (wps != null && wps.Count() > 0)
            {
                Dictionary<long, DTO.Profile> profileMap = new Dictionary<long, DTO.Profile>();
                Dictionary<long, DTO.Team> teamMap = new Dictionary<long, DTO.Team>();
                Dictionary<long, DTO.WeeklyPlan> wpMap = new Dictionary<long, DTO.WeeklyPlan>();
                List<long> wpIDs = new List<long>();

                foreach (weekly_plan wp in wps)
                {
                    DTO.WeeklyPlan weeklyPlan = new WALT.DTO.WeeklyPlan();
                        
                    if (!profileMap.ContainsKey(wp.profile_id))
                    {
                        weeklyPlan.Profile = _mediator.GetProfileProcessor().CreateProfileDTO(wp.profile2, false, false);
                        profileMap.Add(wp.profile_id, weeklyPlan.Profile);
                    }
                    else
                    {
                        weeklyPlan.Profile = profileMap[wp.profile_id];
                    }

                    if (!teamMap.ContainsKey(wp.team_id))
                    {
                        weeklyPlan.Team = _mediator.GetAdminProcessor().CreateTeamDTO(wp.team, false);
                        teamMap.Add(wp.team_id, weeklyPlan.Team);
                    }
                    else
                    {
                        weeklyPlan.Team = teamMap[wp.team_id];
                    }

                    weeklyPlan.Id = wp.id;
                    weeklyPlan.WeekEnding = wp.week_ending;
                    weeklyPlan.PlanSubmitted = wp.plan_submitted;
                    weeklyPlan.LogSubmitted = wp.log_submitted;
                    weeklyPlan.State = (wp.state == null) ? DTO.WeeklyPlan.StatusEnum.NEW :
                        (DTO.WeeklyPlan.StatusEnum)Enum.Parse(typeof(DTO.WeeklyPlan.StatusEnum), wp.state, true);

                    plans.Add(weeklyPlan);

                    wpIDs.Add(weeklyPlan.Id);
                    wpMap.Add(weeklyPlan.Id, weeklyPlan);

                    if (group1 == "dir")
                    {
                        groupMap1.Add(weeklyPlan, dirMap[weeklyPlan.Team.ParentId]);
                    }
                    else if (group1 == "team")
                    {
                        groupMap1.Add(weeklyPlan, weeklyPlan.Team.Name);
                    }
                    else if (group1 == "profile")
                    {
                        groupMap1.Add(weeklyPlan, weeklyPlan.Profile.DisplayName);
                    }
                    else
                    {
                        groupMap1.Add(weeklyPlan, group1);
                    }

                    if (group2 == "dir")
                    {
                        groupMap2.Add(weeklyPlan, dirMap[weeklyPlan.Team.ParentId]);
                    }
                    else if (group2 == "team")
                    {
                        groupMap2.Add(weeklyPlan, weeklyPlan.Team.Name);
                    }
                    else if (group2 == "profile")
                    {
                        groupMap2.Add(weeklyPlan, weeklyPlan.Profile.DisplayName);
                    }
                    else
                    {
                        groupMap2.Add(weeklyPlan, group2);
                    }
                }

                List<weekly_task> wts = new List<weekly_task>();

                for (int i = 0; i < wpIDs.Count; i += 2000)
                {
                    IQueryable<weekly_task> iwts = null;
                    List<long> subwp = wpIDs.Skip(i).Take(2000).ToList();

                    if (loadPlanned && loadUnplanned)
                    {
                        iwts = _db.GetContext().weekly_tasks.Where(x => subwp.Contains(x.weekly_plan_id));
                    }
                    else if (loadPlanned)
                    {
                        iwts = _db.GetContext().weekly_tasks.Where(x => subwp.Contains(x.weekly_plan_id) && !x.unplanned);
                    }
                    else if (loadUnplanned)
                    {
                        iwts = _db.GetContext().weekly_tasks.Where(x => subwp.Contains(x.weekly_plan_id) && x.unplanned);
                    }

                    if (iwts != null)
                    {
                        wts.AddRange(iwts.ToList());
                    }
                }

                if (wts.Count > 0)
                {
                    Dictionary<long, DTO.Task> taskMap = new Dictionary<long, DTO.Task>();
                    Dictionary<long, DTO.UnplannedCode> codeMap = new Dictionary<long, DTO.UnplannedCode>();
                    Dictionary<long, DTO.WeeklyTask> wtMap = new Dictionary<long, DTO.WeeklyTask>();
                    List<long> wtIDs = new List<long>();

                    if (loadUnplannedCodes)
                    {
                        List<long> codeIDs = wts.Where(x => x.unplanned_code_id.HasValue).Select(x => x.unplanned_code_id.Value).Distinct().ToList();
                        IQueryable<unplanned_code> codes = _db.GetContext().unplanned_codes.Where(x => codeIDs.Contains(x.id));

                        foreach (unplanned_code uc in codes)
                        {
                            DTO.UnplannedCode code = _mediator.GetAdminProcessor().CreateUnplannedCodeDTO(uc);
                            codeMap.Add(code.Id, code);
                        }
                    }

                    if (loadTaskInfo)
                    {
                        List<long> taskIDs = wts.Select(x => x.task_id).Distinct().ToList();
                        List<task> tasks = new List<task>();

                        for (int i = 0; i < taskIDs.Count; i += 2000)
                        {
                            List<long> subtasks = taskIDs.Skip(i).Take(2000).ToList();
                            IQueryable<task> itasks = _db.GetContext().tasks.Where(x => subtasks.Contains(x.id));

                            if (itasks != null)
                            {
                                tasks.AddRange(itasks.ToList());
                            }
                        }

                        Dictionary<long, DTO.Program> programMap = new Dictionary<long, DTO.Program>();

                        if (loadTaskPrograms)
                        {
                            List<long> programIDs = tasks.Where(x => x.program_id.HasValue).Select(x => x.program_id.Value).Distinct().ToList();
                            IQueryable<program> programs = _db.GetContext().programs.Where(x => programIDs.Contains(x.id));

                            foreach (program p in programs)
                            {
                                DTO.Program prog = new DTO.Program();
                                prog.Id = p.id;
                                prog.Title = p.title;
                                programMap.Add(prog.Id, prog);
                            }
                        }

                        foreach (task t in tasks)
                        {
                            DTO.Task task = new DTO.Task();
                            task.Id = t.id;
                            task.Title = t.title;
                            task.Estimate = t.estimate ?? 0;

                            if (t.program_id.HasValue && loadTaskPrograms)
                            {
                                task.Program = programMap[t.program_id.Value];
                            }

                            taskMap.Add(task.Id, task);
                        }
                    }

                    foreach (weekly_task wt in wts)
                    {
                        DTO.WeeklyTask weeklyTask = new DTO.WeeklyTask();
                        weeklyTask.Id = wt.id;
                        weeklyTask.WeeklyPlanId = wt.weekly_plan_id;
                        weeklyTask.Comment = wt.comment ?? string.Empty;
                        weeklyTask.PlanDayComplete = wt.plan_day_complete.GetValueOrDefault(-1);
                        weeklyTask.ActualDayComplete = wt.actual_day_complete.GetValueOrDefault(-1);

                        for (int i = 0; i < 7; i++)
                        {
                            weeklyTask.ActualHours[i] = 0;
                            weeklyTask.PlanHours[i] = 0;
                        }

                        if (loadTaskInfo)
                        {
                            weeklyTask.Task = taskMap[wt.task_id];
                        }

                        if (wt.unplanned_code_id.HasValue)
                        {
                            if (loadUnplannedCodes)
                            {
                                weeklyTask.UnplannedCode = codeMap[wt.unplanned_code_id.Value];
                            }
                            else
                            {
                                weeklyTask.UnplannedCode = new DTO.UnplannedCode();
                                weeklyTask.UnplannedCode.Id = wt.unplanned_code_id.Value;
                            }
                        }

                        wtIDs.Add(weeklyTask.Id);
                        wtMap.Add(weeklyTask.Id, weeklyTask);

                        DTO.WeeklyPlan weeklyPlan = wpMap[wt.weekly_plan_id];
                        weeklyPlan.WeeklyTasks.Add(weeklyTask);
                    }

                    if (loadTaskHours)
                    {
                        for (int i = 0; i < wtIDs.Count; i += 2000)
                        {
                            List<long> subwt = wtIDs.Skip(i).Take(2000).ToList();
                            IQueryable<weekly_task_hour> hours =
                                _db.GetContext().weekly_task_hours.Where(x => subwt.Contains(x.weekly_task_id));

                            foreach (weekly_task_hour wth in hours)
                            {
                                DTO.WeeklyTask weeklyTask = wtMap[wth.weekly_task_id];
                                weeklyTask.PlanHours[wth.day_of_week] = wth.plan_hours;
                                weeklyTask.ActualHours[wth.day_of_week] = wth.actual_hours;
                            }
                        }
                    }

                    List<weekly_task_barrier> wbs = new List<weekly_task_barrier>();

                    for (int i = 0; i < wtIDs.Count; i += 2000)
                    {
                        IQueryable<weekly_task_barrier> iwbs = null;
                        List<long> subwt = wtIDs.Skip(i).Take(2000).ToList();

                        if (loadDelayBarrier && loadEffBarrier)
                        {
                            iwbs = _db.GetContext().weekly_task_barriers.Where(x => subwt.Contains(x.weekly_task_id));
                        }
                        else if (loadEffBarrier)
                        {
                            iwbs = _db.GetContext().weekly_task_barriers.Where(
                                x => subwt.Contains(x.weekly_task_id) && x.barrier_type == DTO.WeeklyBarrier.BarriersEnum.EFFICIENCY.ToString());
                        }
                        else if (loadDelayBarrier)
                        {
                            iwbs = _db.GetContext().weekly_task_barriers.Where(
                                x => subwt.Contains(x.weekly_task_id) && x.barrier_type == DTO.WeeklyBarrier.BarriersEnum.DELAY.ToString());
                        }

                        if (iwbs != null)
                        {
                            wbs.AddRange(iwbs.ToList());
                        }
                    }

                    if (wbs.Count > 0)
                    {
                        List<long> wbIDs = new List<long>();
                        Dictionary<long, DTO.WeeklyBarrier> wbMap = new Dictionary<long,DTO.WeeklyBarrier>();
                        Dictionary<long, DTO.Barrier> barrierMap = new Dictionary<long,DTO.Barrier>();

                        if (loadBarrierInfo)
                        {
                            List<long> barIDs = wbs.Select(x => x.barrier_id).Distinct().ToList();

                            for (int i = 0; i < barIDs.Count; i += 2000)
                            {
                                List<long> subbar = barIDs.Skip(0).Take(2000).ToList();
                                IQueryable<barrier> bars = _db.GetContext().barriers.Where(x => subbar.Contains(x.id));

                                foreach (barrier b in bars)
                                {
                                    DTO.Barrier barrier = _mediator.GetAdminProcessor().CreateBarrierDTO(b);
                                    barrierMap.Add(barrier.Id, barrier);
                                }
                            }
                        }

                        foreach (weekly_task_barrier wtb in wbs)
                        {
                            DTO.WeeklyBarrier wb = new DTO.WeeklyBarrier();
                            wb.Id = wtb.id;
                            wb.WeeklyTaskId = wtb.weekly_task_id;
                            wb.Comment = wtb.comment;
                            wb.Ticket = wtb.ticket ?? string.Empty;
                            wb.BarrierType = wtb.barrier_type == "EFFICIENCY" ?
                                DTO.WeeklyBarrier.BarriersEnum.EFFICIENCY : DTO.WeeklyBarrier.BarriersEnum.DELAY;

                            for (int i = 0; i < 7; i++)
                            {
                                wb.Hours[i] = 0;
                            }

                            if (loadBarrierInfo)
                            {
                                wb.Barrier = barrierMap[wtb.barrier_id];
                            }

                            wbIDs.Add(wb.Id);
                            wbMap.Add(wb.Id, wb);

                            DTO.WeeklyTask weeklyTask = wtMap[wtb.weekly_task_id];
                            weeklyTask.Barriers.Add(wb);
                        }

                        if (loadBarrierHours)
                        {
                            for (int i = 0; i < wbIDs.Count; i += 2000)
                            {
                                List<long> subwb = wbIDs.Skip(i).Take(2000).ToList();
                                IQueryable<weekly_task_barrier_hour> hours =
                                    _db.GetContext().weekly_task_barrier_hours.Where(x => subwb.Contains(x.weekly_task_barrier_id));

                                foreach (weekly_task_barrier_hour wbh in hours)
                                {
                                    DTO.WeeklyBarrier wb = wbMap[wbh.weekly_task_barrier_id];
                                    wb.Hours[wbh.day_of_week] = wbh.hours;
                                }
                            }
                        }
                    }
                }
            }

            if (teams > 0)
            {
                DTO.ReportGroup subgroup = new DTO.ReportGroup();
                subgroup.Teams = group.Teams;

                GetReportData(subgroup, weekStart, weekEnd, plans, groupMap1, groupMap2,
                    loadPlanned, loadUnplanned, loadUnplannedCodes, loadTaskInfo, loadTaskPrograms, loadTaskHours,
                    loadEffBarrier, loadDelayBarrier, loadBarrierInfo, loadBarrierHours, groupBy);
            }

            if (profiles > 0)
            {
                DTO.ReportGroup subgroup = new DTO.ReportGroup();
                subgroup.Profiles = group.Profiles;

                GetReportData(subgroup, weekStart, weekEnd, plans, groupMap1, groupMap2,
                    loadPlanned, loadUnplanned, loadUnplannedCodes, loadTaskInfo, loadTaskPrograms, loadTaskHours,
                    loadEffBarrier, loadDelayBarrier, loadBarrierInfo, loadBarrierHours, groupBy);
            }

            if (groups > 0)
            {
                foreach (DTO.ReportGroup subgroup in group.Groups)
                {
                    GetReportData(subgroup, weekStart, weekEnd, plans, groupMap1, groupMap2,
                        loadPlanned, loadUnplanned, loadUnplannedCodes, loadTaskInfo, loadTaskPrograms, loadTaskHours,
                        loadEffBarrier, loadDelayBarrier, loadBarrierInfo, loadBarrierHours, subgroup.Name);
                }
            }
        }

        
    }
}
