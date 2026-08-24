using System;
using System.Reflection;
using HarmonyLib;

namespace ChineseGold
{
    internal static class CampaignHudGoldPatch
    {
        private static Harmony? _harmony;

        public static void Apply(Harmony harmony)
        {
            _harmony = harmony;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in GetTypes(assembly))
                {
                    if (!type.FullName?.Contains("ViewModelCollection", StringComparison.Ordinal) == true)
                    {
                        continue;
                    }

                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (property.PropertyType != typeof(string) || property.SetMethod == null)
                        {
                            continue;
                        }

                        if (!property.Name.Contains("Gold", StringComparison.OrdinalIgnoreCase) &&
                            !property.Name.Contains("Denar", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        _harmony.Patch(
                            property.SetMethod,
                            prefix: new HarmonyMethod(typeof(CampaignHudGoldPatch), nameof(GoldSetterPrefix)));
                    }
                }
            }
        }

        private static void GoldSetterPrefix(ref string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (int.TryParse(value.Replace(",", string.Empty), out int gold))
            {
                value = ChineseGoldFormatter.Format(gold);
            }
        }

        private static Type[] GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types ?? Array.Empty<Type>();
            }
        }
    }
}
