using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Management;
using System.Diagnostics;
using System.Web.SessionState;
using System.Data;

namespace WALT.UIL
{
    public partial class Status : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
            {
                Response.Redirect("/");
            }
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        void LoadData()
        {
            Dictionary<string, HttpSessionState> sessions = (Dictionary<string, HttpSessionState>)Application["Sessions"];

            Label1.Text = Convert.ToString(sessions.Count);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ID"));
            dt.Columns.Add(new DataColumn("Created"));
            dt.Columns.Add(new DataColumn("PageLoad"));
            dt.Columns.Add(new DataColumn("ObjectCount"));
            dt.Columns.Add(new DataColumn("Profile"));

            List<string> keys = sessions.Keys.OrderByDescending(x => sessions[x]["PageLoad"]).ToList();

            foreach (string key in keys)
            {
                DataRow row = dt.NewRow();
                DTO.Profile p = (DTO.Profile)sessions[key]["_profile"];
                row["ID"] = sessions[key].SessionID;
                row["Created"] = sessions[key]["Created"];
                row["PageLoad"] = sessions[key]["PageLoad"];
                row["ObjectCount"] = sessions[key].Keys.Count;
                row["Profile"] = p == null ? "Unknown" : p.DisplayName;
                dt.Rows.Add(row);
            }

            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            Dictionary<string, HttpSessionState> sessions = (Dictionary<string, HttpSessionState>)Application["Sessions"];
            string id = e.Values[0].ToString();
            BLL.Manager.ClearSession(sessions[id]);
            LoadData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button1_Click(object sender, EventArgs e)
        {
            BLL.AdminManager.GetInstance().SaveSystemMessage(TextBox1.Text);
            Utility.DisplayInfoMessage("System message saved");
        }

        protected void btnSendSysAlert_Click(object sender, EventArgs e)
        {
            if (txtSysAlertSub.Text != string.Empty && txtSysAlertMsg.Text != string.Empty)
            {
                BLL.AdminManager.GetInstance().SendSystemAlert(txtSysAlertSub.Text, txtSysAlertMsg.Text);
                Utility.DisplayInfoMessage("System alert with subject \"" + txtSysAlertSub.Text + "\" sent.");
                txtSysAlertSub.Text = string.Empty;
                txtSysAlertMsg.Text = string.Empty;
            }
        }
    }
}
