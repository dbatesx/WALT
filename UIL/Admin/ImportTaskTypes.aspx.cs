using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WALT.UIL.Admin
{
    public partial class ImportTaskTypes : System.Web.UI.Page
    {
        private long _dirID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                _dirID = -1;
                bool admin = false;

                if (HttpContext.Current.Request["dirID"] != null)
                {
                    if (HttpContext.Current.Request["dirID"] == "all")
                    {
                        if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
                        {
                            _dirID = BLL.AdminManager.GetInstance().GetOrg().Id;
                            lblDirectorate.Text = "All";
                            admin = true;
                        }
                    }
                    else if (long.TryParse(HttpContext.Current.Request["dirID"], out _dirID))
                    {
                        DTO.Team team = BLL.AdminManager.GetInstance().GetTeam(_dirID, false);
                        lblDirectorate.Text = team.Name;

                        admin = BLL.ProfileManager.GetInstance().IsDirectorateAdmin(_dirID);
                    }
                }

                if (_dirID == -1 || !admin)
                {
                    Response.Redirect("/");
                }

                ViewState["dirID"] = _dirID;
            }
            else
            {
                _dirID = (long)ViewState["dirID"];
            }
        }

        protected void btnProcess_Click(object sender, EventArgs e)
        {
            if (TaskTypeFile.HasFile)
            {
                if (TaskTypeFile.FileName.EndsWith(".xlsx"))
                {
                    try
                    {
                        string path = "c:\\temp\\" + TaskTypeFile.FileName;
                        TaskTypeFile.SaveAs(path);

                        List<DTO.TaskType> types = BLL.AdminManager.GetInstance().ImportTaskTypes(path, _dirID);
                        GridView1.DataSource = types;
                        GridView1.DataBind();

                        phImportTable.Visible = true;
                        lblFilename.Text = TaskTypeFile.FileName;

                        if (types.Where(x => x.Error).Count() == 0)
                        {
                            btnSave.Visible = true;
                            lblSaveError.Visible = false;
                            HttpContext.Current.Session["imported_tasktypes"] = types;
                        }
                        else
                        {
                            btnSave.Visible = false;
                            lblSaveError.Visible = true;
                            HttpContext.Current.Session.Remove("imported_tasktypes");
                        }
                    }
                    catch (Exception ex)
                    {
                        Utility.DisplayException(ex);
                    }
                }
                else
                {
                    phFileError.Controls.Add(new LiteralControl("<div style=\"color: red\">File does not have a .xlsx extension</div>"));
                    phImportTable.Visible = false;
                }
            }
            else
            {
                phFileError.Controls.Add(new LiteralControl("<div style=\"color: red\">No file specified</div>"));
                phImportTable.Visible = false;
            }

            lblResult.Text = string.Empty;
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            string status = e.Row.Cells[1].Text;

            if (status == "Fail")
            {
                e.Row.Style.Add("background-color", "#ffcccc");
            }
            else if (status != "Status")
            {
                if (e.Row.Cells[0].Text.Contains("Warning"))
                {
                    e.Row.Style.Add("background-color", "#ffffcc");
                }
                else
                {
                    e.Row.Style.Add("background-color", "#ccffcc");
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                List<DTO.TaskType> types = (List<DTO.TaskType>)HttpContext.Current.Session["imported_tasktypes"];
                BLL.AdminManager.GetInstance().SaveImportedTaskTypes(types, _dirID);
                HttpContext.Current.Session.Remove("imported_tasktypes");

                btnSave.Visible = false;
                lblResult.Text = "<b>Task Types Saved.</b>";
                HttpContext.Current.Session["adminTasks_refresh"] = true;
            }
            catch (Exception ex)
            {
                lblResult.Text = "<b>Save failed:</b> " + ex.Message;
            }
        }
    }
}