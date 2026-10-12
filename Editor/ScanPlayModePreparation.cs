using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Keropoi.ScanKit.Editor
{
    // DressingFramework starts its Play Mode build in Awake, before NDMF invokes SDK callbacks.
    // Prepend the same preparation used by uploads to that editor-only lifecycle entrypoint.
    // This changes only the Play Mode copy and adds no components to the uploaded avatar.
    [InitializeOnLoad]
    public static class ScanPlayModePreparation
    {
        public static bool IsInstalled { get; private set; }

        static ScanPlayModePreparation()
        {
            Install();
        }

        static void Install()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var builder = assemblies.Select(a => a.GetType(
                "Chocopoi.DressingFramework.Detail.DK.Triggers.PlayMode.AutoPlayModeBuild"))
                .FirstOrDefault(t => t != null);
            var spawner = assemblies.Select(a => a.GetType(
                "Chocopoi.DressingFramework.Detail.DK.Triggers.PlayMode.PlayModeBuildTriggerSpawner"))
                .FirstOrDefault(t => t != null);
            if (builder == null || spawner == null) return;

            // Force the package's initializer first so it cannot overwrite our wrapper later.
            RuntimeHelpers.RunClassConstructor(builder.TypeHandle);
            var field = spawner.GetField("OnAvatarLifecycle", BindingFlags.Public | BindingFlags.Static);
            if (field == null) throw new InvalidOperationException("DressingFramework Play Mode lifecycle API changed.");
            var original = field.GetValue(null) as Delegate;
            if (original == null) throw new InvalidOperationException("DressingFramework Play Mode callback is missing.");
            var arguments = field.FieldType.GetMethod("Invoke").GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            var before = typeof(ScanPlayModePreparation).GetMethod(nameof(BeforeDressingTools), BindingFlags.NonPublic | BindingFlags.Static);
            var body = Expression.Block(Expression.Call(before, arguments[1]),
                Expression.Invoke(Expression.Constant(original, field.FieldType), arguments));
            field.SetValue(null, Expression.Lambda(field.FieldType, body, arguments).Compile());
            IsInstalled = true;
        }

        static void BeforeDressingTools(GameObject root)
        {
            if (!EditorApplication.isPlaying || !root || !root.transform.Find(ScanBuilder.Module)) return;
            ScanBuildPreparation.Prepare(root);
            Debug.Log("[ScanKit] Prepared scan outfits before DressingTools Play Mode build.", root);
        }
    }
}
