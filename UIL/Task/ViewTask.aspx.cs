using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WALT.UIL.Task
{
    public partial class ViewTask : System.Web.UI.Page
    {      
        private DTO.Profile _profile;
        private DTO.Task _task;
        private BLL.TaskManager.PermissionType currPermission;
        bool _error = false;
        string _errorMsg = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            _profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();

            string javaScriptFunction = @"
                    $(function () {   
                     
                        if( $('.SelectedNode').length > 0){                      
                            var treeDivHeight =$('#TaskTreeViewDiv').height() /2;
                            var treeDivTop = $('#TaskTreeViewDiv').position().top;
                            var selectPosTop = $('.SelectedNode').position().top;
                            var scrolToTop =selectPosTop  - treeDivHeight - treeDivTop;
                                     
                            $('#TaskTreeViewDiv').scrollTop(scrolToTop);                         
                        }     
   
                    });";
            AjaxControlToolkit.ToolkitScriptManager.RegisterStartupScript(this, this.GetType(), "viewTaskScript", javaScriptFunction, true);

            long _taskID;

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["id"]))
            {
                string tempTaskID = HttpContext.Current.Request.Params["id"];

                if (long.TryParse(tempTaskID, out _taskID))
                {
                    _task = BLL.TaskManager.GetInstance().GetTask(_taskID, false);//true);
                    currPermission = BLL.TaskManager.GetInstance().GetTaskPermission(_task);
                }
                else
                {
                    _error = true;
                    _errorMsg = "Task ID was not Recognized";                    
                }
            }
            else
            {
                _error = true;
                _errorMsg = "No Task ID!";                
            }


            if (!Page.IsPostBack && !_error)
            {
                if (_task != null)
                {
                    DTO.Task parentTask = _task;

                    if (_task.ParentId != 0)
                    {
                        parentTask = WALT.BLL.TaskManager.GetInstance().GetRootTask(_task, _profile);
                    }
                    else 
                    {
                        parentTask = WALT.BLL.TaskManager.GetInstance().GetTask(_task.Id, true, true);
                    }

                    TreeView1.Nodes.Add(CreateNode(parentTask, _task));

                    LoadTaskForView();
                }
                else
                {
                    _error = true;
                    _errorMsg = "Task ID was not Recognized";
                }
            }
            else 
            {
                ViewFormPlaceHolder.Visible = false;
                Utility.DisplayErrorMessage(_errorMsg);
            }

        }

        private void LoadTaskForView()
        {   
            if (currPermission != BLL.TaskManager.PermissionType.None) 
            {
                string source = string.Empty;

                if (HttpContext.Current.Request["Source"] != null)
                {
                    source = "&Source=" + HttpContext.Current.Request["Source"];
                }

                EditButton1.PostBackUrl = "TaskForm.aspx?Id=" + TreeView1.SelectedNode.Value + source;
                EditButton2.PostBackUrl = EditButton1.PostBackUrl;

                EditButton1.Visible = true;
                EditButton2.Visible = true;
            }            
            else
            {
                EditButton1.PostBackUrl = "";
                EditButton2.PostBackUrl = "";

                EditButton1.Visible = false;
                EditButton2.Visible = false;
            }

            bool inPlan = WALT.BLL.TaskManager.GetInstance().IsTaskPlanned(_task);

            if (!inPlan && _task.Status == DTO.Task.StatusEnum.OPEN &&
                (currPermission == BLL.TaskManager.PermissionType.Both ||
                 currPermission == BLL.TaskManager.PermissionType.Assignee))
            {
                AddChildButton1.PostBackUrl = "TaskForm.aspx?ParentId=" + _task.Id.ToString();
                AddChildButton2.PostBackUrl = "TaskForm.aspx?ParentId=" + _task.Id.ToString();
                AddChildButton1.Visible = true;
                AddChildButton2.Visible = true;
            }
            else
            {
                AddChildButton1.PostBackUrl = "";
                AddChildButton2.PostBackUrl = "";

                AddChildButton1.Visible = false;
                AddChildButton2.Visible = false;
            }

            if (BLL.TaskManager.GetInstance().IsTaskAllowedToBeRejected(_task, currPermission))
            {
                RejectButton1.Visible = true;
                RejectButton2.Visible = true;
            }
            else 
            {
                RejectButton1.Visible = false;
                RejectButton2.Visible = false;
            }

            if (BLL.TaskManager.GetInstance().IsTaskAllowedToBeDeleted(_task, currPermission))
            {
                DeleteButton1.Visible = true;
                DeleteButton2.Visible = true;
            }
            else
            {
                DeleteButton1.Visible = false;
                DeleteButton2.Visible = false;
            }
            
            if (!string.IsNullOrEmpty(_task.Title))
            {               
                titleLabel.Text = _task.Title;
            }

            if (_task.ParentId > 0 && _task.ParentId != _task.Id)
            {
                parentPlaceHolder.Visible = true;
                parentStaticLabel.Text = BLL.TaskManager.GetInstance().GetTask(_task.ParentId, false, true).Title;
            }

            if (!_task.BaseTask)
            {
                fullyAllocatedPlaceHolder.Visible = true;
                fullyAllocatedLabel.Text = _task.FullyAllocated.ToString();
            }

            if (!string.IsNullOrEmpty(_task.Program.Title))
                programLabel.Text = _task.Program.Title;

            if (!string.IsNullOrEmpty(_task.WBS))
                        WBSLabel.Text = _task.WBS;

            if (!string.IsNullOrEmpty(_task.Status.ToString()))
                StatusLabel.Text = Utility.UppercaseFirst(_task.Status.ToString());
            
            if (_task.Hours > 0)
                hrsTextBox.Text = _task.Hours.ToString();

            hrsSpendLabel.Text = _task.Spent.ToString();

            if (!_task.BaseTask)
            {
                hrsTextBox.Text += " (" + _task.HoursAllocatedToChildren.ToString() + " Hours Allocated To Children)";
            }

            if ( _task.Created != null)
            {
                CreatedLabel.Text = Utility.ConvertToLocal(_task.Created).ToString();
            }

            if (_task.Status == DTO.Task.StatusEnum.COMPLETED && _task.CompletedDate != null)
            {
                CompletedPlaceHolder.Visible = true;
                CompletedLabel.Text = Utility.ConvertToLocal(_task.CompletedDate.Value).ToString();
            }
            else if (_task.Status == DTO.Task.StatusEnum.HOLD && _task.OnHoldDate != null)
            {
                OnHoldPlaceHolder.Visible = true;
                OnHoldLabel.Text = Utility.ConvertToLocal(_task.OnHoldDate.Value).ToString();
            }

            if (_task.StartDate != null)
                ReqStartLabel.Text = _task.StartDate.Value.ToShortDateString();

            if (_task.DueDate != null)
                DueDateLabel.Text = _task.DueDate.Value.ToShortDateString();

            if (!string.IsNullOrEmpty(_task.OriginatorDisplayName))
                CreatedByLabel.Text = _task.OriginatorDisplayName;

            if (!string.IsNullOrEmpty(_task.AssigneeDisplayName))
                assigneeLabel.Text =_task.AssigneeDisplayName;

            if (!string.IsNullOrEmpty(_task.OriginatorDisplayName))
                OwnerLabel.Text = _task.OwnerDisplayName;
            
            if (_task.TaskType != null)
                TaskTypeLabel.Text = _task.TaskType.Title;

            if (_task.Estimate > 0)
            {
                RELabel.Text = _task.Estimate.ToString();
                REPlaceHolder.Visible = true;
            }

            if (_task.Complexity != null)
            {
                ComplexityLabel.Text = _task.Complexity.Title;
                ComplexityPlaceHolder.Visible = true;
            }

            if (!string.IsNullOrEmpty(_task.OwnerComments))
                OwnerCommentsLabel.Text = _task.OwnerComments.Replace(Environment.NewLine, "<br />"); 

            if (!string.IsNullOrEmpty(_task.AssigneeComments))
                AssigneeCommentsLabel.Text = _task.AssigneeComments.Replace(Environment.NewLine, "<br />"); 

            if (!string.IsNullOrEmpty(_task.ExitCriteria))
                ExitCritriaTextBox.Text = _task.ExitCriteria.Replace(Environment.NewLine, "<br />"); 
        }

        protected void TreeView1_PreRender(object sender, EventArgs e)
        {
            if (TreeView1.SelectedNode != null)
            {
                PreLink.NavigateUrl = MoveToPrevNode(TreeView1.SelectedNode);
                NextLink.NavigateUrl = MoveToNextNode(TreeView1.SelectedNode);

                if (TreeView1.Nodes.IndexOf(TreeView1.SelectedNode) == -1)
                {
                    PreLink.Enabled = true;                   

                    int Nodecount = TreeView1.Nodes.Count;
                    TreeNode lastNode = new TreeNode();

                    if (Nodecount > 0)
                    {
                        lastNode = TreeView1.Nodes[Nodecount - 1];

                        while (lastNode.ChildNodes.Count > 0)
                        {
                            lastNode = lastNode.ChildNodes[lastNode.ChildNodes.Count - 1];
                        }
                    }

                    if (TreeView1.SelectedNode == lastNode)
                    {
                        NextLink.Enabled = false;                      
                    }
                    else
                        NextLink.Enabled = true;
                }
                else
                {
                    if (TreeView1.Nodes.IndexOf(TreeView1.SelectedNode) == 0)
                        PreLink.Enabled = false;
                    else
                        PreLink.Enabled = true;
                }              


                if (TreeView1.SelectedNode.Parent != null)
                {
                    TreeNode parentNode = TreeView1.SelectedNode.Parent;

                    while (parentNode != null)
                    {
                        parentNode.Expanded = true;

                        parentNode = parentNode.Parent;
                    }

                }
            }
        }

        private TreeNode CreateNode(DTO.Task task, DTO.Task selected)
        {
            TreeNode node = CreateNode(task.Id.ToString(), task.Title, task.Children.Count);

            if (selected != null && selected.Id == task.Id)
            {               
                node.Selected = true;
                node.SelectAction = TreeNodeSelectAction.None;

                //_task = BLL.TaskManager.GetInstance().GetTask(selected.Id, false);
                //LoadTaskForView();
            }

            if (task.Children.Count > 0)
            {
                foreach (DTO.Task child in task.Children)
                {
                    node.ChildNodes.Add(CreateNode(child, selected));
                }
            }

            return node;
        }

        private TreeNode CreateNode(
            string id,
            string text,
            int children
        )
        {
            TreeNode node = new TreeNode();

            node.Text = text;
            node.Value = id;

            if (children > 0)
                node.ImageUrl = "~/css/images/parent_task_icon.png";

            else
                node.ImageUrl = "~/css/images/base_task_icon.png";

            node.NavigateUrl = "/Task/ViewTask.aspx?id=" + id;
          
            return node;
        }

        protected void Delete_Click(object sender, EventArgs e)
        {
            if (_task != null && !_task.Instantiated &&
                (currPermission == BLL.TaskManager.PermissionType.Both || currPermission == BLL.TaskManager.PermissionType.Owner))
            {
                BLL.TaskManager.GetInstance().DeleteTask(_task);
                Response.Redirect("/Task/TaskQueuePage.aspx");
            }
        }

        protected void Reject_Click(object sender, EventArgs e)
        {
            if (_task != null && !_task.Instantiated &&
                (currPermission == BLL.TaskManager.PermissionType.Both || currPermission == BLL.TaskManager.PermissionType.Assignee))
            {
                BLL.TaskManager.GetInstance().RejectTask(_task);
                Response.Redirect("/Task/TaskQueuePage.aspx");
            }           
        }

        protected void Close_Click(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request["Source"] != null)
            {
                Response.Redirect(HttpContext.Current.Request["Source"]);
            }
            else
            {
                Response.Redirect("/Task/TaskQueuePage.aspx");
            }
        }

        
        private string MoveToPrevNode(TreeNode currNode)
        {
            string postBackUrl = string.Empty;
            TreeNode parent = new TreeNode();
            TreeNode prevNode = new TreeNode();

            int index = 0;

            if (currNode.Parent != null)
            {
                parent = currNode.Parent;
                index = parent.ChildNodes.IndexOf(currNode);

                if (index != 0)
                {
                    currNode = parent.ChildNodes[index - 1];
                    while (currNode.ChildNodes.Count > 0)
                    {
                        currNode = currNode.ChildNodes[currNode.ChildNodes.Count - 1];
                    }
                    return postBackUrl = "/Task/ViewTask.aspx?id=" + currNode.Value;
                }
                else
                {

                    currNode = parent;
                    return postBackUrl = "/Task/ViewTask.aspx?id=" + currNode.Value;
                }
            }
            else
            {
                index = TreeView1.Nodes.IndexOf(currNode);

                if (index != 0)
                {
                    return postBackUrl = "/Task/ViewTask.aspx?id=" + TreeView1.Nodes[index - 1].Value;
                }
            }

            return postBackUrl;
        }

        private string MoveToNextNode(TreeNode currNode)
        {
            string postBackUrl = string.Empty;

            TreeNode parent = new TreeNode();
            TreeNode nextNode = new TreeNode();
            int total = 0;
            int index = 0;

            if (currNode.ChildNodes.Count > 0)
            {
                return postBackUrl = "/Task/ViewTask.aspx?id=" + currNode.ChildNodes[0].Value;
            }
            else
            {
                if (currNode.Parent != null)
                {
                    parent = currNode.Parent;

                    total = parent.ChildNodes.Count - 1;
                    index = parent.ChildNodes.IndexOf(currNode);

                    if (index < total)
                    {
                        return postBackUrl = "/Task/ViewTask.aspx?id=" + parent.ChildNodes[index + 1].Value;
                    }
                    else
                    {
                        while (index == total)
                        {
                            currNode = currNode.Parent;

                            if (currNode.Parent != null)
                            {
                                parent = currNode.Parent;

                                total = parent.ChildNodes.Count - 1;
                                index = parent.ChildNodes.IndexOf(currNode);

                                if (index < total)
                                {
                                    return postBackUrl = "/Task/ViewTask.aspx?id=" + parent.ChildNodes[index + 1].Value;
                                }
                            }
                            else
                                return postBackUrl;
                        }

                    }
                }

            }
            return postBackUrl;
        }


        //private void MoveToPrevNode(TreeNode currNode) 
        //{
        //    TreeNode parent = new TreeNode();
        //    TreeNode prevNode = new TreeNode();
           
        //    int index = 0;

        //    if (currNode.Parent != null)
        //    {
        //        parent = currNode.Parent;
        //        index = parent.ChildNodes.IndexOf(currNode);

        //        if (index != 0)
        //        {
        //            currNode = parent.ChildNodes[index - 1];
        //            while (currNode.ChildNodes.Count > 0)
        //            {
        //                currNode = currNode.ChildNodes[currNode.ChildNodes.Count - 1];
        //            }

        //            prevNode = currNode;
        //            prevNode.Selected = true;
        //        }
        //        else
        //        {

        //            currNode = parent;                  
        //            prevNode = currNode;
        //            prevNode.Selected = true;
        //        }
        //    }
        //    else 
        //    {
        //        index = TreeView1.Nodes.IndexOf(currNode);

        //        if (index != 0)
        //        {
        //            prevNode = TreeView1.Nodes[index - 1];
        //            prevNode.Selected = true;
        //        }

        //    }
        //}

        //private void MoveToNextNode(TreeNode currNode)
        //{
        //    TreeNode parent = new TreeNode();
        //    TreeNode nextNode = new TreeNode();
        //    int total = 0;
        //    int index = 0;

        //    if (currNode.ChildNodes.Count > 0)
        //    {
        //        nextNode = currNode.ChildNodes[0];
        //        nextNode.Selected = true;
        //    }
        //    else
        //    {
        //        if (currNode.Parent != null)
        //        {
        //            parent = currNode.Parent;

        //            total = parent.ChildNodes.Count - 1;
        //            index = parent.ChildNodes.IndexOf(currNode);

        //            if (index < total)
        //            {
        //                nextNode = parent.ChildNodes[index + 1];
        //                nextNode.Selected = true;
        //            }
        //            else
        //            {
        //                while (index == total)
        //                {
        //                    currNode = currNode.Parent;

        //                    if (currNode.Parent != null)
        //                    {
        //                        parent = currNode.Parent;

        //                        total = parent.ChildNodes.Count - 1;
        //                        index = parent.ChildNodes.IndexOf(currNode);

        //                        if (index < total)
        //                        {
        //                            nextNode = parent.ChildNodes[index + 1];
        //                            nextNode.Selected = true;
        //                            break;
        //                        }                               
        //                    }
        //                    else
        //                        break;
        //                }

        //            }
        //        }

        //    }

        //}

       
    }
}