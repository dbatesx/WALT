using System;
using System.Collections;
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
using System.Drawing;
using WALT.DTO;

namespace WALT.UIL
{
    public partial class Weekly : System.Web.UI.Page
    {
        private bool myShowWE;
        private bool myModError;
        private string myControlPrefix;
        private ArrayList myPlanIDs;
        private ArrayList myUnplanIDs;
        private ArrayList myRemoveIDs;
        private List<long> myCarryIDs;
        private List<double> myPlanTotals;
        private List<double> myActualTotals;
        private List<double> myUnplanTotals;
        private List<double> myBarrierTotals;
        private List<double> myDelayTotals;
        private Dictionary<string, Control> myControls;

        protected void Page_Load(object sender, EventArgs e)
        {
            myControls = new Dictionary<string, Control>();
            myShowWE = false;
            myModError = false;
            myControlPrefix = string.Empty;
            myCarryIDs = null;

            if (!IsPostBack || HttpContext.Current.Request["clearSession"] == "y")
            {
                long profileID = -1;
                Profile member = null;
                DateTime week = DateTime.Now.Date;
                bool loadSession = false;
                bool weekRequest = false;

                if (HttpContext.Current.Request["id"] != null)
                {
                    long id;

                    if (long.TryParse(HttpContext.Current.Request["id"], out id))
                    {
                        WeeklyPlan wp = BLL.TaskManager.GetInstance().GetWeeklyPlan(id, true);
                        profileID = wp.Profile.Id;
                        week = wp.WeekEnding;
                        weekRequest = true;
                    }
                }
                else
                {
                    if (HttpContext.Current.Request["profileID"] != null)
                    {
                        long.TryParse(HttpContext.Current.Request["profileID"], out profileID);
                    }

                    if (HttpContext.Current.Request["week"] != null&&
                        DateTime.TryParse(HttpUtility.UrlDecode(HttpContext.Current.Request["week"]), out week))
                    {
                        weekRequest = true;
                    }
                }

                if (CheckSessionKey("modError"))
                {
                    myModError = true;
                }
                else if (CheckSessionKey("profileID") && HttpContext.Current.Request["clearSession"] != "y")
                {
                    long profileCheck = long.Parse(GetSessionValue("profileID"));
                    DateTime weekCheck = DateTime.Parse(GetSessionValue("week"));

                    while (week.DayOfWeek != DayOfWeek.Sunday)
                    {
                        week = week.AddDays(1);
                    }

                    if (CheckSessionKey("updated"))
                    {
                        loadSession = true;

                        if ((profileID != -1 && profileCheck != profileID) || (weekRequest && week != weekCheck))
                        {
                            Profile planProfile = BLL.ProfileManager.GetInstance().GetProfile(profileCheck);
                            LiteralControl script = new LiteralControl("<script type=\"text/javascript\">\n");

                            script.Text += @"
if (confirm('Viewing the requested plan/log will undo changes made to the " + weekCheck.ToString("M/d/yy") + " plan/log for " + planProfile.DisplayName + @", proceed?'))
{
    document.getElementById('clearSession').value = 'y';
    __doPostBack('', '');
}
</script>";

                            phHeader.Controls.Add(script);
                        }

                        profileID = profileCheck;
                        week = weekCheck;
                    }
                    else if (profileID == -1)
                    {
                        profileID = profileCheck;
                        week = weekCheck;
                    }
                }

                LoadWeeks(week);

                if (profileID != -1)
                {
                    member = BLL.ProfileManager.GetInstance().GetProfile(profileID);
                }
                else
                {
                    member = BLL.ProfileManager.GetInstance().GetProfile();
                }

                Team memberTeam = BLL.AdminManager.GetInstance().GetTeamByRole(member, "MEMBER");
                Team selectedTeam = null;
                long selectedUser = -1;

                if (memberTeam != null)
                {
                    selectedTeam = memberTeam;
                    selectedUser = member.Id;
                }
                else
                {
                    List<Team> adminTeams = BLL.AdminManager.GetInstance().GetTeamsOwned();

                    if (adminTeams.Count > 0)
                    {
                        selectedTeam = adminTeams[0];
                    }
                    else
                    {
                        adminTeams = BLL.AdminManager.GetInstance().GetTeams("ADMIN");

                        if (adminTeams.Count > 0)
                        {
                            selectedTeam = adminTeams[0];
                        }
                    }
                }

                Team selectedDir = null;
                List<Team> dirs = BLL.AdminManager.GetInstance().GetTeams(Team.TypeEnum.DIRECTORATE);

                foreach (Team dir in dirs)
                {
                    ddDirectorate.Items.Add(new ListItem(dir.Name, dir.Id.ToString()));

                    if (selectedTeam != null && selectedTeam.ParentId == dir.Id)
                    {
                        ddDirectorate.SelectedValue = dir.Id.ToString();
                        selectedDir = dir;
                    }
                }

                if (selectedDir != null)
                {
                    LoadTeams(selectedTeam.Id, selectedUser);
                }
                else
                {
                    ddDirectorate.Items.Insert(0, new ListItem());
                }

                if (HttpContext.Current.Session["weekly_showBarriers"] != null)
                {
                    cellToggleBarriers.Text = "Hide Barriers";
                }

                if (HttpContext.Current.Session["weekly_showWE"] != null)
                {
                    myShowWE = true;
                    lnkToggleWE.Text = "-";
                }

                LoadData(true, loadSession);
            }
            else
            {
                string show = HttpContext.Current.Request["showBarriers"];

                if (show == "n")
                {
                    HttpContext.Current.Session.Remove("weekly_showBarriers");
                    cellToggleBarriers.Text = "Show Barriers";
                }
                else if (show == "y" || HttpContext.Current.Session["weekly_showBarriers"] != null)
                {
                    cellToggleBarriers.Text = "Hide Barriers";
                    HttpContext.Current.Session["weekly_showBarriers"] = true;
                }

                show = HttpContext.Current.Request["showWE"];
                lnkToggleWE.Text = "+";

                if (show == "n")
                {
                    HttpContext.Current.Session.Remove("weekly_showWE");
                }
                else if (show == "y" || HttpContext.Current.Session["weekly_showWE"] != null)
                {
                    myShowWE = true;
                    lnkToggleWE.Text = "-";
                    HttpContext.Current.Session["weekly_showWE"] = true;
                }

                LoadData(false, false);
            }

            cellAddTask.Attributes["onclick"] = "DoPostBack('lnkAddTask')";
            cellAddPrev.Attributes["onclick"] = "DoPostBack('lnkAddPrev')";
            cellAddFav.Attributes["onclick"] = "DoPostBack('lnkAddFav')";

            popupTask.Visible = false;
            popupPrevTasks.Visible = false;
            popupBarrier.Visible = false;
            popupAlert.Visible = false;
            popupCarry.Visible = false;
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (IsPostBack &&
                HttpContext.Current.Request["redirectURL"] != string.Empty &&
                Utility.GetPostBackControlId(this) == string.Empty)
            {
                StoreChanges();
                string url = HttpContext.Current.Request["redirectURL"];

                if (!url.Contains("javascript"))
                {
                    Response.Redirect(HttpContext.Current.Request["redirectURL"]);
                }
            }

            if (myModError)
            {
                Utility.DisplayErrorMessage("The plan/log was modified by another user or in another window and has been reloaded from the database.");
            }
        }

        private DateTime LoadWeeks(DateTime week)
        {
            while (week.DayOfWeek != DayOfWeek.Sunday)
            {
                week = week.AddDays(1);
            }

            ddWeek.Items.Clear();

            for (int i = 4; i > -5; i--)
            {
                DateTime date = week.AddDays(i * 7);
                ListItem op = new ListItem(
                    date.AddDays(-6).ToString("M/d/yy") + " - " + date.ToString("M/d/yy"),
                    date.ToShortDateString());

                ddWeek.Items.Add(op);
            }

            ddWeek.SelectedValue = week.ToShortDateString();
            weekIndex.Value = ddWeek.SelectedIndex.ToString();

            if (ddTeam.SelectedValue != string.Empty)
            {
                long id = -1;

                if (ddProfile.SelectedValue != string.Empty)
                {
                    id = long.Parse(ddProfile.SelectedValue);
                }

                LoadUsers(id);
            }

            return week.Date;
        }

        private void LoadTeams(long selectedTeam, long selectedUser)
        {
            ddTeam.Items.Clear();
            if (ddDirectorate.SelectedValue == string.Empty) return;
            dirIndex.Value = ddDirectorate.SelectedIndex.ToString();

            long dirID = long.Parse(ddDirectorate.SelectedValue);
            List<Team> teams = BLL.AdminManager.GetInstance().GetTeamsByParent(dirID);

            foreach (Team team in teams)
            {
                ddTeam.Items.Add(new ListItem(team.Name, team.Id.ToString()));

                if (selectedTeam == team.Id)
                {
                    ddTeam.SelectedValue = team.Id.ToString();
                    hdrComp.Visible = team.ComplexityBased;
                }
            }

            if (selectedTeam == -1)
            {
                ddTeam.Items.Insert(0, new ListItem());
            }

            LoadUsers(selectedUser);
        }

        private void LoadUsers(long selectedUser)
        {
            ddProfile.Items.Clear();
            if (ddTeam.SelectedValue == string.Empty) return;
            teamIndex.Value = ddTeam.SelectedIndex.ToString();

            string selVal = string.Empty;
            long teamID = long.Parse(ddTeam.SelectedValue);
            DateTime week = DateTime.Parse(ddWeek.SelectedValue);
            List<Profile> members = BLL.AdminManager.GetInstance().GetTeamMembers(teamID, week, week, true);

            foreach (Profile user in members)
            {
                ddProfile.Items.Add(new ListItem(user.DisplayName, user.Id.ToString()));

                if (selectedUser == user.Id)
                {
                    selVal = user.Id.ToString();
                }
            }

            if (selVal != string.Empty)
            {
                ddProfile.SelectedValue = selVal;
                profileIndex.Value = ddProfile.SelectedIndex.ToString();
            }
            else
            {
                ddProfile.Items.Insert(0, new ListItem());
            }
        }
        
        private void LoadData(bool loadData, bool loadSession)
        {
            WeeklyPlan plan = null;
            Dictionary<string, WeeklyTask> taskMap = new Dictionary<string, WeeklyTask>();
            Dictionary<string, WeeklyBarrier> barrierMap = new Dictionary<string, WeeklyBarrier>();
            bool tempsLoaded = false;

            myPlanTotals = new List<double>();
            myActualTotals = new List<double>();
            myUnplanTotals = new List<double>();
            myBarrierTotals = new List<double>();
            myDelayTotals = new List<double>();

            for (int i = 0; i < 8; i++)
            {
                myPlanTotals.Add(0);
                myActualTotals.Add(0);
                myUnplanTotals.Add(0);
                myBarrierTotals.Add(0);
                myDelayTotals.Add(0);
            }

            hdrToggleWE.Visible = true;

            if (loadData)
            {
                DateTime week = DateTime.Parse(ddWeek.SelectedValue);
                long profileId = -1;
                bool owner = false;
                bool admin = false;

                if (!loadSession)
                {
                    Session.Remove("weekly_values");
                }

                ViewState.Clear();
                sessionChanges.Value = string.Empty;

                // clear viewstate of rows from previously loaded plan/log, ViewState.Clear() doesn't clear everything for some reason
                // adding empty rows causes the viewstate of previous rows to be loaded on them
                // the empty rows are removed further down before adding the real rows
                // viewstate only gets loaded once per row, so when the real rows are added the viewstate for that row won't get loaded again

                if (HttpContext.Current.Session["weekly_rows"] != null)
                {
                    int rows = (int)HttpContext.Current.Session["weekly_rows"];

                    for (int i = 0; i < rows; i++)
                    {
                        tblPlanLog.Rows.Add(new TableRow());
                    }
                }

                if (ddProfile.SelectedValue != string.Empty)
                {
                    profileId = long.Parse(ddProfile.SelectedValue);
                    owner = (profileId == BLL.ProfileManager.GetInstance().GetProfile().Id);
                    admin = BLL.AdminManager.GetInstance().IsTeamAdmin(long.Parse(ddTeam.SelectedValue));

                    Team team = null;

                    if (!owner)
                    {
                        team = BLL.AdminManager.GetInstance().GetTeamByRole("MEMBER");

                        if (!admin && !BLL.AdminManager.GetInstance().IsDirectorateAdminMgr(long.Parse(ddDirectorate.SelectedValue)))
                        {
                            int i = 0;

                            while (i < ddWeek.Items.Count)
                            {
                                DateTime date = DateTime.Parse(ddWeek.Items[i].Value);

                                if (DateTime.Now.AddDays(-14).CompareTo(date) >= 0)
                                {
                                    ddWeek.Items.RemoveAt(i);
                                }
                                else
                                {
                                    i++;
                                }
                            }

                            if (DateTime.Now.AddDays(-14).CompareTo(week) > 0)
                            {
                                week = LoadWeeks(DateTime.Now.AddDays(-14));
                                LoadUsers(profileId);
                            }

                            phHeader.Controls.Add(new LiteralControl(
                                "<input type=\"hidden\" name=\"minDate\" id=\"minDate\" value=\"y\">"));
                        }
                    }

                    cellAddToFavs.Visible = (owner || (team != null && team.Id.ToString() == ddTeam.SelectedValue));
                    tblButtons.Visible = true;
                }

                for (int i = 0; i < 5; i++)
                {
                    hdrPlan1.Cells[i + 9].Text = week.AddDays(i - 6).ToString("ddd M/d");
                    hdrTotals.Cells[i + 1].Text = hdrPlan1.Cells[i + 9].Text;
                }

                for (int i = 5; i < 7; i++)
                {
                    hdrPlan1.Cells[i + 10].Text = week.AddDays(i - 6).ToString("ddd M/d");
                    hdrTotals.Cells[i + 1].Text = hdrPlan1.Cells[i + 10].Text;
                }

                if (ddProfile.SelectedValue == string.Empty)
                {
                    lblPlanTable.Text = "Weekly Plan/Log";
                    lblMod.Text = string.Empty;
                    rowPlanBtns.Visible = false;
                    rowTblPlanLog.Visible = false;
                    tblButtons.Visible = false;
                    rowTotalsTbl.Visible = false;
                    btnUndo.Style.Add(HtmlTextWriterStyle.Display, "none");
                    btnSave.Style.Add(HtmlTextWriterStyle.Display, "none");
                    updateMade.Value = string.Empty;
                    return;
                }

                myPlanIDs = new ArrayList();
                myUnplanIDs = new ArrayList();
                myRemoveIDs = new ArrayList();
                List<long> taskIDs = new List<long>();

                ViewState["profileId"] = profileId;
                ViewState["week"] = week;
                plan = BLL.TaskManager.GetInstance().GetWeeklyPlan(profileId, week);

                if (plan == null)
                {
                    plan = new WeeklyPlan();
                    plan.State = WeeklyPlan.StatusEnum.NEW;
                    lblStatus.Text = "Not Started";
                    lblMod.Text = string.Empty;
                    ViewState["modified"] = string.Empty;

                    if (!loadSession && owner)
                    {
                        List<WeeklyTask> tempTasks = BLL.TaskManager.GetInstance().GetTaskTemplates(profileId);

                        foreach (WeeklyTask wt in tempTasks)
                        {
                            wt.Id = -wt.Task.Id;
                            string id = "n_" + wt.Id.ToString();
                            taskMap.Add(id, wt);
                            myPlanIDs.Add(id);
                            tempsLoaded = true;
                        }
                    }
                }
                else
                {
                    if (loadSession && CheckSessionKey("modified") &&
                        (string)GetSessionObject("modified") != plan.Modified.ToString())
                    {
                        myModError = true;
                        Session.Remove("weekly_values");
                        loadSession = false;
                    }

                    lblMod.Text = "Last modified on " + Utility.ConvertToLocal(plan.Modified).ToString("M/d/yy h:mm tt");
                    ViewState["modified"] = plan.Modified.ToString();

                    if (plan.State == WeeklyPlan.StatusEnum.NEW)
                    {
                        lblStatus.Text = "New";
                    }
                    else if (plan.State == WeeklyPlan.StatusEnum.PLAN_READY ||
                             plan.State == WeeklyPlan.StatusEnum.LOG_READY)
                    {
                        lblStatus.Text = "Ready for Approval";
                    }
                    else if (plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED)
                    {
                        lblStatus.Text = "Plan Approved by " + plan.PlanApprovedBy.DisplayName;
                    }
                    else if (plan.State == WeeklyPlan.StatusEnum.LOG_APPROVED)
                    {
                        lblStatus.Text = "Log Approved by " + plan.LogApprovedBy.DisplayName;
                    }

                    foreach (WeeklyTask wt in plan.WeeklyTasks)
                    {
                        string id = "t_" + wt.Id.ToString();
                        taskIDs.Add(wt.Task.Id);
                        taskMap.Add(id, wt);
                        ViewState["taskID_" + id] = wt.Task.Id;

                        if (wt.UnplannedCode == null)
                        {
                            myPlanIDs.Add(id);
                        }
                        else
                        {
                            myUnplanIDs.Add(id);
                        }

                        foreach (WeeklyBarrier wb in wt.Barriers)
                        {
                            id = "b_" + wb.Id.ToString();

                            if (wt.UnplannedCode == null)
                            {
                                myPlanIDs.Add(id);
                            }
                            else
                            {
                                myUnplanIDs.Add(id);
                            }

                            barrierMap.Add(id, wb);
                        }
                    }
                }

                cellCarry.Visible = ((owner || admin) &&
                    plan.State != WeeklyPlan.StatusEnum.NEW && plan.State != WeeklyPlan.StatusEnum.PLAN_READY);

                weeklyPlanID.Value = plan.Id.ToString();
                string mode = "view";

                if (owner || admin)
                {
                    if (plan.State == WeeklyPlan.StatusEnum.NEW ||
                        plan.State == WeeklyPlan.StatusEnum.PLAN_READY)
                    {
                        mode = "plan";
                    }
                    else if (plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED ||
                        plan.State == WeeklyPlan.StatusEnum.LOG_READY)
                    {
                        mode = "log";
                    }
                }

                if (mode == "view" || !loadSession)
                {
                    bool carryLoaded = false;
                    loadSession = false;
                    updateMade.Value = string.Empty;

                    if (mode != "view" && myCarryIDs != null)
                    {
                        foreach (long taskID in myCarryIDs)
                        {
                            if (!taskIDs.Contains(taskID))
                            {
                                WeeklyTask wt = CreateWeeklyTask(taskID);

                                if (wt.Task != null)
                                {
                                    string sid = "n_" + taskID.ToString();
                                    taskMap.Add(sid, wt);
                                    carryLoaded = true;

                                    if (mode == "plan")
                                    {
                                        myPlanIDs.Add(sid);
                                    }
                                    else if (mode == "log")
                                    {
                                        myUnplanIDs.Add(sid);
                                    }
                                }
                            }
                        }
                    }

                    if (!tempsLoaded && !carryLoaded)
                    {
                        btnSave.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                    else
                    {
                        btnSave.Style.Clear();
                    }

                    if (!carryLoaded)
                    {
                        btnUndo.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                    else
                    {
                        btnUndo.Style.Clear();
                    }
                }
                else
                {
                    updateMade.Value = "y";

                    if (CheckSessionKey("removeIDs"))
                    {
                        myRemoveIDs = (ArrayList)GetSessionObject("removeIDs");
                    }

                    if (CheckSessionKey("newIDs"))
                    {
                        Dictionary<int, string> newIDs = (Dictionary<int, string>)GetSessionObject("newIDs");

                        if (newIDs != null && newIDs.Count > 0)
                        {
                            int planCount = (int)GetSessionObject("planCount");
                            bool isPlanned = (mode == "plan");
                            List<int> inserts = newIDs.Keys.ToList();
                            inserts.Sort();

                            foreach (int idx in inserts)
                            {
                                string sid = newIDs[idx];

                                if (IsTask(sid))
                                {
                                    taskMap.Add(sid, null);
                                }
                                else
                                {
                                    barrierMap.Add(sid, null);
                                }

                                if (idx < planCount)
                                {
                                    if (idx < myPlanIDs.Count)
                                    {
                                        myPlanIDs.Insert(idx, sid);
                                    }
                                    else
                                    {
                                        myPlanIDs.Add(sid);
                                    }
                                }
                                else if (idx - planCount < myUnplanIDs.Count)
                                {
                                    myUnplanIDs.Insert(idx - planCount, sid);
                                }
                                else
                                {
                                    myUnplanIDs.Add(sid);
                                }
                            }
                        }
                    }

                    if (CheckSessionKey("sesssionChanges"))
                    {
                        sessionChanges.Value = "y";
                    }
                }

                if (!myRemoveIDs.Contains("leave") && plan.LeavePlanned.HasValue)
                {
                    ViewState["leavePlanned"] = plan.LeavePlanned.Value;
                }

                SetMode(mode);
                cellReady.Visible = false;
                cellApprove.Visible = false;

                if (owner && plan.State != WeeklyPlan.StatusEnum.LOG_APPROVED)
                {
                    cellReady.Visible = true;

                    if (plan.State == WeeklyPlan.StatusEnum.NEW || plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED)
                    {
                        btnReady.Text = "Ready for Approval";
                        btnReady.Width = 135;
                        btnReady.OnClientClick = "if(!CheckHours())return false";
                    }
                    else
                    {
                        btnReady.Text = "Undo Ready for Approval";
                        btnReady.Width = 165;
                        btnReady.OnClientClick = string.Empty;
                    }
                }

                if (admin)
                {
                    if (plan.State == WeeklyPlan.StatusEnum.PLAN_READY || plan.State == WeeklyPlan.StatusEnum.LOG_READY)
                    {
                        cellApprove.Visible = true;
                        btnApprove.Text = "Approve " + (plan.State == WeeklyPlan.StatusEnum.PLAN_READY ? "Plan" : "Log");
                        btnApprove.Width = 125;
                        btnApprove.OnClientClick = string.Empty;
                    }
                    else if (plan.State == WeeklyPlan.StatusEnum.LOG_APPROVED)
                    {
                        if (DateTime.Now.CompareTo(plan.WeekEnding.AddDays(14)) < 0)
                        {
                            cellApprove.Visible = true;
                            btnApprove.Text = "Undo Approval";
                            btnApprove.Width = 125;
                            btnApprove.OnClientClick = string.Empty;
                        }
                    }
                    else if (!owner)
                    {
                        cellApprove.Visible = true;
                        btnApprove.Text = "Approve " + (plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED ? "Log" : "Plan") + " (Not Ready)";
                        btnApprove.Width = 175;
                        btnApprove.OnClientClick = "if(!confirm('This " +
                            (plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED ? "log" : "plan") +
                            " is not ready for approval, approve anyway?'))return false";
                    }
                }

                ViewState["weeklyPlanIDs"] = myPlanIDs;
                ViewState["weeklyUnplanIDs"] = myUnplanIDs;
                ViewState["weeklyRemoveIDs"] = myRemoveIDs;
            }
            else if (ViewState["modified"] != null)
            {
                long profileId = (long)ViewState["profileId"];
                DateTime week = (DateTime)ViewState["week"];

                if (profileId.ToString() == ddProfile.SelectedValue &&
                    week.ToShortDateString() == ddWeek.SelectedValue)
                {
                    DateTime? modified = BLL.TaskManager.GetInstance().GetWeeklyPlanModified(profileId, week);

                    if (modified.HasValue && 
                        (string)ViewState["modified"] != modified.Value.ToString())
                    {
                        StoreSessionValue("modError", true, false);
                        Response.Redirect("/weekly.aspx");
                    }
                }

                myPlanIDs = (ArrayList)ViewState["weeklyPlanIDs"];
                myUnplanIDs = (ArrayList)ViewState["weeklyUnplanIDs"];
                myRemoveIDs = (ArrayList)ViewState["weeklyRemoveIDs"];
            }
            else
            {
                return;
            }

            while (tblPlanLog.Rows.Count > 2)
            {
                tblPlanLog.Rows.RemoveAt(2);
            }

            hdrToggleWE.RowSpan = hdrPlan2.Visible ? 3 : 2;
            myControls.Clear();

            ArrayList list = myPlanIDs;
            bool planned = true;
            bool weShown = myShowWE;
            bool taskRemoved = false;
            bool? showLeave = null;

            if (!myRemoveIDs.Contains("leave"))
            {
                if (plan != null)
                {
                    if (loadSession && CheckSessionKey("leavePlanned"))
                    {
                        plan.LeavePlanned = (bool)GetSessionObject("leavePlanned");
                    }

                    showLeave = plan.LeavePlanned;
                }
                else if (ViewState["leavePlanned"] != null)
                {
                    showLeave = (bool)ViewState["leavePlanned"];
                }
            }

            for (int j = 0; j < 2; j++)
            {
                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid))
                    {
                        bool setUpdated = !tempsLoaded && IsNew(sid);

                        if (IsTask(sid))
                        {
                            WeeklyTask wt = null;
                            taskRemoved = false;

                            if (plan != null)
                            {
                                wt = taskMap[sid];

                                if (loadSession)
                                {
                                    SetWeeklyTask(ref wt, sid, true);
                                }

                                if (wt == null || wt.Task == null ||
                                    ((plan.State == WeeklyPlan.StatusEnum.NEW || plan.State == WeeklyPlan.StatusEnum.PLAN_READY) &&
                                    wt.Task.Status != DTO.Task.StatusEnum.OPEN && wt.Task.Status != DTO.Task.StatusEnum.HOLD) ||
                                    !wt.Task.BaseTask)
                                {
                                    taskRemoved = true;
                                    myRemoveIDs.Add(sid);
                                    btnSave.Style.Clear();
                                    updateMade.Value = "y";
                                }

                                if (loadSession && CheckSessionKey(sid + "_hours"))
                                {
                                    bool comp = (bool)GetSessionObject(sid + "_comp");
                                    setUpdated = true;

                                    if (myMode.Value == "plan")
                                    {
                                        wt.PlanDayComplete = comp ? 0 : -1;
                                    }
                                    else if (myMode.Value == "log")
                                    {
                                        wt.ActualDayComplete = -1;
                                    }

                                    List<double> hours = (List<double>)GetSessionObject(sid + "_hours");

                                    for (int i = 0; i < 7; i++)
                                    {
                                        if (myMode.Value == "plan")
                                        {
                                            wt.PlanHours[i] = hours[i];
                                        }
                                        else if (myMode.Value == "log")
                                        {
                                            wt.Task.Spent += hours[i] - wt.ActualHours[i];
                                            wt.ActualHours[i] = hours[i];

                                            if (hours[i] > 0 && comp)
                                            {
                                                wt.ActualDayComplete = i;
                                            }
                                        }
                                    }
                                }

                            }

                            if (!taskRemoved)
                            {
                                AddTaskRow(sid, wt, planned, false, setUpdated);
                            }
                        }
                        else if (!taskRemoved)
                        {
                            WeeklyBarrier wb = null;

                            if (plan != null)
                            {
                                wb = barrierMap[sid];

                                if (loadSession)
                                {
                                    SetWeeklyBarrier(ref wb, sid, true);

                                    if (CheckSessionKey(sid + "_hours"))
                                    {
                                        List<double> hours = (List<double>)GetSessionObject(sid + "_hours");
                                        setUpdated = true;

                                        for (int i = 0; i < 7; i++)
                                        {
                                            wb.Hours[i] = hours[i];
                                        }
                                    }
                                }
                            }

                            AddBarrierRow(sid, string.Empty, wb, planned, setUpdated);
                        }
                        else
                        {
                            myRemoveIDs.Add(sid);
                        }
                    }
                }

                if (showLeave.HasValue && showLeave.Value == planned)
                {
                    bool setUpdated = false;

                    if (loadSession && CheckSessionKey("leave_hours"))
                    {
                        List<double> hours = (List<double>)GetSessionObject("leave_hours");
                        setUpdated = true;

                        for (int i = 0; i < 7; i++)
                        {
                            if (myMode.Value == "plan")
                            {
                                plan.LeavePlanHours[i] = hours[i];
                            }
                            else if (myMode.Value == "log")
                            {
                                plan.LeaveActualHours[i] = hours[i];
                            }
                        }
                    }

                    AddLeaveRow(plan, showLeave.Value, false, setUpdated);
                }

                if (j == 0 && myMode.Value != "plan" &&
                    (hdrCode.Visible || (showLeave.HasValue && !showLeave.Value)))
                {
                    list = myUnplanIDs;
                    planned = false;
                }
                else
                {
                    j = 2;
                }
            }

            SetTotalRows();

            if (!weShown && myShowWE)
            {
                ShowWE();
                hdrToggleWE.Visible = false;

                if (myControls.ContainsKey("hdrToggleWE2"))
                {
                    ((TableHeaderCell)myControls["hdrToggleWE2"]).Visible = false;
                }
            }
            else if (myShowWE)
            {
                hdrSat.Style.Clear();
                hdrSun.Style.Clear();

                for (int i = 10; i < 14; i++)
                {
                    hdrPlan2.Cells[i].Style.Clear();
                }
            }

            if (!IsPostBack && HttpContext.Current.Request["taskId"] != null &&
                AddTaskToPlan(HttpContext.Current.Request["taskId"], false))
            {
                btnUndo.Style.Clear();
                btnSave.Style.Clear();
                updateMade.Value = "y";
            }

            HttpContext.Current.Session["weekly_rows"] = tblPlanLog.Rows.Count;
        }

        private WeeklyTask CreateWeeklyTask(long taskID)
        {
            WeeklyTask wt = new WeeklyTask();
            wt.Task = BLL.TaskManager.GetInstance().GetTask(taskID, false);
            wt.PlanDayComplete = -1;
            wt.ActualDayComplete = -1;

            for (int i = 0; i < 7; i++)
            {
                wt.PlanHours.Add(i, 0);
                wt.ActualHours.Add(i, 0);
            }

            return wt;
        }

        private int GetPlanCount(bool includeLeave)
        {
            int count = 0;

            foreach (string sid in myPlanIDs)
            {
                if (!myRemoveIDs.Contains(sid))
                {
                    count++;
                }
            }

            if (includeLeave && ViewState["leavePlanned"] != null && (bool)ViewState["leavePlanned"])
            {
                count++;
            }

            return count;
        }

        private int GetUnplanCount(bool includeLeave)
        {
            int count = 0;

            foreach (string sid in myUnplanIDs)
            {
                if (!myRemoveIDs.Contains(sid))
                {
                    count++;
                }
            }

            if (includeLeave && ViewState["leavePlanned"] != null && !(bool)ViewState["leavePlanned"])
            {
                count++;
            }

            return count;
        }

        private void SetMode(string mode)
        {
            int colspan = 2;
            int rowspan = 2;

            myMode.Value = mode;
            rowTblPlanLog.Visible = true;
            rowPlanBtns.Visible = true;

            if (mode == "plan")
            {
                lblPlanTable.Text = "Weekly Plan";
                hdrPlan2.Visible = false;
                hdrCode.Visible = false;
                cellAddMenu.Visible = true;
                cellAddPrev.Visible = true;
                cellAddBarrier.Visible = false;
                cellLeave.Visible = true;
                cellEdit.Visible = true;                
                cellRemove.Visible = true;
                cellActionMenu.Style.Remove("border-left");
                cellToggleBarriers.Visible = false;
                hdrCellSpent.CssClass = "field";
                hdrCellComp.CssClass = "field2";
                hdrCellComp.Text = "Plan to<br>Complete";
                hdrCellComp.Visible = true;
                btnUndo.Visible = true;
                btnSave.Visible = true;
                rowTotalsTbl.Visible = false;
                rowspan = 1;
                colspan = 1;
            }
            else if (mode == "log")
            {
                lblPlanTable.Text = "Weekly Log";
                hdrPlan2.Visible = true;
                hdrCode.Visible = GetUnplanCount(false) > 0;
                cellAddMenu.Visible = true;
                cellAddPrev.Visible = false;
                cellLeave.Visible = true;
                cellAddBarrier.Visible = true;
                cellEdit.Visible = true;
                cellRemove.Visible = true;
                cellActionMenu.Style.Remove("border-left");
                cellToggleBarriers.Visible = true;
                hdrCellSpent.CssClass = "field";
                hdrCellComp.CssClass = "field2";
                hdrCellComp.Text = "Completed";
                hdrCellComp.Visible = true;
                btnUndo.Visible = true;
                btnSave.Visible = true;
                rowTotalsTbl.Visible = true;
            }
            else if (mode == "view")
            {
                lblPlanTable.Text = "Weekly Log";
                hdrPlan2.Visible = true;
                hdrCode.Visible = GetUnplanCount(false) > 0;
                cellAddMenu.Visible = false;
                cellEdit.Visible = false;
                cellRemove.Visible = false;
                cellActionMenu.Style.Add("border-left", "1px solid Black");
                cellToggleBarriers.Visible = true;                
                hdrCellSpent.CssClass = "field2";
                hdrCellComp.Visible = false;
                btnUndo.Visible = false;
                btnSave.Visible = false;
                rowTotalsTbl.Visible = true;
            }

            for (int i = 0; i < 9; i++)
            {
                hdrPlan1.Cells[i].RowSpan = rowspan;
            }

            for (int i = 9; i < 14; i++)
            {
                hdrPlan1.Cells[i].ColumnSpan = colspan;
            }

            for (int i = 15; i < 18; i++)
            {
                hdrPlan1.Cells[i].ColumnSpan = colspan;
            }
        }

        private void AddTaskRow(string taskID, WeeklyTask wt, bool planned, bool insert, bool setUpdated)
        {
            int i = 1;
            int compDay = 7;
            double hours = 0;
            string prefix = GetControlPrefix();
            string status = string.Empty;
            string spent;
            TableCell cell = null;
            TableCell statusCell = null;
            HiddenField hdnUpdate = new HiddenField();
            hdnUpdate.ID = "hdnUpdate_" + taskID;

            if (setUpdated)
            {
                hdnUpdate.Value = "y";
            }

            if (myRemoveIDs.Contains(taskID))
            {
                myRemoveIDs.Remove(taskID);
            }

            CheckBox chk = new CheckBox();
            chk.ID = "chk_" + taskID;
            chk.CssClass = "rowCheck";
            chk.Attributes.Add("onclick", "RowChecked(this, '" + (planned ? "p" : "u") + "')");

            TableRow tblRow = new TableRow();
            tblRow.ID = "row_" + taskID;
            tblRow.Cells.Add(CreateCell(hdnUpdate, chk));
            myControls.Add(tblRow.ID, tblRow);

            LinkButton lnkTitle = new LinkButton();
            lnkTitle.ID = "lnkTitle_" + taskID;
            lnkTitle.Click += lnkTitle_Click;

            if (wt != null)
            {
                lnkTitle.Text = wt.Task.Title;
            }
            
            tblRow.Cells.Add(CreateCell(lnkTitle, taskID, hdrCode.Visible && planned ? 2 : 1));

            if (!planned)
            {
                if (wt != null)
                {
                    tblRow.Cells.Add(CreateCell(wt.UnplannedCode.Code + " - " + wt.UnplannedCode.Title, taskID, 0, 1));
                }
                else
                {
                    tblRow.Cells.Add(CreateCell(taskID, 0, 1));
                }
            }

            if (wt != null)
            {
                if (myMode.Value == "log")
                {
                    if (wt.ActualDayComplete != -1 && wt.Task.Status == DTO.Task.StatusEnum.OPEN)
                    {
                        wt.Task.Status = DTO.Task.StatusEnum.COMPLETED;
                        wt.Task.CompletedDate = null;
                    }
                    else if (wt.ActualDayComplete == -1 && wt.Task.Status == DTO.Task.StatusEnum.COMPLETED)
                    {
                        wt.Task.Status = DTO.Task.StatusEnum.OPEN;
                    }
                }

                status = FormatEnum(wt.Task.Status);
                tblRow.Cells.Add(CreateCell(wt.Task.TaskType != null ? wt.Task.TaskType.Title : string.Empty, taskID, i++, 1));
                tblRow.Cells.Add(CreateCell(wt.Task.Program != null ? wt.Task.Program.Title : string.Empty, taskID, i++, 1));
                tblRow.Cells.Add(CreateCell(status, taskID, i++, 1));

                if (hdrComp.Visible)
                {
                    tblRow.Cells.Add(CreateCell(wt.Task.Complexity != null ? wt.Task.Complexity.Title : string.Empty, taskID, i++, 1, "number"));
                }

                tblRow.Cells.Add(CreateCell(wt.Task.Estimate.ToString(), taskID, i++, 1, "number"));
                spent = wt.Task.Spent.ToString();
            }
            else
            {
                int cols = hdrComp.Visible ? 6 : 5;

                for (i = 1; i < cols; i++)
                {
                    if (i < 4)
                    {
                       tblRow.Cells.Add(CreateCell(taskID, i, 1));
                    }
                    else
                    {
                        tblRow.Cells.Add(CreateCell(taskID, i, 1, "number"));
                    }
                }

                statusCell = tblRow.Cells[planned ? 4 : 5];
                status = statusCell.Text;
                spent = HttpContext.Current.Request["spent_" + taskID];
            }

            cell = CreateCell("<div id=\"spentDiv_" + taskID + "\">" + spent +
                "</div><input type=\"hidden\" id=\"spent_" + taskID + "\" name=\"spent_" + taskID + "\" value=\"" +
                spent + "\"/>", "number");

            tblRow.Cells.Add(cell);

            string tbl = planned ? "P" : "U";

            if (myMode.Value != "view")
            {
                chk = new CheckBox();
                chk.ID = "chkComp_" + taskID;

                if (myMode.Value == "plan")
                {
                    chk.Attributes.Add("onclick", "SetRowUpdated('" + taskID + "')");
                }
                else
                {
                    chk.Attributes.Add("onclick", "CompleteClick(this, '" + taskID + "', '" + tbl + "')");
                }

                if (status == "OBE" || status == "Hold")
                {
                    chk.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                else if (wt != null)
                {
                    if (myMode.Value == "plan")
                    {
                        chk.Checked = wt.PlanDayComplete != -1;
                    }
                    else
                    {
                        chk.Checked = wt.ActualDayComplete != -1;
                    }
                }

                tblRow.Cells.Add(CreateCell(chk, "lastFieldChk"));
            }
            else
            {
                cell.CssClass = "lastFieldNum";
            }

            if (myMode.Value == "log")
            {
                if (wt != null && ((wt.Task.Status == DTO.Task.StatusEnum.HOLD && wt.Task.OnHoldDate.HasValue) ||
                ((wt.Task.Status == DTO.Task.StatusEnum.COMPLETED || wt.Task.Status == DTO.Task.StatusEnum.OBE) && wt.Task.CompletedDate.HasValue)))
                {
                    if (wt.Task.Status == DTO.Task.StatusEnum.HOLD)
                    {
                        compDay = 6 - ((TimeSpan)(DateTime.Parse(ddWeek.SelectedValue) - wt.Task.OnHoldDate.Value.Date)).Days;
                    }
                    else
                    {
                        compDay = 6 - ((TimeSpan)(DateTime.Parse(ddWeek.SelectedValue) - wt.Task.CompletedDate.Value.Date)).Days;

                        if (wt.Task.Status == DTO.Task.StatusEnum.COMPLETED && wt.ActualDayComplete != compDay)
                        {
                            compDay = 7;
                        }
                    }

                    if (compDay < 7)
                    {
                        ViewState["comp_" + taskID] = compDay;
                    }
                }
                else if (wt == null)
                {
                    string statusCheck = status;
                    string compCheck = HttpContext.Current.Request[prefix + chk.ID];

                    if (compCheck == "on" && status == FormatEnum(DTO.Task.StatusEnum.OPEN))
                    {
                        statusCheck = FormatEnum(DTO.Task.StatusEnum.COMPLETED);
                    }
                    else if (compCheck != "on" && status == FormatEnum(DTO.Task.StatusEnum.COMPLETED))
                    {
                        statusCheck = FormatEnum(DTO.Task.StatusEnum.OPEN);
                    }

                    if (status != statusCheck)
                    {
                        status = statusCheck;
                        statusCell.Text = status;
                        ViewState[statusCell.ID] = status;
                        compDay = 7;
                        ViewState.Remove("comp_" + taskID);
                    }
                    else if (ViewState["comp_" + taskID] != null)
                    {
                        compDay = (int)ViewState["comp_" + taskID];
                    }
                }
            }

            tblRow.CssClass = status.ToLower();

            double planTotal = 0;
            double actualTotal = 0;
            bool addedDiv = false;
            DateTime week = DateTime.Parse(ddWeek.SelectedValue).Date.AddDays(-6);
            DateTime now = DateTime.Now.Date;

            for (int col = 0; col < 7; col++)
            {
                if (planned && myMode.Value != "plan")
                {
                    if (wt != null)
                    {
                        hours = wt.PlanHours[col];
                        cell = CreateCell(FormatHours(hours), taskID + "p", col, 1,
                            wt.PlanDayComplete != col ? "planHour" : "compPlanHour", true);
                    }
                    else
                    {
                        cell = CreateCell(taskID + "p", col, 1, true);
                        hours = 0;
                        double.TryParse(cell.Text, out hours);
                    }

                    CheckWE(cell, col, hours);
                    tblRow.Cells.Add(cell);

                    planTotal += hours;
                    myPlanTotals[col] += hours;
                    myPlanTotals[7] += hours;
                }

                if (myMode.Value == "view")
                {
                    if (wt != null)
                    {
                        hours = wt.ActualHours[col];
                        cell = CreateCell(FormatHours(hours), taskID + "a", col,
                            planned ? 1 : 2, wt.ActualDayComplete != col ? "hour" : "compHour", true);
                    }
                    else
                    {
                        cell = CreateCell(taskID + "a", col, planned ? 1 : 2, true);
                        hours = 0;
                        double.TryParse(cell.Text, out hours);
                    }

                    CheckWE(cell, col, hours);
                    tblRow.Cells.Add(cell);
                    actualTotal += hours;

                    if (planned)
                    {
                        myActualTotals[col] += hours;
                        myActualTotals[7] += hours;
                    }
                    else
                    {
                        myUnplanTotals[col] += hours;
                        myUnplanTotals[7] += hours;
                    }
                }
                else if (myMode.Value == "plan" && ViewState["cell_" + taskID + "_3"].ToString() == "Hold")
                {
                    cell = CreateCell(string.Empty, taskID + "a", col, planned ? 1 : 2, "hour");
                    CheckWE(cell, col, hours);
                    tblRow.Cells.Add(cell);
                }
                else
                {
                    string updFunc = "UpdateTotal(this," + col.ToString() + ",'" + tbl + "','" + taskID + "'";

                    TextBox txt = new TextBox();
                    txt.ID = "txtHours" + tbl + "_" + taskID + "_" + col.ToString();
                    txt.Width = 26;
                    txt.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                    txt.Attributes.Add("onchange", updFunc + ",1)");
                    txt.Attributes.Add("onkeydown", "return CancelEnter(event)");
                    txt.Attributes.Add("onkeyup", updFunc + ")");

                    if ((myMode.Value == "log" && week.AddDays(col).CompareTo(now) > 0) || col > compDay)
                    {
                        txt.Text = string.Empty;
                        txt.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }

                    if (wt != null)
                    {
                        if (myMode.Value == "plan")
                        {
                            hours = wt.PlanHours[col];
                        }
                        else
                        {
                            hours = wt.ActualHours[col];
                        }

                        txt.Text = FormatHours(hours);
                    }
                    else
                    {
                        hours = 0;
                        double.TryParse(HttpContext.Current.Request[prefix + txt.ID], out hours);
                    }

                    cell = CreateCell(txt, planned ? 1 : 2,
                        (status == "Completed" && col == compDay) ? "inputHourComp" : "inputHour");
                    
                    CheckWE(cell, col, hours);
                    tblRow.Cells.Add(cell);

                    if (myMode.Value == "plan")
                    {
                        planTotal += hours;
                        myPlanTotals[col] += hours;
                        myPlanTotals[7] += hours;
                    }
                    else
                    {
                        actualTotal += hours;

                        if (planned)
                        {
                            myActualTotals[col] += hours;
                            myActualTotals[7] += hours;
                        }
                        else
                        {
                            myUnplanTotals[col] += hours;
                            myUnplanTotals[7] += hours;
                        }
                    }
                }

                if (!planned && col == 4)
                {
                    addedDiv = AddUnplanDivider(tblRow, insert);
                }
            }

            if (planned && myMode.Value == "plan")
            {
                tblRow.Cells.Add(CreateTotalCell("<div id=\"totalRow_" + taskID + "\">" + 
                    Utility.RoundDoubleToString(planTotal) + "</div>", "totalRow"));
            }
            else
            {
                if (planned)
                {
                    tblRow.Cells.Add(CreateTotalCell(Utility.RoundDoubleToString(planTotal), "totalRowPlan"));
                }

                tblRow.Cells.Add(CreateTotalCell("<div id=\"totalRow_" + taskID + "\">" +
                    Utility.RoundDoubleToString(actualTotal) + "</div>", "totalRow", planned ? 1 : 2));
            }

            if (planned)
            {
                if (insert)
                {
                    tblPlanLog.Rows.AddAt(GetPlanCount(false) + 1, tblRow);
                }
                else
                {
                    tblPlanLog.Rows.Add(tblRow);
                }

                hdrToggleWE.RowSpan++;
            }
            else
            {
                TableHeaderCell hdrToggleWE2 = (TableHeaderCell)myControls["hdrToggleWE2"];

                if (insert)
                {
                    int idx = tblPlanLog.Rows.Count - 1;
                    
                    if (ViewState["leavePlanned"] != null && !(bool)ViewState["leavePlanned"])
                    {
                        idx--;

                        if (GetUnplanCount(false) == 1)
                        {
                            ((TableRow)myControls["row_leave"]).Cells.Remove(hdrToggleWE2);
                            tblRow.Cells.AddAt(tblRow.Cells.Count - 3, hdrToggleWE2);
                        }
                    }

                    tblPlanLog.Rows.AddAt(idx, tblRow);
                }
                else
                {
                    tblPlanLog.Rows.Add(tblRow);
                }

                if (addedDiv)
                {
                    hdrToggleWE2.RowSpan = 2;
                }
                else
                {
                    hdrToggleWE2.RowSpan++;
                }
            }
        }

        private bool AddUnplanDivider(TableRow tblRow, bool insert)
        {
            if (!myControls.ContainsKey("hdrToggleWE2"))
            {
                int colspan = hdrComp.Visible ? 22 : 21;
                if (myMode.Value == "view") colspan--;
                if (hdrCode.Visible) colspan++;
                if (myShowWE) colspan += 4;

                TableHeaderCell cell = CreateTotalCell("Unplanned Tasks", "divider", colspan);
                cell.ID = "cellUnplanDiv";
                myControls.Add(cell.ID, cell);

                TableHeaderRow unplanDiv = new TableHeaderRow();
                unplanDiv.ID = "rowUnplanDiv";
                unplanDiv.Cells.Add(cell);
                myControls.Add(unplanDiv.ID, unplanDiv);

                if (insert)
                {
                    tblPlanLog.Rows.AddAt(GetPlanCount(true) + 2, unplanDiv);
                }
                else
                {
                    tblPlanLog.Rows.Add(unplanDiv);
                }

                hdrToggleWE.RowSpan--;

                LinkButton lnkToggleWE2 = new LinkButton();
                lnkToggleWE2.ID = "lnkToggleWE2";
                lnkToggleWE2.Text = lnkToggleWE.Text;
                lnkToggleWE2.OnClientClick = lnkToggleWE.OnClientClick;
                lnkToggleWE2.Font.Underline = false;
                myControls.Add(lnkToggleWE2.ID, lnkToggleWE2);

                TableHeaderCell hdrToggleWE2 = new TableHeaderCell();
                hdrToggleWE2.ID = "hdrToggleWE2";
                hdrToggleWE2.VerticalAlign = VerticalAlign.Top;
                hdrToggleWE2.CssClass = "planDay";
                hdrToggleWE2.Visible = hdrToggleWE.Visible;
                hdrToggleWE2.Controls.Add(lnkToggleWE2);
                myControls.Add(hdrToggleWE2.ID, hdrToggleWE2);

                tblRow.Cells.Add(hdrToggleWE2);
                return true;
            }

            return false;
        }

        private void SetTotalRows()
        {
            TableHeaderRow rowPlanTotals;

            if (myControls.ContainsKey("rowPlanTotals"))
            {
                rowPlanTotals = (TableHeaderRow)myControls["rowPlanTotals"];
            }
            else
            {
                int colspan = myMode.Value == "view" ? 7 : 8;
                if (hdrCode.Visible) colspan++;
                if (hdrComp.Visible) colspan++;

                rowPlanTotals = new TableHeaderRow();
                rowPlanTotals.ID = "rowPlanTotals";
                rowPlanTotals.Cells.Add(CreateTotalCell("Total:", "totalText", colspan));

                for (int i = 0; i < 8; i++)
                {
                    string we = (i == 5 || i == 6) ? "WE" : string.Empty;

                    if (myMode.Value != "plan")
                    {
                        rowPlanTotals.Cells.Add(CreateTotalCell("totalFooterPlan" + we));
                    }
                    
                    rowPlanTotals.Cells.Add(CreateTotalCell("totalFooter" + we));
                }

                myControls.Add(rowPlanTotals.ID, rowPlanTotals);
                tblPlanLog.Rows.Add(rowPlanTotals);
            }

            for (int i = 0; i < 8; i++)
            {
                string planTotals = Utility.RoundDoubleToString(myPlanTotals[i]);

                if (myMode.Value == "plan")
                {
                    rowPlanTotals.Cells[i + 1].Text =
                        "<div id=\"totalP_" + i.ToString() + "\">" + planTotals + "</div>";
                }
                else
                {
                    string totalActuals = Utility.RoundDoubleToString(myActualTotals[i] + myUnplanTotals[i]);

                    rowPlanTotals.Cells[(i * 2) + 1].Text = planTotals;
                    rowPlanTotals.Cells[(i * 2) + 2].Text =
                        "<div id=\"totalP_" + i.ToString() + "\">" + totalActuals + "</div>";

                    rowTotalsPlan.Cells[i + 1].Text = planTotals;
                    rowTotalsPlanAct.Cells[i + 1].Text =
                        "<div id=\"totalP2_" + i.ToString() + "\">" + Utility.RoundDoubleToString(myActualTotals[i]) + "</div>";

                    rowTotalsUnplan.Cells[i + 1].Text =
                        "<div id=\"totalU2_" + i.ToString() + "\">" + Utility.RoundDoubleToString(myUnplanTotals[i]) + "</div>";

                    rowTotalsActual.Cells[i + 1].Text =
                        "<div id=\"totalA_" + i.ToString() + "\">" + totalActuals + "</div>";

                    rowTotalsBarrier.Cells[i + 1].Text =
                        "<div id=\"totalE_" + i.ToString() + "\">" + Utility.RoundDoubleToString(myBarrierTotals[i]) + "</div>";

                    rowTotalsDelay.Cells[i + 1].Text =
                        "<div id=\"totalD_" + i.ToString() + "\">" + Utility.RoundDoubleToString(myDelayTotals[i]) + "</div>";
                }
            }
        }

        private void AddBarrierRow(string barrierID, string taskID, WeeklyBarrier wb, bool planned, bool setUpdated)
        {
            double total = 0;
            string prefix = GetControlPrefix();
            string btype;
            DateTime week = DateTime.Parse(ddWeek.SelectedValue).Date.AddDays(-6);
            DateTime now = DateTime.Now.Date;

            HiddenField hdnUpdate = new HiddenField();
            hdnUpdate.ID = "hdnUpdate_" + barrierID;

            if (setUpdated)
            {
                hdnUpdate.Value = "y";
            }

            if (myRemoveIDs.Contains(barrierID))
            {
                myRemoveIDs.Remove(barrierID);
            }

            TableRow tblRow = new TableRow();
            tblRow.ID = "row_" + barrierID;
            tblRow.CssClass = planned ? "pBarrierRow" : "uBarrierRow";
            myControls.Add(tblRow.ID, tblRow);

            if (cellToggleBarriers.Text == "Show Barriers")
            {
                tblRow.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            if (cellToggleBarriers.Text != "Show Barriers" ||
                HttpContext.Current.Request.UserAgent.Contains("MSIE 7"))
            {
                if (planned)
                {
                    hdrToggleWE.RowSpan++;
                }
                else
                {
                    ((TableHeaderCell)myControls["hdrToggleWE2"]).RowSpan++;
                }
            }

            TableCell cell;
            CheckBox chk = new CheckBox();
            chk.ID = "chk_" + barrierID;
            chk.CssClass = "rowCheck";
            chk.Attributes.Add("onclick", "RowChecked(this, 'b')");
            tblRow.Cells.Add(CreateCell(hdnUpdate, chk));

            int colspan = hdrComp.Visible ? 6 : 5;
            if (myMode.Value == "view") colspan--;

            if (wb != null)
            {
                string enumStr = FormatEnum(wb.BarrierType);
                btype = enumStr.Substring(0, 1);

                tblRow.Cells.Add(CreateCell("<table class=\"invisible\"><tr><td width=\"20\" style=\"white-space: nowrap\"> &nbsp; &nbsp; </td><td>" +
                    wb.Barrier.Code + " - " + wb.Barrier.Title + "</td></tr></table>", barrierID, 0, hdrCode.Visible ? 2 : 1));
                
                tblRow.Cells.Add(CreateCell(enumStr, barrierID, 1, 1));
                cell = CreateCell(wb.Comment, barrierID, 2, colspan);
            }
            else
            {
                tblRow.Cells.Add(CreateCell(barrierID, 0, hdrCode.Visible ? 2 : 1));

                cell = CreateCell(barrierID, 1, 1);
                tblRow.Cells.Add(cell);
                btype = cell.Text.Substring(0, 1);

                cell = CreateCell(barrierID, 2, colspan);
            }

            if (btype == "P")
            {
                btype = "E";
            }

            cell.CssClass = "lastField";
            tblRow.Cells.Add(cell);

            for (int col = 0; col < 7; col++)
            {
                double hours = 0;

                if (myMode.Value == "view")
                {
                    if (wb != null)
                    {
                        hours = wb.Hours[col];
                        cell = CreateCell(FormatHours(hours), barrierID + "v", col, 2, "barrierHour");
                    }
                    else
                    {
                        cell = CreateCell(barrierID + "v", col, 2, "barrierHour");
                        double.TryParse(cell.Text, out hours);
                    }
                    
                    tblRow.Cells.Add(cell);
                }
                else
                {
                    TextBox txt = new TextBox();
                    txt.ID = "txtBarHours" + btype + "_" + barrierID + "_" + col.ToString();
                    txt.Style.Add(HtmlTextWriterStyle.TextAlign, "right");

                    if (week.AddDays(col).CompareTo(now) > 0)
                    {
                        txt.Text = "0";
                        txt.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                    else
                    {
                        string updFunc = "UpdateTotal(this," + col.ToString() + ",'" + btype + "','" + barrierID + "'";
                        txt.Width = 26;
                        txt.Attributes.Add("onchange", updFunc + ",1)");
                        txt.Attributes.Add("onkeydown", "return CancelEnter(event)");
                        txt.Attributes.Add("onkeyup", updFunc + ")");
                    }

                    if (wb != null)
                    {
                        hours = wb.Hours[col];
                        txt.Text = FormatHours(hours);
                    }
                    else
                    {
                        double.TryParse(HttpContext.Current.Request[prefix + txt.ID], out hours);
                    }

                    cell = CreateCell(txt, 2, "inputBarrierHour");
                    tblRow.Cells.Add(cell);
                }

                CheckWE(cell, col, hours);
                total += hours;

                if (btype == "E")
                {
                    myBarrierTotals[col] += hours;
                    myBarrierTotals[7] += hours;
                }
                else
                {
                    myDelayTotals[col] += hours;
                    myDelayTotals[7] += hours;
                }
            }

            tblRow.Cells.Add(CreateTotalCell("<div id=\"totalRow_" + barrierID + "\">" +
                Utility.RoundDoubleToString(total) + "</div>", "barrierTotal", 2));
            
            if (taskID == string.Empty)
            {
                tblPlanLog.Rows.Add(tblRow);
            }
            else
            {
                int i = 2;
                int idx = 0;

                while (idx == 0 && i < tblPlanLog.Rows.Count - 1)
                {
                    TableRow row = tblPlanLog.Rows[i];
                    i++;

                    if (row.ID == "row_" + taskID)
                    {
                        idx = i;
                    }
                }

                if (planned)
                {
                    myPlanIDs.Insert(myPlanIDs.IndexOf(taskID) + 1, barrierID);
                }
                else
                {
                    myUnplanIDs.Insert(myUnplanIDs.IndexOf(taskID) + 1, barrierID);
                }

                tblPlanLog.Rows.AddAt(idx, tblRow);
            }
        }

        private void AddLeaveRow(WeeklyPlan wp, bool planned, bool insert, bool setUpdated)
        {
            string prefix = GetControlPrefix();
            ViewState["leavePlanned"] = planned;
            cellLeave.Visible = false;

            TableRow tblRow = new TableRow();
            tblRow.ID = "row_leave";
            myControls.Add(tblRow.ID, tblRow);

            HiddenField hdnUpdate = new HiddenField();
            hdnUpdate.ID = "hdnUpdate_leave";

            if (setUpdated)
            {
                hdnUpdate.Value = "y";
            }

            if (myRemoveIDs.Contains("leave"))
            {
                myRemoveIDs.Remove("leave");
            }

            if (!planned || myMode.Value == "plan")
            {
                CheckBox chk = new CheckBox();
                chk.ID = "chk_leave";
                chk.CssClass = "rowCheck";
                chk.Attributes.Add("onclick", "RowChecked(this, '" + (planned ? "p" : "u") + "')");
                tblRow.Cells.Add(CreateCell(hdnUpdate, chk));
            }
            else
            {
                tblRow.Cells.Add(CreateCell(hdnUpdate, "chkbox"));
            }

            TableCell cell = new TableCell();
            cell.Text = planned ? "Leave/Holiday" : "Unplanned Leave/Holiday";
            cell.CssClass = "lastField";
            cell.ColumnSpan = myMode.Value == "view" ? 6 : 7;
            if (hdrCode.Visible) cell.ColumnSpan++;
            if (hdrComp.Visible) cell.ColumnSpan++;
            tblRow.Cells.Add(cell);

            double hours;
            double planTotal = 0;
            double actualTotal = 0;
            string tbl = planned ? "P" : "U";
            bool addedDiv = false;

            for (int i = 0; i < 7; i++)
            {
                if (planned && myMode.Value != "plan")
                {
                    if (wp != null)
                    {
                        hours = wp.LeavePlanHours[i];
                        cell = CreateCell(FormatHours(hours), "leave", i, 1, "planHour");
                    }
                    else
                    {
                        cell = CreateCell("leave", i, 1, "planHour");
                        hours = 0;
                        double.TryParse(cell.Text, out hours);
                    }

                    CheckWE(cell, i, hours);
                    tblRow.Cells.Add(cell);                    

                    planTotal += hours;
                    myPlanTotals[i] += hours;
                    myPlanTotals[7] += hours;
                }

                if (myMode.Value == "view")
                {
                    if (wp != null)
                    {
                        hours = wp.LeaveActualHours[i];
                        cell = CreateCell(FormatHours(hours), "leaveA", i, planned ? 1 : 2, "hour");
                    }
                    else
                    {
                        cell = CreateCell("leaveA", i, planned ? 1 : 2, "hour");
                        hours = 0;
                        double.TryParse(cell.Text, out hours);
                    }

                    CheckWE(cell, i, hours);
                    tblRow.Cells.Add(cell);
                    actualTotal += hours;

                    if (planned)
                    {
                        myActualTotals[i] += hours;
                        myActualTotals[7] += hours;
                    }
                    else
                    {
                        myUnplanTotals[i] += hours;
                        myUnplanTotals[7] += hours;
                    }
                }
                else
                {
                    string updFunc = "UpdateTotal(this," + i.ToString() + ",'" + tbl + "','leave'";
                    TextBox txt = new TextBox();
                    txt.ID = "txtHours" + tbl + "_leave_" + i.ToString();
                    txt.Width = 26;
                    txt.Style.Add(HtmlTextWriterStyle.TextAlign, "right");                    
                    txt.Attributes.Add("onchange", updFunc + ",1)");
                    txt.Attributes.Add("onkeydown", "return CancelEnter(event)");
                    txt.Attributes.Add("onkeyup", updFunc + ")");

                    if (wp != null)
                    {
                        if (myMode.Value == "plan")
                        {
                            hours = wp.LeavePlanHours[i];
                        }
                        else
                        {
                            hours = wp.LeaveActualHours[i];
                        }

                        txt.Text = FormatHours(hours);
                    }
                    else
                    {
                        hours = 0;
                        double.TryParse(HttpContext.Current.Request[prefix + txt.ID], out hours);
                    }

                    cell = CreateCell(txt, planned ? 1 : 2, "inputHour");
                    CheckWE(cell, i, hours);
                    tblRow.Cells.Add(cell);

                    if (myMode.Value == "plan")
                    {
                        planTotal += hours;
                        myPlanTotals[i] += hours;
                        myPlanTotals[7] += hours;
                    }
                    else
                    {
                        actualTotal += hours;

                        if (planned)
                        {
                            myActualTotals[i] += hours;
                            myActualTotals[7] += hours;
                        }
                        else
                        {
                            myUnplanTotals[i] += hours;
                            myUnplanTotals[7] += hours;
                        }
                    }
                }

                if (!planned && i == 4)
                {
                    addedDiv = AddUnplanDivider(tblRow, insert);
                }
            }

            if (planned && myMode.Value == "plan")
            {
                tblRow.Cells.Add(CreateTotalCell("<div id=\"totalRow_leave\">" +
                    Utility.RoundDoubleToString(planTotal) + "</div>", "totalRow"));
            }
            else
            {
                if (planned)
                {
                    tblRow.Cells.Add(CreateTotalCell(Utility.RoundDoubleToString(planTotal), "totalRowPlan"));
                }

                tblRow.Cells.Add(CreateTotalCell("<div id=\"totalRow_leave\">" +
                    Utility.RoundDoubleToString(actualTotal) + "</div>", "totalRow", planned ? 1 : 2));
            }

            if (insert)
            {
                tblPlanLog.Rows.AddAt(tblPlanLog.Rows.Count - 1, tblRow);
            }
            else
            {
                tblPlanLog.Rows.Add(tblRow);
            }

            if (planned)
            {
                hdrToggleWE.RowSpan++;
            }
            else if (addedDiv)
            {
                ((TableHeaderCell)myControls["hdrToggleWE2"]).RowSpan = 2;
            }
            else
            {
                ((TableHeaderCell)myControls["hdrToggleWE2"]).RowSpan++;
            }
        }

        private TableCell CreateCell(
            string text,
            string cssClass
        )
        {
            TableCell cell = new TableCell();
            cell.Text = text;
            cell.CssClass = cssClass;
            return cell;
        }

        private TableCell CreateCell(
            string text,
            string id, 
            int col,
            int colspan)
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = text;
            cell.ColumnSpan = colspan;
            ViewState[cell.ID] = text;
            return cell;
        }

        private TableCell CreateCell(
            string id,
            int col,
            int colspan)
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.ColumnSpan = colspan;
            cell.Text = (string)ViewState[cell.ID];
            return cell;
        }

        private TableCell CreateCell(
            string text,
            string id,
            int col,
            int colspan,
            string cssClass
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = text;
            cell.ColumnSpan = colspan;
            cell.CssClass = cssClass;
            ViewState[cell.ID] = text;
            return cell;
        }

        private TableCell CreateCell(
            string text,
            string id,
            int col,
            int colspan,
            string cssClass,
            bool storeClass
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = text;
            cell.ColumnSpan = colspan;

            if (cssClass != string.Empty)
            {
                cell.CssClass = cssClass;

                if (storeClass)
                {
                    ViewState[cell.ID + "_css"] = cssClass;
                }
            }

            ViewState[cell.ID] = text;
            return cell;
        }

        private TableCell CreateCell(
            string id,
            int col,
            int colspan,
            string cssClass
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.ColumnSpan = colspan;
            cell.CssClass = cssClass;
            cell.Text = (string)ViewState[cell.ID];
            return cell;
        }

        private TableCell CreateCell(
            string id,
            int col,
            int colspan,
            bool loadClass)
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.ColumnSpan = colspan;

            if (loadClass && ViewState[cell.ID + "_css"] != null)
            {
                cell.CssClass = (string)ViewState[cell.ID + "_css"];
            }

            cell.Text = (string)ViewState[cell.ID];
            return cell;
        }

        private TableCell CreateCell(LinkButton lnk, string id, int colspan)
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_title";
            cell.ColumnSpan = colspan;
            cell.Controls.Add(lnk);

            if (lnk.Text != string.Empty)
            {
                ViewState[lnk.ID] = lnk.Text;
            }
            else
            {
                lnk.Text = (string)ViewState[lnk.ID];
            }

            return cell;
        }

        private TableCell CreateCell(
            Control ctrl,
            string cssClass
        )
        {
            TableCell cell = new TableCell();
            cell.CssClass = cssClass;
            cell.Controls.Add(ctrl);
            myControls.Add(ctrl.ID, ctrl);
            return cell;
        }

        private TableCell CreateCell(Control ctrl, int colspan, string cssClass)
        {
            TableCell cell = new TableCell();
            cell.Controls.Add(ctrl);
            cell.ColumnSpan = colspan;
            cell.CssClass = cssClass;
            myControls.Add(ctrl.ID, ctrl);
            return cell;
        }

        private TableCell CreateCell(Control ctrl1, Control ctrl2)
        {
            TableCell cell = new TableCell();
            cell.Controls.Add(ctrl1);
            cell.Controls.Add(ctrl2);
            cell.CssClass = "chkbox";
            myControls.Add(ctrl1.ID, ctrl1);
            myControls.Add(ctrl2.ID, ctrl2);
            return cell;
        }

        private TableHeaderCell CreateTotalCell(
            string text,
            string cssClass,
            int colspan
        )
        {
            TableHeaderCell cell = new TableHeaderCell();
            cell.Text = text;
            cell.ColumnSpan = colspan;
            cell.HorizontalAlign = HorizontalAlign.Right;

            if (cssClass != string.Empty)
            {
                cell.CssClass = cssClass;

                if (!myShowWE && cssClass.EndsWith("WE"))
                {
                    cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
            }

            return cell;
        }

        private TableHeaderCell CreateTotalCell(string cssClass)
        {
            return CreateTotalCell(string.Empty, cssClass, 1);
        }

        private TableHeaderCell CreateTotalCell(string text, string cssClass)
        {
            return CreateTotalCell(text, cssClass, 1);
        }

        protected void lnkChangeProfile_Click(object sender, EventArgs e)
        {
            int count = ddProfile.Items.Count;
            int idx = ddProfile.SelectedIndex;

            if (count > 0 && idx != -1)
            {
                if (((ImageButton)sender).ID == "lnkPrevProfile")
                {
                    idx--;
                    if (idx == -1) idx = count - 1;
                }
                else
                {
                    idx++;
                    if (idx == count) idx = 0;
                }

                ddProfile.SelectedIndex = idx;
                ddProfile_SelectedIndexChanged(ddProfile, null);
            }
        }

        protected void lnkPrevWeek_Click(object sender, EventArgs e)
        {
            LoadWeeks(DateTime.Parse(ddWeek.SelectedValue).AddDays(-7));
            LoadData(true, false);
        }

        protected void lnkNextWeek_Click(object sender, EventArgs e)
        {
            LoadWeeks(DateTime.Parse(ddWeek.SelectedValue).AddDays(7));
            LoadData(true, false);
        }

        protected void ddWeek_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadWeeks(DateTime.Parse(ddWeek.SelectedValue));
            LoadData(true, false);
        }

        protected void txtWeek_TextChanged(object sender, EventArgs e)
        {
            LoadWeeks(DateTime.Parse(txtWeek.Text));
            LoadData(true, false);
        }

        protected void lnkAddTask_Click(object sender, EventArgs e)
        {
            List<long> usedWeeklyTasks = new List<long>();
            List<long> usedTasks = new List<long>();
            ArrayList list = myPlanIDs;

            for (int i = 0; i < 2; i++)
            {
                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid) && IsTask(sid))
                    {
                        if (IsNew(sid))
                        {
                            usedTasks.Add(long.Parse(sid.Substring(2)));
                        }
                        else
                        {
                            usedWeeklyTasks.Add(long.Parse(sid.Substring(2)));
                        }
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            int taskCount = 0;
            List<DTO.Task> tasks = BLL.TaskManager.GetInstance().GetUnplannedTaskList(long.Parse(ddProfile.SelectedValue), usedWeeklyTasks, usedTasks);
            string script = string.Empty;

            if (tasks.Count > 0)
            {
                addTaskSelector.Items.Clear();

                foreach (DTO.Task task in tasks)
                {
                    if (myMode.Value == "plan" || task.Status != DTO.Task.StatusEnum.HOLD)
                    {
                        addTaskSelector.Items.Add(new ListItem(task.Title, task.Id.ToString()));
                        string idStr = task.Id.ToString();
                        script += "var taskProgram_" + idStr + "='" +
                            (task.Program != null ? task.Program.Title : string.Empty) +
                            "';\nvar taskWBS_" + idStr + "='" + task.WBS +
                            "';\nvar taskAllocated_" + idStr + "='" + (task.Hours > 0 ? task.Hours.ToString() : string.Empty) +
                            "';\nvar taskStart_" + idStr + "='" + (task.StartDate.HasValue ? task.StartDate.Value.ToString("M/d/yy") : string.Empty) +
                            "';\nvar taskDue_" + idStr + "='" + (task.DueDate.HasValue ? task.DueDate.Value.ToString("M/d/yy") : string.Empty) +
                            "';\nvar taskStatus_" + idStr + "='" + FormatEnum(task.Status) +
                            "';\nvar taskExit_" + idStr + "='" + JSComments(task.ExitCriteria, true) +
                            "';\nvar taskOComments_" + idStr + "='" + JSComments(task.OwnerComments, true) +
                            "';\nvar taskAComments_" + idStr + "='" + JSComments(task.AssigneeComments, false) +
                            "';\nvar taskType_" + idStr + "=" + (task.TaskType != null ? task.TaskType.Id.ToString() : "-1") +
                            ";\nvar taskTypeTitle_" + idStr + "='" + (task.TaskType != null ? task.TaskType.Title : string.Empty) +
                            "';\nvar taskInst_" + idStr + "='" + task.Instantiated.ToString() + "';\n";

                        if (hdrComp.Visible)
                        {
                            script += "var taskComp_" + idStr + "='" + 
                                (task.Complexity != null ? task.Complexity.Id.ToString() : string.Empty) + "';\nvar taskRE_" + idStr + "='";

                            if (task.Instantiated)
                            {
                                script += (task.Estimate > 0 ? task.Estimate.ToString() : string.Empty) + "'\n";
                            }
                            else
                            {
                                script += (task.Complexity != null ? task.Complexity.Hours.ToString() : string.Empty) + "'\n";
                            }
                        }
                        else
                        {
                            script += "var taskRE_" + idStr + "='" + (task.Estimate > 0 ? task.Estimate.ToString() : string.Empty) + "'\n";
                        }

                        taskCount++;
                    }
                }
            }

            if (taskCount > 0)
            {
                PopupForm(popupTask);
                popupTask.Attributes.Add("title", "Add Task");
                phAddTaskSelector.Visible = true;
                phAddFav.Visible = false;
                trProgram.Visible = false;
                trOwner.Visible = true;
                phEditTaskInfo.Visible = false;
                phAddTaskComp.Visible = hdrComp.Visible;
                phAddTaskCode.Visible = myMode.Value == "log";
                divAddTaskTypeTree.Style.Add(HtmlTextWriterStyle.Display, "none");
                btnAddTaskToPlan.Visible = true;
                btnAddFavToPlan.Visible = false;
                btnUpdTask.Visible = false;

                lblEditTaskOComments.Visible = false;
                lblEditTaskType.Visible = false;
                lblEditTaskComp.Visible = false;
                lblEditTaskRE.Visible = false;
                lblEditTaskExit.Visible = false;

                addTaskType.Visible = true;
                addTaskRE.Visible = true;
                addTaskExit.Visible = true;
                addTaskRE.Style.Add(HtmlTextWriterStyle.Display, "none");
                addTaskExit.Style.Add(HtmlTextWriterStyle.Display, "none");

                cellAddTask.Attributes["onclick"] = "$('.popupTask').dialog('open');CloseAddMenu(1)";

                if (myMode.Value == "log")
                {
                    addUnplanCode.LoadUnplannedCodes(GetWeeklyTeam());
                }

                addTaskRE.Text = string.Empty;
                addTaskAComments.Text = string.Empty;
                addTaskType.LoadTaskTypes(GetWeeklyTeam(), hdrComp.Visible);
            }
            else
            {
                script += "alert('There are no unused open tasks in your task queue');";
            }

            AjaxControlToolkit.ToolkitScriptManager.RegisterStartupScript(this, this.GetType(), "taskInfo", script + "\n", true);
            StoreChanges();
        }

        private void PopupForm(Panel form)
        {
            form.Visible = true;

            if (form.ID != "popupTask")
            {
                popupTask.Visible = false;
            }

            if (form.ID != "popupPrevTasks")
            {
                popupPrevTasks.Visible = false;
            }

            if (form.ID != "popupBarrier")
            {
                popupBarrier.Visible = false;
            }

            if (form.ID != "popupAlert")
            {
                popupAlert.Visible = false;
            }

            if (form.ID != "popupCarry")
            {
                popupCarry.Visible = false;
            }

            phUpdatePanel.Controls.Add(new LiteralControl(
                "<input type=\"hidden\" id=\"popupForm\" name=\"popupForm\" value=\"" + form.ID + "\" />"));
        }

        private string JSComments(string txt, bool br)
        {
            string fix = string.Empty;

            if (txt != null)
            {
                fix = txt.Replace("'", "\\'");

                if (br)
                {
                    fix = fix.Replace("\r\n", "<br>");
                }
                else
                {
                    fix = fix.Replace("\r\n", "\\r\\n");
                }
            }

            return fix;
        }

        protected void lnkAddFav_Click(object sender, EventArgs e)
        {
            ArrayList list = myPlanIDs;
            List<long> favIDs = new List<long>();

            for (int i = 0; i < 2; i++)
            {
                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid) && sid.Contains('-'))
                    {
                        favIDs.Add(long.Parse(sid.Substring(3)));
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            lbFavs.Items.Clear();
            string script = string.Empty;
            List<Favorite> favs = BLL.TaskManager.GetInstance().GetFavorites(long.Parse(ddProfile.SelectedValue));

            foreach (Favorite fav in favs)
            {
                if (!favIDs.Contains(fav.Id))
                {
                    string idStr = fav.Id.ToString();
                    lbFavs.Items.Add(new ListItem(fav.Title, idStr));
                    
                    script += "var favProgram_" + idStr + "='" +
                        (fav.Program != null ? fav.Program.Id.ToString() : string.Empty) +
                        "';\nvar favAllocated_" + idStr + "='" + (fav.Hours > 0 ? fav.Hours.ToString() : string.Empty) +
                        "';\nvar favType_" + idStr + "=" + (fav.TaskType != null ? fav.TaskType.Id.ToString() : "-1") + ";\n";

                    if (hdrComp.Visible)
                    {
                        script += "var favComp_" + idStr + "='" +
                            (fav.Complexity != null ? fav.Complexity.Id.ToString() : string.Empty) + "';\nvar favRE_" + idStr + "='" +
                            (fav.Complexity != null ? fav.Complexity.Hours.ToString() : string.Empty) + "'\n";
                    }
                    else
                    {
                        script += "var favRE_" + idStr + "='" + (fav.Estimate > 0 ? fav.Estimate.ToString() : string.Empty) + "'\n";
                    }
                }
            }

            if (lbFavs.Items.Count > 0)
            {
                PopupForm(popupTask);
                popupTask.Attributes.Add("title", "Add Favorite");
                phAddTaskSelector.Visible = false;
                phAddFav.Visible = true;
                trProgram.Visible = true;
                trOwner.Visible = false;
                phEditTaskInfo.Visible = false;
                phAddTaskComp.Visible = hdrComp.Visible;
                phAddTaskCode.Visible = myMode.Value == "log";
                btnAddTaskToPlan.Visible = false;
                btnAddFavToPlan.Visible = true;
                btnUpdTask.Visible = false;

                lblEditTaskOComments.Visible = false;
                lblEditTaskType.Visible = false;
                lblEditTaskComp.Visible = false;
                lblEditTaskRE.Visible = false;
                lblEditTaskExit.Visible = false;

                addTaskType.Visible = true;
                addTaskRE.Visible = true;
                addTaskExit.Visible = true;

                divAddTaskTypeTree.Style.Clear();
                addTaskRE.Style.Clear();
                addTaskExit.Style.Clear();

                cellAddFav.Attributes["onclick"] = "$('.popupTask').dialog('open');CloseAddMenu(1)";

                if (myMode.Value == "log")
                {
                    addUnplanCode.LoadUnplannedCodes(GetWeeklyTeam());
                }

                addTaskRE.Text = string.Empty;
                addTaskAComments.Text = string.Empty;
                addTaskExit.Text = string.Empty;
                addTaskType.LoadTaskTypes(GetWeeklyTeam(), hdrComp.Visible);

                LoadPrograms();
            }
            else
            {
                script += "alert('There are no unused favorites to select');";
            }

            AjaxControlToolkit.ToolkitScriptManager.RegisterStartupScript(this, this.GetType(), "favInfo", script + "\n", true);
            StoreChanges();
        }

        protected void lnkAddPrev_Click(object sender, EventArgs e)
        {
            long profileId = long.Parse(ddProfile.SelectedValue);
            DateTime lastWeek = DateTime.Parse(ddWeek.SelectedValue).AddDays(-7);
            WeeklyPlan.StatusEnum? status = BLL.TaskManager.GetInstance().GetWeeklyPlanStatus(profileId, lastWeek);
            lbPrevTasks.Items.Clear();

            if (status.HasValue)
            {
                List<long> taskIDs = new List<long>();
                ArrayList list = myPlanIDs;

                for (int i = 0; i < 2; i++)
                {
                    foreach (string taskID in list)
                    {
                        if (!myRemoveIDs.Contains(taskID) && IsTask(taskID))
                        {
                            if (IsNew(taskID))
                            {
                                taskIDs.Add(long.Parse(taskID.Substring(2)));
                            }
                            else if (ViewState["taskID_" + taskID] != null)
                            {
                                taskIDs.Add((long)ViewState["taskID_" + taskID]);
                            }
                        }
                    }

                    if (myMode.Value == "plan")
                    {
                        i = 2;
                    }
                    else if (i == 0)
                    {
                        list = myUnplanIDs;
                    }
                }

                List<DTO.Task> tasks = BLL.TaskManager.GetInstance().GetOpenTasksByWeek(profileId, lastWeek);

                foreach (DTO.Task task in tasks)
                {
                    if (!taskIDs.Contains(task.Id))
                    {
                        lbPrevTasks.Items.Add(new ListItem(task.Title, task.Id.ToString()));
                    }
                }
            }

            if (lbPrevTasks.Items.Count > 0)
            {
                lblPrevWeek.Text = lastWeek.AddDays(-6).ToString("M/d/yy") + " - " + lastWeek.ToString("M/d/yy");
                PopupForm(popupPrevTasks);
                cellAddPrev.Attributes["onclick"] = "$('.popupPrevTasks').dialog('open');CloseAddMenu(1)";
            }
            else
            {
                AjaxControlToolkit.ToolkitScriptManager.RegisterStartupScript(this, this.GetType(), "noPrev",
                    "alert('There are no unused open tasks from the previous week');\n", true);
            }

            StoreChanges();
        }

        protected void btnAddPrev_Click(object sender, EventArgs e)
        {
            bool success = true;

            foreach (ListItem item in lbPrevTasks.Items)
            {
                if (item.Selected)
                {
                    success = success && AddTaskToPlan(item.Value, false);
                }
            }

            if (success)
            {
                btnUndo.Style.Clear();
                btnSave.Style.Clear();
                updateMade.Value = "y";
            }

            StoreChanges();
        }

        private void LoadPrograms()
        {
            ddProgram.Items.Clear();
            ddProgram.Items.Add(new ListItem());
            List<Program> progList = BLL.AdminManager.GetInstance().GetProgramList();

            foreach (Program prog in progList)
            {
                ddProgram.Items.Add(new ListItem(prog.Title, prog.Id.ToString()));
            }
        }

        protected void lnkAddBarrier_Click(object sender, EventArgs e)
        {
            ArrayList list = myPlanIDs;
            string taskID = string.Empty;

            for (int i = 0; i < 2; i++)
            {
                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid) && IsTask(sid) &&
                        ((CheckBox)myControls["chk_" + sid]).Checked)
                    {
                        taskID = sid;
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            if (taskID != string.Empty)
            {
                PopupForm(popupBarrier);
                popupBarrier.Attributes.Add("title", "Add Barrier");
                barTask.Text = (string)ViewState["lnkTitle_" + taskID];
                barTaskID.Value = taskID;
                barCode.LoadBarriers(GetWeeklyTeam());
                barComment.Text = string.Empty;
                barTicket.Text = string.Empty;
                btnAddBarrier.Visible = true;
                btnUpdBarrier.Visible = false;
            }

            StoreChanges();
        }

        private Team GetWeeklyTeam()
        {
            Team team = new Team();
            team.Id = long.Parse(ddTeam.SelectedValue);
            return team;
        }

        protected void btnAddTaskToPlan_Click(object sender, EventArgs e)
        {
            if (AddTaskToPlan(addTaskSelector.SelectedValue, true))
            {
                StoreChanges();
            }
        }

        protected void btnAddFavToPlan_Click(object sender, EventArgs e)
        {
            string sid = "n_-" + lbFavs.SelectedValue;
            WeeklyTask wt = BLL.TaskManager.GetInstance().GetTaskTemplate(long.Parse(lbFavs.SelectedValue));

            long progID = long.Parse(ddProgram.SelectedValue);

            if (wt.Task.Program == null || wt.Task.Program.Id != progID)
            {
                wt.Task.Program = new Program();
                wt.Task.Program.Id = progID;
                wt.Task.Program.Title = ddProgram.SelectedItem.Text;
                StoreSessionValue(sid + "_progID", progID, true);
            }

            if (hdrComp.Visible)
            {
                long compID = addTaskType.GetComplexityID();

                if (compID != -1)
                {
                    Complexity comp = BLL.TaskManager.GetInstance().GetComplexity(compID);

                    if (wt.Task.Complexity == null || comp.Id != wt.Task.Complexity.Id || comp.Hours != wt.Task.Estimate)
                    {
                        wt.Task.Complexity = comp;
                        wt.Task.Estimate = comp.Hours;
                        StoreSessionValue(sid + "_compID", compID, true);
                    }
                }
            }
            else
            {
                double re;

                if (Double.TryParse(addTaskRE.Text, out re))
                {
                    if (wt.Task.Estimate != re)
                    {
                        wt.Task.Estimate = re;
                        StoreSessionValue(sid + "_re", re, true);
                    }
                }
            }
                    
            long ttId = addTaskType.GetSelectedId();

            if (wt.Task.TaskType == null || ttId != wt.Task.TaskType.Id)
            {
                wt.Task.TaskType = new TaskType();
                wt.Task.TaskType.Id = ttId;
                wt.Task.TaskType.Title = addTaskType.GetSelectedValue();
                StoreSessionValue(sid + "_type", ttId, true);
            }

            if (wt.Task.ExitCriteria != addTaskExit.Text)
            {
                StoreSessionValue(sid + "_exit", addTaskExit.Text, true);
            }

            if (wt.Task.AssigneeComments != addTaskAComments.Text)
            {
                StoreSessionValue(sid + "_comment", addTaskAComments.Text, true);
            }

            if (myMode.Value == "plan")
            {
                myPlanIDs.Add(sid);
                AddTaskRow(sid, wt, true, true, true);
            }
            else
            {
                wt.ActualDayComplete = -1;
                wt.UnplannedCode = BLL.AdminManager.GetInstance().GetUnplannedCode(addUnplanCode.GetSelectedId());
                StoreSessionValue(sid + "_ucode", wt.UnplannedCode.Id, true);

                if (!hdrCode.Visible)
                {
                    foreach (string pid in myPlanIDs)
                    {
                        if (!myRemoveIDs.Contains(pid))
                        {
                            ((TableRow)myControls["row_" + pid]).Cells[1].ColumnSpan++;
                        }
                    }

                    if (ViewState["leavePlanned"] != null)
                    {
                        ((TableRow)myControls["row_leave"]).Cells[1].ColumnSpan++;
                    }

                    if (myControls.ContainsKey("cellUnplanDiv"))
                    {
                        ((TableHeaderCell)myControls["cellUnplanDiv"]).ColumnSpan++;
                    }

                    ((TableHeaderRow)myControls["rowPlanTotals"]).Cells[0].ColumnSpan++;
                    hdrCode.Visible = true;
                }

                myUnplanIDs.Add(sid);
                AddTaskRow(sid, wt, false, true, true);
            }

            StoreChanges();
        }

        private bool AddTaskToPlan(string taskID, bool update)
        {
            long id;
            string sid = "n_" + taskID;

            if (!long.TryParse(taskID, out id) || myPlanIDs.Contains(sid) || myUnplanIDs.Contains(sid))
            {
                return false;
            }

            WeeklyTask wt = new WeeklyTask();
            wt.Task = BLL.TaskManager.GetInstance().GetTask(id, false);

            if (update)
            {
                if (!wt.Task.Instantiated)
                {
                    if (hdrComp.Visible)
                    {
                        long compID = addTaskType.GetComplexityID();

                        if (compID != -1)
                        {
                            Complexity comp = BLL.TaskManager.GetInstance().GetComplexity(compID);

                            if (wt.Task.Complexity == null || comp.Id != wt.Task.Complexity.Id || comp.Hours != wt.Task.Estimate)
                            {
                                wt.Task.Complexity = comp;
                                wt.Task.Estimate = comp.Hours;
                                StoreSessionValue(sid + "_compID", compID, true);
                            }
                        }
                    }
                    else
                    {
                        double re;

                        if (Double.TryParse(addTaskRE.Text, out re))
                        {
                            if (wt.Task.Estimate != re)
                            {
                                wt.Task.Estimate = re;
                                StoreSessionValue(sid + "_re", re, true);
                            }
                        }
                    }
                    
                    long ttId = addTaskType.GetSelectedId();

                    if (ttId != -1 && (wt.Task.TaskType == null || ttId != wt.Task.TaskType.Id))
                    {
                        wt.Task.TaskType = new TaskType();
                        wt.Task.TaskType.Id = ttId;
                        wt.Task.TaskType.Title = addTaskType.GetSelectedValue();
                        StoreSessionValue(sid + "_type", ttId, true);
                    }

                    if (wt.Task.ExitCriteria != addTaskExit.Text)
                    {
                        StoreSessionValue(sid + "_exit", addTaskExit.Text, true);
                    }
                }

                if (wt.Task.AssigneeComments != addTaskAComments.Text)
                {
                    StoreSessionValue(sid + "_comment", addTaskAComments.Text, true);
                }
            }

            wt.PlanDayComplete = -1;
            wt.ActualDayComplete = -1;
            Favorite fav = null;

            if (myMode.Value == "plan" && HttpContext.Current.Request["favID"] != null &&
                long.TryParse(HttpContext.Current.Request["favID"], out id))
            {
                fav = BLL.TaskManager.GetInstance().GetFavorite(id);
            }

            for (int i = 0; i < 7; i++)
            {
                if (fav != null)
                {
                    wt.PlanHours.Add(i, fav.PlanHours[i]);
                }
                else
                {
                    wt.PlanHours.Add(i, 0);
                }

                wt.ActualHours.Add(i, 0);
            }

            if (myMode.Value == "plan")
            {
                myPlanIDs.Add(sid);
                AddTaskRow(sid, wt, true, true, true);
            }
            else
            {
                if (update)
                {
                    wt.UnplannedCode = BLL.AdminManager.GetInstance().GetUnplannedCode(addUnplanCode.GetSelectedId());
                }
                else if (HttpContext.Current.Request["ucode"] != null &&
                    long.TryParse(HttpContext.Current.Request["ucode"], out id))
                {
                    wt.UnplannedCode = BLL.AdminManager.GetInstance().GetUnplannedCode(id);
                }
                else
                {
                    return false;
                }

                StoreSessionValue(sid + "_ucode", wt.UnplannedCode.Id, true);

                if (!hdrCode.Visible)
                {
                    foreach (string pid in myPlanIDs)
                    {
                        if (!myRemoveIDs.Contains(pid))
                        {
                            ((TableRow)myControls["row_" + pid]).Cells[1].ColumnSpan++;
                        }
                    }

                    if (ViewState["leavePlanned"] != null)
                    {
                        ((TableRow)myControls["row_leave"]).Cells[1].ColumnSpan++;
                    }

                    if (myControls.ContainsKey("cellUnplanDiv"))
                    {
                        ((TableHeaderCell)myControls["cellUnplanDiv"]).ColumnSpan++;
                    }

                    ((TableHeaderRow)myControls["rowPlanTotals"]).Cells[0].ColumnSpan++;
                    hdrCode.Visible = true;
                }                

                myUnplanIDs.Add(sid);
                AddTaskRow(sid, wt, false, true, true);
            }

            return true;
        }

        protected void btnAddBarrier_Click(object sender, EventArgs e)
        {
            string val = barCode.GetSelectedValue();

            WeeklyBarrier wb = new WeeklyBarrier();
            wb.Barrier = new Barrier();
            wb.Barrier.Id = barCode.GetSelectedId();
            wb.Barrier.Code = val.Substring(0, val.IndexOf(" -"));
            wb.Barrier.Title = val.Substring(val.IndexOf(" -") + 3);
            wb.Comment = barComment.Text;
            wb.BarrierType = barTypeEfficiency.Checked ?
                WeeklyBarrier.BarriersEnum.EFFICIENCY : WeeklyBarrier.BarriersEnum.DELAY;

            for (int i = 0; i < 7; i++)
            {
                wb.Hours.Add(i, 0);
            }

            string bid = "a_" + (myPlanIDs.Count + myUnplanIDs.Count).ToString();
            AddBarrierRow(bid, barTaskID.Value, wb, (myPlanIDs.Contains(barTaskID.Value)), true);
            StoreSessionValue(bid + "_id", wb.Barrier.Id, true);

            if (barTicket.Text != string.Empty)
            {
                StoreSessionValue(bid + "_ticket", barTicket.Text, false);
            }

            if (cellToggleBarriers.Text == "Show Barriers")
            {
                cellToggleBarriers.Text = "Hide Barriers";
                HttpContext.Current.Session["weekly_showBarriers"] = true;
                ArrayList list = myPlanIDs;

                for (int i = 0; i < 2; i++)
                {
                    foreach (string sid in list)
                    {
                        if (!myRemoveIDs.Contains(sid) && IsBarrier(sid))
                        {
                            TableRow tblRow = (TableRow)myControls["row_" + sid];
                            tblRow.Style.Clear();

                            if (!HttpContext.Current.Request.UserAgent.Contains("MSIE 7"))
                            {
                                if (i == 0)
                                {
                                    hdrToggleWE.RowSpan++;
                                }
                                else
                                {
                                    ((TableHeaderCell)myControls["hdrToggleWE2"]).RowSpan++;
                                }
                            }
                        }
                    }

                    if (myMode.Value == "plan")
                    {
                        i = 2;
                    }
                    else if (i == 0)
                    {
                        list = myUnplanIDs;
                    }
                }
            }

            StoreChanges();
            ClearRowChecks();
        }

        protected void btnUpdBarrier_Click(object sender, EventArgs e)
        {
            string bid = barTaskID.Value;
            long barID = barCode.GetSelectedId();
            bool updated = false;
            WeeklyBarrier wb = BuildWeeklyBarrier(bid, false, false);
            TableRow tblRow = (TableRow)myControls["row_" + bid];

            if (wb.Barrier.Id != barID)
            {
                UpdateCell(tblRow.Cells[1], "<table class=\"invisible\"><tr><td width=\"20\" style=\"white-space: nowrap\"> &nbsp; &nbsp; </td><td>" +
                    barCode.GetSelectedValue() + "</td></tr></table>");

                StoreSessionValue(bid + "_id", barID, true);
                updated = true;
            }

            if ((wb.BarrierType == WeeklyBarrier.BarriersEnum.EFFICIENCY && barTypeDelay.Checked) ||
                (wb.BarrierType == WeeklyBarrier.BarriersEnum.DELAY && barTypeEfficiency.Checked))
            {
                string btype;
                string newType;

                if (barTypeEfficiency.Checked)
                {
                    UpdateCell(tblRow.Cells[2], "Efficiency");
                    btype = "D_";
                    newType = "E_";
                }
                else
                {
                    UpdateCell(tblRow.Cells[2], "Delay");
                    btype = "E_";
                    newType = "D_";
                }

                for (int i = 0; i < 7; i++)
                {
                    if (myControls.ContainsKey("txtBarHours" + btype + bid + "_" + i.ToString()))
                    {
                        TextBox txt = (TextBox)myControls["txtBarHours" + btype + bid + "_" + i.ToString()];
                        myControls.Remove(txt.ID);
                        txt.ID = "txtBarHours" + newType + bid + "_" + i.ToString();
                        myControls.Add(txt.ID, txt);

                        if (txt.Style.Count == 1)
                        {
                            double hours;
                            txt.Attributes["onchange"] = "UpdateTotal(this," + i.ToString() + ",'" + newType[0].ToString() + "','" + bid + "',1)";

                            if (double.TryParse(txt.Text, out hours))
                            {
                                if (barTypeEfficiency.Checked)
                                {
                                    myBarrierTotals[i] += hours;
                                    myBarrierTotals[7] += hours;
                                    myDelayTotals[i] -= hours;
                                    myDelayTotals[7] -= hours;
                                }
                                else
                                {
                                    myBarrierTotals[i] -= hours;
                                    myBarrierTotals[7] -= hours;
                                    myDelayTotals[i] += hours;
                                    myDelayTotals[7] += hours;
                                }
                            }
                        }
                    }
                }

                SetTotalRows();
                updated = true;
            }

            if (wb.Comment != barComment.Text)
            {
                UpdateCell(tblRow.Cells[3], barComment.Text);
                updated = true;
            }

            if (wb.Ticket != barTicket.Text)
            {
                StoreSessionValue(bid + "_ticket", barTicket.Text, true);
            }

            if (updated)
            {
                ((HiddenField)myControls["hdnUpdate_" + bid]).Value = "y";
            }

            StoreChanges();
        }

        protected void lnkNewTask_Click(object sender, EventArgs e)
        {
            StoreChanges();

            if (myMode.Value == "plan")
            {
                Response.Redirect("/Task/TaskForm.aspx?plan=y&profileID=" + ddProfile.SelectedValue);
            }
            else if (myMode.Value == "log")
            {
                Response.Redirect("/Task/TaskForm.aspx?unplan=y&profileID=" + ddProfile.SelectedValue);
            }
        }

        protected void lnkEdit_Click(object sender, EventArgs e)
        {
            ArrayList list = myPlanIDs;
            bool planned = true;
            string lastTask = string.Empty;
            string taskID = string.Empty;
            string chkID = string.Empty;

            for (int i = 0; i < 2; i++)
            {
                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid))
                    {
                        if (((CheckBox)myControls["chk_" + sid]).Checked)
                        {
                            chkID = sid;

                            if (IsTask(sid))
                            {
                                planned = i == 0;
                            }
                            else
                            {
                                taskID = lastTask;
                            }
                        }

                        if (IsTask(sid))
                        {
                            lastTask = sid;
                        }
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            if (chkID != string.Empty)
            {
                if (IsTask(chkID))
                {
                    WeeklyTask wt = BuildWeeklyTask(chkID, false);

                    PopupForm(popupTask);
                    popupTask.Attributes.Add("title", "Edit Task");
                    phAddTaskSelector.Visible = false;
                    phAddFav.Visible = false;
                    phEditTaskInfo.Visible = true;
                    phAddTaskCode.Visible = !planned;
                    divAddTaskTypeTree.Style.Clear();
                    btnAddTaskToPlan.Visible = false;
                    btnAddFavToPlan.Visible = false;
                    btnUpdTask.Visible = true;
                    trProgram.Visible = chkID.Contains('-');
                    trProgramInfo.Visible = !trProgram.Visible;
                    trOwner.Visible = true;
                    lblEditTaskOComments.Visible = true;

                    hdnEditTaskID.Value = chkID;
                    hdnEditTaskInst.Value = wt.Task.Instantiated.ToString();
                    lblEditTaskTitle.Text = wt.Task.Title;
                    lblEditTaskProgram.Text = wt.Task.Program != null ? wt.Task.Program.Title : string.Empty;
                    lblEditTaskWBS.Text = wt.Task.WBS;
                    lblEditTaskAllocated.Text = wt.Task.Hours > 0 ? wt.Task.Hours.ToString() : string.Empty;
                    lblEditTaskStart.Text = wt.Task.StartDate.HasValue ? wt.Task.StartDate.Value.ToString("M/d/yy") : string.Empty;
                    lblEditTaskDue.Text = wt.Task.DueDate.HasValue ? wt.Task.DueDate.Value.ToString("M/d/yy") : string.Empty;
                    lblEditTaskStatus.Text = FormatEnum(wt.Task.Status);
                    lblEditTaskOComments.Text = wt.Task.OwnerComments != null ? wt.Task.OwnerComments.Replace("\r\n", "<br>") : string.Empty;
                    lblEditTaskComp.Visible = hdrComp.Visible;
                    addTaskAComments.Text = wt.Task.AssigneeComments;

                    if (wt.Task.Instantiated)
                    {
                        lblEditTaskType.Text = wt.Task.TaskType != null ? wt.Task.TaskType.Title : string.Empty;
                        lblEditTaskRE.Text = wt.Task.Estimate.ToString();
                        lblEditTaskExit.Text = JSComments(wt.Task.ExitCriteria, true);                        

                        if (hdrComp.Visible)
                        {
                            lblEditTaskComp.Text = wt.Task.Complexity != null ? wt.Task.Complexity.Title : string.Empty;
                        }

                        lblEditTaskType.Visible = true;
                        lblEditTaskRE.Visible = true;
                        lblEditTaskExit.Visible = true;
                        phAddTaskComp.Visible = hdrComp.Visible;

                        divAddTaskTypeTree.Style.Add(HtmlTextWriterStyle.Display, "none");
                        addTaskRE.Visible = false;
                        addTaskExit.Visible = false;
                    }
                    else
                    {
                        addTaskType.LoadTaskTypes(GetWeeklyTeam(), hdrComp.Visible);
                        phUpdatePanel.Controls.Add(new LiteralControl(
                            "<input type=\"hidden\" id=\"editTaskType\" value=\"" + wt.Task.TaskType.Id.ToString() + "\" />"));

                        addTaskExit.Text = wt.Task.ExitCriteria;

                        if (hdrComp.Visible)
                        {
                            phUpdatePanel.Controls.Add(new LiteralControl(
                                "<input type=\"hidden\" id=\"editTaskComp\" value=\"" +
                                (wt.Task.Complexity != null ? wt.Task.Complexity.Id.ToString() : "0") + "\" />"));
                        }
                        else
                        {
                            addTaskRE.Text = wt.Task.Estimate > 0 ? wt.Task.Estimate.ToString() : string.Empty;
                        }

                        if (trProgram.Visible)
                        {
                            LoadPrograms();
                            ddProgram.SelectedValue = wt.Task.Program.Id.ToString();
                        }

                        lblEditTaskType.Visible = false;
                        lblEditTaskComp.Visible = false;
                        lblEditTaskRE.Visible = false;
                        lblEditTaskExit.Visible = false;
                        phAddTaskComp.Visible = false;

                        addTaskType.Visible = true;
                        addTaskRE.Visible = true;
                        addTaskExit.Visible = true;
                        addTaskRE.Style.Clear();
                        addTaskExit.Style.Clear();
                    }

                    if (!planned)
                    {
                        addUnplanCode.LoadUnplannedCodes(GetWeeklyTeam());
                        phUpdatePanel.Controls.Add(new LiteralControl(
                            "<input type=\"hidden\" id=\"editUnplanCode\" value=\"" + wt.UnplannedCode.Id.ToString() + "\" />"));
                    }
                }
                else
                {
                    WeeklyBarrier wb = BuildWeeklyBarrier(chkID, false, false);

                    PopupForm(popupBarrier);
                    popupBarrier.Attributes.Add("title", "Edit Barrier");
                    barTask.Text = (string)ViewState["lnkTitle_" + taskID];
                    barTaskID.Value = chkID;
                    barCode.LoadBarriers(GetWeeklyTeam());
                    barComment.Text = wb.Comment;
                    barTicket.Text = wb.Ticket;
                    barTypeEfficiency.Checked = false;
                    barTypeDelay.Checked = false;
                    btnAddBarrier.Visible = false;
                    btnUpdBarrier.Visible = true;

                    if (wb.BarrierType == WeeklyBarrier.BarriersEnum.EFFICIENCY)
                    {
                        barTypeEfficiency.Checked = true;
                    }
                    else
                    {
                        barTypeDelay.Checked = true;
                    }

                    phUpdatePanel.Controls.Add(new LiteralControl(
                        "<input type=\"hidden\" id=\"editBarrier\" value=\"" + wb.Barrier.Id.ToString() + "\" />"));
                }
            }

            StoreChanges();
        }

        protected void btnUpdTask_Click(object sender, EventArgs e)
        {
            string taskID = hdnEditTaskID.Value;
            TableRow tblRow = (TableRow)myControls["row_" + taskID];
            bool planned = myPlanIDs.Contains(taskID);
            bool updated = false;
            int col = planned ? 2 : 3;
            WeeklyTask wt = BuildWeeklyTask(taskID, false);

            if (hdnEditTaskInst.Value == "False")
            {
                if (hdrComp.Visible)
                {
                    long compID = addTaskType.GetComplexityID();

                    if (compID != -1)
                    {
                        Complexity comp = BLL.TaskManager.GetInstance().GetComplexity(compID);

                        if (wt.Task.Complexity == null || wt.Task.Complexity.Title != comp.Title || wt.Task.Estimate != comp.Hours)
                        {
                            UpdateCell(tblRow.Cells[col + 3], comp.Title);
                            UpdateCell(tblRow.Cells[col + 4], comp.Hours.ToString());
                            StoreSessionValue(taskID + "_compID", compID, true);
                            updated = true;
                        }
                    }
                }
                else
                {
                    double re;

                    if (Double.TryParse(addTaskRE.Text, out re) && wt.Task.Estimate != re)
                    {
                        UpdateCell(tblRow.Cells[col + 3], re.ToString());
                        StoreSessionValue(taskID + "_re", re, true);
                        updated = true;
                    }
                }
                    
                long ttId = addTaskType.GetSelectedId();

                if (ttId != -1 && (wt.Task.TaskType == null || wt.Task.TaskType.Id != ttId))
                {
                    UpdateCell(tblRow.Cells[col], addTaskType.GetSelectedValue());
                    StoreSessionValue(taskID + "_type", ttId, true);
                    updated = true;
                }

                if (wt.Task.ExitCriteria != addTaskExit.Text)
                {
                    StoreSessionValue(taskID + "_exit", addTaskExit.Text, true);
                    updated = true;
                }

                if (taskID.Contains('-'))
                {
                    long progID = long.Parse(ddProgram.SelectedValue);

                    if (wt.Task.Program.Id != progID)
                    {
                        UpdateCell(tblRow.Cells[col+1], ddProgram.SelectedItem.Text);
                        StoreSessionValue(taskID + "_progID", progID, true);
                        updated = true;
                    }
                }
            }

            if (wt.Task.AssigneeComments != addTaskAComments.Text)
            {
                StoreSessionValue(taskID + "_comment", addTaskAComments.Text, true);
                updated = true;
            }

            if (!planned)
            {
                long codeID = addUnplanCode.GetSelectedId();

                if (wt.UnplannedCode == null || wt.UnplannedCode.Id != codeID)
                {
                    UpdateCell(tblRow.Cells[2], addUnplanCode.GetSelectedValue());
                    StoreSessionValue(taskID + "_ucode", codeID, true);
                    updated = true;
                }
            }

            if (updated)
            {
                ((HiddenField)myControls["hdnUpdate_" + taskID]).Value = "y";
            }

            StoreChanges();
            ClearRowChecks();
        }

        private void UpdateCell(TableCell cell, string text)
        {
            cell.Text = text;
            ViewState[cell.ID] = text;
        }

        protected void lnkLeave_Click(object sender, EventArgs e)
        {
            WeeklyPlan wp = new WeeklyPlan();

            for (int i = 0; i < 7; i++)
            {
                wp.LeavePlanHours.Add(i, 0);
                wp.LeaveActualHours.Add(i, 0);
            }

            bool planned = myMode.Value == "plan";
            StoreSessionValue("leavePlanned", planned, false);
            AddLeaveRow(wp, planned, true, true);
            StoreChanges();
        }

        protected void lnkAddToFavs_Click(object sender, EventArgs e)
        {
            List<Favorite> favs = new List<Favorite>();
            ArrayList list = myPlanIDs;

            for (int i = 0; i < 2; i++)
            {
                foreach (string taskID in list)
                {
                    if (!myRemoveIDs.Contains(taskID) && IsTask(taskID) &&
                        taskID[2] != '-' && ((CheckBox)myControls["chk_" + taskID]).Checked)
                    {
                        Favorite fav = new Favorite();
                        WeeklyTask wt = BuildWeeklyTask(taskID, false);
                        
                        fav.Title = wt.Task.Title;                        
                        fav.TaskType = wt.Task.TaskType;
                        fav.Program = wt.Task.Program;
                        fav.Hours = wt.Task.Hours;
                        fav.Complexity = wt.Task.Complexity;
                        fav.Estimate = wt.Task.Estimate;
                        fav.Template = false;

                        if (i == 0)
                        {
                            for (int day = 0; day < 7; day++)
                            {
                                double hours = 0;

                                if (myMode.Value == "plan")
                                {
                                    if (myControls.ContainsKey("txtHoursP_" + taskID + "_" + day.ToString()))
                                    {
                                        double.TryParse(((TextBox)myControls["txtHoursP_" + taskID + "_" + day.ToString()]).Text, out hours);
                                    }
                                }
                                else
                                {
                                    double.TryParse(ViewState["cell_" + taskID + "p_" + day.ToString()].ToString(), out hours);
                                }

                                fav.PlanHours.Add(day, hours);
                            }
                        }

                        favs.Add(fav);
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            if (favs.Count > 0)
            {
                try
                {
                    BLL.TaskManager.GetInstance().SaveFavorites(favs);
                    string msg = string.Empty;

                    for (int i = 0; i < favs.Count; i++)
                    {
                        if (i > 0 && favs.Count > 2) msg += ", ";
                        if (favs.Count > 1 && i == favs.Count - 1) msg += " and ";
                        msg += favs[i].Title;
                    }

                    Utility.DisplayInfoMessage(msg + " added to favorites.");
                    ClearRowChecks();
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage(ex.Message);
                }
            }
        }

        protected void lnkCarry_Click(object sender, EventArgs e)
        {
            ddCarryDate.Items.Clear();

            DateTime date = DateTime.Now.Date;

            while (date.DayOfWeek != DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            long profileId = long.Parse(ddProfile.SelectedValue);

            for (int i = 0; i < 4; i++)
            {
                WeeklyPlan.StatusEnum? status = BLL.TaskManager.GetInstance().GetWeeklyPlanStatus(profileId, date);

                if (!status.HasValue ||
                    status.Value == WeeklyPlan.StatusEnum.NEW ||
                    status.Value == WeeklyPlan.StatusEnum.PLAN_READY)
                {
                    ddCarryDate.Items.Add(new ListItem(
                        date.AddDays(-6).ToString("M/d/yy") + " - " + date.ToString("M/d/yy"),
                        date.ToShortDateString()));
                }

                date = date.AddDays(7);
            }

            if (ddCarryDate.Items.Count > 0)
            {
                PopupForm(popupCarry);
            }

            StoreChanges();
        }

        protected void btnCarry_Click(object sender, EventArgs e)
        {
            ArrayList list = myPlanIDs;
            myCarryIDs = new List<long>();

            for (int i = 0; i < 2; i++)
            {
                foreach (string taskID in list)
                {
                    if (!myRemoveIDs.Contains(taskID) && ((CheckBox)myControls["chk_" + taskID]).Checked &&
                        ViewState["taskID_" + taskID] != null)
                    {
                        myCarryIDs.Add((long)ViewState["taskID_" + taskID]);
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            if (myCarryIDs.Count > 0)
            {
                LoadWeeks(DateTime.Parse(ddCarryDate.SelectedValue));
                LoadData(true, false);
            }
        }

        protected void lnkRemove_Click(object sender, EventArgs e)
        {
            double hours;
            bool removed = false;
            bool prevTask = false;
            bool planned = true;
            string prefix = GetControlPrefix();
            string tbl = "P_";
            TableHeaderCell hdrToggleWE2 = null;
            ArrayList list = myPlanIDs;
            List<string> newIDs = new List<string>();

            for (int i = 0; i < 2; i++)
            {
                bool first = true;

                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid))
                    {
                        CheckBox chk = (CheckBox)myControls["chk_" + sid];
                        bool task = IsTask(sid);

                        if ((chk.Checked && (!planned || myMode.Value == "plan" || !task)) || (prevTask && !task))
                        {
                            removed = true;

                            if (task || cellToggleBarriers.Text == "Hide Barriers")
                            {
                                if (planned)
                                {
                                    hdrToggleWE.RowSpan--;
                                }
                                else
                                {
                                    ((TableHeaderCell)myControls["hdrToggleWE2"]).RowSpan--;
                                }
                            }

                            for (int day = 0; day < 7; day++)
                            {
                                hours = 0;

                                if (task)
                                {
                                    double.TryParse(HttpContext.Current.Request[prefix + "txtHours" + tbl + sid + "_" + day.ToString()], out hours);

                                    if (planned)
                                    {
                                        myPlanTotals[day] -= hours;
                                        myPlanTotals[7] -= hours;
                                    }
                                    else
                                    {
                                        myUnplanTotals[day] -= hours;
                                        myUnplanTotals[7] -= hours;
                                    }
                                }
                                else if (ViewState["cell_" + sid + "_1"].ToString() == "Efficiency")
                                {
                                    double.TryParse(HttpContext.Current.Request[prefix + "txtBarHoursE_" + sid + "_" + day.ToString()], out hours);
                                    myBarrierTotals[day] -= hours;
                                    myBarrierTotals[7] -= hours;
                                }
                                else
                                {
                                    double.TryParse(HttpContext.Current.Request[prefix + "txtBarHoursD_" + sid + "_" + day.ToString()], out hours);
                                    myDelayTotals[day] -= hours;
                                    myDelayTotals[7] -= hours;
                                }
                            }

                            TableRow row = (TableRow)myControls["row_" + sid];
                            tblPlanLog.Rows.Remove(row);
                            row.Dispose();

                            if (IsNew(sid))
                            {
                                newIDs.Add(sid);
                            }
                            else
                            {
                                myRemoveIDs.Add(sid);
                            }

                            if (i == 1 && first)
                            {
                                hdrToggleWE2 = ((TableHeaderCell)myControls["hdrToggleWE2"]);
                            }

                            if (!prevTask || task)
                            {
                                prevTask = task;
                            }
                        }
                        else
                        {
                            prevTask = false;
                        }

                        first = false;
                    }
                }

                foreach (string sid in newIDs)
                {
                    if (planned)
                    {
                        myPlanIDs.Remove(sid);
                    }
                    else
                    {
                        myUnplanIDs.Remove(sid);
                    }
                }

                if (ViewState["leavePlanned"] != null && (bool)ViewState["leavePlanned"] == planned &&
                    (!planned || myMode.Value == "plan") &&
                    myControls.ContainsKey("chk_leave") && ((CheckBox)myControls["chk_leave"]).Checked)
                {
                    TableRow tblRow = (TableRow)myControls["row_leave"];
                    tblPlanLog.Rows.Remove(tblRow);

                    if (CheckSessionKey("leavePlanned"))
                    {
                        RemoveSessionValue("leavePlanned");
                    }
                    else
                    {
                        myRemoveIDs.Add("leave");
                    }

                    ViewState.Remove("leavePlanned");
                    cellLeave.Visible = true;
                    removed = true;

                    for (int day = 0; day < 7; day++)
                    {
                        if (double.TryParse(HttpContext.Current.Request[prefix + "txtHours" + tbl + "leave_" + day.ToString()], out hours))
                        {
                            if (planned)
                            {
                                myPlanTotals[day] -= hours;
                                myPlanTotals[7] -= hours;
                            }
                            else
                            {
                                myUnplanTotals[day] -= hours;
                                myUnplanTotals[7] -= hours;
                            }
                        }
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                    planned = false;
                    tbl = "U_";
                }
            }

            if (removed)
            {
                SetTotalRows();

                if (myMode.Value == "log" && myControls.ContainsKey("rowUnplanDiv") && GetUnplanCount(false) == 0)
                {
                    TableHeaderRow div = (TableHeaderRow)myControls["rowUnplanDiv"];

                    if (ViewState["leavePlanned"] == null || (bool)ViewState["leavePlanned"])
                    {
                        tblPlanLog.Rows.Remove(div);
                        div.Dispose();
                        hdrToggleWE.RowSpan++;
                        hdrToggleWE2 = null;
                    }
                    else
                    {
                        div.Cells[0].ColumnSpan--;
                    }

                    if (hdrCode.Visible)
                    {
                        if (ViewState["leavePlanned"] != null)
                        {
                            ((TableRow)myControls["row_leave"]).Cells[1].ColumnSpan--;
                        }

                        foreach (string sid in myPlanIDs)
                        {
                            if (!myRemoveIDs.Contains(sid))
                            {
                                ((TableRow)myControls["row_" + sid]).Cells[1].ColumnSpan--;
                            }
                        }

                        ((TableHeaderRow)myControls["rowPlanTotals"]).Cells[0].ColumnSpan--;
                        hdrCode.Visible = false;
                    }
                }

                if (hdrToggleWE2 != null)
                {
                    int i = 0;

                    while (hdrToggleWE2 != null && i < myUnplanIDs.Count)
                    {
                        string sid = (string)myUnplanIDs[i];

                        if (!myRemoveIDs.Contains(sid))
                        {
                            TableRow tblRow = (TableRow)myControls["row_" + sid];
                            tblRow.Cells.AddAt(tblRow.Cells.Count - 3, hdrToggleWE2);
                            hdrToggleWE2 = null;
                        }

                        i++;
                    }

                    if (hdrToggleWE2 != null && ViewState["leavePlanned"] != null &&
                        !(bool)ViewState["leavePlanned"])
                    {
                        ((TableRow)myControls["row_leave"]).Cells.AddAt(7, hdrToggleWE2);
                    }
                }

                ClearRowChecks();
            }

            StoreChanges();
        }

        protected void lnkAlert_Click(object sender, EventArgs e)
        {
            Profile user = BLL.ProfileManager.GetInstance().GetProfile();
            string sid = HttpContext.Current.Request["alertLinkID"];
            string tid = sid;
            ViewState["alertLinkID"] = sid;
            PopupForm(popupAlert);

            if (ddProfile.SelectedValue == user.Id.ToString())
            {
                bool single = true;
                long mgrID = 0;
                Team team = BLL.AdminManager.GetInstance().GetTeam(long.Parse(ddTeam.SelectedValue), true);

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

                if (IsBarrier(sid))
                {
                    int idx;
                    ArrayList list;

                    if (myPlanIDs.Contains(sid))
                    {
                        idx = myPlanIDs.IndexOf(sid) - 1;
                        list = myPlanIDs;
                    }
                    else
                    {
                        idx = myUnplanIDs.IndexOf(sid) - 1;
                        list = myUnplanIDs;
                    }

                    while (IsBarrier((string)list[idx]))
                    {
                        idx--;
                    }

                    tid = (string)list[idx];
                }

                DTO.Task task;
                long taskID = long.Parse(tid.Substring(2));

                if (IsNew(tid))
                {
                    task = BLL.TaskManager.GetInstance().GetTask(taskID, false);
                }
                else
                {
                    task = BLL.TaskManager.GetInstance().GetTaskByWeeklyTaskID(taskID, false);
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
                cellAlertAssignee.Text = ddProfile.SelectedItem.Text;

                cellAlertALM.Visible = false;
                cellAlertMgr.Visible = false;
                cellAlertOwner.Visible = false;
            }

            cellAlertTaskTitle.InnerHtml = (string)ViewState["lnkTitle_" + tid];

            if (IsBarrier(sid))
            {
                string title = ViewState["cell_" + sid + "_0"].ToString();
                title = title.Substring(title.IndexOf("</td><td>") + 9);
                cellAlertBarrier.InnerHtml = title.Substring(0, title.Length - 18);
                rowAlertBarrier.Visible = true;
            }
            else
            {
                rowAlertBarrier.Visible = false;
            }
        }

        protected void btnCreateAlert_Click(object sender, EventArgs e)
        {
            Alert alert = new Alert();
            alert.Subject = alertSubject.Text;
            alert.Message = alertMessage.Text;
            alert.Creator = BLL.ProfileManager.GetInstance().GetProfile();
            alert.EntryDate = DateTime.Now;

            string sid = (string)ViewState["alertLinkID"];
            alert.LinkedId = long.Parse(sid.Substring(2));
            alert.LinkedType = IsTask(sid) ? Alert.AlertEnum.WEEKLY_TASK : Alert.AlertEnum.BARRIER;
            popupAlert.Visible = true;

            try
            {
                string msg = "Alert sent to ";

                if (cellAlertAssignee.Visible)
                {
                    alert.Profile = BLL.ProfileManager.GetInstance().GetProfile(long.Parse(ddProfile.SelectedValue));
                    msg += alert.Profile.DisplayName;
                    BLL.ProfileManager.GetInstance().SaveAlert(alert);                    
                }
                else
                {
                    List<Profile> sendTo = new List<Profile>();

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
                
                ViewState.Remove("alertLinkID");
                alertSubject.Text = string.Empty;
                alertMessage.Text = string.Empty;
                popupAlert.Visible = false;

                Utility.DisplayInfoMessage(msg + ".");
                ClearRowChecks();
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("Creating Alert Failed:<br>" + ex.Message);
            }

            StoreChanges();
        }

        protected void lnkTitle_Click(object sender, EventArgs e)
        {
            StoreChanges();
            string id = ((LinkButton)sender).ID.Substring(9);


            if (id.Contains("-"))
            {
                Response.Redirect("/favorites.aspx");
            }
            else
            {
                if (IsNew(id))
                {
                    id = id.Substring(2);
                }
                else
                {
                    WeeklyTask wt = BLL.TaskManager.GetInstance().GetWeeklyTask(long.Parse(id.Substring(2)));
                    id = wt.Task.Id.ToString();
                }

                Response.Redirect("/Task/ViewTask.aspx?id=" + id + "&Source=/weekly.aspx");
            }
        }

        private void StoreChanges()
        {
            RemoveSessionValue("newIDs");
            RemoveSessionValue("planCount");
            RemoveSessionValue("removeIDs");

            if (ddProfile.SelectedValue == string.Empty) return;

            int i = 0;
            bool updated = false;
            ArrayList ids = myPlanIDs;
            string hourID = "P_";
            Dictionary<int, string> newIDs = new Dictionary<int, string>();

            for (int idx = 0; idx < 2; idx++)
            {
                foreach (string sid in ids)
                {
                    if (!myRemoveIDs.Contains(sid) &&
                        (IsNew(sid) || ((HiddenField)myControls["hdnUpdate_" + sid]).Value == "y"))
                    {
                        if (IsNew(sid))
                        {
                            newIDs.Add(i, sid);
                        }

                        if (IsTask(sid))
                        {
                            StoreSessionValue(sid + "_comp", ((CheckBox)myControls["chkComp_" + sid]).Checked, false);
                        }
                        else
                        {
                            StoreSessionValue(sid + "_type", ViewState["cell_" + sid + "_1"].ToString()[0], true);
                            StoreSessionValue(sid + "_comment", ViewState["cell_" + sid + "_2"].ToString(), false);
                        }

                        List<double> hours = new List<double>();

                        for (int day = 0; day < 7; day++)
                        {
                            double hour = 0;

                            if (IsTask(sid))
                            {
                                if (myControls.ContainsKey("txtHours" + hourID + sid + "_" + day.ToString()))
                                {
                                    double.TryParse(((TextBox)myControls["txtHours" + hourID + sid + "_" + day.ToString()]).Text, out hour);
                                }
                            }
                            else
                            {
                                string btype = ViewState["cell_" + sid + "_1"].ToString() == "Efficiency" ? "E" : "D";

                                if (myControls.ContainsKey("txtBarHours" + btype + "_" + sid + "_" + day.ToString()))
                                {
                                    double.TryParse(((TextBox)myControls["txtBarHours" + btype + "_" + sid + "_" + day.ToString()]).Text, out hour);
                                }
                            }

                            hours.Add(hour);
                        }

                        StoreSessionValue(sid + "_hours", hours, false);

                        if (((HiddenField)myControls["hdnUpdate_" + sid]).Value == "y")
                        {
                            updated = true;
                        }
                    }
                    else
                    {
                        RemoveSessionValue(sid + "_hours");
                    }

                    i++;
                }

                if (myMode.Value == "plan") break;
                ids = myUnplanIDs;
                hourID = "U_";
            }

            if (ViewState["leavePlanned"] != null && ((HiddenField)myControls["hdnUpdate_leave"]).Value == "y")
            {
                List<double> hours = new List<double>();
                hourID = (bool)ViewState["leavePlanned"] ? "P_leave_" : "U_leave_";

                for (int day = 0; day < 7; day++)
                {
                    double hour = 0;

                    if (myControls.ContainsKey("txtHours" + hourID + day.ToString()))
                    {
                        double.TryParse(((TextBox)myControls["txtHours" + hourID + day.ToString()]).Text, out hour);
                    }

                    hours.Add(hour);
                }

                StoreSessionValue("leave_hours", hours, false);
                updated = true;
            }
            else
            {
                RemoveSessionValue("leavePlanned");
                RemoveSessionValue("leave_hours");
            }

            StoreSessionValue("profileID", ddProfile.SelectedValue, false);
            StoreSessionValue("week", ddWeek.SelectedValue, false);
            StoreSessionValue("modified", ViewState["modified"], false);

            if (updated || myRemoveIDs.Count > 0)
            {
                StoreSessionValue("updated", true, false);

                if (newIDs.Count > 0)
                {
                    StoreSessionValue("newIDs", newIDs, false);
                    StoreSessionValue("planCount", myPlanIDs.Count, false);
                }

                if (myRemoveIDs.Count > 0)
                {
                    StoreSessionValue("removeIDs", myRemoveIDs, false);
                }

                btnUndo.Style.Clear();
                btnSave.Style.Clear();
                updateMade.Value = "y";
            }
        }

        private void ClearRowChecks()
        {
            ArrayList list = myPlanIDs;

            for (int i = 0; i < 2; i++)
            {
                foreach (string sid in list)
                {
                    string key = "chk_" + sid;

                    if (myControls.ContainsKey(key))
                    {
                        ((CheckBox)myControls[key]).Checked = false;
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                }
            }

            if (myControls.ContainsKey("chk_leave"))
            {
                ((CheckBox)myControls["chk_leave"]).Checked = false;
            }
        }

        protected void btnUndo_Click(object sender, EventArgs e)
        {
            LoadData(true, false);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SavePlan(null);
        }

        protected void btnReady_Click(object sender, EventArgs e)
        {
            WeeklyPlan.StatusEnum? status = null;

            if (btnReady.Text == "Ready for Approval")
            {
                if (myMode.Value == "plan")
                {
                    status = WeeklyPlan.StatusEnum.PLAN_READY;
                }
                else if (myMode.Value == "log")
                {
                    status = WeeklyPlan.StatusEnum.LOG_READY;
                }
            }
            else if (btnReady.Text == "Undo Ready for Approval")
            {
                if (myMode.Value == "plan")
                {
                    status = WeeklyPlan.StatusEnum.NEW;
                }
                else if (myMode.Value == "log")
                {
                    status = WeeklyPlan.StatusEnum.PLAN_APPROVED;
                }
            }

            SavePlan(status);
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            WeeklyPlan.StatusEnum? status = null;

            if (myMode.Value == "plan")
            {
                status = WeeklyPlan.StatusEnum.PLAN_APPROVED;
            }
            else if (btnApprove.Text == "Undo Approval")
            {
                status = WeeklyPlan.StatusEnum.LOG_READY;
            }
            else
            {
                status = WeeklyPlan.StatusEnum.LOG_APPROVED;
            }

            SavePlan(status);
        }

        private void SavePlan(WeeklyPlan.StatusEnum? status)
        {
            if (sessionChanges.Value == "y" && !CheckSessionKey("sessionChanges"))
            {
                phHeader.Controls.Add(new LiteralControl(
                    "<script type=\"text/javascript\">alert('Your session has expired. All changes made have been lost.');</script>\n"));

                LoadData(true, false);
            }

            string action = " saved.";
            string lastTask = string.Empty;
            WeeklyPlan plan;
            ArrayList ids = myPlanIDs;
            List<long> removeTasks = new List<long>();
            List<long> removeBarriers = new List<long>();
            List<long> updateTasks = new List<long>();
            Dictionary<string, WeeklyTask> taskMap = new Dictionary<string, WeeklyTask>();
            bool error = false;
            bool update = false;
            bool admin = false;
            bool approvingPlan = false;

            if (weeklyPlanID.Value == "0")
            {
                plan = new WeeklyPlan();
                plan.Profile = BLL.ProfileManager.GetInstance().GetProfile(long.Parse(ddProfile.SelectedValue));
                plan.Team = BLL.AdminManager.GetInstance().GetTeamByRole(plan.Profile, "MEMBER");
                plan.WeekEnding = DateTime.Parse(ddWeek.SelectedValue);
                plan.State = WeeklyPlan.StatusEnum.NEW;
                update = true;
            }
            else
            {
                plan = BLL.TaskManager.GetInstance().GetWeeklyPlan(long.Parse(weeklyPlanID.Value), false);
            }

            if (status.HasValue)
            {
                if (status.Value == WeeklyPlan.StatusEnum.PLAN_READY)
                {
                    plan.PlanSubmitted = DateTime.Now;
                }
                else if (status.Value == WeeklyPlan.StatusEnum.LOG_READY)
                {
                    if (plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED)
                    {
                        plan.LogSubmitted = DateTime.Now;
                    }
                    else
                    {
                        plan.LogApprovedBy = null;
                    }
                }
                else if (status.Value == WeeklyPlan.StatusEnum.LOG_APPROVED ||
                    (status.Value == WeeklyPlan.StatusEnum.PLAN_APPROVED && plan.State != WeeklyPlan.StatusEnum.LOG_READY))
                {
                    if (status.Value == WeeklyPlan.StatusEnum.PLAN_APPROVED)
                    {
                        plan.PlanApprovedBy = BLL.ProfileManager.GetInstance().GetProfile();
                    }
                    else
                    {
                        plan.LogApprovedBy = BLL.ProfileManager.GetInstance().GetProfile();
                    }

                    action = " approved.";
                    admin = !cellReady.Visible;
                    approvingPlan = status.Value == WeeklyPlan.StatusEnum.PLAN_APPROVED;
                }

                plan.State = status.Value;
                update = true;
            }

            for (int idx = 0; idx < 2; idx++)
            {
                foreach (string sid in ids)
                {
                    long id = long.Parse(sid.Substring(2));

                    if (IsTask(sid))
                    {
                        lastTask = sid;
                    }

                    if (myRemoveIDs.Contains(sid))
                    {
                        if (sid[0] == 't')
                        {
                            removeTasks.Add(id);
                            update = true;
                        }
                        else if (sid[0] == 'b')
                        {
                            removeBarriers.Add(id);
                            update = true;
                        }
                    }
                    else if (IsNew(sid) || ((HiddenField)myControls["hdnUpdate_" + sid]).Value == "y")
                    {
                        if (IsTask(sid))
                        {
                            bool taskUpdated = false;
                            WeeklyTask wt = BuildWeeklyTask(sid, true, ref taskUpdated);
                            taskMap.Add(sid, wt);

                            if (wt != null)
                            {
                                plan.WeeklyTasks.Add(wt);

                                if (taskUpdated && wt.Id >= 0)
                                {
                                    updateTasks.Add(wt.Task.Id);
                                }
                            }
                            else
                            {
                                error = true;
                            }
                        }
                        else
                        {
                            WeeklyTask wt;

                            if (taskMap.ContainsKey(lastTask))
                            {
                                wt = taskMap[lastTask];
                            }
                            else
                            {
                                wt = BuildWeeklyTask(lastTask, true);
                                taskMap.Add(lastTask, wt);

                                if (wt != null)
                                {
                                    plan.WeeklyTasks.Add(wt);
                                }
                                else
                                {
                                    error = true;
                                }
                            }

                            if (wt != null)
                            {
                                WeeklyBarrier wb = BuildWeeklyBarrier(sid, false, true);

                                if (wb != null)
                                {
                                    wt.Barriers.Add(wb);
                                }
                                else
                                {
                                    error = true;
                                }
                            }
                        }

                        update = true;
                    }
                }

                if (myMode.Value == "plan") break;
                ids = myUnplanIDs;
            }

            bool updateLeave = false;
            plan.LeavePlanned = null;

            if (ViewState["leavePlanned"] != null && ((HiddenField)myControls["hdnUpdate_leave"]).Value == "y")
            {
                plan.LeavePlanned = (bool)ViewState["leavePlanned"];
                string tbl = plan.LeavePlanned.Value ? "P" : "U";

                for (int i = 0; i < 7; i++)
                {
                    double hours = 0;
                    double.TryParse(((TextBox)myControls["txtHours" + tbl + "_leave_" + i.ToString()]).Text, out hours);

                    if (myMode.Value == "plan")
                    {
                        plan.LeavePlanHours[i] = hours;
                    }
                    else if (myMode.Value == "log")
                    {
                        plan.LeaveActualHours[i] = hours;
                    }
                }

                updateLeave = true;
                update = true;
            }
            else if (myRemoveIDs.Contains("leave"))
            {
                updateLeave = true;
                update = true;
            }

            if (!error && update)
            {
                try
                {
                    BLL.TaskManager.GetInstance().SaveWeeklyPlan(plan, approvingPlan, updateLeave, updateTasks, removeTasks, removeBarriers);

                    if (admin && ddProfile.SelectedIndex < ddProfile.Items.Count - 1)
                    {
                        ddProfile.SelectedIndex++;
                    }

                    string msg;

                    if (myMode.Value == "view")
                    {
                        msg = "Log unapproved.";
                    }
                    else
                    {
                        msg = myMode.Value[0].ToString().ToUpper() + myMode.Value.Substring(1) + action;
                    }

                    Utility.DisplayInfoMessage(msg);
                    LoadData(true, false);

                    return;
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("Save Failed:<br>" + ex.Message);
                }
            }

            StoreChanges();
        }

        private WeeklyTask BuildWeeklyTask(
            string taskID,
            bool loadHours
        )
        {
            bool taskUpdated = false;
            return BuildWeeklyTask(taskID, loadHours, ref taskUpdated);
        }

        private WeeklyTask BuildWeeklyTask(
            string taskID,
            bool loadHours,
            ref bool taskUpdated
        )
        {
            WeeklyTask wt;
            double hours;
            long id = long.Parse(taskID.Substring(2));

            if (IsNew(taskID))
            {
                if (id < 0)
                {
                    wt = BLL.TaskManager.GetInstance().GetTaskTemplate(-id);

                    if (wt != null)
                    {
                        wt.Id = id;

                        if (CheckSessionKey(taskID + "_progID"))
                        {
                            wt.Task.Program = BLL.AdminManager.GetInstance().GetProgram((long)GetSessionObject(taskID + "_progID"));
                        }
                    }
                }
                else
                {
                    wt = CreateWeeklyTask(id);
                }
            }
            else
            {
                wt = BLL.TaskManager.GetInstance().GetWeeklyTask(id);
            }

            if (wt == null || wt.Task == null)
            {
                return wt;
            }

            taskUpdated = SetWeeklyTask(ref wt, taskID, taskUpdated);

            if (loadHours)
            {
                wt.PlanDayComplete = -1;
                wt.ActualDayComplete = -1;

                bool comp = ((CheckBox)myControls["chkComp_" + taskID]).Checked;
                string hourID = myPlanIDs.Contains(taskID) ? "P_" : "U_";

                for (int i = 0; i < 7; i++)
                {
                    if (myControls.ContainsKey("txtHours" + hourID + taskID + "_" + i.ToString()))
                    {
                        hours = 0;
                        double.TryParse(((TextBox)myControls["txtHours" + hourID + taskID + "_" + i.ToString()]).Text, out hours);

                        if (myMode.Value == "plan")
                        {
                            wt.PlanHours[i] = hours;
                        }
                        else if (myMode.Value == "log")
                        {
                            wt.ActualHours[i] = hours;
                        }

                        if (comp && hours > 0)
                        {
                            if (myMode.Value == "plan")
                            {
                                wt.PlanDayComplete = i;
                            }
                            else if (myMode.Value == "log")
                            {
                                wt.ActualDayComplete = i;
                            }
                        }
                    }
                }
            }

            return wt;
        }

        private bool SetWeeklyTask(ref WeeklyTask wt, string taskID, bool fullLoad)
        {
            bool taskUpdated = false;

            if (wt == null)
            {
                wt = BuildWeeklyTask(taskID, false, ref fullLoad);
                return false;
            }

            if (!wt.Task.Instantiated)
            {
                if (CheckSessionKey(taskID + "_type"))
                {
                    if (fullLoad)
                    {
                        wt.Task.TaskType = BLL.AdminManager.GetInstance().GetTaskType((long)GetSessionObject(taskID + "_type"));
                    }
                    else
                    {
                        if (wt.Task.TaskType == null)
                        {
                            wt.Task.TaskType = new TaskType();
                        }

                        wt.Task.TaskType.Id = (long)GetSessionObject(taskID + "_type");
                    }

                    taskUpdated = true;
                }

                if (CheckSessionKey(taskID + "_compID"))
                {
                    wt.Task.Complexity = BLL.AdminManager.GetInstance().GetComplexityCode((long)GetSessionObject(taskID + "_compID"));
                    wt.Task.Estimate = wt.Task.Complexity.Hours;
                    taskUpdated = true;
                }
                else if (CheckSessionKey(taskID + "_re"))
                {
                    wt.Task.Estimate = (double)GetSessionObject(taskID + "_re");
                    taskUpdated = true;
                }

                if (CheckSessionKey(taskID + "_exit"))
                {
                    wt.Task.ExitCriteria = GetSessionValue(taskID + "_exit");
                    taskUpdated = true;
                }
            }

            if (CheckSessionKey(taskID + "_comment"))
            {
                wt.Task.AssigneeComments = GetSessionValue(taskID + "_comment");
                taskUpdated = true;
            }

            if (myPlanIDs.Contains(taskID))
            {
                wt.UnplannedCode = null;
            }
            else if (CheckSessionKey(taskID + "_ucode"))
            {
                if (fullLoad)
                {
                    wt.UnplannedCode = BLL.AdminManager.GetInstance().GetUnplannedCode((long)GetSessionObject(taskID + "_ucode"));
                }
                else
                {
                    if (wt.UnplannedCode == null)
                    {
                        wt.UnplannedCode = new UnplannedCode();
                    }

                    wt.UnplannedCode.Id = (long)GetSessionObject(taskID + "_ucode");
                }
            }

            return taskUpdated;
        }

        private WeeklyBarrier BuildWeeklyBarrier(string barID, bool fromSession, bool loadHours)
        {
            WeeklyBarrier wb;

            if (IsNew(barID))
            {
                wb = new WeeklyBarrier();
                wb.Barrier = new Barrier();
            }
            else
            {
                wb = BLL.TaskManager.GetInstance().GetWeeklyBarrier(long.Parse(barID.Substring(2)));
            }

            if (wb == null || wb.Barrier == null)
            {
                return wb;
            }

            SetWeeklyBarrier(ref wb, barID, fromSession);

            if (loadHours)
            {
                string btype = wb.BarrierType == WeeklyBarrier.BarriersEnum.EFFICIENCY ? "E_" : "D_";
                double hours;

                for (int i = 0; i < 7; i++)
                {
                    if (myControls.ContainsKey("txtBarHours" + btype + barID + "_" + i.ToString()))
                    {
                        hours = 0;
                        double.TryParse(((TextBox)myControls["txtBarHours" + btype + barID + "_" + i.ToString()]).Text, out hours);
                        wb.Hours.Add(i, hours);
                    }
                }
            }

            return wb;
        }

        private void SetWeeklyBarrier(ref WeeklyBarrier wb, string barID, bool fromSession)
        {
            if (wb == null)
            {
                wb = BuildWeeklyBarrier(barID, fromSession, false);
            }
            else
            {
                if (fromSession)
                {
                    if (CheckSessionKey(barID + "_id"))
                    {
                        long id = (long)GetSessionObject(barID + "_id");

                        if (wb.Barrier.Id != id)
                        {
                            wb.Barrier = BLL.AdminManager.GetInstance().GetBarrier(id);
                        }
                    }

                    wb.Comment = GetSessionValue(barID + "_comment");
                    wb.BarrierType = GetSessionValue(barID + "_type") == "D" ?
                        WeeklyBarrier.BarriersEnum.DELAY : WeeklyBarrier.BarriersEnum.EFFICIENCY;
                }
                else
                {
                    if (CheckSessionKey(barID + "_id"))
                    {
                        wb.Barrier.Id = (long)GetSessionObject(barID + "_id");
                    }

                    wb.Comment = ViewState["cell_" + barID + "_2"].ToString();
                    wb.BarrierType = ViewState["cell_" + barID + "_1"].ToString() == "Efficiency" ?
                        WeeklyBarrier.BarriersEnum.EFFICIENCY : WeeklyBarrier.BarriersEnum.DELAY;
                }

                if (CheckSessionKey(barID + "_ticket"))
                {
                    wb.Ticket = GetSessionValue(barID + "_ticket");
                }
            }
        }

        protected void ddDirectorate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddDirectorate.Items[0].Value == string.Empty)
            {
                ddDirectorate.Items.RemoveAt(0);
            }

            LoadTeams(-1, -1);
            LoadData(true, false);
        }

        protected void ddTeam_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddTeam.Items[0].Value == string.Empty)
            {
                ddTeam.Items.RemoveAt(0);
            }

            hdrComp.Visible = BLL.AdminManager.GetInstance().GetTeam(long.Parse(ddTeam.SelectedValue), false).ComplexityBased;
            LoadUsers(-1);
            LoadData(true, false);
        }

        protected void ddProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddProfile.Items[0].Value == string.Empty)
            {
                ddProfile.Items.RemoveAt(0);
            }

            profileIndex.Value = ddProfile.SelectedIndex.ToString();
            LoadData(true, false);
        }

        private void ShowWE()
        {
            lnkToggleWE.Text = "-";
            hdrSat.Style.Clear();
            hdrSun.Style.Clear();

            if (myControls.ContainsKey("lnkToggleWE2"))
            {
                ((LinkButton)myControls["lnkToggleWE2"]).Text = lnkToggleWE.Text;
            }

            for (int i = 10; i < 14; i++)
            {
                hdrPlan2.Cells[i].Style.Clear();
            }

            TableRow tblRow = (TableHeaderRow)myControls["rowPlanTotals"];
            int col = myMode.Value == "plan" ? 6 : 11;

            tblRow.Cells[col].Style.Clear();
            tblRow.Cells[col + 1].Style.Clear();

            if (col == 11)
            {
                tblRow.Cells[col + 2].Style.Clear();
                tblRow.Cells[col + 3].Style.Clear();
            }

            ArrayList list = myPlanIDs;
            col += 7;
            if (myMode.Value == "view") col--;
            if (hdrComp.Visible) col++;

            for (int i = 0; i < 2; i++)
            {
                bool first = true;

                foreach (string sid in list)
                {
                    if (!myRemoveIDs.Contains(sid))
                    {
                        tblRow = (TableRow)myControls["row_" + sid];

                        if (IsTask(sid))
                        {
                            tblRow.Cells[col].Style.Clear();
                            tblRow.Cells[col + 1].Style.Clear();

                            if (col > 16)
                            {
                                tblRow.Cells[col + 2].Style.Clear();
                                tblRow.Cells[col + 3].Style.Clear();
                            }
                        }
                        else
                        {
                            tblRow.Cells[9].Style.Clear();
                            tblRow.Cells[10].Style.Clear();
                        }

                        if (i == 1 && first)
                        {
                            col--;
                        }

                        first = false;
                    }
                }

                if (myMode.Value == "plan")
                {
                    i = 2;
                }
                else if (i == 0)
                {
                    list = myUnplanIDs;
                    col = myMode.Value == "view" ? 14 : 15;
                    if (hdrComp.Visible) col++;

                    if (myControls.ContainsKey("cellUnplanDiv"))
                    {
                        ((TableHeaderCell)myControls["cellUnplanDiv"]).ColumnSpan += 4;
                    }
                }
            }

            if (ViewState["leavePlanned"] != null)
            {
                tblRow = (TableRow)myControls["row_leave"];
                col = myMode.Value == "plan" || !(bool)ViewState["leavePlanned"] ? 7 : 12;
                if (col == 7 && GetUnplanCount(false) == 0) col++;

                tblRow.Cells[col].Style.Clear();
                tblRow.Cells[col + 1].Style.Clear();

                if (col == 12)
                {
                    tblRow.Cells[col + 2].Style.Clear();
                    tblRow.Cells[col + 3].Style.Clear();
                }
            }
        }

        private void CheckWE(TableCell cell, int col, double hours)
        {
            if (col > 4 && hdrToggleWE.Visible)
            {
                if (!myShowWE)
                {
                    if (hours > 0)
                    {
                        myShowWE = true;
                    }
                    else
                    {
                        cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                }
                else if (hours > 0 && hdrToggleWE.Visible)
                {
                    hdrToggleWE.Visible = false;

                    if (myControls.ContainsKey("hdrToggleWE2"))
                    {
                        ((TableHeaderCell)myControls["hdrToggleWE2"]).Visible = false;
                    }
                }

                cell.CssClass += "WE";
            }
        }

        private void StoreSessionValue(string key, object value, bool required)
        {
            Dictionary<string, object> vals;

            if (Session["weekly_values"] == null)
            {
                vals = new Dictionary<string, object>();
                Session["weekly_values"] = vals;
            }
            else
            {
                vals = (Dictionary<string, object>)Session["weekly_values"];
            }

            vals[key] = value;

            if (required)
            {
                sessionChanges.Value = "y";
                StoreSessionValue("sessionChanges", true, false);
            }
        }

        private string GetSessionValue(string key)
        {
            if (Session["weekly_values"] != null)
            {
                Dictionary<string, object> vals = (Dictionary<string, object>)Session["weekly_values"];

                if (vals.ContainsKey(key))
                {
                    return vals[key].ToString();
                }
            }

            return string.Empty;
        }

        private object GetSessionObject(string key)
        {
            if (Session["weekly_values"] != null)
            {
                Dictionary<string, object> vals = (Dictionary<string, object>)Session["weekly_values"];

                if (vals.ContainsKey(key))
                {
                    return vals[key];
                }
            }

            return null;
        }

        private void RemoveSessionValue(string key)
        {
            if (Session["weekly_values"] != null)
            {
                Dictionary<string, object> vals = (Dictionary<string, object>)Session["weekly_values"];

                if (vals.ContainsKey(key))
                {
                    vals.Remove(key);
                }
            }
        }

        private bool CheckSessionKey(string key)
        {
            if (Session["weekly_values"] != null)
            {
                Dictionary<string, object> vals = (Dictionary<string, object>)Session["weekly_values"];
                return vals.ContainsKey(key);
            }

            return false;
        }

        private string GetControlPrefix()
        {
            if (IsPostBack && myControlPrefix == string.Empty)
            {
                int i = 5;
                int count = HttpContext.Current.Request.Params.Keys.Count;

                while (i < count && myControlPrefix == string.Empty)
                {
                    if (HttpContext.Current.Request.Params.Keys[i].Contains("myMode"))
                    {
                        string prefix = HttpContext.Current.Request.Params.Keys[i];
                        myControlPrefix = prefix.Substring(0, prefix.Length - 6);
                    }

                    i++;
                }
            }

            return myControlPrefix;
        }

        private string FormatEnum(object enumObj)
        {
            string str = enumObj.ToString();

            if (str == "EFFICIENCY")
            {
                return "Efficiency";
            }
            else if (str == "OBE")
            {
                return str;
            }

            return str[0] + str.Substring(1).ToLower();
        }

        private string FormatHours(double hours)
        {
            if (hours > 0)
            {
                return hours.ToString();
            }

            return string.Empty;
        }

        private bool IsTask(string id)
        {
            return (id[0] == 't' || id[0] == 'n');
        }

        private bool IsBarrier(string id)
        {
            return (id[0] == 'b' || id[0] == 'a');
        }

        private bool IsNew(string id)
        {
            return (id[0] == 'n' || id[0] == 'a');
        }
    }
}
