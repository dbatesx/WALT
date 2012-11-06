using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;



namespace WALT.UIL.Task
{
    public partial class TaskForm : System.Web.UI.Page
    {
        private Dictionary<string, string> _profilePreferences;
        private DTO.Profile _profile;
        private const string Program_Preference_KEY = "ProgramPreferences";   
        private string selectedTaskType;
        private string selectedUnplannedCode;
        private DTO.Task _task;
        private const string VIEW_STATE_KEY_TASK = "Task";
        private BLL.TaskManager.PermissionType currPermission;
        private enum FormType { Edit, New, NewToParent, NewtoPlan, NewUnplan }
        private bool _error = false;
        private string _errorMsg = string.Empty;
        private long _favId = 0;
        private long _planProfileId = 0;

        private FormType currFormView
        {
            get
            {
                if (ViewState["CurrForm"] != null &&
                    ViewState["CurrForm"] is FormType)
                    return (FormType)ViewState["CurrForm"];
                else
                    return FormType.New;
            }
            set
            {
                ViewState["CurrForm"] = value;
            }
        }

        public String assigneeChoosen
        {
            get { return ViewState["AssigneeChoosen"] as String ?? ""; }
            set { ViewState["AssigneeChoosen"] = value; }
        }
        

        protected void Page_Load(object sender, EventArgs e)
        {
            _profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();
            _profilePreferences = _profile.Preferences;
           
            string javaScriptFunction = @"
                    $(function () { 
                        if( $('.TypeSelectedNode').length > 0){                   
                            var treeDivHeight =$('#taskTypeTreeViewDiv').height() /2;                              
                            var treeDivTop = $('#taskTypeTreeViewDiv').position().top;
                            var selectPosTop = $('.TypeSelectedNode').position().top;                
                            var scrolToTop = selectPosTop  - treeDivHeight - treeDivTop;                                             
                            $('#taskTypeTreeViewDiv').scrollTop(scrolToTop);                                     
                        } 

                    if( $('.unplannedSelectedNode').length > 0){   
                           
                            var treeDivHeight =$('#UnplannedTreeViewDiv').height() /2;
                            var treeDivTop = $('#UnplannedTreeViewDiv').position().top;
                            var selectPosTop = $('.unplannedSelectedNode').position().top;
                            var scrolToTop =selectPosTop  - treeDivHeight - treeDivTop;                 
                          
                            $('#UnplannedTreeViewDiv').scrollTop(scrolToTop);                          
                        } 

                        if( $('.TaskTreeSelectedNode').length > 0){ 
                    
                            var treeDivHeight =$('#TaskTreeViewDiv').height() /2;
                            var treeDivTop = $('#TaskTreeViewDiv').position().top;
                            var selectPosTop = $('.TaskTreeSelectedNode').position().top;
                            var scrolToTop =selectPosTop  - treeDivHeight - treeDivTop;
                                           
                            $('#TaskTreeViewDiv').scrollTop(scrolToTop);                         
                        }   
   
                        if ($('#FormTable').length > 0) {                           
                            var myheight = $('#FormTable').height();
                            $('#TaskTreeViewDiv').height(myheight-22);                           
                        }
                    });";

            AjaxControlToolkit.ToolkitScriptManager.RegisterStartupScript(this, this.GetType(), "newTaskScript", javaScriptFunction, true);
                       
            _task = (DTO.Task)ViewState[VIEW_STATE_KEY_TASK];
        
            if (!Page.IsPostBack)
            {
                long taskId = 0;       
                long parentId = 0;

                if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["id"]))           
                {
                    currFormView = FormType.Edit;
                    if (!long.TryParse(HttpContext.Current.Request.Params["id"], out taskId)) 
                    {
                        _error = true;
                        _errorMsg = "Task ID was not Recognized.";
                    }
                }
                else if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["parentid"]))
                {
                    currFormView = FormType.NewToParent;

                    if (!long.TryParse(HttpContext.Current.Request.Params["parentid"], out parentId))
                    {
                        _error = true;
                        _errorMsg = "Parent Task ID was not Recognized.";
                    }
                }      
                else if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["Plan"]) &&
                    HttpContext.Current.Request.Params["Plan"].ToLower().Equals("y") &&                    
                    !string.IsNullOrEmpty(HttpContext.Current.Request.Params["profileID"]) &&
                    long.TryParse(HttpContext.Current.Request.Params["profileID"], out _planProfileId))
                {
                    currFormView = FormType.NewtoPlan;
                }
                else if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["Unplan"]) &&
                    HttpContext.Current.Request.Params["Unplan"].ToLower().Equals("y") &&
                    !string.IsNullOrEmpty(HttpContext.Current.Request.Params["profileID"]) &&
                    long.TryParse(HttpContext.Current.Request.Params["profileID"], out _planProfileId))
                {
                    currFormView = FormType.NewUnplan;
                }
                else
                {
                    currFormView = FormType.New;
                }

                if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["FavId"]))
                {
                    if (long.TryParse(HttpContext.Current.Request.Params["FavId"], out _favId))
                    {
                       
                    }
                    else 
                    {
                        _error = true;
                        _errorMsg = "Favorite ID was not Recognized.";
                    }
                }

                LoadTask();

                if (!_error && _task != null)
                {
                    if (currPermission != BLL.TaskManager.PermissionType.None)
                    {
                        SetUpTaskPage();
                        LoadTaskForm();
                    }
                    else
                    {
                        Response.Redirect("/Task/ViewTask.aspx?id=" + _task.Id.ToString());
                    }
                }
                else 
                {
                    Utility.DisplayErrorMessage(_errorMsg);
                }
            }
        }

        private void LoadTask()
        {
            if (!_error)
            {
                if (currFormView == FormType.Edit)
                {
                    long taskId = long.Parse(HttpContext.Current.Request.Params["id"]);
                    _task = BLL.TaskManager.GetInstance().GetTask(taskId, false, false);
                }
                else
                {
                    _task = new DTO.Task();
                    _task.BaseTask = true;
                    _task.Owner = _profile;

                    if (currFormView != FormType.NewtoPlan && currFormView != FormType.NewUnplan)
                    {
                        _task.Assigned = _profile;
                    }
                    else 
                    {
                        _task.Assigned = WALT.BLL.ProfileManager.GetInstance().GetProfile(_planProfileId);
                    }
                }

                if (_task != null)
                {
                    if (currFormView == FormType.NewToParent) 
                    {
                        try
                        {
                            DTO.Task parentTask = BLL.TaskManager.GetInstance().GetTask(long.Parse(HttpContext.Current.Request.Params["parentid"]), false);

                            if (parentTask != null)
                            {
                                if (parentTask.Status == DTO.Task.StatusEnum.OPEN && parentTask.Assigned.Id == _profile.Id)
                                {
                                    //set parent task
                                    _task.ParentId = parentTask.Id;

                                    if (_favId == 0)
                                    {                                       
                                        _task.Title = parentTask.Title;
                                        _task.Program = parentTask.Program;
                                        _task.WBS = parentTask.WBS;

                                        if (parentTask.TaskType != null)
                                        {
                                            taskTypeHiddenField.Value = parentTask.TaskType.Id.ToString();
                                        }
                                    }
                                }
                                else 
                                {
                                    _error = true;
                                    _errorMsg = "You cannot create a child for this parent task due to the parent’s status or ownership.";
                                }
                            }
                            else
                            {
                                _error = true;
                                _errorMsg = "Parent Task ID was not Recognized.";
                            }
                        }
                        catch
                        {
                            _error = true;
                            _errorMsg = "Parent Task ID was not Recognized.";
                        }
                    }
                  
                    if(_favId!= 0)
                    {                       
                        DTO.Favorite _fav = BLL.TaskManager.GetInstance().GetFavorite(long.Parse(HttpContext.Current.Request.Params["FavId"]));

                        _task.Title = _fav.Title;
                        _task.Program = _fav.Program;
                        _task.Complexity = _fav.Complexity;
                        _task.TaskType = _fav.TaskType;
                        _task.Hours = _fav.Hours;
                        _task.Estimate = _fav.Estimate;
                    }

                    currPermission = BLL.TaskManager.GetInstance().GetTaskPermission(_task);
                    ViewState[VIEW_STATE_KEY_TASK] = _task;
                }
                else
                {
                    _error = true;
                    _errorMsg = "Task ID was not Recognized.";
                }
            }            
        }

        private void SetUpTaskPage()
        {
            SetUpTaskTreeView();
            BindStatusDropDown();
            BindProgDropDown();
            BindParentTaskDropDown();
            BindFavoriteDropDown();

            if (!_task.BaseTask )
            {
                fullyAllocatedPlaceHolder.Visible = true;
            }

            if (currFormView == FormType.Edit)
            {               
                if (_task.Instantiated)
                {
                    titleEditPlaceHolder.Visible = false;
                    titleStaticPlaceHolder.Visible = true;

                    if (_task.ParentId > 0 || currPermission == BLL.TaskManager.PermissionType.Assignee)
                    {
                        parentEditPlaceHolder.Visible = false;
                        parentStaticPlaceHolder.Visible = true;
                    }

                    programEditPlaceHolder.Visible = false;
                    programStaticPlaceHolder.Visible = true;

                    wbsEditPlaceHolder.Visible = false;
                    wbsStaticPlaceHolder.Visible = true;

                    hrsEditPlaceHolder.Visible = false;
                    hrsStaticPlaceHolder.Visible = true;

                    reqStartEditPlaceHolder.Visible = false;
                    reqStartStaticPlaceHolder.Visible = true;

                    dueDateEditPlaceHolder.Visible = false;
                    dueDateStaticPlaceHolder.Visible = true;

                    if (currPermission == BLL.TaskManager.PermissionType.Assignee)
                    {
                        ownerEditPlaceHolder.Visible = false;
                        ownerStaticPlaceHolder.Visible = true;
                    }

                    assigneeEditPlaceHolder.Visible = false;
                    assigneeStaticPlaceHolder.Visible = true;

                    if (_task.TaskType != null)
                    {
                        TaskTypePlaceHolder.Visible = true;
                        taskTypeEditPlaceHolder.Visible = false;
                        taskTypeStaticPlaceHolder.Visible = true;
                    }

                    if (_task.Estimate > 0)
                    {
                        rePlaceHolder.Visible = true;
                        reEditPlaceHolder.Visible = false;
                        reStaticPlaceHolder.Visible = true;
                    }

                    if (_task.Complexity != null)
                    {
                        complexityPlaceHolder.Visible = true;
                        complexityEditPlaceHolder.Visible = false;
                        complexityStaticPlaceHolder.Visible = true;
                    }

                    ownerCommentsEditPlaceHolder.Visible = false;
                    ownerCommentsStaticPlaceHolder.Visible = true;
                                       
                    exitCritriaEditPlaceHolder.Visible = false;
                    exitCritriaStaticPlaceHolder.Visible = true;
                }
                else if (currPermission == BLL.TaskManager.PermissionType.Assignee) //Uninstantiated 
                {
                    titleEditPlaceHolder.Visible = false;
                    titleStaticPlaceHolder.Visible = true;

                    parentEditPlaceHolder.Visible = false;
                    parentStaticPlaceHolder.Visible = true;

                    programEditPlaceHolder.Visible = false;
                    programStaticPlaceHolder.Visible = true;

                    wbsEditPlaceHolder.Visible = false;
                    wbsStaticPlaceHolder.Visible = true;

                    hrsEditPlaceHolder.Visible = false;
                    hrsStaticPlaceHolder.Visible = true;

                    reqStartEditPlaceHolder.Visible = false;
                    reqStartStaticPlaceHolder.Visible = true;

                    dueDateEditPlaceHolder.Visible = false;
                    dueDateStaticPlaceHolder.Visible = true;

                    ownerEditPlaceHolder.Visible = false;
                    ownerStaticPlaceHolder.Visible = true;

                    ownerCommentsEditPlaceHolder.Visible = false;
                    ownerCommentsStaticPlaceHolder.Visible = true;
                }

                if (currPermission == BLL.TaskManager.PermissionType.Owner)
                {
                    assigneeCommentsEditPlaceHolder.Visible = false;
                    assigneeCommentsStaticPlaceHolder.Visible = true;
                }
            }            
            else if (currFormView == FormType.NewtoPlan) 
            {
                assigneeEditPlaceHolder.Visible = false;
                assigneeStaticPlaceHolder.Visible = true;

                TaskTypePlaceHolder.Visible = true;
                taskTypeCustomValidator.Enabled = true;
                TaskTypeReqSign.Visible = true;

                reRequiredFieldValidator.Enabled = true;
                REReqSign.Visible = true;

                complexityRequiredFieldValidator.Enabled = true;
                complexityReqSign.Visible = true;
            }
            else if (currFormView == FormType.NewUnplan) 
            {
                assigneeEditPlaceHolder.Visible = false;
                assigneeStaticPlaceHolder.Visible = true;

                TaskTypePlaceHolder.Visible = true;
                taskTypeCustomValidator.Enabled = true;
                TaskTypeReqSign.Visible = true;

                reRequiredFieldValidator.Enabled = true;
                REReqSign.Visible = true;

                complexityRequiredFieldValidator.Enabled = true;
                complexityReqSign.Visible = true;

                unplannedPlaceHolder.Visible = true;
                UnplannedCustomValidator.Enabled = true;
            }
            else if (currFormView == FormType.NewToParent) 
            {
                saveNewButton1.Text = "Save & Create another Child";
                saveNewButton1.Width = 200;
                saveNewButton2.Text = "Save & Create another Child";
                saveNewButton2.Width = 200;

                saveNewButton1.Visible = true;
                saveNewButton2.Visible = true;
            }
            else if (currFormView == FormType.New) 
            {
                saveNewButton1.Text = "Save & Create another New Task";
                saveNewButton1.Width = 220;
                saveNewButton2.Text = "Save & Create another New Task";
                saveNewButton2.Width = 220;

                saveNewButton1.Visible = true;
                saveNewButton2.Visible = true;
            }
        }

        private void LoadTaskForm()
        {
            if (!string.IsNullOrEmpty(_task.Title))
            {
                titleTextBox.Text = _task.Title;
                titleLabel.Text = _task.Title;
            }

            if (_task.ParentId != 0 && _task.ParentId != _task.Id)
            {
                string parentTitle = WALT.BLL.TaskManager.GetInstance().GetTask(_task.ParentId, false, true).Title;

                if (parentEditPlaceHolder.Visible)
                {
                    parentTitleLabel.Text = parentTitle;

                    foreach (ListItem parentItem in parentDropDownList.Items)
                    {
                        if (_task.ParentId.ToString().Equals(parentItem.Value))
                        {
                            parentItem.Selected = true;
                        }
                    }
                }
                else
                {
                    parentStaticLabel.Text = parentTitle;
                }
            }
            else 
            {
                parentStaticLabel.Text = "N/A";
                parentTitleLabel.Text = "N/A";
            }

            foreach (ListItem statusItem in statusDropDownList.Items)
            {
                if (_task.Status.ToString().Equals(statusItem.Value))
                    statusItem.Selected = true;
            }

            statusLabel.Text = Utility.UppercaseFirst(_task.Status.ToString());

            if (_task.Created != null && _task.Created != DateTime.MinValue)
            {               
                CreatedLabel.Text = Utility.ConvertToLocal(_task.Created).ToString();
            }

            if (_task.Status == DTO.Task.StatusEnum.COMPLETED && _task.CompletedDate != null)
            {
                CompletedPlaceHolder.Visible = true;
                CompletedLabel.Text = Utility.ConvertToLocal(_task.CompletedDate.Value).ToString();
            }

            if (_task.Program != null)
            {
                bool first = true;

                foreach (ListItem programItem in programDropDownList.Items)
                {
                    if (first && _task.Program.Id.ToString().Equals(programItem.Value))
                    {
                        programItem.Selected = true;
                        first = false;
                    }
                }

                programLabel.Text = _task.ProgramTitle;
            }

            if (!string.IsNullOrEmpty(_task.WBS))
            {
                wbsTextBox.Text = _task.WBS;
                wbsLabel.Text = _task.WBS;
            }

            if (_task.Hours != 0)
            {
                hrsTextBox.Text = _task.Hours.ToString();               
                hrsLabel.Text = _task.Hours.ToString();    
            }

            if (!_task.BaseTask)
            {
                fullyAllocatedCheckBox.Checked = _task.FullyAllocated;
                fullyAllocatedLabel.Text = _task.FullyAllocated.ToString();

                NotAllocatedLabel.Visible = true;
                NotAllocatedLabel.Text = "(" + _task.HoursAllocatedToChildren.ToString() + " Hours Allocated To Children)";
            }

            hrsSpendLabel.Text = _task.Spent.ToString();
               
            if (_task.StartDate != null && _task.StartDate != DateTime.MinValue)
            {
                reqStartTextBox.Text = _task.StartDate.Value.ToShortDateString();
                reqStartLabel.Text = _task.StartDate.Value.ToShortDateString();
            }

            if (_task.DueDate !=null &&  _task.DueDate != DateTime.MinValue)
            {
                dueDateTextBox.Text = _task.DueDate.Value.ToShortDateString();
                dueDateLabel.Text = _task.DueDate.Value.ToShortDateString();
            }

            if (_task.Owner != null)
            {
                List<DTO.Profile> ownerProfile = new List<DTO.Profile>();
                ownerProfile.Add(_task.Owner);

                ownerProfileSelector.ProfilesChosen = ownerProfile;
                ownerLabel.Text = _task.Owner.DisplayName;
            }

            if (_task.Assigned != null)
            {
                List<DTO.Profile> assigneeProfile = new List<DTO.Profile>();
                assigneeProfile.Add(_task.Assigned);

                assigneeProfileSelector.ProfilesChosen = assigneeProfile;

                assigneeLabel.Text = _task.Assigned.DisplayName;

                assigneeChoosen = "-1";
            }

            if (_task.TaskType != null)
            {
                taskTypeHiddenField.Value = _task.TaskType.Id.ToString();
                
                taskTypeLabel.Text = _task.TaskTypeTitle;
            }

            if (_task.Estimate != 0)
            {
                reTextBox.Text = _task.Estimate.ToString();
                reLabel.Text = _task.Estimate.ToString();
            }

            if (_task.Complexity != null)
            {
                complexityHiddenField.Value = _task.Complexity.Id.ToString();
                complexityLabel.Text = _task.Complexity.Title + ", Hours: " + _task.Complexity.Hours.ToString();
            }

            
            if (!string.IsNullOrEmpty(_task.OwnerComments))
            {
                ownerCommentsTextBox.Text = _task.OwnerComments;
                ownerCommentsLabel.Text = _task.OwnerComments.Replace(Environment.NewLine, "<br />"); ;
            }

            if (!string.IsNullOrEmpty(_task.AssigneeComments))
            {
                assigneeCommentsTextBox.Text = _task.AssigneeComments;
                assigneeCommentsLabel.Text = _task.AssigneeComments.Replace(Environment.NewLine, "<br />"); ;
            }

            if (!string.IsNullOrEmpty(_task.ExitCriteria))
            {
                exitCritriaTextBox.Text = _task.ExitCriteria;
                exitCritriaLabel.Text = _task.ExitCriteria.Replace(Environment.NewLine, "<br />"); ;
            }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            if (!_error)
            {
                if (assigneeProfileSelector.ProfilesChosen.Count == 1)
                {
                    DTO.Profile assignee = assigneeProfileSelector.ProfilesChosen[0];

                    DTO.Team assigneeTeam = BLL.AdminManager.GetInstance().GetTeam(assignee, "MEMBER");

                    if (assigneeTeam != null)
                    {
                        if (!assigneeChoosen.Equals(assignee.Id.ToString()))
                        {
                            List<DTO.TaskType> taskTypes = WALT.BLL.AdminManager.GetInstance().GetTaskTypeList(assigneeTeam, false, true, true);

                            TaskTypeTreeView.Nodes.Clear();
                            taskTypeDescLabel.Text = string.Empty;
                                                        
                            if (taskTypes.Count > 0)
                            {
                                foreach (DTO.TaskType type in taskTypes)
                                {
                                    TaskTypeTreeView.Nodes.Add(CreateNode(type));
                                }

                                TaskTypePlaceHolder.Visible = true;
                            }

                            TaskTypeUpdatePanel.Update();

                            if (currFormView == FormType.NewUnplan)
                            {
                                List<DTO.UnplannedCode> unplannedCodes = WALT.BLL.AdminManager.GetInstance().GetUnplannedCodeList(assigneeTeam, true, true);

                                unplannedPlaceHolder.Visible = true;
                                unplannedTreeView.Nodes.Clear();
                                UnplannedHiddenField.Value = "";
                                unplannedDescLabel.Text = string.Empty;

                                if (unplannedCodes.Count > 0)
                                {
                                    foreach (DTO.UnplannedCode code in unplannedCodes)
                                    {
                                        unplannedTreeView.Nodes.Add(CreateNode(code));
                                    }
                                }

                                unplannedUpdatePanel.Update();
                            }
                        }

                        if (!_task.Instantiated)
                        {
                            //R/E based team
                            if (!assigneeTeam.ComplexityBased)
                            {
                                rePlaceHolder.Visible = true;
                                reEditPlaceHolder.Visible = true;
                                reStaticPlaceHolder.Visible = false;
                                REUpdatePanel.Update();

                                complexityDropDownList.Items.Clear();
                                complexityPlaceHolder.Visible = false;
                                complexityUpdatePanel.Update();
                            }
                            else // complexity based team
                            {
                                complexityDropDownList.Items.Clear();

                                long typeID = 0;

                                if (long.TryParse(taskTypeHiddenField.Value, out typeID))
                                {
                                    complexityPlaceHolder.Visible = true;

                                    rePlaceHolder.Visible = true;
                                    reStaticPlaceHolder.Visible = true;
                                    reEditPlaceHolder.Visible = false;

                                    long? dircId = WALT.BLL.AdminManager.GetInstance().GetDirectorateID(assigneeTeam.Id);

                                    if (dircId != null)
                                    {
                                        List<DTO.Complexity> complexityList = WALT.BLL.TaskManager.GetInstance().GetComplexityList(dircId.Value, typeID, true);

                                        if (complexityList.Count > 0)
                                        {
                                            complexityDropDownList.Items.Insert(0, new ListItem("None", "0"));

                                            foreach (DTO.Complexity complex in complexityList)
                                            {
                                                complexityDropDownList.Items.Insert(complexityDropDownList.Items.Count, new ListItem(complex.Title + ", Hours: " + complex.Hours, complex.Id.ToString()));
                                            }
                                            bool foundComp = false;

                                            if (!string.IsNullOrEmpty(complexityHiddenField.Value))
                                            {
                                                foreach (ListItem item in complexityDropDownList.Items)
                                                {
                                                    if (item.Value.Equals(complexityHiddenField.Value))
                                                    {
                                                        item.Selected = true;
                                                        reLabel.Text = BLL.AdminManager.GetInstance().GetComplexityCode(long.Parse(complexityHiddenField.Value)).Hours.ToString();
                                                        foundComp = true;
                                                    }
                                                }

                                                if (!foundComp)
                                                    reLabel.Text = string.Empty;
                                            }
                                            else
                                            {
                                                reLabel.Text = string.Empty;
                                            }
                                        }
                                        else
                                        {
                                            complexityDropDownList.Items.Insert(0, new ListItem("Task Type has no complexity code", "0"));
                                            reLabel.Text = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        complexityDropDownList.Items.Insert(0, new ListItem("GetDirectorate does not Exist.", "0"));
                                        reLabel.Text = string.Empty;
                                    }
                                }
                                else
                                {
                                    complexityPlaceHolder.Visible = false;
                                    rePlaceHolder.Visible = false;
                                }

                                complexityUpdatePanel.Update();
                                REUpdatePanel.Update();
                            }
                        }
                    }
                    else // assignee is not an IC in a team
                    {
                        TaskTypePlaceHolder.Visible = false;
                        TaskTypeTreeView.Nodes.Clear();                     
                        TaskTypeUpdatePanel.Update();

                        unplannedPlaceHolder.Visible = false;
                        unplannedTreeView.Nodes.Clear();                       
                        unplannedUpdatePanel.Update();
                                              
                        rePlaceHolder.Visible = true;
                        REUpdatePanel.Update();

                        complexityDropDownList.Items.Clear();
                        complexityPlaceHolder.Visible = false;
                        complexityUpdatePanel.Update();
                    }

                    assigneeChoosen = assignee.Id.ToString();
                }
                else // No Assignee
                {
                    assigneeChoosen = "";

                    TaskTypePlaceHolder.Visible = false;
                    TaskTypeTreeView.Nodes.Clear();                   
                    TaskTypeUpdatePanel.Update();

                    unplannedPlaceHolder.Visible = false;
                    unplannedTreeView.Nodes.Clear();                 
                    unplannedUpdatePanel.Update();

                    reTextBox.Text = string.Empty;
                    rePlaceHolder.Visible = false;
                    REUpdatePanel.Update();

                    complexityDropDownList.Items.Clear();
                    complexityPlaceHolder.Visible = false;
                    complexityUpdatePanel.Update();
                }
            }            
        }
        
        private void SetUpTaskTreeView()
        {
            // setting up the task tree view            
            DTO.Task parentTask = new DTO.Task();

            if (currFormView == FormType.Edit)
            {
                if (_task.ParentId == 0 || _task.ParentId == _task.Id)
                {
                    parentTask = WALT.BLL.TaskManager.GetInstance().GetTask(_task.Id, true, true);
                }
                else
                {
                    parentTask = WALT.BLL.TaskManager.GetInstance().GetRootTask(_task, _profile);
                }
            }
            else if (currFormView == FormType.NewToParent)
            {
                parentTask.Id = _task.ParentId;
                parentTask = WALT.BLL.TaskManager.GetInstance().GetRootTask(parentTask, _profile);
            }
            else
            {
                parentTask = _task;
            }

            TaskTreeView.Nodes.Add(CreateNode(parentTask, _task));
        }

        protected void favDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (favDropDownList.SelectedItem != null && long.TryParse(favDropDownList.SelectedValue, out _favId))
            {
                if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["parentid"]))
                {
                    Response.Redirect("/Task/TaskForm.aspx?parentid=" + HttpContext.Current.Request.Params["parentid"] + "&favID=" + _favId);
                }
                else if ((!string.IsNullOrEmpty(HttpContext.Current.Request.Params["Plan"])) &&
                    (HttpContext.Current.Request.Params["Plan"].ToLower().Equals("y")))
                {
                    Response.Redirect("/Task/TaskForm.aspx?Plan=Y&favID=" + _favId);
                }
                else if ((!string.IsNullOrEmpty(HttpContext.Current.Request.Params["Unplan"])) &&
                    (HttpContext.Current.Request.Params["Unplan"].ToLower().Equals("y")))
                {
                    Response.Redirect("/Task/TaskForm.aspx?Unplan=Y&favID=" + _favId);
                }
                else
                {
                    Response.Redirect("/Task/TaskForm.aspx?favID=" + _favId);
                }
            }
            else
            {
                Utility.DisplayErrorMessage("Favorite ID was not recognized.");
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            Button saveBtn = (Button)sender;
            if (_task != null)
            {
                if (titleEditPlaceHolder.Visible && !string.IsNullOrEmpty(titleTextBox.Text))
                {
                    _task.Title = titleTextBox.Text;
                }

                if (parentEditPlaceHolder.Visible && !string.IsNullOrEmpty(parentIdHiddenField.Value) && !parentIdHiddenField.Value.Equals("-1"))
                {
                    long parentId;

                    if (long.TryParse(parentDropDownList.SelectedValue, out parentId))
                    {
                        if (parentId.ToString().Equals(parentIdHiddenField.Value))
                            _task.ParentId = parentId;
                    }
                }

                if (statusEditPlaceHolder.Visible && !string.IsNullOrEmpty(statusDropDownList.SelectedValue))
                {
                    try
                    {
                        DTO.Task.StatusEnum stat = (DTO.Task.StatusEnum)Enum.Parse(typeof(DTO.Task.StatusEnum), statusDropDownList.SelectedValue, true);

                        _task.Status = stat;
                    }
                    catch { }
                }

                int progId = 0;

                if (programEditPlaceHolder.Visible && int.TryParse(programDropDownList.SelectedValue, out progId))
                {
                    _task.Program = BLL.AdminManager.GetInstance().GetProgram(progId);
                }

                if (wbsEditPlaceHolder.Visible)
                {
                    _task.WBS = wbsTextBox.Text;
                }

                double hrs = 0;

                if (hrsEditPlaceHolder.Visible && double.TryParse(hrsTextBox.Text, out hrs))
                {
                    _task.Hours = hrs;
                }

                if (fullyAllocatedPlaceHolder.Visible && fullyAllocatedEditPlaceHolder.Visible)
                {
                    _task.FullyAllocated = fullyAllocatedCheckBox.Checked;
                }

                DateTime reqStart;
                if (reqStartEditPlaceHolder.Visible && DateTime.TryParse(reqStartTextBox.Text, out reqStart))
                {
                    _task.StartDate = reqStart;
                }

                DateTime dueDate;
                if (dueDateEditPlaceHolder.Visible && DateTime.TryParse(dueDateTextBox.Text, out dueDate))
                {
                    _task.DueDate = dueDate;
                }

                if (ownerEditPlaceHolder.Visible && ownerProfileSelector.ProfilesChosen.Count == 1)
                {
                    _task.Owner = ownerProfileSelector.ProfilesChosen[0];
                }

                if (assigneeEditPlaceHolder.Visible && assigneeProfileSelector.ProfilesChosen.Count == 1)
                {
                    _task.Assigned = assigneeProfileSelector.ProfilesChosen[0];
                }

                long typeID = 0;
                if (TaskTypePlaceHolder.Visible && taskTypeEditPlaceHolder.Visible && TaskTypeTreeView.SelectedNode != null && long.TryParse(TaskTypeTreeView.SelectedNode.Value, out typeID))
                {
                    _task.TaskType = BLL.AdminManager.GetInstance().GetTaskType(typeID);
                }

                double re = 0;
                if (rePlaceHolder.Visible && reEditPlaceHolder.Visible && double.TryParse(reTextBox.Text, out re))
                {
                    _task.Estimate = re;
                }

                long complexId = 0;
                if (complexityPlaceHolder.Visible && complexityEditPlaceHolder.Visible && long.TryParse(complexityDropDownList.SelectedValue, out complexId))
                {
                    if (complexId != 0)
                    {
                        DTO.Complexity comp = BLL.TaskManager.GetInstance().GetComplexity(complexId);
                        _task.Complexity = comp;
                        _task.Estimate = comp.Hours;
                    }
                    else
                    {
                        _task.Complexity = null;
                        _task.Estimate = 0;
                    }
                }

                if (ownerCommentsEditPlaceHolder.Visible)
                {
                    _task.OwnerComments = ownerCommentsTextBox.Text;
                }

                if (assigneeCommentsEditPlaceHolder.Visible)
                {
                    _task.AssigneeComments = assigneeCommentsTextBox.Text;
                }

                if (exitCritriaEditPlaceHolder.Visible)
                {
                    _task.ExitCriteria = exitCritriaTextBox.Text;
                }

                try
                {
                    WALT.BLL.TaskManager.GetInstance().SaveTask(_task);
                }
                catch (Exception ex)
                {
                    _error = true;
                    Utility.DisplayErrorMessage("There has been an unexpected error. Task was not created! <br />" + ex.Message);
                    return;
                }

                if ((currFormView == FormType.New || currFormView == FormType.NewToParent)
                        && (saveBtn == saveNewButton1 || saveBtn == saveNewButton2))
                {
                    if (currFormView == FormType.NewToParent)
                    {
                        Response.Redirect("/Task/TaskForm.aspx?ParentId=" + _task.ParentId);
                    }
                    else
                    {
                        Response.Redirect("/Task/TaskForm.aspx");
                    }
                }
                else
                {
                    if (currFormView == FormType.NewtoPlan)
                    {
                        if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["FavId"]) && (long.TryParse(HttpContext.Current.Request.Params["FavId"], out _favId)))
                        {
                            Response.Redirect("/weekly.aspx?taskId=" + _task.Id.ToString() + "&favID=" + _favId.ToString());
                        }
                        else
                        {
                            Response.Redirect("/weekly.aspx?taskId=" + _task.Id.ToString());
                        }
                    }
                    else if (currFormView == FormType.NewUnplan)
                    {
                        Response.Redirect("/weekly.aspx?taskId=" + _task.Id.ToString() + "&ucode=" + unplannedTreeView.SelectedValue);
                    }
                    else
                    {
                        Response.Redirect("/Task/ViewTask.aspx?id=" + _task.Id.ToString());
                    }
                }

            }
        }       

        protected void TaskFormCustomValidator_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            bool validate = true;

            if (validate)
            {
                args.IsValid = true;
            }
            else
                args.IsValid = false;
        }

        protected void BindProgDropDown()
        {
           
          Dictionary<long, bool> prefPrograms = new Dictionary<long, bool>();

            //if user has program prefrences 
            if (_profilePreferences != null && _profilePreferences.ContainsKey(Program_Preference_KEY))
            {
                XElement prefProg = XElement.Parse(_profilePreferences[Program_Preference_KEY]);

                foreach (XElement prog in prefProg.Elements("Program"))
                { 
                    long progId= 0;

                    if (long.TryParse(prog.Element("ID").Value.ToString(), out progId))
                    {
                        if (prog.Element("Default").Value.ToString().Equals("true"))
                        {
                            prefPrograms.Add(progId, true);
                        }
                        else
                        {
                            prefPrograms.Add(progId, false);
                        }
                    }
                }
            }            
            
                Dictionary<long, string> programs = BLL.AdminManager.GetInstance().GetProgramDictionary();             
                
                if (prefPrograms != null && prefPrograms.Count > 0) 
                {
                    foreach (KeyValuePair<long, bool> pair in prefPrograms) 
                    {
                       if( programs.ContainsKey(pair.Key))
                       {
                           DTO.Program progInPref = BLL.AdminManager.GetInstance().GetProgram(pair.Key);

                           if (progInPref.Active)
                           {
                               ListItem item = new ListItem(progInPref.Title, progInPref.Id.ToString());

                               if ((currFormView == FormType.New ||
                                   currFormView == FormType.NewUnplan ||
                                   currFormView == FormType.NewToParent ||
                                   currFormView == FormType.NewtoPlan) &&
                                   pair.Value)
                               {
                                   item.Selected = true;
                               }

                               programDropDownList.Items.Insert(programDropDownList.Items.Count, item);
                           }
                       }
                    }
                  
                   programDropDownList.Items.Insert(programDropDownList.Items.Count, new ListItem("=========================", "none"));
                }

                foreach (KeyValuePair<long, string> progPairs in programs)
                {
                    programDropDownList.Items.Insert(programDropDownList.Items.Count, new ListItem(progPairs.Value.ToString(), progPairs.Key.ToString()));
                }

            programDropDownList.Items.Insert(0, new ListItem("Select a Program...", "none"));
            
        }

        protected void TaksTreeView_PreRender(object sender, EventArgs e)
        {
            if (TaskTreeView.SelectedNode != null)
            {
                PreLink.NavigateUrl = MoveToPrevNode(TaskTreeView.SelectedNode);
                NextLink.NavigateUrl = MoveToNextNode(TaskTreeView.SelectedNode);

                if (TaskTreeView.Nodes.IndexOf(TaskTreeView.SelectedNode) == -1)
                {
                    PreLink.Enabled = true;

                    int Nodecount = TaskTreeView.Nodes.Count;
                    TreeNode lastNode = new TreeNode();

                    if (Nodecount > 0)
                    {
                        lastNode = TaskTreeView.Nodes[Nodecount - 1];

                        while (lastNode.ChildNodes.Count > 0)
                        {
                            lastNode = lastNode.ChildNodes[lastNode.ChildNodes.Count - 1];
                        }
                    }

                    if (TaskTreeView.SelectedNode == lastNode)
                    {
                        NextLink.Enabled = false;
                    }
                    else
                        NextLink.Enabled = true;
                }
                else
                {
                    if (TaskTreeView.Nodes.IndexOf(TaskTreeView.SelectedNode) == 0)
                        PreLink.Enabled = false;
                    else
                        PreLink.Enabled = true;
                }

                if (TaskTreeView.SelectedNode.Parent != null)
                {
                    TreeNode parentNode = TaskTreeView.SelectedNode.Parent;

                    while (parentNode != null)
                    {
                        parentNode.Expanded = true;
                        parentNode = parentNode.Parent;
                    }
                }
            }
        }

        protected void TaskTypeTreeView_PreRender(object sender, EventArgs e)
        {
            if (TaskTypeTreeView.SelectedNode != null && TaskTypeTreeView.SelectedNode.Parent != null)
            {
                TreeNode parentNode = TaskTypeTreeView.SelectedNode.Parent;

                while (parentNode != null)
                {
                    parentNode.Expanded = true;
                    parentNode = parentNode.Parent;
                }
            }
        }

        protected void TaskTypeTreeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            long typeID = 0;

            //TaskTypeTreeView.SelectedNode.ImageUrl = "~/css/images/bullet.gif";

            if (long.TryParse(TaskTypeTreeView.SelectedNode.Value, out typeID))
            {
                selectedTaskType = typeID.ToString();
                taskTypeHiddenField.Value = selectedTaskType;
                DTO.TaskType taskTypes = WALT.BLL.AdminManager.GetInstance().GetTaskType(typeID);

                if (!string.IsNullOrEmpty(taskTypes.Description))
                {
                    taskTypeDescLabel.Text = taskTypes.Description.Replace(Environment.NewLine, "<br />");
                }
                else 
                {
                    taskTypeDescLabel.Text = string.Empty;
                }
            }
            else 
            {
                taskTypeHiddenField.Value = "";
            }

            TaskTypeUpdatePanel.Update();
        }

        protected void unplannedTreeView_PreRender(object sender, EventArgs e)
        {
            if (unplannedTreeView.SelectedNode != null && unplannedTreeView.SelectedNode.Parent != null)
            {
                TreeNode parentNode = unplannedTreeView.SelectedNode.Parent;

                while (parentNode != null)
                {
                    parentNode.Expanded = true;
                    parentNode = parentNode.Parent;
                }
            }
        }

        protected void unplannedTreeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            long codeID = 0;

            //unplannedTreeView.SelectedNode.ImageUrl = "~/css/images/bullet.gif";

            if (long.TryParse(unplannedTreeView.SelectedNode.Value, out codeID))
            {

                selectedUnplannedCode = codeID.ToString();
                UnplannedHiddenField.Value = selectedUnplannedCode;

                DTO.UnplannedCode unplan = WALT.BLL.AdminManager.GetInstance().GetUnplannedCode(codeID);

                if (!string.IsNullOrEmpty(unplan.Description))
                {
                    unplannedDescLabel.Text = unplan.Description.Replace(Environment.NewLine, "<br />");
                }
                else 
                {
                    unplannedDescLabel.Text = string.Empty;
                }
            }
            else 
            {
                UnplannedHiddenField.Value = "";
            }

            unplannedUpdatePanel.Update();           
        }

        private TreeNode CreateNode( DTO.Task parent, DTO.Task task)
        {
            bool diabled = false;

            if (task.Id == parent.Id)
                diabled = true;
            
            TreeNode node = CreateNode(parent.Id.ToString(), parent.Title, parent.Children.Count, diabled);
            
            if (currFormView == FormType.NewToParent && parent.Id == _task.ParentId) 
            {
                TreeNode newNode = new TreeNode();
                newNode.Text = "New Task";
                newNode.Value = "0";
                newNode.Selected = true;
                newNode.ImageUrl = "~/css/images/base_task_icon.png";
                newNode.SelectAction = TreeNodeSelectAction.None;

                node.ChildNodes.Add(newNode);
            }

            if (parent.Children.Count > 0)
            {
                foreach (DTO.Task child in parent.Children)
                {
                    node.ChildNodes.Add(CreateNode(child, task));
                }
            }
                      
            return node;
        }

        private TreeNode CreateNode(DTO.TaskType type)
        {
            TreeNode node = CreateTypeNode(type.Id.ToString(), type.Title, type.Children.Count);
            node.Expanded = false;

            if (type.Children.Count > 0)
            {
                foreach (DTO.TaskType child in type.Children)
                {
                    node.ChildNodes.Add(CreateNode(child));
                }
            }
            return node;
        }

        private TreeNode CreateNode(DTO.UnplannedCode code)
        {
            TreeNode node = CreateUnplanNode(code.Id.ToString(), code.Code + "-" + code.Title, code.Children.Count);
            node.Expanded = false;

            if (code.Children.Count > 0)
            {
                foreach (DTO.UnplannedCode child in code.Children)
                {
                    node.ChildNodes.Add(CreateNode(child));
                }
            }
            return node;
        }

        private TreeNode CreateNode(
            string id,
            string text,
            int children,
            bool disabled
        )
        {
            TreeNode node = new TreeNode();
            string title = text;

            if (string.IsNullOrEmpty(title)) 
            {
                switch (currFormView)
                {
                    case FormType.NewtoPlan:
                        title = "New Planned Task";
                        break;

                    case FormType.NewUnplan:
                        title = "New Unplanned Task";
                        break;

                    case FormType.New:
                        title = "New Task";
                        break;
                }
            }

            node.Text = "<span class=\"treeNode\" >" + title + "</span>";
            node.Value = id;

            if (children > 0)
                node.ImageUrl = "~/css/images/parent_task_icon.png";
            else
                node.ImageUrl = "~/css/images/base_task_icon.png";

            node.NavigateUrl = "/Task/TaskForm.aspx?id=" + id;

            if (disabled)
            {
                node.SelectAction = TreeNodeSelectAction.None;
                node.Selected = true;
            }

            return node;
        }

        private TreeNode CreateTypeNode(
            string id,
            string text,
            int children
        )
        {
            TreeNode node = new TreeNode();

            node.Text = text;
            node.Value = id;

            node.ImageUrl = "~/css/images/bullet.gif";
            if (children == 0)
            {
                node.SelectAction = TreeNodeSelectAction.Select;
                //node.ImageUrl = "~/css/images/bullet-blue.gif";
            }
            else
                node.SelectAction = TreeNodeSelectAction.None;

            if (!string.IsNullOrEmpty(taskTypeHiddenField.Value) && id.Equals(taskTypeHiddenField.Value))
            {
                node.Selected = true;
                //node.ImageUrl = "~/css/images/bullet.gif";
            }

            return node;
        }

        private TreeNode CreateUnplanNode(
       string id,
       string text,
       int children
   )
        {
            TreeNode node = new TreeNode();

            node.Text = text;
            node.Value = id;
            node.ImageUrl = "~/css/images/bullet.gif";

            if (children == 0)
            {
                node.SelectAction = TreeNodeSelectAction.Select;
                //node.ImageUrl = "~/css/images/bullet-blue.gif";
            }
            else
                node.SelectAction = TreeNodeSelectAction.None;

            if (!string.IsNullOrEmpty(selectedUnplannedCode) && selectedUnplannedCode.Equals(id.ToString()))
            {
                node.Selected = true;
                //node.ImageUrl = "~/css/images/bullet-blue.gif";
            }         

            return node;
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
                return postBackUrl = "/Task/TaskForm.aspx?id=" + currNode.ChildNodes[0].Value;
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
                        return postBackUrl = "/Task/TaskForm.aspx?id=" + parent.ChildNodes[index + 1].Value;
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
                                    return postBackUrl = "/Task/TaskForm.aspx?id=" + parent.ChildNodes[index + 1].Value;
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
                    return postBackUrl = "/Task/TaskForm.aspx?id=" + currNode.Value;
                }
                else
                {

                    currNode = parent;
                    return postBackUrl = "/Task/TaskForm.aspx?id=" + currNode.Value;
                }
            }
            else
            {
                index = TaskTreeView.Nodes.IndexOf(currNode);

                if (index != 0)
                {
                    return postBackUrl = "/Task/TaskForm.aspx?id=" + TaskTreeView.Nodes[index - 1].Value;
                }
            }

            return postBackUrl;
        }

        protected void BindParentTaskDropDown()
        {
            parentDropDownList.Items.Clear();

            Dictionary<long, string> tasksList = BLL.TaskManager.GetInstance().GetParentableTaskList(_profile, _task);

            parentDropDownList.DataSource = tasksList;
            parentDropDownList.DataTextField = "Value";
            parentDropDownList.DataValueField = "Key";
            parentDropDownList.DataBind();

            parentDropDownList.Items.Insert(0, new ListItem("No Parent", "0"));
            

           // List<DTO.Task> tasksList = BLL.TaskManager.GetInstance().GetTaskList(_profile, DTO.Task.StatusEnum.OPEN, DTO.Task.ColumnEnum.TITLE, true);

            //foreach (DTO.Task t in tasksList)
            //{
            //    bool isPlanned = BLL.TaskManager.GetInstance().IsTaskPlanned(t);

            //    if (_task.Id != t.Id && !isPlanned )
            //        parentDropDownList.Items.Insert(parentDropDownList.Items.Count, new ListItem(t.Title, t.Id.ToString()));
            //}

            //parentDropDownList.Items.Insert(0, new ListItem("No Parent", "0"));
        }

        protected void BindFavoriteDropDown()
        {
            if (currFormView != FormType.Edit)
            {
                List<DTO.Favorite> favList = BLL.TaskManager.GetInstance().GetFavorites();

                if (favList.Count > 0)
                {
                    foreach (DTO.Favorite t in favList)
                    {
                        favDropDownList.Items.Insert(favDropDownList.Items.Count, new ListItem(t.Title, t.Id.ToString()));
                    }

                    favDropDownList.Items.Insert(0, new ListItem("Select a Favorite", "0"));
                }
                else
                {
                    BtnFav.Visible = false;
                }
            }
            else 
            {
                BtnFav.Visible = false;
            }
        }

        protected void BindStatusDropDown()
        {
            bool addClose = BLL.TaskManager.GetInstance().IsTaskAllowedToBeClosed(_task);

            if (currFormView == FormType.NewtoPlan || currFormView == FormType.NewUnplan || (!_task.BaseTask && !addClose))
            {
                foreach (string stat in Enum.GetNames(typeof(DTO.Task.StatusEnum)))
                {
                    if (!stat.Equals(DTO.Task.StatusEnum.COMPLETED.ToString()) &&
                        !stat.Equals(DTO.Task.StatusEnum.REJECTED.ToString()))
                    {
                        statusDropDownList.Items.Insert(statusDropDownList.Items.Count, new ListItem(Utility.UppercaseFirst(stat), stat));
                    }
                }
            }
            else
            {
                foreach (string stat in Enum.GetNames(typeof(DTO.Task.StatusEnum)))
                {
                    if (!stat.Equals(DTO.Task.StatusEnum.REJECTED.ToString()))
                    {
                        statusDropDownList.Items.Insert(statusDropDownList.Items.Count, new ListItem(Utility.UppercaseFirst(stat), stat));
                    }
                }
            }
        }

        protected void Close_Click(object sender, EventArgs e)
        {
            if (_task.Id != 0)
            {
                string source = string.Empty;

                if (HttpContext.Current.Request["Source"] != null)
                {
                    source = "&Source=" + HttpContext.Current.Request["Source"];
                }

                Response.Redirect("/Task/ViewTask.aspx?id=" + _task.Id.ToString() + source);
            }
            else if (!string.IsNullOrEmpty(HttpContext.Current.Request["plan"]) ||
                !string.IsNullOrEmpty(HttpContext.Current.Request["unplan"]))
            {
                Response.Redirect("/weekly.aspx");
            }
            else
            {
                Response.Redirect("/Task/TaskQueuePage.aspx");
            }
        }

        protected void complexityDropDownList_SelectedIndexChanged1(object sender, EventArgs e)
        {
            if (complexityDropDownList.SelectedItem != null && !string.IsNullOrEmpty(complexityDropDownList.SelectedValue) && !complexityDropDownList.SelectedValue.Equals("0"))
            {
                complexityHiddenField.Value = complexityDropDownList.SelectedValue;
            }
            else 
            {
                complexityHiddenField.Value = string.Empty;
            }

            complexityUpdatePanel.Update();     
        }
    }
}