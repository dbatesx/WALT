using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace WALT.UIL.Controls
{
    public partial class ProfileSelector : System.Web.UI.UserControl
    {
        private bool _includePlanExempt = true;
        public bool IncludePlanExempt 
        {
            get { return _includePlanExempt; }
            set { _includePlanExempt = value; }
        }

        private bool _includeTaskExempt = true;
        public bool IncludeTaskExempt
        {
            get { return _includeTaskExempt; }
            set { _includeTaskExempt = value; }
        }

        private bool _directorateAdminsOnly = false;
        public bool DirectorateAdminsOnly
        {
            get { return _directorateAdminsOnly; }
            set { _directorateAdminsOnly = value; }
        }

        private bool _directorateManagersOnly = false;
        public bool DirectorateManagersOnly
        {
            get { return _directorateManagersOnly; }
            set { _directorateManagersOnly = value; }
        }

        private List<DTO.Profile> _profileListToExclude;
        public List<DTO.Profile> ProfileListToExclude
        {
            set
            {
                _profileListToExclude = value;
            }
        }

        private List<DTO.Profile> _profileListToSearch;
        public List<DTO.Profile> ProfileListToSearch
        {
            set
            {
                _profileListToSearch = value;
            }
        }

        private List<DTO.Profile> _profilesChosen;
        public List<DTO.Profile> ProfilesChosen
        {
            get
            {
                if (_profilesChosen == null)
                    _profilesChosen = new List<DTO.Profile>();

                bool found = false;

                if (_allowMultiple)
                {
                    foreach (ListItem item in profileSelected.Items)
                    {
                        // don't add it if it already exists in the chosen list
                        foreach (DTO.Profile profile in _profilesChosen)
                        {
                            if (item.Value.Equals(profile.Id.ToString()))
                                found = true;
                        }

                        if (!found)
                        {
                            long prof_Id = 0;

                            if (long.TryParse(item.Value, out prof_Id))
                            {
                                _profilesChosen.Add(BLL.ProfileManager.GetInstance().GetProfile(prof_Id));
                            }
                        }
                    }
                }
                else
                {
                    if (profileSelected.SelectedItem != null)
                    {
                        // don't add it if it already exists in the chosen list
                        foreach (DTO.Profile profile in _profilesChosen)
                        {
                            if (profileSelected.SelectedItem.Value.Equals(profile.Id.ToString()))
                                found = true;
                        }

                        if (!found)
                        {
                            long prof_Id = 0;

                            if (long.TryParse(profileSelected.SelectedItem.Value, out prof_Id))
                            {
                                _profilesChosen.Add(BLL.ProfileManager.GetInstance().GetProfile(prof_Id));
                            }
                        }
                    }
                }

                return _profilesChosen;
            }
            set // pre-populate the selected profiles in the list
            {
                _profilesChosen = value;

                if (_profilesChosen != null)
                {
                    if (_allowMultiple)
                    {
                        foreach (DTO.Profile profile in _profilesChosen)
                        {
                            profileSelected.Items.Add(new ListItem(profile.DisplayName, profile.Id.ToString()));
                        }
                        foreach (ListItem item in profileSelected.Items)
                        {
                            item.Selected = true;
                        }
                    }
                    else
                    {
                        if (_profilesChosen.Count > 0)
                        {
                            profileSelected.Items.Add(new ListItem(_profilesChosen[0].DisplayName, _profilesChosen[0].Id.ToString()));
                            profileSelected.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        private bool _isRequired = false;
        public virtual bool IsRequired
        {
            get { return _isRequired; }
            set { _isRequired = value; }

        }

        private bool _allowMultiple = true;
        public virtual bool AllowMultiple
        {
            get { return _allowMultiple; }
            set { _allowMultiple = value; }

        }

        private bool _preLoadWithTeammates = false;
        public virtual bool PreLoadWithTeammates
        {
            get { return _preLoadWithTeammates; }
            set { _preLoadWithTeammates = value; }

        }

        public void Clear()
        {
            searchResultListBox.Items.Clear();
            profileSelected.Items.Clear();
            txtFilterItems.Text = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack && _preLoadWithTeammates)
            {
                DTO.Profile profile = WALT.BLL.ProfileManager.GetInstance().GetProfile();

                Dictionary<long, string> profilesFound = BLL.ProfileManager.GetInstance().GetTeammateDictionary(profile.Id);

                if (profilesFound != null && profilesFound.Count > 0)
                {
                    searchResultListBox.DataSource = profilesFound;
                    searchResultListBox.DataTextField = "Value";
                    searchResultListBox.DataValueField = "Key";
                    searchResultListBox.DataBind();
                }
            }
           
                      
            if (!_allowMultiple)
            {
                searchResultListBox.SelectionMode = ListSelectionMode.Single;
            }
            else
            {
                searchResultListBox.SelectionMode = ListSelectionMode.Multiple;
            }

            if (!_isRequired)
                profileRequiredFieldValidator.Enabled = false;

            if (profileSelected != null && profileSelected.Items.Count > 0 && profileSelected.SelectedItem != null)
            {
                
                removeButton.Enabled = true;
            }

            if (!string.IsNullOrEmpty(searchListBoxHidden.Value) && searchListBoxHidden.Value.Equals("doubleclicked"))
            {
                addProfile();
                searchListBoxHidden.Value = string.Empty;   
            }

            
        }      

        protected void txtFilterItems_TextChanged(object sender, EventArgs e)
        {
            string filterText = txtFilterItems.Text;
            Dictionary<long, string> profilesFound= new Dictionary<long,string>();

            if (!string.IsNullOrEmpty(filterText))
            {
                profilesFound = BLL.ProfileManager.GetInstance().GetProfileDictionary(filterText);
            }
                      
            if (profilesFound != null && profilesFound.Count > 0)
            {               
                searchResultListBox.DataSource = profilesFound;
                searchResultListBox.DataTextField = "Value";
                searchResultListBox.DataValueField = "Key";
                searchResultListBox.DataBind();
                lblNoMatch.Visible = false;
            }
            else 
            {
                searchResultListBox.Items.Clear();
                lblNoMatch.Visible = true;
            }
        }

        protected void addButton_Click(object sender, EventArgs e)
        {
            addProfile();            
        }

        protected void addProfile() 
        {
            List<ListItem> itemsToMove = new List<ListItem>();

            if (searchResultListBox.SelectedItem != null) 
            {
                if (!_allowMultiple)
                {
                    itemsToMove.Add(searchResultListBox.SelectedItem);
                }
                else
                {
                    bool add = true;

                    foreach (ListItem selectedItams in profileSelected.Items)
                    {
                        if (selectedItams.Value.Equals(searchResultListBox.SelectedItem.Value))
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add)
                        itemsToMove.Add(searchResultListBox.SelectedItem);
                }
            }
            
            //foreach (ListItem item in searchResultListBox.Items)
            //{
            //    if (item.Selected)
            //        itemsToMove.Add(item);
            //}

            foreach (ListItem item in itemsToMove)
            {
                if (!_allowMultiple)
                {
                    if (profileSelected.Items.Count == 1)
                        profileSelected.Items.RemoveAt(0);
                }

                profileSelected.Items.Add(item);
               // searchResultListBox.Items.Remove(item);
            }

            profilePanel.Update();
        }

        protected override void OnPreRender(EventArgs e)
        {
            int selectedCount = profileSelected.Items.Count;
            int searchCount = searchResultListBox.Items.Count;

            if (selectedCount > 0)
            {
                removeButton.Enabled = true;
            }
            else 
            {
                removeButton.Enabled = false;
            }

            if (searchCount > 0)
            {
                addButton.Enabled = true;
            }
            else
            {
                addButton.Enabled = false;
            }

            profilePanel.Update();
            base.OnPreRender(e);
        }
       

        protected void removeButton_Click(object sender, EventArgs e)
        {
            List<ListItem> itemsToMove = new List<ListItem>();
            
            foreach (ListItem item in profileSelected.Items)
            {
                if (item.Selected)
                    itemsToMove.Add(item);
            }

            foreach (ListItem item in itemsToMove)
            {
                //searchResultListBox.Items.Add(item);
                item.Selected = false;
                profileSelected.Items.Remove(item);
            }
            profileSelected.DataBind();
            profilePanel.Update();

            //if (!AllowMultiple)
            //    searchResultListBox.SelectedItem.Selected = false;

            //// reoder list items
            //List<ListItem> list = new List<ListItem>(searchResultListBox.Items.Cast<ListItem>());        
            //list = list.OrderBy(li => li.Text).ToList<ListItem>();           
            //searchResultListBox.Items.Clear();             
            //searchResultListBox.Items.AddRange(list.ToArray<ListItem>()); 
        }

        protected void profileSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeButton.Enabled = true;
        }

        public void ClearControl() 
        {
            searchResultListBox.Items.Clear();
            profileSelected.Items.Clear();
            txtFilterItems.Text = "";
        }
    }
}