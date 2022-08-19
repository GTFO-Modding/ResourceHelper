using System;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using HarmonyLib;
using UnityEngine.Analytics;
using Il2CppInterop.Runtime.Injection;

using BepInEx.Configuration;
using System.IO;



namespace ResourceHelper
{
	[BepInPlugin("Localia.ResourceHelper", "ResourceHelper", "3.0.0")]
	[BepInDependency("com.dak.MTFO", BepInDependency.DependencyFlags.SoftDependency)]



	public class EntryPoint : BasePlugin
	{
		private Harmony m_Harmony;
		public static ConfigFile config_path= new ConfigFile(Path.Combine(Paths.ConfigPath, "Localia.ResourceHelper-Plus.cfg"), true);
        public static ConfigEntry<float> config_aim_fade_factor{get;set;}

		public override void Load()
		{
			ClassInjector.RegisterTypeInIl2Cpp<Res_Moniter>();
			this.m_Harmony = new Harmony("Localia.ResourceHelper");
			this.m_Harmony.PatchAll();
			
			
			SpriteManager.Initialize();

			config_aim_fade_factor = config_path.Bind<float>("General Settings", "ADS Fade Factor", 0.5f, "The opacity of the icons when ADS. ( 0.0 is completely hidden, 1.0 is no fade.)");
			Res_Manager.value_aim_fade_factor = Math.Clamp(config_aim_fade_factor.Value,0.0f,1.0f);
			Logs.Info("OK");

			
		}
	}
}

