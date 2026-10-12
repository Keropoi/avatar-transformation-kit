using System.Collections.Generic;
using UnityEngine;
namespace Keropoi.ScanKit.Editor
{
    public sealed class KitRegistry : ScriptableObject
    {
        public string owner = "com.keropoi.avatar-transformation";
        public string sourceId;
        public string installedId;
        public string sourceScene;
        public bool sourceWasActive;
        public string folder;
        public KitRecipe recipe;
        public List<string> generations = new List<string>();
    }
}
