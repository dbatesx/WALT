using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace WALT.BLL
{
    /// <summary>
    /// 
    /// </summary>
    public class ProfileManager : Manager
    {
        private static string _id = typeof(ProfileManager).ToString();
        private Dictionary<string, bool> _actions;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ProfileManager GetInstance()
        {
            ProfileManager m = (ProfileManager)GetSessionValue(_id);

            if (m == null)
            {
                m = new ProfileManager();
                SetSessionValue(_id, m);
            }

            return m;
        }

        /// <summary>
        /// 
        /// </summary>
        private ProfileManager()
            : base()
        {
            Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Refresh()
        {
            List<DTO.Action> actions =
                _dalMediator.GetProfileProcessor().GetProfileActions(_profile);

            /* Initialize all actions to false */

            _actions = new Dictionary<string, bool>();

            foreach (string name in Enum.GetNames(typeof(DTO.Action)))
            {
                _actions[name] = false;
            }

            /* Set actions from database roles to true for the profile */

            foreach (DTO.Action a in actions)
            {
                _actions[a.ToString()] = true;
            }
        }

        /// <summary>
        /// Get a list of the active profile usernames.
        /// </summary>
        /// <returns>List of profile usernames</returns>
        public List<DTO.Profile> GetProfileList()
        {
            List<DTO.Profile> profiles = new List<DTO.Profile>();

            try
            {
                profiles = _dalMediator.GetProfileProcessor().GetProfileList();
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetProfileDisplayNameList()
        {
            return _dalMediator.GetProfileProcessor().GetProfileDisplayNameList();
        }
        
        /// <summary>
        /// Get a list of the active profile usernames.
        /// </summary>
        /// <returns>List of profile usernames</returns>
        public List<DTO.Profile> GetProfileList(string filter)
        {
            List<DTO.Profile> profiles = new List<DTO.Profile>();

            try
            {
                profiles = _dalMediator.GetProfileProcessor().GetProfileList(filter);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profiles;
        }

        
        /// <summary>
        /// Get a Dictionary of the active profile usernames.
        /// </summary>
        /// <returns>Dictionary of profile usernames</returns>
        public Dictionary<long, string> GetProfileDictionary(string filter)
        {
            Dictionary<long, string> profiles = new Dictionary<long, string>();
           
            try
            {
                profiles = _dalMediator.GetProfileProcessor().GetProfileDictionary(filter);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profiles;
        }


        /// <summary>
        /// Get a Dictionary of the user's teammates.
        /// </summary>
        /// <returns>Dictionary of profile usernames</returns>
        public Dictionary<long, string> GetTeammateDictionary(long profileID)
        {
            Dictionary<long, string> profiles = new Dictionary<long, string>();
            
            try
            {
                profiles = _dalMediator.GetProfileProcessor().GetTeammateDictionary(profileID);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public List<DTO.Profile> GetProfileList(DTO.Action action)
        {
            return GetProfileList(action, true);
        }

        /// <summary>
        /// Get a list of active profile usernames.
        /// </summary>
        /// <returns>List of active profile usernames</returns>
        public List<DTO.Profile> GetProfileList(DTO.Action action, bool active)
        {
            List<DTO.Profile> profiles = new List<DTO.Profile>();

            try
            {
                profiles = _dalMediator.GetProfileProcessor().GetProfileList(action, active);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profiles;
        }

        /// <summary>
        /// Get a profile object based upon the username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>A Profile DTO based upon the given username</returns>
        public DTO.Profile GetProfile(string username)
        {
            return GetProfile(username, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public DTO.Profile GetProfile(string username, bool expand)
        {
            if (username.Length == 0)
            {
                throw new Exception("Username is blank");
            }

            DTO.Profile profile = new WALT.DTO.Profile();

            try
            {
                profile = _dalMediator.GetProfileProcessor().GetProfile(username, expand);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IsAllowed(WALT.DTO.Action action)
        {
            return _actions[DTO.Action.SYSTEM_MANAGE.ToString()] || _actions[action.ToString()];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorateID"></param>
        /// <returns></returns>
        public bool IsDirectorateAdmin(long directorateID)
        {
            if (_actions[DTO.Action.SYSTEM_MANAGE.ToString()])
            {
                return true;
            }

            if (_actions[DTO.Action.DIRECTORATE_ADMIN.ToString()])
            {
                return _dalMediator.GetProfileProcessor().IsDirectorateAdmin(directorateID, _profile);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsRogueUser()
        {
            return _dalMediator.GetProfileProcessor().IsRogueUser(_profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alert"></param>
        public void AcknowledgeAlert(DTO.Alert alert)
        {
            try
            {
                alert.Acknowledged = true;
                _dalMediator.GetProfileProcessor().SaveAlert(alert);
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
        /// <param name="id"></param>
        public void AcknowledgeAlert(long id)
        {
            try
            {
                _dalMediator.GetProfileProcessor().AcknowledgeAlert(id);
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
        /// <param name="alertIDs"></param>
        public void AcknowledgeAlerts(List<long> alertIDs)
        {
            try
            {
                _dalMediator.GetProfileProcessor().AcknowledgeAlerts(alertIDs);
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
        /// <param name="id"></param>
        /// <returns></returns>
        public DTO.Alert GetAlert(long id)
        {
            DTO.Alert alert = null;

            try
            {
                alert = _dalMediator.GetProfileProcessor().GetAlert(id);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return alert;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Alert> GetAlertList(bool acknowledged)
        {
            List<DTO.Alert> alerts = new List<WALT.DTO.Alert>();

            try
            {
                alerts = _dalMediator.GetProfileProcessor().GetAlertList(_profile, acknowledged);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acknowledged"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<DTO.Alert> GetAlertList(bool acknowledged,
            DTO.Alert.ColumnEnum? sort, bool order, int start, int size, ref int count,
            Dictionary<DTO.Alert.ColumnEnum, string> filters)
        {
            List<DTO.Alert> alerts = new List<WALT.DTO.Alert>();

            try
            {
                alerts = _dalMediator.GetProfileProcessor().GetAlertList(_profile, acknowledged, sort, order, start, size, ref count, filters);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Alert> GetAlertListByCreator()
        {
            List<DTO.Alert> alerts = new List<WALT.DTO.Alert>();

            try
            {
                alerts = _dalMediator.GetProfileProcessor().GetAlertListByCreator(_profile);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<DTO.Alert> GetAlertListByCreator(DTO.Alert.ColumnEnum? sort, bool order,
            int start, int size, ref int count,
            Dictionary<DTO.Alert.ColumnEnum, string> filters)
        {
            List<DTO.Alert> alerts = new List<WALT.DTO.Alert>();

            try
            {
                alerts = _dalMediator.GetProfileProcessor().GetAlertListByCreator(_profile, sort, order, start, size, ref count, filters);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return alerts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Profile> GetAlertCreators()
        {
            return _dalMediator.GetProfileProcessor().GetAlertCreators(_profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DTO.Profile> GetAlertProfiles()
        {
            return _dalMediator.GetProfileProcessor().GetAlertProfiles(_profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetUnreadAlertCount()
        {
            return _dalMediator.GetProfileProcessor().GetUnreadAlertCount(_profile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public DTO.ADEntry GetADEntry(DTO.Profile profile)
        {
            DTO.ADEntry entry = null;

            try
            {
                entry = _dalMediator.GetProfileProcessor().GetADEntry(profile);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return entry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<DTO.ADEntry> GetADEntryList(string field, string value)
        {
            List<DTO.ADEntry> entries = new List<DTO.ADEntry>();

            try
            {
                entries = _dalMediator.GetProfileProcessor().GetADEntryList(field, value);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw;
            }

            return entries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DTO.Profile GetProfile(long id)
        {
            return GetProfile(id, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public DTO.Profile GetProfile(long id, bool expand)
        {
            DTO.Profile p = null;

            try
            {
                p = _dalMediator.GetProfileProcessor().GetProfile(id, expand, true);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alert"></param>
        public void SaveAlert(DTO.Alert alert)
        {
            alert.Subject = alert.Subject.Trim();
            alert.Message = alert.Message.Trim();

            try
            {
                _dalMediator.GetProfileProcessor().SaveAlert(alert);
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
        /// <param name="alert"></param>
        /// <param name="sendTo"></param>
        public void SendAlert(DTO.Alert alert, List<DTO.Profile> sendTo)
        {
            alert.Subject = alert.Subject.Trim();
            alert.Message = alert.Message.Trim();

            try
            {
                _dalMediator.GetProfileProcessor().SendAlert(alert, sendTo);
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
        /// <param name="alert"></param>
        public void ValidateAlert(DTO.Alert alert)
        {
            string errors = string.Empty;

            if (alert.Subject.Length == 0)
            {
                errors += "Alert subject is blank<br>";
            }

            if (alert.Message.Length == 0)
            {
                errors += "Alert message is blank<br>";
            }

            if (errors.Length > 0)
            {
                throw new Exception(errors);
            }
        }

        /// <summary>
        /// This method gets all the records in active directory that are associated with
        /// a directorate org code and makes sure there is a record in the WALT database.
        /// If the record exists, then it's information is updated.  The employee id is 
        /// the key field, as the username may change.
        /// </summary>
        public void SyncAD()
        {
            List<DTO.Directorate> directorates = BLL.AdminManager.GetInstance().GetDirectorateList();
            Dictionary<DTO.Profile, Boolean> processed = new Dictionary<DTO.Profile, Boolean>();

            foreach (DTO.Directorate d in directorates)
            {
                foreach (string code in d.OrgCodes)
                {
                    Debug.WriteLine(d.Name + ": " + code);

                    List<DTO.ADEntry> items = _dalMediator.GetProfileProcessor().GetADEntryList(
                        "department", code + "*");

                    foreach (DTO.ADEntry entry in items)
                    {
                        Debug.WriteLine("  - " + entry.DisplayName);

                        DTO.Profile p = _dalMediator.GetProfileProcessor().GetProfileByEmployeeID(
                            entry.EmployeeID);

                        if (p == null)
                        {
                            p = _dalMediator.GetProfileProcessor().GetProfileByUsername(
                                entry.Domain + "\\" + entry.Username);

                            if (p == null)
                            {
                                p = new DTO.Profile();
                            }
                        }

                        MapADEntryToProfile(p, entry);
                        _dalMediator.GetProfileProcessor().SaveProfile(p);
                        processed[p] = true;
                    }
                }
            }

            /* 
             * Update any other profiles that exist in the database that are not mapped to 
             * organiztion codes
             */

            List<DTO.Profile> profiles = GetProfileList();

            foreach (DTO.Profile p in profiles)
            {
                if (processed.ContainsKey(p) && processed[p] != true)
                {
                    DTO.ADEntry entry = _dalMediator.GetProfileProcessor().GetADEntry(p);

                    if (entry != null)
                    {
                        MapADEntryToProfile(p, entry);
                        _dalMediator.GetProfileProcessor().SaveProfile(p);
                    }
                }
            }
        }

        void MapADEntryToProfile(DTO.Profile p, DTO.ADEntry e)
        {
            p.DisplayName = e.DisplayName;
            p.EmployeeID = e.EmployeeID;
            p.OrgCode = e.OrgCode;
            p.Manager = _dalMediator.GetProfileProcessor().GetProfileByDisplayName(e.Manager, false);
            p.Username = e.Domain + "\\" + e.Username;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SavePreference(string key, string value)
        {
            if (key != null && value != null && key.Length > 0)
            {
                _profile.Preferences[key] = value;
                SaveProfile(_profile);
            }
        }

        /// <summary>
        /// Save a profile to the database.
        /// </summary>
        /// <param name="profile"></param>
        public void SaveProfile(DTO.Profile profile)
        {
            if (profile == null)
            {
                throw new Exception("Profile is not defined");
            }

            if (profile.Username.Length == 0)
            {
                throw new Exception("Username is blank");
            }

            if (profile.Username.Contains("\\") == false)
            {
                throw new Exception("Username must contain domain");
            }

            // You can save your own profile.  Mostly need for preferences.

            if (profile.Username != _profile.Username && 
                !ProfileManager.GetInstance().IsAllowed(WALT.DTO.Action.PROFILE_MANAGE))
            {
                throw new Exception("You do not have authorization to manage profiles");
            }

            try
            {
                if (ConfigurationManager.AppSettings["SyncADOnProfileSave"] == "1")
                {
                    DTO.ADEntry e = _dalMediator.GetProfileProcessor().GetADEntry(profile);

                    if (e == null)
                    {
                        throw new Exception("Username was not found in Active Directory");
                    }

                    MapADEntryToProfile(profile, e);
                }

                _dalMediator.GetProfileProcessor().SaveProfile(profile);

                LogComment(profile, "Profile " + profile.Username + " saved");
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
        public DTO.Profile AddProfileByADLookup(string search)
        {
            try
            {
                return _dalMediator.GetProfileProcessor().AddProfileByADLookup(search);
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
        /// <param name="search"></param>
        /// <returns></returns>
        public DTO.Profile GetProfileByADLookup(string search)
        {
            return _dalMediator.GetProfileProcessor().GetProfileByADLookup(search);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public WALT.DTO.Profile GetProfileByDisplayName(string p)
        {
            return GetProfileByDisplayName(p, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        public WALT.DTO.Profile GetProfileByDisplayName(string p, bool expand)
        {
            return _dalMediator.GetProfileProcessor().GetProfileByDisplayName(p, expand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        internal DTO.Profile GetProfileByUsername(string username)
        {
            return _dalMediator.GetProfileProcessor().GetProfileByUsername(username);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileIDs"></param>
        /// <param name="exempt"></param>
        public void SetProfilesPlanExempt(List<long> profileIDs, bool exempt)
        {
            _dalMediator.GetProfileProcessor().SetProfilesPlanExempt(profileIDs, exempt);
        }
    }
}
 