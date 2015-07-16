using HtmlAgilityPack;
using System;
using System.Configuration;
using System.Net;

namespace consolePOC
{
    class Program
    {
        static void Main(string[] args)
        {
            GetPYLLData();
        }

        public static void GetPYLLData()
        {
            string retVal = string.Empty;
            string url = @"https://indicators.ic.nhs.uk/webview/velocity?v=2&mode=documentation&submode=ddi&study=http%3A%2F%2F172.16.9.26%3A80%2Fobj%2FfStudy%2FP01800";
            string filename = string.Empty;

            Console.WriteLine("Downloading file from {0}", url);
            try
            {

                var getHtmlWeb = new HtmlWeb();
                var document = getHtmlWeb.Load(url);
                var aTags = document.DocumentNode.SelectNodes("//a");
                if (aTags != null)
                {
                    foreach (var aTag in aTags)
                    {
                        if (aTag.Attributes["href"].Value.Contains(".csv"))
                        {
                            retVal = @"https://indicators.ic.nhs.uk" + aTag.Attributes["href"].Value;
                            String[] strArr = retVal.Split('/');
                            filename = strArr[strArr.Length - 1];
                            using (WebClient webClient = new WebClient())
                            {
                                string csvFilePath = String.Format(@"{0}\PYLL_{1}.csv", ConfigurationManager.AppSettings["downloadPath"], DateTime.Now.Ticks);
                                Console.WriteLine("Saving data to disk file {0}", csvFilePath);
                                webClient.DownloadFile(retVal, csvFilePath);
                                PYLLProcess process = new PYLLProcess();
                                process.SaveCSVToSQLServer(csvFilePath);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            Console.WriteLine(@"File {0} processed successfully.", retVal);
        }
    }
    public class Result
    {
        public string Message { get; set; }
    }
}
