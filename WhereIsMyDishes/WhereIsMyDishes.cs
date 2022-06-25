using HarmonyLib;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class WhereIsMyDishes : Mod
{
    //private static WhereIsMyDishes _instance;
    private Harmony harmony;
    private string harmonyID= "db42.wimd";
    private static string configPath;
    private static JSONObject configData;
    private static string logName = "<color=#204080>[DB42.WhereIsMyDishes]</color>  ";
    private static System.Random rand=new System.Random();


    public void Start()
    {
        harmony = new Harmony(harmonyID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        if (RAPI.IsCurrentSceneGame()) Enable();
        Info("Loaded!");
    }

    public void OnModUnload()
    {
        harmony.UnpatchAll(harmonyID);
        Disable();
    }

    public override void WorldEvent_WorldLoaded() => Enable();

    public override void WorldEvent_WorldUnloaded() => Disable();


    #region Features
    private void Enable()
    {
        configPath = Path.Combine(SaveAndLoad.WorldPath,SaveAndLoad.CurrentGameFileName, "WIMD.json");
        configData = configLoad();
        PatchRecipes(AltJar);
    }
    private void Disable()
    {
        configPath = null;
        configData = null;
    }

    public static bool RandomCheck(float prob) {
        return  rand.NextDouble()*100<prob;
    }

    private static void PatchRecipes(bool state)
    {
        Item_Base jar = ItemManager.GetItemByName("Jar_Honey");
        if (state)
            jar.settings_recipe.NewCost = new CostMultiple[]
            {
            new CostMultiple(new Item_Base[] {ItemManager.GetItemByName("HoneyComb")}, 6),
            new CostMultiple(new Item_Base[] {ItemManager.GetItemByName("DrinkingGlass")}, 1)
            };
        else
            jar.settings_recipe.NewCost = new CostMultiple[]
            {
            new CostMultiple(new Item_Base[] {ItemManager.GetItemByName("HoneyComb")}, 6),
            new CostMultiple(new Item_Base[] {ItemManager.GetItemByName("Glass")}, 1)
            };
    }

    private static void Info(object message) {
        Debug.Log(logName + message.ToString());
    }
    #endregion



    #region Config Properties
    public static int Claybowl
    {
        get
        {
            if (configData.IsNull || !configData.HasField("Claybowl"))
                return 60;
            return (int)configData.GetField("Claybowl").n;
        }
        set
        {
            if (!configData.IsNull && configData.HasField("Claybowl"))
                configData.SetField("Claybowl", value);
            else
                configData.AddField("Claybowl", value);
        }
    }

    public static int Bucket
    {
        get
        {
            if (configData.IsNull || !configData.HasField("Bucket"))
                return 80;
            return (int)configData.GetField("Bucket").n;
        }
        set
        {
            if (!configData.IsNull && configData.HasField("Bucket"))
                configData.SetField("Bucket", value);
            else
                configData.AddField("Bucket", value);
        }
    }

    public static int Jar
    {
        get
        {
            if (configData.IsNull || !configData.HasField("Jar"))
                return 90;
            return (int)configData.GetField("Jar").n;
        }
        set
        {
            if (!configData.IsNull && configData.HasField("Jar"))
                configData.SetField("Jar", value);
            else
                configData.AddField("Jar", value);
        }
    }

    public static int Glass
    {
        get
        {
            if (configData.IsNull || !configData.HasField("Glass"))
                return 90;
            return (int)configData.GetField("Glass").n;
        }
        set
        {
            if (!configData.IsNull && configData.HasField("Glass"))
                configData.SetField("Glass", value);
            else
                configData.AddField("Glass", value);
        }
    }

    public static bool AltJar
    {
        get
        {
            if (configData.IsNull || !configData.HasField("AltJar"))
                return true;
            return configData.GetField("AltJar").b;
        }
        set
        {
            if (!configData.IsNull && configData.HasField("AltJar"))
                configData.SetField("AltJar", value);
            else
                configData.AddField("AltJar", value);
        }
    }
    #endregion



    #region ConfigFile
    private static JSONObject configReset()
    {
        JSONObject data = JSONObject.Create();
        data.SetField("Claybowl", 60);
        data.SetField("Bucket", 80);
        data.SetField("Jar", 90);
        data.SetField("Glass", 90);
        data.SetField("AltJar", true);
        return data;
    }

    private static JSONObject configLoad()
    {
        JSONObject data;
        try
        {
            data = new JSONObject(File.ReadAllText(configPath));
        }
        catch
        {
            data =  configReset();
            configSave(data);
        }
        return data;
    }

    private static void configSave(JSONObject data)
    {
        try
        {
            File.WriteAllText(configPath, data.ToString());
        }
        catch (Exception err)
        {
            Info("Unable to save settings: " + err.Message);
        }
    }
    #endregion



    #region Console
    [ConsoleCommand(name: "wimd", docs: "Syntax: 'wimd <parameter> [value]'  Executes command or sets the value. Use without parameters to get help")]
    public static string MyCommand(string[] args)
    {
        if (!RAPI.IsCurrentSceneGame()) return "Available only in the world";
        if (args.Length == 2)
        {
            switch (args[0].ToLower())
            {
                case "claybowl":
                case "bowl":
                    Claybowl = Int32.Parse(args[1]); break;
                case "milkbucket":
                case "bucket":
                    Bucket = Int32.Parse(args[1]); break;
                case "honeyjar":
                case "jar":
                    Jar = Int32.Parse(args[1]); break;
                case "drinkingglass":
                case "glass":
                    Glass = Int32.Parse(args[1]); break;
                case "altHoneyJar":
                case "altJar":
                    AltJar = Boolean.Parse(args[1]); break;
            }
            configSave(configData);
        }
        if (args.Length == 0 || args[0].ToLower() == "help")
        {
            return "Available commands:" +
                "\n\t<nothing> | help - shows this" +
                "\n\tclaybowl | bowl [value] - sets the chance (%) to get your claybowl back" +
                "\n\tmilkbucket | bucket [value] - sets the chance (%) to get your claybowl back" +
                "\n\thoneyjar | jar [value] - sets the chance (%) to get glass from a jar back" +
                "\n\tdrinkingglass | glass [value] - sets the chance (%) to get your bucket glass back" +
                "\n\taltHoneyjar | altJar [value] - enables/disables/toggles the alternative recipe for honey jar" +
                "\nCurrent config" +
                "\n\tClaybowl: " + Claybowl +" %"+
                "\n\tMilk Bucket: " + Bucket + " %" +
                "\n\tHoney Jar: " + Jar + " %" +
                "\n\tDrinking Glass: " + Glass + " %" +
                "\n\tAlternative Honey Jar: " + AltJar.ToString();
        }
        else if (args.Length == 1|| args.Length == 2) {
            switch (args[0].ToLower()) {
                case "claybowl":
                case "bowl":
                    return "Current value: " + Claybowl + " %";
                case "milkbucket":
                case "bucket":
                    return "Current value: " + Bucket + " %";
                case "honeyjar":
                case "jar":
                    return "Current value: " + Jar + " %";
                case "drinkingglass":
                case "glass":
                    return "Current value: " + Glass + " %";
                case "altHoneyJar":
                case "altJar":
                    return "Current value: " + AltJar.ToString();
                default:return "Unknown parameter";
            }
        }
        else
            return "Too many arguments";
    }
    #endregion



    #region ExtraSettings

    public void ExtraSettingsAPI_SettingsOpen()
    {
        if (!RAPI.IsCurrentSceneGame()) return;
        ExtraSettingsAPI_SetInputValue("Claybowl", Claybowl.ToString()); ;
        ExtraSettingsAPI_SetInputValue("Milk Bucket", Bucket.ToString());
        ExtraSettingsAPI_SetInputValue("Honey Jar", Jar.ToString());
        ExtraSettingsAPI_SetInputValue("Drinking Glass", Glass.ToString());
        ExtraSettingsAPI_SetCheckboxState("Alternative Honey Jar", AltJar);
    }

    public void ExtraSettingsAPI_SettingsClose()
    {
        if (!RAPI.IsCurrentSceneGame()) return;

        try
        {
            Claybowl = Mathf.Clamp(Int32.Parse(ExtraSettingsAPI_GetInputValue("Claybowl")), 0,100);
        }
        catch (Exception e)
        {
            Debug.LogError($"Couldn't parse \"{ExtraSettingsAPI_GetInputValue("Claybowl")}\"\n{e}");
        }
        try
        {
            Bucket = Mathf.Clamp(Int32.Parse(ExtraSettingsAPI_GetInputValue("Milk Bucket")), 0, 100);
        }
        catch (Exception e)
        {
            Debug.LogError($"Couldn't parse \"{ExtraSettingsAPI_GetInputValue("Milk Bucket")}\"\n{e}");
        }
        try
        {
            Jar = Mathf.Clamp(Int32.Parse(ExtraSettingsAPI_GetInputValue("Honey Jar")), 0, 100);
        }
        catch (Exception e)
        {
            Debug.LogError($"Couldn't parse \"{ExtraSettingsAPI_GetInputValue("Honey Jar")}\"\n{e}");
        }
        try
        {
            Glass = Mathf.Clamp(Int32.Parse(ExtraSettingsAPI_GetInputValue("Drinking Glass")), 0, 100);
        }
        catch (Exception e)
        {
            Debug.LogError($"Couldn't parse \"{ExtraSettingsAPI_GetInputValue("Drinking Glass")}\"\n{e}");
        }
        AltJar = ExtraSettingsAPI_GetCheckboxState("Alternative Honey Jar");
        configSave(configData);
    }

    public void ExtraSettingsAPI_ButtonPress(string name)
    {
        if (name == "Reset config")
        {
            configData = configReset();
            ExtraSettingsAPI_SettingsOpen();
        }
    }

    static bool ExtraSettingsAPI_Loaded = false;
    public static bool ExtraSettingsAPI_GetCheckboxState(string SettingName) => false;
    public static string ExtraSettingsAPI_GetInputValue(string SettingName) => "";
    public static void ExtraSettingsAPI_SetCheckboxState(string SettingName, bool value) { }
    public static void ExtraSettingsAPI_SetInputValue(string SettingName, string value) { }

    #endregion
}



#region Harmony

[HarmonyPatch(typeof(CookingTable)), HarmonyPatch("StartCooking")]
internal class CookingPatch
{
    private static void Prefix(CookingTable __instance, SO_CookingTable_Recipe recipe)
    {
        if (recipe != null&&__instance.CanStartCooking())
{
            for (int i = 0; i < __instance.Slots.Length; i++)
            {
                if (__instance.Slots[i].CurrentItem.UniqueName == "Bucket_Milk"
                    && WhereIsMyDishes.RandomCheck(WhereIsMyDishes.Bucket))
                {
                    RAPI.GetLocalPlayer().Inventory.AddItem("Bucket", 1);
                }
            }

        }
    }
}

[HarmonyPatch(typeof(PlayerStats)), HarmonyPatch("Consume")]
internal class ConsumePatch
{
    private static void Postfix(PlayerStats __instance, Item_Base edibleItem)
    {
        if (__instance.GetComponent(typeof(Network_Player)) == RAPI.GetLocalPlayer())
        {
            if ((edibleItem.UniqueName.StartsWith("Claybowl") || edibleItem.UniqueName.StartsWith("Clayplate"))
                && WhereIsMyDishes.RandomCheck(WhereIsMyDishes.Claybowl))
            {
                Debug.Log(true);
                RAPI.GetLocalPlayer().Inventory.AddItem("Claybowl_Empty",1);
            }
            else if (edibleItem.UniqueName.StartsWith("DrinkingGlass")
                && WhereIsMyDishes.RandomCheck(WhereIsMyDishes.Glass))
            {
                RAPI.GetLocalPlayer().Inventory.AddItem("DrinkingGlass", 1);
            }
            else if (edibleItem.UniqueName == "Jar_Honey"
                && WhereIsMyDishes.RandomCheck(WhereIsMyDishes.Jar))
            {
                if (WhereIsMyDishes.AltJar)
                {
                    RAPI.GetLocalPlayer().Inventory.AddItem("DrinkingGlass", 1);
                }
                else
                {
                    RAPI.GetLocalPlayer().Inventory.AddItem("Glass", 1);
                }
            }
        }
    }
}


[HarmonyPatch(typeof(Tank)), HarmonyPatch("ModifyTank")]
internal class ModifyTankPatch
{
    private static void Postfix(Tank __instance, Network_Player player, float amount, Item_Base itemType = null)
    {
        if (player == RAPI.GetLocalPlayer())
        {
            if (itemType.UniqueName == "Jar_Honey"
                && WhereIsMyDishes.RandomCheck(WhereIsMyDishes.Jar)) {
                if (WhereIsMyDishes.AltJar)
                    RAPI.GetLocalPlayer().Inventory.AddItem("DrinkingGlass", 1);
                else
                    RAPI.GetLocalPlayer().Inventory.AddItem("Glass", 1);
            }
        }
    }
}

#endregion