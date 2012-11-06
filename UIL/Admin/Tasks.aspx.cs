using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace WALT.UIL.Admin
{
    public partial class Tasks : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            List<string> directorateNameList = BLL.AdminManager.GetInstance().GetDirectorateNameList();
            directorateNameList.Insert(0, "ALL");

            lblTeam.Visible = false;
            ddlTeam.Visible = false;

            ddlDirectorate.DataSource = directorateNameList;
            ddlDirectorate.DataBind();

            DTO.Directorate directorate = null;
            DTO.Team team = null;

            if (HttpContext.Current.Session["adminTasks_directorate"] != null)
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["adminTasks_directorate"];

                if (HttpContext.Current.Session["adminTasks_refresh"] != null)
                {
                    directorate = BLL.AdminManager.GetInstance().GetDirectorate(directorate.Name, true);
                    HttpContext.Current.Session.Remove("adminTasks_refresh");
                }

                ddlDirectorate.SelectedValue = directorate.Name;
                lblTeam.Visible = true;
                ddlTeam.Visible = true;
                PopulateDDLTeam(directorate.Teams);

                if (HttpContext.Current.Session["adminTasks_team"] != null)
                {
                    long teamID = (long)HttpContext.Current.Session["adminTasks_team"];
                    team = directorate.Teams.Single(x => x.Id == teamID);
                    ddlTeam.SelectedValue = team.Id.ToString();
                }
            }

            controlTaskBarriers.PopulateBarriers(directorate, team);
            controlTaskUnplannedCodes.PopulateUnplanned(directorate, team);
            controlTaskTypes.PopulateTaskTypes(directorate, team, null);

            if (HttpContext.Current.Session["adminTasks_tab"] != null)
            {
                TabContainer1.ActiveTab = TabContainer1.Tabs[(int)HttpContext.Current.Session["adminTasks_tab"]];
            }
        }

        protected void ddlDirectorate_SelectedIndexChanged(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;
            ddlTeam.Items.Clear();

            if (ddlDirectorate.SelectedValue == "ALL")
            {
                lblTeam.Visible = false;
                ddlTeam.Visible = false;
            }
            else
            {
                directorate = BLL.AdminManager.GetInstance().GetDirectorate(ddlDirectorate.SelectedValue, true);

                lblTeam.Visible = true;
                ddlTeam.Visible = true;

                PopulateDDLTeam(directorate.Teams);
            }

            HttpContext.Current.Session["adminTasks_directorate"] = directorate;

            controlTaskBarriers.PopulateBarriers(directorate, null);
            controlTaskUnplannedCodes.PopulateUnplanned(directorate, null);
            controlTaskTypes.PopulateTaskTypes(directorate, null, null);
        }

        private void PopulateDDLTeam(List<DTO.Team> teams)
        {
            foreach (DTO.Team team in teams)
            {
                ListItem item = new ListItem();
                item.Text = team.Name;
                item.Value = team.Id.ToString();

                ddlTeam.Items.Add(item);
            }

            ListItem allItem = new ListItem();
            allItem.Text = "ALL";
            allItem.Value = "ALL";

            if (!ddlTeam.Items.Contains(allItem))
            {
                ddlTeam.Items.Insert(0, "ALL");
            }
        }

        protected void ddlTeam_SelectedIndexChanged(object sender, EventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["adminTasks_directorate"];
            DTO.Team team = null;

            if (ddlTeam.SelectedValue != "ALL")
            {
                team = directorate.Teams.Single(x => x.Id == long.Parse(ddlTeam.SelectedValue));
                HttpContext.Current.Session["adminTasks_team"] = team.Id;
            }
            else
            {
                HttpContext.Current.Session.Remove("adminTasks_team");
            }
            
            controlTaskBarriers.PopulateBarriers(directorate, team);
            controlTaskUnplannedCodes.PopulateUnplanned(directorate, team);
            controlTaskTypes.PopulateTaskTypes(directorate, team, null);
        }

        protected void TabContainer1_ActiveTabChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < TabContainer1.Tabs.Count; i++)
            {
                if (TabContainer1.Tabs[i].ID == TabContainer1.ActiveTab.ID)
                {
                    if (i == 0)
                    {
                        HttpContext.Current.Session.Remove("adminTasks_tab");
                    }
                    else
                    {
                        HttpContext.Current.Session["adminTasks_tab"] = i;
                    }

                    return;
                }
            }
        }
    }
}
