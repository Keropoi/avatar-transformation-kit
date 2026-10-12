using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using nadena.dev.modular_avatar.core;

namespace Keropoi.ScanKit.Editor
{
    // Editor-only: all runtime behavior is baked into standard FX animation curves.
    public static class ScanBodyMasks
    {
        public static void Migrate(GameObject root, ScanSetupData data)
        {
            if (data.bodyMasks == null) data.bodyMasks = new List<ScanBodyMask>();
            foreach (var changer in root.GetComponentsInChildren<ModularAvatarShapeChanger>(true))
            {
                string owner = AnimationUtility.CalculateTransformPath(changer.transform, root.transform);
                // Shape Changers may live on a selected group/container, above its renderer.
                var outfit = data.renderers.FirstOrDefault(e => e.path == owner || e.path.StartsWith(owner + "/", StringComparison.Ordinal));
                // Preserve rules on optional, unused clothing and all unrelated shape adjustments.
                if (outfit == null) continue;
                var migrated = new List<ChangedShape>();
                foreach (var shape in changer.Shapes)
                {
                    var target = shape.Object.Get(changer);
                    var body = target ? target.GetComponent<SkinnedMeshRenderer>() : null;
                    if (!body || AnimationUtility.CalculateTransformPath(body.transform, root.transform) != data.bodyPath || !data.bodyMasks.Any(m => m.shape == shape.ShapeName)) continue;
                    int index = body.sharedMesh.GetBlendShapeIndex(shape.ShapeName);
                    if (index < 0) throw new InvalidOperationException("Body mask shape missing: " + shape.ShapeName);
                    string path = AnimationUtility.CalculateTransformPath(body.transform, root.transform);
                    var entry = data.bodyMasks.FirstOrDefault(e => e.path == path && e.shape == shape.ShapeName);
                    if (entry == null)
                    {
                        entry = new ScanBodyMask { path = path, shape = shape.ShapeName, source = owner,
                            outfitA = body.GetBlendShapeWeight(index), outfitB = body.GetBlendShapeWeight(index) };
                        data.bodyMasks.Add(entry);
                    }
                    float value = shape.ChangeType == ShapeChangeType.Delete ? 100 : shape.Value;
                    if (outfit.outfitB) entry.outfitB = value; else entry.outfitA = value;
                    migrated.Add(shape);
                }
                if (migrated.Count == 0) continue;
                Undo.RecordObject(changer, "Transfer clothing body masks to scan animation");
                changer.Shapes = changer.Shapes.Except(migrated).ToList();
                EditorUtility.SetDirty(changer);
                PrefabUtility.RecordPrefabInstancePropertyModifications(changer);
            }
            EditorUtility.SetDirty(data);
        }

        public static void WriteCurves(AnimationClip clip, ScanSetupData data, bool toB, bool moving)
        {
            foreach (var entry in data.bodyMasks)
            {
                float end = toB ? entry.outfitB : entry.outfitA;
                float start = toB ? entry.outfitA : entry.outfitB;
                // During the wipe both outfits are enabled. Their old Delete rules must not run:
                // either outfit may expose skin, especially when full-body tracking raises an arm.
                // Restore before the plane moves; apply the destination mask only after it stops.
                float visible = Mathf.Min(entry.outfitA, entry.outfitB);
                float len = moving ? data.duration : 1f / 60;
                float scanStart = ScanMotion.ScanStart(data, toB), scanEnd = ScanMotion.ScanEnd(data, toB);
                float[] times = moving ? new[] {0f, scanStart * .5f, scanStart,
                    scanEnd, Mathf.Lerp(scanEnd, len, .5f), len} : new[] {0f, len};
                float[] values = moving ? new[] {start, visible, visible, visible, end, end} : new[] {end, end};
                var curve = new AnimationCurve(times.Select((t, i) => new Keyframe(t, values[i])).ToArray());
                for (int i = 0; i < curve.length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(entry.path,
                    typeof(SkinnedMeshRenderer), "blendShape." + entry.shape), curve);
            }
        }

        public static void Apply(GameObject root, ScanSetupData data, AnimationClip clip, float time)
        {
            foreach (var entry in data.bodyMasks)
            {
                var body = root.transform.Find(entry.path).GetComponent<SkinnedMeshRenderer>();
                int index = body.sharedMesh.GetBlendShapeIndex(entry.shape);
                var curve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(entry.path,
                    typeof(SkinnedMeshRenderer), "blendShape." + entry.shape));
                if (index >= 0 && curve != null) body.SetBlendShapeWeight(index, curve.Evaluate(time));
            }
        }

        public static Dictionary<SkinnedMeshRenderer, Dictionary<int, float>> Snapshot(GameObject root, ScanSetupData data)
        {
            var result = new Dictionary<SkinnedMeshRenderer, Dictionary<int, float>>();
            foreach (var entry in data.bodyMasks)
            {
                var body = root.transform.Find(entry.path).GetComponent<SkinnedMeshRenderer>();
                int index = body.sharedMesh.GetBlendShapeIndex(entry.shape);
                if (!result.ContainsKey(body)) result[body] = new Dictionary<int, float>();
                if (index >= 0) result[body][index] = body.GetBlendShapeWeight(index);
            }
            return result;
        }

        public static void Restore(Dictionary<SkinnedMeshRenderer, Dictionary<int, float>> snapshot)
        {
            if (snapshot == null) return;
            foreach (var body in snapshot)
                if (body.Key) foreach (var shape in body.Value) body.Key.SetBlendShapeWeight(shape.Key, shape.Value);
        }
    }
}
