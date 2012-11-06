using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace WALT.UIL.Controls
{
    public partial class TaskUnplannedCodes : System.Web.UI.UserControl
    {
        DTO.Directorate _directorate;
        DTO.Team _team;
        bool _allowDAEdit = false;

        private void ClearDetailPanel()
        {
            lblROUnplannedActive.Text = string.Empty;
            lblROUnplannedActive.BackColor = System.Drawing.Color.LightGray;
            lblROUnplannedCode.Text = string.Empty;
            lblROUnplannedTitle.Text = string.Empty;
            txtROUnplannedSADescription.Text = string.Empty;
            txtROUnplannedDADescription.Text = string.Empty;

            btnUnplannedActive.Text = string.Empty;
            btnUnplannedActive.BackColor = System.Drawing.Color.LightGray;
            txtUnplannedCode.Text = string.Empty;
            txtUnplannedSADescription.Text = string.Empty;
            txtUnplannedDADescription.Text = string.Empty;
            txtUnplannedTitle.Text = string.Empty;

            ddlNewUnplannedParent.Items.Clear();
        }

        public void PopulateUnplanned(DTO.Directorate directorate, DTO.Team team)
        {
            _directorate = directorate;
            _team = team;

            HttpContext.Current.Session["tasks_unplanned_directorate"] = _directorate;
            HttpContext.Current.Session["tasks_unplanned_team"] = team;

            treeViewUnplanned.Nodes.Clear();

            ClearDetailPanel();

            panelUnplannedEditing.Enabled = false;

            // only SA users can add unplanned codes before a directorate is selected
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
            {
                panelAddUnplanned.Visible = true;
            }
            else
            {
                panelAddUnplanned.Visible = false;
            }

            rowApplyAll.Visible = false;

            try
            {
                // if directorate is null, populate the system level unplanned
                if (directorate == null)
                {
                    PopulateUnplannedTreeView(BLL.AdminManager.GetInstance().GetUnplannedCodeList(BLL.AdminManager.GetInstance().GetOrg(), false, null), null);

                    ddlNewUnplannedParent.Items.Insert(0, "None");
                    ddlNewUnplannedParent.Items[0].Value = "0";

                    TeamMode(false);
                }
                // if directorate is selected, but not team, display the system and directorate level unplanned codes
                else if (directorate != null && team == null)
                {
                    PopulateUnplannedTreeView(GetDirectorateUnplanned(directorate, null), null);

                    TeamMode(false);
                    rowApplyAll.Visible = true;
                }
                // if the directorate and team are defined, go into ALM unplanned assigning mode
                else if (directorate != null && team != null)
                {
                    PopulateUnplannedTreeView(GetDirectorateUnplanned(directorate, true), team);

                    TeamMode(true);
                }

                // hide both detail displays until an unplanned code is selected
                if (treeViewUnplanned.SelectedNode == null)
                {
                    panelUnplannedEditing.Visible = false;
                    panelUnplannedReadOnly.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        private List<DTO.UnplannedCode> GetDirectorateUnplanned(DTO.Directorate directorate, bool? active)
        {
            DTO.Team directorateAsTeam = new DTO.Team();
            directorateAsTeam.Id = directorate.Id;

            return BLL.AdminManager.GetInstance().GetUnplannedCodeList(directorateAsTeam, false, active);
        }

        private List<DTO.UnplannedCode> GetAppliedUnplanned(DTO.Team team)
        {
            return team == null ? null : BLL.AdminManager.GetInstance().GetUnplannedCodeList(team, true, true);
        }

        private bool CheckUnplannedApplied(List<DTO.UnplannedCode> appliedList, DTO.UnplannedCode unplanned)
        {
            return appliedList == null ? false : appliedList.Exists(x => x.Title == unplanned.Title);
        }

        private void DisplayForSA()
        {
            panelAddUnplanned.Visible = true;
            panelUnplannedEditing.Visible = true;
            panelUnplannedReadOnly.Visible = false;
        }

        private void DisplayForDA()
        {
            //if (treeViewUnplanned.SelectedNode != null && treeViewUnplanned.SelectedNode.Depth == 1)
            //{
            panelAddUnplanned.Visible = true;
            //}
            //else
            //{
            //    panelAddUnplanned.Visible = false;
            //}

            if (!_allowDAEdit)
            {
                txtUnplannedDADescription.Visible = true;
                panelUnplannedEditing.Visible = false;
                panelUnplannedReadOnly.Visible = true;
            }
            else
            {
                txtUnplannedDADescription.Visible = false;
                panelUnplannedEditing.Visible = true;
                panelUnplannedReadOnly.Visible = false;
            }

            txtROUnplannedDADescription.ReadOnly = false;
            txtROUnplannedDADescription.BackColor = System.Drawing.Color.White;
        }

        private void DisplayForALM()
        {
            panelAddUnplanned.Visible = false;
            panelUnplannedEditing.Visible = false;
            panelUnplannedReadOnly.Visible = true;
            txtROUnplannedDADescription.ReadOnly = true;
        }

        private void CustomizeDisplayForUser()
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];

            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
            {
                DisplayForSA();
            }
            else if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
            {
                DisplayForDA();
            }
            else
            {
                DisplayForALM();
            }
        }

        private bool CanEdit(DTO.Profile profile, DTO.Team team, DTO.Directorate directorate)
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

        private void TeamMode(bool enable)
        {
            DTO.Profile currentUser = BLL.ProfileManager.GetInstance().GetProfile();
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_unplanned_team"];
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];

            if (enable)
            {
                if (CanEdit(currentUser, team, directorate))
                {
                    btnApplyUnplannedToTeam.Visible = true;
                    treeViewUnplanned.ShowCheckBoxes = TreeNodeTypes.All;
                }
                else
                {
                    btnApplyUnplannedToTeam.Visible = false;
                    treeViewUnplanned.ShowCheckBoxes = TreeNodeTypes.None;
                }

                DisplayForALM();

                txtROUnplannedDADescription.Visible = true;
                txtROUnplannedDADescription.ReadOnly = true;
                txtROUnplannedDADescription.BackColor = System.Drawing.Color.LightGray;
                panelROSaveUnplannedButton.Visible = false;
                lnkSelectAll.Visible = true;
            }
            else
            {
                treeViewUnplanned.ShowCheckBoxes = TreeNodeTypes.None;

                btnApplyUnplannedToTeam.Visible = false;
                lnkSelectAll.Visible = false;

                CustomizeDisplayForUser();
            }
        }

        private TreeNode NewUnplannedNode(DTO.UnplannedCode unplanned)
        {
            TreeNode node = new TreeNode();
            node.Text = unplanned.Code + ": " + unplanned.Title;
            node.Value = unplanned.Id.ToString();

            return node;
        }

        private void PopulateUnplannedTreeView(List<DTO.UnplannedCode> unplannedCodes, DTO.Team team)
        {
            List<DTO.UnplannedCode> appliedUnplanned = GetAppliedUnplanned(team);

            foreach (DTO.UnplannedCode unplanned in unplannedCodes)
            {
                // top level (system) unplanned code

                TreeNode node = NewUnplannedNode(unplanned);
                node.Checked = CheckUnplannedApplied(appliedUnplanned, unplanned);
                if (node.Checked)
                    node.Text = MakeBold(node.Text);
                node.Expanded = team == null ? false : true;

                foreach (DTO.UnplannedCode child in unplanned.Children)
                {
                    // second level (system) unplanned code
                    List<DTO.UnplannedCode> appliedChildren = GetAppliedChildren(appliedUnplanned, unplanned);

                    TreeNode childNode = NewUnplannedNode(child);
                    childNode.Checked = CheckUnplannedApplied(appliedChildren, child);
                    if (childNode.Checked)
                        childNode.Text = MakeBold(childNode.Text);
                    childNode.Expanded = team == null ? false : true;

                    foreach (DTO.UnplannedCode grandchild in child.Children)
                    {
                        // directorate level unplanned code
                        List<DTO.UnplannedCode> appliedGrandchildren = GetAppliedChildren(appliedChildren, child);

                        TreeNode grandchildNode = NewUnplannedNode(grandchild);
                        grandchildNode.Checked = CheckUnplannedApplied(appliedGrandchildren, grandchild);
                        if (grandchildNode.Checked)
                            grandchildNode.Text = MakeBold(grandchildNode.Text);
                        childNode.ChildNodes.Add(grandchildNode);
                    }

                    node.ChildNodes.Add(childNode);

                    if (_directorate != null)
                    {
                        // also add to the "new unplanned" parent drop down list
                        ListItem unplannedItem = new ListItem();
                        unplannedItem.Text = child.Code + ": " + child.Title;
                        unplannedItem.Value = child.Id.ToString();
                        ddlNewUnplannedParent.Items.Add(unplannedItem);
                    }
                }

                treeViewUnplanned.Nodes.Add(node);

                if (_directorate == null)
                {
                    // also add to the "new unplanned" parent drop down list
                    ListItem unplannedItem2 = new ListItem();
                    unplannedItem2.Text = unplanned.Code + ": " + unplanned.Title;
                    unplannedItem2.Value = unplanned.Id.ToString();
                    ddlNewUnplannedParent.Items.Add(unplannedItem2);
                }
            }
        }

        private List<DTO.UnplannedCode> GetAppliedChildren(List<DTO.UnplannedCode> appliedUnplanned, DTO.UnplannedCode unplannedToFind)
        {
            DTO.UnplannedCode appliedFound = null;
            List<DTO.UnplannedCode> appliedChildren = null;

            if (appliedUnplanned != null)
            {
                if (unplannedToFind != null)
                    appliedFound = appliedUnplanned.Find(x => x.Id == unplannedToFind.Id);

                if (appliedFound != null && appliedFound.Children != null)
                    appliedChildren = appliedFound.Children;
            }

            return appliedChildren;
        }

        private void AddUnplannedChildrenToDDL(DTO.UnplannedCode unplanned, int level)
        {
            string indent = string.Empty;

            if (unplanned.Children != null && unplanned.Children.Count > 0)
            {
                for (int i = 0; i < level; i++)
                {
                    indent += "-";
                }

                foreach (DTO.UnplannedCode child in unplanned.Children)
                {
                    ListItem unplannedItem = new ListItem();
                    unplannedItem.Text = indent + " " + child.Code + ": " + child.Title;
                    unplannedItem.Value = child.Id.ToString();
                    ddlNewUnplannedParent.Items.Add(unplannedItem);
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            treeViewUnplanned.Attributes.Add("onclick", "postBackByObject()");
            lblApplyUnplannedStatus.Visible = false;
            lblSaveUnplannedStatus.Visible = false;
            lblROSaveUnplannedStatus.Visible = false;

            if (!IsPostBack)
            {
                LoadData();

                DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_unplanned_team"];

                bool teamSelected = team == null ? false : true;

                TeamMode(teamSelected);
            }

            // hide both detail displays until a unplanned code is selected
            if (treeViewUnplanned.SelectedNode == null)
            {
                panelUnplannedEditing.Visible = false;
                panelUnplannedReadOnly.Visible = false;
            }
        }

        private void LoadData()
        {

        }

        protected void btnROSaveUnplanned_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];

                long unplannedId = long.Parse(treeViewUnplanned.SelectedValue);

                DTO.UnplannedCode unplanned = BLL.AdminManager.GetInstance().GetUnplannedCode(unplannedId);

                // also save the DA description text if the user has directorate manage
                if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
                {
                    string descriptionText = string.Empty;

                    // if the user is an SA, the text will be in the normal text box, otherwise it's in the read only textbox
                    if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                        descriptionText = txtUnplannedDADescription.Text;
                    else
                        descriptionText = txtUnplannedDADescription.Text;

                    if (directorate != null)
                        BLL.AdminManager.GetInstance().SaveUnplannedCodeExtendedDescription(directorate.Id, unplanned.Id, descriptionText);
                }

                lblSaveUnplannedStatus.Text = "Saved!";
                lblROSaveUnplannedStatus.Text = "Saved!";
                lblSaveUnplannedStatus.ForeColor = System.Drawing.Color.Green;
                lblROSaveUnplannedStatus.ForeColor = System.Drawing.Color.Green;
                lblSaveUnplannedStatus.Visible = true;
                lblROSaveUnplannedStatus.Visible = true;
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnSaveUnplanned_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];
                DTO.Team directorateAsTeam;

                if (directorate == null)
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetOrg();
                }
                else
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Name);
                }

                long unplannedId = long.Parse(treeViewUnplanned.SelectedValue);

                DTO.UnplannedCode unplanned = BLL.AdminManager.GetInstance().GetUnplannedCode(unplannedId);
                unplanned.Title = txtUnplannedTitle.Text.Trim();
                unplanned.Description = txtUnplannedSADescription.Text.Trim();
                unplanned.Active = cbUnplannedActive.Checked;
                unplanned.Code = txtUnplannedCode.Text.Trim();

                BLL.AdminManager.GetInstance().SaveUnplannedCode(directorateAsTeam, unplanned, false);

                // also save the DA description text if the user has directorate manage
                if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
                {
                    string descriptionText = string.Empty;

                    // if the user is an SA, the text will be in the normal text box, otherwise it's in the read only textbox
                    if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                    {
                        descriptionText = txtUnplannedDADescription.Text;
                    }
                    else
                    {
                        descriptionText = txtUnplannedDADescription.Text;
                    }

                    if (directorate != null)
                    {
                        BLL.AdminManager.GetInstance().SaveUnplannedCodeExtendedDescription(directorate.Id, unplanned.Id, descriptionText);
                    }
                }

                lblSaveUnplannedStatus.Text = "Saved!";
                lblROSaveUnplannedStatus.Text = "Saved!";
                lblSaveUnplannedStatus.ForeColor = System.Drawing.Color.Green;
                lblROSaveUnplannedStatus.ForeColor = System.Drawing.Color.Green;
                lblSaveUnplannedStatus.Visible = true;
                lblROSaveUnplannedStatus.Visible = true;

                treeViewUnplanned.SelectedNode.Text = unplanned.Code + ": " + unplanned.Title;
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnSaveNewUnplanned_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];
                DTO.Team directorateAsTeam;

                if (directorate == null)
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetOrg();
                }
                else
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Name);
                }

                DTO.UnplannedCode unplanned = new DTO.UnplannedCode();
                unplanned.Title = txtNewUnplannedTitle.Text;
                unplanned.Description = txtNewUnplannedDescription.Text;
                unplanned.Active = cbNewUnplannedActive.Checked;
                unplanned.ParentId = long.Parse(ddlNewUnplannedParent.SelectedValue);

                BLL.AdminManager.GetInstance().SaveUnplannedCode(directorateAsTeam, unplanned,
                    (directorate != null && chkApplyAll.Checked));

                txtNewUnplannedDescription.Text = string.Empty;
                txtNewUnplannedTitle.Text = string.Empty;
                cbNewUnplannedActive.Checked = true;
                chkApplyAll.Checked = false;
                ddlNewUnplannedParent.SelectedIndex = 0;

                PopulateUnplanned(directorate, null);
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnCancelUnplannedPopup_Click(object sender, EventArgs e)
        {
            txtNewUnplannedDescription.Text = string.Empty;
            txtNewUnplannedTitle.Text = string.Empty;

            if (ddlNewUnplannedParent.Items.Count > 0)
            {
                ddlNewUnplannedParent.SelectedIndex = 0;
            }
        }

        protected void unplanned_SelectedNodeChanged(object sender, EventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_unplanned_team"];

            TreeView treeViewUnplanned = sender as TreeView;

            if (treeViewUnplanned.SelectedNode == null)
            {
                //reload the page
                Response.Redirect(Request.RawUrl);
            }
            else
            {
                if (treeViewUnplanned != null)
                {
                    panelUnplannedEditing.Enabled = true;

                    TreeNode node = treeViewUnplanned.SelectedNode;

                    DTO.UnplannedCode unplanned = BLL.AdminManager.GetInstance().GetUnplannedCode(long.Parse(node.Value));//FindUnplanned(directorate.Unplanned, node);

                    ListItem selectItem = ddlNewUnplannedParent.Items.FindByValue(unplanned.Id.ToString());
                    if (selectItem != null)
                    {
                        ddlNewUnplannedParent.ClearSelection();
                        selectItem.Selected = true;
                    }

                    if (unplanned != null)
                    {
                        SetActive(unplanned.Active);

                        //read-only controls
                        lblROUnplannedCode.Text = unplanned.Code;
                        lblROUnplannedTitle.Text = unplanned.Title;
                        txtROUnplannedSADescription.Text = unplanned.Description;

                        // edit controls
                        txtUnplannedCode.Text = unplanned.Code;
                        txtUnplannedTitle.Text = unplanned.Title;
                        txtUnplannedSADescription.Text = unplanned.Description;

                        if (directorate != null)
                        {
                            string extendedUnplannedDescription = BLL.AdminManager.GetInstance().GetUnplannedCodeExtendedDescription(directorate.Id, unplanned.Id);

                            txtROUnplannedDADescription.Text = extendedUnplannedDescription;
                            txtUnplannedDADescription.Text = extendedUnplannedDescription;

                            txtROUnplannedDADescription.Visible = true;
                            txtUnplannedDADescription.Visible = true;

                            if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
                            {
                                panelROSaveUnplannedButton.Visible = true;

                                // if the selected barrier is 3 then allow edit
                                if (BLL.AdminManager.GetInstance().GetBarrierLevel(unplanned.Id) == 2)
                                {
                                    _allowDAEdit = true;
                                }   
                            }
                        }
                        else
                        {
                            txtROUnplannedDADescription.Visible = false;
                            txtUnplannedDADescription.Visible = false;
                        }

                        CustomizeDisplayForUser();
                    }
                }

                if (team != null)
                    TeamMode(true);
                else
                    TeamMode(false);
            }
        }

        private void SetActive(bool active)
        {
            if (active)
            {
                cbUnplannedActive.Checked = true;
                btnUnplannedActive.Text = "Active";
                lblROUnplannedActive.Text = "Active";
                btnUnplannedActive.BackColor = System.Drawing.Color.LightGreen;
                lblROUnplannedActive.BackColor = System.Drawing.Color.LightGreen;
            }
            else
            {
                cbUnplannedActive.Checked = false;
                btnUnplannedActive.Text = "Inactive";
                lblROUnplannedActive.Text = "Inactive";
                btnUnplannedActive.BackColor = System.Drawing.Color.PaleVioletRed;
                lblROUnplannedActive.BackColor = System.Drawing.Color.LightGreen;
            }
        }

        protected void btnUnplannedActive_Click(object sender, EventArgs e)
        {
            if (treeViewUnplanned.SelectedNode != null)
                SetActive(!cbUnplannedActive.Checked);
        }

        protected void unplanned_TreeNodeCheckChanged(object sender, TreeNodeEventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];

            TreeView treeViewUnplanned = sender as TreeView;

            if (e.Node != null)
            {
                CheckNode(e.Node, e.Node.Checked);
            }
        }

        private void CheckNode(TreeNode node, bool check)
        {
            node.Checked = check;
            string text = node.Text;

            if (check)
            {
                node.Text = MakeBold(node.Text);
            }
            else
            {
                node.Text = RemoveBold(node.Text);
            }

            if (node.ChildNodes != null)
            {
                foreach (TreeNode child in node.ChildNodes)
                {
                    CheckNode(child, check);
                }
            }
        }

        private string MakeBold(string text)
        {
            return "<b>" + text + "</b>";
        }

        private string RemoveBold(string text)
        {
            text = text.Replace("<b>", string.Empty);
            text = text.Replace("</b>", string.Empty);

            return text;
        }

        protected void btnApplyUnplannedToTeam_Click(object sender, EventArgs e)
        {
            lblApplyUnplannedStatus.Visible = false;

            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_unplanned_team"];
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_unplanned_directorate"];

            List<DTO.UnplannedCode> unplannedList = new List<DTO.UnplannedCode>();

            // loop through the treeview, keep track of the unplanned IDs that are selected
            foreach (TreeNode node in treeViewUnplanned.Nodes)
            {
                ApplyUnplannedList(ref unplannedList, node);
            }

            team.SelectedUnplannedCodes = unplannedList;

            BLL.AdminManager.GetInstance().ApplyUnplannedCodes(team);

            HttpContext.Current.Session["tasks_unplanned_team"] = team;

            PopulateUnplanned(directorate, team);

            lblApplyUnplannedStatus.Visible = true;
        }

        private void ApplyUnplannedList(ref List<DTO.UnplannedCode> unplannedList, TreeNode node)
        {
            if (node.Checked)
            {
                unplannedList.Add(BLL.AdminManager.GetInstance().GetUnplannedCode(long.Parse(node.Value)));
            }

            foreach (TreeNode child in node.ChildNodes)
            {
                ApplyUnplannedList(ref unplannedList, child);
            }
        }

        protected void lnkSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeViewUnplanned.Nodes)
            {
                CheckNode(node, true);
            }
        }
    }
}