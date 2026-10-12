using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using nadena.dev.modular_avatar.core;

namespace Keropoi.ScanKit.Editor
{
    public static class KitBindings
    {
        // User-selected references become local paths; no product files enter the package.
        public static void Configure(GameObject avatar, KitRecipe recipe, GameObject[] outfitA,
            GameObject[] outfitB, Transform hips, GameObject halo, GameObject haloVisual,
            UnityEditor.Animations.AnimatorController haloController)
        {
            KitInstaller.RequireEditMode();
            if (!avatar || !recipe || !recipe.settings) throw new InvalidOperationException("Select an avatar and recipe.");
            string Path(Transform t)
            {
                if (!t || t == avatar.transform || !t.IsChildOf(avatar.transform))
                    throw new InvalidOperationException("Bindings must be descendants of the selected avatar.");
                return AnimationUtility.CalculateTransformPath(t, avatar.transform);
            }
            var d = recipe.settings;
            var a = (outfitA ?? new GameObject[0]).Select(g => Path(g ? g.transform : null)).ToList();
            var b = (outfitB ?? new GameObject[0]).Select(g => Path(g ? g.transform : null)).ToList();
            var hipPath = Path(hips); var haloPath = Path(halo ? halo.transform : null);
            if (!haloVisual || haloVisual.transform == halo.transform || !haloVisual.transform.IsChildOf(halo.transform))
                throw new InvalidOperationException("Halo visual must be a child of the supplied halo.");
            if (!haloController || !AssetDatabase.Contains(haloController)) throw new InvalidOperationException("Supply the imported original halo FX controller.");
            d.groupsA = a; d.groupsB = b; d.hipsPath = hipPath; d.haloPath = haloPath;
            d.haloVisualPath = AnimationUtility.CalculateTransformPath(haloVisual.transform, halo.transform);
            d.haloControllerPath = AssetDatabase.GetAssetPath(haloController);
            EditorUtility.SetDirty(d); EditorUtility.SetDirty(recipe); AssetDatabase.SaveAssets();
        }

        // Bounds are only a starting point; fixed-camera visual acceptance remains required.
        public static void FitScanRange(GameObject avatar, KitRecipe recipe)
        {
            KitInstaller.RequireEditMode();
            if (!avatar || !recipe || !recipe.settings) throw new InvalidOperationException("Missing avatar or recipe.");
            var d = recipe.settings;
            var halo = avatar.transform.Find(d.haloPath + "/" + d.haloVisualPath);
            if (!halo) throw new InvalidOperationException("Configure halo bindings first.");
            var renderers = d.groupsA.Concat(d.groupsB).Select(path => avatar.transform.Find(path))
                .SelectMany(t => t ? t.GetComponentsInChildren<Renderer>(true) : new Renderer[0])
                .Where(r => r is MeshRenderer || r is SkinnedMeshRenderer).ToArray();
            if (renderers.Length == 0) throw new InvalidOperationException("Configure both prepared outfits first.");
            var points = new List<Vector3>();
            foreach (var renderer in renderers)
            {
                var bounds = renderer.bounds;
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                        for (int z = -1; z <= 1; z += 2)
                            points.Add(avatar.transform.InverseTransformPoint(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x,y,z))));
            }
            d.haloStartHeight = avatar.transform.InverseTransformPoint(halo.position).y;
            d.top = Mathf.Max(points.Max(p => p.y) + .08f, d.haloStartHeight + .18f);
            d.bottom = points.Min(p => p.y) - .04f;
            ScanMotion.Validate(d);
            EditorUtility.SetDirty(d); AssetDatabase.SaveAssets();
        }

        public static IEnumerable<Renderer> OutfitRenderers(GameObject avatar, ScanSetupData d)
        {
            foreach (var path in d.groupsA.Concat(d.groupsB))
            {
                var group = avatar.transform.Find(path);
                foreach (var r in group.GetComponentsInChildren<Renderer>(true))
                {
                    if (!r.enabled || (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer))) continue;
                    var t = r.transform;
                    while (t != group && t.gameObject.activeSelf) t = t.parent;
                    if (t == group) yield return r;
                }
            }
        }
    }
}
