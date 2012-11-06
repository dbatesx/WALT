using System;
using System.Collections;
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
using WALT.DTO;

namespace WALT.UIL.Controls
{
    public partial class TreeSelector : System.Web.UI.UserControl
    {
        private Unit _height;
        public Unit Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private Unit _width;
        public Unit Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private Unit _descWidth;
        public Unit DescWidth
        {
            get { return _descWidth; }
            set { _descWidth = value; }
        }

        private bool _loadDesc;
        public bool LoadDesc
        {
            get { return _loadDesc; }
            set { _loadDesc = value; }
        }

        private bool _showScript;
        public bool ShowScript
        {
            get { return _showScript; }
            set { _showScript = value; }
        }

        List<long> myIDs;

        protected void Page_Load(object sender, EventArgs e)
        {
            treePanel.Height = _height;
            treePanel.Width = _width;
            cellTree.Width = _width.ToString();
            treePanel.Width = _width;
            cellDesc.Visible = _loadDesc || phComplexity.Visible;
            cellDesc.Width = _descWidth.ToString();
            phTreeScript.Visible = _showScript;
        }

        public void LoadTaskTypes(Team team, bool loadComplexity)
        {
            List<TaskType> taskTypes;

            if (team == null)
            {
                taskTypes = WALT.BLL.AdminManager.GetInstance().GetTaskTypeList(false, true, true);
            }
            else
            {
                taskTypes = WALT.BLL.AdminManager.GetInstance().GetTaskTypeList(team, false, true, true);
            }

            treeScript.Text = "\n<script type=\"text/javascript\">\n";

            myIDs = new List<long>();
            selNode.Value = "-1";
            tree.Nodes.Clear();            

            foreach (TaskType tt in taskTypes)
            {
                tree.Nodes.Add(CreateNode(tt));
            }

            if (loadComplexity)
            {
                phComplexity.Visible = true;

                List<Complexity> comps;

                if (team == null)
                {
                    comps = WALT.BLL.TaskManager.GetInstance().GetComplexityList(team, true);
                }
                else
                {
                    comps = WALT.BLL.TaskManager.GetInstance().GetComplexityList(true);
                }

                Dictionary<long, int> ttMap = new Dictionary<long, int>();

                foreach (Complexity comp in comps)
                {
                    if (myIDs.Contains(comp.TaskType.Id))
                    {
                        int idx = 0;

                        if (!ttMap.ContainsKey(comp.TaskType.Id))
                        {
                            treeScript.Text += "var compID_" + comp.TaskType.Id.ToString() + " = new Array();\n" +
                                "var compTitle_" + comp.TaskType.Id.ToString() + " = new Array();\n" +
                                "var compRE_" + comp.TaskType.Id.ToString() + " = new Array();\n";

                            ttMap.Add(comp.TaskType.Id, idx);
                        }
                        else
                        {
                            ttMap[comp.TaskType.Id]++;
                            idx = ttMap[comp.TaskType.Id];
                        }

                        string arrIdx = comp.TaskType.Id.ToString() + "[" + idx.ToString() + "] = ";
                        treeScript.Text += "compID_" + arrIdx + comp.Id.ToString() + ";\n" +
                            "compTitle_" + arrIdx + "'" + comp.Title + "';\n" +
                            "compRE_" + arrIdx + comp.Hours + ";\n";
                    }
                }
            }
            else
            {
                phComplexity.Visible = false;
            }

            if (loadComplexity || _loadDesc)
            {
                treeScript.Text += "</script>\n";
                cellDesc.Visible = true;
            }
            else
            {
                treeScript.Text = string.Empty;
            }

            if (_showScript)
            {
                treeScript.Text += "<input type=\"hidden\" id=\"complexityTree\" name=\"complexityTree\" value=\"" +
                    (loadComplexity ? this.ID : string.Empty) + "\" />\n";
            }

            tree.CollapseAll();
        }

        public void LoadUnplannedCodes(Team team)
        {
            List<UnplannedCode> codes;

            if (team == null)
            {
                codes = WALT.BLL.AdminManager.GetInstance().GetUnplannedCodeList();
            }
            else
            {
                codes = WALT.BLL.AdminManager.GetInstance().GetUnplannedCodeList(team, true, true);
            }

            treeScript.Text = "\n<script type=\"text/javascript\">\n";

            selNode.Value = "-1";
            tree.Nodes.Clear();

            foreach (UnplannedCode code in codes)
            {
                tree.Nodes.Add(CreateNode(code));
            }

            if (_loadDesc)
            {
                treeScript.Text += "</script>\n";
            }
            else
            {
                treeScript.Text = string.Empty;
            }

            tree.CollapseAll();
        }

        public void LoadBarriers(Team team)
        {
            List<Barrier> barriers;

            if (team == null)
            {
                barriers = WALT.BLL.AdminManager.GetInstance().GetBarrierList();
            }
            else
            {
                barriers = WALT.BLL.AdminManager.GetInstance().GetBarrierList(team, true, true);
            }

            treeScript.Text = "\n<script type=\"text/javascript\">\n";

            selNode.Value = "-1";
            tree.Nodes.Clear();

            foreach (Barrier bar in barriers)
            {
                tree.Nodes.Add(CreateNode(bar));
            }

            if (_loadDesc)
            {
                treeScript.Text += "</script>\n";
            }
            else
            {
                treeScript.Text = string.Empty;
            }

            tree.CollapseAll();
        }

        public void LoadTaskTree(DTO.Task parentTask)
        {
            treeScript.Text = "\n<script type=\"text/javascript\">\n";

            selNode.Value = "-1";
            tree.Nodes.Clear();
            
            tree.Nodes.Add(CreateNode(parentTask));
            
            treeScript.Text += "</script>\n";
            tree.CollapseAll();
        }

        private TreeNode CreateNode(TaskType tt)
        {
            TreeNode node = CreateNode(tt.Id, tt.Title, tt.Description, tt.Children.Count);

            if (tt.Children.Count == 0)
            {
                myIDs.Add(tt.Id);
            }
            else
            {
                foreach (TaskType child in tt.Children)
                {
                    node.ChildNodes.Add(CreateNode(child));
                }
            }

            return node;
        }

        private TreeNode CreateNode(UnplannedCode code)
        {
            TreeNode node = CreateNode(code.Id, code.Code + " - " + code.Title, code.Description, code.Children.Count);

            if (code.Children.Count > 0)
            {
                foreach (UnplannedCode child in code.Children)
                {
                    node.ChildNodes.Add(CreateNode(child));
                }
            }

            return node;
        }

        private TreeNode CreateNode(Barrier bar)
        {
            TreeNode node = CreateNode(bar.Id, bar.Code + " - " + bar.Title, bar.Description, bar.Children.Count);

            if (bar.Children.Count > 0)
            {
                foreach (Barrier child in bar.Children)
                {
                    node.ChildNodes.Add(CreateNode(child));
                }
            }

            return node;
        }

        private TreeNode CreateNode(DTO.Task task)
        {
            TreeNode node = CreateNode(task.Id, task.Title,"", task.Children.Count);

            if (task.Children.Count > 0)
            {
                foreach (DTO.Task child in task.Children)
                {
                    node.ChildNodes.Add(CreateNode(child));
                }
            }

            return node;
        }     

        private TreeNode CreateNode(
            long id,
            string text,
            string desc,
            int children
        )
        {
            TreeNode node;

            if (children > 0)
            {
                node = new TreeNode("<font style=\"font-size: 2pt\">&nbsp;</font><br><a style=\"color: #555555\">" + text + "</a>");
            }
            else
            {
                node = new TreeNode("<font style=\"font-size: 1pt\">&nbsp;</font><br><a id=\"" + this.ID + "_node_" + id.ToString() +
                    "\" style=\"cursor: pointer; color: #000000\" onclick=\"SelectNode('" + this.ID + "', " + id.ToString() + "," +
                    _loadDesc.ToString().ToLower() + ", false)\">" + text + "</a>");

                if (_loadDesc)
                {
                    treeScript.Text += "var " + this.ID + "_nodeDesc_" + id.ToString() + " = '" + desc.Replace('\'', '"') + "';\n";
                }
            }

            return node;
        }

        protected void tree_SelectedNodeChanged(object sender, EventArgs e)
        {
            
        }

        public long GetSelectedId()
        {
            return Convert.ToInt64(selNode.Value);
        }

        public string GetSelectedValue()
        {
            return selValue.Value;
        }

        public long GetComplexityID()
        {
            long id = -1;

            if (HttpContext.Current.Request["ddComplexity"] != null)
            {
                id = Convert.ToInt64(HttpContext.Current.Request["ddComplexity"].ToString());
            }

            return id;
        }
    }
}