using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.DirectoryServices;
using System.Configuration;

namespace WALT.UIL
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!BLL.ProfileManager.GetInstance().IsAllowed(DTO.Action.SYSTEM_MANAGE))
            {
                Response.Redirect("/");
            }

            if (!IsPostBack)
            {
                DropDownList1.Items.Add("name");
                DropDownList1.Items.Add("manager");
                DropDownList1.Items.Add("samaccountname");
                DropDownList1.Items.Add("department");
                DropDownList1.Items.Add("employeeid");
            }
        }        

        protected void X3_Click1(object sender, EventArgs e)
        {
          
        }

        protected void btnADSearch_Click(object sender, EventArgs e)
        {
            if (txtADSearchStr.Text.Length > 0 && DropDownList1.SelectedItem.Text.Length > 0)
            {
                lblADSearchResults.Text = SearchActiveDirectory(txtADSearchStr.Text, DropDownList1.SelectedItem.Text);
            }
            else
            {
                Utility.DisplayErrorMessage("Search field and string required");
            }
        }

        private string SearchActiveDirectory(string filter, string field)
        {
            string found = string.Empty;

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADDomains"]))
            {
                int i = 0;
                string[] domains = ConfigurationManager.AppSettings["ADDomains"].Split(',');
                string dc = string.Empty;
                
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADDomainComponents"]))
                {
                    string[] split = ConfigurationManager.AppSettings["ADDomainComponents"].Split(',');

                    foreach (string component in split)
                    {
                        dc += ",DC=" + component;
                    }
                }

                while (i < domains.Length && found == string.Empty)
                {
                    using (DirectoryEntry en = new DirectoryEntry("LDAP://DC=" + domains[i] + dc))
                    {
                        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADUsername"]))
                        {
                            en.Username = ConfigurationManager.AppSettings["ADUsername"];
                        }

                        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ADPassword"]))
                        {
                            en.Password = ConfigurationManager.AppSettings["ADPassword"];
                        }

                        using (DirectorySearcher searcher = new DirectorySearcher(en))
                        {
                            SearchResultCollection results = null;
                            searcher.Filter = field + "=" + filter + "";
                            SearchResult single = searcher.FindOne();

                            if (single == null)
                            {
                                searcher.Filter = field + "=*" + filter + "*";
                                results = searcher.FindAll();

                                if (results != null && results.Count == 1)
                                {
                                    single = results[0];
                                }
                            }

                            if (single != null)
                            {
                                foreach (string prop in single.Properties.PropertyNames)
                                {
                                    found += "<br><b>" + prop + ":</b><br>";
                                    ResultPropertyValueCollection vals = single.Properties[prop];

                                    for (int k = 0; k < vals.Count; k++)
                                    {
                                        if (vals.Count > 1) found += k.ToString() + " - ";
                                        found += vals[k].ToString() + "<br>";
                                    }
                                }
                            }
                            else if (results != null && results.Count > 1)
                            {
                                found += "<p/>Found multiple results:<p/>";

                                foreach (SearchResult result in results)
                                {
                                    found += result.Properties["name"][0] + "<br>";
                                }
                            }
                        }
                    }

                    i++;
                }
            }

            return found;
        }
    }
}