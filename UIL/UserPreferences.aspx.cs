using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;


namespace WALT.UIL
{
    public partial class UserPreferences : System.Web.UI.Page
    {
        private Dictionary<string, string> _profilePreferences;
        private DTO.Profile _profile;
        private const string Program_Preference_KEY = "ProgramPreferences";
       // DataTable _dataTable;

        protected void Page_Load(object sender, EventArgs e)
        {
            _profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();
            _profilePreferences = _profile.Preferences;

            //_dataTable = new DataTable();

            //_dataTable.Columns.Add(new DataColumn("progId", typeof(long)));          
            //_dataTable.Columns.Add(new DataColumn("default", typeof(bool)));
                        
            if (!Page.IsPostBack)
            {
                BindProgDropDown();
                BindrptProgPrefrence();   
            }            
        }

        protected void btnAddProgram_Click(object sender, EventArgs e)
        {
            XElement prefProg = null;

            if (_profilePreferences != null && _profilePreferences.ContainsKey(Program_Preference_KEY))
            {
                prefProg = XElement.Parse(_profilePreferences[Program_Preference_KEY]);
            }
            else 
            {
                prefProg = new XElement("ProgramCollection");
            }

            if (ddlPrograms.SelectedItem != null && ddlPrograms.SelectedIndex != 0)
            {
                bool alreadySaved = false;

                foreach (XElement prog in prefProg.Elements("Program"))
                {
                    if (prog.Element("ID").Value.Equals(ddlPrograms.SelectedValue))
                    {
                        alreadySaved = true;
                        break;
                    }
                }

                if (!alreadySaved)
                {
                    prefProg.Add(new XElement("Program",
                           new XElement("ID", ddlPrograms.SelectedValue),                            
                              new XElement("Default", "false")));
                }
            }          
                       
            try
            {
                WALT.BLL.ProfileManager.GetInstance().SavePreference(Program_Preference_KEY, prefProg.ToString());
                Utility.DisplayInfoMessage("Your user preferences were saved.");
                BindrptProgPrefrence();

            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }   
            
        }
        protected void lbtnRemove_Click(object sender, EventArgs e) 
        {
            LinkButton removeBtn = (LinkButton)sender;
            string progID = removeBtn.CommandArgument;

            XElement prefProg = null;

            if (_profilePreferences != null && _profilePreferences.ContainsKey(Program_Preference_KEY))
            {
                prefProg = XElement.Parse(_profilePreferences[Program_Preference_KEY]);
            }
            else
            {
                prefProg = new XElement("ProgramCollection");
            }

            if (!string.IsNullOrEmpty(progID))
            { 
                foreach (XElement prog in prefProg.Elements("Program"))
                {
                    if (prog.Element("ID").Value.Equals(progID))
                    {
                        prog.Remove();
                    }
                }                
            }

            try
            {
                WALT.BLL.ProfileManager.GetInstance().SavePreference(Program_Preference_KEY, prefProg.ToString());
                Utility.DisplayInfoMessage("Your user preferences was saved.");
                BindrptProgPrefrence();
            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }
        }

        protected void cbDefault_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox defaultCb = (CheckBox)sender;
            string cbId = defaultCb.ClientID;
            string progId = string.Empty;
            XElement prefProg = null;

            foreach (RepeaterItem progItem in rptProgPrefrence.Items) 
            {
                CheckBox cbProgDefault = (CheckBox)progItem.FindControl("cbDefault");
                HiddenField hdnProgId = (HiddenField)progItem.FindControl("hdnComplexityId");

                if (!cbId.Equals(cbProgDefault.ClientID))
                {
                    cbProgDefault.Checked = false;
                }
                else 
                {
                    progId = hdnProgId.Value;
                }
            }           

            if (_profilePreferences != null && _profilePreferences.ContainsKey(Program_Preference_KEY))
            {
                prefProg = XElement.Parse(_profilePreferences[Program_Preference_KEY]);
            }
            else
            {
                prefProg = new XElement("ProgramCollection");
            }

            if (!string.IsNullOrEmpty(progId))
            {
                foreach (XElement prog in prefProg.Elements("Program"))
                {
                    if (prog.Element("ID").Value.Equals(progId))
                    {
                        prog.Element("Default").Value = "true";
                    }
                    else 
                    {
                        prog.Element("Default").Value = "false";
                    }
                }                
            }

            try
            {
                WALT.BLL.ProfileManager.GetInstance().SavePreference(Program_Preference_KEY, prefProg.ToString());
                Utility.DisplayInfoMessage("Your user preferences was saved.");
                BindrptProgPrefrence();

            }
            catch (Exception ex)
            {
                Utility.DisplayException(ex);
            }  
        }
        

        protected void BindProgDropDown()
        {
            Dictionary<long, string> programs = BLL.AdminManager.GetInstance().GetProgramDictionary();

            ddlPrograms.DataSource = programs;
            ddlPrograms.DataTextField = "Value";
            ddlPrograms.DataValueField = "Key";
            ddlPrograms.DataBind();

            ddlPrograms.Items.Insert(0, new ListItem("Select a Program...", "none"));
        }

        protected void BindrptProgPrefrence()
        {
            List<KeyValuePair<long, bool>> prefPrograms = new List<KeyValuePair<long, bool>>();

            if (_profilePreferences != null && _profilePreferences.ContainsKey(Program_Preference_KEY))
            {
                XElement prefProg = XElement.Parse(_profilePreferences[Program_Preference_KEY]);

                foreach (XElement prog in prefProg.Elements("Program"))
                { 
                    long progId= 0;

                    if (long.TryParse(prog.Element("ID").Value.ToString(), out progId))
                    {
                        if (prog.Element("Default").Value.ToString().Equals("true"))
                        {
                            prefPrograms.Add(new KeyValuePair<long, bool>(progId, true));
                        }
                        else
                        {
                            prefPrograms.Add(new KeyValuePair<long, bool>(progId, false));
                        }
                    }
                }
            }


            if (prefPrograms!= null)
            {                
                DataTable dataTable = new DataTable();

                dataTable.Columns.Add(new DataColumn("progId", typeof(long)));
                dataTable.Columns.Add(new DataColumn("progTitle", typeof(string)));
                dataTable.Columns.Add(new DataColumn("default", typeof(bool)));  

                foreach (KeyValuePair<long, bool> progKey in prefPrograms) 
                {
                    DTO.Program prog = BLL.AdminManager.GetInstance().GetProgram(progKey.Key);

                    if (prog != null && prog.Active)
                    {
                        DataRow row = dataTable.NewRow();

                        row["progId"] = prog.Id;
                        row["progTitle"] = prog.Title;
                        row["default"] = progKey.Value;

                        dataTable.Rows.Add(row);
                    }
                }
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    //dataTable.DefaultView.Sort = "progTitle ASC";
                    rptProgPrefrence.DataSource = dataTable;
                    rptProgPrefrence.DataBind();
                }
            }
            else
            {
                rptProgPrefrence.Visible = false;
            }
        }       
    }
}