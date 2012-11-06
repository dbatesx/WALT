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
    public partial class Roles : System.Web.UI.Page
    {
        private List<DTO.Role> _roles;

        protected void Page_Load(object sender, EventArgs e)
        {   
            _roles = (List<DTO.Role>)HttpContext.Current.Session["admin_roles"];

            if (!IsPostBack)
            {
                LoadData();
            }

            // check if the current user has permission to edit roles
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.ROLE_MANAGE))
            {
                btnMoveLeft.Enabled = true;
                btnMoveRight.Enabled = true;
                panelAddRole.Enabled = true;
            }
            else
            {
                btnMoveLeft.Enabled = false;
                btnMoveRight.Enabled = false;
                panelAddRole.Enabled = false;
            }
        }

        private void Bind()
        {
            GridView1.DataSource = _roles;

            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.ROLE_MANAGE))
                GridView1.AutoGenerateEditButton = true;
            else
                GridView1.AutoGenerateEditButton = false;

            GridView1.AutoGenerateColumns = false;
            GridView1.DataBind();
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstRoleActions.DataSource = _roles[GridView1.SelectedIndex].Actions;
            lstRoleActions.DataBind();

            var query = from item1 in BLL.AdminManager.GetInstance().GetSystemActionList()
                        where !((from item2 in _roles[GridView1.SelectedIndex].Actions
                                select item2.ToString()).Contains(item1))
                        select item1;

            lstSystemActions.DataSource = query;
            lstSystemActions.DataBind();
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            Bind();
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            Bind();
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {           
            TextBox tb1 = (TextBox)GridView1.Rows[e.RowIndex].Cells[1].Controls[0];
            TextBox tb2 = (TextBox)GridView1.Rows[e.RowIndex].Cells[2].Controls[0];
            CheckBox cb1 = (CheckBox)GridView1.Rows[e.RowIndex].Cells[3].Controls[0];

            _roles[e.RowIndex].Title = tb1.Text;
            _roles[e.RowIndex].Description = tb2.Text;
            _roles[e.RowIndex].Active = cb1.Checked;

            SaveRole(e.RowIndex);

            GridView1.EditIndex = -1;
            Bind();
        }

        private void SaveRole(int index)
        {
            try
            {
                DTO.Role role = _roles[index];
                BLL.AdminManager.GetInstance().SaveRole(role);
                _roles[index] = role;
                Utility.DisplayInfoMessage("Role saved.");
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnMoveLeft_Click(object sender, EventArgs e)
        {
            if (GridView1.SelectedIndex > -1)
            {
                string selectedRole = GridView1.DataKeys[GridView1.SelectedRow.RowIndex].Value.ToString();
                DTO.Role role = _roles.Find(delegate(DTO.Role r) { return r.Description == selectedRole; });

                List<ListItem> actionsToMove = new List<ListItem>();

                foreach (ListItem item in lstSystemActions.Items)
                {
                    if (item.Selected)
                        actionsToMove.Add(item);
                }

                foreach (ListItem actionsItem in actionsToMove)
                {
                    role.Actions.Add((DTO.Action)Enum.Parse(typeof(DTO.Action), actionsItem.Text));

                    if (SaveRole(role))
                    {
                        lstRoleActions.Items.Add(actionsItem);
                        lstSystemActions.Items.Remove(actionsItem);
                    }
                }
            }
        }

        protected void btnMoveRight_Click(object sender, EventArgs e)
        {
            if (GridView1.SelectedIndex > -1)
            {
                string selectedRole = GridView1.DataKeys[GridView1.SelectedRow.RowIndex].Value.ToString();
                DTO.Role role = _roles.Find(delegate(DTO.Role r) { return r.Description == selectedRole; });

                List<ListItem> actionsToMove = new List<ListItem>();

                foreach (ListItem item in lstRoleActions.Items)
                {
                    if (item.Selected)
                        actionsToMove.Add(item);
                }

                foreach (ListItem actionsItem in actionsToMove)
                {
                    DTO.Action actionToRemove = role.Actions.Find(delegate(DTO.Action a) { return a == (DTO.Action)Enum.Parse(typeof(DTO.Action), actionsItem.Text); });

                    role.Actions.Remove(actionToRemove);

                    if (SaveRole(role))
                    {
                        lstSystemActions.Items.Add(actionsItem);
                        lstRoleActions.Items.Remove(actionsItem);
                    }
                }
            }
        }

        private void LoadData()
        {
            _roles = BLL.AdminManager.GetInstance().GetRoleList();
            HttpContext.Current.Session["admin_roles"] = _roles;
            GridView1.EditIndex = -1;
            Bind();
        }

        private bool SaveRole(DTO.Role r)
        {
            bool result = true;

            try
            {
                BLL.AdminManager.GetInstance().SaveRole(r);
                Utility.DisplayInfoMessage("Role saved.");
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
                result = false;
            }

            return result;
        }

        protected void btnAddRole_Click(object sender, EventArgs e)
        {
            DTO.Role r = new DTO.Role();
            r.Title = txtAddRoleTitle.Text;
            r.Description = txtAddRoleDescription.Text;

            if (SaveRole(r))
            {
                txtAddRoleTitle.Text = "";
                txtAddRoleDescription.Text = "";
                LoadData();
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            Bind();
        }
    }
}
