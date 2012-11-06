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
    public partial class TaskBarriers : System.Web.UI.UserControl
    {
        DTO.Directorate _directorate;
        DTO.Team _team;
        bool _allowDAEdit = false;

        private void ClearDetailPanel()
        {
            lblROBarrierActive.Text = string.Empty;
            lblROBarrierActive.BackColor = System.Drawing.Color.LightGray;
            lblROBarrierCode.Text = string.Empty;
            lblROBarrierTitle.Text = string.Empty;
            txtROBarrierSADescription.Text = string.Empty;
            txtROBarrierDADescription.Text = string.Empty;

            btnBarrierActive.Text = string.Empty;
            btnBarrierActive.BackColor = System.Drawing.Color.LightGray;
            txtBarrierCode.Text = string.Empty;
            txtBarrierSADescription.Text = string.Empty;
            txtBarrierDADescription.Text = string.Empty;
            txtBarrierTitle.Text = string.Empty;

            ddlNewBarrierParent.Items.Clear();
        }

        public void PopulateBarriers(DTO.Directorate directorate, DTO.Team team)
        {
            _directorate = directorate;
            _team = team;

            HttpContext.Current.Session["tasks_barriers_directorate"] = directorate;
            HttpContext.Current.Session["tasks_barriers_team"] = team;

            treeViewBarriers.Nodes.Clear();

            ClearDetailPanel();

            panelBarrierEditing.Enabled = false;

            // only SA users can add barriers before a directorate is selected
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                panelAddBarrier.Visible = true;
            else
                panelAddBarrier.Visible = false;

            rowApplyAll.Visible = false;

            try
            {
                // if directorate is null, populate the system level barriers
                if (directorate == null)
                {
                    PopulateBarrierTreeView(BLL.AdminManager.GetInstance().GetBarrierList(BLL.AdminManager.GetInstance().GetOrg(), false, null), null);

                    ddlNewBarrierParent.Items.Insert(0, "None");
                    ddlNewBarrierParent.Items[0].Value = "0";

                    TeamMode(false);
                }
                // if directorate is selected, but not team, display the system and directorate level barriers
                else if (directorate != null && team == null)
                {
                    PopulateBarrierTreeView(GetDirectorateBarriers(directorate, null), null);

                    TeamMode(false);
                    rowApplyAll.Visible = true;
                }
                // if the directorate and team are defined, go into ALM barrier assigning mode
                else if (directorate != null && team != null)
                {
                    PopulateBarrierTreeView(GetDirectorateBarriers(directorate, true), team);

                    TeamMode(true);
                }

                // hide both detail displays until a barrier is selected
                if (treeViewBarriers.SelectedNode == null)
                {
                    panelBarrierEditing.Visible = false;
                    panelBarrierReadOnly.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        private List<DTO.Barrier> GetDirectorateBarriers(DTO.Directorate directorate, bool? active)
        {
            DTO.Team directorateAsTeam = new DTO.Team();
            directorateAsTeam.Id = directorate.Id;            
            return BLL.AdminManager.GetInstance().GetBarrierList(directorateAsTeam, false, active);
        }

        private List<DTO.Barrier> GetAppliedBarriers(DTO.Team team)
        {
            return team == null ? null : BLL.AdminManager.GetInstance().GetBarrierList(team, true, true);
        }

        private bool CheckBarrierApplied(List<DTO.Barrier> appliedList, DTO.Barrier barrier)
        {
            return appliedList == null ? false : appliedList.Exists(x => x.Title == barrier.Title);
        }

        private void DisplayForSA()
        {
            panelAddBarrier.Visible = true;
            panelBarrierEditing.Visible = true;
            panelBarrierReadOnly.Visible = false;
        }

        private void DisplayForDA()
        {
            //if (treeViewBarriers.SelectedNode != null && treeViewBarriers.SelectedNode.Depth == 1)
            //{
                panelAddBarrier.Visible = true;
            //}
            //else
            //{
            //    panelAddBarrier.Visible = false;
            //}

                if (!_allowDAEdit)
                {
                    txtBarrierDADescription.Visible = true;
                    panelBarrierEditing.Visible = false;
                    panelBarrierReadOnly.Visible = true;
                }
                else 
                {
                    txtBarrierDADescription.Visible = false;
                    panelBarrierEditing.Visible = true;
                    panelBarrierReadOnly.Visible = false;
                }
            txtROBarrierDADescription.ReadOnly = false;
            txtROBarrierDADescription.BackColor = System.Drawing.Color.White;
        }

        private void DisplayForALM()
        {
            panelAddBarrier.Visible = false;
            panelBarrierEditing.Visible = false;
            panelBarrierReadOnly.Visible = true;
            txtROBarrierDADescription.ReadOnly = true;
        }

        private void CustomizeDisplayForUser()
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];

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

        private void TeamMode(bool enable)
        {
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_barriers_team"];
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];

            if (enable)
            {
                if (CanEdit(team, directorate))
                {
                    btnApplyBarriersToTeam.Visible = true;
                    treeViewBarriers.ShowCheckBoxes = TreeNodeTypes.All;
                }
                else
                {
                    btnApplyBarriersToTeam.Visible = false;
                    treeViewBarriers.ShowCheckBoxes = TreeNodeTypes.None;
                }

                DisplayForALM();

                txtROBarrierDADescription.Visible = true;
                txtROBarrierDADescription.ReadOnly = true;
                txtROBarrierDADescription.BackColor = System.Drawing.Color.LightGray;
                panelROSaveBarrierButton.Visible = false;
                lnkSelectAll.Visible = true;
            }
            else
            {
                treeViewBarriers.ShowCheckBoxes = TreeNodeTypes.None;

                btnApplyBarriersToTeam.Visible = false;
                lnkSelectAll.Visible = false;

                CustomizeDisplayForUser();
            }
        }

        private TreeNode NewBarrierNode(DTO.Barrier barrier)
        {
            TreeNode node = new TreeNode();
            node.Text = barrier.Code + ": " + barrier.Title;
            node.Value = barrier.Id.ToString();

            return node;
        }

        private void PopulateBarrierTreeView(List<DTO.Barrier> barriers, DTO.Team team)
        {
            List<DTO.Barrier> appliedBarriers = GetAppliedBarriers(team);

            foreach (DTO.Barrier barrier in barriers)
            {
                // top level (system) barrier

                TreeNode node = NewBarrierNode(barrier);
                node.Checked = CheckBarrierApplied(appliedBarriers, barrier);
                if (node.Checked)
                    node.Text = MakeBold(node.Text);
                node.Expanded = team == null ? false : true;

                foreach (DTO.Barrier child in barrier.Children)
                {
                    // second level (system) barrier
                    List<DTO.Barrier> appliedChildren = GetAppliedChildren(appliedBarriers, barrier);

                    TreeNode childNode = NewBarrierNode(child);
                    childNode.Checked = CheckBarrierApplied(appliedChildren, child);
                    if (childNode.Checked)
                        childNode.Text = MakeBold(childNode.Text);
                    childNode.Expanded = team == null ? false : true;

                    foreach (DTO.Barrier grandchild in child.Children)
                    {
                        // directorate level barrier
                        List<DTO.Barrier> appliedGrandchildren = GetAppliedChildren(appliedChildren, child);

                        TreeNode grandchildNode = NewBarrierNode(grandchild);
                        grandchildNode.Checked = CheckBarrierApplied(appliedGrandchildren, grandchild);
                        if (grandchildNode.Checked)
                            grandchildNode.Text = MakeBold(grandchildNode.Text);
                        childNode.ChildNodes.Add(grandchildNode);
                    }

                    node.ChildNodes.Add(childNode);

                    if (_directorate != null)
                    {
                        // also update the "new barrier" drop down list
                        ListItem barrierItem = new ListItem();
                        barrierItem.Text = child.Code + ": " + child.Title;
                        barrierItem.Value = child.Id.ToString();
                        ddlNewBarrierParent.Items.Add(barrierItem);
                    }
                }

                treeViewBarriers.Nodes.Add(node);

                if (_directorate == null)
                {
                    // also update the "new barrier" drop down list
                    ListItem barrierItem2 = new ListItem();
                    barrierItem2.Text = barrier.Code + ": " + barrier.Title;
                    barrierItem2.Value = barrier.Id.ToString();
                    ddlNewBarrierParent.Items.Add(barrierItem2);
                }
            }
        }

        private List<DTO.Barrier> GetAppliedChildren(List<DTO.Barrier> appliedBarriers, DTO.Barrier barrierToFind)
        {
            DTO.Barrier appliedFound = null;
            List<DTO.Barrier> appliedChildren = null;

            if (appliedBarriers != null)
            {
                if (barrierToFind != null)
                    appliedFound = appliedBarriers.Find(x => x.Id == barrierToFind.Id);

                if (appliedFound != null && appliedFound.Children != null)
                    appliedChildren = appliedFound.Children;
            }

            return appliedChildren;
        }

        private void AddBarrierChildrenToDDL(DTO.Barrier barrier, int level)
        {
            string indent = string.Empty;

            if (barrier.Children != null && barrier.Children.Count > 0)
            {
                for (int i = 0; i < level; i++)
                {
                    indent += "-";
                }

                foreach (DTO.Barrier child in barrier.Children)
                {
                    ListItem barrierItem = new ListItem();
                    barrierItem.Text = indent + " " + child.Code + ": " + child.Title;
                    barrierItem.Value = child.Id.ToString();
                    ddlNewBarrierParent.Items.Add(barrierItem);
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            treeViewBarriers.Attributes.Add("onclick", "postBackByObject()");
            lblApplyBarriersStatus.Visible = false;
            lblSaveBarrierStatus.Visible = false;
            lblROSaveBarrierStatus.Visible = false;

            if (!IsPostBack)
            {
                LoadData();

                DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_barriers_team"];

                bool teamSelected = team == null ? false : true;

                TeamMode(teamSelected);
            }

            // hide both detail displays until a barrier is selected
            if (treeViewBarriers.SelectedNode == null)
            {
                panelBarrierEditing.Visible = false;
                panelBarrierReadOnly.Visible = false;
            }
        }

        private void LoadData()
        {
            
        }

        protected void btnROSaveBarrier_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];

                long barrierId = long.Parse(treeViewBarriers.SelectedValue);

                DTO.Barrier barrier = BLL.AdminManager.GetInstance().GetBarrier(barrierId);
                
                // also save the DA description text if the user has directorate manage
                if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
                {
                    string descriptionText = string.Empty;

                    // if the user is an SA, the text will be in the normal text box, otherwise it's in the read only textbox
                    if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                        descriptionText = txtBarrierDADescription.Text;
                    else
                        descriptionText = txtROBarrierDADescription.Text;

                    if (directorate != null) 
                        BLL.AdminManager.GetInstance().SaveBarrierExtendedDescription(directorate.Id, barrier.Id, descriptionText);
                }

                lblSaveBarrierStatus.Text = "Saved!";
                lblROSaveBarrierStatus.Text = "Saved!";
                lblSaveBarrierStatus.ForeColor = System.Drawing.Color.Green;
                lblROSaveBarrierStatus.ForeColor = System.Drawing.Color.Green;
                lblSaveBarrierStatus.Visible = true;
                lblROSaveBarrierStatus.Visible = true;
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnSaveBarrier_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];
                DTO.Team directorateAsTeam;

                if (directorate == null)
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetOrg();
                else
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Name);

                long barrierId = long.Parse(treeViewBarriers.SelectedValue);

                DTO.Barrier barrier = BLL.AdminManager.GetInstance().GetBarrier(barrierId);
                barrier.Title = txtBarrierTitle.Text.Trim();
                barrier.Description = txtBarrierSADescription.Text.Trim();
                barrier.Active = cbBarrierActive.Checked;
                barrier.Code = txtBarrierCode.Text.Trim();

                BLL.AdminManager.GetInstance().SaveBarrier(directorateAsTeam, barrier, false);

                // also save the DA description text if the user has directorate manage
                if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
                {
                    string descriptionText = string.Empty;

                    // if the user is an SA, the text will be in the normal text box, otherwise it's in the read only textbox
                    if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                    {
                        descriptionText = txtBarrierDADescription.Text;
                    }
                    else
                    {
                        descriptionText = txtROBarrierDADescription.Text;
                    }

                    if (directorate != null)
                    {
                        BLL.AdminManager.GetInstance().SaveBarrierExtendedDescription(directorate.Id, barrier.Id, descriptionText);
                    }
                }

                lblSaveBarrierStatus.Text = "Saved!";
                lblROSaveBarrierStatus.Text = "Saved!";
                lblSaveBarrierStatus.ForeColor = System.Drawing.Color.Green;
                lblROSaveBarrierStatus.ForeColor = System.Drawing.Color.Green;
                lblSaveBarrierStatus.Visible = true;
                lblROSaveBarrierStatus.Visible = true;

                treeViewBarriers.SelectedNode.Text = barrier.Code + ": " + barrier.Title;
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnSaveNewBarrier_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];
                DTO.Team directorateAsTeam;

                if (directorate == null)
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetOrg();
                }
                else
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Name);
                }

                DTO.Barrier barrier = new DTO.Barrier();
                barrier.Title = txtNewBarrierTitle.Text;
                barrier.Description = txtNewBarrierDescription.Text;
                barrier.Active = cbNewBarrierActive.Checked;
                barrier.ParentId = long.Parse(ddlNewBarrierParent.SelectedValue);

                BLL.AdminManager.GetInstance().SaveBarrier(directorateAsTeam, barrier,
                    (directorate != null && chkApplyAll.Checked));

                txtNewBarrierDescription.Text = string.Empty;
                txtNewBarrierTitle.Text = string.Empty;
                cbNewBarrierActive.Checked = true;
                chkApplyAll.Checked = false;
                ddlNewBarrierParent.SelectedIndex = 0;

                PopulateBarriers(directorate, null);
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnCancelBarrierPopup_Click(object sender, EventArgs e)
        {
            txtNewBarrierDescription.Text = string.Empty;
            txtNewBarrierTitle.Text = string.Empty;

            if (ddlNewBarrierParent.Items.Count > 0)
            {
                ddlNewBarrierParent.SelectedIndex = 0;
            }
        }

        protected void barriers_SelectedNodeChanged(object sender, EventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_barriers_team"];

            TreeView treeViewBarriers = sender as TreeView;

            if (treeViewBarriers.SelectedNode == null)
            {
                //reload the page
                Response.Redirect(Request.RawUrl);
            }
            else
            {
                if (treeViewBarriers != null)
                {
                    panelBarrierEditing.Enabled = true;

                    TreeNode node = treeViewBarriers.SelectedNode;

                    DTO.Barrier barrier = BLL.AdminManager.GetInstance().GetBarrier(long.Parse(node.Value));

                    ListItem selectItem = ddlNewBarrierParent.Items.FindByValue(barrier.Id.ToString());
                    if (selectItem != null)
                    {
                        ddlNewBarrierParent.ClearSelection();
                        selectItem.Selected = true;
                    }

                    if (barrier != null)
                    {
                        SetActive(barrier.Active);

                        // read-only controls
                        lblROBarrierCode.Text = barrier.Code;
                        lblROBarrierTitle.Text = barrier.Title;
                        txtROBarrierSADescription.Text = barrier.Description;

                        // edit controls
                        txtBarrierCode.Text = barrier.Code;
                        txtBarrierTitle.Text = barrier.Title;
                        txtBarrierSADescription.Text = barrier.Description;

                        if (directorate != null)
                        {
                            string extendedBarrierDescription = BLL.AdminManager.GetInstance().GetBarrierExtendedDescription(directorate.Id, barrier.Id);

                            txtROBarrierDADescription.Text = extendedBarrierDescription;
                            txtBarrierDADescription.Text = extendedBarrierDescription;

                            txtROBarrierDADescription.Visible = true;
                            txtBarrierDADescription.Visible = true;

                            if (BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate))
                            {
                                panelROSaveBarrierButton.Visible = true;

                                // if the selected barrier is 3 then allow edit
                                if (BLL.AdminManager.GetInstance().GetBarrierLevel(barrier.Id) == 2)
                                {
                                    _allowDAEdit = true;
                                }                               
                            }
                        }
                        else
                        {
                            txtROBarrierDADescription.Visible = false;
                            txtBarrierDADescription.Visible = false;
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
                cbBarrierActive.Checked = true;
                btnBarrierActive.Text = "Active";
                lblROBarrierActive.Text = "Active";
                btnBarrierActive.BackColor = System.Drawing.Color.LightGreen;
                lblROBarrierActive.BackColor = System.Drawing.Color.LightGreen;
            }
            else
            {
                cbBarrierActive.Checked = false;
                btnBarrierActive.Text = "Inactive";
                lblROBarrierActive.Text = "Inactive";
                btnBarrierActive.BackColor = System.Drawing.Color.PaleVioletRed;
                lblROBarrierActive.BackColor = System.Drawing.Color.PaleVioletRed;
            }
        }

        protected void btnBarrierActive_Click(object sender, EventArgs e)
        {
            if (treeViewBarriers.SelectedNode != null)
                SetActive(!cbBarrierActive.Checked);
        }

        protected void barriers_TreeNodeCheckChanged(object sender, TreeNodeEventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];

            TreeView treeViewBarriers = sender as TreeView;

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

        protected void btnApplyBarriersToTeam_Click(object sender, EventArgs e)
        {
            lblApplyBarriersStatus.Visible = false;

            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_barriers_team"];
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_barriers_directorate"];

            List<DTO.Barrier> barrierList = new List<DTO.Barrier>();

            // loop through the treeview, keep track of the barrier IDs that are selected
            foreach (TreeNode node in treeViewBarriers.Nodes)
            {
                ApplyBarrierList(ref barrierList, node);
            }

            team.SelectedBarriers = barrierList;

            BLL.AdminManager.GetInstance().ApplyBarriers(team);

            HttpContext.Current.Session["tasks_barriers_team"] = team;

            PopulateBarriers(directorate, team);

            lblApplyBarriersStatus.Visible = true;
        }

        private void ApplyBarrierList(ref List<DTO.Barrier> barrierList, TreeNode node)
        {
            if (node.Checked)
            {
                barrierList.Add(BLL.AdminManager.GetInstance().GetBarrier(long.Parse(node.Value)));
            }

            foreach (TreeNode child in node.ChildNodes)
            {
                ApplyBarrierList(ref barrierList, child);
            }
        }

        protected void lnkSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeViewBarriers.Nodes)
            {
                CheckNode(node, true);
            }
        }
    }
}