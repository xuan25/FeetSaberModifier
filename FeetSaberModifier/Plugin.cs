using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Util;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace FeetSaberModifier
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public const string HarmonyId = "com.github.xuan25.FeetSaberModifier";
        internal static Harmony harmony = new Harmony(HarmonyId);

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        internal static string Name => "FeetSaberModifier";
        internal static string TabName => "FeetSaber";
        internal static FeetSaberModifierController PluginController { get { return FeetSaberModifierController.instance; } }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("FeetSaberModifier initialized.");

            Logger.log = logger;
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Config.Read();
            MainMenuAwaiter.MainMenuInitializing += OnMainMenuInitializing;
            new GameObject("FeetSaberModifierController").AddComponent<FeetSaberModifierController>();
            ApplyHarmonyPatches();
        }

        private void OnMainMenuInitializing()
        {
            GameplaySetup.Instance.AddTab(TabName, $"{Name}.UI.ModifierUI.bsml", ModifierUI.instance);
        }

        public static void ApplyHarmonyPatches()
        {
            try
            {
                Logger.log.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Error applying Harmony patches: " + ex.Message);
                Logger.log.Debug(ex);
            }
        }

        public static void RemoveHarmonyPatches()
        {
            try
            {
                harmony.UnpatchSelf();
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Error removing Harmony patches: " + ex.Message);
                Logger.log.Debug(ex);
            }
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

        }
    }
}
