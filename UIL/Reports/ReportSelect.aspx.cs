using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace WALT.UIL.Reports
{
    public partial class ReportSelect : System.Web.UI.Page
    {
        List<ListItem> _reportGroupPublic;
        List<DTO.ReportGroup> _reportGroupPrivate;
        List<string> _profiles;
        List<DTO.Team> _teams;
        List<ListItem> _reports;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.REPORT_MANAGE))
            {
                if (!IsPostBack)
                {
                    LoadData();
                    phPublicChk.Visible = BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN);
                }
                else
                {
                    _reports = (List<ListItem>)HttpContext.Current.Session["report_names"];
                    _reportGroupPublic = (List<ListItem>)HttpContext.Current.Session["report_group_public"];
                    _reportGroupPrivate = (List<DTO.ReportGroup>)HttpContext.Current.Session["report_group_priv"];
                    _profiles = (List<string>)HttpContext.Current.Session["report_profiles"];
                    _teams = (List<DTO.Team>)HttpContext.Current.Session["report_teams"];
                }

                GroupFilterPanel.Visible = false;
                SetVisibility();

                Panel2.Visible = true;
            }
            else
            {
                Label6.Text = "You do not have permission to generate reports";
                Panel2.Visible = false;
            }

            lblTitleReq.Visible = false;
        }

        void SetVisibility()
        {
            Label1.Visible = false;

            if ((ddlReportType.SelectedValue.StartsWith("PARETO"))
                || (ddlReportType.SelectedValue.StartsWith("SUMMARY")))
            {
                Tr1.Visible = true;
                Tr2.Visible = false;
            }
            else if (ddlReportType.SelectedValue == DTO.Report.TypeEnum.TEAM_INFO.ToString())
            {
                Tr1.Visible = false;
                Tr2.Visible = false;
            }
            else
            {
                Tr1.Visible = false;
                Tr2.Visible = true;

                // %BASE / %GOAL only visible for Operating Reports.
                if (ddlReportType.SelectedValue.StartsWith("OPERATING"))
                {
                    tblBaseGoal.Visible = true;
                    lblBaseGoal.Visible = (ddlReportType.SelectedValue == DTO.Report.TypeEnum.OPERATING_SUMMARY.ToString());
                    phBaseGoal.Visible = lblBaseGoal.Visible;
                }
                else
                {
                    tblBaseGoal.Visible = false;
                }
            }

            SetUserFilterSelectionVisibility();
        }

        void ClearDialog()
        {
            selProfiles.Value = string.Empty;
            selTeams.Value = string.Empty;
            selDirs.Value = string.Empty;
            selGroups.Value = string.Empty;
            txtGroupName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtGroupProfile.Text = string.Empty;
            ddGroupTeams.SelectedValue = string.Empty;
            ddGroupDirs.SelectedValue = string.Empty;
            ddGroupGroups.SelectedValue = string.Empty;
            lbGroupProfiles.Items.Clear();
            lbGroupTeams.Items.Clear();
            lbGroupDirs.Items.Clear();
            lbGroupGroups.Items.Clear();
            CheckBox1.Checked = false;
        }

        private void DataBindReports(bool existingOnly)
        {
            ddlExistingReports.DataSource = _reports;
            ddlExistingReports.DataTextField = "Text";
            ddlExistingReports.DataValueField = "Value";
            ddlExistingReports.DataBind();

            if (!existingOnly)
            {
                ddlReportType.Items.Clear();

                foreach (DTO.Report.TypeEnum type in Enum.GetValues(typeof(DTO.Report.TypeEnum)).Cast<DTO.Report.TypeEnum>())
                {
                    ddlReportType.Items.Add(new ListItem(DTO.Report.GetReportName(type), type.ToString()));
                }

                rbUserFilterType.DataSource = BLL.ReportManager.GetInstance().GetUserGroupTypes();
                rbUserFilterType.DataBind();
            }
        }

        private void DataBindUserFilterTypes()
        {
            ddlPublicFilter.DataSource = _reportGroupPublic;
            ddlPublicFilter.DataTextField = "Text";
            ddlPublicFilter.DataValueField = "Value";
            ddlPublicFilter.DataBind();

            privateFilter.DataSource = _reportGroupPrivate;
            privateFilter.DataBind();
        }

        private void LoadData()
        {
            // Set _reports and Session["report_names"]
            LoadReports(false);

            _profiles = BLL.ProfileManager.GetInstance().GetProfileDisplayNameList();
            _teams = BLL.AdminManager.GetInstance().GetAllTeams();

            HttpContext.Current.Session["report_profiles"] = _profiles;
            HttpContext.Current.Session["report_teams"] = _teams;

            LoadDropDowns();
        }

        /// <summary>
        /// Called to populate _reports.
        /// </summary>
        private void LoadReports(bool existingOnly)
        {
            bool first = true;
            bool showHeadings = cbShowPublic.Checked && cbShowPrivate.Checked;
            bool publicReports = cbShowPublic.Checked;
            
            _reports = new List<ListItem>();
            _reports.Add(new ListItem());

            List<DTO.Report> reports = BLL.ReportManager.GetInstance().GetReportList(false);

            foreach(DTO.Report r in reports)
            {
                if (publicReports && !r.Public && cbShowPrivate.Checked)
                {
                    publicReports = false;
                    first = true;
                }

                if (publicReports == r.Public)
                {
                    if (first && showHeadings)
                    {
                        if (!publicReports)
                        {
                            if (_reports.Count == 1)
                            {
                                _reports.Add(new ListItem("--- Public Reports ---"));
                            }

                            _reports.Add(new ListItem());
                        }                        

                        _reports.Add(new ListItem(publicReports ?
                            "--- Public Reports ---" : "--- Your Private Reports ---"));

                        first = false;
                    }

                    _reports.Add(new ListItem(r.Title, r.Id.ToString()));
                }
            }

            // Put the reports on the session and databind them.
            HttpContext.Current.Session["report_names"] = _reports;
            DataBindReports(existingOnly);
        }

        private void LoadUserFilterTypeData()
        {
            switch (rbUserFilterType.SelectedValue)
            {
                case "Public Group": // Create _reportGroupPublic
                    LoadPublicGroups();
                    break;

                case "Activity Log Team": // Create _reportGroupActivityLogTeam
                case "Directorate": // Create _reportGroupDirectorate
                case "Individual Contributor":
                    break;

                case "Private Group": // Create _reportGroupPrivate
                default: // Visibility will force Private to be default, so load now.
                    _reportGroupPrivate = BLL.ReportManager.GetInstance().GetPrivateReportGroups();

                    // Add create group selection, and blank header.
                    DTO.ReportGroup g = new DTO.ReportGroup();
                    g.Name = "-- Create a New Group --";
                    _reportGroupPrivate.Insert(0, g);

                    g = new DTO.ReportGroup();
                    g.Name = "";
                    _reportGroupPrivate.Insert(0, g);
                    break;
            }

            HttpContext.Current.Session["report_group_public"] = _reportGroupPublic;
            HttpContext.Current.Session["report_group_priv"] = _reportGroupPrivate;

            DataBindUserFilterTypes();
        }

        /// <summary>
        /// Load the Group Filter editor dialog drop downs.
        /// Puts both public and private groups in the groups drop down.
        /// </summary>
        private void LoadDropDowns()
        {
            LoadDropDowns(false);
        }

        /// <summary>
        /// Load the Group Filter editor dialog drop downs.
        /// Puts only public groups in the groups drop down when isPublic is true.
        /// </summary>
        /// <param name="isPublic">Set true when public report to hide private groups in private drop down.</param>
        private void LoadDropDowns(bool isPublic)
        {
            ddlActivityLogTeam.Items.Clear();
            ddGroupTeams.Items.Clear();
            directorate.Items.Clear();
            ddGroupDirs.Items.Clear();
            ddGroupGroups.Items.Clear();

            Dictionary<long, string> dirMap = new Dictionary<long, string>();

            foreach (DTO.Team team in _teams)
            {
                dirMap.Add(team.Id, team.Name);
            }

            List<string> selTeams = new List<string>();
            Dictionary<string, long> teamIDs = new Dictionary<string, long>();

            foreach (DTO.Team team in _teams)
            {
                if (team.Type == DTO.Team.TypeEnum.TEAM && team.ParentId > 0)
                {
                    string teamName = dirMap[team.ParentId] + ": " + team.Name;
                    selTeams.Add(teamName);
                    teamIDs.Add(teamName, team.Id);
                }
            }

            selTeams.Sort();
            ddlActivityLogTeam.Items.Add(new ListItem());
            ddGroupTeams.Items.Add(new ListItem());

            foreach (string teamName in selTeams)
            {
                ListItem item = new ListItem(teamName, teamIDs[teamName].ToString());
                ddlActivityLogTeam.Items.Add(item);
                ddGroupTeams.Items.Add(item);
            }

            List<string> dirNames = new List<string>();
            Dictionary<string, long> dirIDs = new Dictionary<string, long>();

            foreach (DTO.Team team in _teams)
            {
                if (team.Type == DTO.Team.TypeEnum.DIRECTORATE)
                {
                    dirNames.Add(team.Name);
                    dirIDs.Add(team.Name, team.Id);
                }
            }

            dirNames.Sort();
            directorate.Items.Add(new ListItem());
            ddGroupDirs.Items.Add(new ListItem());
            ddGroupDirs.Items.Add(new ListItem("All", "-1"));

            foreach (string dirName in dirNames)
            {
                ListItem item = new ListItem(dirName, dirIDs[dirName].ToString());
                directorate.Items.Add(item);
                ddGroupDirs.Items.Add(item);
            }

            /* Populate Groups drop down list */
            ddGroupGroups.Items.Add(new ListItem()); // Blank entry at top.

            // Only make the private reports available if this is a private report
            if (false == isPublic)
            {
                if (_reportGroupPrivate == null)
                {
                    _reportGroupPrivate = BLL.ReportManager.GetInstance().GetPrivateReportGroups();
                }

                foreach (DTO.ReportGroup itemPrivateGroup in _reportGroupPrivate)
                {
                    if (itemPrivateGroup.Id != 0)
                    {
                        ddGroupGroups.Items.Add(new ListItem(itemPrivateGroup.Name + " (Private)", itemPrivateGroup.Id.ToString()));
                    }
                }
            }

            if (_reportGroupPublic == null)
            {
                // Do not add 'Create new Group' or blank top item
                LoadPublicGroups(false, false);
            }

            if (_reportGroupPublic.Count() > 0)
            {
                foreach (ListItem itemPublicGroup in _reportGroupPublic)
                {
                    if (itemPublicGroup.Text != string.Empty
                        && !itemPublicGroup.Text.Contains("-- Create a New Group --"))
                    {
                        ddGroupGroups.Items.Add(new ListItem(itemPublicGroup.Text + " (Public)", itemPublicGroup.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Get data for and populate _reportGroupPublic.
        /// </summary>
        private void LoadPublicGroups(bool addCreateNewOption, bool addBlankItemTop)
        {
            // Add these reports to _reportGroupPublic for the session.
            _reportGroupPublic = new List<ListItem>();

            if (true == addBlankItemTop)
            {
                _reportGroupPublic.Add(new ListItem());
            }

            if ((true == addCreateNewOption)
                && (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN)))
            {
                _reportGroupPublic.Add(new ListItem("-- Create a New Group --", "-1"));
            }

            List<DTO.ReportGroup> reportGroupPublic = BLL.ReportManager.GetInstance().GetPublicReportGroups();

            foreach (DTO.ReportGroup rg in reportGroupPublic)
            {
                ListItem item = new ListItem();
                item.Text = rg.Name;
                item.Value = rg.Id.ToString();

                _reportGroupPublic.Add(item);
            }
        }

        /// <summary>
        /// Get data for and populate _reportGroupPublic.
        /// </summary>
        private void LoadPublicGroups()
        {
            LoadPublicGroups(true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        void SaveReportGroupPublic(bool as_new)
        {
            try
            {
                DTO.ReportGroup g;
                bool modify = false;
                long result = 0;

                // Convert the selected value from string to long, and 
                // attempt to get the Report with that Id.
                if (!as_new && ddlPublicFilter.SelectedIndex > 1 &&
                    long.TryParse(ddlPublicFilter.SelectedValue, out result))
                {
                    g = BLL.ReportManager.GetInstance().GetReportGroup(result);
                    modify = true;
                }
                else
                {
                    g = new DTO.ReportGroup();
                }

                Dictionary<string, DTO.Team> teamMap = new Dictionary<string, DTO.Team>();

                foreach (DTO.Team team in _teams)
                {
                    teamMap.Add(team.Id.ToString(), team);
                }

                Dictionary<string, DTO.ReportGroup> reportGroupMap = new Dictionary<string, DTO.ReportGroup>();

                foreach (DTO.ReportGroup rg in _reportGroupPrivate)
                {
                    if (0 != rg.Id) // Don't store invalid groups such as blank entry or "create new."
                    {
                        reportGroupMap[rg.Id.ToString()] = rg;
                    }
                }

                foreach (ListItem rg in _reportGroupPublic)
                {
                    if ("0" != rg.Value) // Don't store invalid groups such as blank entry or "create new."
                    {
                        DTO.ReportGroup reportGroup = CreateReportGroupPublic(rg.Value);
                        reportGroupMap.Add(rg.Value, reportGroup);
                    }
                }

                lbGroupProfiles.Items.Clear();
                lbGroupTeams.Items.Clear();
                lbGroupDirs.Items.Clear();
                lbGroupGroups.Items.Clear();

                if (selProfiles.Value != string.Empty)
                {
                    string[] split = selProfiles.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string username in split)
                    {
                        lbGroupProfiles.Items.Add(username);
                    }
                }

                if (selTeams.Value != string.Empty)
                {
                    string[] split = selTeams.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sid in split)
                    {
                        DTO.Team team = teamMap[sid];
                        lbGroupTeams.Items.Add(new ListItem(team.Name, sid));
                    }
                }

                if (selDirs.Value != string.Empty)
                {
                    string[] split = selDirs.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sid in split)
                    {
                        DTO.Team team = teamMap[sid];
                        lbGroupDirs.Items.Add(new ListItem(team.Name, sid));
                    }
                }

                if (selGroups.Value != string.Empty)
                {
                    string[] split = selGroups.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sid in split)
                    {
                        DTO.ReportGroup rg = reportGroupMap[sid];
                        lbGroupGroups.Items.Add(new ListItem(rg.Name, rg.Id.ToString()));
                    }
                }

                // Alert the original owner their public report group has been changed.
                if (modify && g.Owner != null &&
                    g.Owner.Id != BLL.ProfileManager.GetInstance().GetProfile().Id)
                {
                    GenerateAlertForModifiedReportGroup(g);
                }
                else
                {
                    g.Owner = BLL.ProfileManager.GetInstance().GetProfile();
                }
                
                g.Name = txtGroupName.Text;
                g.Public = true;
                g.Description = txtDescription.Text;

                g.Profiles.Clear();
                g.Teams.Clear();
                g.Directorates.Clear();
                g.Groups.Clear();

                foreach (ListItem item in lbGroupProfiles.Items)
                {
                    g.Profiles.Add(BLL.ProfileManager.GetInstance().GetProfileByDisplayName(item.Text, false));
                }

                foreach (ListItem item in lbGroupTeams.Items)
                {
                    g.Teams.Add(teamMap[item.Value]);
                }

                foreach (ListItem item in lbGroupDirs.Items)
                {
                    DTO.Team team = teamMap[item.Value];
                    DTO.Directorate dir = new DTO.Directorate();
                    dir.Id = team.Id;
                    dir.Name = team.Name;
                    g.Directorates.Add(dir);
                }

                foreach (ListItem item in lbGroupGroups.Items)
                {
                    g.Groups.Add(reportGroupMap[item.Value]);
                }
                
                BLL.ReportManager.GetInstance().SaveReportGroup(g);

                rbUserFilterType_SelectedIndexChanged(null, null);

                ddlPublicFilter.SelectedValue = g.Id.ToString();
                ddlPublicFilter_SelectedIndexChanged(null, null);

                Utility.DisplayInfoMessage("Public Group \"" + g.Name + "\" Saved.");
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }
        }

        /// <summary>
        /// Compare the given group to the current data
        /// and generate a report if they differ.
        /// </summary>
        private void GenerateAlertForModifiedReportGroup(DTO.ReportGroup g)
        {
            DTO.Profile profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();

            if (g != null && g.Owner.Id != profile.Id)
            {
                DTO.Alert alert = new DTO.Alert();

                StringBuilder sb = new StringBuilder();
                sb.Append("Your Public User Group Filter has been modified.");
                alert.Subject = sb.ToString();

                sb = new StringBuilder();
                sb.Append("Your Public Group Filter \"");
                sb.Append(g.Name);
                sb.Append("\" has been modified by ");
                sb.Append(profile.DisplayName);
                sb.Append(". ");

                if (0 != String.Compare(g.Name, txtGroupName.Text))
                {
                    sb.Append("It has been renamed to \"");
                    sb.Append(txtGroupName.Text);
                    sb.Append(".\" ");
                }

                /* If the description has changed, notify the user. */
                if ((!(String.IsNullOrEmpty(g.Description)) && (txtDescription.Text != string.Empty))
                    && (0 != String.Compare(g.Description, txtDescription.Text)))
                {
                    sb.Append("The description has been changed to \"");
                    sb.Append(txtDescription.Text);
                    sb.Append(".\" ");
                }

                /* If ICs have been added, notify the user. */
                List<DTO.Profile> addedIcList = new List<DTO.Profile>();

                for (int i = 0; i < lbGroupProfiles.Items.Count; i++)
                {
                    var added = g.Profiles.Select(x => x.DisplayName).Contains(lbGroupProfiles.Items[i].Text);

                    if (!added)
                    {
                        addedIcList.Add(BLL.ProfileManager.GetInstance().GetProfileByDisplayName(
                            lbGroupProfiles.Items[i].Text, false));
                    }
                }

                if (addedIcList.Count() > 0)
                {
                    if (addedIcList.Count() < 2)
                    {
                        sb.Append("The following Individual Contributor has been added: ");
                    }
                    else
                    {
                        sb.Append("The following Individual Contributors have been added: ");
                    }

                    int count = 0;
                    foreach (DTO.Profile added in addedIcList)
                    {
                        count++;

                        sb.Append(added.DisplayName);

                        if ((count > 0) && (count < addedIcList.Count()))
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.Append(". ");
                }

                /* If ICs have been removed, notify the user. */
                List<DTO.Profile> removedIcList = new List<DTO.Profile>();

                foreach (DTO.Profile p in g.Profiles)
                {
                    ListItem removed = lbGroupProfiles.Items.FindByText(p.DisplayName);

                    if (removed == null)
                    {
                        removedIcList.Add(BLL.ProfileManager.GetInstance().GetProfile(p.Id, false));
                    }
                }

                if (removedIcList.Count() > 0)
                {
                    if (removedIcList.Count() < 2)
                    {
                        sb.Append("The following Individual Contributor has been removed: ");
                    }
                    else
                    {
                        sb.Append("The following Individual Contributors have been removed: ");
                    }

                    int count = 0;
                    foreach (DTO.Profile added in removedIcList)
                    {
                        count++;

                        sb.Append(added.DisplayName);

                        if ((count > 0) && (count < removedIcList.Count()))
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.Append(". ");
                }

                /* If teams have been added, notify the user. */
                List<DTO.Team> addedTeamList = new List<DTO.Team>();

                for (int i = 0; i < lbGroupTeams.Items.Count; i++)
                {
                    var added = g.Teams.Select(x => x.Name).Contains(lbGroupTeams.Items[i].Text);

                    if (!added)
                    {
                        addedTeamList.Add(BLL.AdminManager.GetInstance().GetTeam(lbGroupTeams.Items[i].Text, false));
                    }
                }

                if (addedTeamList.Count() > 0)
                {
                    if (addedTeamList.Count() < 2)
                    {
                        sb.Append("The following Activity Log Team has been added: ");
                    }
                    else
                    {
                        sb.Append("The following Activity Log Teams have been added: ");
                    }

                    int count = 0;
                    foreach (DTO.Team added in addedTeamList)
                    {
                        count++;

                        sb.Append(added.Name);

                        if ((count > 0) && (count < addedTeamList.Count()))
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.Append(". ");
                }

                /* If teams have been removed, notify the user. */
                List<DTO.Team> removedTeamList = new List<DTO.Team>();

                foreach (DTO.Team t in g.Teams)
                {
                    ListItem removed = lbGroupTeams.Items.FindByText(t.Name);

                    if (removed == null)
                    {
                        removedTeamList.Add(BLL.AdminManager.GetInstance().GetTeam(t.Id, false));
                    }
                }

                if (removedTeamList.Count() > 0)
                {
                    if (removedTeamList.Count() < 2)
                    {
                        sb.Append("The following Activity Log Team has been removed: ");
                    }
                    else
                    {
                        sb.Append("The following Activity Log Teams have been removed: ");
                    }

                    int count = 0;
                    foreach (DTO.Team added in removedTeamList)
                    {
                        count++;

                        sb.Append(added.Name);

                        if ((count > 0) && (count < removedTeamList.Count()))
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.Append(". ");
                }

                /* If directorates have been added, notify the user. */
                List<DTO.Directorate> addedDirectorateList = new List<DTO.Directorate>();

                for (int i = 0; i < lbGroupDirs.Items.Count; i++)
                {
                    var added = g.Directorates.Select(x => x.Name).Contains(lbGroupDirs.Items[i].Text);

                    if (!added)
                    {
                        addedDirectorateList.Add(BLL.AdminManager.GetInstance().GetDirectorate(
                            lbGroupDirs.Items[i].Text, false));
                    }
                }

                if (addedDirectorateList.Count() > 0)
                {
                    if (addedDirectorateList.Count() < 2)
                    {
                        sb.Append("The following Directorate has been added: ");
                    }
                    else
                    {
                        sb.Append("The following Directorates have been added: ");
                    }

                    int count = 0;
                    foreach (DTO.Directorate added in addedDirectorateList)
                    {
                        count++;

                        sb.Append(added.Name);

                        if ((count > 0) && (count < addedDirectorateList.Count()))
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.Append(". ");
                }

                /* If teams have been removed, notify the user. */
                List<DTO.Directorate> removedDirectorateList = new List<DTO.Directorate>();

                foreach (DTO.Directorate d in g.Directorates)
                {
                    ListItem removed = lbGroupDirs.Items.FindByText(d.Name);

                    if (removed == null)
                    {
                        removedDirectorateList.Add(d);
                    }
                }

                if (removedDirectorateList.Count() > 0)
                {
                    if (removedDirectorateList.Count() < 2)
                    {
                        sb.Append("The following Directorate has been removed: ");
                    }
                    else
                    {
                        sb.Append("The following Directorates have been removed: ");
                    }

                    int count = 0;
                    foreach (DTO.Directorate added in removedDirectorateList)
                    {
                        count++;

                        sb.Append(added.Name);

                        if ((count > 0) && (count < removedDirectorateList.Count()))
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.Append(". ");
                }

                alert.Profile = g.Owner;
                alert.Message = sb.ToString();
                alert.Creator = profile;
                alert.EntryDate = DateTime.Now;

                WALT.BLL.ProfileManager.GetInstance().SaveAlert(alert);
            }
        }

        void SaveReportGroupPrivate(bool as_new)
        {
            try
            {
                Dictionary<string, DTO.Team> teamMap = new Dictionary<string, DTO.Team>();

                foreach (DTO.Team team in _teams)
                {
                    teamMap.Add(team.Id.ToString(), team);
                }

                Dictionary<string, DTO.ReportGroup> reportGroupMap = new Dictionary<string, DTO.ReportGroup>();

                foreach (DTO.ReportGroup rg in _reportGroupPrivate)
                {
                    if (rg.Id != 0)
                    {
                        reportGroupMap.Add(rg.Id.ToString(), rg);
                    }
                }

                foreach (ListItem rg in _reportGroupPublic)
                {
                    DTO.ReportGroup reportGroup = CreateReportGroupPublic(rg.Value);

                    if (reportGroup != null)
                    {
                        reportGroupMap.Add(rg.Value, reportGroup);
                    }
                }

                DTO.ReportGroup g;

                if (!as_new &&
                    privateFilter.SelectedIndex > 1)
                {
                    g = _reportGroupPrivate[privateFilter.SelectedIndex];
                }
                else
                {
                    g = new WALT.DTO.ReportGroup();
                }

                g.Name = txtGroupName.Text;
                g.Description = txtDescription.Text;
                g.Public = false;
                g.Owner = BLL.ProfileManager.GetInstance().GetProfile();

                g.Profiles.Clear();
                g.Teams.Clear();
                g.Directorates.Clear();
                g.Groups.Clear();

                if (selProfiles.Value != string.Empty)
                {
                    string[] split = selProfiles.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string username in split)
                    {
                        g.Profiles.Add(BLL.ProfileManager.GetInstance().GetProfileByDisplayName(username, false));
                    }
                }

                if (selTeams.Value != string.Empty)
                {
                    string[] split = selTeams.Value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sid in split)
                    {
                        g.Teams.Add(teamMap[sid]);
                    }
                }

                if (selDirs.Value != string.Empty)
                {
                    string[] split = selDirs.Value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sid in split)
                    {
                        DTO.Team team = teamMap[sid];
                        DTO.Directorate dir = new DTO.Directorate();
                        dir.Id = team.Id;
                        dir.Name = team.Name;                            
                        g.Directorates.Add(dir);
                    }
                }

                if (selGroups.Value != string.Empty)
                {
                    string[] split = selGroups.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sid in split)
                    {
                        g.Groups.Add(reportGroupMap[sid]);
                    }
                }

                BLL.ReportManager.GetInstance().SaveReportGroup(g);

                rbUserFilterType_SelectedIndexChanged(null, null);

                privateFilter.SelectedValue = txtGroupName.Text;
                privateFilter_SelectedIndexChanged(null, null);

                Utility.DisplayInfoMessage("Private Group \"" + g.Name + "\" Saved.");
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }
        }

        DTO.ReportGroup SaveReportGroupActivityLogTeam(bool isPublic)
        {
            try
            {
                DTO.ReportGroup g = new WALT.DTO.ReportGroup();
                g.Name = "__" + ddlActivityLogTeam.SelectedItem.Text;
                g.Public = isPublic; // Needs to be true, or saving an ALT to public report fails.
                g.Owner = BLL.ProfileManager.GetInstance().GetProfile();

                foreach (DTO.Team team in _teams)
                {
                    if (team.Id.ToString() == ddlActivityLogTeam.SelectedValue)
                    {
                        g.Teams.Add(team);
                        break;
                    }
                }

                BLL.ReportManager.GetInstance().SaveReportGroup(g);
                return g;
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }

            return null;
        }

        DTO.ReportGroup SaveReportGroupDirectorate(bool isPublic)
        {
            try
            {
                DTO.ReportGroup g = new WALT.DTO.ReportGroup();
                g.Name = "__" + directorate.SelectedItem.Text;
                g.Public = isPublic;
                g.Owner = BLL.ProfileManager.GetInstance().GetProfile();

                foreach (DTO.Team team in _teams)
                {
                    if (team.Id.ToString() == directorate.SelectedValue)
                    {
                        DTO.Directorate dir = new DTO.Directorate();
                        dir.Id = team.Id;
                        dir.Name = team.Name;
                        g.Directorates.Add(dir);
                        break;
                    }
                }

                BLL.ReportManager.GetInstance().SaveReportGroup(g);
                return g;
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }

            return null;
        }

        private DTO.ReportGroup CreateReportGroupIndividualContributor()
        {
            try
            {
                DTO.ReportGroup g = new WALT.DTO.ReportGroup();

                StringBuilder sb = new StringBuilder();
                sb.Append("__");
                sb.Append(tbDisplayName.Text.ToString());
                g.Name = sb.ToString();

                g.Public = false;
                g.Owner = BLL.ProfileManager.GetInstance().GetProfile();

                g.Profiles.Clear();

                // Note that the Display Name may not be unique.
                // Therefore, get the profile by UserName, which must
                // be unique due to Active Directory.
                g.Profiles.Add(BLL.ProfileManager.GetInstance().GetProfile(tbUserName.Text, false));

                g.Teams.Clear();
                g.Directorates.Clear();

                return g;
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }

            return null;
        }

        DTO.ReportGroup SaveReportGroupIndividualContributor(bool isPublic)
        {
            try
            {
                DTO.ReportGroup g = CreateReportGroupIndividualContributor();

                g.Public = isPublic; // Needs to be true, or saving an IC to public report fails.

                if (g != null)
                {
                    BLL.ReportManager.GetInstance().SaveReportGroup(g);
                }

                return g;
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }

            return null;
        }

        /// <summary>
        /// Creates a new DTO.ReportGroup containing one team.
        /// The owner of the DTO.ReportGroup will be the current user.
        /// The group Name will have __ appended to it to signify this
        ///   is a special case, ALT User Filter Type.
        /// </summary>
        /// <param name="id">The name of the Team id of type TEAM to add to a new group.</param>
        /// <returns></returns>
        private DTO.ReportGroup CreateReportGroupALT(string id)
        {
            try
            {
                DTO.ReportGroup g = new WALT.DTO.ReportGroup();
                g.Public = true;
                g.Owner = BLL.ProfileManager.GetInstance().GetProfile();

                foreach (DTO.Team team in _teams)
                {
                    if (team.Id.ToString() == id)
                    {
                        g.Name = "__" + team.Name;
                        g.Teams.Add(team);
                        break;
                    }
                }
                
                return g;
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }

            return null;
        }

        /// <summary>
        /// Creates a new DTO.ReportGroup containing one Directorate.
        /// The owner of the DTO.ReportGroup will be the current user.
        /// The group Name will have __ appended to it to signify this
        ///   is a special case, Directorate User Filter Type.
        /// </summary>
        /// <param name="name">The name of the Directorate (DTO.Team of type DIRECTORATE) to add to a new group.</param>
        /// <returns></returns>
        private DTO.ReportGroup CreateReportGroupDirectorate(string id)
        {
            try
            {
                DTO.ReportGroup g = new WALT.DTO.ReportGroup();
                g.Public = true;
                g.Owner = BLL.ProfileManager.GetInstance().GetProfile();

                foreach (DTO.Team team in _teams)
                {
                    if (team.Id.ToString() == id)
                    {
                        g.Name = "__" + team.Name;
                        DTO.Directorate dir = new DTO.Directorate();
                        dir.Id = team.Id;
                        dir.Name = team.Name;
                        g.Directorates.Add(dir);
                        break;
                    }
                }

                return g;
            }
            catch (Exception e)
            {
                Label1.Text = e.Message;
                Label1.Visible = true;
            }

            return null;
        }

        /// <summary>
        /// Creates a new DTO.ReportGroup containing the given report group id.
        /// </summary>
        /// <returns></returns>
        private DTO.ReportGroup CreateReportGroupPublic(string id)
        {
            DTO.ReportGroup g = new DTO.ReportGroup();
            long result = 0;

            // Convert id from string to long.
            if (long.TryParse(id, out result))
            {
                // If we were sucessful in getting the Id, 
                // get the DTO.Report object with the Id.
                g = BLL.ReportManager.GetInstance().GetReportGroup(result);
            }

            return g;
        }

        /// <summary>
        /// Called when 'Exisiting Reports' drop down changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlExistingReports_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DTO.Report r = new DTO.Report();
                long result = 0;

                // Convert _report[].Value from string to long.
                if (long.TryParse(_reports[ddlExistingReports.SelectedIndex].Value, out result))
                {
                    // If we were sucessful in getting the Id, 
                    // get the DTO.Report object with the Id.
                    r = BLL.ReportManager.GetInstance().GetReport(result);
                }
                else if (sender != null)
                {
                    Clear();
                    return;
                }

                if (sender != null &&
                    (r.Title == "--- Your Private Reports ---" || r.Title == "--- Public Reports ---"))
                {
                    Clear();
                    return;
                }

                if (r.Owner != null)
                {
                    lExistingReportLastSavedBy.Text = "Created by " + r.Owner.DisplayName;
                }
                else
                {
                    lExistingReportLastSavedBy.Text = string.Empty;
                }

                tbReportTitle.Text = r.Title;

                if (phPublicChk.Visible)
                {
                    chkPublic.Checked = r.Public;
                    btnDeleteExisitingReport.Visible = true;
                    btnExistingReportSave.Visible = true;
                }
                else
                {
                    btnDeleteExisitingReport.Visible = !r.Public;
                    btnExistingReportSave.Visible = !r.Public;
                }

                description.Text = r.Description;
                ddlReportType.SelectedValue = r.Type.ToString();
                ddlReportType_SelectedIndexChanged(null, null);
                tbDateRangeFrom.Text = r.FromDate.HasValue == false ? "" : r.FromDate.Value.ToShortDateString();
                tbDateRangeTo.Text = r.ToDate.HasValue == false ? "" : r.ToDate.Value.ToShortDateString();
                weekEnding.Text = r.ToDate.HasValue == false ? "" : r.ToDate.Value.ToShortDateString();

                if (r.Type.ToString().StartsWith("OPERATING"))
                {
                    percentBase.Text = Convert.ToString(r.PercentBase);
                    percentGoal.Text = Convert.ToString(r.PercentGoal);

                    if (r.Type == DTO.Report.TypeEnum.OPERATING_SUMMARY && !string.IsNullOrEmpty(r.Attributes))
                    {
                        string[] split = r.Attributes.Split(',');
                        txtBarrierBase.Text = split[0];
                        txtBarrierGoal.Text = split[1];
                        txtAdhereBase.Text = split[2];
                        txtAdhereGoal.Text = split[3];
                        txtAttainBase.Text = split[4];
                        txtAttainGoal.Text = split[5];
                        txtProdBase.Text = split[6];
                        txtProdGoal.Text = split[7];
                        txtUnplanBase.Text = split[8];
                        txtUnplanGoal.Text = split[9];
                    }
                }

                // Set RadioButton1 selection based on what is saved in the group
                if (r.Group != null)
                {
                    if ((r.Group.Profiles.Count() == 1)
                        && (r.Group.Teams.Count() == 0)
                        && (r.Group.Directorates.Count() == 0))
                    {
                        rbUserFilterType.SelectedValue = "Individual Contributor";
                        rbUserFilterType_SelectedIndexChanged(null, null);

                        tbUserName.Text = r.Group.Profiles.First().Username;
                        tbDisplayName.Text = r.Group.Profiles.First().DisplayName;
                    }
                    else if ((r.Group.Profiles.Count() == 0)
                        && (r.Group.Teams.Count() == 1)
                        && (r.Group.Directorates.Count() == 0))
                    {
                        rbUserFilterType.SelectedValue = "Activity Log Team";
                        rbUserFilterType_SelectedIndexChanged(null, null);
                        ddlActivityLogTeam.SelectedValue = r.Group != null ? r.Group.Teams[0].Id.ToString() : "";
                    }
                    else if ((r.Group.Profiles.Count() == 0)
                        && (r.Group.Teams.Count() == 0)
                        && (r.Group.Directorates.Count() == 1))
                    {
                        rbUserFilterType.SelectedValue = "Directorate";
                        rbUserFilterType_SelectedIndexChanged(null, null);
                        directorate.SelectedValue = r.Group != null ? r.Group.Directorates[0].Id.ToString() : "";
                    }
                    else
                    {
                        if (r.Group.Public == true)
                        {
                            rbUserFilterType.SelectedValue = "Public Group";
                            rbUserFilterType_SelectedIndexChanged(null, null);
                            ddlPublicFilter.SelectedValue = r.Group != null ? r.Group.Id.ToString() : "";
                            ddlPublicFilter_SelectedIndexChanged(null, null);
                        }
                        else
                        {
                            rbUserFilterType.SelectedValue = "Private Group";
                            rbUserFilterType_SelectedIndexChanged(null, null);
                            privateFilter.SelectedValue = r.Group != null ? r.Group.Name : "";
                            privateFilter_SelectedIndexChanged(null, null);
                        }
                    }
                }
                else
                {
                    tbDisplayName.Text = "";
                }

                SetVisibility();
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        /// <summary>
        /// Called when the Panel2 Clear button is pressed.
        /// </summary>
        void Clear()
        {
            ddlExistingReports.SelectedIndex = 0;
            ddlExistingReports_SelectedIndexChanged(null, null);

            lExistingReportLastSavedBy.Text = string.Empty;
            lPublicFilterLastSavedBy.Text = string.Empty;

            tbDisplayName.Text = string.Empty;

            ddlActivityLogTeam.SelectedValue = string.Empty;
            ddlActivityLogTeam_SelectedIndexChanged(null, null);

            directorate.SelectedValue = string.Empty;
            directorate_SelectedIndexChanged(null, null);

            ddlPublicFilter.SelectedValue = string.Empty;
            ddlPublicFilter_SelectedIndexChanged(null, null);

            privateFilter.SelectedValue = string.Empty;
            privateFilter_SelectedIndexChanged(null, null);
        }

        /// <summary>
        /// Called when the Public Filter drop down selection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlPublicFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPublicFilter.SelectedValue == "-1")
            {
                ClearDialog();
                CheckBox1.Checked = true;
                CheckBox1.Enabled = false;

                // Pass 'true' to not load private reports, as this is a public group.
                LoadDropDowns(true);

                ShowDialog();
                ddlPublicFilter.Text = "";
                lPublicFilterLastSavedBy.Text = string.Empty;
            }
            else if (ddlPublicFilter.SelectedIndex > 0)
            {
                DTO.ReportGroup g = new DTO.ReportGroup();
                long result = 0;

                // Convert _report[].Value from string to long.
                if (long.TryParse(ddlPublicFilter.SelectedValue, out result))
                {
                    // If we were sucessful in getting the Id, 
                    // get the DTO.Report object with the Id.
                    g = BLL.ReportManager.GetInstance().GetReportGroup(result);
                }

                if ((g.Owner != null && g.Owner.Id == BLL.ReportManager.GetInstance().GetProfile().Id) ||
                    BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN))
                {
                    Button3.Visible = true; // Modify button
                    Button4.Visible = true; // Delete button
                }
                else
                {
                    Button3.Visible = false; // Modify button
                    Button4.Visible = false; // Delete button
                }

                if (g.Owner != null)
                {
                    lPublicFilterLastSavedBy.Text = "Created by " + g.Owner.DisplayName;
                }
                else
                {
                    lPublicFilterLastSavedBy.Text = string.Empty;
                }
            }
            else
            {
                Button3.Visible = false; // Modify button
                Button4.Visible = false; // Delete button
                lPublicFilterLastSavedBy.Text = string.Empty;
            }
        }

        /// <summary>
        /// Group Filter Modify button callback for Public groups.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button3_Click(object sender, EventArgs e)
        {
            ClearDialog();

            DTO.ReportGroup g = new DTO.ReportGroup();
            long result = 0;

            // Convert _report[].Value from string to long.
            if (true == long.TryParse(ddlPublicFilter.SelectedValue, out result))
            {
                // If we were sucessful in getting the Id, 
                // get the DTO.Report object with the Id.
                g = BLL.ReportManager.GetInstance().GetReportGroup(result);
            }

            txtGroupName.Text = g.Name;

            foreach (DTO.Profile p in g.Profiles)
            {
                lbGroupProfiles.Items.Add(p.DisplayName);
            }

            foreach (DTO.Team t in g.Teams)
            {
                lbGroupTeams.Items.Add(new ListItem(GetTeamNameWithDirectorate(t), t.Id.ToString()));
            }

            foreach (DTO.Directorate d in g.Directorates)
            {
                lbGroupDirs.Items.Add(new ListItem(d.Name, d.Id.ToString()));
            }

            // Load the drop down menus.
            // Pass 'true' to load only public groups to the groups drop down.
            LoadDropDowns(true);

            // Remove this group from the drop down list to stop the user
            // from adding the group to itself.
            try
            {
                ddGroupGroups.Items.Remove(ddGroupGroups.Items.FindByValue(g.Id.ToString()));
            }
            catch
            {
                // Empty
            }

            // Put the groups contained in this group into the list box.
            foreach (DTO.ReportGroup rg in g.Groups)
            {
                lbGroupGroups.Items.Add(new ListItem(rg.Public == true ? rg.Name + " (Public)" : rg.Name + " (Private)", rg.Id.ToString()));

                // Remove the items placed in the list box from the drop down.
                try
                {
                    ddGroupGroups.Items.Remove(ddGroupGroups.Items.FindByValue(rg.Id.ToString()));
                }
                catch
                {
                    // Empty
                }
            }

            CheckBox1.Checked = g.Public;

            ShowDialog(BLL.ReportManager.GetInstance().IsReportGroupInAnotherGroup(g.Id));
        }

        private string GetTeamNameWithDirectorate(DTO.Team team)
        {
            foreach (DTO.Team t in _teams)
            {
                if (t.Id == team.ParentId)
                {
                    return t.Name + ": " + team.Name;
                }
            }

            return team.Name;
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            // Delete group filter

            DTO.ReportGroup g = new DTO.ReportGroup();
            long result = 0;

            // Convert _report[].Value from string to long.
            if (true == long.TryParse(ddlPublicFilter.SelectedValue, out result))
            {
                // If we were sucessful in getting the Id, 
                // get the DTO.Report object with the Id.
                g = BLL.ReportManager.GetInstance().GetReportGroup(result);
            }

            if (g.Id > 0)
            {
                try
                {
                    BLL.ReportManager.GetInstance().DeleteFilterGroup(g);
                    rbUserFilterType_SelectedIndexChanged(null, null);
                    ddlPublicFilter_SelectedIndexChanged(null, null);
                    Utility.DisplayInfoMessage("Public Group \"" + g.Name + "\" Deleted.");
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }
        }

        /// <summary>
        /// GroupFilterPanel 'save' button callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button5_Click(object sender, EventArgs e)
        {
            if (rbUserFilterType.SelectedValue == "Public Group")
            {
                SaveReportGroupPublic(false);
            }
            else if (rbUserFilterType.SelectedValue == "Private Group")
            {
                SaveReportGroupPrivate(false);
            }
        }

        /// <summary>
        /// GroupFilterPanel 'Save As' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button19_Click(object sender, EventArgs e)
        {
            switch (rbUserFilterType.SelectedValue)
            {
                case "Public Group":

                    SaveReportGroupPublic(true);

                    try
                    {
                        ddlPublicFilter.SelectedValue = txtGroupName.Text;
                        ddlPublicFilter_SelectedIndexChanged(null, null);
                    }
                    catch { }
                    break;
                case "Private Group":

                    SaveReportGroupPrivate(true);

                    try
                    {
                        privateFilter.SelectedValue = txtGroupName.Text;
                        privateFilter_SelectedIndexChanged(null, null);
                    }
                    catch { }
                    break;
                case "Individual Contributor":
                case "Activity Log Team":
                case "Directorate":
                default:
                    break;
            }
        }

        /// <summary>
        /// Group Filter Modify button callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button17_Click(object sender, EventArgs e)
        {
            ClearDialog();

            DTO.ReportGroup g = _reportGroupPrivate[privateFilter.SelectedIndex];

            txtGroupName.Text = g.Name;

            txtDescription.Text = g.Description;

            foreach (DTO.Profile p in g.Profiles)
            {
                lbGroupProfiles.Items.Add(p.DisplayName);
            }

            foreach (DTO.Team t in g.Teams)
            {
                lbGroupTeams.Items.Add(new ListItem(GetTeamNameWithDirectorate(t), t.Id.ToString()));
            }

            foreach (DTO.Directorate d in g.Directorates)
            {
                lbGroupDirs.Items.Add(new ListItem(d.Name, d.Id.ToString()));
            }

            // Load the drop down menus.
            // Pass 'false' to load the private groups, as this is a private report.
            LoadDropDowns(false);

            // Remove this group from the drop down list to stop the user
            // from adding the group to itself.
            try
            {
                ddGroupGroups.Items.Remove(ddGroupGroups.Items.FindByValue(g.Id.ToString()));
            }
            catch
            {
                // Empty
            }

            // Put the groups contained in this group into the list box.
            foreach (DTO.ReportGroup rg in g.Groups)
            {
                lbGroupGroups.Items.Add(new ListItem(rg.Public == true ? rg.Name + " (Public)" : rg.Name + " (Private)", rg.Id.ToString()));
                
                // Remove the items placed in the list box from the drop down.
                try
                {
                    ddGroupGroups.Items.Remove(ddGroupGroups.Items.FindByValue(rg.Id.ToString()));
                }
                catch
                {
                    // Empty
                }
            }

            CheckBox1.Checked = g.Public;

            ShowDialog(BLL.ReportManager.GetInstance().IsReportGroupInAnotherGroup(g.Id));
        }

        /// <summary>
        /// Deletes a private group filter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button18_Click(object sender, EventArgs e)
        {
            // Delete private group filter

            DTO.ReportGroup g = _reportGroupPrivate[privateFilter.SelectedIndex];

            if (g.Id > 0)
            {
                try
                {
                    BLL.ReportManager.GetInstance().DeleteFilterGroup(g);
                    rbUserFilterType_SelectedIndexChanged(null, null);
                    Utility.DisplayInfoMessage("Private Group \"" + g.Name + "\" Deleted.");
                }
                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        protected void btnExistingReportSave_Click(object sender, EventArgs e)
        {
            SaveReport(false);
        }

        protected void btnExistingReportSaveAs_Click(object sender, EventArgs e)
        {
            SaveReport(true);
        }

        protected void btnDeleteExisitingReport_Click(object sender, EventArgs e)
        {
            DeleteReport();
        }

        /// <summary>
        /// Displays the Group Filter Editor dialog, GroupFilterPanel.
        /// </summary>
        void ShowDialog()
        {
            ShowDialog(false);
        }

        /// <summary>
        /// Displays the Group Filter Editor dialog, GroupFilterPanel.
        /// 
        /// If the group being modified is in another group, hide the groups
        /// area by setting hideGroups to true.
        /// </summary>
        /// <param name="hideGroups">Set true to hide groups dropdown, listbox, and buttons.</param>
        void ShowDialog(bool hideGroups)
        {
            string script = "<script>$(function() { var profiles = [";

            for (int i = 0; i < _profiles.Count; i++)
            {
                script += "\"" + _profiles[i] + "\"";

                if (i < _profiles.Count - 1)
                {
                    script += ",";
                }
            }

            script += @"];
        $('.txtGroupProfile').autocomplete({
            source: profiles,
            select: function(event, ui) { GroupAddProfile(this, ui.item.value); }
        });
    });
</script>";

            Label2.Text = script;
            Panel2.Controls.Add(new LiteralControl("<input type=\"hidden\" id=\"showdialog\" name=\"showdialog\" />"));

            // Hide the groups drop down, listbox, add button, and remove
            // button if this group is in another group.
            // This prevents groups in groups.
            if (hideGroups)
            {
                tdGroupGroupsDd.Visible = false;
                tdGroupGroupsAddBtn.Visible = false;
                tdGroupGroupsLb.Visible = false;
                tdGroupGroupsRemoveBtn.Visible = false;

                tdGroupGroupsDdHidden.Visible = true;
                tdGroupGroupsLbHidden1.Visible = true;
                tdGroupGroupsLbHidden2.Visible = true;
                tdGroupGroupsRemoveBtnHidden.Visible = true;
            }
            else
            {
                tdGroupGroupsDd.Visible = true;
                tdGroupGroupsAddBtn.Visible = true;
                tdGroupGroupsLb.Visible = true;
                tdGroupGroupsRemoveBtn.Visible = true;

                tdGroupGroupsDdHidden.Visible = false;
                tdGroupGroupsLbHidden1.Visible = false;
                tdGroupGroupsLbHidden2.Visible = false;
                tdGroupGroupsRemoveBtnHidden.Visible = false;
            }

            GroupFilterPanel.Visible = true;
        }

        /// <summary>
        /// Called to delete the selected Exisiting Report.
        /// The report deleted is the one selected in the 
        /// 'exisitingReports' drop down list.
        /// </summary>
        void DeleteReport()
        {
            try
            {
                DTO.Report r = new DTO.Report();
                long result = 0;
                if (true == long.TryParse(ddlExistingReports.SelectedValue, out result))
                {
                    r = BLL.ReportManager.GetInstance().GetReport(result);

                    BLL.ReportManager.GetInstance().DeleteReport(r);
                    LoadData();
                    ddlExistingReports.SelectedIndex = 0;
                    ddlExistingReports_SelectedIndexChanged(null, null);

                    Utility.DisplayInfoMessage("Report \"" + r.Title + "\" Deleted.");
                }
            }
            catch (Exception e)
            {
                Utility.DisplayException(e);
            }
        }

        /// <summary>
        /// Called when Deleting an exisiting report to notify
        /// the owner that someone else has deleted their public report.
        /// </summary>
        //private void GenerateAlertForDeleteReport(string reportTitle, DTO.Profile reportOwner)
        //{
        //    DTO.Alert alert = new DTO.Alert();

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("Your Public Exisiting Report has been deleted.");
        //    alert.Subject = sb.ToString();

        //    sb = new StringBuilder();
        //    sb.Append("Exisiting report \"");
        //    sb.Append(reportTitle);
        //    sb.Append("\" was deleted by ");
        //    sb.Append(WALT.BLL.ProfileManager.GetInstance().GetProfile().DisplayName);
        //    sb.Append(".");
        //    alert.Message = sb.ToString();

        //    alert.Profile = reportOwner;
        //    alert.Creator = WALT.BLL.ProfileManager.GetInstance().GetProfile();
        //    alert.EntryDate = DateTime.Now;

        //    WALT.BLL.ProfileManager.GetInstance().SaveAlert(alert);
        //}

        void GenerateReport()
        {
            try
            {
                DTO.Report r = new DTO.Report();
                long result = 0;

                if (long.TryParse(ddlExistingReports.SelectedValue, out result))
                {
                    r.Id = result;
                }
                else
                {
                    r.Id = -1;
                }

                // If the user changed any data, ensure this is captured
                // prior to putting the report object onto the session.
                r.Title = tbReportTitle.Text;

                if (phPublicChk.Visible)
                {
                    r.Public = chkPublic.Checked;
                }
                else
                {
                    r.Public = false;
                }

                r.Description = description.Text;
                r.Type = (DTO.Report.TypeEnum)Enum.Parse(typeof(DTO.Report.TypeEnum), ddlReportType.SelectedValue, true);

                if (weeklyOrMonthly.SelectedValue == "Monthly")
                {
                    r.IsMonthly = true;
                }
                else
                {
                    r.IsMonthly = false;
                }

                if (ddlReportType.SelectedValue.StartsWith("OPERATING"))
                {
                    r.PercentBase = Convert.ToInt32(percentBase.Text);
                    r.PercentGoal = Convert.ToInt32(percentGoal.Text);

                    if (ddlReportType.SelectedValue == DTO.Report.TypeEnum.OPERATING_SUMMARY.ToString())
                    {
                        r.LoadBase = r.PercentBase;
                        r.LoadGoal = r.PercentGoal;
                        r.BarrierBase = Convert.ToInt32(txtBarrierBase.Text);
                        r.BarrierGoal = Convert.ToInt32(txtBarrierGoal.Text);
                        r.AdherenceBase = Convert.ToInt32(txtAdhereBase.Text);
                        r.AdherenceGoal = Convert.ToInt32(txtAdhereGoal.Text);
                        r.AttainmentBase = Convert.ToInt32(txtAttainBase.Text);
                        r.AttainmentGoal = Convert.ToInt32(txtAttainGoal.Text);
                        r.ProductivityBase = Convert.ToInt32(txtProdBase.Text);
                        r.ProductivityGoal = Convert.ToInt32(txtProdGoal.Text);
                        r.UnplannedBase = Convert.ToInt32(txtUnplanBase.Text);
                        r.UnplannedGoal = Convert.ToInt32(txtUnplanGoal.Text);
                    }
                }

                // Try to get the entered date(s).
                DateTime returnValue;
                r.FromDate = null;
                r.ToDate = null;

                if (ddlReportType.SelectedValue.StartsWith("PARETO") ||
                    ddlReportType.SelectedValue.StartsWith("SUMMARY"))
                {
                    if (DateTime.TryParse(tbDateRangeFrom.Text, out returnValue))
                    {
                        r.FromDate = returnValue;
                    }

                    if (DateTime.TryParse(tbDateRangeTo.Text, out returnValue))
                    {
                        r.ToDate = returnValue;

                        if (!r.FromDate.HasValue)
                        {
                            r.FromDate = returnValue;
                        }
                    }
                    else if (r.FromDate.HasValue)
                    {
                        r.ToDate = r.FromDate;
                    }
                }
                else if (DateTime.TryParse(weekEnding.Text, out returnValue))
                {
                    r.FromDate = returnValue;
                    r.ToDate = returnValue;
                }

                try
                {
                    switch (rbUserFilterType.SelectedValue)
                    {
                        case "Individual Contributor":
                            r.Group = CreateReportGroupIndividualContributor();
                            break;
                        case "Activity Log Team":
                            r.Group = CreateReportGroupALT(ddlActivityLogTeam.SelectedValue);
                            break;
                        case "Directorate":
                            r.Group = CreateReportGroupDirectorate(directorate.SelectedValue);
                            break;
                        case "Public Group":
                            r.Group = CreateReportGroupPublic(ddlPublicFilter.SelectedValue);
                            break;
                        case "Private Group":
                            r.Group = _reportGroupPrivate[privateFilter.SelectedIndex];
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("ReportSelect.aspx.cs: GenerateReport() -\"r.Group\" not set. Not generating report.");
                    return;
                }


                // For Pareto and Summary reports, 
                //  the From Date or the To Date can be blank. Handle this.
                ReportDateCheck(r);

                HttpContext.Current.Session["report_generate"] = r;
                AjaxControlToolkit.ToolkitScriptManager.RegisterClientScriptBlock(this.upanelReportSelect, this.upanelReportSelect.GetType(), "AnyScriptNameYouLike", "window.open('ReportView.aspx');", true);
            }
            catch
            {
                Console.WriteLine("ReportSelect.aspx.cs: GenerateReport() - _reports index invalid. r not set. Not generating report.");
                return;
            }
        }

        /// <summary>
        /// Called when Save or Save As is selected on ReportSelect.aspx
        /// to save the report.
        /// </summary>
        /// <param name="as_new">When True, create a copy of the exisiting report with any user changes.</param>
        void SaveReport(bool as_new)
        {
            if (tbReportTitle.Text == string.Empty)
            {
                lblTitleReq.Visible = true;
                return;
            }

            try
            {
                DTO.Report report;
                bool setOwner = true;
                long result = 0;

                // Conert the selected value from string to long, and 
                // attempt to get the Report with that Id.
                if (!as_new && long.TryParse(ddlExistingReports.SelectedValue, out result))
                {
                   // report = BLL.ReportManager.GetInstance().GetReport(result, false);
                    report = BLL.ReportManager.GetInstance().GetReport(result, true);

                    // Alert the original owner their report has been changed.
                    if (report.Public && report.Owner != null && report.Owner.Id != BLL.ProfileManager.GetInstance().GetProfile().Id)
                    {
                        GenerateAlertForModifiedReport(report);
                        setOwner = false;
                    }
                }
                else
                {
                    report = new DTO.Report();
                }

                report.Title = tbReportTitle.Text;

                if (phPublicChk.Visible)
                {
                    report.Public = chkPublic.Checked;
                }
                else
                {
                    report.Public = false;
                }

                report.Description = description.Text;
                report.Type = (DTO.Report.TypeEnum)Enum.Parse(typeof(DTO.Report.TypeEnum), ddlReportType.SelectedValue, true);

                if (setOwner)
                {
                    report.Owner = BLL.ReportManager.GetInstance().GetProfile();
                }

                report.ToDate = null;
                report.FromDate = null;

                if (ddlReportType.SelectedValue.StartsWith("PARETO") ||
                    ddlReportType.SelectedValue.StartsWith("SUMMARY"))
                {
                    DateTime date;

                    if (DateTime.TryParse(tbDateRangeFrom.Text, out date))
                    {
                        report.FromDate = date;
                    }

                    if (DateTime.TryParse(tbDateRangeTo.Text, out date))
                    {
                        report.ToDate = date;
                    }
                }
                else if (ddlReportType.SelectedValue == DTO.Report.TypeEnum.LOG_PARTICIPATION.ToString() ||
                    ddlReportType.SelectedValue.StartsWith("OPERATING"))
                {
                    DateTime date;

                    if (DateTime.TryParse(weekEnding.Text, out date))
                    {
                        report.ToDate = date;
                    }
                }

                switch (rbUserFilterType.SelectedValue)
                {
                    case "Individual Contributor":
                        report.Group = SaveReportGroupIndividualContributor(report.Public);
                        break;
                    case "Activity Log Team":
                        report.Group = SaveReportGroupActivityLogTeam(report.Public);
                        break;
                    case "Directorate":
                        report.Group = SaveReportGroupDirectorate(report.Public);
                        break;
                    case "Public Group":
                        report.Group = CreateReportGroupPublic(ddlPublicFilter.SelectedValue);
                        break;
                    case "Private Group":
                        report.Group = _reportGroupPrivate[privateFilter.SelectedIndex];
                        break;
                    default:
                        break;
                }

                if (ddlReportType.SelectedValue.StartsWith("OPERATING"))
                {
                    // Try to get the entered percentages.
                    int percent = 100;
                    int.TryParse(percentBase.Text, out percent);
                    report.PercentBase = percent;

                    percent = 100;
                    int.TryParse(percentGoal.Text, out percent);
                    report.PercentGoal = percent;

                    if (ddlReportType.SelectedValue == DTO.Report.TypeEnum.OPERATING_SUMMARY.ToString())
                    {
                        percent = 0;
                        int.TryParse(txtBarrierBase.Text, out percent);
                        report.Attributes = percent.ToString();

                        percent = 0;
                        int.TryParse(txtBarrierGoal.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 100;
                        int.TryParse(txtAdhereBase.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 100;
                        int.TryParse(txtAdhereGoal.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 100;
                        int.TryParse(txtAttainBase.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 100;
                        int.TryParse(txtAttainGoal.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 100;
                        int.TryParse(txtProdBase.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 100;
                        int.TryParse(txtProdGoal.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 0;
                        int.TryParse(txtUnplanBase.Text, out percent);
                        report.Attributes += "," + percent.ToString();

                        percent = 0;
                        int.TryParse(txtUnplanGoal.Text, out percent);
                        report.Attributes += "," + percent.ToString();
                    }
                }
                
                BLL.ReportManager.GetInstance().SaveReport(report);
                Utility.DisplayInfoMessage("Report Saved.");

                LoadData();

                // Reload the report
                for (int i = 0; i < _reports.Count; i++)
                {
                    if (0 == String.Compare(_reports[i].Value, report.Id.ToString()))
                    {
                        ddlExistingReports.SelectedIndex = i;
                        ddlExistingReports_SelectedIndexChanged(null, null);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Utility.DisplayException(e);
            }
        }

        /// <summary>
        /// Send alert to original owner that their
        /// Public Existing Report has been modified by another user.
        /// </summary>
        /// <param name="report"></param>
        private void GenerateAlertForModifiedReport(DTO.Report report)
        {
            DTO.Alert alert = new DTO.Alert();

            StringBuilder sb = new StringBuilder();
            sb.Append("Your Public Exisiting Report has been modified.");
            alert.Subject = sb.ToString();

            if (report != null)
            {
                sb = new StringBuilder();
                sb.Append("Your Public Exisiting Report \"");
                sb.Append(report.Title);
                sb.Append("\" has been modified by ");
                sb.Append(WALT.BLL.ProfileManager.GetInstance().GetProfile().DisplayName);
                sb.Append(". ");

                /* If the Title has changed, notify the user. */
                if (0 != String.Compare(report.Title, tbReportTitle.Text))
                {
                    sb.Append("It has been renamed to \"");
                    sb.Append(tbReportTitle.Text);
                    sb.Append(".\" ");
                }

                /* If the description has changed, notify the user. */
                if ((!(String.IsNullOrEmpty(report.Description)) && (description.Text != string.Empty))
                    && (0 != String.Compare(report.Description, description.Text)))
                {
                    sb.Append("The description has been changed from \"");
                    sb.Append(report.Description);
                    sb.Append("\" to \"");
                    sb.Append(description.Text);
                    sb.Append(".\" ");
                }

                /* If the Report Type has changed, notify the user. */
                if (report.Type.ToString() != ddlReportType.SelectedValue)
                {
                    sb.Append("The Report Type has been changed from \"");
                    sb.Append(report.Type.ToString());
                    sb.Append("\" to \"");
                    sb.Append(ddlReportType.SelectedItem.Text);
                    sb.Append(".\" ");
                }

                /* If the Date Range / Period End change, notify the user. */
                if ((ddlReportType.SelectedValue.StartsWith("PARETO"))
                    || (ddlReportType.SelectedValue.StartsWith("SUMMARY")))
                {
                    if ((report.ToDate.HasValue ? report.ToDate.Value.ToShortDateString() : "") != tbDateRangeTo.Text)
                    {
                        sb.Append("The To Date has been changed from \"");
                        sb.Append(report.ToDate);
                        sb.Append("\" to \"");
                        sb.Append(tbDateRangeTo.Text);
                        sb.Append(".\" ");
                    }

                    if ((report.FromDate.HasValue ? report.FromDate.Value.ToShortDateString() : "") != tbDateRangeFrom.Text)
                    {
                        sb.Append("The From Date has been changed from \"");
                        sb.Append(report.FromDate.HasValue ? report.FromDate.Value.ToShortDateString() : "");
                        sb.Append("\" to \"");
                        sb.Append(tbDateRangeFrom.Text);
                        sb.Append(".\" ");
                    }
                }
                else
                {
                    if (report.ToDate.ToString() != weekEnding.Text)
                    {
                        sb.Append("The Period End has been changed from \"");
                        sb.Append(report.ToDate.HasValue ? report.ToDate.Value.ToShortDateString() : "");
                        sb.Append("\" to \"");
                        sb.Append(weekEnding.Text);
                        sb.Append(".\" ");
                    }
                }

                /* If the User Filter Type changed, notify the user. */
                string oldReportGroupName = report.Group.Name.StartsWith("__") ? report.Group.Name.Substring(2, report.Group.Name.Length-2) : report.Group.Name;
                string newReportGroupName = string.Empty;

                if (rbUserFilterType.SelectedValue.Equals("Individual Contributor"))
                {
                    newReportGroupName = tbDisplayName.Text;
                }
                else if (rbUserFilterType.SelectedValue.Equals("Activity Log Team"))
                {
                    newReportGroupName = ddlActivityLogTeam.SelectedItem.Text;
                }
                else if (rbUserFilterType.SelectedValue.Equals("Directorate"))
                {
                    newReportGroupName = directorate.SelectedItem.Text;
                }
                else if (rbUserFilterType.SelectedValue.Equals("Public Group"))
                {
                    newReportGroupName = ddlPublicFilter.SelectedItem.Text;
                }
                else
                {
                    newReportGroupName = string.Empty;
                }

                if (0 != String.Compare(oldReportGroupName, newReportGroupName))
                {
                    sb.Append("The User Filter Type has been changed from \"");
                    sb.Append(oldReportGroupName);
                    sb.Append("\" to \"");
                    sb.Append(newReportGroupName);
                    sb.Append(".\"");
                }

                alert.Profile = report.Owner;
            }

            alert.Message = sb.ToString();
            alert.Creator = WALT.BLL.ProfileManager.GetInstance().GetProfile();
            alert.EntryDate = DateTime.Now;

            WALT.BLL.ProfileManager.GetInstance().SaveAlert(alert);
        }

        /// <summary>
        /// Callback for the Clear button on Panel2 of ReportSelect.aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button12_Click(object sender, EventArgs e)
        {
            Clear();
        }

        /// <summary>
        /// If the report type is changed, default date range to last week.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rtype = ddlReportType.SelectedValue;

            if (rtype.StartsWith("OPERATING"))
            {
                if (rtype.Contains("BARRIER") || rtype.Contains("UNPLANNED"))
                {
                    percentBase.Text = "0";
                    percentGoal.Text = "0";
                }
                else
                {
                    percentBase.Text = "100";
                    percentGoal.Text = "100";

                    if (rtype == "Operating Report Summary")
                    {
                        txtBarrierBase.Text = "0";
                        txtBarrierGoal.Text = "0";
                        txtAdhereBase.Text = "100";
                        txtAdhereGoal.Text = "100";
                        txtAttainBase.Text = "100";
                        txtAttainGoal.Text = "100";
                        txtProdBase.Text = "100";
                        txtProdGoal.Text = "100";
                        txtUnplanBase.Text = "0";
                        txtUnplanGoal.Text = "0";
                    }
                }
            }

            //if ((ddlReportType.SelectedValue.StartsWith("Pareto"))
            //    || (ddlReportType.SelectedValue.StartsWith("Summary")))
            //{
            //    tbDateRangeFrom.Text = BLL.ReportManager.GetInstance().GetLastSunday(null).Date.ToShortDateString(); ;
            //    tbDateRangeTo.Text = tbDateRangeFrom.Text;

            //    Tr1.Visible = true;
            //    Tr2.Visible = false;

            //}
            //else if (ddlReportType.SelectedValue.Equals("WALT Team Information Report"))
            //{
            //    Tr1.Visible = false;
            //    Tr2.Visible = false;
            //}
            //else // Operating, Auditing
            //{
            //    weekEnding.Text = BLL.ReportManager.GetInstance().GetLastSunday(null).Date.ToShortDateString();

            //    weeklyOrMonthly.SelectedIndex = 0; // Weekly

            //    Tr1.Visible = false;
            //    Tr2.Visible = true;
            //}
        }

        /// <summary>
        /// Radio Button for User Group Type:
        ///   Individual Contributor
        ///   AL Team
        ///   Directorate
        ///   Public Group
        ///   Private Group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbUserFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetUserFilterSelectionVisibility();
            LoadUserFilterTypeData();
            
            // The drop down associated with the selected radio buton,
            // will have the first option selected (""). 
            // Therefore, reset items that could have been activated
            Button3.Visible = false; // Public Filter - Modify Button
            Button4.Visible = false; // Public Filter - Delete Button
            lPublicFilterLastSavedBy.Text = "";

            Button17.Visible = false; // Private Filter - Modify Button
            Button18.Visible = false; // Private Filter - Delete Button
        }

        /// <summary>
        /// Called when the Directorate drop down selected value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void directorate_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when the Activity Log Team drop down selected value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlActivityLogTeam_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Called when the Private Filter drop down selected value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void privateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (privateFilter.SelectedIndex > 0)
            {
                DTO.ReportGroup g = _reportGroupPrivate[privateFilter.SelectedIndex];

                if (((g.Owner != null) && (g.Owner.Id == BLL.ReportManager.GetInstance().GetProfile().Id))
                    || (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.DIRECTORATE_ADMIN) == true))
                {
                    Button17.Visible = true; // Private Filter - Modify Button
                    Button18.Visible = true; // Private Filter - Delete Button
                }
                else
                {
                    Button17.Visible = false; // Private Filter - Modify Button
                    Button18.Visible = false; // Private Filter - Delete Button
                }

                if (privateFilter.Text == "-- Create a New Group --")
                {
                    ClearDialog();
                    CheckBox1.Checked = false;
                    CheckBox1.Enabled = false;

                    // Pass 'false' to load the private and public groups, as this is a private report.
                    LoadDropDowns(false);

                    ShowDialog();
                    privateFilter.Text = "";
                }
            }
            else
            {
                Button17.Visible = false; // Private Filter - Modify Button
                Button18.Visible = false; // Private Filter - Delete Button
            }
        }

        protected void DropDownList7_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        void SetUserFilterSelectionVisibility()
        {
            // Set visible row below User Filter Type
            if (rbUserFilterType.SelectedValue.Equals("Individual Contributor"))
            {
                Tr3.Visible = false; // Public Group
                Tr4.Visible = true; // Individual Contributor
                Tr5.Visible = false; // Activity Log Team
                Tr6.Visible = false; // Private Group
                Tr7.Visible = false; // Directorate
            }
            else if (rbUserFilterType.SelectedValue.Equals("Activity Log Team"))
            {
                Tr3.Visible = false; // Public Group
                Tr4.Visible = false; // Individual Contributor
                Tr5.Visible = true; // Activity Log Team
                Tr6.Visible = false; // Private Group
                Tr7.Visible = false; // Directorate
            }
            else if (rbUserFilterType.SelectedValue.Equals("Directorate"))
            {
                Tr3.Visible = false; // Public Group
                Tr4.Visible = false; // Individual Contributor
                Tr5.Visible = false; // Activity Log Team
                Tr6.Visible = false; // Private Group
                Tr7.Visible = true; // Directorate
            }
            else if (rbUserFilterType.SelectedValue.Equals("Public Group"))
            {
                Tr3.Visible = true; // Public Group
                Tr4.Visible = false; // Individual Contributor
                Tr5.Visible = false; // Activity Log Team
                Tr6.Visible = false; // Private Group
                Tr7.Visible = false; // Directorate
            }
            else if (rbUserFilterType.SelectedValue.Equals("Private Group"))
            {
                Tr3.Visible = false; // Public Group
                Tr4.Visible = false; // Individual Contributor
                Tr5.Visible = false; // Activity Log Team
                Tr6.Visible = true; // Private Group
                Tr7.Visible = false; // Directorate
            }
            else
            {
                rbUserFilterType.SelectedIndex = 0;
                rbUserFilterType_SelectedIndexChanged(null, null);

                Tr3.Visible = false; // Public Group
                Tr4.Visible = false; // Individual Contributor
                Tr5.Visible = false; // Activity Log Team
                Tr6.Visible = true; // Private Group
                Tr7.Visible = false; // Directorate
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSaveIc_Click(object sender, EventArgs e)
        {
            try
            {
                DTO.Profile profile;

                profile = profileSelectorBackups.ProfilesChosen[0];

                // update the Individual Contributor text box
                tbDisplayName.Text = profile.DisplayName;
                tbUserName.Text = profile.Username;
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
            finally 
            {
                profileSelectorBackups.Clear();
            }
        }

        /// <summary>
        /// Called when Show Public check box is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cbShowPublic_CheckChanged(object sender, EventArgs e)
        {
            if (!cbShowPublic.Checked && !cbShowPrivate.Checked)
            {
                cbShowPrivate.Checked = true;
            }

            LoadReports(true);
            lExistingReportLastSavedBy.Text = string.Empty;
        }

        /// <summary>
        /// Called when Show Public check box is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cbShowPrivate_CheckChanged(object sender, EventArgs e)
        {
            if (!cbShowPublic.Checked && !cbShowPrivate.Checked)
            {
                cbShowPublic.Checked = true;
            }

            LoadReports(true);
            lExistingReportLastSavedBy.Text = string.Empty;
        }

        /// <summary>
        /// The From Date and/or the To Date can be blank.
        /// If From Date is blank, but To Date is not, set From Date to To Date.
        /// If To Date is blank, but From Date is not, set To Date to From Date.
        /// If both dates are blank, the BLL will handle it, as last week's date.
        /// </summary>
        /// <param name="report"></param>
        private void ReportDateCheck(DTO.Report report)
        {
            if (report != null)
            {
                if (report.Type.ToString().StartsWith("PARETO") || report.Type.ToString().StartsWith("SUMMARY"))
                {
                    if (!report.FromDate.HasValue && report.ToDate.HasValue)
                    {
                        report.FromDate = report.ToDate;
                        Utility.DisplayInfoMessage("'From Week' was blank. Using 'To Week' to generate report with one week of data.");
                    }

                    if (!report.ToDate.HasValue && report.FromDate.HasValue)
                    {
                        report.ToDate = report.FromDate;
                        Utility.DisplayInfoMessage("'To Week' was blank. Using 'From Week' to generate report with one week of data.");
                    }

                    if (!report.ToDate.HasValue && !report.FromDate.HasValue)
                    {
                        Utility.DisplayInfoMessage("No date range specified; generating report on last week.");
                    }
                }
                else if (report.Type.ToString().StartsWith("OPERATING") || report.Type == DTO.Report.TypeEnum.LOG_PARTICIPATION)
                {
                    if (weekEnding.Text == string.Empty)
                    {
                        Utility.DisplayInfoMessage("No date period specified; generating report on previous period.");
                    }
                }
                else
                {
                    // Empty
                }
            }

            return;
        }
    }
}

