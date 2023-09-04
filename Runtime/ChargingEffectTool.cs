using Sirenix.OdinInspector;
using Peg;
using Peg.Behaviours;
using Peg.Lazarus;
using UnityEngine;
using Peg.Lib;

namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Fx - Charged", menuName = "Assets/Useable Tools/Fx - Charged")]
    public class ChargingEffectTool : AbstractToolEffect
    {
        public enum SoundPlayModes
        {
            StopLastSavePlayed,
            SavePlayed,
            PlayAndDontSave,
        }

        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        [Tooltip("How charge levels scale over time")]
        public AnimationCurve ChargeScale;

        [ShowIf("Trigger", Tool.TriggerPoint.OnEndUse)]
        [Indent]
        [Tooltip("If set, EndUse() cannot trigger until a paired Use() call has been made first.")]
        public bool RequireUseBeforeEnd = true;
        [Tooltip("If effects were played in one trigger, are they stopped if the other trigger occurs?")]
        public bool StopOnTriggerSwitch;
        [Tooltip("The index of the TempAudio source to play any sound effects on.")]
        public int AudioIndex;
        [Tooltip("Should the audio effects have their mixer groups set?")]
        public bool OverrideMixerGroup;
        [Tooltip("If overriding, what mixer group should the audio effects play on?")]
        [ShowIf("OverrideMixerGroup")]
        [Indent(1)]
        public UnityEngine.Audio.AudioMixerGroup MixerGroup;
        [Tooltip("Should this effect be parented to the tool emitting it?")]
        public bool ParentToSource = true;
        [Tooltip("An additional aim-offset applied to this effect in relation to the tool's own aim-offset.")]
        public Vector3 LocalAimOffset;
        [Tooltip("Should particle systems face the direction they are being fired, mirror on the x-axis, or stay the way they were spawned?")]
        public AimOffsetModes ParticleAimMode = AimOffsetModes.AimSpace;
        [Tooltip("Volume of the sounds being played.")]
        public float SfxVolume = 1;
        [Tooltip("Is the last sound fx stopped before playing a new one?")]
        public SoundPlayModes SoundPlayMode = SoundPlayModes.StopLastSavePlayed;
        [Tooltip("A cooldown between sound effects being played.")]
        public float SoundCooldown;
        [Tooltip("When the trigger switches, does the sound cooldown timer reset?")]
        public bool ResetSoundCooldown = false;
        [Tooltip("A cooldown between sound effects being played.")]
        public float ParticleCooldown;
        public AudioClip[] ClipsUse;
        public ParticleSystem[] Effects;

        string StartTime;
        string LastSoundTime;
        string LastParticleTime;
        string Using;


        protected override void OnEnable()
        {
            base.OnEnable();
            StartTime = RegisterVar("StartTime");
            LastSoundTime = RegisterVar("LastSoundTime");
            LastParticleTime = RegisterVar("LastParticleTime");
            Using = RegisterVar("Using");
        }

        protected override void OnDisable()
        {
        }

        protected override void OnDestroy()
        {
        }

        public override void ToolDisabled(ITool tool)
        {
            TempAudioSourcePlayer.Instance.StopAllFromSource(tool.gameObject.GetInstanceID());
        }

        public override void ToolDestroyed(ITool tool)
        {
            TempAudioSourcePlayer.Instance.StopAllFromSource(tool.gameObject.GetInstanceID());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void Use(ITool tool)
        {
            if (RequireUseBeforeEnd)
                tool.SetInstVar(Using, true);
            if (tool.GetInstVar<float>(StartTime) == 0.0f)
                tool.SetInstVar(StartTime, Time.time);
            if (Trigger == Tool.TriggerPoint.OnEndUse)
            {
                if (ResetSoundCooldown)
                    tool.SetInstVar(LastSoundTime, 0.0f);
                if (StopOnTriggerSwitch)
                    TempAudioSourcePlayer.Instance.StopAllFromSource(tool.gameObject.GetInstanceID());
            }

            if (Trigger == Tool.TriggerPoint.OnUse)
                Process(tool, Mathf.FloorToInt(ChargeScale.Evaluate(Time.time - tool.GetInstVar<float>(StartTime) )));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void EndUse(ITool tool)
        {
            if (!RequireUseBeforeEnd || tool.GetInstVar<bool>(Using))
            {
                tool.SetInstVar(Using, false);
                if (Trigger == Tool.TriggerPoint.OnUse)
                {
                    if (ResetSoundCooldown)
                        tool.SetInstVar(LastSoundTime, 0.0f);
                    if (StopOnTriggerSwitch)
                        TempAudioSourcePlayer.Instance.StopAllFromSource(tool.gameObject.GetInstanceID());
                }

                if (Trigger == Tool.TriggerPoint.OnEndUse)
                    Process(tool, Mathf.FloorToInt(ChargeScale.Evaluate(Time.time - tool.GetInstVar<float>(StartTime))));
            }

            if (ResetSoundCooldown)
                tool.SetInstVar(LastSoundTime, 0.0f);
            tool.SetInstVar(StartTime, 0.0f);
        }

        /// <summary>
        /// Resets the internal use timer
        /// </summary>
        /// <param name="tool"></param>
        public override void CancelUse(ITool tool)
        {
            tool.SetInstVar(StartTime, 0.0f);
            tool.SetInstVar(LastSoundTime, 0.0f);
            tool.SetInstVar(LastParticleTime, 0.0f);
            tool.SetInstVar(Using, false);
        }

        public void Process(ITool tool, int level)
        {
            float t = Time.time;

            //If you are reading this, you have spent way too much time looking at this small clip of the video.
            var toolGo = tool.gameObject;
            var toolTrans = toolGo.transform;
            var userTrans = toolTrans.parent == null ? toolTrans : toolTrans.parent;
            var userPos = toolTrans.position;
            

            if (Effects != null && Effects.Length > 0)
            {
                var randPart = Effects[level];
                if (randPart != null && (ParticleCooldown <= 0 || t - tool.GetInstVar<float>(LastParticleTime) >= ParticleCooldown))
                {
                    tool.SetInstVar(LastParticleTime, t);
                    var go = Lazarus.Instance.Summon(randPart.gameObject, false);
                    var part = go.GetComponent<ParticleSystem>();
                    Vector3 localOffset = tool.MirrorLocalOffset ? new Vector3(-LocalAimOffset.x, LocalAimOffset.y, LocalAimOffset.z) : LocalAimOffset;

                    var toolForward = toolTrans.forward;
                    var trans = part.transform;
                    if (ParentToSource)
                        trans.SetParent(toolTrans); //making it a child of the user so that it follows if parent moves. Particles can always be made to emit in world space if needed
                    if (tool.AimMode == AimOffsetModes.AimSpace)
                    {
                        trans.SetPositionAndRotation(MathUtils.ForwardSpaceOffset(toolTrans.position, toolForward, LocalAimOffset + tool.AimOffset),
                                                     Quaternion.LookRotation(toolForward, toolTrans.up));
                    }
                    else if (tool.AimMode == AimOffsetModes.Bilateral)
                    {
                        trans.position = userTrans.position + localOffset + tool.AimOffset;
                        if (ParticleAimMode == AimOffsetModes.AimSpace)
                            trans.rotation = Quaternion.LookRotation(toolForward, toolTrans.up);
                        else if (ParticleAimMode == AimOffsetModes.Bilateral)
                            trans.rotation = toolForward.x > 0 ? Quaternion.LookRotation(Vector3.right, trans.up) : Quaternion.LookRotation(Vector3.left, trans.up);
                    }
                    else
                    {
                        trans.position = toolTrans.position + LocalAimOffset + tool.AimOffset;
                    }
                    part.gameObject.SetActive(true);
                    part.Play();
                }
            }

            if (ClipsUse != null && ClipsUse.Length > 0)
            {
                if (SoundCooldown <= 0 || t - tool.GetInstVar<float>(LastSoundTime) >= SoundCooldown)
                {
                    tool.SetInstVar(LastSoundTime, t);
                    var randClip = ClipsUse[level];
                    if (SoundPlayMode == SoundPlayModes.StopLastSavePlayed)
                    {
                        if(OverrideMixerGroup)
                            TempAudioSourcePlayer.Instance.StopLastAndPlayNew(AudioIndex, toolGo.GetInstanceID(), randClip, MixerGroup, userPos, SfxVolume);
                        else TempAudioSourcePlayer.Instance.StopLastAndPlayNew(AudioIndex, toolGo.GetInstanceID(), randClip, userPos, SfxVolume);
                    }
                    else if (SoundPlayMode == SoundPlayModes.SavePlayed)
                    {
                        if(OverrideMixerGroup)
                            TempAudioSourcePlayer.Instance.Play(AudioIndex, toolGo.GetInstanceID(), randClip, MixerGroup, userPos, SfxVolume);
                        else TempAudioSourcePlayer.Instance.Play(AudioIndex, toolGo.GetInstanceID(), randClip, userPos, SfxVolume);
                    }
                    else
                    {
                        if(OverrideMixerGroup)
                            TempAudioSourcePlayer.Instance.Play(AudioIndex, randClip, MixerGroup, userPos, SfxVolume);
                        else TempAudioSourcePlayer.Instance.Play(AudioIndex, randClip, userPos, SfxVolume);
                    }
                }
            }
        }
    }
}
