using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

public static class EnumExtensions
{
    // 🎯 Dictionnaire des valeurs custom
    public static readonly Dictionary<Type, Dictionary<int, string>> customEnumValues = new Dictionary<Type, Dictionary<int, string>>
    {
        {
            typeof(EItemType), new Dictionary<int, string>
            {
                { 125, "BoosterStellar" },
                { 126, "DisplayStellar" },
                { 127, "BoosterStellarTaux" },
                { 128, "DisplayStellarTaux" },
                { 129, "CaleconStellar" },
                { 130, "StarterApocalypse" },
                { 131, "StarterShowtime" },
                { 132, "TapisS41" },
                { 133, "TapisS42" },
                { 134, "ClasseurS4" }
            }
        },
        {
            typeof(ECollectionPackType), new Dictionary<int, string>
            {
                { 15, "Stellar" },
                { 16, "StellarTaux" },
            }
        }
    };

    public static readonly Dictionary<Type, Dictionary<int, int>> remappedEnumValues = new Dictionary<Type, Dictionary<int, int>>
    {
        {
            typeof(ECollectionPackType), new Dictionary<int, int>
            {
                { 15, 17 },
            }
        }
    };

    public static EItemType SafeParseEItemType(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("[WankulCrazy] Erreur JSON: Valeur itemType vide ou null !");
            return (EItemType)0; // Valeur par défaut
        }

        // Vérifie si c'est une valeur définie dans l'Enum
        if (Enum.TryParse(typeof(EItemType), value, true, out object result))
        {
            return (EItemType)result;
        }

        // Vérifie si c'est une valeur custom
        if (EnumExtensions.customEnumValues[typeof(EItemType)].ContainsValue(value))
        {
            return (EItemType)EnumExtensions.customEnumValues[typeof(EItemType)].FirstOrDefault(x => x.Value == value).Key;
        }

        Debug.LogError($"[WankulCrazy] Erreur JSON: '{value}' n'est pas une valeur valide pour EItemType.");
        return (EItemType)0; // Valeur par défaut
    }

    public static ECollectionPackType SafeParseECollectionPackType(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("[WankulCrazy] Erreur JSON: Valeur itemType vide ou null !");
            return (ECollectionPackType)0; // Valeur par défaut
        }

        // Vérifie si c'est une valeur définie dans l'Enum
        if (Enum.TryParse(typeof(ECollectionPackType), value, true, out object result))
        {
            return (ECollectionPackType)result;
        }

        // Vérifie si c'est une valeur custom
        if (EnumExtensions.customEnumValues[typeof(ECollectionPackType)].ContainsValue(value))
        {
            return (ECollectionPackType)EnumExtensions.customEnumValues[typeof(ECollectionPackType)].FirstOrDefault(x => x.Value == value).Key;
        }

        Debug.LogError($"[WankulCrazy] Erreur JSON: '{value}' n'est pas une valeur valide pour EItemType.");
        return (ECollectionPackType)0; // Valeur par défaut
    }

    public static string GetEnumName(Type enumType, int value)
    {
        if (Enum.IsDefined(enumType, value))
            return Enum.GetName(enumType, value);
        if (customEnumValues.ContainsKey(enumType) && customEnumValues[enumType].ContainsKey(value))
            return customEnumValues[enumType][value];
        return "Unknown";
    }

    public static bool IsValidEnumValue(Type enumType, int value)
    {
        return Enum.IsDefined(enumType, value) ||
               (customEnumValues.ContainsKey(enumType) && customEnumValues[enumType].ContainsKey(value));
    }
}

// 🎯 Patch (int)myitem.type → Supporte 999
class Patch_Enum_Transpiler
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => AccessTools.GetDeclaredMethods(t))
            .Where(m => m.GetParameters().Any(p => EnumExtensions.customEnumValues.ContainsKey(p.ParameterType)) ||
                        EnumExtensions.customEnumValues.ContainsKey(m.ReturnType));
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Conv_I4)
            {
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call,
                    typeof(Patch_Enum_Transpiler).GetMethod(nameof(HandleCustomEnumValue))));
            }
        }

        return codes;
    }

    public static int HandleCustomEnumValue(int originalValue)
    {
        foreach (var customEnum in EnumExtensions.customEnumValues)
        {
            // Si la valeur existe dans le dictionnaire custom, on la garde
            if (customEnum.Value.ContainsKey(originalValue))
            {
                UnityEngine.Debug.Log($"Custom Enum détecté : {originalValue}");
                return originalValue;
            }
        }

        // Vérifie si la valeur doit être remappée
        foreach (var remappedEnum in EnumExtensions.remappedEnumValues)
        {
            if (remappedEnum.Value.ContainsKey(originalValue))
            {
                int newValue = remappedEnum.Value[originalValue];
                UnityEngine.Debug.Log($"Valeur Enum remappée : {originalValue} → {newValue}");
                return newValue;
            }
        }

        return originalValue;
    }
}

// 🎯 Patch == et != → Supporte 999
class Patch_Enum_Comparison
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => AccessTools.GetDeclaredMethods(t))
            .Where(m => m.GetMethodBody() != null);
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count - 1; i++)
        {
            if (codes[i].opcode == OpCodes.Beq || codes[i].opcode == OpCodes.Bne_Un)
            {
                codes.Insert(i, new CodeInstruction(OpCodes.Call,
                    typeof(Patch_Enum_Comparison).GetMethod(nameof(HandleEnumComparison))));
            }
        }

        return codes;
    }

    public static bool HandleEnumComparison(int a, int b)
    {
        foreach (var customEnum in EnumExtensions.customEnumValues)
        {
            if (customEnum.Value.ContainsKey(a) || customEnum.Value.ContainsKey(b))
                return a == b;
        }
        return a == b;
    }
}

// 🎯 Patch Enum.GetName() et Enum.IsDefined()
class Patch_Enum_GetName
{
    static bool Prefix(Type enumType, object value, ref string __result)
    {
        if (EnumExtensions.customEnumValues.ContainsKey(enumType) &&
            EnumExtensions.customEnumValues[enumType].TryGetValue((int)value, out string name))
        {
            __result = name;
            return false; // Skip l'original
        }
        return true;
    }
}

class Patch_Enum_IsDefined
{
    static bool Prefix(Type enumType, object value, ref bool __result)
    {
        if (EnumExtensions.customEnumValues.ContainsKey(enumType))
        {
            __result = EnumExtensions.IsValidEnumValue(enumType, (int)value);
            return false; // Skip l'original
        }
        return true;
    }
}

// 🎯 Patch Enum.Parse() pour supporter CustomItem
class Patch_Enum_Parse
{
    static bool Prefix(Type enumType, string value, bool ignoreCase, ref object __result)
    {
        if (EnumExtensions.customEnumValues.ContainsKey(enumType))
        {
            // Vérifie si la valeur existe dans l'Enum d'origine
            if (Enum.IsDefined(enumType, value))
            {
                int parsedValue = (int)Enum.Parse(enumType, value, ignoreCase);

                // Vérifie si la valeur doit être remappée
                if (EnumExtensions.remappedEnumValues.ContainsKey(enumType) &&
                    EnumExtensions.remappedEnumValues[enumType].ContainsKey(parsedValue))
                {
                    parsedValue = EnumExtensions.remappedEnumValues[enumType][parsedValue];
                }

                __result = (Enum)Enum.ToObject(enumType, parsedValue);
                return false; // Skip l'original
            }

            // Vérifie si c'est une valeur custom
            if (EnumExtensions.customEnumValues[enumType].ContainsValue(value))
            {
                __result = (Enum)Enum.ToObject(enumType, EnumExtensions.customEnumValues[enumType].FirstOrDefault(x => x.Value == value).Key);
                return false; // Skip l'original
            }

            Debug.LogError($"[WankulCrazy] Erreur JSON: '{value}' n'est pas une valeur valide pour {enumType.Name}.");
            __result = Activator.CreateInstance(enumType); // Valeur par défaut
            return false; // Skip l'original
        }

        return true; // Continue normalement pour les autres Enums
    }
}