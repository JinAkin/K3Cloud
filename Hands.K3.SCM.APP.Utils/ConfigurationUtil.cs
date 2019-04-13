

using System.Configuration;

namespace Hands.K3.SCM.APP.Utils
{
    public class ConfigurationUtil
    {
        public static string GetAppSetting(string settingName)
        {
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

                return config.AppSettings.Settings[settingName].Value;
            }

            return null;
        }
    }
}
