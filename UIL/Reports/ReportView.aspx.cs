using System;
using System.Collections;
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
using System.Collections.Generic;
using Microsoft.Reporting.WebForms;

namespace WALT.UIL.Reports
{
    public partial class ReportView : System.Web.UI.Page
    {
        private string _title;

        protected void Page_Load(object sender, EventArgs e)
        {
            DTO.Report report = (DTO.Report)HttpContext.Current.Session["report_generate"];

            // This postback check is required or report viewer shows the
            // loading indicator indefinitely - the report never loads.
            if (!IsPostBack && report != null)
            {
                string filename = "";
                string reportName = DTO.Report.GetReportName(report.Type);
                _title = "WALT - " + reportName;
                reportName += " ";

                ReportViewer1.Reset();
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.EnableExternalImages = true;
                ReportViewer1.ShowPrintButton = false;
                BLL.ReportManager.GetInstance().Clear();

                if (report.Type != DTO.Report.TypeEnum.TEAM_INFO)
                {
                    DateTime from = BLL.ReportManager.GetInstance().GetWeekEnding(report.FromDate);
                    DateTime to = BLL.ReportManager.GetInstance().GetWeekEnding(report.ToDate);
                    reportName += from.ToShortDateString();

                    if (from.ToShortDateString() != to.ToShortDateString())
                    {
                        reportName += " - " + to.ToShortDateString();
                    }
                }
                else
                {
                    reportName += DateTime.Now.ToShortDateString();
                }

                List<ReportParameter> parameterList = new List<ReportParameter>();
                parameterList.Add(new ReportParameter("reportName", reportName));
                parameterList.Add(new ReportParameter("reportTitle", String.IsNullOrEmpty(report.Title) ? " " : report.Title));
                parameterList.Add(new ReportParameter("logo", 
                    Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "") + @"/images/logo.jpg"));
                parameterList.Add(new ReportParameter("userFilter", BLL.ReportManager.GetInstance().GetFilterGroupAsString(report.Group)));

                if (report.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_TABLE)
                {
                    filename = "Report1";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParetoReportUnplanned_codeList",
                        BLL.ReportManager.GetInstance().GetParetoReportUnplannedCodeList(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_GRAPH)
                {
                    filename = "Report1a";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParetoReportUnplanned_codeList",
                        BLL.ReportManager.GetInstance().GetParetoReportUnplannedCodeList(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_RAW)
                {
                    filename = "paretoReportRaw";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParetoReportTable",
                        BLL.ReportManager.GetInstance().DownloadParetoReportUnplannedCode(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_TABLE ||
                    report.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_TABLE)
                {
                    filename = "Report2";
                    parameterList.Add(new ReportParameter("BarrierType",
                       report.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_TABLE ? "Efficiency" : "Task Delay"));

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParetoReportBarriers_codeList",
                        BLL.ReportManager.GetInstance().GetParetoReportBarriers(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_GRAPH ||
                    report.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_GRAPH)
                {
                    filename = "Report2a";
                    parameterList.Add(new ReportParameter("BarrierType",
                        report.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_GRAPH ? "Efficiency" : "Task Delay"));

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParetoReportBarriers_codeList",
                        BLL.ReportManager.GetInstance().GetParetoReportBarriers(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_RAW ||
                    report.Type == DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW)
                {
                    filename = "paretoReportRaw";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParetoReportTable",
                        BLL.ReportManager.GetInstance().DownloadParetoReportBarriers(report)));
                }
                else if (report.Type.ToString().StartsWith("OPERATING"))
                {
                    parameterList.Add(new ReportParameter("percentBase", Convert.ToString(report.PercentBase)));
                    parameterList.Add(new ReportParameter("percentGoal", Convert.ToString(report.PercentGoal)));

                    if (!report.Type.ToString().EndsWith("GRAPH"))
                    {
                        bool expand = false;

                        if (report.Group.Directorates.Count == 0)
                        {
                            int groupings = report.Group.Teams.Count + report.Group.Profiles.Count + report.Group.Groups.Count;

                            if (groupings == 1 || report.Group.Profiles.Count == groupings)
                            {
                                expand = true;
                            }
                        }

                        filename = "operatingReportTable";
                        parameterList.Add(new ReportParameter("typeAverageTitle", report.IsMonthly ? "6 Month" : "5 Week"));
                        parameterList.Add(new ReportParameter("Expand", expand.ToString()));

                        if (report.Type == DTO.Report.TypeEnum.OPERATING_SUMMARY)
                        {
                            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OperatingReportTable",
                                BLL.ReportManager.GetInstance().GetOperatingReportSummary(report)));
                        }
                        else
                        {
                            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OperatingReportTable",
                                BLL.ReportManager.GetInstance().GetOperatingReport(report)));
                        }
                    }
                    else
                    {
                        if (report.Type == DTO.Report.TypeEnum.OPERATING_LOAD_GRAPH)
                        {
                            parameterList.Add(new ReportParameter("graphTitle", "Combined Load"));
                            parameterList.Add(new ReportParameter("legend", "TOTAL % LOAD"));
                            parameterList.Add(new ReportParameter("isMonthly", report.IsMonthly ? "true" : "false"));
                        }
                        else if (report.Type == DTO.Report.TypeEnum.OPERATING_BARRIER_GRAPH)
                        {
                            parameterList.Add(new ReportParameter("graphTitle", "Combined Efficiency"));
                            parameterList.Add(new ReportParameter("legend", "TOTAL % EFFICIENCY"));
                            parameterList.Add(new ReportParameter("isMonthly", report.IsMonthly ? "true" : "false"));
                        }
                        else if (report.Type == DTO.Report.TypeEnum.OPERATING_ADHERENCE_GRAPH)
                        {
                            parameterList.Add(new ReportParameter("graphTitle", "Combined Plan Adherence"));
                            parameterList.Add(new ReportParameter("legend", "Completed Planned / Planned"));
                            parameterList.Add(new ReportParameter("isMonthly", report.IsMonthly ? "true" : "false"));
                        }
                        else if (report.Type == DTO.Report.TypeEnum.OPERATING_ATTAINMENT_GRAPH)
                        {
                            parameterList.Add(new ReportParameter("graphTitle", "Combined Plan Attainment"));
                            parameterList.Add(new ReportParameter("legend", "Completed / Planned"));
                            parameterList.Add(new ReportParameter("isMonthly", report.IsMonthly ? "true" : "false"));
                        }
                        else if (report.Type == DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_GRAPH)
                        {
                            parameterList.Add(new ReportParameter("graphTitle", "Combined Productivity"));
                            parameterList.Add(new ReportParameter("legend", "TOTAL % PRODUCTIVITY"));
                            parameterList.Add(new ReportParameter("isMonthly", report.IsMonthly ? "true" : "false"));
                        }
                        else if (report.Type == DTO.Report.TypeEnum.OPERATING_UNPLANNED_GRAPH)
                        {
                            parameterList.Add(new ReportParameter("graphTitle", "Combined Unplanned"));
                            parameterList.Add(new ReportParameter("legend", "Unplanned Hours / Planned Hours"));
                            parameterList.Add(new ReportParameter("isMonthly", report.IsMonthly ? "true" : "false"));
                        }

                        filename = "operatingReportGraph";
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("OperatingReportGraph",
                            BLL.ReportManager.GetInstance().GetOperatingReportGraph(report)));
                    }
                }
                else if (report.Type == DTO.Report.TypeEnum.SUMMARY_EFFICIENCY_BARRIER ||
                    report.Type == DTO.Report.TypeEnum.SUMMARY_DELAY_BARRIER)
                {
                    filename = "SummaryReportTable";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SummaryReportTable",
                        BLL.ReportManager.GetInstance().GetSummaryReportBarrier(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.SUMMARY_UNPLANNED)
                {
                    filename = "SummaryReportTable";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SummaryReportTable",
                        BLL.ReportManager.GetInstance().GetSummaryReportUnplanned(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.LOG_PARTICIPATION)
                {
                    filename = "auditReportParticipationTable";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParticipationReportParticipation",
                        BLL.ReportManager.GetInstance().GetParticipationReport(report)));
                }
                else if (report.Type == DTO.Report.TypeEnum.TEAM_INFO)
                {
                    filename = "waltTeamInfo";

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("waltTeamInformation",
                        BLL.ReportManager.GetInstance().GetWaltTeamInformation(report)));
                }
                else
                {
                    // Error - Unknown report selected
                }

                // The source of the report definition (ReportPath) must be specified before
                // a call to SetParameters is made or you will get the following:
                //   "Microsoft.Reporting.WebForms.MissingReportSourceException:
                //   The source of the report definition has not been specified"
                ReportViewer1.LocalReport.ReportPath = "Reports\\" + filename + ".rdlc";

                ReportViewer1.LocalReport.SetParameters(parameterList);
                BLL.ReportManager.GetInstance().Clear();
            }
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            this.Title = _title;
        }
    }
}
