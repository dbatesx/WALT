using System;
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
using System.IO;
using System.Data.OleDb;

namespace WALT.UIL.Admin
{
    public partial class Programs : System.Web.UI.Page
    {
        List<DTO.Program> _programs;

        protected void Page_Load(object sender, EventArgs e)
        {
            _programs = (List<DTO.Program>)Session["admin_programs"];

            if (!IsPostBack)
            {
                LoadData();

                if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.REPORT_MANAGE))
                {
                    panelAddProgram.Enabled = true;
                }
                else
                {
                    panelAddProgram.Enabled = false;
                }
            }
        }

        void LoadData()
        {
            GridView1.PageIndex = 0;
            _programs = BLL.AdminManager.GetInstance().GetProgramList();
            Session["admin_programs"] = _programs;
            Bind();
        }

        void Bind()
        {
            if (BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.REPORT_MANAGE))
            {
                GridView1.AutoGenerateEditButton = true;
            }
            else
            {
                GridView1.AutoGenerateEditButton = false;
            }

            GridView1.DataSource = _programs;
            GridView1.DataBind();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            Bind();
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            TextBox tb1 = (TextBox)GridView1.Rows[e.RowIndex].Cells[1].Controls[0];
            CheckBox cb1 = (CheckBox)GridView1.Rows[e.RowIndex].Cells[2].Controls[0];

            _programs[e.RowIndex].Title = tb1.Text;
            _programs[e.RowIndex].Active = cb1.Checked;

            SaveProgram(e.RowIndex);

            GridView1.EditIndex = -1;
            Bind();
        }

        void SaveProgram(int index)
        {
            try
            {
                DTO.Program program = _programs[index];
                BLL.AdminManager.GetInstance().SaveProgram(program);
                _programs[index] = program;
                Utility.DisplayInfoMessage("Program saved.");
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            Bind();
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            Bind();
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            DTO.Program p = new WALT.DTO.Program();
            p.Title = TextBox1.Text;

            try
            {
                BLL.AdminManager.GetInstance().SaveProgram(p);
                Utility.DisplayInfoMessage("Program saved.");
                LoadData();                
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }
        //update the excel template
        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                string sourcFilePath = Server.MapPath("/Task/BlankTemplate.xlsx");

                string destFilePath = sourcFilePath.Remove(sourcFilePath.IndexOf("BlankTemplate.xlsx"));
                destFilePath += "TaskTemplate.xlsx";

                File.Copy(sourcFilePath, destFilePath, true);


                string strExcelConn = "Provider=Microsoft.ACE.OLEDB.12.0;" +
                "Data Source=" + destFilePath + ";Excel 12.0;HDR=YES;";

                OleDbConnection connExcel = new OleDbConnection(strExcelConn);
                OleDbCommand cmdExcel = new OleDbCommand();

                try
                {
                    cmdExcel.Connection = connExcel;

                    Dictionary<long, string> programs = BLL.AdminManager.GetInstance().GetProgramDictionary();

                    foreach (KeyValuePair<long, string> progPairs in programs)
                    {
                        cmdExcel.CommandText = "INSERT INTO [Sheet2$] (Programs) values ('" + progPairs.Value + "')";

                        connExcel.Open();
                        cmdExcel.ExecuteNonQuery();
                        connExcel.Close();
                    }
                }

                catch (Exception ex)
                {
                    Utility.DisplayException(ex);
                }

                finally
                {
                    cmdExcel.Dispose();
                    connExcel.Dispose();
                }
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }
    }
}
