using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;

namespace Keropoi.ScanKit.Editor
{
    public static class ScanHaloDefaults
    {
        public static void Configure(GameObject avatar)
        {
            var halo = avatar.transform.Find(ScanBuilder.Data.haloPath);
            if (!halo) throw new InvalidOperationException("Original halo not found.");
            string sourcePath = ScanBuilder.Data.haloControllerPath;
            string fixedPath = ScanBuilder.Folder + "/HaloDefaultsFX.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(fixedPath);
            if (!controller)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, fixedPath)) throw new InvalidOperationException("Could not copy original halo controller.");
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(fixedPath);
            }
            // Preserve the asset's continuous wing and float animation, without expression controls.
            for (int i = controller.layers.Length - 1; i >= 0; i--)
                if (controller.layers[i].name == "Angelring_Active" || controller.layers[i].name == "Angelring_Effect") controller.RemoveLayer(i);
            controller.parameters = new AnimatorControllerParameter[0];
            var merge = halo.GetComponent<ModularAvatarMergeAnimator>(); merge.animator = controller;
            foreach (var installer in halo.GetComponents<ModularAvatarMenuInstaller>()) Undo.DestroyObjectImmediate(installer);
            foreach (var parameters in halo.GetComponents<ModularAvatarParameters>()) Undo.DestroyObjectImmediate(parameters);
            halo.gameObject.SetActive(true); halo.Find(ScanBuilder.Data.haloVisualPath).gameObject.SetActive(true);
            foreach (var particle in halo.GetComponentsInChildren<ParticleSystem>(true))
            {
                // AR_Effect=0.5 is the prefab's default, corresponding to 30 particles/second.
                var emission = particle.emission; emission.enabled = true; emission.rateOverTime = 30;
                EditorUtility.SetDirty(particle);
            }
            EditorUtility.SetDirty(merge); EditorUtility.SetDirty(controller);
        }
    }
}
