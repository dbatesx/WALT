using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Security.Principal;
using System.Diagnostics;
using System.Collections;
using System.Web.SessionState;

namespace WALT.BLL
{
    /// <summary>
    /// 
    /// </summary>
    abstract public class Manager
    {
        private static string _dalId = typeof(WALT.DAL.Mediator).ToString();
        private static Hashtable _sessionHash = new Hashtable();

        /// <summary>
        /// 
        /// </summary>
        protected WALT.DAL.Mediator _dalMediator;

        /// <summary>
        /// 
        /// </summary>
        protected WALT.DTO.Profile _profile;
        
        /// <summary>
        /// 
        /// </summary>
        protected Logger _logger;

        /// <summary>
        /// 
        /// </summary>
        protected Manager()
        {
            _dalMediator = (WALT.DAL.Mediator)GetSessionValue(_dalId);
            _logger = (Logger)GetSessionValue("_logger");
            _profile = (WALT.DTO.Profile)GetSessionValue("_profile");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AppInit()
        {
            WALT.DAL.Mediator m = new DAL.Mediator();
            m.Refresh();
            m.GetDatabase().Upgrade();
            m.GetAdminProcessor().Init();
            m.Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Shutdown(HttpSessionState session)
        {
            DAL.Mediator m = (DAL.Mediator)session[_dalId];

            if (m != null)
            {
                m.Shutdown();
            }

            session.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DTO.Profile GetProfile()
        {
            return _profile;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Init()
        {
            System.Security.Principal.IIdentity identity = (HttpContext.Current == null) ?
                WindowsIdentity.GetCurrent() : HttpContext.Current.User.Identity;

            DAL.Mediator mediator = (WALT.DAL.Mediator)GetSessionValue(_dalId);

            if (mediator == null)
            {
                mediator = new WALT.DAL.Mediator();
                SetSessionValue(_dalId, mediator);
            }

            mediator.Refresh();

            DTO.Profile profile = mediator.GetProfileProcessor().GetProfileByUsername(identity.Name, true, false);

            if (profile == null)
            {
                throw new Exception(identity.Name + " is not registered with this application");
            }
            else if (profile.Active == false)
            {
                throw new Exception(identity.Name + " is disabled");
            }
            else
            {
                SetSessionValue("_profile", profile);
            }

            Logger logger = (Logger)GetSessionValue("_logger");

            if (logger == null)
            {
                logger = new Logger(profile, mediator);
                SetSessionValue("_logger", logger);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static Object GetSessionValue(string key)
        {
            return HttpContext.Current != null ? HttpContext.Current.Session[key] : _sessionHash[key];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        protected static void SetSessionValue(string key, Object o)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Session[key] = o;
            }
            else
            {
                _sessionHash[key] = o;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void LogInfo(string item)
        {
            _logger.LogInfo(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void LogWarning(string item)
        {
            _logger.LogWarning(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void LogError(string item)
        {
            _logger.LogError(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comment"></param>
        public void LogComment(DTO.Object source, string comment)
        {
            _logger.LogComment(source, comment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="linkedType"></param>
        /// <param name="linkedID"></param>
        public void Alert(DTO.Profile profile, string subject, string message, DTO.Alert.AlertEnum? linkedType, long linkedID)
        {
            if (profile != null)
            {
                try
                {
                    DTO.Alert alert = new WALT.DTO.Alert();
                    alert.Message = message;
                    alert.Subject = subject;
                    alert.Profile = profile;
                    alert.Acknowledged = false;
                    alert.Creator = GetProfile();
                    alert.EntryDate = DateTime.Now;
                    alert.LinkedType = linkedType;
                    alert.LinkedId = linkedID;

                    _dalMediator.GetProfileProcessor().SaveAlert(alert);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public void Alert(DTO.Profile profile, string subject, string message)
        {
            Alert(profile, subject, message, null, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void SetProfile(DTO.Profile p)
        {
            _profile = p;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Done()
        {
            DAL.Mediator mediator = (WALT.DAL.Mediator)GetSessionValue(_dalId);

            if (mediator != null)
            {
                if (mediator.Done())
                {
                    SetSessionValue(_dalId, null);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetNumDatabaseConnections()
        {
            int connections = 0;

            DAL.Mediator mediator = (WALT.DAL.Mediator)GetSessionValue(_dalId);

            if (mediator != null)
            {
                connections = mediator.GetNumDatabaseConnections();
            }

            return connections;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        protected bool Compare(string s1, string s2)
        {
            string c1 = s1 == null ? "" : s1;
            string c2 = s2 == null ? "" : s2;
            return c1 == c2;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public static void ClearSession(HttpSessionState session)
        {
            BLL.Manager.Shutdown(session);
            session.Clear();
            session["Created"] = DateTime.Now;
        }
    }
}
