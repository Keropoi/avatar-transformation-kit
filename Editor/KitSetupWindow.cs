using System;
using UnityEngine;
using UnityEditor;

namespace Keropoi.ScanKit.Editor
{
    public sealed class KitSetupWindow : EditorWindow
    {
        GameObject source, halo, visual;
        Transform hips;
        UnityEditor.Animations.AnimatorController haloController;
        KitRecipe recipe;
        GameObject[] outfitA = new GameObject[1], outfitB = new GameObject[1];
        Vector2 scroll;

        [MenuItem("Tools/ScanKit/Setup User Outfits")]
        public static void Open() => GetWindow<KitSetupWindow>("ScanKit Setup");

        void OnEnable()
        {
            source = Selection.activeGameObject;
            recipe = AssetDatabase.LoadAssetAtPath<KitRecipe>(KitInstaller.RecipePath);
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox("Prepare and rig both outfits first. Import and position your purchased Angelring Halo. Bind objects below; use local recipe settings for body masks and scan size.", MessageType.Info);
            source = (GameObject)EditorGUILayout.ObjectField("Source avatar", source, typeof(GameObject), true);
            recipe = (KitRecipe)EditorGUILayout.ObjectField("Local recipe", recipe, typeof(KitRecipe), false);
            if (GUILayout.Button("Create / select core recipe"))
            {
                KitInstaller.CreateRecipe(); recipe = AssetDatabase.LoadAssetAtPath<KitRecipe>(KitInstaller.RecipePath);
            }
            hips = (Transform)EditorGUILayout.ObjectField("Avatar hips", hips, typeof(Transform), true);
            DrawGroups("Outfit A", ref outfitA); DrawGroups("Outfit B", ref outfitB);
            halo = (GameObject)EditorGUILayout.ObjectField("Source halo", halo, typeof(GameObject), true);
            visual = (GameObject)EditorGUILayout.ObjectField("Halo visual child", visual, typeof(GameObject), true);
            haloController = (UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField("Original halo FX", haloController, typeof(UnityEditor.Animations.AnimatorController), false);
            if (GUILayout.Button("Save bindings")) Run(() => KitBindings.Configure(source, recipe, outfitA, outfitB, hips, halo, visual, haloController));
            if (GUILayout.Button("Fit scan heights from outfit bounds")) Run(() => KitBindings.FitScanRange(source, recipe));
            if (recipe && recipe.settings && GUILayout.Button("Select local settings (body masks / timing / size)")) Selection.activeObject = recipe.settings;
            if (GUILayout.Button("Preflight")) Run(() => KitValidation.Preflight(source, recipe));
            if (GUILayout.Button("Install on copy and save scene")) Run(() => {
                var copy = KitInstaller.Install(source, recipe);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
            });
            EditorGUILayout.EndScrollView();
        }

        static void DrawGroups(string label, ref GameObject[] objects)
        {
            int count = Mathf.Clamp(EditorGUILayout.IntField(label + " groups", objects.Length), 1, 32);
            if (count != objects.Length) Array.Resize(ref objects, count);
            for (int i = 0; i < objects.Length; i++)
                objects[i] = (GameObject)EditorGUILayout.ObjectField(label + " " + (i + 1), objects[i], typeof(GameObject), true);
        }

        static void Run(Action action)
        {
            try { action(); Debug.Log("[ScanKit] Setup operation completed."); }
            catch (Exception e) { Debug.LogError("[ScanKit] " + e.Message); }
        }
    }
}
