using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WALT.DTO;
using System.Configuration;
using System.DirectoryServices;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data;

namespace WALT.DAL
{
    public class AdminProcessor : Processor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public AdminProcessor(Mediator mediator) 
            : base(mediator)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            role r = _db.GetContext().roles.SingleOrDefault(x => x.title == "SA");

            if (r == null)
            {
                r = new role();
                r.title = "SA";
                r.description = "System Administrator";
                r.active = true;
                _db.GetContext().roles.InsertOnSubmit(r);
                _db.SubmitChanges();

                role_action ra = new role_action();
                ra.role_id = r.id;
                ra.action_id = _db.GetContext().actions.Single(x => x.title == "SYSTEM_MANAGE").id;
                _db.GetContext().role_actions.InsertOnSubmit(ra);
                _db.SubmitChanges();
            }

            DTO.Role role = CreateRoleDTO(r);

            // Make sure root users are created in the database

            string[] profiles = ConfigurationManager.AppSettings["RootProfiles"].Split(',');

            foreach (string p in profiles)
            {
                DTO.Profile profile = _mediator.GetProfileProcessor().GetProfileByUsername(p);

                if (profile == null)
                {
                    profile = new Profile();
                    profile.Username = p;
                    profile.DisplayName = p;
                    _mediator.GetProfileProcessor().SaveProfile(profile);

                    profile.Roles.Add(role);
                    _mediator.GetProfileProcessor().SaveProfile(profile);
                }
            }

            // Make sure the root org is created

            string org = ConfigurationManager.AppSettings["ORG"];

            DTO.Team team = _mediator.GetAdminProcessor().GetTeam(org);

            if (team == null)
            {
                team = new Team();
                team.Name = org;
                team.Type = Team.TypeEnum.ORG;
                _mediator.GetAdminProcessor().SaveTeam(null, team, false, false, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSystemMessage()
        {
            string message = null;    

            string[] profiles = ConfigurationManager.AppSettings["RootProfiles"].Split(',');

            if (profiles.Length > 0)
            {
                DTO.Profile profile = _mediator.GetProfileProcessor().GetProfileByUsername(profiles[0]);

                if (profile != null)
                {
                    preference p = _db.GetContext().preferences.SingleOrDefault(x => x.profile_id == profile.Id && x.name == "SystemMessage");

                    if (p != null)
                    {
                        message = p.value;
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DTO.Program CreateProgramDTO(program p)
        {
            Program program = new Program();

            if (p != null)
            {
                program.Id = p.id;
                program.Title = p.title;
                program.Active = p.active;
            }

            return program;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Team GetOrg()
        {
            return GetTeam(ConfigurationManager.AppSettings["ORG"]);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Program> GetProgramList()
        {
            List<Program> programs = new List<Program>();

            var query = from item in _db.GetContext().programs
                        orderby item.title
                        select item;

            foreach (var rec in query)
            {
                programs.Add(CreateProgramDTO(rec));
            }

            return programs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, string> GetProgramDictionary()
        {
            var query = (from item in _db.GetContext().programs
                         where item.active == true
                         orderby item.title
                         select item).ToDictionary(item => item.id, item => item.title); 

            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public DTO.Role CreateRoleDTO(role rec)
        {
            DTO.Role r = new Role();

            r.Id = rec.id;
            r.Title = rec.title;
            r.Description = rec.description ?? string.Empty;
            r.Active = rec.active;
            r.Actions = new List<WALT.DTO.Action>();

            var query = from item in _db.GetContext().role_actions
                        where item.role_id == r.Id
                        select item;

            foreach (var item in query)
            {
                try
                {
                    r.Actions.Add((DTO.Action)Enum.Parse(typeof(DTO.Action), item.action.title, true));
                }
                catch
                {
                }
            }

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Role> GetRoleList()
        {
            List<DTO.Role> roles = new List<Role>();

            var query = from item in _db.GetContext().roles
                        orderby item.title
                        select item;

            foreach (var rec in query)
            {
                roles.Add(CreateRoleDTO(rec));
            }

            return roles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetSystemActionList()
        {
            List<string> actions = new List<string>();

            foreach (string s in Enum.GetNames(typeof(DTO.Action)))
            {
                actions.Add(s);
            }

            actions.Sort();

            return actions;
        }

        /// <summary>
        /// From the teams table, return all teams
        /// of type "TEAM" as a list of strings.
        /// </summary>
        /// <returns>A list of strings containing all 'TEAM' teams from database table teams.</returns>
        public List<string> GetTeamNameList()
        {
            List<string> teams = new List<string>();

            var query = from item in _db.GetContext().teams
                        where item.type == "TEAM"
                        orderby item.title
                        select item;

            foreach (var rec in query)
            {
                teams.Add(rec.title);
            }

            return teams;
        }

        /// <summary>
        /// From the teams table, return all teams
        /// of type "TEAM" as a list of strings
        /// prepended with the directorate name.
        /// </summary>
        /// <returns>A list of strings containing all 'TEAM' teams from database table teams prepended with "[Directorate]: ."</returns>
        public List<string> GetTeamNameListWithDirectorate()
        {
            List<string> teams = new List<string>();

            var query = from item in _db.GetContext().teams
                        where item.type == DTO.Team.TypeEnum.TEAM.ToString()
                        orderby item.title
                        select item;

            StringBuilder sb;

            foreach (var team in query)
            {
                sb = new StringBuilder();

                if (true == team.parent_id.HasValue)
                {
                    sb.Append(GetDirectorateTitle(team.parent_id.Value));
                    sb.Append(": ");
                }

                sb.Append(team.title);

                teams.Add(sb.ToString());
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public Barrier CreateBarrierDTO(barrier b)
        {
            Barrier barrier = null;

            if (b != null)
            {
                barrier = new Barrier();
                barrier.Id = b.id;
                barrier.ParentId = b.parent_id.GetValueOrDefault(0);
                barrier.Code = b.code;
                barrier.Title = b.title;
                barrier.Description = b.description ?? string.Empty;
                barrier.Active = b.active;
            }

            return barrier;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team_id"></param>
        /// <param name="id_list"></param>
        long? GetTeamParentIds(long team_id, List<long> id_list)
        {
            team t = _db.GetContext().teams.SingleOrDefault(x => x.id == team_id);

            if (t != null && t.parent_id != null && t.parent_id != t.id)
            {
                id_list.Insert(0, t.parent_id.Value);
                GetTeamParentIds(t.parent_id.Value, id_list);

                if (t.type == Team.TypeEnum.TEAM.ToString())
                {
                    return t.parent_id;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Barrier GetBarrier(long id)
        {
            Barrier barrier = null;
            barrier b = _db.GetContext().barriers.SingleOrDefault(x => x.id == id);

            if (b != null)
            {
                barrier = CreateBarrierDTO(b);
            }

            return barrier;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetBarrierLevel(long id)
        {
            int level = -1;

            barrier b = _db.GetContext().barriers.SingleOrDefault(x => x.id == id);

            if (b != null)
            {
                if (b.parent_id == null || b.parent_id == b.id || b.parent_id == 0)
                {
                    return level = 0;
                }
                else 
                {                   
                    barrier parent = _db.GetContext().barriers.SingleOrDefault(x => x.id == b.parent_id);

                    if (parent != null)
                    {
                        if (parent.parent_id == null || parent.parent_id == parent.id || parent.parent_id == 0)
                        {
                            return level = 1;
                        }
                        else
                        {
                            barrier parent2 = _db.GetContext().barriers.SingleOrDefault(x => x.id == parent.parent_id);

                            if (parent2 != null)
                            {
                                return level = 2;
                            }
                        }
                    }
                }
            }

            return level;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="barrierId"></param>
        /// <returns></returns>
        public List<Barrier> GetBarrierChildren(long barrierId, long teamId)
        {
            List<Barrier> children = new List<Barrier>();
            barrier b = _db.GetContext().barriers.SingleOrDefault(x => x.id == barrierId);

            if (b != null)
            {
                List<barrier> barriers = (from item in _db.GetContext().barriers
                                          orderby item.code, item.title
                                          where (item.id != barrierId) && (item.parent_id == b.id) && (item.team_id == teamId)
                                          select item).ToList();

                foreach (var rec in barriers)
                {
                    children.Add(CreateBarrierDTO(rec));
                }
            }

            return children;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team_name"></param>
        /// <returns></returns>
        public List<Barrier> GetBarrierList(Team team, bool filtered, bool? active)
        {
            List<Barrier> barriers = new List<Barrier>();
            IQueryable<barrier> query;

            if (!filtered)
            {
                List<long> parents = new List<long>();
                GetTeamParentIds(team.Id, parents);
                parents.Add(team.Id);
            
                if (active.HasValue)
                {
                    query = from item in _db.GetContext().barriers                            
                            where parents.Contains(item.team_id) && item.active == active.Value
                            orderby item.code, item.title
                            select item;
                }
                else
                {
                    query = from item in _db.GetContext().barriers
                            where parents.Contains(item.team_id)
                            orderby item.code, item.title
                            select item;
                }
            }
            else if (active.HasValue)
            {
                query = from b in _db.GetContext().barriers
                        join tb in _db.GetContext().team_barriers on b.id equals tb.barrier_id
                        where tb.team_id == team.Id && b.active == active.Value
                        orderby b.code, b.title
                        select b;
            }
            else
            {
                query = from b in _db.GetContext().barriers
                        join tb in _db.GetContext().team_barriers on b.id equals tb.barrier_id
                        where tb.team_id == team.Id
                        orderby b.code, b.title
                        select b;
            }

            List<barrier> childList = new List<barrier>();
            List<long> parentList = new List<long>();
            Dictionary<long, barrier> barrierMap = new Dictionary<long, barrier>();
            Dictionary<long, Barrier> parentMap = new Dictionary<long, Barrier>();

            foreach (barrier rec in query)
            {
                barrierMap.Add(rec.id, rec);

                if (!rec.parent_id.HasValue || rec.parent_id.Value == rec.id)
                {
                    Barrier bar = CreateBarrierDTO(rec);
                    barriers.Add(bar);
                    parentMap.Add(bar.Id, bar);
                }
                else
                {
                    childList.Add(rec);

                    if (!parentList.Contains(rec.parent_id.Value))
                    {
                        parentList.Add(rec.parent_id.Value);
                    }
                }
            }

            foreach (long parentID in parentList)
            {
                if (barrierMap.ContainsKey(parentID) && !parentMap.ContainsKey(parentID))
                {
                    Barrier bar = CreateBarrierDTO(barrierMap[parentID]);
                    parentMap.Add(bar.Id, bar);
                }
            }

            foreach (barrier child in childList)
            {
                if (parentMap.ContainsKey(child.parent_id.Value))
                {
                    Barrier parent = parentMap[child.parent_id.Value];
                    Barrier bar;

                    if (parentMap.ContainsKey(child.id))
                    {
                        bar = parentMap[child.id];
                    }
                    else
                    {
                        bar = CreateBarrierDTO(child);
                    }

                    parent.Children.Add(bar);
                }
            }

            return barriers;
        }

        /// <summary>
        /// From the teams table, return all teams
        /// of type "TEAM" as a list of DTO.Team objects.
        /// </summary>
        /// <returns>Returns a list of DTO.Team ojects of teams of type TEAM from the teams table.</returns>
        public List<Team> GetTeamList()
        {
            List<Team> teams = new List<Team>();

            var query = from item in _db.GetContext().teams
                        where item.type == DTO.Team.TypeEnum.TEAM.ToString()
                        orderby item.title
                        select item;

            foreach (var rec in query)
            {
                teams.Add(CreateTeamDTO(rec, false));
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public DTO.Team CreateTeamDTO(team t)
        {
            return CreateTeamDTO(t, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public DTO.Team CreateTeamDTO(team t, bool loadMembers)
        {
            DTO.Team team = new DTO.Team();
            team.Id = t.id;
            team.ParentId = t.parent_id.GetValueOrDefault(0);
            team.Name = t.title;
            team.Members = new List<Profile>();
            team.Admins = new List<Profile>();
            team.Owner = _mediator.GetProfileProcessor().CreateProfileDTO(t.profile, false, false);
            team.Active = t.active;
            team.ComplexityBased = t.complexity_based;
            team.SelectedBarriers = new List<Barrier>();
            team.SelectedUnplannedCodes = new List<UnplannedCode>();
            team.SelectedTaskTypes = new List<TaskType>();

            if (t.type == "TEAM")
            {
                team.Type = DTO.Team.TypeEnum.TEAM;
            }
            else if (t.type == "DIRECTORATE")
            {
                team.Type = DTO.Team.TypeEnum.DIRECTORATE;
            }
            else if (t.type == "ORG")
            {
                team.Type = DTO.Team.TypeEnum.ORG;
            }
            else // Handle unknown type.
            {
                team.Type = DTO.Team.TypeEnum.TEAM;
            }
            
            if (loadMembers)
            {
                foreach (var rec2 in t.team_profiles)
                {
                    if (rec2.role == "ADMIN")
                    {
                        team.Admins.Add(_mediator.GetProfileProcessor().CreateProfileDTO(rec2.profile, false, false));
                    }
                    else
                    {
                        team.Members.Add(_mediator.GetProfileProcessor().CreateProfileDTO(rec2.profile, false, false));
                    }
                }

                team.Members = team.Members.OrderBy(x => x.DisplayName).ToList();
                team.Admins = team.Admins.OrderBy(x => x.DisplayName).ToList();
            }

            return team;
        }

        public List<Profile> GetTeamMembers(long teamId)
        {
            List<Profile> members = new List<Profile>();

            var query = from p in _db.GetContext().profiles
                        join tp in _db.GetContext().team_profiles on p.id equals tp.profile_id
                        where tp.team_id == teamId && tp.role == "MEMBER"
                        select p;

            foreach (profile p in query)
            {
                members.Add(_mediator.GetProfileProcessor().CreateProfileDTO(p, false, false));
            }

            members = members.OrderBy(x => x.DisplayName).ToList();
            return members;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="includeCurrent"></param>
        /// <returns></returns>
        public List<DTO.Profile> GetTeamMembers(long teamId, DateTime start, DateTime end, bool includeCurrent)
        {
            List<DTO.Profile> members = new List<DTO.Profile>();

            var query = (from wp in _db.GetContext().weekly_plans
                         join p in _db.GetContext().profiles on wp.profile_id equals p.id
                         where wp.team_id == teamId && wp.week_ending >= start && wp.week_ending <= end
                         select p).Distinct();

            foreach (profile p in query)
            {
                members.Add(_mediator.GetProfileProcessor().CreateProfileDTO(p, false, false));
            }

            if (includeCurrent)
            {
                List<long> profileIDs = members.Select(x => x.Id).ToList();

                var query2 = from p in _db.GetContext().profiles
                             join tp in _db.GetContext().team_profiles on p.id equals tp.profile_id
                             where tp.team_id == teamId && tp.role == "MEMBER" && !profileIDs.Contains(p.id) &&
                                _db.GetContext().weekly_plans.SingleOrDefault(x => x.profile_id == p.id && x.week_ending >= start && x.week_ending <= end) == null
                             select p;

                foreach (profile p in query2)
                {
                    members.Add(_mediator.GetProfileProcessor().CreateProfileDTO(p, false, false));
                }
            }

            members = members.OrderBy(x => x.DisplayName).ToList();
            return members;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Complexity CreateComplexityDTO(complexity c)
        {
             Complexity complex = null;

             if (c != null)
             {
                 complex = new Complexity();
                 complex.Id = c.id;
                 complex.Title = c.title;
                 complex.Hours = (double)c.hours;
                 complex.Active = c.active;
                 complex.SortOrder = c.sort_order;
             }

            return complex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="directorateID"></param>
        /// <param name="parents"></param>
        /// <param name="recursive"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public TaskType CreateTaskTypeDTO(task_type t, long? directorateID)
        {
            TaskType type = new TaskType();
            type.Id = t.id;
            type.ParentId = t.parent_id.GetValueOrDefault(0);
            type.Title = t.title;
            type.Description = t.description ?? string.Empty;
            type.Active = t.active;
            type.TeamId = t.team_id ?? 0;

            if (directorateID.HasValue)
            {
                var query = from item in _db.GetContext().complexities
                            where item.task_type_id == t.id && item.team_id == directorateID.Value
                            orderby item.sort_order
                            select item;

                foreach (var rec in query)
                {
                    type.Complexities.Add(CreateComplexityDTO(rec));
                }
            }

            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public List<DTO.TaskType> GetTaskTypeList(Team team, bool loadComplexity, bool filtered, bool? active)
        {
            List<TaskType> types = new List<TaskType>();
            IQueryable<task_type> query;
            long? directorateID = null;

            if (!filtered)
            {
                List<long> parents = new List<long>();
                directorateID = GetTeamParentIds(team.Id, parents);
                parents.Add(team.Id);

                if (!loadComplexity || !team.ComplexityBased)
                {
                    directorateID = null;
                }

                if (active.HasValue)
                {
                    query = from item in _db.GetContext().task_types
                            where item.team_id.HasValue && parents.Contains(item.team_id.Value) && item.active == active.Value
                            orderby item.team_id, item.title
                            select item;
                }
                else
                {
                    query = from item in _db.GetContext().task_types
                            where item.team_id.HasValue && parents.Contains(item.team_id.Value)
                            orderby item.team_id, item.title
                            select item;
                }
            }
            else
            {
                if (loadComplexity && team.ComplexityBased)
                {
                    directorateID = team.ParentId;
                }

                if (active.HasValue)
                {
                    query = from tt in _db.GetContext().task_types
                            join teamt in _db.GetContext().team_task_types on tt.id equals teamt.task_type_id
                            where teamt.team_id == team.Id && tt.active == active.Value
                            orderby tt.team_id, tt.title
                            select tt;
                }
                else
                {
                    query = from tt in _db.GetContext().task_types
                            join teamt in _db.GetContext().team_task_types on tt.id equals teamt.task_type_id
                            where teamt.team_id == team.Id
                            orderby tt.team_id, tt.title
                            select tt;
                }
            }

            List<task_type> childList = new List<task_type>();
            List<long> parentList = new List<long>();
            Dictionary<long, task_type> typeMap = new Dictionary<long, task_type>();
            Dictionary<long, TaskType> parentMap = new Dictionary<long, TaskType>();

            foreach (task_type rec in query)
            {      
                typeMap.Add(rec.id, rec);

                if (!rec.parent_id.HasValue || rec.parent_id.Value == rec.id)
                {
                    TaskType tt = CreateTaskTypeDTO(rec, directorateID);
                    types.Add(tt);
                    parentMap.Add(tt.Id, tt);
                }
                else
                {
                    childList.Add(rec);

                    if (!parentList.Contains(rec.parent_id.Value))
                    {
                        parentList.Add(rec.parent_id.Value);
                    }
                }
            }

            foreach (long parentID in parentList)
            {
                if (typeMap.ContainsKey(parentID) && !parentMap.ContainsKey(parentID))
                {
                    TaskType tt = CreateTaskTypeDTO(typeMap[parentID], directorateID);
                    parentMap.Add(tt.Id, tt);
                }
            }

            foreach (task_type child in childList)
            {
                if (parentMap.ContainsKey(child.parent_id.Value))
                {
                    TaskType parent = parentMap[child.parent_id.Value];
                    TaskType tt;

                    if (parentMap.ContainsKey(child.id))
                    {
                        tt = parentMap[child.id];
                    }
                    else
                    {
                        tt = CreateTaskTypeDTO(child, directorateID);
                    }

                    parent.Children.Add(tt);
                }                
            }

            return types;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(long id, bool loadMembers)
        {
            DTO.Team team = null;
            team teamRec = _db.GetContext().teams.SingleOrDefault(x => x.id == id);
            
            if (teamRec != null)
            {
                team = CreateTeamDTO(teamRec, loadMembers);
            }
            
            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(string name)
        {
            DTO.Team team = null;

            var query = from item in _db.GetContext().teams
                        where item.title == name
                            select item;

            if (query.Count() > 0)
            {
                var rec = query.First();
                team = CreateTeamDTO(rec);
            }

            return team;
        }

        /// <summary>
        /// Get the Team based on the Team Title, aka Name.
        /// Set expand to true to get the full DTO.Team.
        /// Set expand to false to get a partial DTO.Team.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expand">True to get the full DTO.Team.</param>
        /// <returns></returns>
        public DTO.Team GetTeam(string name, bool loadMembers)
        {
            DTO.Team team = null;
            team t = _db.GetContext().teams.FirstOrDefault(x => x.title == name);

            if (t != null)
            {
                team = CreateTeamDTO(t, loadMembers);
            }

            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeamsOwned(DTO.Profile profile)
        {
            List<DTO.Team> teams = new List<Team>();

            var query = from item in _db.GetContext().teams
                        where item.owner_id == profile.Id
                        select item;

            foreach (var team in query)
            {
                teams.Add(CreateTeamDTO(team, false));
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetTeamsOwnedDictionary(DTO.Profile profile)
        {
            Dictionary<long, string> teams = new Dictionary<long, string>();

            var query = from item in _db.GetContext().teams
                        where item.owner_id == profile.Id
                        select item;

            foreach (var team in query)
            {
                teams.Add(team.id, team.title);
            }

            var query2 = from item in _db.GetContext().team_profiles
                         where item.profile_id == profile.Id &&
                         item.role == "ADMIN" &&
                         !query.Select(x => x.id).Contains(item.team_id)
                         select item;

            foreach (var team in query2)
            {
                teams.Add(team.team_id, team.team.title);
            }


            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<long> GetDirectorateManagedIds(DTO.Profile profile)
        {
            List<long> directoratesIDs = new List<long>();

            var query = from item1 in _db.GetContext().teams
                        join item2 in _db.GetContext().team_profiles on item1.id equals item2.team_id
                        where item2.profile_id == profile.Id && item2.role == "MANAGER" && item1.type == "DIRECTORATE"
                        orderby item1.title
                        select item1;


            foreach (var dir in query)
            {
                directoratesIDs.Add(dir.id);
            }

            return directoratesIDs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetDirectorateForALMsDMs(DTO.Profile profile)
        {
            Dictionary<long, string> directorates = new Dictionary<long, string>();

            var query = from item1 in _db.GetContext().teams
                        join item2 in _db.GetContext().team_profiles on item1.id equals item2.team_id
                        where item2.profile_id == profile.Id && item2.role == "MANAGER" && item1.type == "DIRECTORATE"
                        orderby item1.title
                        select item1;


            foreach (var dir in query)
            {
                directorates.Add(dir.id, dir.title);
            }

            var query1 = from item in _db.GetContext().teams
                        where item.owner_id == profile.Id
                        select item;

            foreach (var team in query)
            {
                var query2 = (from item in _db.GetContext().teams
                             where item.id == team.parent_id
                             select item).SingleOrDefault();

                if (query2 != null && directorates.ContainsKey(query2.id))
                {
                    directorates.Add(query2.id, query2.title);
                }
            }

            var query3 = from item in _db.GetContext().team_profiles
                         where item.profile_id == profile.Id &&
                         item.role == "ADMIN" &&
                         !query.Select(x => x.id).Contains(item.team_id)
                         select item;

            foreach (var team in query3)
            {
                var query4 = (from item in _db.GetContext().teams
                              where item.id == team.profile_id
                              select item).SingleOrDefault();

                if (query4 != null && directorates.ContainsKey(query4.id))
                {
                    directorates.Add(query4.id, query4.title);
                }
            }

            return directorates;
        }

        public List<DTO.Team> GetTeamsByParent(long parentID, bool expand)
        {
            List<DTO.Team> teams = new List<Team>();

            var query = from item in _db.GetContext().teams
                        where item.parent_id == parentID
                        orderby item.title
                        select item;

            foreach (team t in query)
            {
                teams.Add(CreateTeamDTO(t, expand));
            }

            return teams;
        }

        public Dictionary<long, string> GetTeamsDictionaryByParent(long parentID)
        {
            Dictionary<long, string> teams = new Dictionary<long, string>();

            var query = from item in _db.GetContext().teams
                        where item.parent_id == parentID
                        orderby item.title
                        select item;

            foreach (team t in query)
            {
                teams.Add(t.id,t.title);
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UnplannedCode GetUnplannedCode(long id)
        {
            UnplannedCode unplanned = null;
            unplanned_code u = _db.GetContext().unplanned_codes.SingleOrDefault(x => x.id == id);

            if (u != null)
            {
                unplanned = CreateUnplannedCodeDTO(u);
            }

            return unplanned;
        }

        /// <summary>
        /// 
        /// </summary>       
        /// <returns></returns>
        public int GetUnplannedCodeLevel(long id)
        {
            int level = -1;

            unplanned_code b = _db.GetContext().unplanned_codes.SingleOrDefault(x => x.id == id);

            if (b != null)
            {
                if (b.parent_id == null || b.parent_id == b.id || b.parent_id == 0)
                {
                    return level = 0;
                }
                else
                {
                    unplanned_code parent = _db.GetContext().unplanned_codes.SingleOrDefault(x => x.id == b.parent_id);

                    if (parent != null)
                    {
                        if (parent.parent_id == null || parent.parent_id == parent.id || parent.parent_id == 0)
                        {
                            return level = 1;
                        }
                        else
                        {
                            unplanned_code parent2 = _db.GetContext().unplanned_codes.SingleOrDefault(x => x.id == parent.parent_id);

                            if (parent2 != null)
                            {
                                return level = 2;
                            }
                        }
                    }
                }
            }

            return level;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="barrierId"></param>
        /// <returns></returns>
        public List<UnplannedCode> GetUnplannedCodeChildren(long unplannedId, long teamId)
        {
            List<UnplannedCode> children = new List<UnplannedCode>();
            unplanned_code c = _db.GetContext().unplanned_codes.SingleOrDefault(x => x.id == unplannedId);

            if (c != null)
            {
                List<unplanned_code> unplannedCodes = (from item in _db.GetContext().unplanned_codes
                                                       orderby item.code, item.title
                                                       where (item.id != unplannedId) && (item.parent_id == c.id) && (item.team_id == teamId)
                                                       select item).ToList();

                foreach (var rec in unplannedCodes)
                {
                    children.Add(CreateUnplannedCodeDTO(rec));
                }
            }

            return children;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public UnplannedCode CreateUnplannedCodeDTO(unplanned_code c)
        {
            UnplannedCode code = null;

            if (c != null)
            {
                code = new UnplannedCode();
                code.Id = c.id;
                code.ParentId = c.parent_id.GetValueOrDefault(0);
                code.Code = c.code;
                code.Title = c.title;
                code.Description = c.description ?? string.Empty;
                code.Active = c.active;
            }

            return code;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public List<UnplannedCode> GetUnplannedCodeList(Team team, bool filtered, bool? active)
        {
            List<UnplannedCode> codes = new List<UnplannedCode>();
            IQueryable<unplanned_code> query;

            if (!filtered)
            {
                List<long> parents = new List<long>();
                GetTeamParentIds(team.Id, parents);
                parents.Add(team.Id);

                if (active.HasValue)
                {
                    query = from item in _db.GetContext().unplanned_codes                            
                            where parents.Contains(item.team_id) && item.active == active.Value
                            orderby item.code, item.title
                            select item;
                }
                else
                {
                    query = from item in _db.GetContext().unplanned_codes                            
                            where parents.Contains(item.team_id)
                            orderby item.code, item.title
                            select item;
                }
            }
            else if (active.HasValue)
            {
                query = from uc in _db.GetContext().unplanned_codes
                        join tc in _db.GetContext().team_unplanned_codes on uc.id equals tc.unplanned_code_id
                        where tc.team_id == team.Id && uc.active == active.Value
                        orderby uc.code, uc.title
                        select uc;
            }
            else
            {
                query = from uc in _db.GetContext().unplanned_codes
                        join tc in _db.GetContext().team_unplanned_codes on uc.id equals tc.unplanned_code_id
                        where tc.team_id == team.Id
                        orderby uc.code, uc.title
                        select uc;
            }

            List<unplanned_code> childList = new List<unplanned_code>();
            List<long> parentList = new List<long>();
            Dictionary<long, unplanned_code> codeMap = new Dictionary<long, unplanned_code>();
            Dictionary<long, UnplannedCode> parentMap = new Dictionary<long, UnplannedCode>();

            foreach (unplanned_code rec in query)
            {
                codeMap.Add(rec.id, rec);

                if (!rec.parent_id.HasValue || rec.parent_id.Value == rec.id)
                {
                    UnplannedCode code = CreateUnplannedCodeDTO(rec);
                    codes.Add(code);
                    parentMap.Add(code.Id, code);
                }
                else
                {
                    childList.Add(rec);

                    if (!parentList.Contains(rec.parent_id.Value))
                    {
                        parentList.Add(rec.parent_id.Value);
                    }
                }
            }

            foreach (long parentID in parentList)
            {
                if (codeMap.ContainsKey(parentID) && !parentMap.ContainsKey(parentID))
                {
                    UnplannedCode code = CreateUnplannedCodeDTO(codeMap[parentID]);
                    parentMap.Add(code.Id, code);
                }
            }

            foreach (unplanned_code child in childList)
            {
                if (parentMap.ContainsKey(child.parent_id.Value))
                {
                    UnplannedCode parent = parentMap[child.parent_id.Value];
                    UnplannedCode code;

                    if (parentMap.ContainsKey(child.id))
                    {
                        code = parentMap[child.id];
                    }
                    else
                    {
                        code = CreateUnplannedCodeDTO(child);
                    }

                    parent.Children.Add(code);
                }
            }

            return codes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Directorate CreateDirectorateDTO(team t, bool loadTeams)
        {
            DTO.Team team = CreateTeamDTO(t, false);
            Directorate d = new Directorate();
            d.Id = t.id;
            d.Name = team.Name;

            if (loadTeams)
            {
                var query1 = from item in _db.GetContext().teams
                             where item.parent_id == d.Id
                             orderby item.title
                             select item;

                foreach (var rec in query1)
                {
                    d.Teams.Add(CreateTeamDTO(rec, false));
                }
            }

            var query2 = from tp in _db.GetContext().team_profiles
                         join p in _db.GetContext().profiles on tp.profile_id equals p.id
                         where tp.team_id == t.id
                         select new { tp, p };
            
            foreach (var rec2 in query2)
            {
                if (rec2.tp.role == "ADMIN")
                {
                    d.Admins.Add(_mediator.GetProfileProcessor().CreateProfileDTO(rec2.p, false, false));
                }
                else if (rec2.tp.role == "MANAGER")
                {
                    d.Managers.Add(_mediator.GetProfileProcessor().CreateProfileDTO(rec2.p, false, false));
                }
            }

            d.Admins = d.Admins.OrderBy(x => x.DisplayName).ToList();
            d.Managers = d.Managers.OrderBy(x => x.DisplayName).ToList();

            var query3 = from item in _db.GetContext().team_org_codes
                         where item.team_id == t.id
                         select item;

            foreach (var rec in query3)
            {
                d.OrgCodes.Add(rec.org_code);
            }

            return d;
        }

        /// <summary>
        /// Given a directorate name as a string,
        /// return the DTO.Directorate object with that name.
        /// </summary>
        /// <param name="name">Directorate name as a string,</param>
        /// <param name="expand">True to get a fully qualified DTO.Directorate with all class members populated.</param>
        /// <returns>Return the DTO.Directorate object with given name.</returns>
        public Directorate GetDirectorate(string name, bool loadTeams)
        {
            Directorate dir = null;
            team t = _db.GetContext().teams.SingleOrDefault(x => x.title == name && x.type == DTO.Team.TypeEnum.DIRECTORATE.ToString());

            if (t != null)
            {
                dir = CreateDirectorateDTO(t, loadTeams);
            }

            return dir;
        }

        /// <summary>
        /// Given a directorate id as a long,
        /// return the directorate team title.
        /// </summary>
        /// <param name="id">Directorate database id as a long,</param>
        /// <returns>Returns the name of the directorate team with the given id.</returns>
        public String GetDirectorateTitle(long id)
        {
            var query = from item in _db.GetContext().teams
                        where item.id == id && item.type == DTO.Team.TypeEnum.DIRECTORATE.ToString()
                        select item.title;

            if (query.Count() > 0)
            {
                return query.First();
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        public void SaveRole(Role role)
        {
            long id = role.Id;
            role r = null;
            bool insert = false;

            var query = from item in _db.GetContext().roles
                        where item.id == id
                        select item;

            if (query.Count() == 0)
            {
                insert = true;
                r = new role();
            }
            else
            {
                r = query.First();
            }

            r.title = role.Title;
            r.description = role.Description;
            r.active = role.Active;

            if (insert)
            {
                _db.GetContext().roles.InsertOnSubmit(r);
            }

            _db.SubmitChanges();

            role.Id = r.id;

            if (!insert)
            {
                var query2 = from item2 in _db.GetContext().role_actions
                             where item2.role_id == r.id
                             select item2;

                foreach (var rec in query2)
                {
                    _db.GetContext().role_actions.DeleteOnSubmit(rec);
                }

                _db.SubmitChanges();
            }

            if (role.Actions != null)
            {
                foreach (DTO.Action act in role.Actions)
                {
                    var query3 = from item3 in _db.GetContext().actions
                                 where item3.title == act.ToString()
                                 select item3;

                    role_action ra = new role_action();
                    ra.role_id = r.id;
                    ra.action_id = query3.First().id;
                    _db.GetContext().role_actions.InsertOnSubmit(ra);
                }

                _db.SubmitChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        public void SaveTeam(string directorate, Team team,
            bool applyBarriers, bool applyUnplanned, bool applyTaskTypes)
        {
            Team d = directorate != null ? GetTeam(directorate) : null;
            long id = team.Id;
            //long parentId = team.ParentId;
            team t = id > 0 ? _db.GetContext().teams.SingleOrDefault(x => x.id == id) : null;
            bool insert = false;

            if (t == null)
            {
                t = new team();
                _db.GetContext().teams.InsertOnSubmit(t);
                insert = true;
            }

            t.parent_id = d != null ? d.Id : (team.ParentId == 0 ? (long?)null : team.ParentId);
            t.title = team.Name;

            if (team.Owner != null)
            {
                t.profile = _db.GetContext().profiles.SingleOrDefault(x => x.id == team.Owner.Id);
            }

            t.complexity_based = team.ComplexityBased;
            t.active = team.Active;
            t.type = team.Type.ToString();

            _db.SubmitChanges();

            id = team.Id = t.id;            

            var query = from item in _db.GetContext().team_profiles
                        where item.team_id == id
                        select item;

            foreach (var rec in query)
            {
                _db.GetContext().team_profiles.DeleteOnSubmit(rec);
            }

            foreach (Profile p in team.Members)
            {
                team_profile r = new team_profile();
                r.team_id = id;
                r.profile_id = p.Id;
                r.role = "MEMBER";
                _db.GetContext().team_profiles.InsertOnSubmit(r);
            }

            foreach (Profile p in team.Admins)
            {
                team_profile r = new team_profile();
                r.team_id = id;
                r.profile_id = p.Id;
                r.role = "ADMIN";
                _db.GetContext().team_profiles.InsertOnSubmit(r);
            }

            if (d != null && insert)
            {
                if (applyBarriers)
                {
                    List<Barrier> barriers = GetBarrierList(d, false, true);

                    foreach (Barrier b in barriers)
                    {
                        ApplyBarrier(t, b);
                    }
                }

                if (applyUnplanned)
                {
                    List<UnplannedCode> codes = GetUnplannedCodeList(d, false, true);

                    foreach (UnplannedCode uc in codes)
                    {
                        ApplyUnplannedCode(t, uc);
                    }
                }

                if (applyTaskTypes)
                {
                    List<TaskType> types = GetTaskTypeList(d, false, false, true);

                    foreach (TaskType tt in types)
                    {
                        ApplyTaskType(t, tt);
                    }
                }
            }

            _db.SubmitChanges();
        }

        private void ApplyBarrier(team team, Barrier b)
        {
            team_barrier r = new team_barrier();
            r.team = team;
            r.barrier_id = b.Id;
            _db.GetContext().team_barriers.InsertOnSubmit(r);

            foreach (Barrier child in b.Children)
            {
                ApplyBarrier(team, child);
            }
        }

        private void ApplyUnplannedCode(team team, UnplannedCode uc)
        {
            team_unplanned_code r = new team_unplanned_code();
            r.team = team;
            r.unplanned_code_id = uc.Id;
            _db.GetContext().team_unplanned_codes.InsertOnSubmit(r);

            foreach (UnplannedCode child in uc.Children)
            {
                ApplyUnplannedCode(team, child);
            }
        }

        private void ApplyTaskType(team team, TaskType tt)
        {
            team_task_type r = new team_task_type();
            r.team = team;
            r.task_type_id = tt.Id;
            _db.GetContext().team_task_types.InsertOnSubmit(r);

            foreach (TaskType child in tt.Children)
            {
                ApplyTaskType(team, child);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorate"></param>
        public void SaveDirectorate(string org, Directorate directorate)
        {
            team d = _db.GetContext().teams.SingleOrDefault(x => x.type == "ORG");

            // TODO: Need to add ability to define and handle multiple top level organizations

            if (d == null)
            {
                throw new Exception("No top level organization is defined");
            }

            long id = directorate.Id;
            team t = id > 0 ? _db.GetContext().teams.SingleOrDefault(x => x.id == id) : null;

            if (t == null)
            {
                // need to create the team (directorate)
                t = new team();
                _db.GetContext().teams.InsertOnSubmit(t);
            }

            t.parent_id = d.id;
            t.title = directorate.Name;
            t.type = DTO.Team.TypeEnum.DIRECTORATE.ToString();

            _db.SubmitChanges();

            id = t.id;

            // Save the admins

            var query1 = from item1 in _db.GetContext().team_profiles
                         where item1.team_id == id
                         select item1;

            foreach (var rec1 in query1)
            {
                _db.GetContext().team_profiles.DeleteOnSubmit(rec1);
            }

            foreach (Profile p in directorate.Admins)
            {
                team_profile rec = new team_profile();
                rec.team_id = id;
                rec.profile_id = p.Id;
                rec.role = "ADMIN";
                _db.GetContext().team_profiles.InsertOnSubmit(rec);
            }

            foreach (Profile p in directorate.Managers)
            {
                team_profile rec = new team_profile();
                rec.team_id = id;
                rec.profile_id = p.Id;
                rec.role = "MANAGER";
                _db.GetContext().team_profiles.InsertOnSubmit(rec);
            }

            var query2 = from item in _db.GetContext().team_org_codes
                         where item.team_id == id
                         select item;

            foreach (var rec in query2)
            {
                _db.GetContext().team_org_codes.DeleteOnSubmit(rec);
            }

            foreach (string code in directorate.OrgCodes)
            {
                team_org_code orgCode = new team_org_code();
                orgCode.team_id = id;
                orgCode.org_code = code;
                _db.GetContext().team_org_codes.InsertOnSubmit(orgCode);
            }

            // Make sure each team has the directorate as its parent

            foreach (Team t1 in directorate.Teams)
            {
                team rec = _db.GetContext().teams.SingleOrDefault(x => x.id == t1.Id);
                rec.parent_id = id;
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="program"></param>
        public void SaveProgram(Program program)
        {
            long id = program.Id;
            program p = null;

            if (id == 0)
            {
                 p = new program();
                _db.GetContext().programs.InsertOnSubmit(p);
            }
            else
            {
                p = _db.GetContext().programs.SingleOrDefault(x => x.id == id);
            }

            p.title = program.Title;
            p.active = program.Active;

            _db.SubmitChanges();

            program.Id = p.id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="barrier"></param>
        public void SaveBarrier(Team team, Barrier barrier, bool applyToTeams)
        {
            _db.BeginTransaction();

            try
            {
                barrier b = null;
                bool insert = false;

                if (barrier.Id > 0)
                {
                    b = _db.GetContext().barriers.SingleOrDefault(x => x.id == barrier.Id);
                }

                if (b == null)
                {
                    b = new barrier();
                    _db.GetContext().barriers.InsertOnSubmit(b);
                    insert = true;
                }

                b.code = barrier.Code;
                b.barrier1 = barrier.ParentId == 0 ? null : _db.GetContext().barriers.Single(x => x.id == barrier.ParentId);
                b.title = barrier.Title;
                b.description = barrier.Description;
                b.active = barrier.Active;
                b.team_id = b.team_id == 0 ? team.Id : b.team_id;

                _db.SubmitChanges();

                barrier.Id = b.id;

                if (applyToTeams && insert && team.Type == Team.TypeEnum.DIRECTORATE)
                {
                    var childTeams = from item in _db.GetContext().teams
                                     where item.parent_id == team.Id && item.id != team.Id
                                     select item;

                    foreach (team ct in childTeams)
                    {
                        team_barrier r = new team_barrier();
                        r.team = ct;
                        r.barrier = b;
                        _db.GetContext().team_barriers.InsertOnSubmit(r);
                    }

                    _db.SubmitChanges();
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unplaned_code"></param>
        public void SaveUnplannedCode(Team team, UnplannedCode code, bool applyToTeams)
        {
            _db.BeginTransaction();

            try
            {
                unplanned_code c = null;
                bool insert = false;

                if (code.Id > 0)
                {
                    c = _db.GetContext().unplanned_codes.SingleOrDefault(x => x.id == code.Id);
                }

                if (c == null)
                {
                    c = new unplanned_code();
                    _db.GetContext().unplanned_codes.InsertOnSubmit(c);
                    insert = true;
                }

                c.code = code.Code;
                c.unplanned_code1 = code.ParentId == 0 ? null : _db.GetContext().unplanned_codes.Single(x => x.id == code.ParentId);
                c.title = code.Title;
                c.active = code.Active;
                c.description = code.Description;
                c.team_id = c.team_id == 0 ? team.Id : c.team_id;

                _db.SubmitChanges();

                code.Id = c.id;

                if (applyToTeams && insert && team.Type == Team.TypeEnum.DIRECTORATE)
                {
                    var childTeams = from item in _db.GetContext().teams
                                     where item.parent_id == team.Id && item.id != team.Id
                                     select item;

                    foreach (team ct in childTeams)
                    {
                        team_unplanned_code r = new team_unplanned_code();
                        r.team = ct;
                        r.unplanned_code = c;
                        _db.GetContext().team_unplanned_codes.InsertOnSubmit(r);
                    }

                    _db.SubmitChanges();
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task_type"></param>
        public void SaveComplexityCode(Team team, TaskType type, Complexity complex)
        {
            complexity c = null;

            if (complex.Id > 0)
            {
                c = _db.GetContext().complexities.SingleOrDefault(x => x.id == complex.Id);
            }

            if (c == null)
            {
                c = new complexity();
                _db.GetContext().complexities.InsertOnSubmit(c);
            }

            c.title = complex.Title;
            c.active = complex.Active;
            c.hours = complex.Hours;
            c.task_type = _db.GetContext().task_types.Single(x => x.id == type.Id);
            c.team = _db.GetContext().teams.Single(x => x.id == team.Id);
            c.sort_order = complex.SortOrder;

            _db.SubmitChanges();

            complex.Id = c.id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task_type"></param>
        public void SaveTaskType(Team team, TaskType type, bool applyToTeams)
        {
            _db.BeginTransaction();

            try
            {
                task_type t = null;
                bool insert = false;

                if (type.Id > 0)
                {
                    t = _db.GetContext().task_types.SingleOrDefault(x => x.id == type.Id);
                }

                if (t == null)
                {
                    t = new task_type();
                    _db.GetContext().task_types.InsertOnSubmit(t);
                    insert = true;
                }

                t.task_type1 = type.ParentId == 0 ? null : _db.GetContext().task_types.Single(x => x.id == type.ParentId);
                t.title = type.Title;
                t.active = type.Active;
                t.description = type.Description;
                t.team = _db.GetContext().teams.Single(x => x.id == team.Id);

                _db.SubmitChanges();
                type.Id = t.id;

                if (applyToTeams && insert && team.Type == Team.TypeEnum.DIRECTORATE)
                {
                    var childTeams = from item in _db.GetContext().teams
                                  where item.parent_id == team.Id && item.id != team.Id
                                  select item;

                    foreach (team ct in childTeams)
                    {
                        team_task_type r = new team_task_type();
                        r.team = ct;
                        r.task_type = t;
                        _db.GetContext().team_task_types.InsertOnSubmit(r);
                    }

                    _db.SubmitChanges();
                }
                                
                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool IsProfileOnTeam(Profile profile)
        {
            return _db.GetContext().team_profiles.Count(x => x.profile_id == profile.Id && x.role == "MEMBER") > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public bool IsTeamAdmin(long profileId, long teamId)
        {
            if (teamId == -1)
            {
                team_profile tp = _db.GetContext().team_profiles.SingleOrDefault(x => x.profile_id == profileId && x.role == "MEMBER");

                if (tp != null)
                {
                    teamId = tp.team_id;
                }
                else
                {
                    return false;
                }
            }

            if (_db.GetContext().teams.Count(x => x.id == teamId && x.owner_id == profileId) > 0)
            {
                return true;
            }
            else if (_db.GetContext().team_profiles.Count(x => x.profile_id == profileId && x.team_id == teamId && x.role == "ADMIN") > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamID"></param>
        /// <returns></returns>
        public bool IsDirectorateAdminMgr(long profileID, long directorateID)
        {
            return _db.GetContext().team_profiles.Count(
                x => x.profile_id == profileID && x.team_id == directorateID && (x.role == "ADMIN" || x.role == "MANAGER")) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="barriers"></param>
        public void ApplyBarriers(DTO.Team team)
        {
            team t = _db.GetContext().teams.SingleOrDefault(x => x.id == team.Id);

            _db.GetContext().team_barriers.DeleteAllOnSubmit(from item in _db.GetContext().team_barriers
                                                             where item.team_id == team.Id
                                                             select item);

            _db.SubmitChanges();

            t = _db.GetContext().teams.SingleOrDefault(x => x.id == team.Id);

            foreach (DTO.Barrier b in team.SelectedBarriers)
            {
                team_barrier r = new team_barrier();
                r.team_id = team.Id;
                r.barrier_id = b.Id;
                _db.GetContext().team_barriers.InsertOnSubmit(r);
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="task_types"></param>
        public void ApplyTaskTypes(Team team)
        {
            var query = from item in _db.GetContext().team_task_types
                        where item.team_id == team.Id
                        select item;

            foreach (var rec in query)
            {
                _db.GetContext().team_task_types.DeleteOnSubmit(rec);
            }

            foreach (DTO.TaskType t in team.SelectedTaskTypes)
            {
                team_task_type r = new team_task_type();
                r.team_id = team.Id;
                r.task_type_id = t.Id;
                _db.GetContext().team_task_types.InsertOnSubmit(r);
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="codes"></param>
        public void ApplyUnplannedCodes(Team team)
        {
            var t = _db.GetContext().teams.SingleOrDefault(x => x.id == team.Id);

            var query = from item in _db.GetContext().team_unplanned_codes
                        where item.team_id == team.Id
                        select item;

            foreach (var rec in query)
            {
                _db.GetContext().team_unplanned_codes.DeleteOnSubmit(rec);
            }

            _db.SubmitChanges();

            t = _db.GetContext().teams.SingleOrDefault(x => x.id == team.Id);

            foreach (DTO.UnplannedCode u in team.SelectedUnplannedCodes)
            {
                team_unplanned_code r = new team_unplanned_code();
                r.team_id = team.Id;
                r.unplanned_code_id = u.Id;
                _db.GetContext().team_unplanned_codes.InsertOnSubmit(r);
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Program GetProgram(long id)
        {
            Program prog = null;
            program p = _db.GetContext().programs.SingleOrDefault(x => x.id == id);

            if (p != null)
            {
                prog = CreateProgramDTO(p);
            }

            return prog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public TaskType GetTaskType(long taskTypeID)
        {
            task_type rec = _db.GetContext().task_types.SingleOrDefault(x => x.id == taskTypeID);
            return CreateTaskTypeDTO(rec, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public TaskType GetTaskType(long t, long? teamID)
        {
            task_type rec = _db.GetContext().task_types.SingleOrDefault(x => x.id == t);
            long? directorateID = null;

            if (teamID.HasValue)
            {
                _db.GetContext().teams.Where(
                     x => x.id == teamID.Value && x.type == Team.TypeEnum.TEAM.ToString()).Select(x => x.parent_id).SingleOrDefault();

                if (!directorateID.HasValue)
                {
                    directorateID = teamID;
                }
            }

            return CreateTaskTypeDTO(rec, directorateID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Complexity GetComplexityCode(long c)
        {
            complexity rec = _db.GetContext().complexities.SingleOrDefault(x => x.id == c);
            return CreateComplexityDTO(rec);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(Profile profile)
        {
            DTO.Team team = null;

            team_profile rec = _db.GetContext().team_profiles.SingleOrDefault(x => x.profile_id == profile.Id && x.role == "MEMBER");

            if (rec != null)
            {
                team = GetTeam(rec.team_id, false);
            }

            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(Profile profile, string role)
        {
            DTO.Team team = null;

            if (profile != null)
            {
                team_profile rec = _db.GetContext().team_profiles.FirstOrDefault(x => x.profile_id == profile.Id && x.role == role);

                if (rec != null)
                {
                    team = GetTeam(rec.team_id, false);
                }
            }

            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Team> GetTeams(DTO.Team.TypeEnum type)
        {
            List<DTO.Team> teams = new List<Team>();

            var query = from item in _db.GetContext().teams
                        where item.type == type.ToString()
                        orderby item.title
                        select item;

            foreach (var team in query)
            {
                teams.Add(CreateTeamDTO(team, false));
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Team> GetAllTeams()
        {
            List<DTO.Team> teams = new List<Team>();

            var query = from item in _db.GetContext().teams
                        orderby item.title
                        select item;

            foreach (var team in query)
            {
                teams.Add(CreateTeamDTO(team, false));
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeams(Profile profile)
        {
            List<DTO.Team> teams = new List<Team>();

            var query = from item1 in _db.GetContext().teams
                        join item2 in _db.GetContext().team_profiles on item1.id equals item2.team_id
                        where item2.profile_id == profile.Id
                        orderby item1.title
                        select item1;

            foreach (var team in query)
            {
                teams.Add(CreateTeamDTO(team, false));
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeams(Profile profile, string role)
        {
            List<DTO.Team> teams = new List<Team>();

            var query = from item1 in _db.GetContext().teams
                        join item2 in _db.GetContext().team_profiles on item1.id equals item2.team_id
                        where item2.profile_id == profile.Id && item2.role == role
                        orderby item1.title
                        select item1;

            foreach (var team in query)
            {
                teams.Add(CreateTeamDTO(team, false));
            }

            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DTO.Team GetDirectorate(Profile profile)
        {
            DTO.Team t = null;

            team_profile rec1 = _db.GetContext().team_profiles.SingleOrDefault(x => x.profile_id == profile.Id && x.role == "MEMBER");

            if (rec1 != null)
            {
                team rec2 = _db.GetContext().teams.SingleOrDefault(x => x.parent_id == rec1.team_id);

                if (rec2 != null)
                {
                    t = GetTeam(rec2.id, false);
                }
                else
                {
                    t = GetTeam(rec1.team_id, false);
                }
            }

            return t;
        }

        public long? GetDirectorateId(long teamId)
        {
            team rec = (from child in _db.GetContext().teams
                        join parent in _db.GetContext().teams on child.parent_id equals parent.id
                        where child.id == teamId
                        select parent).SingleOrDefault();

            if (rec != null)
            {
                return rec.id;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Directorate> GetDirectorateList()
        {
            List<DTO.Directorate> items = new List<DTO.Directorate>();

            var query = from item in _db.GetContext().teams
                        where item.type == DTO.Team.TypeEnum.DIRECTORATE.ToString()
                        orderby item.title
                        select item;

            foreach (var rec in query)
            {
                items.Add(GetDirectorate(rec.title, false));
            }

            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetDirectorateNameList()
        {
            List<string> names;

            var query = from item in _db.GetContext().teams
                        where item.type == DTO.Team.TypeEnum.DIRECTORATE.ToString()
                        orderby item.title
                        select item.title;

            if (query.Count() > 0)
            {
                names = query.ToList();
            }
            else
            {
                names = new List<string>();
            }

            return names;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Profile> GetDirectorateMembersByOrgCodes(DTO.Directorate directorate, bool loadCodes)
        {
            List<Profile> members = new List<Profile>();
            List<string> orgCodes;

            if (loadCodes)
            {
                orgCodes = (from item in _db.GetContext().team_org_codes
                            where item.team_id == directorate.Id
                            select item.org_code).ToList();
            }
            else
            {
                orgCodes = directorate.OrgCodes;
            }

            foreach (string code in orgCodes)
            {
                IQueryable<profile> membersQuery = from p in _db.GetContext().profiles
                                                   where p.org_code.StartsWith(code)
                                                   select p;

                foreach (var p in membersQuery)
                {
                    members.Add(_mediator.GetProfileProcessor().CreateProfileDTO(p, false, false));
                }
            }

            return members.OrderBy(x => x.DisplayName).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetBarrierExtendedDescription(long team_id, long barrier_id)
        {
            var extendedDescription = _db.GetContext().barriers_descriptions.SingleOrDefault(item => item.team_id == team_id && item.barrier_id == barrier_id);

            return extendedDescription == null ? null : extendedDescription.description;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SaveBarrierExtendedDescription(long team_id, long barrier_id, string description)
        {
            barriers_description d = _db.GetContext().barriers_descriptions.SingleOrDefault(x => (x.barrier_id == barrier_id && x.team_id == team_id));

            if (d == null)
            {
                d = new barriers_description();
                _db.GetContext().barriers_descriptions.InsertOnSubmit(d);

                d.team_id = team_id;
                d.barrier_id = barrier_id;

                _db.SubmitChanges();
            }

            d.description = description;

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUnplannedCodeExtendedDescription(long team_id, long unplanned_code_id)
        {
            var extendedDescription = _db.GetContext().unplanned_codes_descriptions.SingleOrDefault(item => item.team_id == team_id && item.unplanned_code_id == unplanned_code_id);

            return extendedDescription == null ? null : extendedDescription.description;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SaveUnplannedCodeExtendedDescription(long team_id, long unplanned_code_id, string description)
        {
            unplanned_codes_description d = _db.GetContext().unplanned_codes_descriptions.SingleOrDefault(x => (x.unplanned_code_id == unplanned_code_id && x.team_id == team_id));

            if (d == null)
            {
                d = new unplanned_codes_description();
                _db.GetContext().unplanned_codes_descriptions.InsertOnSubmit(d);

                d.team_id = team_id;
                d.unplanned_code_id = unplanned_code_id;
            }

            d.description = description;

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SaveSystemMessage(string message)
        {
            string[] profiles = ConfigurationManager.AppSettings["RootProfiles"].Split(',');

            if (profiles.Length > 0)
            {
                DTO.Profile profile = _mediator.GetProfileProcessor().GetProfileByUsername(profiles[0]);

                if (profile != null)
                {
                    preference p = _db.GetContext().preferences.SingleOrDefault(x => x.profile_id == profile.Id && x.name == "SystemMessage");

                    if (p == null)
                    {
                        p = new preference();
                        _db.GetContext().preferences.InsertOnSubmit(p);
                    }

                    p.profile_id = profile.Id;
                    p.name = "SystemMessage";
                    p.value = message;

                    _db.SubmitChanges();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="profile"></param>
        public void SendSystemAlert(string subject, string message, DTO.Profile profile)
        {
            if (subject != string.Empty && message != string.Empty)
            {
                List<long> profileIDs = _db.GetContext().team_profiles.Select(x => x.profile_id).Distinct().ToList();

                profileIDs.AddRange(_db.GetContext().role_profiles.Where(
                    x => !profileIDs.Contains(x.profile_id)).Select(x => x.profile_id).Distinct().ToList());

                List<DTO.Profile> profiles = new List<Profile>();

                foreach (long id in profileIDs)
                {
                    DTO.Profile p = new Profile();
                    p.Id = id;
                    profiles.Add(p);
                }

                Alert alert = new Alert();
                alert.Creator = profile;
                alert.EntryDate = DateTime.Now;
                alert.Subject = subject;
                alert.Message = message;
                alert.LinkedId = -1;
                alert.LinkedType = DTO.Alert.AlertEnum.SYSTEM;
                alert.Acknowledged = false;

                _mediator.GetProfileProcessor().SendAlert(alert, profiles);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="directorateID"></param>
        /// <returns></returns>
        public List<TaskType> ImportTaskTypes(string path, long directorateID)
        {
            List<TaskType> taskTypes = new List<TaskType>();
            OleDbConnection conn;
            string fileTitle = string.Empty;

            string connstr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                path + ";Extended Properties=Excel 12.0";

            conn = new OleDbConnection(connstr);
            conn.Open();

            try
            {
                DataTable table = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                // Get the name of the first sheet in the workbook

                string sheet_name = table.Rows[0]["TABLE_NAME"].ToString();

                // Select all records into a DataSet
                OleDbDataAdapter da = new OleDbDataAdapter();
                da.SelectCommand = new OleDbCommand("select * from [" + sheet_name + "]", conn);
                DataSet ds = new DataSet();
                da.Fill(ds, "TaskTypes");

                if (!ds.Tables[0].Columns.Contains("Task Type"))
                {
                    throw new Exception("Spreadsheet must have a \"Title\" column");
                }

                double hours;
                List<task_type> tts = _db.GetContext().task_types.Where(x => x.team_id == directorateID).ToList();
                long? parentID = _db.GetContext().teams.Where(x => x.id == directorateID).Select(x => x.parent_id).SingleOrDefault();

                if (parentID.HasValue)
                {
                    tts.AddRange(_db.GetContext().task_types.Where(x => x.team_id == parentID.Value).ToList());
                }

                List<string> existing = tts.Select(x => x.title).ToList();
                List<string> children = tts.Where(x => x.parent_id.HasValue && x.parent_id.Value != x.id).Select(x => x.title).ToList();
                List<string> parents = new List<string>();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    TaskType tt = new TaskType();

                    if (!string.IsNullOrEmpty(ds.Tables[0].Rows[i]["Task Type"].ToString()))
                    {
                        tt.Title = ds.Tables[0].Rows[i]["Task Type"].ToString();

                        if (existing.Contains(tt.Title))
                        {
                            tt.ErrorMessage.Add("Warning: Task type \"" + tt.Title + "\" already exists and will not be imported");
                        }
                        else
                        {
                            existing.Add(tt.Title);
                        }
                    }
                    else
                    {
                        tt.Error = true;
                        tt.ErrorMessage.Add("Error: You must enter a task type");
                    }

                    if (ds.Tables[0].Columns.Contains("Task Type Parent") &&
                        !string.IsNullOrEmpty(ds.Tables[0].Rows[i]["Task Type Parent"].ToString()))
                    {
                        tt.ParentTitle = ds.Tables[0].Rows[i]["Task Type Parent"].ToString();

                        if (children.Contains(tt.ParentTitle))
                        {
                            tt.Error = true;
                            tt.ErrorMessage.Add("Error: Parent \"" + tt.ParentTitle + "\" is not a root level task type");
                        }
                        else if (parents.Contains(tt.Title))
                        {
                            tt.Error = true;
                            tt.ErrorMessage.Add("Error: Task type \"" + tt.Title + "\" has children and can not be a parent");
                        }
                        else
                        {
                            children.Add(tt.Title);
                            parents.Add(tt.ParentTitle);
                        }
                    }

                    if (ds.Tables[0].Columns.Contains("Description"))
                    {
                        tt.Description = ds.Tables[0].Rows[i]["Description"].ToString();
                    }


                    for (int j = 1; j <= 6; j++)
                    {
                        string level = "Level " + j.ToString();

                        if (ds.Tables[0].Columns.Contains(level) &&
                            !string.IsNullOrEmpty(ds.Tables[0].Rows[i][level].ToString()))
                        {
                            string hourStr = ds.Tables[0].Rows[i][level].ToString();

                            if (double.TryParse(hourStr, out hours))
                            {
                                Complexity comp = new Complexity();
                                comp.Title = level;
                                comp.Hours = Convert.ToDouble(decimal.Round(Convert.ToDecimal(hours), 1));
                                comp.SortOrder = j;
                                tt.Complexities.Add(comp);

                                if (comp.Hours != hours)
                                {
                                    tt.ErrorMessage.Add("Warning: " + level + " hours rounded");
                                }
                            }
                            else
                            {
                                tt.Error = true;
                                tt.ErrorMessage.Add("Error: " + hourStr + " is not a valid number for " + level);
                            }
                        }
                    }

                    taskTypes.Add(tt);
                }
            }
            catch
            {
                conn.Close();
                throw;
            }

            conn.Close();
            return taskTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypes"></param>
        /// <param name="directorateID"></param>
        public void SaveImportedTaskTypes(List<TaskType> taskTypes, long directorateID)
        {
            Team dir = GetTeam(directorateID, false);
            List<task_type> tts = _db.GetContext().task_types.Where(x => x.team_id == directorateID).ToList();
            long? parentID = _db.GetContext().teams.Where(x => x.id == directorateID).Select(x => x.parent_id).SingleOrDefault();

            if (parentID.HasValue)
            {
                tts.AddRange(_db.GetContext().task_types.Where(x => x.team_id == parentID.Value).ToList());
            }

            Dictionary<string, long> typeIDs = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            List<string> existing = new List<string>();

            foreach (task_type tt in tts)
            {
                typeIDs[tt.title] = tt.id;
                existing.Add(tt.title);
            }

            _db.BeginTransaction();

            try
            {
                foreach (TaskType type in taskTypes)
                {
                    if (!type.Error && !existing.Contains(type.Title))
                    {
                        if (!string.IsNullOrEmpty(type.ParentTitle) && type.Title != type.ParentTitle)
                        {
                            if (typeIDs.ContainsKey(type.ParentTitle))
                            {
                                type.ParentId = typeIDs[type.ParentTitle];
                            }
                            else
                            {
                                TaskType parent = new TaskType();
                                parent.Title = type.ParentTitle;
                                parent.Active = true;
                                SaveTaskType(dir, parent, false);

                                type.ParentId = parent.Id;
                                typeIDs.Add(parent.Title, parent.Id);
                            }
                        }
                        else
                        {
                            type.ParentId = 0;
                        }

                        if (typeIDs.ContainsKey(type.Title))
                        {
                            type.Id = typeIDs[type.Title];
                        }

                        type.Active = true;
                        SaveTaskType(dir, type, false);

                        typeIDs.Add(type.Title, type.Id);
                        existing.Add(type.Title);

                        foreach (Complexity comp in type.Complexities)
                        {
                            comp.Active = true;
                            SaveComplexityCode(dir, type, comp);
                        }
                    }
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }
    }
}
