using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Keropoi.ScanKit.Editor
{
    public static class ScanBuilder
    {
        public static string Folder => KitInstaller.WorkingFolder;
        public const string Module = "KAT Transformation Scan";
        public const string Parameter = "KAT/OutfitB";
        public static ScanSetupData Data => AssetDatabase.LoadAssetAtPath<ScanSetupData>(Folder + "/ScanSetup.asset");
        public static GameObject Avatar => UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.GetComponent<VRCAvatarDescriptor>() != null && (Data == null || g.name == Data.avatarName));
        static string PathOf(Transform t) => AnimationUtility.CalculateTransformPath(t, Avatar.transform);

        public static void Build()
        {
            if (EditorApplication.isPlaying || AnimationMode.InAnimationMode()) throw new InvalidOperationException("Stop play/preview before building.");
            var root = Avatar;
            if (!root) throw new InvalidOperationException("Avatar not found.");
            Directory.CreateDirectory(Folder + "/Materials");
            Directory.CreateDirectory(Folder + "/Animations");
            AssetDatabase.Refresh();
            var d = Data;
            if (d.renderers.Count == 0)
            {
                foreach (var groupPath in d.groupsA.Concat(d.groupsB))
                {
                    var group = root.transform.Find(groupPath);
                    bool b = d.groupsB.Contains(groupPath);
                    foreach (var r in group.GetComponentsInChildren<Renderer>(true))
                    {
                        if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) continue;
                        if (!r.enabled || !EnabledBelowGroup(r.transform, group)) continue;
                        var skinned = r as SkinnedMeshRenderer;
                        var matrix = root.transform.worldToLocalMatrix * (skinned && skinned.rootBone ? skinned.rootBone.localToWorldMatrix : r.localToWorldMatrix);
                        var y = new Vector3(matrix.m10, matrix.m11, matrix.m12);
                        var e = new ScanRenderer { path = PathOf(r.transform), outfitB = b, originals = r.sharedMaterials,
                            direction = y.normalized * (b ? 1 : -1), scale = y.magnitude, offset = matrix.m13 };
                        e.scanning = e.originals.Select((m, i) => MakeMaterial(m, e, d, d.renderers.Count, i)).ToArray();
                        d.renderers.Add(e);
                    }
                }
            }
            ScanMotion.Validate(d);
            foreach (var e in d.renderers)
            {
                var r = root.transform.Find(e.path).GetComponent<Renderer>();
                var sk = r as SkinnedMeshRenderer;
                var matrix = root.transform.worldToLocalMatrix * (sk && sk.rootBone ? sk.rootBone.localToWorldMatrix : r.localToWorldMatrix);
                var y = new Vector3(matrix.m10, matrix.m11, matrix.m12);
                e.direction = y.normalized * (e.outfitB ? 1 : -1); e.scale = y.magnitude; e.offset = matrix.m13;
                foreach (var m in e.scanning) { m.SetVector("_DissolveParams", new Vector4(3, 1, e.Threshold(d.top), d.edgeWidth / e.scale)); m.SetVector("_DissolvePos", e.direction); EditorUtility.SetDirty(m); }
            }
            Undo.RegisterFullObjectHierarchyUndo(root, "Build avatar scan");
            var module = root.transform.Find(Module);
            if (!module) { var go = new GameObject(Module); module = go.transform; module.SetParent(root.transform, false); }
            if (!module.Find("ScanHalo")) CreateHalo(module);
            var proxy = module.GetComponent<ModularAvatarBoneProxy>() ?? module.gameObject.AddComponent<ModularAvatarBoneProxy>();
            proxy.target = root.transform.Find(d.hipsPath); proxy.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
            var halo = module.Find("ScanHalo");
            var ring = halo.GetComponentInChildren<MeshFilter>(true).transform;
            var planeOffset = halo.InverseTransformPoint(ring.TransformPoint(ring.GetComponent<MeshFilter>().sharedMesh.bounds.center)).y;
            foreach (Transform visual in halo) visual.localPosition -= Vector3.up * planeOffset;
            ScanBodyMasks.Migrate(root, d);
            d.idleA = MakeClip(d, "KAT_Idle_A", false, false);
            d.idleB = MakeClip(d, "KAT_Idle_B", true, false);
            d.forward = MakeClip(d, "KAT_A_to_B", true, true);
            d.reverse = MakeClip(d, "KAT_B_to_A", false, true);
            MakeController(d, module.gameObject);
            ScanAudioSetup.Configure(d, module.gameObject);
            ScanHaloDefaults.Configure(root);
            // Match the accessory visibility the author prepared in the scene; keep their existing menu toggles usable.
            foreach (var settings in root.GetComponentsInChildren<ModularAvatarParameters>(true))
            {
                var serialized = new SerializedObject(settings);
                var parameters = serialized.FindProperty("parameters");
                for (int i = 0; i < parameters.arraySize; i++)
                {
                    var item = parameters.GetArrayElementAtIndex(i);
                    string name = item.FindPropertyRelative("nameOrPrefix").stringValue;
                    if (name != "Bag_On" && name != "GSToggle") continue;
                    item.FindPropertyRelative("defaultValue").floatValue = 1;
                    item.FindPropertyRelative("hasExplicitDefaultValue").boolValue = true;
                    item.FindPropertyRelative("m_overrideAnimatorDefaults").boolValue = true;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            ApplyIdle(false);
            EditorUtility.SetDirty(d);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[ScanKit] Created complementary scan for " + d.renderers.Count + " renderers.");
        }

        static bool EnabledBelowGroup(Transform t, Transform group)
        {
            while (t != group) { if (!t.gameObject.activeSelf) return false; t = t.parent; }
            return true;
        }

        static Material MakeMaterial(Material source, ScanRenderer e, ScanSetupData d, int index, int slot)
        {
            if (!source) throw new InvalidOperationException("Missing material: " + e.path);
            Material m;
            if (source.shader.name.ToLowerInvariant().Contains("liltoon"))
            {
                m = new Material(source);
                if (!source.shader.name.Contains("Transparent") && !source.shader.name.Contains("Refraction") && !source.shader.name.Contains("Cutout"))
                    lilToon.lilToonInspector.SetupMaterialWithRenderingMode(m, lilToon.RenderingMode.Cutout, lilToon.TransparentMode.Normal,
                        source.shader.name.Contains("Outline"), false, false, false);
            }
            else
            {
                // Use the original sword texture during the wipe, and restore its Poiyomi material at rest.
                m = new Material(Shader.Find("Hidden/lilToonCutout"));
                m.SetTexture("_MainTex", source.mainTexture);
                m.SetColor("_Color", source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white);
                if (source.HasProperty("_BumpMap")) { m.SetTexture("_BumpMap", source.GetTexture("_BumpMap")); m.SetFloat("_UseBumpMap", 1); }
                lilToon.lilToonInspector.SetupMaterialWithRenderingMode(m, lilToon.RenderingMode.Cutout, lilToon.TransparentMode.Normal, false, false, false, false);
            }
            m.name = (e.outfitB ? "B_" : "A_") + index.ToString("D2") + "_" + slot + "_" + source.name;
            m.SetVector("_DissolveParams", new Vector4(3, 1, e.Threshold(d.top), d.edgeWidth / e.scale));
            m.SetVector("_DissolvePos", e.direction);
            m.SetFloat("_DissolveNoiseStrength", 0);
            m.SetColor("_DissolveColor", new Color(2.5f, 1.7f, 0.55f, 1));
            var path = Folder + "/Materials/" + (e.outfitB ? "B" : "A") + index.ToString("D2") + "_" + slot + ".mat";
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static void CreateHalo(Transform module)
        {
            var source = Avatar.transform.Find(Data.haloPath + "/" + Data.haloVisualPath);
            if (!source) throw new InvalidOperationException("Source halo not found.");
            var halo = new GameObject("ScanHalo").transform;
            halo.SetParent(module, false);
            halo.localPosition = new Vector3(0, Data.top, -0.04f);
            var mat = new Material(source.GetComponentInChildren<Renderer>().sharedMaterial);
            mat.name = "Scan Halo White Gold";
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", new Color(1, .92f, .65f, 1));
            AssetDatabase.CreateAsset(mat, Folder + "/Materials/ScanHalo.mat");
            foreach (var r in source.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer) continue;
                var go = new GameObject(r.name);
                go.transform.SetParent(halo, false);
                go.transform.localPosition = Avatar.transform.InverseTransformVector(r.transform.position - source.position);
                go.transform.localRotation = Quaternion.Inverse(Avatar.transform.rotation) * r.transform.rotation;
                go.transform.localScale = r.transform.lossyScale;
                Mesh mesh;
                var skinned = r as SkinnedMeshRenderer;
                if (skinned)
                {
                    mesh = new Mesh { name = "ScanHaloMesh" };
                    skinned.BakeMesh(mesh);
                    AssetDatabase.CreateAsset(mesh, Folder + "/ScanHaloMesh_" + r.name + ".asset");
                }
                else mesh = r.GetComponent<MeshFilter>().sharedMesh;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            halo.gameObject.SetActive(false);
        }

        static AnimationClip MakeClip(ScanSetupData d, string name, bool b, bool moving)
        {
            var path = Folder + "/Animations/" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (!clip) { clip = new AnimationClip { name = name, frameRate = 60 }; AssetDatabase.CreateAsset(clip, path); }
            clip.ClearCurves();
            float len = moving ? d.duration : 1f / 60;
            foreach (var g in d.groupsA) Constant(clip, g, typeof(GameObject), "m_IsActive", moving || !b ? 1 : 0, len);
            foreach (var g in d.groupsB) Constant(clip, g, typeof(GameObject), "m_IsActive", moving || b ? 1 : 0, len);
            foreach (var e in d.renderers)
            {
                var renderer = Avatar.transform.Find(e.path).GetComponent<Renderer>();
                var type = renderer.GetType();
                var mats = moving ? e.scanning : e.originals;
                for (int i = 0; i < mats.Length; i++)
                {
                    AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve(e.path, type, "m_Materials.Array.data[" + i + "]"),
                        new[] { new ObjectReferenceKeyframe { time = 0, value = mats[i] }, new ObjectReferenceKeyframe { time = len, value = mats[i] } });
                }
                Constant(clip, e.path, type, "material._DissolveParams.x", moving ? 3 : 0, len);
                Constant(clip, e.path, type, "material._DissolveParams.y", 1, len);
                Constant(clip, e.path, type, "material._DissolveParams.w", d.edgeWidth / e.scale, len);
                for (int i = 0; i < 3; i++) Constant(clip, e.path, type, "material._DissolvePos." + "xyz"[i], e.direction[i], len);
                if (moving) AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(e.path, type, "material._DissolveParams.z"), ScanMotion.Map(ScanMotion.Plane(d, b), (e.outfitB ? 1 : -1) / e.scale, -(e.outfitB ? 1 : -1) * e.offset / e.scale));
                else Constant(clip, e.path, type, "material._DissolveParams.z", e.Threshold(b ? d.bottom : d.top), len);
            }
            string hp = Module + "/ScanHalo";
            Constant(clip, d.haloPath, typeof(GameObject), "m_IsActive", moving ? 0 : 1, len);
            Constant(clip, hp, typeof(GameObject), "m_IsActive", moving ? 1 : 0, len);
            if (moving)
            {
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(hp, typeof(Transform), "m_LocalPosition.y"), ScanMotion.HaloHeight(d, b));
                foreach (var axis in new[] {"x", "y", "z"})
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(hp, typeof(Transform), "m_LocalScale." + axis), ScanMotion.HaloScale(d, b, axis == "y"));
            }
            else
            {
                Constant(clip, hp, typeof(Transform), "m_LocalPosition.y", d.top, len);
                foreach (var axis in new[] {"x", "y", "z"}) Constant(clip, hp, typeof(Transform), "m_LocalScale." + axis, 1, len);
            }
            ScanBodyMasks.WriteCurves(clip, d, b, moving);
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = false; AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        static void Constant(AnimationClip clip, string path, Type type, string prop, float v, float len)
            => Curve(clip, path, type, prop, new[] {0f, len}, new[] {v, v});
        static void Curve(AnimationClip clip, string path, Type type, string prop, float[] times, float[] values)
        {
            var curve = new AnimationCurve(times.Select((t, i) => new Keyframe(t, values[i])).ToArray());
            for (int i = 0; i < curve.length; i++) { AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear); AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear); }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, prop), curve);
        }

        static void MakeController(ScanSetupData d, GameObject module)
        {
            string path = Folder + "/TransformationFX.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            if (!controller.parameters.Any(p => p.name == Parameter)) controller.AddParameter(Parameter, AnimatorControllerParameterType.Bool);
            var sm = controller.layers[0].stateMachine;
            foreach (var old in sm.states) sm.RemoveState(old.state);
            var a = sm.AddState("A - Ready", new Vector3(200, 40)); a.motion = d.idleA;
            var ab = sm.AddState("Transform A to B", new Vector3(450, 40)); ab.motion = d.forward;
            var b = sm.AddState("B - Ready", new Vector3(450, 180)); b.motion = d.idleB;
            var ba = sm.AddState("Transform B to A", new Vector3(200, 180)); ba.motion = d.reverse;
            foreach (var state in new[] {a, ab, b, ba}) state.writeDefaultValues = false;
            sm.defaultState = a;
            Transition(a, ab, true); Transition(b, ba, false);
            Finish(ab, b); Finish(ba, a);
            // A change of mind during a sweep is queued until the current sweep finishes.
            var layers = controller.layers; layers[0].name = "Transformation - Complementary Scan"; layers[0].defaultWeight = 1; controller.layers = layers;
            var merge = module.GetComponent<ModularAvatarMergeAnimator>() ?? module.AddComponent<ModularAvatarMergeAnimator>();
            merge.animator = controller; merge.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            merge.pathMode = MergeAnimatorPathMode.Absolute; merge.layerPriority = 100;
            merge.matchAvatarWriteDefaults = false; merge.deleteAttachedAnimator = false;
            var pars = module.GetComponent<ModularAvatarParameters>() ?? module.AddComponent<ModularAvatarParameters>();
            pars.parameters = new List<ParameterConfig> { new ParameterConfig { nameOrPrefix = Parameter, syncType = ParameterSyncType.Bool, saved = true, defaultValue = 0, hasExplicitDefaultValue = true } };
            var menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(Folder + "/TransformationMenu.asset");
            if (!menu) { menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>(); AssetDatabase.CreateAsset(menu, Folder + "/TransformationMenu.asset"); }
            menu.controls = new List<VRCExpressionsMenu.Control> { new VRCExpressionsMenu.Control { name = "变身 / Outfit B", type = VRCExpressionsMenu.Control.ControlType.Toggle, parameter = new VRCExpressionsMenu.Control.Parameter { name = Parameter }, value = 1 } };
            var installer = module.GetComponent<ModularAvatarMenuInstaller>() ?? module.AddComponent<ModularAvatarMenuInstaller>();
            installer.menuToAppend = menu;
            EditorUtility.SetDirty(controller); EditorUtility.SetDirty(menu); EditorUtility.SetDirty(module);
        }
        static void Transition(AnimatorState from, AnimatorState to, bool condition)
        {
            var t = from.AddTransition(to); t.duration = 0; t.hasExitTime = false; t.canTransitionToSelf = false;
            t.AddCondition(condition ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, Parameter);
        }
        static void Finish(AnimatorState from, AnimatorState to)
        { var t = from.AddTransition(to); t.duration = 0; t.hasExitTime = true; t.exitTime = 1; t.canTransitionToSelf = false; }

        public static void ApplyIdle(bool b)
        {
            if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
            var d = Data; var root = Avatar;
            foreach (var e in d.renderers)
            {
                var r = root.transform.Find(e.path).GetComponent<Renderer>();
                r.sharedMaterials = e.originals; r.SetPropertyBlock(null); for (int i = 0; i < e.originals.Length; i++) r.SetPropertyBlock(null, i);
            }
            foreach (var g in d.groupsA) root.transform.Find(g).gameObject.SetActive(!b);
            foreach (var g in d.groupsB) root.transform.Find(g).gameObject.SetActive(b);
            ScanBodyMasks.Apply(root, d, b ? d.idleB : d.idleA, 0);
            root.transform.Find(d.haloPath).gameObject.SetActive(true);
            root.transform.Find(Module + "/ScanHalo").gameObject.SetActive(false);
            SceneView.RepaintAll();
        }

    }
}
