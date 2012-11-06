using System;
using System.Collections.Generic;
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

namespace WALT.UIL.Admin
{
    public partial class Teams : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {           
            if (!IsPostBack)
            {
                LoadData();
            }
        }
        
        private void LoadData()
        {
            ddlDirectorate.DataSource = BLL.AdminManager.GetInstance().GetDirectorateNameList();
            ddlDirectorate.DataBind();

            ddlDirectorate.Items.Insert(0, string.Empty);
            ddlDirectorate.Items[0].Value = null;
        }

        private List<DTO.Profile> ProfileListFromSelections(ListBox listBox)
        {
            List<DTO.Profile> list = new List<DTO.Profile>();

            foreach (ListItem item in listBox.Items)
            {
                if (item.Selected)
                {
                    DTO.Profile profile = BLL.ProfileManager.GetInstance().GetProfile(item.Value);

                    if (profile != null)
                        list.Add(profile);
                }
            }

            return list;
        }

        protected void ddlDirectorate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearTeamInfoControls();
            panelTeamData.Enabled = false;

            if (ddlDirectorate.SelectedValue != string.Empty)
            {
                DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(ddlDirectorate.SelectedValue, true);

                HttpContext.Current.Session["admin_teams_directorate"] = directorate;

                List<DTO.Team> teams = directorate.Teams;
                gridTeams.DataSource = teams;
                gridTeams.DataBind();

                HttpContext.Current.Session["admin_teams"] = teams;
            }
            else
            {
                gridTeams.DataSource = null;
                gridTeams.DataBind();

                HttpContext.Current.Session["admin_teams"] = null;
            }
        }

        private void ClearTeamInfoControls()
        {
            tbEditTeamName.Text = string.Empty;
            txtALManagerRO.Text = string.Empty;
            ddlManager.Items.Clear();
            cbActive.Checked = false;
            rbComplexityBased.Checked = false;
            rbHandEntered.Checked = false;
            lbBackups.Items.Clear();
            lbMembers.Items.Clear();
        }

        protected void ddlManager_SelectedIndexChanged(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                string directorateName = ddlDirectorate.SelectedValue;

                try
                {
                    DTO.Profile teamOwner = BLL.ProfileManager.GetInstance().GetProfile(ddlManager.SelectedValue);

                    team.Owner = teamOwner;

                    BLL.AdminManager.GetInstance().SaveTeam(directorateName, team);
                    Utility.DisplayInfoMessage("Team saved.");
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }

        }

        protected void gridTeams_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

        }

        protected void gridTeams_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {

        }

        protected void gridTeams_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {

        }

        protected void gridTeams_RowEditing(object sender, GridViewEditEventArgs e)
        {

        }

        protected void gridTeams_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {   
            gridTeams.PageIndex = e.NewPageIndex;
            gridTeams.DataSource = (List<DTO.Team>)HttpContext.Current.Session["admin_teams"];
            gridTeams.DataBind();
            
        }

        protected void btnSaveBackups_Click(object sender, EventArgs e)
        {   
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                try
                {
                    DTO.Profile profile;

                    for (int i = 0; i < profileSelectorBackups.ProfilesChosen.Count; i++)
                    {

                        profile = profileSelectorBackups.ProfilesChosen[i];
                        BLL.AdminManager.GetInstance().AddTeamBackupALM(team, profile);
                        //team.Admins.Add(profile);
                    }

                    // update the backups list display
                    lbBackups.DataSource = team.Admins;
                    lbBackups.DataTextField = "DisplayName";
                    lbBackups.DataValueField = "Username";
                    lbBackups.DataBind();

                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
                finally
                {
                    profileSelectorBackups.Clear();
                }
            }
        }

        protected void btnAddBackupCancel_Click(object sender, EventArgs e)
        {
            profileSelectorBackups.Clear();
            modalPopupExtenderAddBackups.Hide();
        }

        protected void btnRemoveBackups_Click(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                try
                {
                    foreach (ListItem item in lbBackups.Items)
                    {
                        if (item.Selected)
                        {
                            DTO.Profile found = team.Admins.Find(delegate(DTO.Profile p) { return p.Username == item.Value; });

                            if (found != null)
                            {
                                BLL.AdminManager.GetInstance().RemoveTeamBackupALM(team, found);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                // update the backups list display
                lbBackups.DataSource = team.Admins;
                lbBackups.DataTextField = "DisplayName";
                lbBackups.DataValueField = "Username";
                lbBackups.DataBind();
            }
        }

        
        protected void btnSaveMembers_Click(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                try
                {
                    DTO.Profile profile;

                    for (int i = 0; i < profileSelectorMembers.ProfilesChosen.Count; i++)
                    {
                        profile = profileSelectorMembers.ProfilesChosen[i];
                        //team.Admins.Add(profile);

                        BLL.AdminManager.GetInstance().AddTeamMember(team, profile);
                    }

                    // update the members list display
                    lbMembers.DataSource = team.Members;
                    lbMembers.DataTextField = "DisplayName";
                    lbMembers.DataValueField = "Username";
                    lbMembers.DataBind();

                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                finally
                {
                    profileSelectorMembers.Clear();
                }
            }
        }

        protected void btnAddMemberCancel_Click(object sender, EventArgs e)
        {
            profileSelectorMembers.Clear();
            modalPopupExtenderAddMembers.Hide();
        }

        protected void btnRemoveMembers_Click(object sender, EventArgs e)
        {          
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);
                try
                {
                    foreach (int index in lbMembers.GetSelectedIndices())
                    {
                        DTO.Profile found = team.Members.Find(p => p.Username == lbMembers.Items[index].Value.ToString());

                        if (found != null)
                        {
                            BLL.AdminManager.GetInstance().RemoveTeamMember(team, found);
                            team.Members.Remove(found);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                // update the backups list display
                lbMembers.DataSource = team.Members;
                lbMembers.DataTextField = "DisplayName";
                lbMembers.DataValueField = "Username";
                lbMembers.DataBind();
            }
        }

        protected void gridTeams_SelectedIndexChanged(object sender, EventArgs e)
        {            
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["admin_teams_directorate"];

                ClearTeamInfoControls();

                panelTeamData.Enabled = true;

                if (CanEdit(team, directorate))
                {
                    panelAddBackupButton.Visible = true;
                    panelAddMemberButton.Visible = true;
                    btnSaveTeamName.Visible = true;
                    tbEditTeamName.ReadOnly = false;
                    ddlManager.Visible = true;
                    txtALManagerRO.Visible = false;
                }
                else
                {
                    panelAddBackupButton.Visible = false;
                    panelAddMemberButton.Visible = false;
                    btnSaveTeamName.Visible = false;
                    tbEditTeamName.ReadOnly = true;
                    ddlManager.Visible = false;
                    txtALManagerRO.Visible = true;
                }

                lbBackups.DataSource = team.Admins;
                lbBackups.DataTextField = "DisplayName";
                lbBackups.DataValueField = "Username";
                lbBackups.DataBind();

                lbMembers.DataSource = team.Members;
                lbMembers.DataTextField = "DisplayName";
                lbMembers.DataValueField = "Username";
                lbMembers.DataBind();

                tbEditTeamName.Text = team.Name;

                List<DTO.Profile> possibleManagers = BLL.ProfileManager.GetInstance().GetProfileList(DTO.Action.TEAM_MANAGE, true);

                if (team.Owner != null && !possibleManagers.Exists(x => x.Username == team.Owner.Username))
                {
                    possibleManagers.Add(team.Owner);
                    possibleManagers.Sort(delegate(DTO.Profile p1, DTO.Profile p2) { return p1.Username.CompareTo(p2.Username); });
                }

                if (possibleManagers.Count == 0)
                {
                    ddlManager.Items.Insert(0, string.Empty);
                    ddlManager.Items[0].Value = null;
                }
                else
                {
                    ddlManager.DataSource = possibleManagers;
                    ddlManager.DataTextField = "DisplayName";
                    ddlManager.DataValueField = "Username";
                    ddlManager.DataBind();
                    ddlManager.Items.Insert(0, string.Empty);
                    ddlManager.Items[0].Value = null;

                    if (team.Owner != null)
                        ddlManager.SelectedValue = team.Owner.Username;
                }

                if (team.Owner != null)
                    txtALManagerRO.Text = team.Owner.DisplayName;

                cbActive.Checked = team.Active;

                if (team.ComplexityBased)
                {
                    rbComplexityBased.Checked = true;
                    rbHandEntered.Checked = false;
                }
                else
                {
                    rbComplexityBased.Checked = false;
                    rbHandEntered.Checked = true;
                }
            }
        }

        protected void btnSaveTeamName_Click(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                if (tbEditTeamName.Text.Length > 0)
                {
                    DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);
                    string directorateName = ddlDirectorate.SelectedValue;

                    try
                    {
                        team.Name = tbEditTeamName.Text;
                        BLL.AdminManager.GetInstance().SaveTeam(directorateName, team);
                        Utility.DisplayInfoMessage("Team saved.");
                    }
                    catch (Exception ex)
                    {
                        Utility.DisplayException(ex);
                    }
                }
            }
        }

        protected void cbActive_CheckedChanged(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {

                DTO.Profile currentUser = BLL.ProfileManager.GetInstance().GetProfile();
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);
                DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["admin_teams_directorate"];

                if (CanEdit(team, directorate))
                {
                    if (gridTeams.SelectedIndex > -1)
                    {

                        string directorateName = ddlDirectorate.SelectedValue;

                        try
                        {
                            team.Active = cbActive.Checked;
                            BLL.AdminManager.GetInstance().SaveTeam(directorateName, team);
                            Utility.DisplayInfoMessage("Team saved.");
                        }
                        catch (Exception ex)
                        {
                            Utility.DisplayException(ex);
                        }

                    }
                }
            }
        }

        protected void rbComplexityBased_CheckedChanged(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["admin_teams_directorate"];

                if (CanEdit(team, directorate))
                {
                    if (gridTeams.SelectedIndex > -1)
                    {

                        string directorateName = ddlDirectorate.SelectedValue;

                        try
                        {
                            team.ComplexityBased = rbComplexityBased.Checked;
                            BLL.AdminManager.GetInstance().SaveTeam(directorateName, team);
                            Utility.DisplayInfoMessage("Team saved.");
                        }
                        catch (Exception ex)
                        {
                            Utility.DisplayException(ex);
                        }
                    }
                }
            }
        }

        protected void rbHandEntered_CheckedChanged(object sender, EventArgs e)
        {
            HiddenField hiddenID = gridTeams.SelectedRow.Cells[1].FindControl("hfdTeamId") as HiddenField;
            long teamID = 0;

            if (hiddenID != null && !string.IsNullOrEmpty(hiddenID.Value) && long.TryParse(hiddenID.Value, out teamID))
            {
                DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(teamID, true);

                DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["admin_teams_directorate"];

                if (CanEdit(team, directorate))
                {
                    if (gridTeams.SelectedIndex > -1)
                    {

                        string directorateName = ddlDirectorate.SelectedValue;

                        try
                        {
                            team.ComplexityBased = rbComplexityBased.Checked;
                            BLL.AdminManager.GetInstance().SaveTeam(directorateName, team);
                            Utility.DisplayInfoMessage("Team saved.");
                        }
                        catch (Exception ex)
                        {
                            Utility.DisplayException(ex);
                        }
                    }
                }
            }
        }

        private bool CanEdit(DTO.Team team, DTO.Directorate directorate)
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE) ||
                (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TEAM_MANAGE) &&
                 BLL.AdminManager.GetInstance().IsTeamAdmin(team.Id)) ||
                (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) &&
                 BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate)))
            {
                return true;
            }

            return false;
        }

        protected string GetId(object obj)
        {
            string val = string.Empty;

            DTO.Team teamItem = (DTO.Team)obj;
            val = teamItem.Id.ToString();

            return val;
        }
    }
}
