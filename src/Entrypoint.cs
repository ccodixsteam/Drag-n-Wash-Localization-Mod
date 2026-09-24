using System;
using System.Reflection;
using HarmonyLib;

namespace Doorstop
{
    public static class Entrypoint
    {
        public static void Start()
        {
            try
            {
                Harmony harmony = new Harmony("com.ccodix.dragnwash.localization");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LocalizationMod] Hook error: " + ex.Message);
            }
        }
    }
}
