using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnProcess_Click(object sender, EventArgs e)
        {
            ServiceReference1.Service1Client wsClient = new ServiceReference1.Service1Client();
            string result = wsClient.GetPYLLData();
            Label1.Text = result;
        }
    }
}