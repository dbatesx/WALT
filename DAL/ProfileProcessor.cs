using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WALT.DTO;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.Configuration;

namespace WALT.DAL
{
    public class ProfileProcessor : Processor
    {
        public ProfileProcessor(Mediator mediator)
            : base(mediator)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public DTO.Profile CreateProfileDTO(profile rec)
        {
            return CreateProfileDTO(rec, true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public DTO.Profile CreateProfileDTO(profile rec, bool expand, bool loadManager)
        {
            DTO.Profile profile = null;

            if (rec != null)
            {
                profile = new DTO.Profile();

                profile.Id = rec.id;
                profile.Username = rec.username;
                profile.ExemptPlan = rec.exempt_plan;
                profile.ExemptTask = rec.exempt_task;
                profile.CanTask = rec.can_task;
                profile.DisplayName = rec.display_name ?? profile.Username;
                profile.EmployeeID = rec.employee_id ?? string.Empty;
                profile.Active = rec.active;
                profile.Roles = new List<Role>();
                profile.Preferences = new Dictionary<string, string>();
                profile.OrgCode = rec.org_code ?? string.Empty;
                profile.Manager = loadManager && rec.manager.HasValue ? CreateProfileDTO(rec.profile1, false, false) : null;

                if (profile.DisplayName == null || profile.DisplayName.Length == 0)
                {
                    profile.DisplayName = profile.Username;
                }

                if (expand)
                {
                    var query2 = from item1 in _db.GetContext().roles
                                 join item2 in _db.GetContext().role_profiles
                                 on item1.id equals item2.role_id
                                 where item2.profile_id == profile.Id
                                 select item1;

                    foreach (var rec2 in query2)
                    {
                        profile.Roles.Add(_mediator.GetAdminProcessor().CreateRoleDTO(rec2));
                    }

                    var query3 = from item in _db.GetContext().preferences
                                 where item.profile_id == profile.Id
                                 select item;

                    foreach (var rec3 in query3)
                    {
                        profile.Preferences[rec3.name] = rec3.value;
                    }
                }
            }

            return profile;
        }

        public Profile GetProfile(long id)
        {
            return GetProfile(id, true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Profile GetProfile(long id, bool expand, bool loadManager)
        {
            Profile profile = null;
            profile p = _db.GetContext().profiles.SingleOrDefault(x => x.id == id);

            if (p != null)
            {
                profile = CreateProfileDTO(p, expand, loadManager);
            }

            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Profile GetProfile(string username, bool expand)
        {
            Profile profile = null;
            profile p = _db.GetContext().profiles.SingleOrDefault(x => x.username == username);

            if (p != null)
            {
                profile = CreateProfileDTO(p, expand, true);
            }

            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Profile> GetProfileList()
        {
            List<DTO.Profile> profiles = new List<DTO.Profile>();

            var query = from item in _db.GetContext().profiles
                        orderby item.display_name
                        select item;

            foreach (var rec in query)
            {
                profiles.Add(CreateProfileDTO(rec));
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Profile> GetProfileList(string filter)
        {
            List<DTO.Profile> profiles = new List<DTO.Profile>();

            var query = from item in _db.GetContext().profiles
                        where item.display_name.Contains(filter)
                        orderby item.display_name
                        select item;

            foreach (var rec in query)
            {
                profiles.Add(CreateProfileDTO(rec));
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, string> GetProfileDictionary(string filter)
        {
            Dictionary<long, string> profiles = new Dictionary<long, string>();

            var query = from item in _db.GetContext().profiles
                        where item.display_name.Contains(filter)
                        orderby item.display_name
                        select item;

            foreach (var rec in query)
            {
                profiles.Add(rec.id, rec.display_name);
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, string> GetTeammateDictionary(long profileId)
        {
            List<KeyValuePair<long, string>> profiles = new List<KeyValuePair<long, string>>();
             
            var query = from item1 in _db.GetContext().teams
                        join item2 in _db.GetContext().team_profiles on item1.id equals item2.team_id
                        where item2.profile_id == profileId
                        orderby item1.title
                        select item1;

            foreach (var team in query)
            {    
                if (team.owner_id != null) 
                {
                    var owner = _db.GetContext().profiles.SingleOrDefault(x => x.id == team.owner_id);
                    if (!profiles.Contains(new KeyValuePair<long, string>(owner.id, owner.display_name)))
                    {
                        profiles.Add(new KeyValuePair<long, string>(owner.id,
                            string.IsNullOrEmpty(owner.display_name) ? owner.username : owner.display_name));
                    }
                }

                var members = from item in _db.GetContext().team_profiles
                              where item.team_id == team.id
                              select item;

                foreach (var user in members)
                {
                    if (!profiles.Contains(new KeyValuePair<long, string>(user.profile.id, user.profile.display_name)))
                    {
                        profiles.Add(new KeyValuePair<long, string>(user.profile.id,
                            string.IsNullOrEmpty(user.profile.display_name) ? user.profile.username : user.profile.display_name));
                    }
                }
            }

            var teamsOwned = from item in _db.GetContext().teams
                        where item.owner_id == profileId
                        select item;

            foreach (var oteam in teamsOwned)
            {
                var members = from item in _db.GetContext().team_profiles
                              where item.team_id == oteam.id
                              select item;

                foreach (var user in members)
                {
                    if (!profiles.Contains(new KeyValuePair<long, string>(user.profile.id, user.profile.display_name)))
                    {
                        profiles.Add(new KeyValuePair<long, string>(user.profile.id,
                            string.IsNullOrEmpty(user.profile.display_name) ? user.profile.username : user.profile.display_name));
                    }
                }
            }

            profiles.Sort(delegate(KeyValuePair<long, string> x, KeyValuePair<long, string> y) { return x.Value.CompareTo(y.Value); });

            return profiles.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);


        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Profile> GetProfileList(DTO.Action action, bool active)
        {
            List<DTO.Profile> profiles = new List<DTO.Profile>();

            var query1 = (from item1 in _db.GetContext().profiles
                         join item2 in _db.GetContext().role_profiles on item1.id equals item2.profile_id
                         join item3 in _db.GetContext().roles on item2.role_id equals item3.id
                         join item4 in _db.GetContext().role_actions on item2.role_id equals item4.role_id
                         join item5 in _db.GetContext().actions on item4.action_id equals item5.id
                         where item5.title == action.ToString()
                         select item1).Distinct();

            foreach (profile p in query1)
            {
                DTO.Profile profileDTO = GetProfile(p.id);

                if (profileDTO != null && IsAllowed(profileDTO, action))
                {
                    profiles.Add(CreateProfileDTO(p, false, false));
                }
            }

            profiles = profiles.OrderBy(x => x.DisplayName).ToList();

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IsAllowed(WALT.DTO.Profile profile, WALT.DTO.Action action)
        {
            bool allowed = false;

            if (profile != null)
            {
                foreach (DTO.Role role in profile.Roles)
                {
                    if (role.Actions.Exists(x => x.ToString() == action.ToString()))
                    {
                        allowed = true;
                        break;
                    }
                }
            }

            return allowed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorateID"></param>
        /// <returns></returns>
        public bool IsDirectorateAdmin(long directorateID, DTO.Profile profile)
        {
            return _db.GetContext().team_profiles.Count(
                x => x.profile_id == profile.Id && x.team_id == directorateID && x.role == "ADMIN") > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool IsRogueUser(Profile profile)
        {
            return _db.GetContext().team_profiles.Count(x => x.profile_id == profile.Id && x.role == "MEMBER") == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void SaveProfile(Profile p)
        {
            long id = p.Id;
            profile n = _db.GetContext().profiles.SingleOrDefault(x => x.id == id);

            if (n == null)
            {
                n = new profile();
                _db.GetContext().profiles.InsertOnSubmit(n);
            }

            n.username = p.Username;
            n.exempt_plan = p.ExemptPlan;
            n.exempt_task = p.ExemptTask;
            n.can_task = p.CanTask;
            n.active = p.Active;
            n.employee_id = p.EmployeeID;
            n.display_name = p.DisplayName;
            n.org_code = p.OrgCode;
            n.profile1 = p.Manager != null ? _db.GetContext().profiles.SingleOrDefault(x => x.id == p.Manager.Id) : null; //p.Manager != null ? (long?)p.Manager.Id : null;

            if (n.display_name == null || n.display_name.Length == 0)
            {
                n.display_name = n.username;
            }

            _db.SubmitChanges();

            p.Id = n.id;
            id = p.Id;

            var query2 = from item2 in _db.GetContext().role_profiles
                    where item2.profile_id == id
                    select item2;

            foreach (var rec2 in query2)
            {
                _db.GetContext().role_profiles.DeleteOnSubmit(rec2);
            }

            _db.SubmitChanges();

            foreach (DTO.Role r in p.Roles)
            {
                role_profile rp = new role_profile();
                rp.profile_id = id;
                rp.role_id = r.Id;
                _db.GetContext().role_profiles.InsertOnSubmit(rp);
            }

            _db.SubmitChanges();

            var query3 = from item3 in _db.GetContext().preferences
                         where item3.profile_id == id
                         select item3;

            foreach (var rec3 in query3)
            {
                _db.GetContext().preferences.DeleteOnSubmit(rec3);
            }

            _db.SubmitChanges();

            for (int i = 0; i < p.Preferences.Keys.Count; i++)
            {
                preference rec = new preference();
                rec.profile_id = p.Id;
                rec.name = p.Preferences.Keys.ElementAt(i).ToString();
                rec.value = p.Preferences[rec.name];
                _db.GetContext().preferences.InsertOnSubmit(rec);
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// Creates a list of actions that the given profile
        /// is allowed to perform.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<DTO.Action> GetProfileActions(Profile profile)
        {
            List<WALT.DTO.Action> actions = new List<WALT.DTO.Action>();

            if (profile != null)
            {
                var query1 = from item1 in _db.GetContext().role_profiles
                             join item2 in _db.GetContext().role_actions
                             on item1.role_id equals item2.role_id
                             where item1.profile_id == profile.Id
                             select new { item2.action_id };

                var query2 = from item1 in _db.GetContext().actions
                             join item2 in query1
                             on item1.id equals item2.action_id
                             select new { item1.title };


                foreach (var rec in query2)
                {
                    try
                    {
                        actions.Add((DTO.Action)Enum.Parse(
                            typeof(DTO.Action), rec.title, true));
                    }
                    catch
                    {
                    }
                }
            }

            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="o"></param>
        /// <param name="type"></param>
        /// <param name="comment"></param>
        public void LogComment(Profile profile, DTO.Object o, string type, string comment)
        {
            log l = new log();
            l.profile_id = profile.Id;
            l.category = o.GetType().ToString();
            l.source_id = o.Id;
            l.comment = comment;
            l.entry_type = type;
            l.entry_date = DateTime.Now;
            _db.GetContext().logs.InsertOnSubmit(l);
            _db.SubmitChanges();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Alert CreateAlertDTO(alert a)
        {
            Alert alert = null;

            if (a != null)
            {
                alert = new Alert();
                alert.Id = a.id;
                alert.Profile = CreateProfileDTO(a.profile1);
                alert.Creator = CreateProfileDTO(a.profile);
                alert.EntryDate = a.entry_date;
                alert.Subject = a.subject;
                alert.Message = a.message;
                alert.LinkedId = a.linked_id.GetValueOrDefault(-1);
                alert.Acknowledged = a.acknowledged;
                alert.LinkedType = a.linked_type != null && a.linked_type != string.Empty ? 
                    (Alert.AlertEnum)Enum.Parse(typeof(Alert.AlertEnum), a.linked_type) : (Alert.AlertEnum?)null;
            }

            return alert;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Alert GetAlert(long id)
        {
            return CreateAlertDTO(_db.GetContext().alerts.SingleOrDefault(x => x.id == id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="acknowledged"></param>
        /// <returns></returns>
        public List<Alert> GetAlertList(Profile profile, bool acknowledged)
        {
            List<Alert> alerts = new List<Alert>();

            var query = from item in _db.GetContext().alerts
                        where item.profile_id == profile.Id && item.acknowledged == acknowledged
                        select item;

            foreach (alert a in query)
            {
                alerts.Add(CreateAlertDTO(a));
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="acknowledged"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<Alert> GetAlertList(Profile profile, bool acknowledged,
            Alert.ColumnEnum? sort, bool order, int start, int size, ref int count,
            Dictionary<Alert.ColumnEnum, string> filters)
        {
            List<Alert> alerts = new List<Alert>();

            IQueryable<alert> query = from item in _db.GetContext().alerts
                                      where item.profile_id == profile.Id && item.acknowledged == acknowledged
                                      select item;

            query = FilterAlerts(query, filters);
            count = query.Count();
            query = SortAlerts(query, sort, order).Skip(start).Take(size);

            foreach (alert a in query)
            {
                alerts.Add(CreateAlertDTO(a));
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<Alert> GetAlertListByCreator(Profile profile)
        {
            List<Alert> alerts = new List<Alert>();

            IQueryable<alert> query = from item in _db.GetContext().alerts
                                      where item.creator_id == profile.Id
                                      select item;

            foreach (alert a in query)
            {
                alerts.Add(CreateAlertDTO(a));
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<Alert> GetAlertListByCreator(Profile profile,
            Alert.ColumnEnum? sort, bool order, int start, int size, ref int count,
            Dictionary<Alert.ColumnEnum, string> filters)
        {
            List<Alert> alerts = new List<Alert>();

            IQueryable<alert> query = from item in _db.GetContext().alerts
                        where item.creator_id == profile.Id && item.linked_type != Alert.AlertEnum.SYSTEM.ToString()
                        select item;

            query = FilterAlerts(query, filters);
            count = query.Count();
            query = SortAlerts(query, sort, order).Skip(start).Take(size);

            foreach (alert a in query)
            {
                alerts.Add(CreateAlertDTO(a));
            }

            return alerts;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alerts"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        private IQueryable<alert> FilterAlerts(IQueryable<alert> alerts, Dictionary<Alert.ColumnEnum, string> filters)
        {
            if (filters != null && filters.Count > 0)
            {
                long id;
                var predicate = PredicateBuilder.True<alert>();

                foreach (Alert.ColumnEnum col in filters.Keys)
                {
                    string filter = filters[col];

                    switch (col)
                    {
                        case Alert.ColumnEnum.ACKNOWLEDGED:
                            predicate = predicate.And(x => x.acknowledged == Convert.ToBoolean(filter));
                            break;

                        case Alert.ColumnEnum.SUBJECT:
                            predicate = predicate.And(x => x.subject.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Alert.ColumnEnum.MESSAGE:
                            predicate = predicate.And(x => x.message.Contains(filter));
                            break;

                        case Alert.ColumnEnum.PROFILE:

                            if (long.TryParse(filter, out id))
                            {
                                predicate = predicate.And(x => x.profile_id == id);
                            }

                            break;

                        case Alert.ColumnEnum.CREATOR:

                            if (long.TryParse(filter, out id))
                            {
                                predicate = predicate.And(x => x.creator_id == id);
                            }

                            break;

                        case Alert.ColumnEnum.ENTRYDATE:
                            string[] split = filter.Split(',');

                            for (int i = 0; i + 1 < split.Length; i += 2)
                            {
                                DateTime date = Convert.ToDateTime(split[i+1]);

                                if (split[i] == ">")
                                {
                                    predicate = predicate.And(x => x.entry_date.CompareTo(date) > 0);
                                }
                                else if (split[i] == "<")
                                {
                                    date = date.AddDays(1);
                                    predicate = predicate.And(x => x.entry_date.CompareTo(date) < 0);
                                }
                            }

                            break;
                    }
                }

                return alerts.AsQueryable().Where(predicate);
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alerts"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private IQueryable<alert> SortAlerts(IQueryable<alert> alerts, Alert.ColumnEnum? sort, bool order)
        {
            if (sort.HasValue)
            {
                switch (sort.Value)
                {
                    case Alert.ColumnEnum.PROFILE:
                        if (order)
                        {
                            return alerts.OrderBy(x => x.profile1.display_name);
                        }
                        else
                        {
                            return alerts.OrderByDescending(x => x.profile1.display_name);
                        }

                    case Alert.ColumnEnum.CREATOR:
                        if (order)
                        {
                            return alerts.OrderBy(x => x.profile.display_name);
                        }
                        else
                        {
                            return alerts.OrderByDescending(x => x.profile.display_name);
                        }

                    case Alert.ColumnEnum.SUBJECT:
                        if (order)
                        {
                            return alerts.OrderBy(x => x.subject);
                        }
                        else
                        {
                            return alerts.OrderByDescending(x => x.subject);
                        }

                    case Alert.ColumnEnum.LINKEDTYPE:
                        if (order)
                        {
                            return alerts.OrderBy(x => x.linked_type);
                        }
                        else
                        {
                            return alerts.OrderByDescending(x => x.linked_type);
                        }

                    case Alert.ColumnEnum.ENTRYDATE:
                        if (order)
                        {
                            return alerts.OrderBy(x => x.entry_date);
                        }
                        else
                        {
                            return alerts.OrderByDescending(x => x.entry_date);
                        }

                    case Alert.ColumnEnum.ACKNOWLEDGED:
                        if (order)
                        {
                            return alerts.OrderBy(x => x.acknowledged);
                        }
                        else
                        {
                            return alerts.OrderByDescending(x => x.acknowledged);
                        }
                }
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alert"></param>
        public void SaveAlert(Alert alert)
        {
            alert a = null;

            if (alert.Id > 0)
            {
                a = _db.GetContext().alerts.SingleOrDefault(x => x.id == alert.Id);
            }

            if (a == null)
            {
                a = new alert();
                _db.GetContext().alerts.InsertOnSubmit(a);
            }

            a.profile1 = _db.GetContext().profiles.Single(x => x.id == alert.Profile.Id);
            a.profile = _db.GetContext().profiles.Single(x => x.id == alert.Creator.Id);
            a.entry_date = alert.EntryDate;
            a.subject = alert.Subject;
            a.message = alert.Message;
            a.linked_id = alert.LinkedId != -1 ? alert.LinkedId : (long?)null;
            a.linked_type = alert.LinkedType.HasValue ? alert.LinkedType.Value.ToString() : string.Empty;
            a.acknowledged = alert.Acknowledged;

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alert"></param>
        /// <param name="sendTo"></param>
        public void SendAlert(Alert alert, List<Profile> sendTo)
        {
            foreach (Profile profile in sendTo)
            {
                alert a = new alert();
                a.profile1 = _db.GetContext().profiles.Single(x => x.id == profile.Id);
                a.profile = _db.GetContext().profiles.Single(x => x.id == alert.Creator.Id);
                a.entry_date = alert.EntryDate;
                a.subject = alert.Subject;
                a.message = alert.Message;
                a.linked_id = alert.LinkedId != -1 ? alert.LinkedId : (long?)null;
                a.linked_type = alert.LinkedType.HasValue ? alert.LinkedType.Value.ToString() : string.Empty;
                a.acknowledged = alert.Acknowledged;

                _db.GetContext().alerts.InsertOnSubmit(a);
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void AcknowledgeAlert(long id)
        {
            alert alert = _db.GetContext().alerts.SingleOrDefault(x => x.id == id);

            if (alert != null)
            {
                alert.acknowledged = true;
                _db.SubmitChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alertIDs"></param>
        public void AcknowledgeAlerts(List<long> alertIDs)
        {
            var query = from item in _db.GetContext().alerts
                        where alertIDs.Contains(item.id)
                        select item;

            foreach (alert a in query)
            {
                a.acknowledged = true;
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<Profile> GetAlertCreators(Profile profile)
        {
            List<Profile> creators = new List<Profile>();
            var query = (from item1 in _db.GetContext().alerts
                         join item2 in _db.GetContext().profiles on item1.creator_id equals item2.id
                         where item1.profile_id == profile.Id
                         orderby item2.display_name
                         select item2).Distinct();

            foreach (profile p in query)
            {
                creators.Add(CreateProfileDTO(p, false, false));
            }

            return creators;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<Profile> GetAlertProfiles(Profile profile)
        {
            List<Profile> profiles = new List<Profile>();
            var query = (from item1 in _db.GetContext().alerts
                         join item2 in _db.GetContext().profiles on item1.profile_id equals item2.id
                         where item1.creator_id == profile.Id
                         orderby item2.display_name
                         select item2).Distinct();

            foreach (profile p in query)
            {
                profiles.Add(CreateProfileDTO(p, false, false));
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public int GetUnreadAlertCount(Profile profile)
        {
            return _db.GetContext().alerts.Count(x => x.profile_id == profile.Id && !x.acknowledged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        ADEntry CreateADEntryDTO(string domain, DirectoryEntry entry)
        {
            ADEntry ad = new ADEntry();

            try
            {
                ad.Domain = domain.ToUpper();
                ad.Username = entry.Properties["samaccountname"].Value.ToString().ToLower();
                ad.DisplayName = entry.Properties["displayname"].Value as string;
                ad.EmployeeID = entry.Properties["employeeid"].Value as string;
                ad.OrgCode = entry.Properties["department"].Value as string;
                string manager = entry.Properties["manager"].Value as string;

                if (!string.IsNullOrEmpty(manager))
                {
                    string[] parts2 = entry.Properties["manager"].Value.ToString().Split(',');
                    string[] parts3 = parts2[0].Split('=');
                    ad.Manager = parts3[1];
                }
            }
            catch
            {
                return null;
            }

            return ad;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public ADEntry GetADEntry(DTO.Profile profile)
        {
            ADEntry ad = null;

            if (profile.Username.Contains("\\") == false)
            {
                throw new Exception("Username must contain domain");
            }

            string[] parts = profile.Username.Split('\\');

            string dc = "LDAP://DC=" + parts[0];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADDomainComponents"]))
            {
                string[] split = ConfigurationManager.AppSettings["ADDomainComponents"].Split(',');

                foreach (string component in split)
                {
                    dc += ",DC=" + component;
                }
            }

            DirectoryEntry de = new DirectoryEntry(dc);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADUsername"]))
            {
                de.Username = ConfigurationManager.AppSettings["ADUsername"];
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADPassword"]))
            {
                de.Password = ConfigurationManager.AppSettings["ADPassword"];
            }

            DirectorySearcher ds = new DirectorySearcher(de);

            if (profile.EmployeeID == null || profile.EmployeeID.Length == 0)
            {
                ds.Filter = "samaccountname=" + parts[1];
            }
            else
            {
                ds.Filter = "employeeid=" + profile.EmployeeID;
            }

            ds.PropertiesToLoad.Add("samaccountname");
            ds.PropertiesToLoad.Add("displayname");
            ds.PropertiesToLoad.Add("employeeid");
            ds.PropertiesToLoad.Add("department");
            ds.PropertiesToLoad.Add("manager");

            SearchResultCollection results = ds.FindAll();

            if (results.Count > 0)
            {
                DirectoryEntry entry = results[0].GetDirectoryEntry();
                ad = CreateADEntryDTO(parts[0], entry);
            }

            return ad;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Profile GetProfileByDisplayName(string displayName, bool expand)
        {
            Profile p = null;

            if (!string.IsNullOrEmpty(displayName))
            {
                profile rec = _db.GetContext().profiles.FirstOrDefault(x => x.display_name.ToUpper() == displayName.ToUpper());

                if (rec != null)
                {
                    p = CreateProfileDTO(rec, expand, true);
                }
            }

            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public DTO.Profile AddProfileByADLookup(string search)
        {
            DTO.Profile profile = null;
            ADEntry entry = GetADEntry(search, true);

            if (entry != null)
            {
                string username = entry.Domain + "\\" + entry.Username;

                if (_db.GetContext().profiles.SingleOrDefault(x => x.username.ToUpper() == username.ToUpper()) != null)
                {
                    throw new Exception("Profile for " + entry.DisplayName + " already exists");
                }

                profile = new Profile();
                profile.Username = username;
                profile.DisplayName = entry.DisplayName;
                profile.EmployeeID = entry.EmployeeID;
                profile.OrgCode = entry.OrgCode;

                if (!string.IsNullOrEmpty(entry.Manager))
                {
                    profile.Manager = GetProfileByDisplayName(entry.Manager, false);
                }

                SaveProfile(profile);
            }

            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public DTO.Profile GetProfileByADLookup(string search)
        {
            ADEntry entry = GetADEntry(search, false);

            if (entry != null)
            {
                return GetProfileByUsername(entry.Domain + "\\" + entry.Username, false, false);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="multipleError"></param>
        /// <returns></returns>
        public ADEntry GetADEntry(string search, bool multipleError)
        {
            int i = 0;
            bool error = false;
            string[] searchFields = { "name", "samaccountname", "employeeid" };
            string domain = string.Empty;
            List<ADEntry> entryList = new List<ADEntry>();

            search = search.Trim();

            if (search.Contains("\\"))
            {
                string[] split = search.Split('\\');
                domain = split[0];
                search = split[1];
            }

            while (entryList.Count != 1 && i < searchFields.Count())
            {
                entryList = GetADEntryList(searchFields[i], search, domain);

                if (entryList.Count > 1)
                {
                    error = true;
                }

                i++;
            }

            if (entryList.Count == 1)
            {
                return entryList[0];
            }
            else if (multipleError && error)
            {
                throw new Exception("Multiple users found for " + search + ".");
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<ADEntry> GetADEntryList(string field, string value)
        {
            return GetADEntryList(field, value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<ADEntry> GetADEntryList(string field, string value, string domain)
        {
            List<ADEntry> entries = new List<ADEntry>();
            List<string> domains = new List<string>();
            string dc = string.Empty;

            if (!string.IsNullOrEmpty(domain))
            {
                domains.Add(domain);
            }
            else if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADDomains"]))
            {
                domains.AddRange(ConfigurationManager.AppSettings["ADDomains"].Split(','));
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADDomainComponents"]))
            {
                string[] split = ConfigurationManager.AppSettings["ADDomainComponents"].Split(',');

                foreach (string component in split)
                {
                    dc += ",DC=" + component;
                }
            }

            foreach (string d in domains)
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://DC=" + d + dc);

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADUsername"]))
                {
                    de.Username = ConfigurationManager.AppSettings["ADUsername"];
                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADPassword"]))
                {
                    de.Password = ConfigurationManager.AppSettings["ADPassword"];
                }

                DirectorySearcher ds = new DirectorySearcher(de);

                ds.Filter = field + "=" + value;
                ds.PropertiesToLoad.Add("samaccountname");
                ds.PropertiesToLoad.Add("displayname");
                ds.PropertiesToLoad.Add("employeeid");
                ds.PropertiesToLoad.Add("department");
                ds.PropertiesToLoad.Add("manager");

                SearchResultCollection results = ds.FindAll();

                foreach (SearchResult item in results)
                {
                    ADEntry entry = CreateADEntryDTO(d, item.GetDirectoryEntry());

                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Profile GetProfileByUsername(string username)
        {
            return GetProfileByUsername(username, true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Profile GetProfileByUsername(string username, bool expand, bool loadManager)
        {
            DTO.Profile rec = null;
            profile p = _db.GetContext().profiles.SingleOrDefault(x => x.username.ToUpper() == username.ToUpper());

            if (p != null)
            {
                rec = CreateProfileDTO(p, expand, loadManager);
            }

            return rec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Profile GetProfileByEmployeeID(string id)
        {
            DTO.Profile rec = null;

            profile p = _db.GetContext().profiles.SingleOrDefault(x => x.employee_id == id);

            if (p != null)
            {
                rec = CreateProfileDTO(p);
            }

            return rec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetProfileDisplayNameList()
        {
            List<string> profiles = new List<string>();

            var query = from item in _db.GetContext().profiles orderby item.display_name select item;

            foreach (profile p in query)
            {
                profiles.Add(p.display_name ?? p.username);
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileIDs"></param>
        /// <param name="exempt"></param>
        public void SetProfilesPlanExempt(List<long> profileIDs, bool exempt)
        {
            IQueryable<profile> profiles = _db.GetContext().profiles.Where(x => profileIDs.Contains(x.id));

            foreach (profile p in profiles)
            {
                p.exempt_plan = exempt;
            }

            _db.SubmitChanges();
        }
    }
}
