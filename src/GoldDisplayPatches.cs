using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;

namespace ChineseGold
{
    internal static class GoldDisplayPatches
    {
        private const string HarmonyId = "Luguode.ChineseGold";
        private static Harmony? _harmony;

        public static void Apply()
        {
            _harmony = new Harmony(HarmonyId);

            PatchMapInfo();
            PatchMissionConversation();
            PatchBarterItem();
        }

        private static void PatchMapInfo()
        {
            Type? type = FindType(
                "TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapInfoVM");

            MethodInfo? target = type?.GetMethod(
                "UpdatePlayerInfo",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (target == null)
            {
                return;
            }

            MethodInfo? postfix = typeof(GoldDisplayPatches).GetMethod(
                nameof(MapInfoPostfix),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (postfix != null)
            {
                _harmony!.Patch(target, postfix: new HarmonyMethod(postfix));
            }
        }

        private static void PatchMissionConversation()
        {
            Type? type = FindType(
                "TaleWorlds.CampaignSystem.ViewModelCollection.Conversation.MissionConversationVM");

            MethodInfo? target = type?.GetMethod(
                "Refresh",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (target == null)
            {
                return;
            }

            MethodInfo? postfix = typeof(GoldDisplayPatches).GetMethod(
                nameof(MissionConversationPostfix),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (postfix != null)
            {
                _harmony!.Patch(target, postfix: new HarmonyMethod(postfix));
            }
        }

        private static void PatchBarterItem()
        {
            Type? type = FindType(
                "TaleWorlds.CampaignSystem.ViewModelCollection.Barter.BarterItemVM");

            if (type == null)
            {
                return;
            }

            PatchProperty(type, "CurrentOfferedAmountText");
            PatchProperty(type, "TotalItemCountText");
        }

        private static void PatchProperty(Type type, string propertyName)
        {
            PropertyInfo? property = type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            MethodInfo? setter = property?.SetMethod;
            if (setter == null)
            {
                return;
            }

            MethodInfo? prefix = typeof(GoldDisplayPatches).GetMethod(
                nameof(GoldTextSetterPrefix),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (prefix != null)
            {
                _harmony!.Patch(setter, prefix: new HarmonyMethod(prefix));
            }
        }

        private static void MapInfoPostfix(object __instance)
        {
            try
            {
                int gold = GetPlayerGold();

                foreach (PropertyInfo property in __instance.GetType().GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        continue;
                    }

                    if (property.GetValue(__instance) is not IEnumerable items)
                    {
                        continue;
                    }

                    foreach (object? item in items)
                    {
                        if (item == null || !string.Equals(
                                GetStringProperty(item, "Id"),
                                "gold",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        SetStringProperty(
                            item,
                            "Value",
                            ChineseGoldFormatter.Format(gold));

                        return;
                    }
                }
            }
            catch
            {
                // Display-only modification: keep vanilla output if a game
                // build changes the internal MapInfoVM shape.
            }
        }

        private static void MissionConversationPostfix(object __instance)
        {
            try
            {
                SetStringProperty(
                    __instance,
                    "GoldText",
                    ChineseGoldFormatter.Format(GetPlayerGold()));
            }
            catch
            {
                // Keep vanilla output on an unexpected game build.
            }
        }

        private static void GoldTextSetterPrefix(object __instance, ref string value)
        {
            try
            {
                if (!IsGoldBarterItem(__instance))
                {
                    return;
                }

                value = ChineseGoldFormatter.Format(
                    GetIntProperty(__instance, "CurrentOfferedAmount"));
            }
            catch
            {
                // Keep vanilla output.
            }
        }

        private static bool IsGoldBarterItem(object instance)
        {
            object? barterable = GetPropertyValue(instance, "Barterable");
            object? group = barterable == null
                ? null
                : GetPropertyValue(barterable, "Group");

            return group != null && string.Equals(
                group.GetType().Name,
                "GoldBarterGroup",
                StringComparison.Ordinal);
        }

        private static int GetPlayerGold()
        {
            Type? heroType = FindType("TaleWorlds.CampaignSystem.Hero");
            object? hero = heroType?.GetProperty(
                    "MainHero",
                    BindingFlags.Static | BindingFlags.Public)
                ?.GetValue(null);

            return hero is null
                ? 0
                : GetIntProperty(hero, "Gold");
        }

        private static int GetIntProperty(object instance, string propertyName)
        {
            object? value = GetPropertyValue(instance, propertyName);
            return value is int number ? number : 0;
        }

        private static string? GetStringProperty(object instance, string propertyName)
        {
            object? value = GetPropertyValue(instance, propertyName);
            return value as string;
        }

        private static void SetStringProperty(
            object instance,
            string propertyName,
            string value)
        {
            PropertyInfo? property = instance.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.SetMethod != null && property.PropertyType == typeof(string))
            {
                property.SetValue(instance, value);
            }
        }

        private static object? GetPropertyValue(
            object instance,
            string propertyName)
        {
            return instance.GetType().GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(instance);
        }

        private static Type? FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type? type = assembly.GetType(fullName, false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
