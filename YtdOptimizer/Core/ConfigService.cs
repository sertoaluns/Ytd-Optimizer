using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization; // or custom simple json

namespace YtdOptimizer.Core
{
    public static class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ytd_optimizer_config.json");

        public static OptimizerConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath, Encoding.UTF8);
                    var serializer = new JavaScriptSerializer();
                    var config = serializer.Deserialize<OptimizerConfig>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
            }
            catch
            {
                // Fallback to default
            }

            return OptimizerConfig.Default();
        }

        public static void Save(OptimizerConfig config)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(config);
                File.WriteAllText(ConfigPath, json, Encoding.UTF8);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
