using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WALT.UIL
{
    public partial class login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                int hourDiff = 0;
                DateTime localTime;

                if (HttpContext.Current.Request["localTime"] != null &&
                    DateTime.TryParse(HttpContext.Current.Request["localTime"].ToString(), out localTime))
                {
                    TimeSpan diff = localTime - DateTime.Now;
                    hourDiff = diff.Hours;

                    if (diff.Minutes > 30) hourDiff++;
                    else if (diff.Minutes < -30) hourDiff--;
                }

                HttpContext.Current.Session["timeDiff"] = hourDiff;
                
                Response.Redirect(HttpContext.Current.Request["src"]);
            }
        }
    }
}