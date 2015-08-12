using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Common
    {
        public static string GetAppConfigValue(string key)
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            KeyValueConfigurationElement setting;

            if (config.AppSettings.Settings.Count > 0)
            {
                setting = config.AppSettings.Settings[key];
            }
            else
            {
                throw new ConfigurationErrorsException("Missing element in config.web with key: " + key);
            }

            return setting.Value;

        }

        public static void LogError(Exception ex)
        {
            MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();

            ErrorLog log = new ErrorLog();

            log.DatetTimeStamp = DateTime.UtcNow.AddHours(2);
            log.ErrorMessage = ex.Message + ": " + ex.StackTrace.ToString();

            db.ErrorLogSet.Add(log);
            db.SaveChanges();
        }
    }
}