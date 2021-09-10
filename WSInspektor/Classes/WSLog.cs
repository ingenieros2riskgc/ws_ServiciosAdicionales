using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Net;

namespace WSInspektor.Classes
{
    public class WSLog : System.Web.UI.Page
    {
        public void errorMessage(string message)
        {
            StreamWriter sWErrorMess = new StreamWriter(Server.MapPath("~/Log/WSLog.txt"), true);

            try
            {
                sWErrorMess.WriteLine(message + " " + Convert.ToString(DateTime.Now));
                sWErrorMess.Flush();
                sWErrorMess.Close();
            }
            catch
            {
                sWErrorMess.Flush();
                sWErrorMess.Close();
            }
        }
    }
}