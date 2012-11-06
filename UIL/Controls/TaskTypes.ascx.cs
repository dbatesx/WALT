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
    public partial class TaskTypes : System.Web.UI.UserControl
    {
        DTO.Directorate _directorate;
        DTO.Team _team;     

        private void ClearDetailPanel()
        {
            btnTaskTypeActive.Text = string.Empty;
            btnTaskTypeActive.BackColor = System.Drawing.Color.LightGray;
            lblROTaskTypeActive.Text = string.Empty;
            lblROTaskTypeActive.BackColor = System.Drawing.Color.LightGray;
            txtTaskTypeDescription.Text = string.Empty;
            txtTaskTypeTitle.Text = string.Empty;

            ddlNewTaskTypeParent.Items.Clear();
            ddParent.Items.Clear();
            //BindComplexities(!BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TASK_MANAGE));
        }

        public void PopulateTaskTypes(DTO.Directorate directorate, DTO.Team team, long? selId)
        {
            _directorate = directorate;
            _team = team;

            HttpContext.Current.Session["tasks_taskTypes_directorate"] = directorate;
            HttpContext.Current.Session["tasks_taskTypes_team"] = team;

            treeViewTaskTypes.Nodes.Clear();

            ClearDetailPanel();

            panelTaskTypeEditing.Enabled = false;

            // only SA users can add top level task types
            btnAddTaskType.Visible = BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE);
            btnImport.Visible = btnAddTaskType.Visible;

            rowApplyAll.Visible = false;
            complexityTbl.Visible = true;

            try
            {
                // if directorate is null, populate the system level taskTypes
                if (directorate == null)
                {
                    PopulateTaskTypeTreeView(BLL.AdminManager.GetInstance().GetTaskTypeList(BLL.AdminManager.GetInstance().GetOrg(), false, false, null), null, selId);

                    TeamMode(false);
                    complexityTbl.Visible = false;
                }
                // if directorate is selected, but not team, display the system and directorate level task types
                else if (directorate != null && team == null)
                {
                    PopulateTaskTypeTreeView(GetDirectorateTaskTypes(directorate, null), null, selId);

                    TeamMode(false);                    
                    rowApplyAll.Visible = true;
                }
                // if the directorate and team are defined, go into ALM taskType assigning mode (team mode)
                else if (directorate != null && team != null)
                {
                    PopulateTaskTypeTreeView(GetDirectorateTaskTypes(directorate, true), team, selId);

                    TeamMode(true);
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        private List<DTO.TaskType> GetDirectorateTaskTypes(DTO.Directorate directorate, bool? active)
        {
            DTO.Team directorateAsTeam = new DTO.Team();
            directorateAsTeam.Id = directorate.Id;

            return BLL.AdminManager.GetInstance().GetTaskTypeList(directorateAsTeam, false, false, active);
        }

        private List<DTO.TaskType> GetAppliedTaskTypes(DTO.Team team)
        {
            return team == null ? null : BLL.AdminManager.GetInstance().GetTaskTypeList(team, false, true, true);
        }

        private bool CheckTaskTypeApplied(List<DTO.TaskType> appliedList, DTO.TaskType taskType)
        {
            return appliedList == null ? false : appliedList.Exists(x => x.Title == taskType.Title);
        }

        private void BindComplexities(DTO.Directorate directorate, bool readOnly)
        {
            if (directorate == null)
            {
                complexityTbl.Visible = false;
                return;
            }

            List<DTO.Complexity> complexityList = new List<DTO.Complexity>();
            long taskTypeId = 0;

            if (treeViewTaskTypes.SelectedNode != null && long.TryParse(treeViewTaskTypes.SelectedValue, out taskTypeId))
            {
                DTO.TaskType taskType = BLL.AdminManager.GetInstance().GetTaskType(taskTypeId, directorate.Id);

                if (taskType.Children.Count == 0)
                {
                    complexityTbl.Visible = true;
                    complexityList = new List<DTO.Complexity>();
                    Dictionary<int, DTO.Complexity> compMap = new Dictionary<int,DTO.Complexity>();

                    foreach (DTO.Complexity comp in taskType.Complexities)
                    {
                        compMap[comp.SortOrder] = comp;
                    }

                    int maxComplexityLevel = 6;

                    if (ConfigurationManager.AppSettings["MaxComplexityLevels"] != null &&
                       ConfigurationManager.AppSettings["MaxComplexityLevels"].Length > 0 &&
                       int.TryParse(ConfigurationManager.AppSettings["MaxComplexityLevels"], out maxComplexityLevel))
                    {
                    }

                    for (int i = 1; i <= maxComplexityLevel; i++)
                    {
                        if (compMap.ContainsKey(i))
                        {
                            complexityList.Add(compMap[i]);
                        }
                        else
                        {
                            DTO.Complexity newComp = new DTO.Complexity();
                            newComp.Title = "Level " + i.ToString();
                            newComp.Hours = 0;
                            newComp.Active = false;
                            complexityList.Add(newComp);
                        }
                    }

                    rptComplexity.DataSource = complexityList;
                    rptComplexity.DataBind();

                    bool showTextBox = true;
                    bool showLabel = false;

                    if (readOnly)
                    {
                        showTextBox = false;
                        showLabel = true;
                        btnSaveComplexityCode.Visible = false;
                    }

                    foreach (RepeaterItem compItem in rptComplexity.Items)
                    {
                        TextBox hrsText = (TextBox)compItem.FindControl("txtComplexityHrs");
                        hrsText.Visible = showTextBox;

                        Label hrsLabel = (Label)compItem.FindControl("lblComplexityHrs");
                        hrsLabel.Visible = showLabel;
                    }
                }
                else
                {
                    complexityTbl.Visible = false;
                }
            }

            else
            {
                complexityTbl.Visible = false;
            }

            upanelComplexityCode.Update();
        }

        protected string GetId(object obj)
        {
            string val = string.Empty;

            DTO.Complexity complex = (DTO.Complexity)obj;
            val = complex.Id.ToString();

            return val;
        }

        private bool CanEdit(DTO.Team team, DTO.Directorate directorate)
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE) ||
                (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.TEAM_MANAGE) &&
                 BLL.AdminManager.GetInstance().IsTeamAdmin(team)) ||
                (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) &&
                 BLL.AdminManager.GetInstance().IsDirectorateAdmin(directorate)))
            {
                return true;
            }

            return false;
        }

        private void TeamMode(bool enable)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_taskTypes_team"];

            panelTaskTypeEditing.Enabled = false;
            panelSaveTaskTypeButton.Visible = false;
            panelComplexEditing.Enabled = false;
            rowAddComplexityCode.Visible = false;
            rowParent.Visible = false;

            // if we're going into team mode, the only interaction is the checkboxes and the "apply" button
            if (enable)
            {
                btnAddTaskType.Visible = false;
                btnImport.Visible = false;

                // if user is an SA, or an ALM, let them apply task types to the team
                if (CanEdit(team, directorate))
                {
                    btnApplyTaskTypesToTeam.Visible = true;
                    treeViewTaskTypes.ShowCheckBoxes = TreeNodeTypes.All;
                }
                else
                {
                    btnApplyTaskTypesToTeam.Visible = false;
                    treeViewTaskTypes.ShowCheckBoxes = TreeNodeTypes.None;
                }

                btnTaskTypeActive.Visible = false;
                lblROTaskTypeActive.Visible = true;
                txtTaskTypeTitle.ReadOnly = true;
                txtTaskTypeDescription.ReadOnly = true;
                lnkSelectAll.Visible = true;

                BindComplexities(directorate, true);              
            }
            else
            {
                treeViewTaskTypes.ShowCheckBoxes = TreeNodeTypes.None;
                lnkSelectAll.Visible = false;

                btnApplyTaskTypesToTeam.Visible = false;

                // if the user is an SA, they can edit anything, as can a DA if the task is theirs
                if ((BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                     || (directorate != null
                         && BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN)
                         && directorate.Admins.Find(x => x.Username == BLL.ProfileManager.GetInstance().GetProfile().Username) != null))
                {
                    btnAddTaskType.Visible = true;
                    btnImport.Visible = true;
                    btnTaskTypeActive.Visible = true;
                    lblROTaskTypeActive.Visible = false;
                    txtTaskTypeTitle.ReadOnly = false;
                    txtTaskTypeDescription.ReadOnly = false;

                    if (treeViewTaskTypes.SelectedNode != null)
                    {
                        panelTaskTypeEditing.Enabled = true;
                        panelSaveTaskTypeButton.Visible = true;
                        panelComplexEditing.Enabled = true;
                        rowAddComplexityCode.Visible = true;
                        rowParent.Visible = true;
                    }
                    
                    BindComplexities(directorate, false);
                }
                else
                {
                    btnAddTaskType.Visible = false;
                    btnImport.Visible = false;
                    btnTaskTypeActive.Visible = false;
                    lblROTaskTypeActive.Visible = true;
                    txtTaskTypeTitle.ReadOnly = true;
                    txtTaskTypeDescription.ReadOnly = true;

                    BindComplexities(directorate, true);                
                }
            }
        }

        private TreeNode NewTaskTypeNode(DTO.TaskType type, long? selId)
        {
            TreeNode node = new TreeNode();
            node.Text = type.Title;
            node.Value = type.Id.ToString();
            node.Selected = selId.HasValue && type.Id == selId.Value;

            return node;
        }

        private void PopulateTaskTypeTreeView(List<DTO.TaskType> taskTypes, DTO.Team team, long? selId)
        {
            if (team == null)
            {
                ddlNewTaskTypeParent.Items.Clear();
                ddlNewTaskTypeParent.Items.Add(new ListItem("None", "0"));
            }

            List<DTO.TaskType> appliedTaskTypes = GetAppliedTaskTypes(team);

            foreach (DTO.TaskType taskType in taskTypes)
            {
                // top level taskType

                TreeNode node = NewTaskTypeNode(taskType, selId);
                node.Checked = CheckTaskTypeApplied(appliedTaskTypes, taskType);

                if (node.Checked)
                {
                    node.Text = MakeBold(node.Text);
                }

                node.Expanded = team == null ? false : true;

                foreach (DTO.TaskType child in taskType.Children)
                {
                    // second level taskType
                    List<DTO.TaskType> appliedChildren = GetAppliedChildren(appliedTaskTypes, taskType);
                    TreeNode childNode = NewTaskTypeNode(child, selId);
                    childNode.Checked = CheckTaskTypeApplied(appliedChildren, child);
                    
                    if (childNode.Checked)
                    {
                        childNode.Text = MakeBold(childNode.Text);
                    }

                    childNode.Expanded = team == null ? false : true;
                    node.ChildNodes.Add(childNode);

                    if (childNode.Selected)
                    {
                        node.Expanded = true;
                    }
                }

                treeViewTaskTypes.Nodes.Add(node);

                if (team == null)
                {
                    // also add to the "new taskType" parent drop down list
                    ddlNewTaskTypeParent.Items.Add(new ListItem(taskType.Title, taskType.Id.ToString()));
                }
            }

            if (selId.HasValue)
            {
                taskTypes_SelectedNodeChanged(null, null);
            }
        }

        private List<DTO.TaskType> GetAppliedChildren(List<DTO.TaskType> appliedTaskTypes, DTO.TaskType taskTypeToFind)
        {
            DTO.TaskType appliedFound = null;
            List<DTO.TaskType> appliedChildren = null;

            if (appliedTaskTypes != null)
            {
                if (taskTypeToFind != null)
                    appliedFound = appliedTaskTypes.Find(x => x.Id == taskTypeToFind.Id);

                if (appliedFound != null && appliedFound.Children != null)
                    appliedChildren = appliedFound.Children;
            }

            return appliedChildren;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            treeViewTaskTypes.Attributes.Add("onclick", "postBackByObject()");
            lblSaveTaskTypeStatus.Visible = false;

            if (!IsPostBack)
            {
                LoadData();
                
                DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_taskTypes_team"];

                bool teamSelected = team == null ? false : true;

                TeamMode(teamSelected);
            }

            
        }

        private void LoadData()
        {

        }

        protected void btnSaveNewTaskType_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];
                DTO.Team directorateAsTeam;

                if (directorate == null)
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetOrg();
                }
                else
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Name);
                }

                DTO.TaskType taskType = new DTO.TaskType();
                taskType.Title = txtNewTaskTypeTitle.Text;
                taskType.Description = txtNewTaskTypeDescription.Text;
                taskType.Active = cbNewTaskTypeActive.Checked;
                taskType.ParentId = long.Parse(ddlNewTaskTypeParent.SelectedValue);

                BLL.AdminManager.GetInstance().SaveTaskType(directorateAsTeam, taskType,
                    (directorate != null && chkApplyAll.Checked));

                txtNewTaskTypeTitle.Text = string.Empty;
                txtNewTaskTypeDescription.Text = string.Empty;
                cbNewTaskTypeActive.Checked = true;
                chkApplyAll.Checked = false;
                ddlNewTaskTypeParent.SelectedIndex = 0;

                PopulateTaskTypes(directorate, null, taskType.Id);
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnSaveTaskType_Click(object sender, EventArgs e)
        {
            DTO.Directorate directorate = null;

            try
            {
                directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];
                DTO.Team directorateAsTeam;

                if (directorate == null)
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetOrg();
                }
                else
                {
                    directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Name);
                }

                long taskId = long.Parse(treeViewTaskTypes.SelectedValue);

                DTO.TaskType taskType = BLL.AdminManager.GetInstance().GetTaskType(taskId);
                taskType.Title = txtTaskTypeTitle.Text.Trim();
                taskType.Description = txtTaskTypeDescription.Text.Replace(Environment.NewLine, "<br />"); 
                taskType.Active = cbTaskTypeActive.Checked;
                taskType.ParentId = Convert.ToInt64(ddParent.SelectedValue);
                
                BLL.AdminManager.GetInstance().SaveTaskType(directorateAsTeam, taskType, false);

                lblSaveTaskTypeStatus.Text = "Saved!";
                lblSaveTaskTypeStatus.ForeColor = System.Drawing.Color.Green;
                lblSaveTaskTypeStatus.Visible = true;

                PopulateTaskTypes(directorate, null, taskId);
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void btnCancelTaskTypePopup_Click(object sender, EventArgs e)
        {
            txtNewTaskTypeDescription.Text = string.Empty;
            txtNewTaskTypeTitle.Text = string.Empty;

            ddlNewTaskTypeParent.SelectedIndex = 0;
        }

        protected void taskTypes_SelectedNodeChanged(object sender, EventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_taskTypes_team"];

            if (treeViewTaskTypes.SelectedNode == null)
            {
                //reload the page
                Response.Redirect(Request.RawUrl);
            }
            else
            {
                if (team != null)
                {
                    TeamMode(true);
                }
                else
                {
                    TeamMode(false);
                }

                if (treeViewTaskTypes != null)
                {
                    TreeNode node = treeViewTaskTypes.SelectedNode;
                    DTO.TaskType taskType = BLL.AdminManager.GetInstance().GetTaskType(long.Parse(node.Value));
                    ListItem selectItem = ddlNewTaskTypeParent.Items.FindByValue(taskType.Id.ToString());

                    if (selectItem != null)
                    {
                        ddlNewTaskTypeParent.ClearSelection();
                        selectItem.Selected = true;
                    }

                    if (taskType != null)
                    {
                        SetActive(taskType.Active);
                        txtTaskTypeTitle.Text = taskType.Title;

                        if (taskType.Description != null)
                        {
                            txtTaskTypeDescription.Text = taskType.Description.Replace(Environment.NewLine, "<br />");
                        }

                        if (directorate != null && team == null && taskType.TeamId != directorate.Id)
                        {
                            panelTaskTypeEditing.Enabled = false;
                            panelSaveTaskTypeButton.Visible = false;
                        }

                        //BindComplexities(true);
                    }

                    if (team == null)
                    {
                        ddParent.Items.Clear();

                        if (node.ChildNodes.Count > 0)
                        {
                            ddParent.Items.Add(new ListItem("None", "0"));
                        }
                        else
                        {
                            foreach (ListItem op in ddlNewTaskTypeParent.Items)
                            {
                                if (op.Value != node.Value)
                                {
                                    ddParent.Items.Add(new ListItem(op.Text, op.Value));
                                }
                            }
                        }

                        if (node.Parent == null)
                        {
                            ddParent.SelectedValue = "0";
                        }
                        else
                        {
                            ddParent.SelectedValue = node.Parent.Value;
                        }
                    }
                }
            }
        }

        private void SetActive(bool active)
        {
            if (active)
            {
                cbTaskTypeActive.Checked = true;
                btnTaskTypeActive.Text = "Active";
                lblROTaskTypeActive.Text = "Active";
                btnTaskTypeActive.BackColor = System.Drawing.Color.LightGreen;
                lblROTaskTypeActive.BackColor = System.Drawing.Color.LightGreen;
            }
            else
            {
                cbTaskTypeActive.Checked = false;
                btnTaskTypeActive.Text = "Inactive";
                lblROTaskTypeActive.Text = "Inactive";
                btnTaskTypeActive.BackColor = System.Drawing.Color.PaleVioletRed;
                lblROTaskTypeActive.BackColor = System.Drawing.Color.PaleVioletRed;
            }
        }

        protected void btnTaskTypeActive_Click(object sender, EventArgs e)
        {
            if (treeViewTaskTypes.SelectedNode != null)
            {
                SetActive(!cbTaskTypeActive.Checked);
            }
        }

        protected void taskTypes_TreeNodeCheckChanged(object sender, TreeNodeEventArgs e)
        {
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];

            TreeView treeViewTaskTypes = sender as TreeView;

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

        protected void btnApplyTaskTypesToTeam_Click(object sender, EventArgs e)
        {
            DTO.Team team = (DTO.Team)HttpContext.Current.Session["tasks_taskTypes_team"];
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];

            List<DTO.TaskType> taskTypeList = new List<DTO.TaskType>();

            // loop through the treeview, keep track of the taskType IDs that are selected
            foreach (TreeNode node in treeViewTaskTypes.Nodes)
            {
                ApplyTaskTypeList(ref taskTypeList, node);
            }

            team.SelectedTaskTypes = taskTypeList;

            BLL.AdminManager.GetInstance().ApplyTaskTypes(team);

            HttpContext.Current.Session["tasks_taskTypes_team"] = team;

            PopulateTaskTypes(directorate, team, null);
        }

        private void ApplyTaskTypeList(ref List<DTO.TaskType> taskTypeList, TreeNode node)
        {
            if (node.Checked)
            {
                taskTypeList.Add(BLL.AdminManager.GetInstance().GetTaskType(long.Parse(node.Value)));
            }

            foreach (TreeNode child in node.ChildNodes)
            {
                ApplyTaskTypeList(ref taskTypeList, child);
            }
        }

        protected void btnSaveComplexityCode_Click(object sender, EventArgs e)
        {           
            DTO.Directorate directorate = (DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"];

            if (directorate == null)
            {
                return;
            }

            DTO.Team directorateAsTeam = BLL.AdminManager.GetInstance().GetTeam(directorate.Id, false);
            long taskTypeId = long.Parse(treeViewTaskTypes.SelectedValue);
            DTO.TaskType taskType = BLL.AdminManager.GetInstance().GetTaskType(taskTypeId);

            try
            {
                for (int i = 0; i < rptComplexity.Items.Count; i++)
                {
                    TextBox txt = rptComplexity.Items[i].FindControl("txtComplexityHrs") as TextBox;
                    HiddenField hdn = rptComplexity.Items[i].FindControl("hdnComplexityId") as HiddenField;
                    Label lbl = rptComplexity.Items[i].FindControl("lblComplexityTitle") as Label;

                    if (txt != null)
                    {
                        //Existing Complexity Code
                        if (hdn.Value != null && !hdn.Value.Equals("0"))
                        {
                            long complexId = 0;
                            double hrs = 0;

                            if (long.TryParse(hdn.Value, out complexId) && (double.TryParse(txt.Text, out hrs) || string.IsNullOrEmpty(txt.Text)))
                            {
                                DTO.Complexity oldComplexity = BLL.AdminManager.GetInstance().GetComplexityCode(complexId);
                                
                                oldComplexity.Title = lbl.Text;

                                if (double.TryParse(txt.Text, out hrs))
                                {
                                    oldComplexity.Hours = double.Parse(txt.Text);
                                }
                                else 
                                {
                                    oldComplexity.Hours = 0;
                                }

                                if (hrs == 0 || string.IsNullOrEmpty(txt.Text))
                                {
                                    oldComplexity.Active = false;
                                }
                                else 
                                {
                                    oldComplexity.Active = true;
                                }

                                if (lbl.Text.Contains("Level"))
                                {
                                    string sortStr = lbl.Text.Substring(lbl.Text.LastIndexOf("Level") + 6);
                                    int sortOrder = 1;
                                    int.TryParse(sortStr, out sortOrder);
                                    oldComplexity.SortOrder = sortOrder;
                                }

                                try
                                {
                                    BLL.AdminManager.GetInstance().SaveComplexityCode(directorateAsTeam, taskType, oldComplexity);
                                    Utility.DisplayInfoMessage("Your data has been successfully saved");
                                }
                                catch (Exception ex)
                                {
                                    Utility.DisplayException(ex);
                                }
                            }
                            else 
                            {
                                Utility.DisplayErrorMessage("Please enter a valid hour");
                            }

                        }                           
                        //New Complexity Code
                        else if (double.Parse(txt.Text) > 0)
                        {
                            DTO.Complexity newComplexity = new DTO.Complexity();

                            newComplexity.Title = lbl.Text;
                            newComplexity.Hours = double.Parse(txt.Text);
                            newComplexity.Active = true;

                            if (lbl.Text.Contains("Level"))
                            {
                                string sortStr = lbl.Text.Substring(lbl.Text.LastIndexOf("Level") + 6);
                                int sortOrder = 1;
                                int.TryParse(sortStr, out sortOrder);
                                newComplexity.SortOrder = sortOrder;
                            }

                            try
                            {
                                BLL.AdminManager.GetInstance().AddTaskTypeComplexity(directorateAsTeam, taskType, newComplexity);
                                Utility.DisplayInfoMessage("Your data has been successfully saved");
                            }
                            catch (Exception ex)
                            {
                                Utility.DisplayException(ex);
                            }
                        }

                    }

                }

                BindComplexities(directorate, false);
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void rptComplexity_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DTO.Complexity drv = (DTO.Complexity)e.Item.DataItem;
                if (drv.Active)
                {
                    Panel pnlWrapper = (Panel)e.Item.FindControl("pnlItemWrapper");
                    pnlWrapper.BackColor = System.Drawing.ColorTranslator.FromHtml("#ccffcc");
                }
                else 
                {
                    Panel pnlWrapper = (Panel)e.Item.FindControl("pnlItemWrapper");
                    pnlWrapper.BackColor = System.Drawing.ColorTranslator.FromHtml("#ffcccc");
                }
            }
        }

        protected void lnkSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeViewTaskTypes.Nodes)
            {
                CheckNode(node, true);
            }
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
            string dirID = "all";

            if (HttpContext.Current.Session["tasks_taskTypes_directorate"] != null)
            {
                dirID = ((DTO.Directorate)HttpContext.Current.Session["tasks_taskTypes_directorate"]).Id.ToString();
            }
            
            Response.Redirect("/Admin/ImportTaskTypes.aspx?dirID=" + dirID);
        }
    }
}