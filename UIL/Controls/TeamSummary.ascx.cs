using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using WALT.DTO;

namespace WALT.UIL.Controls
{
    public partial class TeamSummary : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ViewState["tsRows"] == null)
            {
                long selID = -1;
                DateTime week = DateTime.Now.Date;
                List<Team> adminTeams = WALT.BLL.AdminManager.GetInstance().GetTeamsOwned();

                if (adminTeams.Count > 0)
                {
                    foreach (Team team in adminTeams)
                    {
                        if (selID == -1 && team.Type == Team.TypeEnum.TEAM)
                        {
                            selID = team.Id;
                        }
                    }
                }

                adminTeams.AddRange(WALT.BLL.AdminManager.GetInstance().GetTeams("ADMIN"));

                List<Team> selTeams = new List<Team>();
                List<long> teamIDs = new List<long>();

                foreach (Team team in adminTeams)
                {
                    if (team.Type == Team.TypeEnum.DIRECTORATE)
                    {
                        List<Team> dirTeams = BLL.AdminManager.GetInstance().GetTeamsByParent(team.Id, false);

                        foreach (Team dirTeam in dirTeams)
                        {
                            if (!teamIDs.Contains(dirTeam.Id))
                            {
                                selTeams.Add(dirTeam);
                                teamIDs.Add(dirTeam.Id);
                            }
                        }
                    }
                    else if (!teamIDs.Contains(team.Id))
                    {
                        selTeams.Add(team);
                        teamIDs.Add(team.Id);
                    }
                }

                if (selTeams.Count == 0)
                {
                    if (BLL.AdminManager.GetInstance().GetTeamByRole("MEMBER") == null ||
                        BLL.ProfileManager.GetInstance().GetProfile().ExemptPlan)
                    {
                        tsContent.Visible = false;
                        return;
                    }

                    rowTitle.Visible = false;
                    cellTeam.Visible = false;
                    cellTitle.Visible = true;
                    hdrColumn.Text = "Week";

                    if (Session["tsWeek"] != null)
                    {
                        week = (DateTime)Session["tsWeek"];
                    }
                }
                else
                {
                    int i = 0;
                    selTeams = selTeams.OrderBy(x => x.Name).ToList();

                    foreach (Team team in selTeams)
                    {
                        ddTeam.Items.Add(new ListItem(team.Name, team.Id.ToString()));
                        ViewState["owner_" + i.ToString()] = team.Owner.DisplayName;
                        i++;
                    }

                    if (selID != -1)
                    {
                        ddTeam.SelectedValue = selID.ToString();
                    }

                    rowTitle.Visible = true;
                    cellTeam.Visible = true;
                    cellTitle.Visible = false;
                    hdrColumn.Text = "Employee";

                    if (Session["tsView"] != null)
                    {
                        string[] str = Session["tsView"].ToString().Split(',');
                        ddTeam.SelectedValue = str[0];
                        week = Convert.ToDateTime(str[1]);
                    }
                    else
                    {
                        ddTeam.SelectedIndex = 0;
                    }
                }

                LoadData(week);
            }
            else
            {
                LoadData(null);
            }
        }

        private void LoadData(DateTime? date)
        {
            while (tblSummary.Rows.Count > 2)
            {
                tblSummary.Rows.RemoveAt(2);
            }

            if (date.HasValue)
            {
                DateTime week = date.Value;

                while (week.DayOfWeek != DayOfWeek.Sunday)
                {
                    week = week.AddDays(1);
                }

                ViewState["tsWeek"] = week;

                if (rowTitle.Visible)
                {
                    int i = 0;
                    long teamID = Convert.ToInt64(ddTeam.SelectedValue);
                    List<Profile> profiles = BLL.AdminManager.GetInstance().GetTeamMembers(teamID, week, week, true);

                    lblALM.Text = (string)ViewState["owner_" + ddTeam.SelectedIndex.ToString()];
                    lblWeek.Text = week.AddDays(-6).ToString("M/d/yy") + " - " + week.ToString("M/d/yy");

                    foreach (Profile profile in profiles)
                    {
                        if (!profile.ExemptPlan)
                        {
                            AddRow(BLL.TaskManager.GetInstance().GetWeeklyPlan(week, profile),
                                week, profile, i);

                            i++;
                        }
                    }

                    ViewState["tsRows"] = i;
                }
                else
                {
                    int row = 0;
                    Profile profile = BLL.ProfileManager.GetInstance().GetProfile();

                    lblWeek.Text = week.AddDays(-13).ToString("M/d/yy") + " - " + week.AddDays(7).ToString("M/d/yy");

                    for (int i = -7; i < 8; i += 7)
                    {
                        DateTime weekEnding = week.AddDays(i);
                        WeeklyPlan plan = BLL.TaskManager.GetInstance().GetWeeklyPlan(weekEnding);
                        AddRow(plan, weekEnding, profile, row);
                        row++;
                    }

                    ViewState["tsRows"] = row;
                }
            }
            else
            {
                int rows = (int)ViewState["tsRows"];

                for (int i = 0; i < rows; i++)
                {
                    TableRow tblRow = new TableRow();
                    tblRow.Cells.Add(CreateCell(i, 0));
                    tblRow.Cells.Add(CreateCell(i, 1));
                    tblRow.Cells.Add(CreateCell(i, 2));
                    tblRow.Cells.Add(CreateCell(i, 3));
                    tblRow.Cells.Add(CreateCell(i, 4));
                    tblRow.Cells.Add(CreateCell(i, 5));
                    tblRow.Cells.Add(CreateCell(i, 6));
                    tblRow.Cells.Add(CreateCell(i, 7));
                    tblRow.Cells.Add(CreateCell(i, 8));
                    tblSummary.Rows.Add(tblRow);
                }
            }
        }

        private void AddRow(WeeklyPlan plan, DateTime week, Profile profile, int row)
        {
            string status = "Not started";
            string mod = string.Empty;
            double planned = 0;
            double pActual = 0;
            double unplanned = 0;
            double tActual = 0;
            double barrier = 0;
            double delay = 0;
            TableRow tblRow = new TableRow();

            if (plan != null)
            {
                status = FormatStatus(plan.State);

                if (plan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED)
                {
                    status += " by " + plan.PlanApprovedBy.DisplayName;
                }
                else if (plan.State == WeeklyPlan.StatusEnum.LOG_APPROVED)
                {
                    status += " by " + plan.LogApprovedBy.DisplayName;
                    tblRow.BackColor = Color.FromArgb(204, 255, 204);
                }
                else if (plan.State == WeeklyPlan.StatusEnum.LOG_READY ||
                         plan.State == WeeklyPlan.StatusEnum.PLAN_READY)
                {
                    tblRow.BackColor = Color.Yellow;
                }

                mod = Utility.ConvertToLocal(plan.Modified).ToString("M/d h:mm tt");
                planned = plan.WeeklyTasks.Sum(item => item.PlanHours.Values.Sum());
                unplanned = plan.WeeklyTasks.Where(item => item.UnplannedCode != null).Sum(item => item.ActualHours.Values.Sum());
                tActual = plan.WeeklyTasks.Sum(item => item.ActualHours.Values.Sum());
                pActual = tActual - unplanned;
                barrier = plan.WeeklyTasks.Sum(item => item.Barriers.Where(
                    item2 => item2.BarrierType == WeeklyBarrier.BarriersEnum.EFFICIENCY).Sum(item3 => item3.Hours.Values.Sum()));

                delay = plan.WeeklyTasks.Sum(item => item.Barriers.Where(
                    item2 => item2.BarrierType == WeeklyBarrier.BarriersEnum.DELAY).Sum(item3 => item3.Hours.Values.Sum()));
            }

            if (rowTitle.Visible)
            {
                tblRow.Cells.Add(CreateCell(row, 0, profile.DisplayName));
            }
            else
            {
                tblRow.Cells.Add(CreateCell(row, 0, week.AddDays(-6).ToString("M/d/yy") + " - " + week.ToString("M/d/yy")));
            }

            tblRow.Cells.Add(CreateCell(row, 1,
                "<a href=\"weekly.aspx?profileID=" + profile.Id.ToString() + "&week=" +
                HttpUtility.UrlEncode(week.ToShortDateString()) + "\">" + status + "</a>"));

            tblRow.Cells.Add(CreateCell(row, 2, mod));
            tblRow.Cells.Add(CreateCell(row, 3, Utility.RoundDoubleToString(planned)));
            tblRow.Cells.Add(CreateCell(row, 4, Utility.RoundDoubleToString(pActual)));
            tblRow.Cells.Add(CreateCell(row, 5, Utility.RoundDoubleToString(unplanned)));
            tblRow.Cells.Add(CreateCell(row, 6, Utility.RoundDoubleToString(tActual)));
            tblRow.Cells.Add(CreateCell(row, 7, Utility.RoundDoubleToString(barrier)));
            tblRow.Cells.Add(CreateCell(row, 8, Utility.RoundDoubleToString(delay)));
            tblSummary.Rows.Add(tblRow);

            for (int i = 3; i <= 8; i++)
            {
                tblRow.Cells[i].HorizontalAlign = HorizontalAlign.Right;
            }
        }

        private TableCell CreateCell(int row, int col, string text)
        {
            TableCell cell = new TableCell();
            cell.Text = text;
            ViewState["cell_" + row.ToString() + "_" + col.ToString()] = text;
            return cell;
        }

        private TableCell CreateCell(int row, int col)
        {
            TableCell cell = new TableCell();
            cell.Text = ViewState["cell_" + row.ToString() + "_" + col.ToString()].ToString();
            return cell;
        }

        protected void ddTeam_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime week = Convert.ToDateTime(ViewState["tsWeek"]);
            Session["tsView"] = ddTeam.SelectedValue + "," + week.ToShortDateString();
            LoadData(week);
        }

        protected void lnkPrevWeek_Click(object sender, EventArgs e)
        {
            DateTime week;

            if (rowTitle.Visible)
            {
                week = ((DateTime)ViewState["tsWeek"]).AddDays(-7);
                Session["tsView"] = ddTeam.SelectedValue + "," + week.ToShortDateString();
            }
            else
            {
                week = ((DateTime)ViewState["tsWeek"]).AddDays(-21);
                Session["tsWeek"] = week;
            }

            LoadData(week);
        }

        protected void lnkNextWeek_Click(object sender, EventArgs e)
        {
            DateTime week;

            if (rowTitle.Visible)
            {
                week = ((DateTime)ViewState["tsWeek"]).AddDays(7);
                Session["tsView"] = ddTeam.SelectedValue + "," + week.ToShortDateString();
            }
            else
            {
                week = ((DateTime)ViewState["tsWeek"]).AddDays(21);
                Session["tsWeek"] = week;
            }

            LoadData(week);
        }

        private string FormatStatus(WeeklyPlan.StatusEnum status)
        {
            string state = status.ToString().Replace('_', ' ');
            return state[0] + state.Substring(1).ToLower();
        }
    }
}