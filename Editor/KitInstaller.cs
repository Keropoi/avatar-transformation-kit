using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Keropoi.ScanKit.Editor
{
    public static class KitInstaller
    {
        public const string LocalRoot = "Assets/ScanKitLocal";
        public const string RegistryPath = LocalRoot + "/Registry.asset";
        public const string RecipePath = LocalRoot + "/CoreRecipe.asset";
        public const string PlumRecipePath = LocalRoot + "/PlumRecipe.asset";
        static string staging;
        public static KitRegistry Registry => AssetDatabase.LoadAssetAtPath<KitRegistry>(RegistryPath);
        public static string WorkingFolder => staging ?? (Registry ? Registry.folder : LocalRoot + "/NotInstalled");

        [MenuItem("Tools/ScanKit/1 Create Core Recipe")]
        public static void CreateRecipe() => CreateRecipeAt(RecipePath, false);

        [MenuItem("Tools/ScanKit/6 Create Optional Plum Recipe")]
        public static void CreatePlumRecipe() => CreateRecipeAt(PlumRecipePath, true);

        static void CreateRecipeAt(string path, bool plum)
        {
            RequireEditMode();
            if (AssetDatabase.LoadAssetAtPath<KitRecipe>(path)) { Selection.activeObject = AssetDatabase.LoadAssetAtPath<KitRecipe>(path); return; }
            EnsureLocalRoot();
            var recipe = ScriptableObject.CreateInstance<KitRecipe>();
            AssetDatabase.CreateAsset(recipe, path);
            recipe.settings = ScriptableObject.CreateInstance<ScanSetupData>();
            recipe.settings.name = plum ? "Plum adapter (local bindings)" : "Core recipe (user outfits)";
            AssetDatabase.AddObjectToAsset(recipe.settings, recipe);
            string presetName = plum ? "plum-verified" : "core";
            var preset = AssetDatabase.FindAssets(presetName).Select(AssetDatabase.GUIDToAssetPath)
                .Single(p => p.EndsWith("/Presets/" + presetName + ".json", StringComparison.Ordinal));
            JsonUtility.FromJsonOverwrite(File.ReadAllText(preset), recipe.settings);
            EditorUtility.SetDirty(recipe); EditorUtility.SetDirty(recipe.settings);
            AssetDatabase.SaveAssets(); Selection.activeObject = recipe;
        }

        [MenuItem("Tools/ScanKit/2 Install on Selected Avatar")]
        public static void InstallSelected() => Install(Selection.activeGameObject, AssetDatabase.LoadAssetAtPath<KitRecipe>(RecipePath));

        // One managed installation per project in 0.1. All mutations are on a disposable copy.
        public static GameObject Install(GameObject source, KitRecipe recipe)
        {
            RequireEditMode();
            KitValidation.Preflight(source, recipe);
            var previous = Registry;
            if (previous && previous.owner != "com.keropoi.avatar-transformation") throw new InvalidOperationException("Registry owner mismatch.");
            if (previous && previous.sourceId != Id(source)) throw new InvalidOperationException("One installation per project. Uninstall the existing installation first.");
            var oldCopy = previous ? Resolve(previous.installedId) : null;
            if (previous && !oldCopy) throw new InvalidOperationException("Installed copy is not loaded. Open the recorded scene before updating.");
            EnsureLocalRoot();
            string folder = LocalRoot + "/Generated_" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            GameObject copy = null;
            var originalActive = source.activeSelf;
            bool oldActive = oldCopy && oldCopy.activeSelf;
            try
            {
                copy = UnityEngine.Object.Instantiate(source);
                copy.name = source.name + " [ScanKit " + Path.GetFileName(folder).Substring(10, 8) + "]";
                SceneManagerMove(copy, source);
                copy.SetActive(true);
                var data = UnityEngine.Object.Instantiate(recipe.settings);
                data.name = "ScanSetup"; data.avatarName = copy.name;
                data.renderers.Clear(); data.idleA = data.idleB = data.forward = data.reverse = null;
                AssetDatabase.CreateAsset(data, folder + "/ScanSetup.asset");
                staging = folder;
                EnableWriteDefaultsFix(copy);
                ScanBuilder.Build();
                // Validation is done before changing the working avatar or registry.
                KitValidation.ValidateGenerated(copy, data);
                AssetDatabase.SaveAssets();
                var registry = previous;
                if (!registry)
                {
                    registry = ScriptableObject.CreateInstance<KitRegistry>();
                    registry.sourceId = Id(source); registry.sourceScene = source.scene.path;
                    registry.sourceWasActive = originalActive;
                    AssetDatabase.CreateAsset(registry, RegistryPath);
                }
                registry.folder = folder; registry.recipe = recipe;
                registry.installedId = Id(copy); registry.generations.Add(folder);
                EditorUtility.SetDirty(registry); AssetDatabase.SaveAssets();
                source.SetActive(false);
                if (oldCopy) UnityEngine.Object.DestroyImmediate(oldCopy);
                EditorSceneManager.MarkSceneDirty(source.scene);
                Selection.activeGameObject = copy;
                Debug.Log("[ScanKit] Installed. Original retained inactive; save your scene. Run local build validation next.");
                return copy;
            }
            catch
            {
                if (copy) UnityEngine.Object.DestroyImmediate(copy);
                source.SetActive(originalActive); if (oldCopy) oldCopy.SetActive(oldActive);
                AssetDatabase.DeleteAsset(folder);
                throw;
            }
            finally { staging = null; }
        }

        [MenuItem("Tools/ScanKit/3 Update from Source Avatar")]
        public static void Update()
        {
            var registry = Registry;
            if (!registry) throw new InvalidOperationException("Nothing installed.");
            Install(Resolve(registry.sourceId), registry.recipe);
        }

        [MenuItem("Tools/ScanKit/5 Uninstall")]
        public static void Uninstall()
        {
            RequireEditMode();
            var registry = Registry;
            if (!registry) return;
            if (registry.owner != "com.keropoi.avatar-transformation") throw new InvalidOperationException("Registry owner mismatch.");
            var source = Resolve(registry.sourceId); var installed = Resolve(registry.installedId);
            if (!source) throw new InvalidOperationException("Open the source installation scene before uninstalling.");
            // Check all targets before deleting any; never delete imported products or the recipe.
            foreach (var folder in registry.generations)
                if (!folder.StartsWith(LocalRoot + "/Generated_", StringComparison.Ordinal) || folder.Contains("..") || folder.Substring(LocalRoot.Length + 1).Contains("/"))
                    throw new InvalidOperationException("Invalid owned output folder: " + folder);
            if (installed) UnityEngine.Object.DestroyImmediate(installed);
            source.SetActive(registry.sourceWasActive);
            EditorSceneManager.MarkSceneDirty(source.scene);
            foreach (var folder in registry.generations)
                if (AssetDatabase.IsValidFolder(folder) && !AssetDatabase.DeleteAsset(folder)) throw new IOException("Could not remove owned folder; retry uninstall: " + folder);
            AssetDatabase.DeleteAsset(RegistryPath);
            EditorSceneManager.MarkSceneDirty(source.scene); Selection.activeGameObject = source;
            Debug.Log("[ScanKit] Uninstalled; source and recipe retained. Save the scene.");
        }

        static void EnsureLocalRoot()
        {
            Directory.CreateDirectory(LocalRoot); AssetDatabase.Refresh();
        }
        static void SceneManagerMove(GameObject copy, GameObject source)
        {
            if (copy.scene != source.scene) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(copy, source.scene);
        }
        public static string Id(GameObject go) => GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
        public static GameObject Resolve(string id) => GlobalObjectId.TryParse(id, out var parsed) ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsed) as GameObject : null;
        public static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || AnimationMode.InAnimationMode())
                throw new InvalidOperationException("Stop Play/animation preview and wait for compilation.");
        }
        static void EnableWriteDefaultsFix(GameObject root)
        {
            Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);
            var componentType = Find("VF.Model.VRCFury"); var featureType = Find("VF.Model.Feature.FixWriteDefaults");
            if (componentType == null || featureType == null) throw new InvalidOperationException("Tested VRCFury API unavailable.");
            var field = componentType.GetField("content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) throw new InvalidOperationException("VRCFury content API changed.");
            foreach (var existing in root.GetComponentsInChildren(componentType, true))
                if (field.GetValue(existing)?.GetType() == featureType)
                {
                    var mode = featureType.GetField("mode");
                    if (mode.GetValue(field.GetValue(existing)).ToString() != "Auto") throw new InvalidOperationException("Existing Write Defaults override is not Auto.");
                    return;
                }
            var feature = Activator.CreateInstance(featureType);
            field.SetValue(root.AddComponent(componentType), feature);
        }
    }
}
