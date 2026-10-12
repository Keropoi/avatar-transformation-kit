using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;
using Newtonsoft.Json.Linq;

namespace Keropoi.ScanKit.Editor
{
    public sealed class ScanBuildPreparation : IVRCSDKPreprocessAvatarCallback
    {
        // DressingTools' preparation runs at -12000 and ignores inactive wearable roots.
        public int callbackOrder => -20000;
        public bool OnPreprocessAvatar(GameObject root)
        {
            try { Prepare(root); return true; }
            catch (Exception e) { Debug.LogError("[ScanKit] Preparation failed: " + e); return false; }
        }
        public static void Prepare(GameObject root)
        {
            if (!root.transform.Find(ScanBuilder.Module) || !ScanBuilder.Data) return;
            foreach (var path in ScanBuilder.Data.groupsA.Concat(ScanBuilder.Data.groupsB))
            {
                var group = root.transform.Find(path);
                if (!group) throw new InvalidOperationException("Outfit group missing: " + path);
                group.gameObject.SetActive(true);
                if (!ScanBuilder.Data.useDressingToolsAdapter) continue;
                // The prepared A/B selection is owned by the scan controller. Noir's legacy cabinet
                // module renames its root and writes competing on/off clips; preserve rig and shape modules.
                foreach (var component in group.GetComponents<Component>().Where(c => c && c.GetType().FullName == "Chocopoi.DressingTools.Components.OneConf.DTWearable"))
                {
                    var serialized = new SerializedObject(component);
                    var property = serialized.FindProperty("configJson");
                    var config = JObject.Parse(property.stringValue);
                    var modules = config["modules"] as JArray;
                    if (modules == null) continue;
                    var conflicts = modules.Where(m => (string)m["moduleName"] == "com.chocopoi.dressingtools.built-in.wearable.cabinet-anim").ToArray();
                    if (conflicts.Length == 0) continue;
                    foreach (var module in conflicts) module.Remove();
                    property.stringValue = config.ToString(Newtonsoft.Json.Formatting.None);
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }

    // Runs on the SDK's temporary avatar after MA and DressingTools have changed renderer root bones.
    public sealed class ScanBuildCalibration : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => int.MaxValue - 100;
        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try { Calibrate(avatarGameObject); return true; }
            catch (Exception e) { Debug.LogError("[ScanKit] Scan calibration failed: " + e); return false; }
        }

        // VRCFury prefixes copied clip names, e.g. "Copied from Plum_FX/A_to_B".
        static string ClipName(AnimationClip clip) => clip.name.Substring(clip.name.LastIndexOf('/') + 1);

        public static int Calibrate(GameObject root)
        {
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (!descriptor) return 0;
            var fx = descriptor.baseAnimationLayers.FirstOrDefault(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
            if (!fx || !fx.animationClips.Any(c => ClipName(c) == "KAT_A_to_B" && AnimationUtility.GetCurveBindings(c).Any(b => b.propertyName == "material._DissolveParams.z"))) return 0;
            var d = ScanBuilder.Data;
            if (!d) throw new InvalidOperationException("ScanSetup.asset is missing.");
            var clips = fx.animationClips.Where(c => new[] {"KAT_A_to_B", "KAT_B_to_A", "KAT_Idle_A", "KAT_Idle_B"}.Contains(ClipName(c))).Distinct().ToArray();
            var hips = root.transform.Find(d.hipsPath);
            if (!hips) throw new InvalidOperationException("Avatar hips not found.");
            // The movable sword needs skinning to express its cut in the same moving hip space.
            var paths = AnimationUtility.GetCurveBindings(clips.First(c => ClipName(c) == "KAT_A_to_B"))
                .Where(b => b.propertyName == "material._DissolveParams.z").Select(b => b.path).Distinct().ToArray();
            foreach (var path in paths)
            {
                var t = root.transform.Find(path);
                if (!t) throw new InvalidOperationException("Animated renderer missing: " + path);
                var mr = t.GetComponent<MeshRenderer>();
                if (!mr) continue;
                var source = t.GetComponent<MeshFilter>().sharedMesh;
                var mesh = UnityEngine.Object.Instantiate(source); mesh.name = source.name + " ScanSkin";
                mesh.bindposes = new[] { Matrix4x4.identity };
                mesh.boneWeights = Enumerable.Repeat(new BoneWeight { boneIndex0 = 0, weight0 = 1 }, mesh.vertexCount).ToArray();
                string meshPath = ScanBuilder.Folder + "/Skin_" + Array.IndexOf(paths, path) + ".asset";
                var savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if (savedMesh) { EditorUtility.CopySerialized(mesh, savedMesh); UnityEngine.Object.DestroyImmediate(mesh); mesh = savedMesh; }
                else AssetDatabase.CreateAsset(mesh, meshPath);
                AssetDatabase.SaveAssetIfDirty(mesh);
                var materials = mr.sharedMaterials; var shadow = mr.shadowCastingMode; var receive = mr.receiveShadows; var anchor = mr.probeAnchor;
                UnityEngine.Object.DestroyImmediate(mr);
                var sk = t.gameObject.AddComponent<SkinnedMeshRenderer>();
                sk.sharedMesh = mesh; sk.sharedMaterials = materials; sk.bones = new[] {t}; sk.rootBone = hips;
                sk.shadowCastingMode = shadow; sk.receiveShadows = receive; sk.probeAnchor = anchor;
                sk.localBounds = new Bounds(Vector3.zero, Vector3.one * 4); sk.updateWhenOffscreen = true;
                foreach (var clip in clips) RebindType(clip, path);
            }
            int count = 0;
            foreach (var clip in clips)
            {
                bool moving = ClipName(clip) == "KAT_A_to_B" || ClipName(clip) == "KAT_B_to_A";
                bool toB = ClipName(clip) == "KAT_A_to_B" || ClipName(clip) == "KAT_Idle_B";
                float len = moving ? d.duration : 1f / 60;
                foreach (var path in paths)
                {
                    var r = root.transform.Find(path).GetComponent<Renderer>();
                    var sk = r as SkinnedMeshRenderer;
                    if (sk && (!sk.rootBone || !sk.rootBone.IsChildOf(hips)))
                        throw new InvalidOperationException("Wearable is not bound to the avatar hips: " + path + ". Run DressingTools preparation before NDMF.");
                    if (sk && sk.rootBone != hips) RebaseBounds(sk, hips);
                    var matrix = root.transform.worldToLocalMatrix * (sk && sk.rootBone ? sk.rootBone.localToWorldMatrix : r.localToWorldMatrix);
                    var axis = new Vector3(matrix.m10, matrix.m11, matrix.m12);
                    // Infer ownership from the authored forward threshold direction. This survives
                    // MA reparenting without imposing product names or (B) suffixes on user objects.
                    var forward = clips.First(c => ClipName(c) == "KAT_A_to_B");
                    var thresholdCurve = AnimationUtility.GetEditorCurve(forward,
                        EditorCurveBinding.FloatCurve(path, r.GetType(), "material._DissolveParams.z"));
                    bool outfitB = thresholdCurve.Evaluate(d.scanStart) > thresholdCurve.Evaluate(d.scanEnd);
                    float sign = outfitB ? 1 : -1;
                    float scale = axis.magnitude, offset = matrix.m13;
                    var direction = sign * axis.normalized;
                    if (moving)
                        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, r.GetType(), "material._DissolveParams.z"),
                            ScanMotion.Map(ScanMotion.Plane(d, toB), sign / scale, -sign * offset / scale));
                    else
                    {
                        float threshold = sign * ((toB ? d.bottom : d.top) - offset) / scale;
                        SetCurve(clip, path, r.GetType(), "material._DissolveParams.z", new[] {0f, len}, new[] {threshold, threshold});
                    }
                    SetCurve(clip, path, r.GetType(), "material._DissolveParams.w", new[] {0f, len}, new[] {d.edgeWidth / scale, d.edgeWidth / scale});
                    for (int i = 0; i < 3; i++) SetCurve(clip, path, r.GetType(), "material._DissolvePos." + "xyz"[i], new[] {0f, len}, new[] {direction[i], direction[i]});
                    count++;
                }
            }
            // Read processed activation paths; their source paths may have changed during binding.
            var idle = clips.First(c => ClipName(c) == "KAT_Idle_A");
            foreach (var binding in AnimationUtility.GetCurveBindings(idle).Where(b => b.type == typeof(GameObject) && b.propertyName == "m_IsActive"))
            {
                var target = root.transform.Find(binding.path);
                if (target) target.gameObject.SetActive(AnimationUtility.GetEditorCurve(idle, binding).Evaluate(0) > .5f);
            }
            Debug.Log("[ScanKit] Calibrated " + paths.Length + " renderer planes after avatar preprocessing.");
            return count;
        }
        static void RebindType(AnimationClip clip, string path)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip).Where(b => b.path == path && b.type == typeof(MeshRenderer)))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding); AnimationUtility.SetEditorCurve(clip, binding, null);
                var target = binding; target.type = typeof(SkinnedMeshRenderer); AnimationUtility.SetEditorCurve(clip, target, curve);
            }
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip).Where(b => b.path == path && b.type == typeof(MeshRenderer)))
            {
                var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding); AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                var target = binding; target.type = typeof(SkinnedMeshRenderer); AnimationUtility.SetObjectReferenceCurve(clip, target, curve);
            }
        }
        static void RebaseBounds(SkinnedMeshRenderer renderer, Transform hips)
        {
            var matrix = hips.worldToLocalMatrix * renderer.rootBone.localToWorldMatrix;
            var bounds = renderer.localBounds;
            var converted = new Bounds(matrix.MultiplyPoint3x4(bounds.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        converted.Encapsulate(matrix.MultiplyPoint3x4(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x,y,z))));
            renderer.rootBone = hips;
            renderer.localBounds = converted;
        }
        static void SetCurve(AnimationClip clip, string path, Type type, string property, float[] times, float[] values)
        {
            var curve = new AnimationCurve(times.Select((t, i) => new Keyframe(t, values[i])).ToArray());
            for (int i = 0; i < curve.length; i++) { AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear); AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear); }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), curve);
        }
    }
}
