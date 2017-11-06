using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Research.Science.Data;
using NLog;

namespace AVISTEDNetCDFConverter.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string Get(string path, bool outfolder)
        {
            logger.Log(LogLevel.Info, "Entered AVISTEDNetCDFConverter GET()");
            try
            {
                string result = "false";
                string content = File.ReadAllText(path);
                List<Dictionary<string, string>> data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(content);

                string randomlyGeneratedFolderNamePart = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

                string timeRelatedFolderNamePart = DateTime.Now.Year.ToString()
                                                 + DateTime.Now.Month.ToString()
                                                 + DateTime.Now.Day.ToString()
                                                 + DateTime.Now.Hour.ToString()
                                                 + DateTime.Now.Minute.ToString()
                                                 + DateTime.Now.Second.ToString()
                                                 + DateTime.Now.Millisecond.ToString();

                string processRelatedFolderNamePart = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                string copypath = "";
                if (outfolder)
                {
                    copypath = ConfigurationManager.AppSettings["Save_Downloads"].ToString();
                }
                else
                {
                    copypath = ConfigurationManager.AppSettings["Converters"].ToString();
                }
                string temporaryDirectoryName = Path.Combine(copypath
                                                            , timeRelatedFolderNamePart
                                                            + processRelatedFolderNamePart
                                                            + randomlyGeneratedFolderNamePart);
                System.IO.Directory.CreateDirectory(temporaryDirectoryName);

                logger.Log(LogLevel.Info, "Created Directory");
                string uri = Path.Combine(temporaryDirectoryName, "result" + ".nc") + "?openMode=create";
                DataSet dscopy = DataSet.Open(uri);

                string[] results = new string[data.Count + 1];
                int i = 0, j = 0;
                Dictionary<string, string> resultdict = new Dictionary<string, string>();
                Dictionary<string, string> tempdict = data.First();

                string[] names = tempdict.Keys.ToArray();
                string[] values = new string[names.Length];
                foreach (Dictionary<string, string> dict in data)
                {
                    var value = dict.Values.ToArray();

                    if (j == 0)
                    {
                        for (int k = 0; k < values.Length; k++)
                        {
                            values[k] = value[k];
                        }
                        j = 1;

                    }
                    else
                    {
                        for (int k = 0; k < values.Length; k++)
                        {
                            values[k] += "," + value[k];
                        }

                    }
                }
                int index = 0;
                foreach (string s in names)
                {
                    if (s.Equals("date"))
                    {
                        string[] strings = values[index++].Split(',');
                        DateTime[] date = new DateTime[strings.Length];
                        int l = 0;
                        foreach (string d in strings)
                        {
                            date[l++] = DateTime.Parse(d);
                        }
                        dscopy.AddVariable<DateTime>(s, date);

                        logger.Log(LogLevel.Info, "Created parameter {0}", s);
                    }
                    else
                    {
                        string[] strings = values[index++].Split(',');
                        float[] vl = new float[strings.Length];
                        int l = 0;
                        foreach (string d in strings)
                        {
                            vl[l++] = float.Parse(d);
                        }
                        dscopy.AddVariable<float>(s, vl);
                        logger.Log(LogLevel.Info, "Created parameter {0}", s);
                    }
                }
                dscopy.Commit();
                dscopy.Dispose();

                string SourceFolderPath = temporaryDirectoryName;
                return SourceFolderPath;
            }
            catch (Exception ex)
            {
                logger.Error("AVISTEDNetCDFConverter:Failed with exception {0}", ex.Message);
            }
            return "Error";
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
