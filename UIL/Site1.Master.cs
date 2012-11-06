using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WALT.UIL
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        static string _buildDateTime = null;
        private bool _showMsg = false;

        protected void Page_Init(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["timeDiff"] == null)
            {
                if (IsPostBack)
                {
                    HttpContext.Current.Session["sessionMsg"] = true;
                }

                string url = HttpContext.Current.Request.RawUrl;

                if (url.Substring(url.Length - 1) == "?")
                {
                    url = url.Remove(url.Length - 1);
                }

                Response.Redirect("/login.aspx?src=" + url);
            }

            BLL.Manager.Init();
            BLL.ProfileManager.GetInstance().Refresh();
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            BLL.Manager.Done();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["sessionMsg"] != null)
            {
                DisplayErrorMessage("Your session has timed out. The requested action could not be completed.");
                HttpContext.Current.Session.Remove("sessionMsg");
            }

            HttpContext.Current.Session["master_page"] = this;
            HttpContext.Current.Session["PageLoad"] = DateTime.Now;

            Label1.Text = BLL.ProfileManager.GetInstance().GetProfile().DisplayName;
            MessagePanel.Visible = _showMsg;

            int interval = 0;
            string ping = ConfigurationManager.AppSettings["PingInterval"];

            if (Int32.TryParse(ping, out interval) && interval > 0)
            {
                pingFrame.Visible = true;
                pingFrame.Attributes.Add("src", "/ping.aspx?interval=" + ping);
            }
            else
            {
                pingFrame.Visible = false;
            }

            string test_mode = ConfigurationManager.AppSettings["TestMode"];

            Label1.Visible = test_mode == "0" ? true : false;
            DropDownList1.Visible = test_mode == "1" ? true : false;

            if (test_mode == "1")
            {
                if (!IsPostBack || DropDownList1.Items.Count == 0)
                {
                    if (Session["test_profile_list"] == null)
                    {
                        Session["test_profile_list"] = BLL.ProfileManager.GetInstance().GetProfileDisplayNameList();
                    }

                    try
                    {
                        DropDownList1.DataSource = Session["test_profile_list"];
                        DropDownList1.DataBind();
                    }
                    catch (Exception ex)
                    {
                        Utility.DisplayException(ex);
                    }

                    DropDownList1.Text = BLL.ProfileManager.GetInstance().GetProfile().DisplayName;
                }
            }

            int alerts = 0;

            if (!HttpContext.Current.Request.RawUrl.ToLower().Contains("default") &&
                HttpContext.Current.Request.RawUrl.ToLower().Contains(".aspx"))
            {
                alerts = BLL.ProfileManager.GetInstance().GetUnreadAlertCount();
            }

            if (alerts > 0)
            {
                lnkAlertMsg.Visible = true;
                lnkAlertMsg.InnerText = "You have " + alerts.ToString() + " unread alerts";

                if (HttpContext.Current.Request.RawUrl.Contains("weekly.aspx"))
                {
                    lnkAlertMsg.Attributes.Add("onclick", "return PrepareRedirect('/')");
                }
            }
            else
            {
                lnkAlertMsg.Visible = false;
            }

            if (HttpContext.Current.Request.RawUrl.Contains("weekly.aspx"))
            {
                lnkMain.Attributes.Add("onclick", "return PrepareRedirect('/')");
            }

            string message = BLL.AdminManager.GetInstance().GetSystemMessage();

            if (message != null && message.Length > 0)
            {
                Label5.Text = message;
                Panel1.Visible = true;
            }
            else
            {
                Panel1.Visible = false;
            }

            if (_buildDateTime == null)
            {
                string path = Server.MapPath(@"\bin\walt.uil.dll");
                DateTime dt = File.GetLastWriteTime(path);
                _buildDateTime = dt.ToString();
            }

            VersionLabel.Text = "Version " + Utility.GetVersion() + " " + _buildDateTime;

            if (HttpContext.Current.Request.UserAgent.Contains("MSIE 7") &&
                HttpContext.Current.Request.UserAgent.Contains("Trident"))
            {
                Menu1.Visible = false;
                compError.Visible = true;
                ContentPlaceHolder1.Visible = false;

                compError.Controls.Add(new LiteralControl("<br><br>" + HttpContext.Current.Request.UserAgent));
            }
            else
            {
                Menu1.Visible = true;
                compError.Visible = false;
                ContentPlaceHolder1.Visible = true;
            }
        }

        void DisplayMessage(string message, System.Drawing.Color background)
        {
            MessagePanel.BackColor = background;
            Label2.Text = message;
            MessagePanel.Visible = true;
            _showMsg = true;
        }

        public void DisplayErrorMessage(string message)
        {
            DisplayMessage(message, System.Drawing.Color.Pink);
        }

        internal void DisplayInfoMessage(string message)
        {
            DisplayMessage(message, System.Drawing.Color.LightGreen);
        }

        internal void DisplayWarningMessage(string message)
        {
            DisplayMessage(message, System.Drawing.Color.Yellow);
        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {            
            HttpContext.Current.Session.Clear();
            BLL.Manager.Init();

            DTO.Profile p = BLL.ProfileManager.GetInstance().GetProfileByDisplayName(DropDownList1.SelectedValue);

            if (p == null)
            {
                p = BLL.ProfileManager.GetInstance().GetProfile(DropDownList1.SelectedValue);
            }

            BLL.TaskManager.GetInstance().SetProfile(p);
            BLL.ReportManager.GetInstance().SetProfile(p);
            BLL.AdminManager.GetInstance().SetProfile(p);
            BLL.ProfileManager.GetInstance().SetProfile(p);
            BLL.ProfileManager.GetInstance().Refresh();

            Response.Redirect(Request.RawUrl);
        }

        protected void Menu1_MenuItemDataBound(object sender, MenuEventArgs e)
        {
            if ((e.Item.Text == "Admin" &&
                 (!BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.ROLE_MANAGE) &&
                  !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.METADATA_MANAGE) &&
                  !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.PROFILE_MANAGE) &&
                  !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) &&
                  !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TEAM_MANAGE) &&
                  !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TASK_MANAGE) &&
                  !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))) ||
                (e.Item.Text == "Roles" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.ROLE_MANAGE)) ||
                (e.Item.Text == "Programs" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.METADATA_MANAGE)) ||
                (e.Item.Text == "Profiles" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.PROFILE_MANAGE)) ||
                (e.Item.Text == "Directorates" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN)) ||
                (e.Item.Text == "Teams" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TEAM_MANAGE)) ||
                (e.Item.Text == "Tasks" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TASK_MANAGE)) ||
                (e.Item.Text == "Active Directory" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE)) ||
                (e.Item.Text == "Status" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE)) ||
                (e.Item.Text == "Reports" && !BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.REPORT_MANAGE)) ||
                (e.Item.Text == "Favorites" && BLL.ProfileManager.GetInstance().IsRogueUser()))
            {
                e.Item.Parent.ChildItems.Remove(e.Item);
            }
            else if (e.Item.Text == "Submit Ticket")
            {
                e.Item.NavigateUrl = ConfigurationManager.AppSettings["SubmitTicketURL"];
                e.Item.Target = "_blank";
            }
            else if (e.Item.Text == "Release Notes")
            {
                e.Item.NavigateUrl = ConfigurationManager.AppSettings["ReleaseNotesURL"];
                e.Item.Target = "_blank";
            }               
            else if (e.Item.Text == "FAQ")
            {
                e.Item.NavigateUrl = ConfigurationManager.AppSettings["FAQURL"];
                e.Item.Target = "_blank";
            }
        }
    }
}
