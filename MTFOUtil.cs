using BepInEx;
using BepInEx.IL2CPP;
using System;
using System.Linq;
using System.Reflection;

namespace ResourceHelper
{
    // code from Flowaria-ExtraEnemyCustomization
    // https://github.com/GTFO-Modding/ExtraEnemyCustomization
    public static class MTFOUtil
    {
        public const string PLUGIN_GUID = "com.dak.MTFO";
        public const BindingFlags PUBLIC_STATIC = BindingFlags.Public | BindingFlags.Static;

        public static string CustomPath { get; private set; } = string.Empty;
        public static bool IsLoaded { get; private set; } = false;

        static MTFOUtil()
        {
            if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
                return;

            InitMTFO_V5(info);

        }

        private static void InitMTFO_V5(PluginInfo info)
        {
            try
            {
                var ddAsm = info?.Instance?.GetType()?.Assembly ?? null;

                if (ddAsm is null)
                    throw new Exception("Assembly is Missing!");

                var types = ddAsm.GetTypes();
                var cfgManagerType = types.First(t => t.Name == "ConfigManager");

                if (cfgManagerType is null)
                    throw new Exception("Unable to Find ConfigManager Class");

                var customPathField = cfgManagerType.GetField("CustomPath", PUBLIC_STATIC);

                if (customPathField is null)
                    throw new Exception("Unable to Find Field: CustomPath");
            

                CustomPath = (string)customPathField.GetValue(null);
  

                IsLoaded = true;
            }
            catch (Exception e)
            {
                Logs.Error($"Exception thrown while reading metadata from MTFO (V{info.Metadata.Version}): \n{e}");
            }
        }

     
    }
}