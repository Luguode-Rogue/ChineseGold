using System;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ChineseGold
{
    public sealed class SubModule : MBSubModuleBase
    {
        private static bool _initialized;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            if (_initialized)
            {
                return;
            }

            try
            {
                GoldDisplayPatches.Apply();
                CampaignHudGoldPatch.Apply(new Harmony("Luguode.ChineseGold.Hud"));
                _initialized = true;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        "[ChineseGold] Failed to initialize: " + ex.Message));
            }
        }
    }
}
