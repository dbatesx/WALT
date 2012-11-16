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
    public partial class Profiles : System.Web.UI.Page
    {
        List<DTO.Profile> _profiles;

        protected void Page_Load(object sender, EventArgs e)
        {
            _profiles = (List<DTO.Profile>)Session["admin_profiles"];

            if (!IsPostBack)
            {
                LoadData();
            }

            if (IsAllowed(DTO.Action.PROFILE_MANAGE))
            {
                btnMoveLeft.Enabled = true;
                btnMoveRight.Enabled = true;
            }
            else
            {
                btnMoveLeft.Enabled = false;
                btnMoveRight.Enabled = false;
            }

            phSync.Visible = IsAllowed(DTO.Action.SYSTEM_MANAGE);
        }

        void LoadData()
        {
            GridView1.PageIndex = 0;
            GridView1.SelectedIndex = -1;
            lstAvailableRoles.Items.Clear();
            lstRoles.Items.Clear();

            _profiles = new List<DTO.Profile>();

            //if (TextBox1.Text.Length > 0)
            {
                List<string> profiles = BLL.ProfileManager.GetInstance().GetProfileDisplayNameList();

                foreach (string p in profiles)
                {
                    if (p.ToLower().Contains(TextBox1.Text.ToLower()))
                    {
                        _profiles.Add(BLL.ProfileManager.GetInstance().GetProfileByDisplayName(p));
                    }
                }
            }

            Label1.Text = _profiles.Count + " Profiles found.";

            Session["admin_profiles"] = _profiles;

            Bind();
        }

        void Bind()
        {
            //if (IsAllowed(DTO.Action.PROFILE_MANAGE))
            //{
            //    GridView1.AutoGenerateEditButton = true;
            //}
            //else
            //{
            //    GridView1.AutoGenerateEditButton = false;
            //}
            
            GridView1.AutoGenerateEditButton = IsAllowed(DTO.Action.PROFILE_MANAGE);

            GridView1.DataSource = _profiles;
            GridView1.DataBind();
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            lstAvailableRoles.Items.Clear();
            lstRoles.Items.Clear();

            GridView1.PageIndex = e.NewPageIndex;
            GridView1.SelectedIndex = -1;
            Bind();
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            Bind();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // TODO: if (bManualProfiles) {
            ShowProfileEditor(e.NewEditIndex);
            //IdxProfile.Value = e.NewEditIndex.ToString();
            //DTO.Profile profile = _profiles[e.NewEditIndex];
            //txtUserName.Text = profile.Username;
            //txtDisplayName.Text = profile.DisplayName;
            //txtEmployeeID.Text = profile.EmployeeID;
            //txtOrgCode.Text = profile.OrgCode;

            //divEditProfilePopupHeader.InnerText = "Edit Profile for " + profile.DisplayName;

            //modalPopupExtenderEditProfile.Show();
            // }

            GridView1.EditIndex = e.NewEditIndex;
            ((BoundField)GridView1.Columns[0]).ReadOnly = true;
            ((BoundField)GridView1.Columns[1]).ReadOnly = true;
            Bind();
        }

        protected void ShowProfileEditor(int editIndex)
        {
            PopulateRoles(lbxRoles);

            if (editIndex > -1)
            {
                IdxProfile.Value = editIndex.ToString();
                DTO.Profile profile = _profiles[editIndex];
                txtUserName.Text = profile.Username;
                txtDisplayName.Text = profile.DisplayName;
                txtEmployeeID.Text = profile.EmployeeID;
                txtOrgCode.Text = profile.OrgCode;
                SelectRoles(lbxRoles, profile.Roles);

                divEditProfilePopupHeader.InnerText = "Edit Profile for " + profile.DisplayName;

            }
            modalPopupExtenderEditProfile.Show();

        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            CheckBox cbActive = (CheckBox)GridView1.Rows[e.RowIndex].Cells[3].Controls[0];
            CheckBox cbPlanExempt = (CheckBox)GridView1.Rows[e.RowIndex].Cells[4].Controls[0];

            try
            {
                DTO.Profile profile = _profiles[GridView1.EditIndex];
                profile.Active = cbActive.Checked;
                profile.ExemptPlan = cbPlanExempt.Checked;
                BLL.ProfileManager.GetInstance().SaveProfile(profile);
                Utility.DisplayInfoMessage("Profile saved.");
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }

            GridView1.EditIndex = -1;
            Bind();
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<DTO.Role> usersRoles = null;

                //if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                //{
                //    usersRoles = _profiles[GridView1.SelectedIndex].Roles;
                //}
                //else
                //{
                //    usersRoles = (from item in _profiles[GridView1.SelectedIndex].Roles
                //                where !item.Actions.Contains(DTO.Action.SYSTEM_MANAGE)
                //                select item).ToList();
                //}

                usersRoles = (
                    from item in _profiles[GridView1.SelectedIndex].Roles
                    where (!item.Actions.Contains(DTO.Action.SYSTEM_MANAGE) ||
                          IsAllowed(DTO.Action.SYSTEM_MANAGE))
                    select item).ToList();

                lstRoles.DataSource = usersRoles;
                lstRoles.DataTextField = "Description";
                lstRoles.DataBind();

                List<DTO.Role> allRoles = BLL.AdminManager.GetInstance().GetRoleList();

                List<DTO.Role> unassignedRoles = null;

                //if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                //{
                //    unassignedRoles = (from DTO.Role role in allRoles
                //                       where !usersRoles.Exists(x => x.Id == role.Id)
                //                       select role).ToList();
                //}
                //else
                //{
                //    unassignedRoles = (from DTO.Role role in allRoles
                //                       where !usersRoles.Exists(x => x.Id == role.Id) &&
                //                       !role.Actions.Contains(DTO.Action.SYSTEM_MANAGE)
                //                       select role).ToList();
                //}

                unassignedRoles = (
                    from DTO.Role role in allRoles
                    where !usersRoles.Exists(x => x.Id == role.Id) &&
                    (!role.Actions.Contains(DTO.Action.SYSTEM_MANAGE) ||
                        IsAllowed(DTO.Action.SYSTEM_MANAGE))
                    select role).ToList();

                lstAvailableRoles.DataSource = unassignedRoles;
                lstAvailableRoles.DataTextField = "Description";
                lstAvailableRoles.DataBind();
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnSynchAD_Click(object sender, EventArgs e)
        {
            BLL.ProfileManager.GetInstance().SyncAD();
            LoadData();
        }

        protected void btnAddUser_Click(object sender, EventArgs e)
        {
            if (txtAddUser.Text != string.Empty)
            {
                try
                {
                    DTO.Profile profile = BLL.ProfileManager.GetInstance().AddProfileByADLookup(txtAddUser.Text);

                    if (profile != null)
                    {
                        Utility.DisplayInfoMessage("Profile for " + profile.DisplayName + " created.");
                    }
                    else
                    {
                        Utility.DisplayErrorMessage("No users found for " + txtAddUser.Text + ".");
                    }
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }

            LoadData();
        }

        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            IdxProfile.Value = "-1";
            txtUserName.Text = txtAddUser.Text;
            txtDisplayName.Text = txtAddUser.Text.Contains('\\') ? txtAddUser.Text.Split('\\')[1] : txtAddUser.Text;
            txtEmployeeID.Text = "";
            //txtOrgCode.Text = "";

            divEditProfilePopupHeader.InnerText = "New Profile" + (string.IsNullOrEmpty(txtDisplayName.Text) ? "" : " for " + txtDisplayName.Text);

            modalPopupExtenderEditProfile.Show();
        }

        protected void PopulateRoles(ListControl lstCtrl)
        {
            List<DTO.Role> allRoles = BLL.AdminManager.GetInstance().GetRoleList();

            var unassignedRoles = (
                from DTO.Role role in allRoles
                where (!role.Actions.Contains(DTO.Action.SYSTEM_MANAGE) ||
                    IsAllowed(DTO.Action.SYSTEM_MANAGE))
                select role).ToList();

            lstCtrl.DataSource = unassignedRoles;
            lstCtrl.DataTextField = "Description";
            lstCtrl.DataBind();

        }

        protected void SelectRoles(ListControl lstCtrl, List<DTO.Role> roles)
        {
            foreach (var role in roles)
            {
                //TODO: this will error if role no longer exists
                lstCtrl.Items.FindByText(role.Description).Selected = true;
            }
        }

        protected void btnCancelEditProfile_Click(object sender, EventArgs e)
        { }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text != string.Empty)
            {
                try
                {
                    DTO.Profile profile;
                    int profile_index = int.Parse(IdxProfile.Value);
                    if (profile_index > -1) // ie we're editing an existing profile
                    {
                        profile = _profiles[int.Parse(IdxProfile.Value)];
                    }
                    else
                    {
                        profile = new DTO.Profile();
                    }
                    // Update profile values:
                    profile.Username = txtUserName.Text;
                    profile.DisplayName = txtDisplayName.Text;
                    profile.EmployeeID = txtEmployeeID.Text;
                    profile.OrgCode = txtOrgCode.Text;

                    // Save the profile:
                    BLL.ProfileManager.GetInstance().SaveProfile(profile);

                    Utility.DisplayInfoMessage("Profile for " + profile.DisplayName + " saved.");
                    txtUserName.Text = string.Empty;
                    txtEmployeeID.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }

            LoadData();
        }

        protected void ButtonMoveLeft_Click(object sender, EventArgs e)
        {
            if (GridView1.SelectedIndex > -1)
            {
                DTO.Profile userProfile = _profiles[GridView1.SelectedIndex];

                List<ListItem> rolesToMove = new List<ListItem>();

                foreach (ListItem item in lstAvailableRoles.Items)
                {
                    if (item.Selected)
                    {
                        rolesToMove.Add(item);
                    }
                }

                List<DTO.Role> roles = BLL.AdminManager.GetInstance().GetRoleList();

                foreach (ListItem roleItem in rolesToMove)
                {
                    DTO.Role roleToAdd = (from role in roles
                                          where role.Description == roleItem.Value
                                          select role).Single();

                    userProfile.Roles.Add(roleToAdd);

                    if (SaveProfile(userProfile))
                    {
                        lstRoles.Items.Add(roleItem);
                        lstRoles.Items[lstRoles.Items.Count - 1].Selected = false;
                        lstAvailableRoles.Items.Remove(roleItem);
                    }
                }
            }
        }

        protected void ButtonMoveRight_Click(object sender, EventArgs e)
        {
            if (GridView1.SelectedIndex > -1)
            {
                DTO.Profile userProfile = _profiles[GridView1.SelectedIndex];

                List<ListItem> rolesToMove = new List<ListItem>();

                foreach (ListItem item in lstRoles.Items)
                {
                    if (item.Selected)
                    {
                        rolesToMove.Add(item);
                    }
                }

                foreach (ListItem roleItem in rolesToMove)
                {
                    DTO.Role roleToRemove = userProfile.Roles.Find(delegate(DTO.Role r) { return r.Description == roleItem.Text; });

                    userProfile.Roles.Remove(roleToRemove);

                    if (SaveProfile(userProfile))
                    {
                        lstAvailableRoles.Items.Add(roleItem);
                        lstAvailableRoles.Items[lstAvailableRoles.Items.Count - 1].Selected = false;
                        lstRoles.Items.Remove(roleItem);
                    }
                }
            }
        }

        private bool SaveProfile(DTO.Profile p)
        {
            bool result = true;

            try
            {
                BLL.ProfileManager.GetInstance().SaveProfile(p);
                Utility.DisplayInfoMessage("Profile saved.");
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
                result = false;
            }

            return result;
        }

        private bool IsAllowed(DTO.Action action)
        {
            return BLL.ProfileManager.GetInstance().IsAllowed(action);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
