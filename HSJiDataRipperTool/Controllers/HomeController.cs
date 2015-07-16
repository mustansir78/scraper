using HSJiDataRipperTool.Models;
using HSJiDataRipperTool.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HSJiDataRipperTool.Controllers
{
    public class HomeController : Controller
    {
        Service1Client _proxy;
        RequestCallback _callback;

        public ActionResult Index()
        {
            return View();
        }

        private PYLLModel GetPYLLFromDB(string criteria)
        {
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings[Constants.DEFAULT_CONN].ConnectionString);
            SqlCommand cmd = new SqlCommand("PYLL_Report", sqlConn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("gender", criteria));
            PYLLModel model = new PYLLModel();

            try
            {
                sqlConn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    PYLLRecord recPYLL = new PYLLRecord();
                    recPYLL.Period = dr["Period"].ToString();
                    recPYLL.Breakdown = dr["Breakdown"].ToString();
                    recPYLL.Level = dr["Level"].ToString();
                    recPYLL.LevelDescription = dr["Level Description"].ToString();
                    recPYLL.Gender = dr["Gender"].ToString();
                    recPYLL.DSR = dr["DSR"].ToString();
                    model.PYLLList.Add(recPYLL);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }
            return model;
        }

        private string CreatePYLLCSVFile(string criteria)
        {
            PYLLModel model = GetPYLLFromDB(criteria);

            string csv = "Year, Breakdown, Level, Level Description, Gender, DSR\n";

            foreach (PYLLRecord item in model.PYLLList)
            {
                csv += String.Format("{0}, {1}, {2}, {3}, {4}, {5}\n", 
                    item.Period.Replace(",", ""),
                    item.Breakdown.Replace(",", ""),
                    item.Level.Replace(",", ""),
                    item.LevelDescription.Replace(",", ""),
                    item.Gender.Replace(",", ""),
                    item.DSR.Replace(",", ""));
            }

            return csv;
        }

        public ActionResult PYLLMale()
        {
            ViewBag.ReportTitle = "Demographics - PYLL - Male";
            return View(GetPYLLFromDB("Male"));
        }

        public ActionResult PYLLFemale()
        {
            ViewBag.ReportTitle = "Demographics - PYLL - Female";
            return View(GetPYLLFromDB("Female"));
        }

        public FileContentResult PYLLMaleExportCSV()
        {
            string csv = CreatePYLLCSVFile("Male");
            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "download.csv");
        }

        public FileContentResult PYLLFemaleExportCSV()
        {
            string csv = CreatePYLLCSVFile("Female");
            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "download.csv");
        }

        void _callback_MessageReceived(object sender, EventArgs e)
        {
            StreamWriter writer = new StreamWriter(String.Format(@"{0}\ServiceLog.txt", ConfigurationManager.AppSettings["logPath"]), true);
            writer.WriteLine(_callback.Message);
            writer.Close();
        }

        public ActionResult DoProcess()
        {
            _callback = new RequestCallback();
            _callback.MessageReceived += _callback_MessageReceived;
            InstanceContext context = new InstanceContext(_callback);
            _proxy = new Service1Client(context);

            _proxy.GetPYLLData();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}