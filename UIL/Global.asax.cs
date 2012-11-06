using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Diagnostics;

namespace WALT.UIL
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            Application["Sessions"] = new Dictionary<string, HttpSessionState>();
            BLL.Manager.AppInit();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Application.Lock();
            Dictionary<string, HttpSessionState> sessions = (Dictionary<string, HttpSessionState>)Application["Sessions"];
            Session["Created"] = DateTime.Now;
            sessions[Session.SessionID] = Session;
            Application.UnLock();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                Session["LastErrorMessage"] = Server.GetLastError().GetBaseException().Message +
                    "\n\n" + Server.GetLastError().GetBaseException().StackTrace.ToString();
            }
            catch { }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            BLL.Manager.Shutdown(Session);

            Application.Lock();
            Dictionary<string, HttpSessionState> sessions = (Dictionary<string, HttpSessionState>)Application["Sessions"];
            sessions.Remove(Session.SessionID);
            Application.UnLock();
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}