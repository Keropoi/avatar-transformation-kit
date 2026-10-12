using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Keropoi.ScanKit.Editor
{
    // Shared by clip authoring, editor preview, and post-build renderer calibration.
    // Cubic Hermite segments provide exact ease-out and stronger cubic ease-in-out, including mapped tangents.
    public static class ScanMotion
    {
        public static void Validate(ScanSetupData d)
        {
            if (!(0 < d.riseEnd && d.riseEnd < d.scanStart && d.scanStart < d.scanEnd && d.scanEnd < d.duration))
                throw new InvalidOperationException("Expected 0 < riseEnd < scanStart < scanEnd < duration.");
            if (!(d.top > d.haloStartHeight && d.haloStartHeight > d.bottom && d.expandedScale > 1))
                throw new InvalidOperationException("Expected top > haloStartHeight > bottom and expandedScale > 1.");
        }

        public static float ScanStart(ScanSetupData d, bool toB) => toB ? d.scanStart : d.duration - d.scanEnd;
        public static float ScanEnd(ScanSetupData d, bool toB) => toB ? d.scanEnd : d.duration - d.scanStart;

        public static AnimationCurve Plane(ScanSetupData d, bool toB)
        {
            // Hold above the avatar through the entire rise and expansion prelude.
            return Direction(d, toB, Curve(new Keyframe(0, d.top), new Keyframe(d.scanStart, d.top),
                ScanMidpoint(d), new Keyframe(d.scanEnd, d.bottom), new Keyframe(d.duration, d.bottom)));
        }

        public static AnimationCurve HaloHeight(ScanSetupData d, bool toB)
        {
            float riseSpeed = 3 * (d.top - d.haloStartHeight) / d.riseEnd;
            return Direction(d, toB, Curve(new Keyframe(0, d.haloStartHeight, 0, riseSpeed),
                new Keyframe(d.riseEnd, d.top), new Keyframe(d.scanStart, d.top),
                ScanMidpoint(d), new Keyframe(d.scanEnd, d.bottom), new Keyframe(d.duration, d.bottom)));
        }

        static Keyframe ScanMidpoint(ScanSetupData d)
        {
            // Two cubic segments: 4u^3, then 1 - 4(1-u)^3. The first/last quarter
            // each cover only 6.25% of the distance; velocity remains continuous at the midpoint.
            float speed = 3 * (d.bottom - d.top) / (d.scanEnd - d.scanStart);
            return new Keyframe((d.scanStart + d.scanEnd) * .5f, (d.top + d.bottom) * .5f, speed, speed);
        }

        public static AnimationCurve HaloScale(ScanSetupData d, bool toB, bool vertical)
        {
            if (vertical)
                return Direction(d, toB, Curve(new Keyframe(0, 1), new Keyframe(d.scanEnd, 1), new Keyframe(d.duration, .01f)));
            // Quintic ease-out: a quick opening followed by a pronounced gentle settle.
            // Analytic values and derivatives sampled into Hermite segments retain monotonic motion.
            var keys = new System.Collections.Generic.List<Keyframe> { new Keyframe(0, 1) };
            float delta = d.expandedScale - 1, span = d.scanStart - d.riseEnd;
            for (int i = 0; i <= 16; i++)
            {
                float u = i / 16f, remaining = 1 - u;
                float value = 1 + delta * (1 - Mathf.Pow(remaining, 5));
                float speed = 5 * delta * Mathf.Pow(remaining, 4) / span;
                keys.Add(new Keyframe(d.riseEnd + u * span, value, i == 0 ? 0 : speed, speed));
            }
            keys.Add(new Keyframe(d.scanEnd, d.expandedScale));
            keys.Add(new Keyframe(d.duration, .01f));
            return Direction(d, toB, Curve(keys.ToArray()));
        }

        public static AnimationCurve Map(AnimationCurve curve, float multiplier, float offset)
        {
            return Curve(curve.keys.Select(k => new Keyframe(k.time, k.value * multiplier + offset,
                k.inTangent * multiplier, k.outTangent * multiplier)).ToArray());
        }

        static AnimationCurve Direction(ScanSetupData d, bool toB, AnimationCurve curve)
        {
            if (toB) return curve;
            // B->A retraces the forward motion, then settles the small ring back over the head.
            return Curve(curve.keys.Reverse().Select(k => new Keyframe(d.duration - k.time, k.value,
                -k.outTangent, -k.inTangent)).ToArray());
        }

        static AnimationCurve Curve(params Keyframe[] keys)
        {
            var result = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                AnimationUtility.SetKeyBroken(result, i, true);
                AnimationUtility.SetKeyLeftTangentMode(result, i, AnimationUtility.TangentMode.Free);
                AnimationUtility.SetKeyRightTangentMode(result, i, AnimationUtility.TangentMode.Free);
                // Preserve independent incoming/outgoing derivatives at phase boundaries.
                var key = result.keys[i]; key.inTangent = keys[i].inTangent; key.outTangent = keys[i].outTangent;
                key.weightedMode = WeightedMode.None; result.MoveKey(i, key);
            }
            return result;
        }
    }
}
