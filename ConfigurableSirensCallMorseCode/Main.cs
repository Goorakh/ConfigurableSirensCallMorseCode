using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Diagnostics;
using UnityEngine;

namespace ConfigurableSirensCallMorseCode
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(PluginGUIDs.RISK_OF_OPTIONS, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "ConfigurableSirensCallMorseCode";
        public const string PluginVersion = "1.0.0";

        static ConfigEntry<string> _customMessage;
        static ConfigEntry<float> _messageRepeatDelay;
        static ConfigEntry<float> _delayBetweenCharacters;
        static ConfigEntry<float> _spaceDelay;

        void Awake()
        {
            Log.Init(Logger);

            Stopwatch stopwatch = Stopwatch.StartNew();

            _customMessage = Config.Bind(new ConfigDefinition("General", "Message"), "u p d o g", new ConfigDescription("The custom message to display"));
            _messageRepeatDelay = Config.Bind(new ConfigDefinition("General", "Message Repeat Duration"), 60f, new ConfigDescription("The time to wait between starting one message and the next (this value does not account for the length of the message, and it just the delay between the starting points)"));
            _delayBetweenCharacters = Config.Bind(new ConfigDefinition("General", "Delay Between Characters"), 0.3f, new ConfigDescription("The time to wait between each character in the message"));
            _spaceDelay = Config.Bind(new ConfigDefinition("General", "Space Wait Time"), 1f, new ConfigDescription("The time to wait when encountering a space character"));

            if (Chainloader.PluginInfos.ContainsKey(PluginGUIDs.RISK_OF_OPTIONS))
            {
                riskOfOptionsCompat();
            }

            SceneCatalog.onMostRecentSceneDefChanged += onMostRecentSceneDefChanged;

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        static void riskOfOptionsCompat()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("Change the morse code message on Sirens Call", PluginGUID, "Configurable Sirens Call Morse Code");

            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(_customMessage));

            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StepSliderOption(_messageRepeatDelay, new RiskOfOptions.OptionConfigs.StepSliderConfig
            {
                min = 0f,
                max = 60f * 2.5f,
                increment = 5f,
                formatString = "{0:F0}s"
            }));

            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StepSliderOption(_delayBetweenCharacters, new RiskOfOptions.OptionConfigs.StepSliderConfig
            {
                min = 0f,
                max = 1f,
                increment = 0.05f,
                formatString = "{0:F2}s"
            }));

            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StepSliderOption(_spaceDelay, new RiskOfOptions.OptionConfigs.StepSliderConfig
            {
                min = 0f,
                max = 3f,
                increment = 0.1f,
                formatString = "{0:F1}s"
            }));
        }

        static void onMostRecentSceneDefChanged(SceneDef scene)
        {
            if (scene.sceneDefIndex != SceneCatalog.FindSceneIndex("shipgraveyard"))
                return;

            MorsecodeFlasher morsecodeFlasher = GameObject.FindObjectOfType<MorsecodeFlasher>();
            if (!morsecodeFlasher)
            {
                Log.Error("Could not find morse code flasher component");
                return;
            }

            morsecodeFlasher.morsecodeMessage = _customMessage.Value;
            morsecodeFlasher.messageRepeatDelay = _messageRepeatDelay.Value;
            morsecodeFlasher.delayBetweenCharacters = _delayBetweenCharacters.Value;
            morsecodeFlasher.spaceDelay = _spaceDelay.Value;
        }
    }
}
