using System;
using System.Linq;
using HtmlAgilityPack;
using System.Net;
using System.Data.OleDb;
using System.Data.SqlClient;
using CsvHelper;
using System.IO;
using System.Data;

namespace WcfService1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public string GetPYLLData()
        {
            string retVal = string.Empty;
            string url = @"https://indicators.ic.nhs.uk/webview/velocity?v=2&mode=documentation&submode=ddi&study=http%3A%2F%2F172.16.9.26%3A80%2Fobj%2FfStudy%2FP01800";
            string filename = string.Empty;
            var getHtmlWeb = new HtmlWeb();
            var document = getHtmlWeb.Load(url);
            var aTags = document.DocumentNode.SelectNodes("//a");
            if (aTags != null)
            {
                foreach (var aTag in aTags)
                {
                    if (aTag.Attributes["href"].Value.Contains(".csv"))
                    {
                        try
                        {
                            retVal = @"https://indicators.ic.nhs.uk" + aTag.Attributes["href"].Value;
                            String[] strArr = retVal.Split('/');
                            filename = strArr[strArr.Length - 1];
                            using (WebClient webClient = new WebClient())
                            {
                                webClient.DownloadFile(retVal, @"D:\PYLL.csv");
                                SaveCSVToSQLServer(@"D:\PYLL.csv");
                                //SavePYLLToSQLServer(@"D:\PYLL.xlsx", filename.Substring(0, filename.Length - 5));
                            }
                        }
                        catch (Exception ex)
                        {
                            retVal = ex.Message;
                        }
                        break;
                    }
                }
            }
            return retVal;
        }

        //private string GetSheetName(string filePath)
        //{
        //    string sheetName = string.Empty;

        //    //ExcelFile xlFile = ExcelFile.Load(filePath);

        //    //return xlFile.Worksheets[0].Name;

        //    //Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
        //    //Microsoft.Office.Interop.Excel.Workbook xlBook = xlApp.Workbooks.Open(filePath);

        //    //if (xlBook != null)
        //    //{
        //    //    foreach (Microsoft.Office.Interop.Excel.Worksheet sheet in xlBook.Worksheets)
        //    //    {
        //    //        sheetName = sheet.Name;
        //    //    }
        //    //}

        //    //xlBook.Close();

        //    //return sheetName;
        //}

        private void SavePYLLToSQLServer(string filePath, string sheetName)
        {
            string sqlTable = "PYLL_Temp";

            try
            {
                //string excelConnString = @"provider=microsoft.jet.oledb.4.0;data source=" + filePath + ";extended properties=" + "\"excel 8.0;hdr=yes;\"";
                string excelConnString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES;'", filePath);
                string sqlConnString = @"server=.\SQLExpress;user id=sa;password=pass@word1;database=DB1;connection reset=false";

                string excelSheetname = string.Empty; //GetSheetName(filePath);
                string excelQuery = @"select [Reporting Period], [Breakdown], [Level], [Level description], [Condition], [Gender], [DSR], [CI Lower], [CI Upper], [Registered patients], [Years of life lost], [Observed deaths] from [" + excelSheetname + "$]";

                OleDbConnection oledbConn = new OleDbConnection(excelConnString);
                OleDbCommand oledbCmd = new OleDbCommand(excelQuery, oledbConn);
                oledbConn.Open();

                // Read sheet data and transfer to SQL Server table
                OleDbDataReader dr = oledbCmd.ExecuteReader();
                SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnString);
                bulkCopy.DestinationTableName = sqlTable;
                while (dr.Read())
                {
                    bulkCopy.WriteToServer(dr);
                }

                oledbConn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveCSVToSQLServer(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            var csv = new CsvReader(reader);
            csv.Configuration.RegisterClassMap<PYLLMap>();

            var records = csv.GetRecords<PYLLRecord>().ToList();

            SqlConnection sqlConn = new SqlConnection(@"server=.\SQLExpress;user id=sa;password=pass@word1;database=DB1;connection reset=false");
            try
            {
                sqlConn.Open();

                foreach (PYLLRecord record in records)
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.CommandText = "AddPYLLRecord";
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Connection = sqlConn;

                    sqlCmd.Parameters.AddWithValue("@Reporting_Period", record.Reporting_Period);
                    sqlCmd.Parameters.AddWithValue("@Breakdown", record.Breakdown);
                    sqlCmd.Parameters.AddWithValue("@Level", record.Level);
                    sqlCmd.Parameters.AddWithValue("@Level_Description", record.Level_Description);
                    sqlCmd.Parameters.AddWithValue("@Condition", record.Condition);
                    sqlCmd.Parameters.AddWithValue("@Gender", record.Gender);
                    sqlCmd.Parameters.AddWithValue("@DSR", record.DSR);
                    sqlCmd.Parameters.AddWithValue("@CI_Lower", record.CI_Lower);
                    sqlCmd.Parameters.AddWithValue("@CI_Upper", record.CI_Upper);
                    sqlCmd.Parameters.AddWithValue("@Registered_Patients", record.Registered_Patients);
                    sqlCmd.Parameters.AddWithValue("@Years_of_life_lost", record.Years_of_life_lost);
                    sqlCmd.Parameters.AddWithValue("@Observed_deaths", record.Observed_deaths);

                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }
        }
    }
}
