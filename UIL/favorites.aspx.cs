using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WALT.DTO;

namespace WALT.UIL
{
    public partial class favorites : System.Web.UI.Page
    {
        private ArrayList myIDs;
        private ArrayList myEditIDs;
        private Dictionary<string, Control> myControls;

        protected void Page_Load(object sender, EventArgs e)
        {
            myControls = new Dictionary<string,Control>();
            LoadData(!IsPostBack);
        }

        private void LoadData(bool refresh)
        {
            Dictionary<long, Favorite> favMap = null;

            if (refresh)
            {
                DTO.Team team = WALT.BLL.AdminManager.GetInstance().GetTeam();
                hdrComp.Visible = team != null && team.ComplexityBased;

                if (hdrComp.Visible)
                {
                    btnUpdTaskType.OnClientClick = "UpdateTaskType()";
                }
                else
                {
                    btnUpdTaskType.OnClientClick = "UpdateTaskType(); return false";
                }

                rowLinkBtns.Visible = true;
                hdrTitle.ColumnSpan = 2;
                btnAdd.Visible = true;
                btnUpdate.Visible = false;
                btnCancel.Visible = false;
                popupTaskType.Visible = false;

                myIDs = new ArrayList();
                myEditIDs = new ArrayList();
                favMap = new Dictionary<long, Favorite>();

                List<Favorite> favorites = WALT.BLL.TaskManager.GetInstance().GetFavorites();

                foreach (Favorite fav in favorites)
                {
                    myIDs.Add(fav.Id);
                    favMap.Add(fav.Id, fav);
                }

                ViewState["IDs"] = myIDs;
                ViewState["EditIDs"] = myEditIDs;
            }
            else
            {
                myIDs = (ArrayList)ViewState["IDs"];
                myEditIDs = (ArrayList)ViewState["EditIDs"];
            }

            myControls.Clear();
            phHeader.Controls.Clear();

            while (tblFavorites.Rows.Count > 1)
            {
                tblFavorites.Rows.RemoveAt(1);
            }

            foreach (long id in myIDs)
            {
                if (!myEditIDs.Contains(id))
                {
                    if (favMap != null)
                    {
                        AddRow(id, favMap[id], false, false);
                    }
                    else
                    {
                        AddRow(id, null, false, false);
                    }
                }
                else
                {
                    AddRow(id, null, true, false);
                }
            }
        }

        private void AddRow(long id, Favorite fav, bool edit, bool update)
        {
            TableRow tblRow;
            HiddenField hdnTTid = null;
            HiddenField hdnTTval = null;
            string idStr = id.ToString();

            if (!myControls.ContainsKey("hdnTaskTypeID_" + idStr))
            {
                hdnTTid = new HiddenField();
                hdnTTid.ID = "hdnTaskTypeID_" + idStr;
                myControls.Add(hdnTTid.ID, hdnTTid);
                phHeader.Controls.Add(hdnTTid);

                hdnTTval = new HiddenField();
                hdnTTval.ID = "hdnTaskTypeVal_" + idStr;
                myControls.Add(hdnTTval.ID, hdnTTval);
                phHeader.Controls.Add(hdnTTval);

                if (id < 0)
                {
                    hdnTTid.Value = "-1";
                }
            }
            else
            {
                hdnTTid = (HiddenField)myControls["hdnTaskTypeID_" + idStr];
                hdnTTval = (HiddenField)myControls["hdnTaskTypeVal_" + idStr];
            }

            if (update)
            {
                tblRow = (TableRow)myControls["row_" + idStr];

                while (tblRow.Cells.Count > 1)
                {
                    tblRow.Cells.RemoveAt(1);
                }
            }
            else
            {
                tblRow = new TableRow();
                tblRow.ID = "row_" + idStr;
                myControls.Add(tblRow.ID, tblRow);

                CheckBox chk = new CheckBox();
                chk.ID = "chk_" + idStr;
                chk.Visible = (id > 0);

                TableCell cell = CreateCell(chk);
                cell.ID = "cellChk_" + idStr;
                tblRow.Cells.Add(cell);
                myControls.Add(cell.ID, cell);
            }

            if (fav != null)
            {
                hdnTTid.Value = fav.TaskType != null ? fav.TaskType.Id.ToString() : "-1";
                hdnTTval.Value = fav.TaskType != null ? fav.TaskType.Title : string.Empty;

                tblRow.Cells.Add(CreateCell(fav.Title, idStr, 0));
                tblRow.Cells.Add(CreateTaskTypeCell(hdnTTval.Value, idStr));
                tblRow.Cells.Add(CreateCell(fav.Program != null ? fav.Program.Title : string.Empty, idStr, 2));
                tblRow.Cells.Add(CreateCell(fav.Hours > 0 ? fav.Hours.ToString() : string.Empty, idStr, 3, HorizontalAlign.Right));

                if (hdrComp.Visible)
                {
                    tblRow.Cells.Add(CreateCell(fav.Complexity != null ? fav.Complexity.Title : string.Empty, idStr, 4, HorizontalAlign.Right));
                    tblRow.Cells.Add(CreateRECell(fav.Estimate > 0 ? fav.Estimate.ToString() : string.Empty, idStr));
                }
                else
                {
                    tblRow.Cells.Add(CreateCell(fav.Estimate > 0 ? fav.Estimate.ToString() : string.Empty, idStr, 4, HorizontalAlign.Right));
                }

                tblRow.Cells.Add(CreateCell(fav.Template ? "Yes" : "No", idStr, 5, HorizontalAlign.Center));

                for (int i = 6; i < 13; i++)
                {
                    tblRow.Cells.Add(CreateHourCell(fav.PlanHours[i-6].ToString(), idStr, i));
                }
            }
            else if (!edit)
            {
                tblRow.Cells.Add(CreateCell(idStr, 0));
                tblRow.Cells.Add(CreateTaskTypeCell(idStr, 2));
                tblRow.Cells.Add(CreateCell(idStr, 2));
                tblRow.Cells.Add(CreateCell(idStr, 3, HorizontalAlign.Right));
                tblRow.Cells.Add(CreateCell(idStr, 4, HorizontalAlign.Right));

                if (hdrComp.Visible)
                {
                    tblRow.Cells.Add(CreateRECell(idStr));
                }

                tblRow.Cells.Add(CreateCell(idStr, 5, HorizontalAlign.Center));

                for (int i = 6; i < 13; i++)
                {
                    tblRow.Cells.Add(CreateHourCell(idStr, i));
                }
            }
            else
            {
                TextBox txtTitle = new TextBox();
                txtTitle.ID = "txtTitle_" + idStr;
                txtTitle.Width = 250;

                TableCell ttCell = CreateTaskTypeCell(idStr, 1);

                Button btn = new Button();
                btn.Text = "...";
                btn.OnClientClick = "return PopTaskType(" + idStr + ")";

                DropDownList dd;

                TextBox txtAllocate = new TextBox();
                txtAllocate.ID = "txtAllocate_" + idStr;
                txtAllocate.Width = 35;
                txtAllocate.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                txtAllocate.Attributes.Add("onchange", "ValidateHours(this, 0)");

                TextBox txtRE = new TextBox();
                txtRE.ID = "txtRE_" + idStr;
                txtRE.Width = 35;
                txtRE.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                txtRE.Attributes.Add("onchange", "ValidateHours(this, 0)");

                CheckBox chk = new CheckBox();
                chk.ID = "chkTemp_" + idStr;

                if (update)
                {
                    txtTitle.Text = ViewState["cell_" + idStr + "_0"].ToString();
                    dd = CreateProgramDD(id, ViewState["cell_" + idStr + "_2"].ToString());
                    txtAllocate.Text = ViewState["cell_" + idStr + "_3"].ToString();
                    txtRE.Text = ViewState["cell_" + idStr + "_4"].ToString();
                    chk.Checked = (ViewState["cell_" + idStr + "_5"].ToString() == "Yes");
                }
                else
                {
                    dd = CreateProgramDD(id, string.Empty);
                }

                tblRow.Cells.Add(CreateCell(txtTitle));
                tblRow.Cells.Add(ttCell);
                tblRow.Cells.Add(CreateTaskTypeCell(btn));
                tblRow.Cells.Add(CreateCell(dd));
                tblRow.Cells.Add(CreateCell(txtAllocate));

                if (hdrComp.Visible)
                {
                    tblRow.Cells.Add(CreateCell(CreateComplexityDD(idStr, txtRE.Text)));
                    tblRow.Cells.Add(CreateRECell(idStr));
                }
                else
                {
                    tblRow.Cells.Add(CreateCell(txtRE));
                }
                
                tblRow.Cells.Add(CreateCell(chk));

                for (int i = 0; i < 7; i++)
                {
                    TextBox txt = new TextBox();
                    txt.ID = "txtHours_" + idStr + "_" + i.ToString();
                    txt.Width = 26;
                    txt.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                    txt.Attributes.Add("onchange", "ValidateHours(this, 1)");

                    if (update)
                    {
                        txt.Text = ViewState["cell_" + idStr + "_" + (i + 6).ToString()].ToString();
                    }

                    tblRow.Cells.Add(CreateCell(txt));
                }
            }

            if (!update)
            {
                tblFavorites.Rows.Add(tblRow);
            }
        }

        private TableCell CreateCell(Control ctrl)
        {
            TableCell cell = new TableCell();
            cell.Controls.Add(ctrl);
            cell.HorizontalAlign = HorizontalAlign.Center;
            myControls.Add(ctrl.ID, ctrl);
            return cell;
        }

        private TableCell CreateCell(
            string text,
            string id,
            int col
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = text;
            ViewState[cell.ID] = text;
            return cell;
        }

        private TableCell CreateCell(
            string id,
            int col
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = (string)ViewState[cell.ID];
            return cell;
        }

        private TableCell CreateCell(
            string text,
            string id,
            int col,
            HorizontalAlign align
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = text;
            cell.HorizontalAlign = align;
            ViewState[cell.ID] = text;
            return cell;
        }

        private TableCell CreateCell(
            string id,
            int col,
            HorizontalAlign align
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = (string)ViewState[cell.ID];
            cell.HorizontalAlign = align;
            return cell;
        }

        private TableCell CreateHourCell(
            string text,
            string id,
            int col
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = text == "0" ? string.Empty : text;
            cell.CssClass = "number";
            ViewState[cell.ID] = cell.Text;
            return cell;
        }

        private TableCell CreateHourCell(
            string id,
            int col
        )
        {
            TableCell cell = new TableCell();
            cell.ID = "cell_" + id + "_" + col.ToString();
            cell.Text = (string)ViewState[cell.ID];
            cell.CssClass = "number";
            return cell;
        }

        private TableCell CreateTaskTypeCell(string text, string id)
        {
            TableCell cell = new TableCell();
            cell.ColumnSpan = 2;
            cell.Text = "<div id=\"ttTitle_" + id + "\">" + text + "</div>";
            return cell;
        }

        private TableCell CreateTaskTypeCell(string id, int colspan)
        {
            string text = HttpContext.Current.Request[GetControlPrefix() + "hdnTaskTypeVal_" + id] != null ?
                HttpContext.Current.Request[GetControlPrefix() + "hdnTaskTypeVal_" + id].ToString() : string.Empty;

            TableCell cell = new TableCell();
            cell.ColumnSpan = colspan;
            cell.Text = "<div id=\"ttTitle_" + id + "\">" + text + "</div>";
            
            if (colspan == 1)
            {
                cell.Style.Add(HtmlTextWriterStyle.BorderWidth, "1px 0px 1px 1px");
            }

            return cell;
        }

        private TableCell CreateRECell(string text, string id)
        {
            TableCell cell;

            if (myControls.ContainsKey("cellRE_" + id))
            {
                cell = (TableCell)myControls["cellRE_" + id];
            }
            else
            {
                cell = new TableCell();
                cell.CssClass = "number";
                cell.EnableViewState = false;
                myControls.Add("cellRE_" + id, cell);
            }

            cell.Text = "<div id=\"reDiv_" + id + "\">" + text +
                "</div><input type=\"hidden\" id=\"re_" + id + "\" name=\"re_" + id + "\" value=\"" + text + "\" />";
            
            return cell;
        }

        private TableCell CreateRECell(string id)
        {
            TableCell cell;

            if (myControls.ContainsKey("cellRE_" + id))
            {
                cell = (TableCell)myControls["cellRE_" + id];
            }
            else
            {
                string val = HttpContext.Current.Request["re_" + id];

                cell = new TableCell();
                cell.CssClass = "number";
                cell.EnableViewState = false;
                cell.Text = "<div id=\"reDiv_" + id + "\">" + val +
                    "</div><input type=\"hidden\" id=\"re_" + id + "\" name=\"re_" + id + "\" value=\"" + val + "\" />";

                myControls.Add("cellRE_" + id, cell);
            }

            return cell;
        }

        private TableCell CreateTaskTypeCell(Control ctrl)
        {
            TableCell cell = new TableCell();
            cell.Controls.Add(ctrl);
            cell.Style.Add(HtmlTextWriterStyle.BorderWidth, "1px 1px 1px 0px");
            cell.HorizontalAlign = HorizontalAlign.Right;
            return cell;
        }

        private string GetControlPrefix()
        {
            int i = 5;
            int count = HttpContext.Current.Request.Params.Keys.Count;

            while (i < count)
            {
                if (HttpContext.Current.Request.Params.Keys[i].Contains("prefix"))
                {
                    string pre = HttpContext.Current.Request.Params.Keys[i];
                    return pre.Substring(0, pre.Length - 6);
                }

                i++;
            }

            return string.Empty;
        }

        protected void lnkCreate_Click(object sender, EventArgs e)
        {
            foreach (long id in myIDs)
            {
                if (((CheckBox)myControls["chk_" + id.ToString()]).Checked)
                {
                    Response.Redirect("/Task/TaskForm.aspx?favID=" + id.ToString());
                    return;
                }
            }
        }

        protected void lnkEdit_Click(object sender, EventArgs e)
        {
            ClearEdits();

            foreach (long id in myIDs)
            {
                if (((CheckBox)myControls["chk_" + id.ToString()]).Checked)
                {
                    myEditIDs.Add(id);
                    AddRow(id, null, true, true);
                }
            }

            SetEditMode();
        }

        protected void lnkDelete_Click(object sender, EventArgs e)
        {
            List<long> delIDs = new List<long>();

            foreach (long id in myIDs)
            {
                if (((CheckBox)myControls["chk_" + id.ToString()]).Checked)
                {
                    if (id > 0)
                    {
                        delIDs.Add(id);
                    }
                    else
                    {
                        TableRow tblRow = (TableRow)myControls["row_" + id.ToString()];
                        tblFavorites.Rows.Remove(tblRow);
                        myIDs.Remove(id);
                    }
                }
            }

            if (delIDs.Count > 0)
            {
                try
                {
                    WALT.BLL.TaskManager.GetInstance().DeleteFavorites(delIDs);
                    LoadData(true);
                }
                catch (Exception ex)
                {
                    WALT.UIL.Utility.DisplayErrorMessage(ex.Message);
                }
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            long id = -myIDs.Count - 1;
            AddRow(id, null, true, false);

            myIDs.Add(id);
            myEditIDs.Add(id);
            SetEditMode();
        }

        private void SetEditMode()
        {
            if (myEditIDs.Count > 0)
            {
                rowLinkBtns.Visible = false;
                hdrTitle.ColumnSpan = 1;
                btnAdd.Visible = false;
                btnUpdate.Visible = true;
                btnCancel.Visible = true;

                phDialogScript.Visible = true;
                popupTaskType.Visible = true;
                treeTaskType.LoadTaskTypes(null, false);

                foreach (long id in myIDs)
                {
                    ((TableCell)myControls["cellChk_" + id.ToString()]).Visible = false;
                }
            }
        }

        protected void btnUpdTaskType_Click(object sender, EventArgs e)
        {
            string favID = HttpContext.Current.Request["favID"];
            CreateComplexityDD(favID, string.Empty);
            CreateRECell(string.Empty, favID);
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            List<Favorite> favorites = new List<Favorite>();

            foreach (long id in myEditIDs)
            {
                string idStr = id.ToString();
                Favorite fav = new Favorite();

                if (id > 0)
                {
                    fav.Id = id;
                }

                fav.Title = ((TextBox)myControls["txtTitle_" + idStr]).Text;

                string ttID = ((HiddenField)myControls["hdnTaskTypeID_" + idStr]).Value;

                if (ttID != "-1")
                {
                    fav.TaskType = new TaskType();
                    fav.TaskType.Id = Convert.ToInt64(ttID);
                }

                string progID = ((DropDownList)myControls["ddProgram_" + idStr]).SelectedValue;

                if (progID != string.Empty)
                {
                    fav.Program = new Program();
                    fav.Program.Id = Convert.ToInt64(progID);
                }

                double hours = -1;
                double.TryParse(((TextBox)myControls["txtAllocate_" + idStr]).Text, out hours);
                fav.Hours = hours;

                if (hdrComp.Visible)
                {
                    string compID = ((DropDownList)myControls["ddComps_" + idStr]).SelectedValue;
                    fav.Complexity = new Complexity();
                    fav.Complexity.Id = Convert.ToInt64(compID);
                    fav.Estimate = -1;
                }
                else
                {
                    hours = -1;
                    double.TryParse(((TextBox)myControls["txtRE_" + idStr]).Text, out hours);
                    fav.Estimate = hours;
                }

                fav.Template = ((CheckBox)myControls["chkTemp_" + idStr]).Checked;

                for (int i = 0; i < 7; i++)
                {
                    hours = 0;
                    double.TryParse(((TextBox)myControls["txtHours_" + idStr + "_" + i.ToString()]).Text, out hours);
                    fav.PlanHours.Add(i, hours);
                }

                favorites.Add(fav);
            }

            try
            {
                WALT.BLL.TaskManager.GetInstance().SaveFavorites(favorites);
                LoadData(true);
            }
            catch (Exception ex)
            {
                WALT.UIL.Utility.DisplayErrorMessage(ex.Message);
                SetEditMode();
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearEdits();
        }
        
        private void ClearEdits()
        {
            foreach (long id in myEditIDs)
            {
                string idStr = id.ToString();
                TableRow tblRow = (TableRow)myControls["row_" + idStr];

                if (id < 0)
                {
                    tblFavorites.Rows.Remove(tblRow);
                    myIDs.Remove(id);
                }
                else
                {
                    AddRow(id, null, false, true);
                }

                if (hdrComp.Visible)
                {
                    Label script = (Label)myControls["compScript_" + idStr];
                    myControls.Remove("compScript_" + idStr);
                    phCompScript.Controls.Remove(script);
                    script.Dispose();
                }
            }

            myEditIDs.Clear();

            foreach (long id in myIDs)
            {
                ((TableCell)myControls["cellChk_" + id.ToString()]).Visible = true;
            }

            rowLinkBtns.Visible = true;
            hdrTitle.ColumnSpan = 2;
            btnAdd.Visible = true;
            btnUpdate.Visible = false;
            btnCancel.Visible = false;
            phDialogScript.Visible = false;
            popupTaskType.Visible = false;
        }

        private DropDownList CreateProgramDD(long id, string selected)
        {
            string selID = string.Empty;
            ArrayList programs;
            ArrayList progIDs;
            DropDownList dd = new DropDownList();
            dd.ID = "ddProgram_" + id.ToString();

            dd.Items.Add(new ListItem());

            if (ViewState["programs"] == null)
            {
                programs = new ArrayList();
                progIDs = new ArrayList();

                List<Program> progList = WALT.BLL.AdminManager.GetInstance().GetProgramList();

                foreach (Program prog in progList)
                {
                    programs.Add(prog.Title);
                    progIDs.Add(prog.Id);
                    dd.Items.Add(new ListItem(prog.Title, prog.Id.ToString()));

                    if (selected == prog.Title)
                    {
                        selID = prog.Id.ToString();
                    }
                }

                ViewState["programs"] = programs;
                ViewState["progIDs"] = progIDs;
            }
            else
            {
                int i = 0;
                programs = (ArrayList)ViewState["programs"];
                progIDs = (ArrayList)ViewState["progIDs"];

                foreach (string prog in programs)
                {
                    dd.Items.Add(new ListItem(prog, progIDs[i].ToString()));

                    if (selected == prog)
                    {
                        selID = progIDs[i].ToString();
                    }

                    i++;
                }
            }

            if (selID != string.Empty)
            {
                dd.SelectedValue = selID;
            }

            return dd;
        }

        private DropDownList CreateComplexityDD(string id, string selected)
        {
            DropDownList ddComps;
            Label script;
            string idStr = id.Replace('-', 'n');

            if (myControls.ContainsKey("ddComps_" + id))
            {
                script = (Label)myControls["compScript_" + id];
                ddComps = (DropDownList)myControls["ddComps_" + id];
                ddComps.Items.Clear();
            }
            else
            {
                ddComps = new DropDownList();
                ddComps.ID = "ddComps_" + id;

                script = new Label();
                script.ID = "compScript_" + id;
                myControls.Add(script.ID, script);
                phCompScript.Controls.Add(script);
            }

            string ttID = ((HiddenField)myControls["hdnTaskTypeID_" + id]).Value;

            if (ttID == string.Empty)
            {
                ttID = HttpContext.Current.Request[GetControlPrefix() + "hdnTaskTypeID_" + id];
            }

            if (ttID != "-1")
            {
                script.Text = "\n<script type=\"text/javascript\">\n\tvar compRE_" + idStr + "_0 = '';\n";
                ddComps.Attributes.Add("onchange", "UpdateRE(" + id + ", '" + idStr + "', this[this.selectedIndex].value)");
                ddComps.Items.Add(new ListItem(string.Empty, "0"));

                List<DTO.Complexity> comps = WALT.BLL.TaskManager.GetInstance().GetComplexityList(Convert.ToInt64(ttID), true);

                foreach (Complexity comp in comps)
                {
                    ddComps.Items.Add(new ListItem(comp.Title, comp.Id.ToString()));
                    script.Text += "\tvar compRE_" + idStr + "_" + comp.Id.ToString() + " = '" + comp.Hours.ToString() + "';\n";

                    if (selected == comp.Title)
                    {
                        ddComps.SelectedValue = comp.Id.ToString();
                    }
                }

                script.Text += "</script>\n";
                script.Visible = true;
                ddComps.Visible = true;
            }
            else
            {
                script.Text = string.Empty;
                script.Visible = false;
                ddComps.Visible = false;
            }

            return ddComps;
        }
    }
}