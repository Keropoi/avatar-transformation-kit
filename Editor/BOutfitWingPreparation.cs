using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Keropoi.ScanKit.Editor
{
    // Optional scene accessory. Process after MA/VRCFury rig assembly, before shader optimization.
    public sealed class BOutfitWingPreparation : IVRCSDKPreprocessAvatarCallback
    {
        static string WingName => ScanBuilder.Data ? ScanBuilder.Data.wingName : "";
        // lilToon scans material/animation references at 100. VRCFury saves transient assets
        // at int.MaxValue - 100. Creating scan materials after either step breaks uploads.
        public int callbackOrder => 50;

        public bool OnPreprocessAvatar(GameObject root)
        {
            if (!ScanBuilder.Data || !ScanBuilder.Data.useWingAdapter) return true;
            var wing = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == WingName);
            if (!wing) return true;
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (!descriptor) return true;
            var fx = descriptor.baseAnimationLayers.FirstOrDefault(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
            if (!fx) return true;
            var clips = fx.animationClips.Distinct().Where(c =>
                new[] { "KAT_Idle_A", "KAT_Idle_B", "KAT_A_to_B", "KAT_B_to_A" }.Contains(ClipName(c)) &&
                AnimationUtility.GetCurveBindings(c).Any(b => b.propertyName == "material._DissolveParams.z")).ToArray();
            if (clips.Length == 0) return true;
            var d = ScanBuilder.Data;
            var hips = root.transform.Find(d.hipsPath);
            if (!d || !hips) throw new InvalidOperationException("Wing scan requires ScanSetup and avatar Hips.");
            var renderers = wing.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var renderer in renderers)
                foreach (var material in renderer.sharedMaterials)
                    if (!material || !material.HasProperty("_DissolveParams") || !material.HasProperty("_DissolvePos"))
                        throw new InvalidOperationException("Wing material does not support plane dissolve: " + renderer.name);

            // The halo plane follows Hips. Use the same shader object space even when Chest
            // or wing PhysBones rotate; skin bones, bind poses and attachment remain unchanged.
            var matrix = root.transform.worldToLocalMatrix * hips.localToWorldMatrix;
            var axis = new Vector3(matrix.m10, matrix.m11, matrix.m12);
            float scale = axis.magnitude, offset = matrix.m13;
            var direction = axis.normalized; // B is visible above the descending plane.
            var wingPath = AnimationUtility.CalculateTransformPath(wing, root.transform);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                RebaseBounds(renderer, hips);
                var originals = renderer.sharedMaterials;
                var scanning = originals.Select((source, slot) =>
                {
                    // Retain transparent rendering, textures and neon emission from the placed asset.
                    // VRCFury only persists objects created by its own object factory. Our
                    // materials must be real assets before clips/prefabs are serialized for upload.
                    var assetPath = ScanBuilder.Folder + "/Materials/B_WingScan_" + rendererIndex + "_" + slot + ".mat";
                    var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    if (!material)
                    {
                        material = new Material(source);
                        AssetDatabase.CreateAsset(material, assetPath);
                    }
                    material.shader = source.shader;
                    material.CopyPropertiesFromMaterial(source);
                    material.name = source.name + " Wing Scan";
                    material.SetVector("_DissolveParams", new Vector4(3, 1, (d.top - offset) / scale, d.edgeWidth / scale));
                    material.SetVector("_DissolvePos", direction);
                    material.SetFloat("_DissolveNoiseStrength", 0);
                    material.SetColor("_DissolveColor", new Color(2.5f, 1.7f, .55f, 1));
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssetIfDirty(material);
                    return material;
                }).ToArray();
                var path = AnimationUtility.CalculateTransformPath(renderer.transform, root.transform);
                foreach (var clip in clips)
                {
                    var name = ClipName(clip);
                    bool moving = name == "KAT_A_to_B" || name == "KAT_B_to_A";
                    bool toB = name == "KAT_A_to_B" || name == "KAT_Idle_B";
                    var materials = moving ? scanning : originals;
                    for (int i = 0; i < materials.Length; i++)
                        AnimationUtility.SetObjectReferenceCurve(clip,
                            EditorCurveBinding.PPtrCurve(path, typeof(SkinnedMeshRenderer), "m_Materials.Array.data[" + i + "]"),
                            new[] { new ObjectReferenceKeyframe { time = 0, value = materials[i] } });
                    Constant(clip, path, typeof(SkinnedMeshRenderer), "material._DissolveParams.x", moving ? 3 : 0);
                    Constant(clip, path, typeof(SkinnedMeshRenderer), "material._DissolveParams.y", 1);
                    Constant(clip, path, typeof(SkinnedMeshRenderer), "material._DissolveParams.w", d.edgeWidth / scale);
                    for (int i = 0; i < 3; i++)
                        Constant(clip, path, typeof(SkinnedMeshRenderer), "material._DissolvePos." + "xyz"[i], direction[i]);
                    if (moving)
                        AnimationUtility.SetEditorCurve(clip,
                            EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer), "material._DissolveParams.z"),
                            ScanMotion.Map(ScanMotion.Plane(d, toB), 1 / scale, -offset / scale));
                    else
                        Constant(clip, path, typeof(SkinnedMeshRenderer), "material._DissolveParams.z", ((toB ? d.bottom : d.top) - offset) / scale);
                }
            }
            foreach (var clip in clips)
            {
                // Both transitions keep the renderer active; only the shared plane controls reveal.
                Constant(clip, wingPath, typeof(GameObject), "m_IsActive", ClipName(clip) == "KAT_Idle_A" ? 0 : 1);
            }
            wing.gameObject.SetActive(false);
            Debug.Log("[ScanKit] Added complementary B wing scan to " + renderers.Length + " renderers in " + clips.Length + " build clips: " + wingPath);
            return true;
        }

        static void RebaseBounds(SkinnedMeshRenderer renderer, Transform hips)
        {
            var previousSpace = renderer.rootBone ? renderer.rootBone : renderer.transform;
            var matrix = hips.worldToLocalMatrix * previousSpace.localToWorldMatrix;
            var bounds = renderer.localBounds;
            var converted = new Bounds(matrix.MultiplyPoint3x4(bounds.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        converted.Encapsulate(matrix.MultiplyPoint3x4(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z))));
            renderer.rootBone = hips;
            renderer.localBounds = converted;
        }

        // A single constant key avoids extending the existing short idle clips.
        static void Constant(AnimationClip clip, string path, Type type, string property, float value)
            => AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), new AnimationCurve(new Keyframe(0, value)));

        static string ClipName(AnimationClip clip) => clip.name.Substring(clip.name.LastIndexOf('/') + 1);
    }
}
