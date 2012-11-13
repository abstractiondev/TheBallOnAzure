﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebInterface
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string hostName = Request.Url.DnsSafeHost;
            if (hostName.StartsWith("oip.") || hostName.StartsWith("demooip.") || hostName.StartsWith("publicoip.") || hostName.StartsWith("demopublicoip."))
                Response.Redirect("public/grp/default/publicsite/oip-public/oip-layout-landing.phtml", true);
            if(hostName.StartsWith("www.") || hostName.StartsWith("demowww"))
                Response.Redirect("www-public/oip-layout-landing.phtml");
        }
    }
}