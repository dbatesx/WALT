using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WALT.UIL.Task
{
    public partial class TaskQueuePage : System.Web.UI.Page
    {
       
        private DTO.Profile _profile;
        private DTO.Profile taskProfile;
        private Dictionary<string, string> _profilePreferences;
        private const string Role_Preference_KEY = "TaskQueueRoleFilter";
        private const string Task_Filter_Preference_KEY = "TaskQueueTaskFilter";

        private Dictionary<DTO.Task.ColumnEnum, String> _filters
       {
           get
           {
               object f = ViewState["TaskFilters"];
               return (f == null ? null : (Dictionary<DTO.Task.ColumnEnum, String>)f);
           }
           set { ViewState["TaskFilters"] = value; }
       }

        protected override void OnInit(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                foreach (string stat in Enum.GetNames(typeof(DTO.Task.StatusEnum)))
                {
                    if (!stat.Equals(DTO.Task.StatusEnum.REJECTED.ToString()))
                    {
                        taskFilterDropDownList.Items.Insert(taskFilterDropDownList.Items.Count, new ListItem(Utility.UppercaseFirst(stat), stat));
                    }
                }

                taskFilterDropDownList.Items.Add(new ListItem("All", "ALL"));
            }
            base.OnInit(e);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            _profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();
            _profilePreferences = _profile.Preferences;
            bool loadData = false;

            if (!Page.IsPostBack)
            {
                BindTeamDropDown();

                if (HttpContext.Current.Session["Team_Filter"] != null)
                {
                    foreach (ListItem roleItem in TeamDropDownList.Items)
                    {
                        if (HttpContext.Current.Session["Team_Filter"].Equals(roleItem.Value))
                        {
                            roleItem.Selected = true;
                            break;
                        }
                    }
                }

                if (TeamDropDownList.SelectedItem != null && TeamDropDownList.SelectedIndex != 0)
                {
                    BindTeamMembersDropDown();

                    if (HttpContext.Current.Session["TeamMember_Filter"] != null)
                    {
                        foreach (ListItem roleItem in TeamMembersDropDownList.Items)
                        {
                            if (HttpContext.Current.Session["TeamMember_Filter"].Equals(roleItem.Value))
                            {
                                roleItem.Selected = true;
                                break;
                            }
                        }
                    }

                }

                if (HttpContext.Current.Session["Role_Filter"] != null)
                {
                    foreach (ListItem roleItem in taskRoleDropDownList.Items)
                    {
                        if (HttpContext.Current.Session["Role_Filter"].Equals(roleItem.Value))
                        {
                            roleItem.Selected = true;
                            break;
                        }
                    }
                }
                else if (_profilePreferences.ContainsKey(Role_Preference_KEY))
                {
                    foreach (ListItem roleItem in taskRoleDropDownList.Items)
                    {
                        if (_profilePreferences[Role_Preference_KEY].Equals(roleItem.Value))
                        {
                            roleItem.Selected = true;
                            break;
                        }
                    }
                }

                if (HttpContext.Current.Session["Status_Filter"] != null)
                {
                    foreach (ListItem filterItem in taskFilterDropDownList.Items)
                    {
                        if (HttpContext.Current.Session["Status_Filter"].Equals(filterItem.Value))
                        {
                            filterItem.Selected = true;
                            break;
                        }
                    }
                }
                else if (_profilePreferences.ContainsKey(Task_Filter_Preference_KEY))
                {
                    foreach (ListItem filterItem in taskFilterDropDownList.Items)
                    {
                        if (_profilePreferences[Task_Filter_Preference_KEY].Equals(filterItem.Value))
                        {
                            filterItem.Selected = true;
                            break;
                        }
                    }
                }

                loadData = true;
            }
            else
            {
                string causeId = Utility.GetPostBackControlId(Page);
                loadData = (!string.IsNullOrEmpty(causeId) && !
                    (causeId.Equals("taskRoleDropDownList") ||
                     causeId.Equals("taskFilterDropDownList") ||
                     causeId.Equals("filterButton") ||
                     causeId.Equals("RemoveAllLinkButton") ||
                     causeId.Contains("RemoveLinkButton")));
            }

            if (TeamMembersDropDownList != null && TeamMembersDropDownList.SelectedItem != null && TeamMembersDropDownList.SelectedIndex > 0)
            {
                long profileID = 0;

                if (long.TryParse(TeamMembersDropDownList.SelectedValue, out profileID))
                {
                    taskProfile = WALT.BLL.ProfileManager.GetInstance().GetProfile(profileID);
                }
            }
            else
            {
                taskProfile = _profile;
            }

            if (loadData)
            {
                LoadData();
            }

            dialogPanel.Visible = false;
            popupAlert.Visible = false;
        }

        private void LoadData()
        {
            string taskDuty = taskRoleDropDownList.SelectedValue.ToLower();

            string taskFilter = taskFilterDropDownList.SelectedValue.ToLower();
            DTO.Task.StatusEnum? taskStatus = null;

            switch (taskFilter)
            {
                case "open":
                    taskStatus = DTO.Task.StatusEnum.OPEN;
                    break;
                case "completed":
                    taskStatus = DTO.Task.StatusEnum.COMPLETED;
                    break;
                case "hold":
                    taskStatus = DTO.Task.StatusEnum.HOLD;
                    break;
                case "obe":
                    taskStatus = DTO.Task.StatusEnum.OBE;
                    break;
                case "rejected":
                    taskStatus = DTO.Task.StatusEnum.REJECTED;
                    break;
            }

            TaskGrid1.ProfilePreferences = _profilePreferences;

            if (taskDuty.Equals("assignee"))
            {
                TaskGrid1.InitControl(taskProfile, null, taskStatus, _filters);
            }
            else
            {
                TaskGrid1.InitControl(null, taskProfile, taskStatus, _filters);
            }           
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            if (TeamDropDownList.SelectedItem != null && TeamDropDownList.SelectedIndex != 0)
            {
                HttpContext.Current.Session["Team_Filter"] = TeamDropDownList.SelectedValue;
            }

            if (TeamMembersDropDownList.SelectedItem != null && TeamMembersDropDownList.SelectedIndex != 0)
            {
                HttpContext.Current.Session["TeamMember_Filter"] = TeamMembersDropDownList.SelectedValue;
            }

            if (taskRoleDropDownList.SelectedItem != null)
            {
                HttpContext.Current.Session["Role_Filter"] = taskRoleDropDownList.SelectedValue;
            }

            if (taskFilterDropDownList.SelectedItem != null)
            {
                HttpContext.Current.Session["Status_Filter"] = taskFilterDropDownList.SelectedValue;
            }

           


            
            if (_profilePreferences != null)
            {
                if ((_profilePreferences.ContainsKey(Role_Preference_KEY) && _profilePreferences.ContainsKey(Task_Filter_Preference_KEY)) &&
                    (taskRoleDropDownList.SelectedValue.Equals(_profilePreferences[Role_Preference_KEY]) &&
                        taskFilterDropDownList.SelectedValue.Equals(_profilePreferences[Task_Filter_Preference_KEY])))
                {
                    btnSaveView.Visible = false;
                }
                else
                {
                    btnSaveView.Visible = true;
                }
            }
        }

        protected void btnSaveView_Click(object sender, EventArgs e)
        {
            try
            {
                WALT.BLL.ProfileManager.GetInstance().SavePreference(Role_Preference_KEY, taskRoleDropDownList.SelectedValue);
                WALT.BLL.ProfileManager.GetInstance().SavePreference(Task_Filter_Preference_KEY, taskFilterDropDownList.SelectedValue);

                _profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();
                _profilePreferences = _profile.Preferences;

                btnSaveView.Visible = false;
            }
            catch
            {
                btnSaveView.Visible = true;
            }
        }

        protected void taskFilters_SelectedIndexChanged(object sender, EventArgs e)
        {            
            LoadData();
        }

        protected void DirDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {            
           
        }       

        protected void TeamMembersDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {            
            LoadData();
        }

        protected void TeamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindTeamMembersDropDown();
            LoadData();
        }

        protected void BindTeamDropDown()
        {
            List<KeyValuePair<long, string>> teams = BLL.AdminManager.GetInstance().GetTeamsOwnedDictionary(_profile).ToList();

            List<long> directoratesId = BLL.AdminManager.GetInstance().GetDirectoratesManagedIds(_profile);

            foreach (long dirId in directoratesId)
            {
                Dictionary<long, string> dirTeams = BLL.AdminManager.GetInstance().GetTeamsDictionaryByParent(dirId);

                foreach (KeyValuePair<long, string> dirPair in dirTeams)
                {
                    bool foundIt = false;

                    foreach (KeyValuePair<long, string> teamPair in teams)
                    {
                        if (teamPair.Key == dirPair.Key)
                        {
                            foundIt = true;
                            break;
                        }
                    }

                    if (!foundIt)
                        teams.Add(new KeyValuePair<long, string>(dirPair.Key, dirPair.Value));
                }
            }

            teams.Sort(delegate(KeyValuePair<long, string> x, KeyValuePair<long, string> y) { return x.Value.CompareTo(y.Value); });

            foreach (KeyValuePair<long, string> pair in teams)
            {
                TeamDropDownList.Items.Insert(TeamDropDownList.Items.Count, new ListItem(pair.Value, pair.Key.ToString()));
            }

            if (TeamDropDownList.Items.Count == 1)
            {
                TeamDropDownList.SelectedIndex = 0;
            }

            TeamDropDownList.Items.Insert(0, new ListItem("Select a team...", "0"));

            if (TeamDropDownList.Items.Count > 1)
                TeamPlaceHolder.Visible = true;
            else
                TeamPlaceHolder.Visible = false;
        }

        protected void BindTeamMembersDropDown()
        {
            long teamID = 0;
            TeamMembersDropDownList.Items.Clear();

            if (TeamDropDownList.SelectedItem != null)
            {
                if (long.TryParse(TeamDropDownList.SelectedValue, out teamID))
                {
                    List<DTO.Profile> teamMembers = BLL.AdminManager.GetInstance().GetTeamMembers(teamID);

                    foreach (DTO.Profile _member in teamMembers)
                    {
                        TeamMembersDropDownList.Items.Insert(TeamMembersDropDownList.Items.Count, new ListItem(_member.DisplayName, _member.Id.ToString()));
                    }

                    TeamMembersDropDownList.Items.Insert(0, new ListItem("Select a member...", "0"));

                    TeamMembersPlaceHolder.Visible = true;
                }
            }
            else
            {
                TeamMembersPlaceHolder.Visible = true;
            }
        }
              

        protected void filterPickDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (filterPickDropDownList.SelectedItem != null && filterPickDropDownList.SelectedIndex > 0) 
            {
                filterPickDropDownList.Items[filterPickDropDownList.SelectedIndex].Enabled = false;

                switch (filterPickDropDownList.SelectedValue)
                {
                    case "OWNER":
                        ownerFilterRow.Visible = true;
                        break;
                    case "ASSIGNED":
                        assigneeFilterRow.Visible = true;
                        break;
                    case "TITLE":
                        titleFilterRow.Visible = true;
                        break;
                    case "TASKTYPE":
                        typeFilterRow.Visible = true;
                        break;
                    case "SOURCE":
                        sourceFilterRow.Visible = true;
                        break;
                    case "SOURCEID":
                        sourceIdFilterRow.Visible = true;
                        break;
                    case "STARTDATE":
                        startFilterRow.Visible = true;
                        break;
                    case "DUEDATE":
                        dueFilterRow.Visible = true;
                        break;
                    case "STATUS":
                        statusFilterRow.Visible = true;
                        break;
                    case "HOURS":
                        hoursFilterRow.Visible = true;
                        break;                    
                    case "ESTIMATE":
                        reFilterRow.Visible = true;
                        break;
                    case "COMPLEXITY":
                        complexityFilterRow.Visible = true;
                        break;
                    case "PROGRAM":
                        if (programDropDownList.Items.Count <= 0)
                            BindProgDropDown();

                        programFilterRow.Visible = true;
                        break;
                    case "CREATED":
                        createdFilterRow.Visible = true;
                        break;
                    case "COMPLETEDDATE":
                        completeFilterRow.Visible = true;
                        break;
                    case "ONHOLDDATE":
                        holdFilterRow.Visible = true;
                        break;
                    case "INSTANTIATED":
                        instFilterRow.Visible = true;
                        break;                   
                    case "EXITCRITERIA":
                        exitFilterRow.Visible = true;
                        break;
                    case "WBS":
                        wbsFilterRow.Visible = true;
                        break;
                    case "OWNERCOMMENTS":
                        ownerCommFilterRow.Visible = true;
                        break;
                    case "ASSIGNEECOMMENTS":
                        assigneeCommFilterRow.Visible = true;
                        break;
                }

                RemoveAllLinkButton.Visible = true;
                filterButton.Visible = true;
                updateLabel.Visible = false;

                filterUpdatePanel.Update();
            }
        }

        protected void filterButton_Click(object sender, EventArgs e)
        {
            _filters = new Dictionary<DTO.Task.ColumnEnum, string>();

            if (ownerFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(ownerFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.OWNER, ownerFilterTextBox.Text);
                }
            }

            if (assigneeFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(assigneeFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.ASSIGNED, assigneeFilterTextBox.Text);
                }
            }

            if (titleFilterRow.Visible) 
            {
                if (!string.IsNullOrEmpty(titleFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.TITLE, titleFilterTextBox.Text);
                }
            }

            if (typeFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(typeFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.TASKTYPE, typeFilterTextBox.Text);
                }
            }

            if (sourceFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(sourceFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.SOURCE, sourceFilterTextBox.Text);
                }
            }

            if (sourceIdFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(sourceIdFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.SOURCEID, sourceIdFilterTextBox.Text);
                }
            }

            if (startFilterRow.Visible)
            {
                string strdate = string.Empty;
                DateTime sdate;

                if (DateTime.TryParse(startAfterFilterTextBox.Text, out sdate))
                {
                    strdate = ">" + sdate.ToShortDateString();
                }

                if (DateTime.TryParse(startBeforeFilterTextBox.Text, out sdate))
                {
                    strdate += strdate.Length > 0 ? "," : "";
                    strdate += "<" + sdate.ToShortDateString();
                }

                if (!string.IsNullOrEmpty(strdate))
                {
                    _filters.Add(DTO.Task.ColumnEnum.STARTDATE, strdate);
                }
            }

            if (dueFilterRow.Visible)
            {
                string strdate = string.Empty;
                DateTime ddate;

                if (DateTime.TryParse(dueAfterFilterTextBox.Text, out ddate))
                {
                    strdate = ">" + ddate.ToShortDateString();
                }

                if (DateTime.TryParse(dueBeforeFilterTextBox.Text, out ddate))
                {
                    strdate += strdate.Length > 0 ? "," : "";
                    strdate += "<" + ddate.ToShortDateString();
                }

                if (!string.IsNullOrEmpty(strdate))
                {
                    _filters.Add(DTO.Task.ColumnEnum.DUEDATE, strdate);
                }
            }

            if (statusFilterRow.Visible) 
            {
                if (statusRadioButtonList.SelectedItem != null) 
                {
                    _filters.Add(DTO.Task.ColumnEnum.STATUS, statusRadioButtonList.SelectedValue);
                }
            }

            if (hoursFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(hoursFilterTextBox.Text))
                {
                    string hrsStr = string.Empty;
                    if (hrsOprDropDownList.SelectedItem != null)
                    {
                        switch (hrsOprDropDownList.SelectedValue) 
                        {
                            case ("Equal"):
                                hrsStr = "=";
                                break;
                            case ("Greater"):
                                hrsStr = ">";
                                break;
                            case ("Less"):
                                hrsStr = "<";
                                break;
                        }
                        _filters.Add(DTO.Task.ColumnEnum.HOURS, hrsStr + hoursFilterTextBox.Text);
                    }
                }
            }
                        
            if (reFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(reFilterTextBox.Text))
                {
                    string reStr = string.Empty;
                    if (reOprDropDownList.SelectedItem != null)
                    {
                        switch (reOprDropDownList.SelectedValue)
                        {
                            case ("Equal"):
                                reStr = "=";
                                break;
                            case ("Greater"):
                                reStr = ">";
                                break;
                            case ("Less"):
                                reStr = "<";
                                break;
                        }
                        _filters.Add(DTO.Task.ColumnEnum.ESTIMATE, reStr + reFilterTextBox.Text);
                    }
                }
            }

            if (complexityFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(complexityFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.COMPLEXITY, complexityFilterTextBox.Text);
                }
            }

            if (programFilterRow.Visible)
            {
                if ( programDropDownList.SelectedItem != null&& !string.IsNullOrEmpty( programDropDownList.SelectedValue))
                {
                    _filters.Add(DTO.Task.ColumnEnum.PROGRAM, programDropDownList.Text);
                }
            }

            if (createdFilterRow.Visible)
            {
                string strdate = string.Empty;
                DateTime cdate;

                if (DateTime.TryParse(createdAfterFilterTextBox.Text, out cdate))
                {
                    strdate = ">" + cdate.ToShortDateString();
                }

                if (DateTime.TryParse(createdBeforeFilterTextBox.Text, out cdate))
                {
                    strdate += strdate.Length > 0 ? "," : "";
                    strdate += "<" + cdate.ToShortDateString();
                }

                if (!string.IsNullOrEmpty(strdate))
                {
                    _filters.Add(DTO.Task.ColumnEnum.CREATED, strdate);
                }
            }

            if (completeFilterRow.Visible)
            {
                string strdate = string.Empty;
                DateTime cdate;

                if (DateTime.TryParse(completeAfterFilterTextBox.Text, out cdate))
                {
                    strdate = ">" + cdate.ToShortDateString();
                }

                if (DateTime.TryParse(completeBeforeFilterTextBox.Text, out cdate))
                {
                    strdate += strdate.Length > 0 ? "," : "";
                    strdate += "<" + cdate.ToShortDateString();
                }

                if (!string.IsNullOrEmpty(strdate))
                {
                    _filters.Add(DTO.Task.ColumnEnum.COMPLETEDDATE, strdate);
                }
            }

            if (holdFilterRow.Visible)
            {
                string strdate = string.Empty;
                DateTime cdate;

                if (DateTime.TryParse(holdAfterFilterTextBox.Text, out cdate))
                {
                    strdate = ">" + cdate.ToShortDateString();
                }

                if (DateTime.TryParse(holdBeforeFilterTextBox.Text, out cdate))
                {
                    strdate += strdate.Length > 0 ? "," : "";
                    strdate += "<" + cdate.ToShortDateString();
                }

                if (!string.IsNullOrEmpty(strdate))
                {
                    _filters.Add(DTO.Task.ColumnEnum.ONHOLDDATE, strdate);
                }
            }

            if (instFilterRow.Visible)
            {
                if ( instFilterRadioButtonList.SelectedItem != null)
                {
                    _filters.Add(DTO.Task.ColumnEnum.INSTANTIATED, instFilterRadioButtonList.SelectedValue);
                }
            }
            
            if (exitFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(exitFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.EXITCRITERIA, exitFilterTextBox.Text);
                }
            }

            if (wbsFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(wbsFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.WBS, wbsFilterTextBox.Text);
                }
            }

            if (ownerCommFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(ownerCommFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.OWNERCOMMENTS, ownerCommFilterTextBox.Text);
                }
            }

            if (assigneeCommFilterRow.Visible)
            {
                if (!string.IsNullOrEmpty(assigneeCommFilterTextBox.Text))
                {
                    _filters.Add(DTO.Task.ColumnEnum.ASSIGNEECOMMENTS, assigneeCommFilterTextBox.Text);
                }
            }

           
            updateLabel.Visible = true;

            LoadData();

            filterUpdatePanel.Update();
        }

        protected void BindProgDropDown()
        {
            programDropDownList.Items.Clear();

            Dictionary<long, string> programs = BLL.AdminManager.GetInstance().GetProgramDictionary();

            programDropDownList.DataSource = programs;
            programDropDownList.DataTextField = "Value";
            programDropDownList.DataValueField = "Key";
            programDropDownList.DataBind();

            programDropDownList.Items.Insert(0, new ListItem("Select a Program...", "none"));
        }

        protected void RemoveLinkButton_Click(object sender, EventArgs e)
        {
            LinkButton myButton = sender as LinkButton;
            string command = myButton.CommandArgument;

            switch (command)
            {
                case "OWNER":
                    ownerFilterRow.Visible = false;
                    ownerCommFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.OWNER))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.OWNER);
                    }
                    break;
                case "ASSIGNED":
                    assigneeFilterRow.Visible = false;
                    assigneeCommFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.ASSIGNED))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.ASSIGNED);
                    }
                    break;
                case "TITLE":
                    titleFilterRow.Visible = false;
                    titleFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.TITLE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.TITLE);
                    }
                    break;
                case "TASKTYPE":
                    typeFilterRow.Visible = false;
                    typeFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.TASKTYPE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.TASKTYPE);
                    }
                    break;
                case "SOURCE":
                    sourceFilterRow.Visible = false;
                    sourceFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.SOURCE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.SOURCE);
                    }
                    break;
                case "SOURCEID":
                    sourceIdFilterRow.Visible = false;
                    sourceIdFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.SOURCEID))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.SOURCEID);
                    }
                    break;
                case "STARTDATE":
                    startFilterRow.Visible = false;
                    startAfterFilterTextBox.Text = string.Empty;
                    startBeforeFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.STARTDATE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.STARTDATE);
                    }
                    break;
                case "DUEDATE":
                    dueFilterRow.Visible = false;
                    dueAfterFilterTextBox.Text = string.Empty;
                    dueBeforeFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.DUEDATE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.DUEDATE);
                    }
                    break;
                case "STATUS":
                    statusFilterRow.Visible = false;
                    statusRadioButtonList.ClearSelection();
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.STATUS))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.STATUS);
                    }
                    break;
                case "HOURS":
                    hoursFilterRow.Visible = false;
                    hoursFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.HOURS))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.HOURS);
                    }
                    break;               
                case "ESTIMATE":
                    reFilterRow.Visible = false;
                    reFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.ESTIMATE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.ESTIMATE);
                    }
                    break;
                case "COMPLEXITY":
                    complexityFilterRow.Visible = false;
                    complexityFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.COMPLEXITY))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.COMPLEXITY);
                    }
                    break;
                case "PROGRAM":
                    programFilterRow.Visible = false;
                    programDropDownList.ClearSelection();
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.PROGRAM))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.PROGRAM);
                    }
                    break;
                case "CREATED":
                    createdFilterRow.Visible = false;
                    createdAfterFilterTextBox.Text = string.Empty;
                    createdBeforeFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.CREATED))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.CREATED);
                    }
                    break;
                case "COMPLETEDDATE":
                    completeFilterRow.Visible = false;
                    completeAfterFilterTextBox.Text = string.Empty;
                    completeBeforeFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.COMPLETEDDATE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.COMPLETEDDATE);
                    }
                    break;
                case "ONHOLDDATE":
                    holdFilterRow.Visible = false;
                    holdAfterFilterTextBox.Text = string.Empty;
                    holdBeforeFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.ONHOLDDATE))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.ONHOLDDATE);
                    }
                    break;
                case "INSTANTIATED":
                    instFilterRow.Visible = false;
                    instFilterRadioButtonList.ClearSelection();
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.INSTANTIATED))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.INSTANTIATED);
                    }
                    break;                
                case "EXITCRITERIA":
                    exitFilterRow.Visible = false;
                    exitFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.EXITCRITERIA))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.EXITCRITERIA);
                    }
                    break;
                case "WBS":
                    wbsFilterRow.Visible = false;
                    wbsFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.WBS))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.WBS);
                    }
                    break;
                case "OWNERCOMMENTS":
                    ownerCommFilterRow.Visible = false;
                    ownerFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.OWNERCOMMENTS))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.OWNERCOMMENTS);
                    }
                    break;
                case "ASSIGNEECOMMENTS":
                    assigneeCommFilterRow.Visible = false;
                    assigneeCommFilterTextBox.Text = string.Empty;
                    if (_filters != null && _filters.ContainsKey(DTO.Task.ColumnEnum.ASSIGNEECOMMENTS))
                    {
                        _filters.Remove(DTO.Task.ColumnEnum.ASSIGNEECOMMENTS);
                    }
                    break;
            }

            foreach (ListItem item in filterPickDropDownList.Items) 
            {
                if(command.Equals(item.Value))
                {
                    item.Enabled = true;
                    break;
                }
            }

           
            updateLabel.Visible = true;            
            filterUpdatePanel.Update();
            if (_filters == null || _filters.Count == 0) 
            {
                RemoveAllLinkButton.Visible = false;
                filterButton.Visible = true;
            }
            LoadData();
        }

        protected void lnkAddFav_Click(object sender, EventArgs e)
        {
            List<DTO.Task> tasks = TaskGrid1.GetSelectedTasks();
            int count = tasks.Count;

            if (count > 0)
            {
                List<long> ids = new List<long>();
                string msg = string.Empty;

                for (int i = 0; i < count; i++)
                {
                    ids.Add(tasks[i].Id);
                    msg += tasks[i].Title + "<br>";
                }

                try
                {
                    WALT.BLL.TaskManager.GetInstance().SaveTasksAsFavorites(ids);
                    Utility.DisplayInfoMessage(TaskListToMsg(msg) + " added to favorites");
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage(ex.Message);
                }
            }
        }

        private void PopupForm(Panel form)
        {
            form.Visible = true;

            if (form.ID != "dialogPanel")
            {
                dialogPanel.Visible = false;
            }

            if (form.ID != "popupAlert")
            {
                popupAlert.Visible = false;
            }

            phUpdatePanel.Controls.Add(new LiteralControl(
                "<input type=\"hidden\" id=\"popupForm\" name=\"popupForm\" value=\"" + form.ID + "\" />"));
        }

        protected void lnkAlert_Click(object sender, EventArgs e)
        {
            List<DTO.Task> tasks = TaskGrid1.GetSelectedTasks();

            if (tasks.Count == 1)
            {
                DTO.Task task = tasks[0];
                DTO.Profile user = BLL.ProfileManager.GetInstance().GetProfile();
                ViewState["alertTaskID"] = task.Id;

                if (task.Assigned.Id == user.Id)
                {
                    bool single = true;
                    long mgrID = 0;
                    DTO.Team team = BLL.AdminManager.GetInstance().GetTeam();

                    if (user.Manager != null && team.Owner.Id != user.Manager.Id)
                    {
                        cellAlertMgr.Visible = true;
                        alertMgr.Text = "FM: " + user.Manager.DisplayName;
                        alertMgr.Checked = false;
                        mgrID = user.Manager.Id;
                        single = false;
                    }
                    else
                    {
                        cellAlertMgr.Visible = false;
                    }

                    if (task.Owner.Id == user.Id && task.ParentId != 0)
                    {
                        task = BLL.TaskManager.GetInstance().GetTask(task.ParentId, false);
                    }

                    if (task.Owner.Id != user.Id && task.Owner.Id != team.Owner.Id && task.Owner.Id != mgrID)
                    {
                        cellAlertOwner.Visible = true;
                        alertOwner.Text = "Owner: " + task.Owner.DisplayName;
                        alertOwner.Checked = false;
                        ViewState["alertOwnerID"] = task.Owner.Id;
                        single = false;
                    }
                    else
                    {
                        cellAlertOwner.Visible = false;
                    }

                    cellAlertALM.Visible = true;
                    alertALM.Checked = single;
                    alertALM.Visible = !single;
                    lblAlertALM.Visible = single;
                    alertALM.Text = "ALM: " + team.Owner.DisplayName;
                    lblAlertALM.Text = alertALM.Text;
                    ViewState["alertAlmID"] = team.Owner.Id;

                    cellAlertAssignee.Visible = false;
                }
                else
                {
                    cellAlertAssignee.Visible = true;
                    cellAlertAssignee.Text = task.Assigned.DisplayName;
                    ViewState["alertAssignID"] = task.Assigned.Id;

                    cellAlertALM.Visible = false;
                    cellAlertMgr.Visible = false;
                    cellAlertOwner.Visible = false;
                }

                cellAlertTaskTitle.InnerText = task.Title;
                PopupForm(popupAlert);
            }
        }

        protected void btnCreateAlert_Click(object sender, EventArgs e)
        {
            DTO.Alert alert = new DTO.Alert();
            alert.Subject = alertSubject.Text;
            alert.Message = alertMessage.Text;
            alert.Creator = BLL.ProfileManager.GetInstance().GetProfile();
            alert.EntryDate = DateTime.Now;

            alert.LinkedId = (long)ViewState["alertTaskID"];
            alert.LinkedType = DTO.Alert.AlertEnum.TASK;
            popupAlert.Visible = true;

            try
            {
                string msg = "Alert sent to ";

                if (cellAlertAssignee.Visible)
                {
                    alert.Profile = BLL.ProfileManager.GetInstance().GetProfile((long)ViewState["alertAssignID"]);
                    msg += alert.Profile.DisplayName;
                    BLL.ProfileManager.GetInstance().SaveAlert(alert);
                    ViewState.Remove("alertAssignID");
                }
                else
                {
                    List<DTO.Profile> sendTo = new List<DTO.Profile>();

                    if (lblAlertALM.Visible || alertALM.Checked)
                    {
                        sendTo.Add(BLL.ProfileManager.GetInstance().GetProfile((long)ViewState["alertAlmID"]));
                    }

                    if (cellAlertMgr.Visible && alertMgr.Checked)
                    {
                        sendTo.Add(alert.Creator.Manager);
                    }

                    if (cellAlertOwner.Visible && alertOwner.Checked)
                    {
                        sendTo.Add(BLL.ProfileManager.GetInstance().GetProfile((long)ViewState["alertOwnerID"]));
                    }

                    BLL.ProfileManager.GetInstance().SendAlert(alert, sendTo);
                    ViewState.Remove("alertAlmID");
                    ViewState.Remove("alertOwnerID");

                    for (int i = 0; i < sendTo.Count; i++)
                    {
                        if (i > 0 && sendTo.Count > 2) msg += ", ";
                        if (sendTo.Count > 1 && i == sendTo.Count - 1) msg += " and ";
                        msg += sendTo[i].DisplayName;
                    }
                }

                ViewState.Remove("alertTaskID");
                alertSubject.Text = string.Empty;
                alertMessage.Text = string.Empty;
                popupAlert.Visible = false;

                Utility.DisplayInfoMessage(msg + ".");
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("Creating Alert Failed:<br>" + ex.Message);
            }
        }

        protected void lnkReassign_Click(object sender, EventArgs e)
        {

        }

        protected void lnkUpdOwner_Click(object sender, EventArgs e)
        {

        }

        protected void lnkReject_Click(object sender, EventArgs e)
        {
            List<DTO.Task> tasks = TaskGrid1.GetSelectedTasks();

            if (tasks.Count > 0)
            {
                ArrayList rejectIDs = new ArrayList();
                string invalidTasks = string.Empty;
                long profileID = WALT.BLL.ProfileManager.GetInstance().GetProfile().Id;
                dialogBody.Text = string.Empty;

                foreach (DTO.Task task in tasks)
                {
                    if (BLL.TaskManager.GetInstance().IsTaskAllowedToBeRejected(task, null))
                    {
                        dialogBody.Text += task.Title + "<br>";
                        rejectIDs.Add(task.Id);
                    }
                    else
                    {
                        invalidTasks += task.Title + "<br>";
                    }
                }

                if (invalidTasks != string.Empty)
                {
                    invalidTasks += "<br>Tasks can only be rejected if they are open, assigned to you, not owned by you, and not included in a plan/log.";
                }

                if (rejectIDs.Count > 0)
                {
                    dialogHdr.Text = "<b>Are you sure you want to reject the following selected task(s)?</b>";

                    if (invalidTasks != string.Empty)
                    {
                        dialogExtraRow.Visible = true;
                        dialogExtra.Text = "<b>The following selected task(s) can not be rejected:</b><br>" + invalidTasks;
                    }
                    else
                    {
                        dialogExtraRow.Visible = false;
                    }

                    dialogBtn.Visible = true;
                    dialogBtn.Text = "Reject Task(s)";
                    dialogBtnClose.Value = "Cancel";
                    ViewState["taskIDs"] = rejectIDs;
                }
                else
                {
                    dialogHdr.Text = "<b>None of the selected tasks can be rejected:</b>";
                    dialogBody.Text = invalidTasks;
                    dialogExtraRow.Visible = false;
                    dialogBtn.Visible = false;
                    dialogBtnClose.Value = "Close";
                }

                dialogPanel.Attributes.Add("title", "Reject Tasks");
                PopupForm(dialogPanel);
            }
        }

        protected void lnkDelete_Click(object sender, EventArgs e)
        {
            List<DTO.Task> tasks = TaskGrid1.GetSelectedTasks();

            if (tasks.Count > 0)
            {
                ArrayList deleteIDs = new ArrayList();
                string invalidTasks = string.Empty;
                long profileID = WALT.BLL.ProfileManager.GetInstance().GetProfile().Id;
                dialogBody.Text = string.Empty;

                foreach (DTO.Task task in tasks)
                {
                    if (BLL.TaskManager.GetInstance().IsTaskAllowedToBeDeleted(task, null))
                    {
                        dialogBody.Text += task.Title + "<br>";
                        deleteIDs.Add(task.Id);
                    }
                    else
                    {
                        invalidTasks += task.Title + "<br>";
                    }
                }

                if (invalidTasks != string.Empty)
                {
                    invalidTasks += "<br>Tasks can only be deleted if they are owned by you and it and all its children are not included in a plan/log.";
                }

                if (deleteIDs.Count > 0)
                {
                    dialogHdr.Text = "<b>Are you sure you want to delete the following selected task(s)?</b>";

                    if (invalidTasks != string.Empty)
                    {
                        dialogExtraRow.Visible = true;
                        dialogExtra.Text = "<b>The following selected task(s) can not be deleted:</b><br>" + invalidTasks;
                    }
                    else
                    {
                        dialogExtraRow.Visible = false;
                    }

                    dialogBtn.Visible = true;
                    dialogBtn.Text = "Delete Task(s)";
                    dialogBtnClose.Value = "Cancel";
                    ViewState["taskIDs"] = deleteIDs;
                }
                else
                {
                    dialogHdr.Text = "<b>None of the selected tasks can be deleted:</b>";
                    dialogBody.Text = invalidTasks;
                    dialogExtraRow.Visible = false;
                    dialogBtn.Visible = false;
                    dialogBtnClose.Value = "Close";
                }

                dialogPanel.Attributes.Add("title", "Delete Tasks");
                PopupForm(dialogPanel);
            }
        }

        protected void dialogBtn_Click(object sender, EventArgs e)
        {
            ArrayList taskIDs = (ArrayList)ViewState["taskIDs"];

            List<long> ids = new List<long>();

            foreach (long id in taskIDs)
            {
                ids.Add(id);
            }

            try
            {
                if (dialogBtn.Text == "Reassign Task(s)")
                {

                }
                else if (dialogBtn.Text == "Reject Task(s)")
                {
                    WALT.BLL.TaskManager.GetInstance().RejectTasks(ids);

                    Utility.DisplayInfoMessage(TaskListToMsg(dialogBody.Text) +
                        (ids.Count == 1 ? " was" : " were") + " rejected.");
                }
                else if (dialogBtn.Text == "Delete Task(s)")
                {
                    WALT.BLL.TaskManager.GetInstance().DeleteTasks(ids);

                    Utility.DisplayInfoMessage(TaskListToMsg(dialogBody.Text) +
                        (ids.Count == 1 ? " was" : " were") + " deleted.");
                }
                else if (dialogHdr.Text.Contains("owner"))
                {

                }
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage(ex.Message);
            }
        }

        private string TaskListToMsg(string tasks)
        {
            string msg = string.Empty;
            string[] split = tasks.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                if (i > 0 && split.Length > 2) msg += ", ";
                if (split.Length > 1 && i == split.Length - 1) msg += " and ";
                msg += split[i];
            }

            return msg;
        }

        protected void RemoveAllLinkButton_Click(object sender, EventArgs e)
        {
            if (_filters != null)
                _filters.Clear();

            if (ownerFilterRow.Visible)
            {
                ownerFilterRow.Visible = false;
                ownerCommFilterTextBox.Text = string.Empty;
            }

            if (assigneeFilterRow.Visible)
            {
                assigneeFilterRow.Visible = false;
                assigneeCommFilterTextBox.Text = string.Empty;
            }
            if (titleFilterRow.Visible)
            {
                titleFilterRow.Visible = false;
                titleFilterTextBox.Text = string.Empty;
            }
            if (typeFilterRow.Visible)
            {
                typeFilterRow.Visible = false;
                typeFilterTextBox.Text = string.Empty;
            }
            if (sourceFilterRow.Visible)
            {
                sourceFilterRow.Visible = false;
                sourceFilterTextBox.Text = string.Empty;
            }
            if (sourceIdFilterRow.Visible)
            {
                sourceIdFilterRow.Visible = false;
                sourceIdFilterTextBox.Text = string.Empty;
            }
            if (startFilterRow.Visible)
            {
                startFilterRow.Visible = false;
                startAfterFilterTextBox.Text = string.Empty;
                startBeforeFilterTextBox.Text = string.Empty;
            }
            if (dueFilterRow.Visible)
            {
                dueFilterRow.Visible = false;
                dueAfterFilterTextBox.Text = string.Empty;
                dueBeforeFilterTextBox.Text = string.Empty;
            }
            if (statusFilterRow.Visible)
            {
                statusFilterRow.Visible = false;
                statusRadioButtonList.ClearSelection();
            }
            if (hoursFilterRow.Visible)
            {
                hoursFilterRow.Visible = false;
                hoursFilterTextBox.Text = string.Empty;
            }            
            if (reFilterRow.Visible)
            {
                reFilterRow.Visible = false;
                reFilterTextBox.Text = string.Empty;
            }
            if (complexityFilterRow.Visible)
            {
                complexityFilterRow.Visible = false;
                complexityFilterTextBox.Text = string.Empty;
            }
            if (programFilterRow.Visible)
            {
                programFilterRow.Visible = false;
                programDropDownList.ClearSelection();
            }
            if (createdFilterRow.Visible)
            {
                createdFilterRow.Visible = false;
                createdAfterFilterTextBox.Text = string.Empty;
                createdBeforeFilterTextBox.Text = string.Empty;
            }
            if (completeFilterRow.Visible)
            {
                completeFilterRow.Visible = false;
                completeAfterFilterTextBox.Text = string.Empty;
                completeBeforeFilterTextBox.Text = string.Empty;
            }
            if (holdFilterRow.Visible)
            {
                holdFilterRow.Visible = false;
                holdAfterFilterTextBox.Text = string.Empty;
                holdBeforeFilterTextBox.Text = string.Empty;
            }
            if (instFilterRow.Visible)
            {
                instFilterRow.Visible = false;
                instFilterRadioButtonList.ClearSelection();
            }            
            if (exitFilterRow.Visible)
            {
                exitFilterRow.Visible = false;
                exitFilterTextBox.Text = string.Empty;
            }
            if (wbsFilterRow.Visible)
            {
                wbsFilterRow.Visible = false;
                wbsFilterTextBox.Text = string.Empty;
            }
            if (ownerCommFilterRow.Visible)
            {
                ownerCommFilterRow.Visible = false;
                ownerFilterTextBox.Text = string.Empty;
            }
            if (assigneeCommFilterRow.Visible)
            {
                assigneeCommFilterRow.Visible = false;
                assigneeCommFilterTextBox.Text = string.Empty;
            }

            foreach (ListItem item in filterPickDropDownList.Items)
            {
                item.Enabled = true;
            }

            RemoveAllLinkButton.Visible = false;
            filterButton.Visible = false;
            updateLabel.Visible = true;
           
            filterUpdatePanel.Update();
            LoadData();
        }

        
    }
}