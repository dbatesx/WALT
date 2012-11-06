using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WALT.BLL
{
    /// <summary>
    /// Provides all the business rules necessary to support
    /// managing all application and organization data.
    /// </summary>
    public class AdminManager : Manager
    {
        private static string _id = typeof(AdminManager).ToString();

        /// <summary>
        /// All BLL managers are singletons.  This method returns
        /// the session instance.
        /// </summary>
        /// <returns>Admin Manager instance</returns>
        public static AdminManager GetInstance()
        {
            AdminManager m = (AdminManager)GetSessionValue(_id);

            if (m == null)
            {
                m = new AdminManager();
                SetSessionValue(_id, m);
            }

            return m;
        }

        /// <summary>
        /// Private constructor.  Can only have one instance
        /// per session.
        /// </summary>
        private AdminManager()
            : base()
        {
        }

        /// <summary>
        /// Get the top level team (organization).
        /// </summary>
        /// <returns>Team object</returns>
        public DTO.Team GetOrg()
        {
            DTO.Team org = null;

            try
            {
                org = _dalMediator.GetAdminProcessor().GetOrg();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return org;
        }

        /// <summary>
        /// Method to retreive a list of system roles.
        /// </summary>
        /// <returns>List of system roles</returns>
        public List<DTO.Role> GetRoleList()
        {
            List<DTO.Role> roles = new List<WALT.DTO.Role>();

            try
            {
                roles = _dalMediator.GetAdminProcessor().GetRoleList();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return roles;
        }

        /// <summary>
        /// Get a list of the system actions.
        /// </summary>
        /// <returns>List of system actions</returns>
        public List<string> GetSystemActionList()
        {
            List<string> actions = new List<string>();

            try
            {
                actions = _dalMediator.GetAdminProcessor().GetSystemActionList();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<long, string> GetProgramDictionary()
        {
            Dictionary<long, string> programs = new Dictionary<long, string>();
                       
            try
            {
                programs = _dalMediator.GetAdminProcessor().GetProgramDictionary();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return programs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DTO.Program GetProgram(long id)
        {
            return _dalMediator.GetAdminProcessor().GetProgram(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Program> GetProgramList()
        {
            List<DTO.Program> programs = new List<WALT.DTO.Program>();

            try
            {
                programs = _dalMediator.GetAdminProcessor().GetProgramList();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return programs;
        }

        /// <summary>
        /// Get a list of team names.
        /// </summary>
        /// <returns>A list of team names</returns>
        public List<string> GetTeamNameList()
        {
            List<string> teams = new List<string>();

            try
            {
                teams = _dalMediator.GetAdminProcessor().GetTeamNameList();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return teams;
        }

        /// <summary>
        /// Get a list of team names with their directorate name.
        /// </summary>
        /// <returns>A list of team names with the directorate name.</returns>
        public List<string> GetTeamNameListWithDirectorate()
        {
            List<string> teams = new List<string>();

            try
            {
                teams = _dalMediator.GetAdminProcessor().GetTeamNameListWithDirectorate();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return teams;
        }

        /// <summary>
        /// Get a barrier object (no child objects)
        /// </summary>
        /// <returns></returns>
        public DTO.Barrier GetBarrier(long id)
        {
            return _dalMediator.GetAdminProcessor().GetBarrier(id);
        }

        /// <summary>
        /// Get a single depth list of child barrier objects for a barrier.
        /// </summary>
        /// <returns></returns>
        public List<DTO.Barrier> GetBarrierChildren(long barrierId, long teamId)
        {
            return _dalMediator.GetAdminProcessor().GetBarrierChildren(barrierId, teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Barrier> GetBarrierList()
        {
            return GetBarrierList(_dalMediator.GetAdminProcessor().GetTeam(_profile), true, true);
        }

        /// <summary>
        /// Get a recursive list of barrier objects for a team.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="filtered"></param>
        /// <param name="active"></param>
        /// <returns>A list of barriers</returns>
        public List<DTO.Barrier> GetBarrierList(DTO.Team team, bool filtered, bool? active)
        {
            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            List<DTO.Barrier> barriers = new List<WALT.DTO.Barrier>();

            try
            {
                barriers = _dalMediator.GetAdminProcessor().GetBarrierList(team, filtered, active);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return barriers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetBarrierLevel(long barrierId)
        {
            return _dalMediator.GetAdminProcessor().GetBarrierLevel(barrierId);
        }

        /// <summary>
        /// Get a recursive list of task types for a team.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="loadComplexity"></param>
        /// <param name="filtered"></param>
        /// <param name="active"></param>
        /// <returns>List of task types</returns>
        public List<DTO.TaskType> GetTaskTypeList(DTO.Team team, bool loadComplexity, bool filtered, bool? active)
        {
            if (team == null)
            {
                throw new Exception("Team is not defined");
            }

            List<DTO.TaskType> types = new List<WALT.DTO.TaskType>();

            try
            {
                types = _dalMediator.GetAdminProcessor().GetTaskTypeList(team, loadComplexity, filtered, active);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return types;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DTO.TaskType GetTaskType(long id)
        {
            return _dalMediator.GetAdminProcessor().GetTaskType(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DTO.TaskType GetTaskType(long id, long? teamID)
        {
            return _dalMediator.GetAdminProcessor().GetTaskType(id, teamID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DTO.Complexity GetComplexityCode(long id)
        {
            return _dalMediator.GetAdminProcessor().GetComplexityCode(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DTO.UnplannedCode GetUnplannedCode(long id)
        {
            return _dalMediator.GetAdminProcessor().GetUnplannedCode(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetUnplannedCodeLevel(long barrierId)
        {
            return _dalMediator.GetAdminProcessor().GetUnplannedCodeLevel(barrierId);
        }

        /// <summary>
        /// Get a single depth list of child unplanned code objects for a unplanned code.
        /// </summary>
        /// <returns></returns>
        public List<DTO.UnplannedCode> GetUnplannedCodeChildren(long unplannedId, long teamId)
        {
            return _dalMediator.GetAdminProcessor().GetUnplannedCodeChildren(unplannedId, teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadComplexity"></param>
        /// <param name="filtered"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<DTO.TaskType> GetTaskTypeList(bool loadComplexity, bool filtered, bool? active)
        {
            return GetTaskTypeList(_dalMediator.GetAdminProcessor().GetTeam(_profile), loadComplexity, filtered, active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.UnplannedCode> GetUnplannedCodeList()
        {
            return GetUnplannedCodeList(_dalMediator.GetAdminProcessor().GetTeam(_profile), true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DTO.Team GetTeam()
        {
            return _dalMediator.GetAdminProcessor().GetTeam(_profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loadMembers"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(long id, bool loadMembers)
        {
            return _dalMediator.GetAdminProcessor().GetTeam(id, loadMembers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(DTO.Profile profile)
        {
            return _dalMediator.GetAdminProcessor().GetTeam(profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public DTO.Team GetTeam(DTO.Profile profile, string role)
        {
            return _dalMediator.GetAdminProcessor().GetTeam(profile, role);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public DTO.Team GetTeamByRole(string role)
        {
            return _dalMediator.GetAdminProcessor().GetTeam(_profile, role);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public DTO.Team GetTeamByRole(DTO.Profile profile, string role)
        {
            return _dalMediator.GetAdminProcessor().GetTeam(profile, role);
        }

        /// <summary>
        /// This returns a list of DTO.Team objects of the given type.
        /// Each DTO.Team is not expanded.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Non-expanded DTO.Team</returns>
        public List<DTO.Team> GetTeams(DTO.Team.TypeEnum type)
        {
            return _dalMediator.GetAdminProcessor().GetTeams(type); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeams(string role)
        {
            return _dalMediator.GetAdminProcessor().GetTeams(_profile, role);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetTeamsDictionaryByParent(long parentID)
        {
            return _dalMediator.GetAdminProcessor().GetTeamsDictionaryByParent(parentID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeamsByParent(long parentID)
        {
            return _dalMediator.GetAdminProcessor().GetTeamsByParent(parentID, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeamsByParent(long parentID, bool expand)
        {
            return _dalMediator.GetAdminProcessor().GetTeamsByParent(parentID, expand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Team> GetTeamsOwned()
        {
            return _dalMediator.GetAdminProcessor().GetTeamsOwned(_profile);
        }

              
         /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
         public Dictionary<long, string> GetTeamsOwnedDictionary(DTO.Profile profile)
        {
            return _dalMediator.GetAdminProcessor().GetTeamsOwnedDictionary(_profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<DTO.Team> GetTeamsOwned(DTO.Profile profile)
        {
            return _dalMediator.GetAdminProcessor().GetTeamsOwned(profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public List<DTO.Profile> GetTeamMembers(long teamId)
        {
            return _dalMediator.GetAdminProcessor().GetTeamMembers(teamId);
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
            return _dalMediator.GetAdminProcessor().GetTeamMembers(teamId, start, end, includeCurrent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public bool IsTeamAdmin(long profileId, long teamId)
        {
            return _dalMediator.GetAdminProcessor().IsTeamAdmin(profileId, teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public bool IsAdminOfMemberTeam(long profileId)
        {
            return _dalMediator.GetAdminProcessor().IsTeamAdmin(profileId, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public bool IsTeamAdmin(long teamId)
        {
            return _dalMediator.GetAdminProcessor().IsTeamAdmin(_profile.Id, teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public bool IsTeamAdmin(DTO.Team team)
        {
            if (team == null)
            {
                return false;
            }

            return _dalMediator.GetAdminProcessor().IsTeamAdmin(_profile.Id, team.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool IsAdminOfAnyTeam(DTO.Profile profile)
        {
            if (_dalMediator.GetAdminProcessor().GetTeamsOwned(profile).Count > 0 ||
                _dalMediator.GetAdminProcessor().GetTeams(profile, "ADMIN").Count > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool IsDirectorateManagerOfAny(DTO.Profile profile)
        {
            if (_dalMediator.GetAdminProcessor().GetDirectorateManagedIds(profile).Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorate"></param>
        /// <returns></returns>
        public bool IsDirectorateAdmin(DTO.Directorate directorate)
        {
            return (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE)
                || directorate != null && directorate.Managers.Exists(x => x.Username == _profile.Username)
                || directorate != null && directorate.Admins.Exists(x => x.Username == _profile.Username));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorateID"></param>
        /// <returns></returns>
        public bool IsDirectorateAdminMgr(long directorateID)
        {
            return _dalMediator.GetAdminProcessor().IsDirectorateAdminMgr(_profile.Id, directorateID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="filtered"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<DTO.UnplannedCode> GetUnplannedCodeList(DTO.Team team, bool filtered, bool? active)
        {
            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            List<DTO.UnplannedCode> unplanned = new List<WALT.DTO.UnplannedCode>();

            try
            {
                unplanned = _dalMediator.GetAdminProcessor().GetUnplannedCodeList(team, filtered, active);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return unplanned;
        }

        /// <summary>
        /// Get a team object based upon the team name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Team object</returns>
        public DTO.Team GetTeam(string name)
        {
            return GetTeam(name, true);
        }

        /// <summary>
        /// Get a team object based upon the team name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadMembers"></param>
        /// <returns>Team object</returns>
        public DTO.Team GetTeam(string name, bool loadMembers)
        {
            if (name == null || name.Length == 0)
            {
                throw new Exception("Team name is blank");
            }

            DTO.Team team = null;

            try
            {
                team = _dalMediator.GetAdminProcessor().GetTeam(name, loadMembers);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return team;
        }

        /// <summary>
        /// Get a directorate object based upon the directorate name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadTeams"></param>
        /// <returns>Directorate object</returns>
        public DTO.Directorate GetDirectorate(string name, bool loadTeams)
        {
            if (name == null || name.Length == 0)
            {
                throw new Exception("Directorate name is blank");
            }

            DTO.Directorate directorate = null;

            try
            {
                directorate = _dalMediator.GetAdminProcessor().GetDirectorate(name, loadTeams);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return directorate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<long> GetDirectoratesManagedIds(DTO.Profile profile)
        {
            return _dalMediator.GetAdminProcessor().GetDirectorateManagedIds(profile);
        }

        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetDirectoratesForALMsDMs(DTO.Profile profile)
        {
            return _dalMediator.GetAdminProcessor().GetDirectorateForALMsDMs(profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public long? GetDirectorateID(long teamId)
        {
            return _dalMediator.GetAdminProcessor().GetDirectorateId(teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Directorate> GetDirectorateList()
        {
            return _dalMediator.GetAdminProcessor().GetDirectorateList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetDirectorateNameList()
        {
            return _dalMediator.GetAdminProcessor().GetDirectorateNameList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Profile> GetDirectorateMembersByOrgCodes(DTO.Directorate directorate, bool loadCodes)
        {
            return _dalMediator.GetAdminProcessor().GetDirectorateMembersByOrgCodes(directorate, loadCodes);
        }

        /// <summary>
        /// Saves a role to the database.
        /// </summary>
        /// <param name="role"></param>
        public void SaveRole(DTO.Role role)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.ROLE_MANAGE))
            {
                throw new Exception("You do not have authorization to manage roles");
            }

            if (role == null)
            {
                throw new Exception("Role is not defined");
            }

            if ((role.Title == null) || (role.Title.Length == 0))
            {
                throw new Exception("Role title is blank");
            }

            if ((role.Description == null) || (role.Description.Length == 0))
            {
                throw new Exception("Role description is blank");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveRole(role);
                LogComment(role, "Role " + role.Title + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorate"></param>
        /// <param name="team"></param>
        /// <param name="applyBarriers"></param>
        /// <param name="applyUnplanned"></param>
        /// <param name="applyTaskTypes"></param>
        public void SaveTeam(string directorate, DTO.Team team,
            bool applyBarriers, bool applyUnplanned, bool applyTaskTypes)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage teams");
            }

            if (team == null)
            {
                throw new Exception("Team is not defined");
            }

            if (team.Name.Length == 0)
            {
                throw new Exception("Team name is blank");
            }

            if (team.Owner == null)
            {
                throw new Exception("Team owner is not set");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveTeam(directorate, team, applyBarriers, applyUnplanned, applyTaskTypes);
                LogComment(team, "Team " + team.Name + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorate"></param>
        /// <param name="team"></param>
        public void SaveTeam(string directorate, DTO.Team team)
        {
            SaveTeam(directorate, team, false, false, false);
        }

        /// <summary>
        /// Saves a team to the database.
        /// </summary>
        /// <param name="team"></param>
        public void SaveTeam(DTO.Team team)
        {
            if (team == null || team.ParentId == 0)
            {
                throw new Exception("You can't create a new team without specifying a Directorate");
            }

            SaveTeam(null, team, false, false, false);
        }

        /// <summary>
        /// Saves a directorate to the database.
        /// </summary>
        /// <param name="directorate"></param>
        public void SaveDirectorate(DTO.Directorate directorate)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.DIRECTORATE_ADMIN))
            {
                throw new Exception("You do not have authorization to manage directorates");
            }

            if (directorate == null)
            {
                throw new Exception("Directorate is not defined");
            }

            if (directorate.Name.Length == 0)
            {
                throw new Exception("Directorate name is blank");
            }

            if (directorate.Admins.Count == 0)
            {
                throw new Exception("Directorate does not have an admin defined");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveDirectorate(null, directorate);
                LogComment(directorate, "Directorate " + directorate.Name + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Saves a program to the database.
        /// </summary>
        /// <param name="program"></param>
        public void SaveProgram(DTO.Program program)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.METADATA_MANAGE))
            {
                throw new Exception("You do not have authorization to manage programs");
            }

            if (program == null)
            {
                throw new Exception("Program is not defined");
            }

            if (program.Title.Length == 0)
            {
                throw new Exception("Program title is blank");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveProgram(program);
                LogComment(program, "Program " + program.Title + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Removes a program from the database by setting it inactive.
        /// </summary>
        /// <param name="program"></param>
        public void RemoveProgram(DTO.Program program)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.METADATA_MANAGE))
            {
                throw new Exception("You do not have authorization to manage programs");
            }

            if (program == null || program.Id == 0)
            {
                throw new Exception("Program is not defined");
            }

            program.Active = false;
            SaveProgram(program);
            LogComment(program, "Program " + program.Title + " set inactive");
        }

        /// <summary>
        /// Saves a barrier to the database.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="barrier"></param>
        /// <param name="applyToTeams"></param>
        public void SaveBarrier(DTO.Team team, DTO.Barrier barrier, bool applyToTeams)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage barriers");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (barrier == null)
            {
                throw new Exception("Barrier is not defined");
            }

            if (barrier.Code == null || barrier.Code.Length == 0)
            {
                // create the barrier code based on the parent's code
                string newCode = string.Empty;
                DTO.Barrier parent = null;

                if (barrier.ParentId > 0)
                    parent = GetBarrier(barrier.ParentId);

                if (parent == null)
                {
                    List<string> codes = (from b in GetBarrierList(GetOrg(), false, null)
                                          orderby b.Code descending
                                          select b.Code).ToList();

                    // assuming for now that the top level code is a single character
                    barrier.Code = (codes.Count == 0) ? "A" : char.ConvertFromUtf32(codes[0].ToCharArray()[0] + 1).ToString();
                }
                else
                {
                    Match match = Regex.Match(parent.Code, @"\b(\D+)(\d+)?\.?(\d+)?\b");

                    if (!match.Groups[2].Success)
                        barrier.Code = match.Groups[1].Value + (GetBarrierChildren(parent.Id, team.Id).Count + 1).ToString();
                    else if (!match.Groups[3].Success)
                        barrier.Code = match.Groups[1].Value + match.Groups[2].Value + "." + (GetBarrierChildren(parent.Id, team.Id).Count + 1).ToString();
                }

                //throw new Exception("Barrier code is blank");
            }

            if (barrier.Title.Length == 0)
            {
                throw new Exception("Barrier title is blank");
            }

            if (barrier.Description.Length == 0)
            {
                throw new Exception("Barrier description is blank");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveBarrier(team, barrier, applyToTeams);
                LogComment(barrier, "Barrier " + barrier.Code + " - " + barrier.Title + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Removes a barrier from the database by setting it inactive.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="barrier"></param>
        public void RemoveBarrier(DTO.Team team, DTO.Barrier barrier)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage barriers");
            }

            if (barrier == null || barrier.Id == 0)
            {
                throw new Exception("Barrier is not defined");
            }

            barrier.Active = false;
            SaveBarrier(team, barrier, false);
            LogComment(barrier, "Barrier " + barrier.Code + " - " + barrier.Title + " set inactive");
        }

        /// <summary>
        /// Saves an unplanned code to the database.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="unplanned_code"></param>
        /// <param name="applyToTeams"></param>
        public void SaveUnplannedCode(DTO.Team team, DTO.UnplannedCode unplanned_code, bool applyToTeams)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage unplanned codes");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (unplanned_code == null)
            {
                throw new Exception("Unplanned code is not defined");
            }

            if (unplanned_code.Code == null || unplanned_code.Code.Length == 0)
            {
                // create the unplanned code based on the parent's code
                string newCode = string.Empty;
                DTO.UnplannedCode parent = null;

                if (unplanned_code.ParentId > 0)
                    parent = GetUnplannedCode(unplanned_code.ParentId);

                if (parent == null)
                {
                    List<string> codes = (from b in GetUnplannedCodeList(GetOrg(), false, true)
                                          orderby b.Code descending
                                          select b.Code).ToList();

                    // assuming for now that the top level code is a single character
                    unplanned_code.Code = (codes.Count == 0) ? "A" : char.ConvertFromUtf32(codes[0].ToCharArray()[0] + 1).ToString();
                }
                else
                {
                    Match match = Regex.Match(parent.Code, @"\b(\D+)(\d+)?\.?(\d+)?\b");

                    if (!match.Groups[2].Success)
                        unplanned_code.Code = match.Groups[1].Value + (GetUnplannedCodeChildren(parent.Id, team.Id).Count + 1).ToString();
                    else if (!match.Groups[3].Success)
                        unplanned_code.Code = match.Groups[1].Value + match.Groups[2].Value + "." + (GetUnplannedCodeChildren(parent.Id, team.Id).Count + 1).ToString();
                }

                //throw new Exception("Unplanned code code is blank");
            }

            if (unplanned_code.Title.Length == 0)
            {
                throw new Exception("Unplanned code title is blank");
            }
            
            try
            {
                _dalMediator.GetAdminProcessor().SaveUnplannedCode(team, unplanned_code, applyToTeams);
                LogComment(unplanned_code, "Unplanned code " + unplanned_code.Code + " - " + unplanned_code.Title + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Saves a task type to the database.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="task_type"></param>
        /// <param name="applyToTeams"></param>
        public void SaveTaskType(DTO.Team team, DTO.TaskType task_type, bool applyToTeams)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage task types");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (task_type == null)
            {
                throw new Exception("Task Type is not defined");
            }

            if (task_type.Title.Length == 0)
            {
                throw new Exception("Task type title is blank");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveTaskType(team, task_type, applyToTeams);
                LogComment(task_type, "Task type " + task_type.Title + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Saves a Complexity Code to the database.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="task_type"></param>
        /// <param name="complexity"></param>
        public void SaveComplexityCode(DTO.Team team, DTO.TaskType task_type, DTO.Complexity complexity)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage complexity codes");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (task_type == null)
            {
                throw new Exception("Task Type is not defined");
            }

            if (complexity == null)
            {
                throw new Exception("Task Type is not defined");
            }

            if (task_type.Title.Length == 0)
            {
                throw new Exception("Task type title is blank");
            }

            if (complexity.Title.Length == 0)
            {
                throw new Exception("Complexity Code title is blank");
            }

            if (complexity.Active && complexity.Hours == 0)
            {
                throw new Exception("Active Complexity Code hours must be greater than 0");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveComplexityCode(team, task_type, complexity);
                LogComment(task_type, "Complexity code " + complexity.Title + " saved");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Removes a task type by setting it inactive.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="task_type"></param>
        public void RemoveTaskType(DTO.Team team, DTO.TaskType task_type)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage task types");
            }

            if (task_type == null || task_type.Id == 0)
            {
                throw new Exception("Task type is not defined");
            }

            task_type.Active = false;
            SaveTaskType(team, task_type, false);
            LogComment(task_type, "Task type " + task_type.Title + " set inactive");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="code"></param>
        public void RemoveUnplannedCode(DTO.Team team, DTO.UnplannedCode code)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TASK_MANAGE))
            {
                throw new Exception("You do not have authorization to manage unplanned codes");
            }

            if (code == null || code.Id == 0)
            {
                throw new Exception("Unplanned code is not defined");
            }

            code.Active = false;
            SaveUnplannedCode(team, code, false);
            LogComment(code, "Unplanned Code " + code.Title + " set inactive");
        }

        /// <summary>
        /// Removes a profile from a team.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="profile"></param>
        public void RemoveTeamMember(DTO.Team team, DTO.Profile profile)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage teams");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (profile == null || profile.Id == 0)
            {
                throw new Exception("Profile is not defined");
            }

            try
            {
                DTO.Profile found = team.Members.Find(delegate(DTO.Profile n) { return n.Id == profile.Id; });

                if (found != null)
                {
                    team.Members.Remove(found);
                    SaveTeam(team);
                    LogComment(team, profile.Username + " removed from ALT " + team.Name);

                    if (team.Owner.Id != _profile.Id)
                    {
                        Alert(team.Owner, "Team member removed", profile.DisplayName + " was removed from your team by " + _profile.DisplayName);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds a profile to a team.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="profile"></param>
        public void AddTeamMember(DTO.Team team, DTO.Profile profile)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage teams");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (profile == null || profile.Id == 0)
            {
                throw new Exception("Profile is not defined");
            }

            try
            {
                if (_dalMediator.GetAdminProcessor().IsProfileOnTeam(profile))
                {
                    DTO.Team t = _dalMediator.GetAdminProcessor().GetTeam(profile);

                    throw new Exception(profile.DisplayName + " is already assigned to team " + t.Name);
                }
                else
                {
                    team.Members.Add(profile);
                    SaveTeam(team);
                    LogComment(team, profile.Username + " added to ALT " + team.Name);

                    if (team.Owner.Id != _profile.Id)
                    {
                        Alert(team.Owner, "Team member added", profile.DisplayName + " was added to your team by " + _profile.DisplayName);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Removes a backup ALM from an ALT.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="profile"></param>
        public void RemoveTeamBackupALM(DTO.Team team, DTO.Profile profile)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage teams");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (profile == null || profile.Id == 0)
            {
                throw new Exception("Profile is not defined");
            }

            try
            {
                if (team.Admins.Find(delegate(DTO.Profile n) { return n.Id == profile.Id; }) != null)
                {
                    team.Admins.Remove(profile);
                    SaveTeam(team);
                    LogComment(team, profile.Username + " removed as backup ALM from ALT " + team.Name);

                    if (team.Owner.Id != _profile.Id)
                    {
                        Alert(team.Owner, "Backup ALM removed", profile.DisplayName + " was removed as a backup ALM from your team by " + _profile.DisplayName);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds a profile as a team backup ALM.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="profile"></param>
        public void AddTeamBackupALM(DTO.Team team, DTO.Profile profile)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage teams");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (profile == null || profile.Id == 0)
            {
                throw new Exception("Profile is not defined");
            }

            try
            {
                DTO.Profile found = team.Admins.Find(delegate(DTO.Profile n) { return n.Id == profile.Id; });

                if (found == null)
                {
                    team.Admins.Add(profile);

                    SaveTeam(team);

                    LogComment(team, profile.Username + " added as backup ALM for ALT " + team.Name);

                    if (team.Owner.Id != _profile.Id)
                    {
                        Alert(team.Owner, "Backup ALM added", profile.DisplayName + " was added as a backup ALM to your team by " + _profile.DisplayName);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds a team to an existing directorate.
        /// </summary>
        /// <param name="directorate"></param>
        /// <param name="team"></param>
        public void AddDirectorateTeam(DTO.Directorate directorate, DTO.Team team)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.DIRECTORATE_ADMIN))
            {
                throw new Exception("You do not have authorization to manage directorates");
            }

            if (directorate == null || directorate.Id == 0)
            {
                throw new Exception("Directorate is not defined");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (directorate.Teams.Find(delegate(DTO.Team n) { return n.Id == team.Id; }) != null)
            {
                throw new Exception("Team already exists in directorate");
            }

            try
            {
                directorate.Teams.Add(team);
                SaveDirectorate(directorate);
                LogComment(directorate, "Team " + team.Name + " added to Directorate " + directorate.Name);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Removes a team from a directorate by setting it inactive.
        /// </summary>
        /// <param name="directorate"></param>
        /// <param name="team"></param>
        public void RemoveDirectorateTeam(DTO.Directorate directorate, DTO.Team team)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.DIRECTORATE_ADMIN))
            {
                throw new Exception("You do not have authorization to manage directorates");
            }

            if (directorate == null || directorate.Id == 0)
            {
                throw new Exception("Directorate is not defined");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (directorate.Teams.Find(delegate(DTO.Team n) { return n.Id == team.Id; }) == null)
            {
                throw new Exception("Team does not exist in directorate");
            }

            try
            {
                team.Active = false;
                directorate.Teams.Remove(team);
                SaveTeam(team);
                LogComment(directorate, "Team " + team.Name + " removed from Directorate " + directorate.Name);

                Alert(team.Owner, "Team removed from directorate", "Your team was removed from the " + directorate.Name + " directorate");
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds a complexity code to a task type.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="taskType"></param>
        /// <param name="complexity"></param>
        public void AddTaskTypeComplexity(DTO.Team team, DTO.TaskType taskType, DTO.Complexity complexity)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage task types");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            if (taskType == null || taskType.Id == 0)
            {
                throw new Exception("Task Type is not defined");
            }

            if (complexity == null)
            {
                throw new Exception("Complexity to add must not be null");
            }

            try
            {
                SaveComplexityCode(team, taskType, complexity);
                //taskType.Complexities.Add(complexity);
                //SaveTaskType(team, taskType);

                LogComment(team, complexity.Title + " added to Task Type " + taskType.Title);

            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Applies a barrier code as application to a team.  The team will inherit all children.
        /// </summary>
        /// <param name="team"></param>
        public void ApplyBarriers(DTO.Team team)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage barriers");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            foreach (DTO.Barrier b in team.SelectedBarriers)
            {
                if (b == null || b.Id == 0)
                {
                    throw new Exception("Barrier is not defined");
                }
            }

            try
            {
                _dalMediator.GetAdminProcessor().ApplyBarriers(team);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Applies a task type as application to a team.  The team will inherit all children.
        /// </summary>
        /// <param name="team"></param>
        public void ApplyTaskTypes(DTO.Team team)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage task types");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            foreach (DTO.TaskType t in team.SelectedTaskTypes)
            {
                if (t == null || t.Id == 0)
                {
                    throw new Exception("Task type is not defined");
                }
            }

            try
            {
                _dalMediator.GetAdminProcessor().ApplyTaskTypes(team);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Applies a unplanned code as application to a team.  The team will inherit all children.
        /// </summary>
        /// <param name="team"></param>
        public void ApplyUnplannedCodes(DTO.Team team)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.TEAM_MANAGE))
            {
                throw new Exception("You do not have authorization to manage unplanned codes");
            }

            if (team == null || team.Id == 0)
            {
                throw new Exception("Team is not defined");
            }

            foreach (DTO.UnplannedCode c in team.SelectedUnplannedCodes)
            {
                if (c == null || c.Id == 0)
                {
                    throw new Exception("Unplanned code is not defined");
                }
            }

            try
            {
                _dalMediator.GetAdminProcessor().ApplyUnplannedCodes(team);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Team> GetTeamList()
        {
            return _dalMediator.GetAdminProcessor().GetTeamList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Team> GetAllTeams()
        {
            return _dalMediator.GetAdminProcessor().GetAllTeams();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetBarrierExtendedDescription(long team_id, long barrier_id)
        {
            if (team_id > 0 && barrier_id > 0)
            {
                try
                {
                    return _dalMediator.GetAdminProcessor().GetBarrierExtendedDescription(team_id, barrier_id);
                }
                catch (Exception e)
                {
                    LogError(e.ToString());
                    throw;
                }
            }
            else
            {
                throw new Exception("Team ID and Barrier ID are required to retrieve a barrier extended description.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SaveBarrierExtendedDescription(long team_id, long barrier_id, string description)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.DIRECTORATE_ADMIN))
            {
                throw new Exception("You do not have authorization to manage barriers");
            }

            if (team_id == 0)
            {
                throw new Exception("Team ID is not valid");
            }

            if (barrier_id == 0)
            {
                throw new Exception("Barrier ID is not defined");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveBarrierExtendedDescription(team_id, barrier_id, description);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUnplannedCodeExtendedDescription(long team_id, long unplanned_code_id)
        {
            if (team_id > 0 && unplanned_code_id > 0)
            {
                try
                {
                    return _dalMediator.GetAdminProcessor().GetUnplannedCodeExtendedDescription(team_id, unplanned_code_id);
                }
                catch (Exception e)
                {
                    LogError(e.ToString());
                    throw;
                }
            }
            else
            {
                throw new Exception("Team ID and Unplanned Code ID are required to retrieve an unplanned code extended description.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SaveUnplannedCodeExtendedDescription(long team_id, long unplanned_code_id, string description)
        {
            if (!ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.DIRECTORATE_ADMIN))
            {
                throw new Exception("You do not have authorization to manage unplanned codes");
            }

            if (team_id == 0)
            {
                throw new Exception("Team ID is not valid");
            }

            if (unplanned_code_id == 0)
            {
                throw new Exception("Unplanned Code ID is not defined");
            }

            try
            {
                _dalMediator.GetAdminProcessor().SaveUnplannedCodeExtendedDescription(team_id, unplanned_code_id, description);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSystemMessage()
        {
            return _dalMediator.GetAdminProcessor().GetSystemMessage();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SaveSystemMessage(string message)
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
            {
                _dalMediator.GetAdminProcessor().SaveSystemMessage(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public void SendSystemAlert(string subject, string message)
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE) &&
                subject != string.Empty && message != string.Empty)
            {
                _dalMediator.GetAdminProcessor().SendSystemAlert(subject, message, _profile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="directorateID"></param>
        /// <returns></returns>
        public List<DTO.TaskType> ImportTaskTypes(string path, long directorateID)
        {
            List<DTO.TaskType> taskTypes;

            try
            {
                taskTypes = _dalMediator.GetAdminProcessor().ImportTaskTypes(path, directorateID);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return taskTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypes"></param>
        /// <param name="directorateID"></param>
        public void SaveImportedTaskTypes(List<DTO.TaskType> taskTypes, long directorateID)
        {
            try
            {
                _dalMediator.GetAdminProcessor().SaveImportedTaskTypes(taskTypes, directorateID);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }
    
    }
}
