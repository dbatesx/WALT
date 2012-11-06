using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace WALT.UIL.Task
{
    public partial class Import1 : System.Web.UI.Page
    {
        List<DTO.Task> _flatTaskList;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        void FlattenList(List<DTO.Task> tasks)
        {
            for (int i = 0; i < tasks.Count(); i++)
            {
                _flatTaskList.Add(tasks[i]);

                if (tasks[i].Children.Count() > 0)
                {
                    FlattenList(tasks[i].Children);
                }
            }
        }

        void FlattenSavedList(List<DTO.Task> tasks)
        {
            for (int i = 0; i < tasks.Count(); i++)
            {
                if (!tasks[i].Error)
                {
                    _flatTaskList.Add(tasks[i]);
                }

                if (tasks[i].Children.Count() > 0)
                {
                    FlattenSavedList(tasks[i].Children);
                }
            }
        }
                
        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (FileUpload1.HasFile && FileUpload1.FileName.EndsWith(".xlsx"))
                {
                    string path = "c:\\temp\\" + FileUpload1.FileName;
                    FileUpload1.SaveAs(path);

                    List<DTO.Task> tasks = BLL.TaskManager.GetInstance().ImportTasksFromExcel(path);//, errors);

                    _flatTaskList = new List<DTO.Task>();
                    FlattenList(tasks);
                    
                    int errorsExist = 0;

                    DataTable dt = new DataTable();                   
                    dt.Columns.Add(new DataColumn("Error"));
                    dt.Columns.Add(new DataColumn("Er"));                 
                    dt.Columns.Add(new DataColumn("OwnerDisplayName"));
                    dt.Columns.Add(new DataColumn("AssigneeDisplayName"));
                    dt.Columns.Add(new DataColumn("Title"));                   
                    dt.Columns.Add(new DataColumn("Source"));
                    dt.Columns.Add(new DataColumn("SourceID"));
                    dt.Columns.Add(new DataColumn("StartDate"));
                    dt.Columns.Add(new DataColumn("DueDate"));                  
                    dt.Columns.Add(new DataColumn("Hours"));                 
                    dt.Columns.Add(new DataColumn("ProgramTitle"));
                    dt.Columns.Add(new DataColumn("ExitCriteria"));
                    dt.Columns.Add(new DataColumn("WBS"));
                    dt.Columns.Add(new DataColumn("OwnerComments"));
                    dt.Columns.Add(new DataColumn("AssigneeComments"));

                    for (int i = 0; i < _flatTaskList.Count; i++)
                    {
                        DataRow row = dt.NewRow();

                        if (_flatTaskList[i].Error) 
                        {
                            errorsExist++;
                        }

                        foreach (string er in _flatTaskList[i].ErrorMessage)
                        {
                            if (!string.IsNullOrEmpty(row["Error"].ToString()))
                                row["Error"] += "<br />";

                            row["Error"] += er;
                        }

                        if (_flatTaskList[i].Error)
                        {
                            row["Er"] = "Fail";
                        }
                        else
                        {
                            row["Er"] = "Pass";
                        }
                                               
                        row["OwnerDisplayName"] = _flatTaskList[i].OwnerDisplayName;
                        row["AssigneeDisplayName"] = _flatTaskList[i].AssigneeDisplayName;
                        row["Title"] = _flatTaskList[i].Title;                     
                        row["Source"] = _flatTaskList[i].Source;
                        row["SourceID"] = string.IsNullOrEmpty(_flatTaskList[i].SourceID) ? "WALT_[ID]" : _flatTaskList[i].SourceID;
                        row["StartDate"] = _flatTaskList[i].StartDate;
                        row["DueDate"] = _flatTaskList[i].DueDate;                      
                        row["Hours"] = _flatTaskList[i].Hours;
                        row["ProgramTitle"] = _flatTaskList[i].ProgramTitle;
                        row["ExitCriteria"] = _flatTaskList[i].ExitCriteria;
                        row["WBS"] = _flatTaskList[i].WBS;
                        row["OwnerComments"] = _flatTaskList[i].OwnerComments;
                        row["AssigneeComments"] = _flatTaskList[i].AssigneeComments;
                        dt.Rows.Add(row);                        
                    }

                    if (_flatTaskList.Count == errorsExist )
                    {
                        Label2.Text = errorsExist + " errors found.  Please correct and try again";
                        Button2.Visible = false;
                    }
                    else
                    {
                        Label2.Text = _flatTaskList.Count - errorsExist + " new tasks ready for import";
                        Button2.Visible = true;
                    }

                    Session["imported_tasks"] = tasks;
                    Session["imported_data_table"] = dt;

                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                    Panel1.Visible = false;
                    Panel2.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            List<DTO.Task> tasks = (List<DTO.Task>)Session["imported_tasks"];
          
            try
            {
                BLL.TaskManager.GetInstance().SaveImportedTaskList(tasks);
                
                Panel1.Visible = true;
                Panel2.Visible = false;                             
                                
                _flatTaskList = new List<DTO.Task>();
                FlattenSavedList(tasks);
                //FlattenList(tasks);
               
                Utility.DisplayInfoMessage("Import complete - " + _flatTaskList.Count() + " tasks imported");
                Session.Remove("imported_tasks");
                Session.Remove("imported_data_table");
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage(ex.Message);
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            Session.Remove("imported_tasks");
            Session.Remove("imported_data_table");
            Panel1.Visible = true;
            Panel2.Visible = false;
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            DataTable dt = (DataTable)Session["imported_data_table"];
            GridView1.PageIndex = e.NewPageIndex;
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[1].Style.Add("border-right", "solid 1px black");
            e.Row.Cells[0].Style.Add("border-left", "solid 1px black");

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lbl_err = (Label)e.Row.FindControl("lblErr");
                Label lbl_errMsg = (Label)e.Row.FindControl("lblErrMsg");

                if (lbl_err.Text.Equals("Fail"))
                {
                    e.Row.Style.Add("background-color", "#ffcccc");
                }
                else if (!string.IsNullOrEmpty(lbl_errMsg.Text))
                {
                    e.Row.Style.Add("background-color", "#ffffcc");
                }
                else
                {
                    e.Row.Style.Add("background-color", "#ccffcc");
                }
            }
        }

    }
}