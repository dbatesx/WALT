using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WALT.UIL
{
    public partial class ping : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (HttpContext.Current.Request["interval"] != null)
                {
                    Timer1.Interval = Convert.ToInt32(HttpContext.Current.Request["interval"]) * 60000;
                }
                else
                {
                    Timer1.Enabled = false;
                }
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["timeDiff"] == null)
            {
                Timer1.Enabled = false;
                HttpContext.Current.Session.Abandon();
            }
        }
    }
}