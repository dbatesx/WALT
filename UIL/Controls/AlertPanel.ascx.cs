using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WALT.DTO;
using WALT.UIL.DataSources;

namespace WALT.UIL.Controls
{
    public partial class AlertPanel : System.Web.UI.UserControl
    {
        AlertDataSource _alertDataSource;

        protected void Page_Load(object sender, EventArgs e)
        {
            string sort = "ENTRYDATE";
            bool order = false;

            if (!IsPostBack)
            {
                if (HttpContext.Current.Session["alert_view"] != null)
                {
                    alertView.SelectedIndex = (int)HttpContext.Current.Session["alert_view"];
                }

                if (HttpContext.Current.Session["alert_page"] != null)
                {
                    alertGrid.PageIndex = (int)HttpContext.Current.Session["alert_page"];
                }

                if (HttpContext.Current.Session["alert_sort"] != null)
                {
                    sort = HttpContext.Current.Session["alert_sort"].ToString();
                    order = (bool)HttpContext.Current.Session["alert_order"];
                }

                if (HttpContext.Current.Session["alert_subject"] != null)
                {
                    txtSubject.Text = (string)HttpContext.Current.Session["alert_subject"];
                }

                if (HttpContext.Current.Session["alert_message"] != null)
                {
                    txtMessage.Text = (string)HttpContext.Current.Session["alert_message"];
                }

                if (HttpContext.Current.Session["alert_from"] != null)
                {
                    dateFrom.Text = (string)HttpContext.Current.Session["alert_from"];
                }

                if (HttpContext.Current.Session["alert_to"] != null)
                {
                    dateTo.Text = (string)HttpContext.Current.Session["alert_to"];
                }

                SetView();
            }

            if (HttpContext.Current.Session["alert_search"] == null)
            {
                rowSearch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            _alertDataSource = new AlertDataSource(alertView.SelectedIndex, sort, order);
            alertGrid.DataSource = _alertDataSource;
            GetAlerts(!IsPostBack);
        }

        private void GetAlerts(bool loadDetail)
        {
            _alertDataSource.ClearFilters();
            btnClear.Visible = false;

            if (txtSubject.Text != string.Empty)
            {
                HttpContext.Current.Session["alert_subject"] = txtSubject.Text;
                _alertDataSource.AddFilter(Alert.ColumnEnum.SUBJECT, txtSubject.Text);
                btnClear.Visible = true;
            }
            else if (HttpContext.Current.Session["alert_subject"] != null)
            {
                HttpContext.Current.Session.Remove("alert_subject");
            }

            if (txtMessage.Text != string.Empty)
            {
                HttpContext.Current.Session["alert_message"] = txtMessage.Text;
                _alertDataSource.AddFilter(Alert.ColumnEnum.MESSAGE, txtMessage.Text);
                btnClear.Visible = true;
            }
            else if (HttpContext.Current.Session["alert_message"] != null)
            {
                HttpContext.Current.Session.Remove("alert_message");
            }

            if (alertView.SelectedIndex == 2)
            {
                if (HttpContext.Current.Session["alert_profile"] != null)
                {
                    _alertDataSource.AddFilter(Alert.ColumnEnum.PROFILE, HttpContext.Current.Session["alert_profile"].ToString());
                    btnClear.Visible = true;
                }
            }
            else if (HttpContext.Current.Session["alert_sender"] != null)
            {
                _alertDataSource.AddFilter(Alert.ColumnEnum.CREATOR, HttpContext.Current.Session["alert_sender"].ToString());
                btnClear.Visible = true;
            }

            string dateFilter = string.Empty;

            if (dateFrom.Text != string.Empty)
            {
                dateFilter = ">," + dateFrom.Text;
            }
            else if (HttpContext.Current.Session["alert_from"] != null)
            {
                HttpContext.Current.Session.Remove("alert_from");
            }

            if (dateTo.Text != string.Empty)
            {
                if (dateFilter != string.Empty)
                {
                    dateFilter += ",";
                }

                dateFilter += "<," + dateTo.Text;
            }
            else if (HttpContext.Current.Session["alert_to"] != null)
            {
                HttpContext.Current.Session.Remove("alert_to");
            }

            if (dateFilter != string.Empty)
            {
                _alertDataSource.AddFilter(Alert.ColumnEnum.ENTRYDATE, dateFilter);
                btnClear.Visible = true;
            }

            alertGrid.DataBind();
            HighlightSelectedRow(null);

            bool alertsFound = alertGrid.Rows.Count > 0;

            if (alertView.SelectedIndex == 0)
            {
                btnMarkRead.Visible = alertsFound;
            }

            lblNone.Visible = !alertsFound;

            if (btnClear.Visible || alertsFound)
            {
                tblSearch.Style.Clear();
                LoadProfiles();
            }
            else
            {
                tblSearch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            if (loadDetail)
            {
                if (HttpContext.Current.Session["alert_id" + alertView.SelectedValue] != null)
                {
                    DisplayAlert((long)HttpContext.Current.Session["alert_id" + alertView.SelectedValue]);
                }
                else if (alertGrid.Rows.Count > 0)
                {
                    DisplayAlert((long)alertGrid.DataKeys[0].Value);
                }
                else
                {
                    cellDetail.Visible = false;
                }
            }
        }

        protected void alertView_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpContext.Current.Session["alert_view"] = alertView.SelectedIndex;
            _alertDataSource.SetView(alertView.SelectedIndex);
            SetView();
            GetAlerts(true);
        }

        private void SetView()
        {
            btnMarkRead.Visible = (alertView.SelectedIndex == 0 && alertGrid.Rows.Count > 0);
            alertGrid.Columns[0].Visible = alertView.SelectedIndex == 0;

            if (alertView.SelectedIndex == 2)
            {
                alertGrid.Columns[3].Visible = false;
                alertGrid.Columns[4].Visible = true;
                rowSender.Visible = false;
                rowSentTo.Visible = true;
            }
            else
            {
                alertGrid.Columns[3].Visible = true;
                alertGrid.Columns[4].Visible = false;
                rowSender.Visible = true;
                rowSentTo.Visible = false;
            }
        }

        protected void alertGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            HttpContext.Current.Session["alert_page"] = e.NewPageIndex;
            alertGrid.PageIndex = e.NewPageIndex;
            GetAlerts(false);
        }

        protected void alertGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            bool order = true;

            if (HttpContext.Current.Session["alert_sort"] != null)
            {
                string lastSort = HttpContext.Current.Session["alert_sort"].ToString();

                if (lastSort == e.SortExpression)
                {
                    order = !(bool)HttpContext.Current.Session["alert_order"];
                }
            }

            _alertDataSource.SetSort(e.SortExpression, order);
            GetAlerts(false);

            HttpContext.Current.Session["alert_sort"] = e.SortExpression;
            HttpContext.Current.Session["alert_order"] = order;
        }

        protected void lnkSubject_Click(object sender, EventArgs e)
        {
            LinkButton lnk = (LinkButton)sender;
            int idx = Convert.ToInt32(lnk.ClientID.Substring(lnk.ClientID.Length - 13, 2)) - 2;
            long id = (long)alertGrid.DataKeys[idx].Value;
            DisplayAlert(id);
        }

        private void DisplayAlert(long id)
        {
            Alert alert = WALT.BLL.ProfileManager.GetInstance().GetAlert(id);

            if (alert != null)
            {
                HttpContext.Current.Session["alert_id" + alertView.SelectedValue] = id;
                HighlightSelectedRow(id);

                lblSubject.Text = alert.Subject;
                lblCreated.Text = Utility.ConvertToLocal(alert.EntryDate).ToString();

                if (alertView.SelectedIndex == 2)
                {
                    cellProfile.Text = "<b>Sent To:</b>";
                    lblProfile.Text = alert.SentTo;
                }
                else
                {
                    cellProfile.Text = "<b>Sender:</b>";
                    lblProfile.Text = alert.Sender;
                }

                if (alert.Message != string.Empty)
                {
                    lblMessage.Text = alert.Message.Replace("\n", "<br>");
                    rowMessage1.Visible = true;
                    rowMessage2.Visible = true;
                }
                else
                {
                    rowMessage1.Visible = false;
                    rowMessage2.Visible = false;
                }

                rowLink.Visible = false;
                rowLinkBarrier1.Visible = false;
                rowLinkBarrier2.Visible = false;
                rowLinkTask.Visible = false;
                rowLinkPlan.Visible = false;

                if (alert.Message != string.Empty || alert.LinkedType.HasValue)
                {
                    cellCreated1.CssClass = "bottom";
                    cellCreated2.CssClass = "bottom";
                }
                else
                {
                    cellCreated1.CssClass = string.Empty;
                    cellCreated2.CssClass = string.Empty;
                }

                if (alert.LinkedType.HasValue &&
                    alert.LinkedType.Value != Alert.AlertEnum.SYSTEM)
                {
                    string type = alert.LinkedType.Value.ToString();
                    WeeklyTask wt = null;

                    if (alert.LinkedType.Value == Alert.AlertEnum.TASK)
                    {
                        DTO.Task task = WALT.BLL.TaskManager.GetInstance().GetTask(alert.LinkedId, false, true);

                        if (task == null || task.Deleted)
                        {
                            lblTaskLink.Text = "Task no longer exists";
                        }
                        else
                        {
                            lblTaskLink.Text = "<a href=\"/Task/ViewTask.aspx?id=" + task.Id.ToString() + "&Source=" +
                                HttpContext.Current.Request.RawUrl + "\">" + task.Title + "</a>";
                        }

                        rowLinkTask.Visible = true;
                    }
                    else if (alert.LinkedType.Value == Alert.AlertEnum.WEEKLY_TASK)
                    {
                        wt = WALT.BLL.TaskManager.GetInstance().GetWeeklyTask(alert.LinkedId);

                        if (wt == null)
                        {
                            lblTaskLink.Text = "Task removed from plan/log";
                            rowLinkTask.Visible = true;
                        }
                    }
                    else if (alert.LinkedType.Value == Alert.AlertEnum.BARRIER)
                    {
                        WeeklyBarrier wb = WALT.BLL.TaskManager.GetInstance().GetWeeklyBarrier(alert.LinkedId);

                        if (wb == null)
                        {
                            lblBarrierType.Text = string.Empty;
                            lblBarrierLink.Text = "Barrier removed from log";
                        }
                        else
                        {
                            wt = WALT.BLL.TaskManager.GetInstance().GetWeeklyTask(wb.WeeklyTaskId);
                            lblBarrierType.Text = wb.BarrierType == WeeklyBarrier.BarriersEnum.DELAY ? "Delay" : "Efficiency";
                            lblBarrierLink.Text = wb.Barrier.Code + " - " + wb.Barrier.Title;
                            lblBarrierComment.Text = wb.Comment;
                            rowLinkBarrier2.Visible = true;
                        }

                        rowLinkBarrier1.Visible = true;
                    }

                    if (wt != null)
                    {
                        WeeklyPlan wp = WALT.BLL.TaskManager.GetInstance().GetWeeklyPlan(wt.WeeklyPlanId, false);
                        lblTaskLink.Text = "<a href=\"/Task/ViewTask.aspx?id=" + wt.Task.Id.ToString() + "\">" + wt.Task.Title + "</a>";
                        lblPlanLink.Text = "<a href=\"/weekly.aspx?id=" + wp.Id.ToString() + "\">" + wp.WeekEnding.ToShortDateString() + ", " + wp.State.ToString() + "</a>";
                        rowLinkTask.Visible = true;
                        rowLinkPlan.Visible = true;
                    }

                    cellLink.CssClass = rowMessage2.Visible ? "divider" : string.Empty;
                    rowLink.Visible = true;
                }

                cellDetail.Visible = true;
            }
        }

        private void HighlightSelectedRow(long? id)
        {
            if (id.HasValue || HttpContext.Current.Session["alert_id" + alertView.SelectedValue] != null)
            {
                if (!id.HasValue)
                {
                    id = (long)HttpContext.Current.Session["alert_id" + alertView.SelectedValue];
                }

                int i = 0;

                foreach (GridViewRow row in alertGrid.Rows)
                {
                    if ((long)alertGrid.DataKeys[i].Value == id.Value)
                    {
                        row.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#99CCFF");
                    }
                    else
                    {
                        row.Style.Clear();
                    }

                    i++;
                }
            }
        }

        protected void btnMarkRead_Click(object sender, EventArgs e)
        {
            int i = 0;
            List<long> alertIDs = new List<long>();

            foreach (GridViewRow row in alertGrid.Rows)
            {
                CheckBox chk = (CheckBox)row.Cells[0].FindControl("chkRead");

                if (HttpContext.Current.Request[chk.ClientID.Replace('_', '$')] == "on")
                {
                    alertIDs.Add((long)alertGrid.DataKeys[i].Value);
                }

                i++;
            }

            if (alertIDs.Count > 0)
            {
                try
                {
                    WALT.BLL.ProfileManager.GetInstance().AcknowledgeAlerts(alertIDs);
                    long id = (long)HttpContext.Current.Session["alert_id0"];

                    if (alertIDs.Contains(id))
                    {
                        HttpContext.Current.Session.Remove("alert_id0");
                        GetAlerts(true);
                    }
                    else
                    {
                        GetAlerts(false);
                        HighlightSelectedRow(id);
                    }
                }
                catch
                {

                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (alertView.SelectedIndex == 2)
            {
                if (ddSentTo.SelectedIndex > 0)
                {
                    HttpContext.Current.Session["alert_profile"] = ddSentTo.SelectedValue;
                }
            }
            else if (ddSender.SelectedIndex > 0)
            {
                HttpContext.Current.Session["alert_sender"] = ddSender.SelectedValue;
            }

            GetAlerts(false);
        }

        protected void lnkToggleSearch_Click(object sender, EventArgs e)
        {
            bool visible = HttpContext.Current.Session["alert_search"] == null;

            HttpContext.Current.Session["alert_search"] = visible;

            if (visible)
            {
                HttpContext.Current.Session["alert_search"] = true;
                rowSearch.Style.Clear();
                LoadProfiles();
            }
            else
            {
                HttpContext.Current.Session.Remove("alert_search");
                rowSearch.Style.Add(HtmlTextWriterStyle.Display, "none");
                ClearSearch();
            }
        }

        private void LoadProfiles()
        {
            if (HttpContext.Current.Session["alert_search"] != null)
            {
                int count;

                if (alertView.SelectedIndex == 2)
                {
                    if (ddSentTo.Items.Count == 0)
                    {
                        string sel = string.Empty;

                        if (!IsPostBack && HttpContext.Current.Session["alert_profile"] != null)
                        {
                            sel = (string)HttpContext.Current.Session["alert_profile"];
                        }

                        List<Profile> profiles = WALT.BLL.ProfileManager.GetInstance().GetAlertProfiles();
                        ddSentTo.Items.Add(new ListItem());

                        foreach (Profile profile in profiles)
                        {
                            ddSentTo.Items.Add(new ListItem(profile.DisplayName, profile.Id.ToString()));

                            if (sel == profile.Id.ToString())
                            {
                                ddSentTo.SelectedValue = sel;
                            }
                        }
                    }

                    count = ddSentTo.Items.Count;
                }
                else
                {
                    if (ddSender.Items.Count == 0)
                    {
                        string sel = string.Empty;

                        if (!IsPostBack && HttpContext.Current.Session["alert_sender"] != null)
                        {
                            sel = (string)HttpContext.Current.Session["alert_sender"];
                        }

                        List<Profile> profiles = WALT.BLL.ProfileManager.GetInstance().GetAlertCreators();
                        ddSender.Items.Add(new ListItem());

                        foreach (Profile profile in profiles)
                        {
                            ddSender.Items.Add(new ListItem(profile.DisplayName, profile.Id.ToString()));

                            if (sel == profile.Id.ToString())
                            {
                                ddSender.SelectedValue = sel;
                            }
                        }
                    }

                    count = ddSender.Items.Count;
                }

                if (count > 1)
                {
                    tblSearch.Style.Clear();
                }
                else
                {
                    tblSearch.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearSearch();
        }

        private void ClearSearch()
        {
            if (btnClear.Visible)
            {
                txtSubject.Text = string.Empty;
                txtMessage.Text = string.Empty;
                ddSender.SelectedIndex = -1;
                ddSentTo.SelectedIndex = -1;
                dateFrom.Text = string.Empty;
                dateTo.Text = string.Empty;

                HttpContext.Current.Session.Remove("alert_sender");
                HttpContext.Current.Session.Remove("alert_profile");

                GetAlerts(false);
            }
        }
    }
}
