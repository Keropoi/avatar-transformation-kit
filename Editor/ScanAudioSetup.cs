using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace Keropoi.ScanKit.Editor
{
    public static class ScanAudioSetup
    {
        public const string Parameter = "KAT/Sound";
        public const string SourcePath = ScanBuilder.Module + "/Transformation Audio";
        public static string ClipPath => ScanBuilder.Data.audioPath;
        public static float AudioLeadSeconds => ScanBuilder.Data.audioLeadSeconds;
        const string LeadState = "A to B - Audio Lead";
        const string MuteLayer = "Transformation - Sound Switch";

        public static void Configure(ScanSetupData d, GameObject module)
        {
            if (string.IsNullOrEmpty(ClipPath)) return;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
            if (!clip) throw new InvalidOperationException("Transformation sound clip is missing: " + ClipPath);
            var child = module.transform.Find("Transformation Audio");
            if (!child) { child = new GameObject("Transformation Audio").transform; child.SetParent(module.transform, false); }
            child.localPosition = new Vector3(0, 1.15f, 0);
            child.gameObject.SetActive(true);
            var source = child.GetComponent<AudioSource>();
            if (!source) source = child.gameObject.AddComponent<AudioSource>();
            source.clip = clip; source.playOnAwake = false; source.loop = false;
            source.volume = d.audioVolume; source.pitch = 1; source.spatialBlend = 1; source.dopplerLevel = 0;
            source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = 1; source.maxDistance = 12;
            var spatial = child.GetComponent<VRCSpatialAudioSource>();
            if (!spatial) spatial = child.gameObject.AddComponent<VRCSpatialAudioSource>();
            spatial.EnableSpatialization = true; spatial.Gain = 0; spatial.Near = 1; spatial.Far = 12;
            spatial.VolumetricRadius = 0; spatial.UseAudioSourceVolumeCurve = true;

            var controller = (AnimatorController)module.GetComponent<ModularAvatarMergeAnimator>().animator;
            if (!controller.parameters.Any(p => p.name == Parameter))
                controller.AddParameter(new AnimatorControllerParameter { name = Parameter, type = AnimatorControllerParameterType.Bool, defaultBool = true });
            var sm = controller.layers[0].stateMachine;
            var a = sm.states.First(s => s.state.name == "A - Ready").state;
            var b = sm.states.First(s => s.state.name == "B - Ready").state;
            var ab = sm.states.First(s => s.state.name == "Transform A to B").state;
            var ba = sm.states.First(s => s.state.name == "Transform B to A").state;
            AddSoundBranch(sm, a, ab, b, true, d.forward, clip);
            AddSoundBranch(sm, b, ba, a, false, d.reverse, clip);
            ConfigureTiming(controller);

            // The source stays active across the ready states, allowing its tail to finish.
            // This independent volume layer also silences an already-playing tail immediately.
            for (int i = controller.layers.Length - 1; i > 0; i--)
                if (controller.layers[i].name == MuteLayer) controller.RemoveLayer(i);
            controller.AddLayer(MuteLayer);
            var layers = controller.layers; var layer = layers[layers.Length - 1]; layer.defaultWeight = 1;
            var on = layer.stateMachine.AddState("Sound On"); on.motion = VolumeClip(true); on.writeDefaultValues = false;
            var off = layer.stateMachine.AddState("Sound Off"); off.motion = VolumeClip(false); off.writeDefaultValues = false;
            layer.stateMachine.defaultState = on;
            Switch(on, off, false); Switch(off, on, true); controller.layers = layers;

            var pars = module.GetComponent<ModularAvatarParameters>();
            pars.parameters.RemoveAll(p => p.nameOrPrefix == Parameter);
            pars.parameters.Add(new ParameterConfig { nameOrPrefix = Parameter, syncType = ParameterSyncType.Bool,
                defaultValue = 1, saved = true, hasExplicitDefaultValue = true });
            var menu = module.GetComponent<ModularAvatarMenuInstaller>().menuToAppend;
            menu.controls.RemoveAll(c => c.parameter != null && c.parameter.name == Parameter);
            menu.controls.Add(new VRCExpressionsMenu.Control { name = "Transformation Sound", type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = Parameter }, value = 1 });
            EditorUtility.SetDirty(source); EditorUtility.SetDirty(spatial); EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(pars); EditorUtility.SetDirty(menu);
        }

        static void AddSoundBranch(AnimatorStateMachine sm, AnimatorState ready, AnimatorState audible,
            AnimatorState destination, bool toB, AnimationClip motion, AudioClip clip)
        {
            ready.transitions.First(t => t.destinationState == audible).AddCondition(AnimatorConditionMode.If, 0, Parameter);
            var silent = sm.AddState(audible.name + " - Silent"); silent.motion = motion; silent.writeDefaultValues = false;
            var enter = ready.AddTransition(silent); enter.duration = 0; enter.hasExitTime = false;
            enter.AddCondition(toB ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, ScanBuilder.Parameter);
            enter.AddCondition(AnimatorConditionMode.IfNot, 0, Parameter);
            var exit = silent.AddTransition(destination); exit.duration = 0; exit.hasExitTime = true; exit.exitTime = 1;
            foreach (var state in new[] { audible, silent })
            {
                var audio = state.AddStateMachineBehaviour<VRCAnimatorPlayAudio>();
                audio.SourcePath = SourcePath; audio.Clips = new[] { clip };
                audio.PlaybackOrder = VRC_AnimatorPlayAudio.Order.Roundabout;
                audio.ClipsApplySettings = VRC_AnimatorPlayAudio.ApplySettings.AlwaysApply;
                audio.LoopApplySettings = VRC_AnimatorPlayAudio.ApplySettings.AlwaysApply;
                audio.PitchApplySettings = VRC_AnimatorPlayAudio.ApplySettings.AlwaysApply;
                audio.VolumeApplySettings = VRC_AnimatorPlayAudio.ApplySettings.NeverApply;
                audio.Pitch = Vector2.one; audio.Loop = false; audio.DelayInSeconds = 0;
                audio.StopOnEnter = true; audio.PlayOnEnter = toB && state == audible;
                audio.StopOnExit = false; audio.PlayOnExit = false;
                EditorUtility.SetDirty(audio);
            }
        }

        // An audio source cannot start before the user's request. Hold outfit A for
        // 0.7 seconds while sound starts, then enter the unchanged transformation clip.
        public static void ConfigureTiming(AnimatorController controller)
        {
            var sm = controller.layers[0].stateMachine;
            var ready = sm.states.First(s => s.state.name == "A - Ready").state;
            var forward = sm.states.First(s => s.state.name == "Transform A to B").state;
            var hold = ready.motion as AnimationClip;
            if (!hold || hold.length <= 0) throw new InvalidOperationException("A ready clip is required for the audio lead-in.");
            var lead = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == LeadState);
            if (!lead) lead = sm.AddState(LeadState, new Vector3(325, -100));
            lead.motion = hold; lead.speed = hold.length / AudioLeadSeconds;
            lead.writeDefaultValues = false;
            var entry = ready.transitions.First(t => t.destinationState == forward || t.destinationState == lead);
            entry.destinationState = lead;
            if (!lead.transitions.Any(t => t.destinationState == forward))
            {
                var transition = lead.AddTransition(forward);
                transition.hasExitTime = true; transition.exitTime = 1;
                transition.duration = 0; transition.canTransitionToSelf = false;
            }
            var template = forward.behaviours.OfType<VRCAnimatorPlayAudio>().First();
            var play = lead.behaviours.OfType<VRCAnimatorPlayAudio>().FirstOrDefault();
            if (!play) play = lead.AddStateMachineBehaviour<VRCAnimatorPlayAudio>();
            EditorUtility.CopySerialized(template, play);
            play.DelayInSeconds = 0; play.PlayOnEnter = true; play.StopOnEnter = true;
            play.PlayOnExit = false; play.StopOnExit = false;
            foreach (var state in sm.states.Select(s => s.state).Where(s => s.name.StartsWith("Transform ")))
                foreach (var audio in state.behaviours.OfType<VRCAnimatorPlayAudio>())
                {
                    audio.DelayInSeconds = 0; audio.PlayOnEnter = false;
                    // Entering the visual animation must not stop or restart its lead-in sound.
                    if (state == forward) audio.StopOnEnter = false;
                    EditorUtility.SetDirty(audio);
                }
            EditorUtility.SetDirty(play); EditorUtility.SetDirty(entry);
            EditorUtility.SetDirty(lead); EditorUtility.SetDirty(sm); EditorUtility.SetDirty(controller);
        }

        static AnimationClip VolumeClip(bool on)
        {
            string path = ScanBuilder.Folder + "/Animations/Sound_" + (on ? "On" : "Off") + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (!clip) { clip = new AnimationClip { name = on ? "Sound_On" : "Sound_Off", frameRate = 60 }; AssetDatabase.CreateAsset(clip, path); }
            clip.ClearCurves();
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(SourcePath, typeof(AudioSource), "m_Volume"),
                AnimationCurve.Constant(0, 1f / 60, on ? ScanBuilder.Data.audioVolume : 0));
            EditorUtility.SetDirty(clip); return clip;
        }

        static void Switch(AnimatorState from, AnimatorState to, bool on)
        {
            var t = from.AddTransition(to); t.duration = 0; t.hasExitTime = false;
            t.AddCondition(on ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, Parameter);
        }
    }
}
