using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WALT.UIL
{
    public partial class ErrorPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LastErrorMessage"] != null)
            {
                Pre1.InnerText = (string)Session["LastErrorMessage"];
                Session.Remove("LastErrorMessage");
            }
        }
    }
}