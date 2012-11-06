using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WALT.BLL;
using System.IO;
using System.Text;

namespace WALT.UIL.Reports
{
    public partial class ReportDownload : System.Web.UI.Page
    {
        /// <summary>
        /// 
        /// Notes:
        /// 
        /// Excel 2007 Extension Warning On Opening Excel Workbook from a Web Site
        ///   Web sites that use the "application/x-msexcel" or 
        ///   "application/vnd.ms-excel" MIME type to open web page content
        ///   inside of Microsoft Excel may encounter the following warning
        ///   prompt when the file attempts to open in Excel 2007:
        ///   
        ///     "The file you are trying to open, '[filename]', is in a different
        ///     format than specified by the file extension. Verify that the file
        ///     is not corrupted and is from a trusted source before opening the
        ///     file. Do you want to open the file now?"  (Yes | No | Help)
        ///   http://blogs.msdn.com/b/vsofficedeveloper/archive/2008/03/11/excel-2007-extension-warning.aspx
        /// 
        /// 
        ///  When you open a file in Excel 2007, you receive a warning that the
        ///  file format differs from the format that the file name extension specifies
        ///    http://support.microsoft.com/kb/948615
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            DTO.Report report = (DTO.Report)HttpContext.Current.Session["report_generate"];

            if ((!IsPostBack) && (report != null))
            {
                if (report.Type == DTO.Report.TypeEnum.PARETO_UNPLANNED_TABLE)
                {
                    List<ParetoReportTable> plans = new List<ParetoReportTable>();
                    plans.AddRange(BLL.ReportManager.GetInstance().DownloadParetoReportUnplannedCode(report));

                    List<string> classAsStringList = new List<string>();

                    // Place header into string list
                    classAsStringList.Add(plans.First().ParetoReportTableCsvHeader(report.Type));

                    // Convert each item into a string and add to list.
                    foreach (ParetoReportTable p in plans)
                    {
                        classAsStringList.AddRange(p.ParetoReportTableToCsvList());
                    }

                    // Stream the string to the user as a text/csv file.
                    string attachment = "attachment; filename=MyCsvReport.xls";

                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", attachment);
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("Pragma", "public");

                    // Stream each item in the list.
                    foreach (var line in classAsStringList)
                    {
                        HttpContext.Current.Response.Write(line);
                    }

                    HttpContext.Current.Response.End();
                }
                else if (report.Type == DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_TABLE)
                {
                    List<ParetoReportTable> plans = new List<ParetoReportTable>();
                    plans.AddRange(BLL.ReportManager.GetInstance().DownloadParetoReportBarriers(report));

                    List<string> classAsStringList = new List<string>();

                    // Place header into string list
                    classAsStringList.Add(plans.First().ParetoReportTableCsvHeader(report.Type));

                    // Convert each item into a string and add to list.
                    foreach (ParetoReportTable p in plans)
                    {
                        classAsStringList.AddRange(p.ParetoReportTableToCsvList());
                    }

                    // Stream the string to the user as a text/csv file.
                    string attachment = "attachment; filename=MyCsvReport.xls";

                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", attachment);
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("Pragma", "public");

                    // Stream each item in the list.
                    foreach (var line in classAsStringList)
                    {
                        HttpContext.Current.Response.Write(line);
                    }

                    HttpContext.Current.Response.End();
                }
                else if (report.Type == DTO.Report.TypeEnum.OPERATING_LOAD_TABLE)
                {
                    List<OperatingReportTable> plans = new List<OperatingReportTable>();
                    plans.AddRange(BLL.ReportManager.GetInstance().GetOperatingReport(report));

                    List<string> classAsStringList = new List<string>();

                    // Place header into string list
                    classAsStringList.Add(plans.First().OperatingReportTableCsvHeader());

                    // Convert each item into a string and add to list.
                    foreach (OperatingReportTable p in plans)
                    {
                        classAsStringList.AddRange(p.OperatingReportTableToCsvList());
                    }

                    // Stream the string to the user as a text/csv file.
                    string attachment = "attachment; filename=MyCsvReport.csv";

                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", attachment);
                    Response.ContentType = "text/csv";
                    Response.AddHeader("Pragma", "public");

                    // Stream each item in the list.
                    foreach (var line in classAsStringList)
                    {
                        HttpContext.Current.Response.Write(line);
                    }

                    HttpContext.Current.Response.End();
                }
                else
                {
                }
            }
        }
    }
}