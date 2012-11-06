using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WALT.UIL.DataSources;

using AjaxControlToolkit;

namespace WALT.UIL.Controls
{
    public partial class TaskGrid : System.Web.UI.UserControl
    {
        private int _taskCount = 0;
        private Dictionary<string, string> _profilePreferences;
        private DTO.Task.StatusEnum? _taskStat;
        private DTO.Profile _assignee;
        private DTO.Profile _owner;
        private Dictionary<DTO.Task.ColumnEnum, string> _filters;
        private bool _dataBound;
        private TaskListDataSource _datasource;
        protected List<GridColList> ListDataItems = new List<GridColList>();

        #region >>> Properties
        public Dictionary<string, string> ProfilePreferences { get { return this._profilePreferences; } set { this._profilePreferences = value; } }
        #endregion

        public String gvSortDirection
        {
            get { return ViewState["SortDirection"] as String ?? "ASC"; }
            set { ViewState["SortDirection"] = value; }
        }

        public String gvSortExpression
        {
            get { return ViewState["SortExpression"] as String ?? "Title"; }
            set { ViewState["SortExpression"] = value; }
        }

        public String sortClicked
        {
            get { return ViewState["SortClicked"] as String ?? ""; }
            set { ViewState["SortClicked"] = value; }
        }

        public String pageSize
        {
            get { return ViewState["pageSize"] as String ?? "25"; }
            set { ViewState["pageSize"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            taskGridView.PageSize = Convert.ToInt32(pageSize);
            _dataBound = false;

            if (HttpContext.Current.Session["col_order"] != null)
            {
                ListDataItems = (List<GridColList>)HttpContext.Current.Session["col_order"];
            }
            else
            {
                GetListData();
            }

            if (!Page.IsPostBack)
            {

                BindReorderList();
            }
        }

        protected void GetListData()
        {
            List<GridColList> firstHalf = new List<GridColList>();
            List<GridColList> secondHalf = new List<GridColList>();
            GridColList colList = null;
            List<string> colToShow = new List<string>();


            if (ProfilePreferences != null && ProfilePreferences.ContainsKey("TaskQueueGridColumns"))
            {
                XElement prefCols = XElement.Parse(ProfilePreferences["TaskQueueGridColumns"]);

                foreach (XElement col in prefCols.Elements("Column"))
                {
                    if (col.Element("Visible").Value.ToString().Equals("true"))
                    {
                        colToShow.Add(col.Element("Name").Value);
                    }
                }
            }
            else
            {
                colToShow.Add("Inst");
                colToShow.Add("Base");
                colToShow.Add("Title");
                colToShow.Add("Status");
                colToShow.Add("Program");
                colToShow.Add("Owner");
                colToShow.Add("Assignee");
                colToShow.Add("TaskType");
                colToShow.Add("Source");
                colToShow.Add("Owner Comments");
                colToShow.Add("Exit Criteria");
            }

            DataControlFieldCollection columns = taskGridView.Columns;

            if (columns != null)
            {
                int index = 0;

                foreach (DataControlField field in columns)
                {
                    if (!string.IsNullOrEmpty(field.HeaderText))
                    {
                        string header = field.HeaderText;

                        if (header.Contains("<br/>"))
                            header = header.Replace("<br/>", " ");

                        if (colToShow.Contains(field.HeaderText) && field.Visible)
                        {
                            colList = new GridColList();

                            colList.ItemName = header;

                            colList.Visible = true;

                            firstHalf.Add(colList);
                            index++;
                        }
                        else
                        {
                            colList = new GridColList();

                            colList.ItemName = header;

                            colList.Visible = false;

                            secondHalf.Add(colList);
                        }
                    }
                }

                foreach (GridColList colItem in firstHalf)
                {
                    ListDataItems.Add(colItem);
                }

                colList = new GridColList();
                colList.ItemName = "Not Visible";
                ListDataItems.Add(colList);

                foreach (GridColList colItem in secondHalf)
                {
                    ListDataItems.Add(colItem);
                }
            }


            HttpContext.Current.Session["col_order"] = ListDataItems;
        }
        
        protected void rlItemList_ItemReorder(object sender, ReorderListItemReorderEventArgs e)
        {
            saveColPref.Visible = true;

            int hideColIndex = -1;

            var OldOrder = e.OldIndex;
            var NewOrder = e.NewIndex;

            GridColList reorderItem = ListDataItems[OldOrder];

            ListDataItems.RemoveAt(OldOrder);
            ListDataItems.Insert(NewOrder, reorderItem);

            for (int i = 0; i < ListDataItems.Count(); i++)
            {
                if (ListDataItems[i].ItemName.Equals("Not Visible"))
                {
                    hideColIndex = i;
                    break;
                }
            }

            if (hideColIndex != -1)
            {
                if (NewOrder < hideColIndex)
                    ListDataItems[NewOrder].Visible = true;
                else
                    ListDataItems[NewOrder].Visible = false;
                BindReorderList();
            }
        }


        protected void BindReorderList()
        {
            rlItemList.DataSource = ListDataItems;
            rlItemList.DataBind();                       
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        /*To be used if AjaxControlToolkit is not allowed
        void setColPrefBtn_Click(object sender, EventArgs e)
        {
            string csvStr = string.Empty;

            foreach (KeyValuePair<CheckBox, DataControlField> pair in _checkboxCollection.Values)
            {
                int hash = Int32.Parse(pair.Key.ID.Split('_')[1]);
                _checkboxCollection[hash].Value.Visible = pair.Key.Checked;

                if (pair.Key.Checked)
                    csvStr += pair.Value.HeaderText + ",";
            }

            WALT.BLL.ProfileManager.GetInstance().SavePreference("TaskQueueGridCols", csvStr);
        }

        void colSelectUpdateBtn_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<CheckBox, DataControlField> pair in _checkboxCollection.Values)
            {
                int hash = Int32.Parse(pair.Key.ID.Split('_')[1]);
                _checkboxCollection[hash].Value.Visible = pair.Key.Checked;
            }             
        }
       */

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            BindData();
            

            /*To be used if AjaxControlToolkit is not allowed
           DataControlFieldCollection columns = this.taskGridView.Columns;

           if (columns != null)
           {
               int index = 0;

               foreach (DataControlField field in columns)
               {
                   int hashCode = (field.HeaderText + index.ToString()).GetHashCode();
                   CheckBox checkBox = _checkboxCollection[hashCode].Key;
                   checkBox.Checked = field.Visible;
                   // checkBox.AutoPostBack = true;

                   index++;
               }
           }
             */
        }

        private void BindData()
        {
            if (!_dataBound)
            {
                DataControlFieldCollection columns = taskGridView.Columns.CloneFields();
                taskGridView.Columns.Clear();

                //adds the first col (checkbox)
                taskGridView.Columns.Add(columns[0]);
                columns.RemoveAt(0);

                foreach (GridColList colItem in ListDataItems)
                {
                    foreach (DataControlField field in columns)
                    {
                        string header = field.HeaderText;

                        if (header.Contains("<br/>"))
                            header = header.Replace("<br/>", " ");

                        if (header.Equals(colItem.ItemName))
                        {
                            if (!colItem.Visible)
                                field.Visible = false;

                            taskGridView.Columns.Add(field);

                            break;
                        }
                    }

                }

                this.taskGridView.DataBind();

                if (taskGridView.Rows.Count > 0)
                {
                    emptyDataPlaceHolder.Visible = false;
                    taskQueuePlaceHolder.Visible = true;
                }
                else
                {
                    _taskCount = 0;
                    emptyDataPlaceHolder.Visible = true;
                    taskQueuePlaceHolder.Visible = false;
                }

                _dataBound = true;
            }
        }

        /* To be used if AjaxControlToolkit is not allowed
        //protected override void OnLoad(EventArgs e)
        //{   
        //    
        //    foreach (KeyValuePair<CheckBox, DataControlField> pair in _checkboxCollection.Values)
        //    {
        //        pair.Key.CheckedChanged += new EventHandler(checkBox_CheckedChanged);
        //    }
        //     

        //    base.OnLoad(e);
        //}
       */

        public void InitControl(DTO.Profile assignee, DTO.Profile owner, DTO.Task.StatusEnum? taskStatus, Dictionary<DTO.Task.ColumnEnum, string> filters)
        {
            _taskStat = taskStatus;
            _assignee = assignee;
            _owner = owner;
            _filters = filters;

            SetDataSource();
        }

        private void SetDataSource()
        {
            this.taskGridView.PageIndex = 0;

            if (sortClicked.Equals("true"))
            {               
                bool sortAscending = false;

                if (gvSortDirection.Equals("ASC"))
                    sortAscending = true;

                DTO.Task.ColumnEnum sortCol = (DTO.Task.ColumnEnum)Enum.Parse(typeof(DTO.Task.ColumnEnum), gvSortExpression, true);

                _datasource = new TaskListDataSource(_assignee, _owner, _taskStat, sortCol, sortAscending, false, _filters);
            }
            else
            {
                _datasource = new TaskListDataSource(_assignee, _owner, _taskStat, DTO.Task.ColumnEnum.TITLE, true, false, _filters);
                sortClicked = "true";
            }

            this.taskGridView.DataSource = _datasource;
        }

        protected void taskGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            SetDataSource();

            this.taskGridView.PageIndex = e.NewPageIndex;
        }

        protected void taskGridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (e.SortExpression != "" & e.SortExpression != null)
            {
                if (gvSortExpression == e.SortExpression)
                    gvSortDirection = GetNewSortDirection();
                else
                    gvSortDirection = "ASC";
                gvSortExpression = e.SortExpression;
            }
            sortClicked = "true";

            SetDataSource();            
        }

        protected void taskGridView_DataBound(object sender, EventArgs e)
        {
            int begCount = (taskGridView.PageIndex * taskGridView.PageSize) + 1;
            int endCount = (taskGridView.PageIndex * taskGridView.PageSize) + _taskCount;

            lblRowCount.Text = "Tasks " + begCount + " - " + endCount + " of total " + _datasource.GetCount().ToString();
        }

        protected void taskGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {           
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                for (int i = 0; i < this.taskGridView.Columns.Count; i++)
                {
                    DataControlField field = this.taskGridView.Columns[i];

                    if (field.SortExpression.Equals(gvSortExpression) && !field.SortExpression.Equals("Title"))
                    {
                        e.Row.Cells[i].CssClass = "sortCell";
                    }
                    else if (field.SortExpression.Equals(gvSortExpression) && field.SortExpression.Equals("Title"))
                    {
                        e.Row.Cells[i].CssClass = "sortCellTitle";
                    }
                }

                _taskCount++;
            }

            if (sender != null && e.Row.RowType == DataControlRowType.Header)
            {
                if (!string.IsNullOrEmpty(sortClicked) && sortClicked.Equals("true"))
                {
                    for (int i = 0; i < taskGridView.Columns.Count; i++)
                    {
                        DataControlField field = taskGridView.Columns[i];

                        if (field.SortExpression.Equals(gvSortExpression) && sortClicked.Equals("true"))
                        {
                            Image filterIcon = new Image();
                            if (gvSortDirection.Equals("ASC"))
                            {
                                filterIcon.ImageUrl = "~/css/images/arrow_down_white.png";
                            }
                            else
                            {
                                filterIcon.ImageUrl = "~/css/images/arrow_up_white.png";
                            }
                            filterIcon.Style[HtmlTextWriterStyle.MarginLeft] = "5px";

                            Literal headerText = new Literal();
                            headerText.Text = field.HeaderText;

                            PlaceHolder panel = new PlaceHolder();
                            panel.Controls.Add(headerText);
                            panel.Controls.Add(filterIcon);

                            e.Row.Cells[i].Controls[0].Controls.Add(panel);

                            break;
                        }
                    }
                }
            }
        }

        protected void PageSizeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            pageSize = PageSizeDropDownList.SelectedValue;
            taskGridView.PageSize = Convert.ToInt32(pageSize);
            taskGridView.PageIndex = 0;
            sortClicked = "false";

            SetDataSource();
        }

        private String GetSortDirection()
        {
            String newSortDirection = String.Empty;
            switch (gvSortDirection)
            {
                case "DESC":
                    newSortDirection = "DESC";
                    break;
                case "ASC":
                    newSortDirection = "ASC";
                    break;
                default:
                    newSortDirection = "ASC";
                    break;
            }
            return newSortDirection;
        }

        private String GetNewSortDirection()
        {
            String newSortDirection = String.Empty;
            switch (gvSortDirection)
            {
                case "DESC":
                    newSortDirection = "ASC";
                    break;
                case "ASC":
                    newSortDirection = "DESC";
                    break;
                default:
                    newSortDirection = "ASC";
                    break;
            }
            return newSortDirection;
        }

        protected string GetId(object obj)
        {
            string val = string.Empty;

            DTO.Task taskItem = (DTO.Task)obj;
            val = taskItem.Id.ToString();

            return val;
        }

        protected string GetBaseIconUrl(bool baseTask)
        {
            string result = string.Empty;

            if (baseTask) result = "~/css/images/base_task_icon.png";
            else result = "~/css/images/parent_task_icon.png";

            return result;
        }

        protected string GetDate(DateTime? date)
        {
            string result = string.Empty;

            if (date.HasValue) 
            {
                result = date.Value.ToShortDateString();
            }

            return result;
        }

        protected string GetDateTime(DateTime? date)
        {
            string result = string.Empty;

            if (date.HasValue)
            {
                result = Utility.ConvertToLocal(date.Value).ToString();
            }

            return result;
        }

        protected string GetTaskUrl(object obj)
        {
            string val = string.Empty;

            DTO.Task taskItem = (DTO.Task)obj;
            val = "<a  href=\"/Task/ViewTask.aspx?id=" + taskItem.Id + "\" >" + taskItem.Title + "</a>";

            return val;
        }

        protected string GetInstantiatedIconUrl(bool instantiated)
        {
            string result = string.Empty;

            if (instantiated) result = "~/css/images/instantiated_task_icon.png";
            else result = "~/css/images/uninstantiated_task_icon.png";

            return result;
        }

        protected void saveColPref_Click(object sender, EventArgs e)
        {
            XElement columns = new XElement("ColumnCollection");

            foreach (GridColList item in ListDataItems)
            {
                if (!item.ItemName.Equals("Not Visible") && item.Visible)
                {
                    columns.Add(new XElement("Column",
                        new XElement("Name", item.ItemName),
                         new XElement("Visible", item.Visible)));
                }
            }
            try
            {
                WALT.BLL.ProfileManager.GetInstance().SavePreference("TaskQueueGridColumns", columns.ToString());
                prefSaveLabel.ForeColor = System.Drawing.Color.Green;
                saveColPref.Visible = false;
                prefSaveLabel.Text = "Preference was saved!";
            }
            catch
            {
                prefSaveLabel.ForeColor = System.Drawing.Color.Red;
                prefSaveLabel.Text = "Error";
            }
        }

        public List<long> GetSelectedTaskIDs()
        {
            BindData();

            List<long> ids = new List<long>();
            int count = taskGridView.Rows.Count;

            for (int i = 0; i < count; i++)
            {
                CheckBox chk = (CheckBox)taskGridView.Rows[i].Cells[0].Controls[1];

                if (HttpContext.Current.Request[chk.ClientID.Replace('_', '$')] == "on")
                {
                    ids.Add((long)taskGridView.DataKeys[i].Value);
                }
            }

            return ids;
        }

        public List<DTO.Task> GetSelectedTasks()
        {
            BindData();

            List<DTO.Task> tasks = new List<DTO.Task>();
            int count = taskGridView.Rows.Count;

            for (int i = 0; i < count; i++)
            {
                CheckBox chk = (CheckBox)taskGridView.Rows[i].Cells[0].Controls[1];

                if (HttpContext.Current.Request[chk.ClientID.Replace('_', '$')] == "on")
                {
                    tasks.Add(_datasource.GetTask(i));
                }
            }

            return tasks;
        }

        /*To be used if AjaxControlToolkit is not allowed
           void checkBox_CheckedChanged(object sender, EventArgs e)
           {     
               CheckBox checkBox = ((CheckBox)sender);

               int hash = Int32.Parse(checkBox.ID.Split('_')[1]);

               _checkboxCollection[hash].Value.Visible = checkBox.Checked;


               if (ColumnsChanged != null)
               {
                   ColumnsChanged(this, null);
               }

           }
         */

    }
}