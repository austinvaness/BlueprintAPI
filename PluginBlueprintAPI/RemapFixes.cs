using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.BlueprintAPI.Plugin
{
    [HarmonyPatch()]
    public class RemapFixes
    {
        public static bool Enabled { get; set; }


    }
}
