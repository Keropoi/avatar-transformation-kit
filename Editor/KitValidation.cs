using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Keropoi.ScanKit.Editor
{
    public static class KitValidation
    {
        public static readonly Dictionary<string, string> TestedVersions = new Dictionary<string, string> {
            {"com.vrchat.avatars","3.10.1"}, {"com.vrchat.base","3.10.1"},
            {"nadena.dev.modular-avatar","1.16.2"}, {"nadena.dev.ndmf","1.11.0"},
            {"com.vrcfury.vrcfury","1.1336.0"}, {"jp.lilxyzw.liltoon","2.3.2"}
        };
        public static readonly Dictionary<string, string> AdapterVersions = new Dictionary<string, string> {
            {"com.poiyomi.toon","10.0.24"}, {"com.poiyomi.thryeditor","2.74.3"},
            {"com.chocopoi.vrc.dressingtools","2.6.0"}, {"com.chocopoi.vrc.dressingframework","2.1.1"},
            {"com.chocopoi.vrc.avatarlib","1.1.1"}
        };

        public static void Preflight(GameObject avatar, KitRecipe recipe)
        {
            KitInstaller.RequireEditMode();
            if (!avatar || !avatar.GetComponent<VRCAvatarDescriptor>() || avatar.transform.parent)
                throw new InvalidOperationException("Select a scene-root VRCAvatarDescriptor.");
            if (avatar.scene != SceneManager.GetActiveScene() || string.IsNullOrEmpty(avatar.scene.path))
                throw new InvalidOperationException("Use the active saved scene.");
            if (!recipe || !recipe.settings) throw new InvalidOperationException("Create and configure a recipe first.");
            if (avatar.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Transformation Scan" || t.name == ScanBuilder.Module))
                throw new InvalidOperationException("Source already contains a transformation. Use an unmodified, dressed source avatar; legacy migration is not automatic.");
            if (recipe.requireTestedVersions)
            {
                if (Application.unityVersion != "2022.3.22f1") throw new InvalidOperationException("Preset tested with Unity 2022.3.22f1 only.");
                var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
                var versions = TestedVersions.AsEnumerable();
                if (recipe.settings.usePlumAdapter) versions = versions.Concat(AdapterVersions);
                else if (recipe.settings.useDressingToolsAdapter) versions = versions.Concat(AdapterVersions.Where(p => p.Key.StartsWith("com.chocopoi.", StringComparison.Ordinal)));
                foreach (var pair in versions)
                {
                    var package = packages.FirstOrDefault(p => p.name == pair.Key);
                    if (package == null || package.version != pair.Value) throw new InvalidOperationException("Version mismatch: " + pair.Key + " requires " + pair.Value);
                }
            }
            var d = recipe.settings; ScanMotion.Validate(d);
            if (avatar.transform.localScale != Vector3.one || Quaternion.Angle(avatar.transform.rotation, Quaternion.identity) > .01f)
                throw new InvalidOperationException("Scan requires an unrotated, unit-scale avatar root.");
            if (d.audioVolume < 0 || d.audioVolume > 1 || d.audioLeadSeconds <= 0) throw new InvalidOperationException("Invalid sound settings.");
            if (!string.IsNullOrEmpty(d.audioPath) && !AssetDatabase.LoadAssetAtPath<AudioClip>(d.audioPath)) throw new InvalidOperationException("Audio path is not an imported AudioClip.");
            if (d.groupsA == null || d.groupsB == null || d.groupsA.Count == 0 || d.groupsB.Count == 0)
                throw new InvalidOperationException("Specify at least one prepared outfit group for both A and B.");
            var groups = d.groupsA.Concat(d.groupsB).ToArray();
            foreach (var path in groups.Concat(new[] {d.hipsPath, d.haloPath, d.haloPath + "/" + d.haloVisualPath}))
            {
                if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException("Empty binding. Configure the recipe for the selected avatar.");
                if (!avatar.transform.Find(path)) throw new InvalidOperationException("Missing binding: " + path);
            }
            if (groups.Distinct().Count() != groups.Length || groups.Any(a => groups.Any(b => a != b && b.StartsWith(a + "/", StringComparison.Ordinal))))
                throw new InvalidOperationException("Outfit groups must be distinct and must not contain one another.");
            if (groups.Any(g => d.hipsPath == g || d.hipsPath.StartsWith(g + "/", StringComparison.Ordinal) || d.haloPath == g || d.haloPath.StartsWith(g + "/", StringComparison.Ordinal)))
                throw new InvalidOperationException("Outfit groups must not contain the avatar hips or source halo.");
            if (!AssetDatabase.LoadAssetAtPath<AnimatorController>(d.haloControllerPath)) throw new InvalidOperationException("Original halo FX controller missing.");
            var halo = avatar.transform.Find(d.haloPath);
            if (!halo.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>())
                throw new InvalidOperationException("Angelring Halo adapter requires the imported halo's MA Merge Animator.");
            if (!avatar.transform.Find(d.haloPath + "/" + d.haloVisualPath).GetComponentsInChildren<Renderer>(true).Any(r => !(r is ParticleSystemRenderer)))
                throw new InvalidOperationException("Halo visual has no mesh renderer.");
            if (d.useWingAdapter)
            {
                if (string.IsNullOrWhiteSpace(d.wingName)) throw new InvalidOperationException("Optional wing adapter requires a wing name.");
                var wing = avatar.GetComponentsInChildren<Transform>(true).Where(t => t.name == d.wingName).ToArray();
                if (wing.Length != 1 || wing[0].GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 0)
                    throw new InvalidOperationException("Optional wing adapter requires one placed wing object with skinned renderers.");
                if (groups.Any(g => wing[0].IsChildOf(avatar.transform.Find(g))))
                    throw new InvalidOperationException("Configure wings either as an ordinary B group or through the wing adapter, not both.");
            }
            if (d.bodyMasks == null) throw new InvalidOperationException("Body mask configuration must be a list, which may be empty.");
            foreach (var rule in d.bodyMasks)
            {
                if (string.IsNullOrWhiteSpace(d.bodyPath) || rule.path != d.bodyPath)
                    throw new InvalidOperationException("Body-mask rules must target the explicitly configured bodyPath.");
                var t = avatar.transform.Find(rule.path); var renderer = t ? t.GetComponent<SkinnedMeshRenderer>() : null;
                if (!renderer || !renderer.sharedMesh || renderer.sharedMesh.GetBlendShapeIndex(rule.shape) < 0)
                    throw new InvalidOperationException("Body mask binding missing: " + rule.path + "/" + rule.shape);
            }
            foreach (var group in groups.Select(g => avatar.transform.Find(g)))
            {
                if (!group.GetComponentsInChildren<Renderer>(true).Any(r => r.enabled && (r is MeshRenderer || r is SkinnedMeshRenderer)))
                    throw new InvalidOperationException("Outfit group has no enabled mesh renderer: " + group.name);
                foreach (var changer in group.GetComponentsInChildren<nadena.dev.modular_avatar.core.ModularAvatarShapeChanger>(true))
                    foreach (var shape in changer.Shapes)
                    {
                        var target = shape.Object.Get(changer);
                        if (!target) continue;
                        var path = AnimationUtility.CalculateTransformPath(target.transform, avatar.transform);
                        if (shape.ChangeType == nadena.dev.modular_avatar.core.ShapeChangeType.Delete && !d.bodyMasks.Any(m => m.path == path && m.shape == shape.ShapeName))
                            throw new InvalidOperationException("Explicit body-mask rule required for destructive outfit shape rule: " + path + "/" + shape.ShapeName);
                    }
            }
            foreach (var renderer in KitBindings.OutfitRenderers(avatar, d))
                foreach (var material in renderer.sharedMaterials)
                    if (material && material.shader && !d.allowSimpleMaterialConversion &&
                        (!material.shader.name.ToLowerInvariant().Contains("liltoon") || !material.HasProperty("_DissolveParams") || !material.HasProperty("_DissolvePos")))
                        throw new InvalidOperationException("Core scan requires lilToon plane-dissolve materials: " + renderer.name + "/" + material.name);
            foreach (var r in avatar.GetComponentsInChildren<Renderer>(true).Where(r => !(r is ParticleSystemRenderer)))
                if (r.sharedMaterials.Any(m => !m || !m.shader)) throw new InvalidOperationException("Missing material/shader: " + r.name);
            // Known optimizer issue: a commented include was treated as an active include.
            string shaderFile = "Packages/jp.lilxyzw.liltoon/CustomShaderResources/Misc/ReferenceUVs.lilblock";
            if (File.Exists(shaderFile) && File.ReadLines(shaderFile).Any(l => l.TrimStart().StartsWith("//#include", StringComparison.Ordinal)))
                throw new InvalidOperationException("lilToon 2.3.2 compatibility preparation required. See Documentation~/COMPATIBILITY.md.");
        }

        public static void ValidateGenerated(GameObject root, ScanSetupData d)
        {
            ScanMotion.Validate(d);
            if (d.renderers.Count == 0) throw new InvalidOperationException("No outfit renderers generated.");
            foreach (var clip in new[] {d.idleA,d.idleB,d.forward,d.reverse})
            {
                if (!clip || !AssetDatabase.Contains(clip)) throw new InvalidOperationException("Animation is not persistent.");
                ValidateMaterialReferences(clip);
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    if (!root.transform.Find(binding.path)) throw new InvalidOperationException("Unresolved animation path: " + binding.path);
            }
            foreach (var e in d.renderers)
            {
                if (!(e.scale > .000001f)) throw new InvalidOperationException("Degenerate renderer transform: " + e.path);
                foreach (var m in e.scanning) CheckMaterial(m);
                // Complementary shader thresholds must represent the same plane as the visible ring.
                var binding = EditorCurveBinding.FloatCurve(e.path, root.transform.Find(e.path).GetComponent<Renderer>().GetType(), "material._DissolveParams.z");
                var curve = AnimationUtility.GetEditorCurve(d.forward, binding);
                foreach (float t in new[] {d.scanStart, (d.scanStart+d.scanEnd)*.5f, d.scanEnd})
                {
                    float physical = curve.Evaluate(t)*e.scale*(e.outfitB ? 1 : -1)+e.offset;
                    if (Mathf.Abs(physical-ScanMotion.HaloHeight(d,true).Evaluate(t)) > .0001f) throw new InvalidOperationException("Plane mismatch: " + e.path);
                }
            }
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ScanBuilder.Folder + "/TransformationFX.controller");
            if (!controller || controller.parameters.Count(p => p.name == ScanBuilder.Parameter) != 1) throw new InvalidOperationException("Missing/duplicate transformation parameter.");
        }

        static void CheckMaterial(Material material)
        {
            if (!material || !AssetDatabase.Contains(material) || !material.shader || ShaderUtil.ShaderHasError(material.shader))
                throw new InvalidOperationException("Missing, transient or failed scan material: " + (material ? material.name : "null"));
        }
        static void ValidateMaterialReferences(AnimationClip clip)
        {
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip).Where(b => b.propertyName.StartsWith("m_Materials", StringComparison.Ordinal)))
                foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip,binding)) CheckMaterial(key.value as Material);
        }

        [MenuItem("Tools/ScanKit/4 Validate Local Build")]
        public static void ValidateLocalBuild()
        {
            KitInstaller.RequireEditMode();
            var registry = KitInstaller.Registry;
            if (!registry) throw new InvalidOperationException("Nothing installed.");
            File.WriteAllText(KitInstaller.LocalRoot + "/validation-report.json", "{\"status\":\"running\"}");
            var installed = KitInstaller.Resolve(registry.installedId);
            if (!installed) throw new InvalidOperationException("Open the installed scene.");
            ValidateGenerated(installed, ScanBuilder.Data);
            var copy = UnityEngine.Object.Instantiate(installed);
            copy.name = installed.name + " (ScanKit validation)";
            try
            {
                RunLocalPreprocessors(copy);
                var descriptor = copy.GetComponent<VRCAvatarDescriptor>();
                if (descriptor.expressionParameters && descriptor.expressionParameters.CalcTotalCost() > VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.MAX_PARAMETER_COST)
                    throw new InvalidOperationException("Built expression parameters exceed the VRChat budget.");
                var fx = descriptor.baseAnimationLayers.First(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
                var clips = fx.animationClips.Where(c => c.name.EndsWith("KAT_A_to_B", StringComparison.Ordinal) || c.name.EndsWith("KAT_B_to_A", StringComparison.Ordinal)).Distinct().ToArray();
                if (clips.Length < 2) throw new InvalidOperationException("Built transformation clips missing.");
                int wingBindings = 0;
                foreach (var clip in clips)
                {
                    ValidateMaterialReferences(clip);
                    int expectedWingRenderers = 0;
                    if (ScanBuilder.Data.useWingAdapter)
                    {
                        var wing = copy.GetComponentsInChildren<Transform>(true).Single(t => t.name == ScanBuilder.Data.wingName);
                        expectedWingRenderers = wing.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
                    }
                    if (AnimationUtility.GetCurveBindings(clip).Count(b => b.propertyName == "material._DissolveParams.z") != ScanBuilder.Data.renderers.Count + expectedWingRenderers)
                        throw new InvalidOperationException("Built scan renderer count mismatch.");
                    foreach (var b in AnimationUtility.GetCurveBindings(clip).Where(b => b.propertyName == "material._DissolveParams.z"))
                    {
                        if (!copy.transform.Find(b.path)) throw new InvalidOperationException("Build changed an animation path: " + b.path);
                        if (ScanBuilder.Data.useWingAdapter && b.path.Split('/').Contains(ScanBuilder.Data.wingName)) wingBindings++;
                        if (clip.name.EndsWith("KAT_A_to_B", StringComparison.Ordinal)) ValidateBuiltPlane(copy, clip, b, ScanBuilder.Data);
                    }
                }
                if (ScanBuilder.Data.useWingAdapter && wingBindings == 0) throw new InvalidOperationException("Wing scan missing from built clips.");
                // Exercise imported materials after serialization, catching the prior transient-material bug.
                var prefabPath = ScanBuilder.Folder + "/LocalValidation.prefab";
                var saved = PrefabUtility.SaveAsPrefabAsset(copy, prefabPath);
                if (!saved) throw new InvalidOperationException("Validation prefab serialization failed.");
                AssetDatabase.SaveAssets();
                var reloaded = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    var savedFx = reloaded.GetComponent<VRCAvatarDescriptor>().baseAnimationLayers.First(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
                    if (!savedFx) throw new InvalidOperationException("Serialized FX controller is missing.");
                    foreach (var clip in savedFx.animationClips.Where(c => c.name.Contains("KAT_"))) ValidateMaterialReferences(clip);
                }
                finally { PrefabUtility.UnloadPrefabContents(reloaded); }
                var report = "{\"status\":\"pass\",\"scope\":\"Local SDK callback simulation with DressingFramework adapter and serialized material references; not a VRChat build/upload or visual acceptance\",\"wingScanBindings\":" + wingBindings + ",\"unity\":\"" + Application.unityVersion + "\",\"utc\":\"" + DateTime.UtcNow.ToString("o") + "\"}";
                File.WriteAllText(KitInstaller.LocalRoot + "/validation-report.json", report);
                Debug.Log("[ScanKit] Local validation passed: " + report);
            }
            catch
            {
                File.WriteAllText(KitInstaller.LocalRoot + "/validation-report.json", "{\"status\":\"failed\",\"details\":\"See the first new Console error\"}");
                throw;
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }

        static void ValidateBuiltPlane(GameObject root, AnimationClip clip, EditorCurveBinding binding, ScanSetupData data)
        {
            var renderer = root.transform.Find(binding.path).GetComponent<Renderer>();
            var skinned = renderer as SkinnedMeshRenderer;
            var space = skinned && skinned.rootBone ? skinned.rootBone : renderer.transform;
            var matrix = root.transform.worldToLocalMatrix * space.localToWorldMatrix;
            var axis = new Vector3(matrix.m10, matrix.m11, matrix.m12);
            var threshold = AnimationUtility.GetEditorCurve(clip, binding);
            float sign = threshold.Evaluate(data.scanStart) > threshold.Evaluate(data.scanEnd) ? 1 : -1;
            foreach (float time in new[] { data.scanStart, (data.scanStart + data.scanEnd) * .5f, data.scanEnd })
            {
                float physical = sign * threshold.Evaluate(time) * axis.magnitude + matrix.m13;
                if (Mathf.Abs(physical - ScanMotion.HaloHeight(data, true).Evaluate(time)) > .0001f)
                    throw new InvalidOperationException("Built plane mismatch: " + binding.path);
            }
        }

        // SDK's dispatcher displays a blocking dialog on failure. Use its registered order,
        // but run the version-locked DressingFramework entry directly: its SDK adapter rejects
        // calls whose stack is not VRCAvatarBuilder. This check does not claim to build a .vrca.
        static void RunLocalPreprocessors(GameObject copy)
        {
            var field = typeof(VRCBuildPipelineCallbacks).GetField("_preprocessAvatarCallbacks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (field == null) throw new InvalidOperationException("SDK callback registry API changed.");
            var callbacks = ((IEnumerable<IVRCSDKPreprocessAvatarCallback>)field.GetValue(null)).OrderBy(c => c.callbackOrder).ToArray();
            foreach (var callback in callbacks)
            {
                if (callback.GetType().FullName == "Chocopoi.DressingFramework.Detail.DK.Triggers.VRChat.BuildAvatarCallback")
                {
                    var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Chocopoi.DressingFramework.Detail.DK.AvatarBuilder")).FirstOrDefault(t => t != null);
                    if (type == null) throw new InvalidOperationException("DressingFramework builder unavailable.");
                    var builder = Activator.CreateInstance(type, new object[] {copy});
                    var run = type.GetMethod("RunStages");
                    run.Invoke(builder, run.GetParameters().Select(p => p.DefaultValue).ToArray());
                    var context = type.GetProperty("Context").GetValue(builder);
                    var report = context.GetType().GetProperty("Report", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(context);
                    var hasLogType = report.GetType().GetMethod("HasLogType");
                    var error = Enum.Parse(hasLogType.GetParameters()[0].ParameterType, "Error");
                    if ((bool)hasLogType.Invoke(report, new[] {error})) throw new InvalidOperationException("DressingFramework reported a build error.");
                }
                else if (!callback.OnPreprocessAvatar(copy)) throw new InvalidOperationException("Local preprocessing failed: " + callback.GetType().FullName);
            }
        }
    }
}
