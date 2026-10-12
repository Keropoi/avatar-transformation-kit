using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keropoi.ScanKit.Editor
{
    public sealed class ScanSetupData : ScriptableObject
    {
        public string avatarName;
        public string hipsPath = "";
        public string haloPath = "Angelring_Halo_White";
        public string haloVisualPath = "Halo_Position";
        public string haloControllerPath = "Assets/comet_shop/Angelring_Halo/Animation/FX_Angelring_Halo.controller";
        public string wingName = "";
        public string bodyPath = "";
        public bool usePlumAdapter;
        public bool useDressingToolsAdapter;
        public bool useWingAdapter;
        public bool allowSimpleMaterialConversion;
        public string audioPath;
        public float audioLeadSeconds = .7f;
        public float audioVolume = .65f;
        public float top = 1.52f;
        public float haloStartHeight = 1.3137f;
        public float riseEnd = 1.2f;
        public float expandedScale = 7.8f;
        public float bottom = -0.08f;
        public float duration = 3.65f;
        public float scanStart = 2.2f;
        public float scanEnd = 3.3f;
        public float edgeWidth = 0.012f;
        public List<string> groupsA = new List<string>();
        public List<string> groupsB = new List<string>();
        public List<ScanRenderer> renderers = new List<ScanRenderer>();
        public AnimationClip idleA, idleB, forward, reverse;
        public List<ScanBodyMask> bodyMasks = new List<ScanBodyMask>();
    }

    [Serializable]
    public sealed class ScanBodyMask
    {
        public string path;
        public string shape;
        public string source;
        public float outfitA;
        public float outfitB;
    }

    [Serializable]
    public sealed class ScanRenderer
    {
        public string path;
        public bool outfitB;
        public Material[] originals;
        public Material[] scanning;
        public Vector3 direction;
        public float scale;
        public float offset;
        public float Threshold(float height) => (outfitB ? 1 : -1) * (height - offset) / scale;
    }
}
