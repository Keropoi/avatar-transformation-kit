using System.Collections.Generic;
using UnityEngine;

namespace Keropoi.ScanKit.Editor
{
    // Object references and generated materials stay in the consumer's project.
    [CreateAssetMenu(menuName = "ScanKit/Plum recipe")]
    public sealed class KitRecipe : ScriptableObject
    {
        public ScanSetupData settings;
        public bool requireTestedVersions = true;
    }

}
