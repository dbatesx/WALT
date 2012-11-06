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
    public partial class Directorates : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
                btnRemoveOrgCode.Attributes.Add("onclick", "return window.confirm('Are you sure?');");
                btnRemoveAdmins.Attributes.Add("onclick", "return window.confirm('Are you sure?');");
                btnRemoveManagers.Attributes.Add("onclick", "return window.confirm('Are you sure?');");
            }

            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN))
            {
                panelDirectoratesButtons.Visible = true;

                if (lstDirectorates.SelectedIndex >= 0)
                    ShowButtonsToDA();
            }
            else
            {
                panelDirectoratesButtons.Visible = false;
                panelAdministratorsButtons.Visible = false;
                panelManagersButtons.Visible = false;
                panelAddTeamButton.Visible = false;
                panelOrgCodeButtons.Visible = false;
            }

            Page.SetFocus(btnDoNothingDirectorates);
        }

        void LoadData()
        {
            lstDirectorates.DataSource = BLL.AdminManager.GetInstance().GetDirectorateNameList();
            lstDirectorates.DataBind();
        }

        private void ShowButtonsToDA()
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["directorates_directorate"];

            if (directorate != null
                && BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
            {
                panelDirectoratesButtons.Visible = true;
                panelAdministratorsButtons.Visible = true;
                panelManagersButtons.Visible = true;
                panelAddTeamButton.Visible = true;
                panelOrgCodeButtons.Visible = true;
            }
        }

        protected void lstDirectorates_SelectedIndexChanged(object sender, EventArgs e)
        {
            // get the selected directorate's information
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);
            HttpContext.Current.Session["directorates_directorate"] = directorate;

            if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
            {
                panelEditDirectorateButton.Visible = true;
                panelAdministratorsButtons.Visible = true;
                panelManagersButtons.Visible = true;
                panelAddTeamButton.Visible = true;
                panelOrgCodeButtons.Visible = true;
                phMemberBtns.Visible = true;
            }
            else
            {
                panelEditDirectorateButton.Visible = false;
                panelAdministratorsButtons.Visible = false;
                panelManagersButtons.Visible = false;
                panelAddTeamButton.Visible = false;
                panelOrgCodeButtons.Visible = false;
                phMemberBtns.Visible = false;
            }

            if (directorate != null)
            {
                // fill out the org codes list
                lstOrgCodes.DataSource = directorate.OrgCodes;
                lstOrgCodes.DataBind();

                // fill out the members list
                LoadMembers(directorate);

                // fill out the administrators list
                lstAdmins.DataSource = directorate.Admins;
                lstAdmins.DataTextField = "DisplayName";
                lstAdmins.DataValueField = "Username";
                lstAdmins.DataBind();

                // fill out the managers list
                lstManagers.DataSource = directorate.Managers;
                lstManagers.DataTextField = "DisplayName";
                lstManagers.DataValueField = "Username";
                lstManagers.DataBind();

                // fill out the teams list
                lstTeams.DataSource = directorate.Teams;
                lstTeams.DataTextField = "Name";
                lstTeams.DataValueField = "Name";
                lstTeams.DataBind();

                ddlTeamManagerToAdd.DataSource = BLL.ProfileManager.GetInstance().GetProfileList(DTO.Action.TEAM_MANAGE);
                ddlTeamManagerToAdd.DataTextField = "DisplayName";
                ddlTeamManagerToAdd.DataValueField = "Username";
                ddlTeamManagerToAdd.DataBind();

                // fill out the "edit directorate" info
                txtEditDirectorateName.Text = directorate.Name;
                //txtEditDirectorateOrgCode.Text = directorate.OrgCode;
            }

        }

        private void LoadMembers(DTO.Directorate directorate)
        {
            if (directorate == null)
            {
                if (lstDirectorates.SelectedValue != string.Empty)
                {
                    directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);
                }
                else
                {
                    return;
                }
            }

            List<DTO.Profile> memberList = BLL.AdminManager.GetInstance().GetDirectorateMembersByOrgCodes(directorate, false);
            lstMembers.DataSource = memberList;
            lstMembers.DataTextField = "DisplayName";
            lstMembers.DataValueField = "ID";
            lstMembers.DataBind();

            for (int i = 0; i < memberList.Count; i++)
            {
                DTO.Profile member = memberList[i];

                // Check if the member is in any ALTs. If not, give them a "*".
                if (BLL.AdminManager.GetInstance().GetTeam(member) == null)
                {
                    lstMembers.Items[i].Text = "* " + lstMembers.Items[i].Text;
                }
                else
                {
                    lstMembers.Items[i].Text = "\xA0\xA0" + lstMembers.Items[i].Text;
                }

                lstMembers.Items[i].Text = (member.ExemptPlan ? "X" : "\xA0\xA0") + lstMembers.Items[i].Text;
            }
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

        private List<DTO.Profile> ProfileListFromListBox(ListBox listBox)
        {
            List<DTO.Profile> list = new List<DTO.Profile>();

            foreach (ListItem item in listBox.Items)
            {
                DTO.Profile profile = BLL.ProfileManager.GetInstance().GetProfile(item.Value);

                if (profile != null)
                    list.Add(profile);
            }

            return list;
        }

        protected void lstOrgCodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemoveOrgCode.Visible = true;
        }

        protected void lstAdmins_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemoveAdmins.Visible = true;
        }

        protected void lstManagers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemoveManagers.Visible = true;
        }

        protected void btnSaveTeam_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);
            DTO.Team team = new DTO.Team();

            try
            {
                team.Name = txtTeamNameToAdd.Text;
                team.Owner = BLL.ProfileManager.GetInstance().GetProfile(ddlTeamManagerToAdd.SelectedValue);

                BLL.AdminManager.GetInstance().SaveTeam(directorate.Name, team,
                    chkApplyBarriers.Checked, chkApplyUnplanned.Checked, chkApplyTaskTypes.Checked);

                Utility.DisplayInfoMessage("Directorate saved.");
                directorate.Teams.Add(team);

                // update the listbox binding
                lstTeams.DataSource = directorate.Teams;
                lstTeams.DataBind();
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }

            txtTeamNameToAdd.Text = string.Empty;
        }

        protected void btnSaveDA_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            try
            {
                DTO.Profile profile;

                for (int i = 0; i < profileSelectorAddDA.ProfilesChosen.Count; i++)
                {
                    profile = profileSelectorAddDA.ProfilesChosen[i];
                    
                    if (!directorate.Admins.Exists(p => p.Id == profile.Id))
                        directorate.Admins.Add(profile);
                }

                BLL.AdminManager.GetInstance().SaveDirectorate(directorate);

                // update the listbox binding
                lstAdmins.DataSource = directorate.Admins;
                lstAdmins.DataTextField = "DisplayName";
                lstAdmins.DataValueField = "Username";
                lstAdmins.DataBind();
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }

            profileSelectorAddDA.Clear();
        }

        protected void btnSaveDM_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            try
            {
                DTO.Profile profile;

                for (int i = 0; i < profileSelectorAddDM.ProfilesChosen.Count; i++)
                {
                    profile = profileSelectorAddDM.ProfilesChosen[i];

                    if (!directorate.Managers.Exists(p => p.Id == profile.Id))
                        directorate.Managers.Add(profile);
                }

                BLL.AdminManager.GetInstance().SaveDirectorate(directorate);

                // update the listbox binding
                lstManagers.DataSource = directorate.Managers;
                lstManagers.DataTextField = "DisplayName";
                lstManagers.DataValueField = "Username";
                lstManagers.DataBind();
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }

            profileSelectorAddDM.Clear();
        }

        protected void btnAddDirectorateOrgCode_Click(object sender, EventArgs e)
        {
            ListItem itemFound = lstNewDirectorateOrgCodes.Items.FindByText(txtNewDirectorateOrgCode.Text);

            if (itemFound == null)
            {
                ListItem newItem = new ListItem(txtNewDirectorateOrgCode.Text);
                lstNewDirectorateOrgCodes.Items.Add(newItem);
                txtNewDirectorateOrgCode.Text = string.Empty;
            }
        }

        protected void btnRemoveDirectorateOrgCode_Click(object sender, EventArgs e)
        {
            lstNewDirectorateOrgCodes.Items.Remove(lstNewDirectorateOrgCodes.SelectedValue);
        }

        protected void btnSaveDirectorate_Click(object sender, EventArgs e)
        {
            DTO.Directorate newDirectorate = new WALT.DTO.Directorate();

            newDirectorate.Name = txtNewDirectorate.Text;
            //newDirectorate.OrgCode = txtNewDirectorateOrgCode.Text;

            // go through the selected items in the list and add them as admins
            try
            {
                DTO.Profile profile;

                for (int i = 0; i < profileSelectorDA.ProfilesChosen.Count; i++)
                {
                    profile = profileSelectorDA.ProfilesChosen[i];
                    newDirectorate.Admins.Add(profile);
                }

                foreach (ListItem orgCodeItem in lstNewDirectorateOrgCodes.Items)
                {
                    newDirectorate.OrgCodes.Add(orgCodeItem.Value);
                }

                if (newDirectorate.Name.Length > 0)
                {
                    BLL.AdminManager.GetInstance().SaveDirectorate(newDirectorate);

                    // update the listbox binding
                    lstDirectorates.DataSource = BLL.AdminManager.GetInstance().GetDirectorateNameList();
                    lstDirectorates.DataBind();
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }

            txtNewDirectorate.Text = string.Empty;
            txtNewDirectorateOrgCode.Text = string.Empty;
            lstNewDirectorateOrgCodes.Items.Clear();
            profileSelectorDA.Clear();
        }

        protected void btnSaveDirectorateEdit_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            directorate.Name = txtEditDirectorateName.Text;
            //directorate.OrgCode = txtEditDirectorateOrgCode.Text;

            if (directorate.Name.Length > 0)
            {
                try
                {
                    BLL.AdminManager.GetInstance().SaveDirectorate(directorate);
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }

            txtEditDirectorateName.Text = lstDirectorates.SelectedValue;
        }

        protected void btnCancelAddDA_Click(object sender, EventArgs e)
        {
            profileSelectorAddDA.Clear();
        }

        protected void btnCancelAddDM_Click(object sender, EventArgs e)
        {
            profileSelectorAddDM.Clear();
        }

        protected void btnCancelAddDirectorate_Click(object sender, EventArgs e)
        {
            txtNewDirectorate.Text = string.Empty;
            txtNewDirectorateOrgCode.Text = string.Empty;
            lstNewDirectorateOrgCodes.Items.Clear();
            profileSelectorDA.Clear();
        }

        protected void btnCancelEditDirectorate_Click(object sender, EventArgs e)
        {
            txtEditDirectorateName.Text = lstDirectorates.SelectedValue;
        }

        protected void btnCancelAddOrgCodes_Click(object sender, EventArgs e)
        {
            txtNewOrgCode.Text = string.Empty;
            lstNewOrgCodes.Items.Clear();
        }

        protected void btnCancelAddTeam_Click(object sender, EventArgs e)
        {
            txtTeamNameToAdd.Text = string.Empty;
        }

        protected void btnRemoveAdmins_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            if (directorate != null)
            {
                try
                {
                    foreach (ListItem item in lstAdmins.Items)
                    {
                        if (item.Selected)
                        {
                            DTO.Profile userToRemove = directorate.Admins.Find(delegate(DTO.Profile p) { return p.Username == item.Value; });
                            directorate.Admins.Remove(userToRemove);
                        }
                    }

                    BLL.AdminManager.GetInstance().SaveDirectorate(directorate);

                    lstAdmins.DataSource = directorate.Admins;
                    lstAdmins.DataTextField = "DisplayName";
                    lstAdmins.DataValueField = "Username";
                    lstAdmins.DataBind();
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                btnRemoveAdmins.Visible = false;
                lstAdmins.ClearSelection();
            }
        }

        protected void btnRemoveManagers_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            if (directorate != null)
            {
                try
                {
                    foreach (ListItem item in lstManagers.Items)
                    {
                        if (item.Selected)
                        {
                            DTO.Profile userToRemove = directorate.Managers.Find(delegate(DTO.Profile p) { return p.Username == item.Value; });
                            directorate.Managers.Remove(userToRemove);
                        }
                    }

                    BLL.AdminManager.GetInstance().SaveDirectorate(directorate);

                    lstManagers.DataSource = directorate.Managers;
                    lstManagers.DataTextField = "DisplayName";
                    lstManagers.DataValueField = "Username";
                    lstManagers.DataBind();
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                btnRemoveManagers.Visible = false;
                lstManagers.ClearSelection();
            }
        }

        protected void btnAddOrgCodeToList_Click(object sender, EventArgs e)
        {
            ListItem itemFound = lstNewOrgCodes.Items.FindByValue(txtNewOrgCode.Text);

            if (itemFound == null)
            {
                ListItem newItem = new ListItem(txtNewOrgCode.Text);
                lstNewOrgCodes.Items.Add(newItem);
                txtNewOrgCode.Text = string.Empty;
            }
        }

        protected void btnRemoveOrgCodeFromList_Click(object sender, EventArgs e)
        {
            lstNewOrgCodes.Items.Remove(lstNewOrgCodes.SelectedValue);
        }

        protected void btnRemoveOrgCode_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            if (directorate != null)
            {
                try
                {
                    directorate.OrgCodes.Remove(lstOrgCodes.SelectedValue);
                    BLL.AdminManager.GetInstance().SaveDirectorate(directorate);
                    lstOrgCodes.Items.Remove(lstOrgCodes.SelectedValue);
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                btnRemoveOrgCode.Visible = false;
                lstOrgCodes.ClearSelection();
            }
        }

        protected void btnSaveOrgCodes_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = BLL.AdminManager.GetInstance().GetDirectorate(lstDirectorates.SelectedValue, true);

            if (lstNewOrgCodes.Items.Count > 0)
            {
                foreach (ListItem newOrgCode in lstNewOrgCodes.Items)
                {
                    if (newOrgCode.Value.Length > 0)
                    {
                        directorate.OrgCodes.Add(newOrgCode.Value);
                    }
                }

                if (directorate.Name.Length > 0)
                {
                    try
                    {
                        BLL.AdminManager.GetInstance().SaveDirectorate(directorate);
                        lstOrgCodes.DataSource = directorate.OrgCodes;
                        lstOrgCodes.DataBind();
                    }
                    catch (Exception ex)
                    {
                        Utility.DisplayException(ex);
                    }
                }
            }

            txtNewOrgCode.Text = string.Empty;
            lstNewOrgCodes.Items.Clear();
        }

        protected void btnSetExempt_Click(object sender, EventArgs e)
        {
            bool exempt = ((Button)sender).ID == "btnExempt";
            List<long> profileIDs = new List<long>();

            foreach (ListItem item in lstMembers.Items)
            {
                if (item.Selected)
                {
                    profileIDs.Add(long.Parse(item.Value));
                }
            }

            if (profileIDs.Count > 0)
            {
                try
                {
                    BLL.ProfileManager.GetInstance().SetProfilesPlanExempt(profileIDs, exempt);
                    LoadMembers(null);
                    Utility.DisplayInfoMessage("Profiles saved.");
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }
            else
            {
                Utility.DisplayErrorMessage("No members selected.");
            }
        }
    }
}
