using Sirenix.OdinInspector;
using Toolbox;
using Toolbox.Behaviours;
using Toolbox.Lazarus;
using Toolbox.Math;
using UnityEngine;


namespace ToolFx
{ 
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Sound Fx", menuName = "Assets/Useable Tools/Fx")]
    public class EffectTool : AbstractFx
    {
        [Space(10)]
        [PropertyOrder(-1)]
        [Tooltip("When is this effect triggered?")]
        public Tool.TriggerPoint Trigger;



        protected override void OnDisable()
        {

        }

        protected override void OnDestroy()
        {

        }

        public override void Use(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
            {
                var trans = tool.gameObject.transform;
                PlayParticle(tool, trans.position, trans.forward);
                PlaySoundOneshot(tool, trans.position);
            }
        }

        public override void EndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnEndUse)
            {
                var trans = tool.gameObject.transform;
                PlayParticle(tool, trans.position, trans.forward);
                PlaySoundOneshot(tool, trans.position);
            }
        }

        public override void UseFailed(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnFailed)
            {
                var trans = tool.gameObject.transform;
                PlayParticle(tool, trans.position, trans.forward);
                PlaySoundOneshot(tool, trans.position);
            }
        }

        public override void ToolDisabled(ITool tool)
        {
            TempAudioSourcePlayer.Instance.StopAllFromSource(tool.gameObject.GetInstanceID());
        }

        public override void ToolDestroyed(ITool tool)
        {
            TempAudioSourcePlayer.Instance.StopAllFromSource(tool.gameObject.GetInstanceID());
        }




    }



    /// <summary>
    /// Base for ToolFx that plays sounds and particle effects.
    /// </summary>
    public abstract class AbstractFx : AbstractToolEffect
    {
        [Space(10)]
        [Title("Shared")]
        [Tooltip("Should this effect be parented to the tool emitting it?")]
        public bool ParentToSource = true;
        [Tooltip("Should particle systems be oriented to the tool's forward vector?")]
        public bool OrientForward = true;
        [Tooltip("An additional aim-offset applied to this effect in relation to the tool's own aim-offset.")]
        public Vector3 LocalAimOffset;

        [Space(10)]
        [Title("Sounds")]
        [Tooltip("A cooldown between sound effects being played.")]
        public float SoundCooldown;
        [Tooltip("The index of the TempAudio source to play any sound effects on.")]
        public int AudioIndex;
        [Tooltip("Should the audio effects have their mixer groups set?")]
        public bool OverrideMixerGroup;
        [ShowIf("OverrideMixerGroup")]
        [Indent(1)]
        [Tooltip("If overriding, what mixer group should the audio effects play on?")]
        public UnityEngine.Audio.AudioMixerGroup MixerGroup;
        [Tooltip("How will this sound integrate with other sounds already playing?")]
        public InterruptModes InterruptMode = InterruptModes.Overlap;
        [Tooltip("Volume of the sounds being played.")]
        public float SfxVolume = 1;
        public ChoiceModes AudioChoice;
        [Indent]
        [ShowIf("IsIndexedAudio")]
        public int ClipIndex = 0;
        public AudioClip[] ClipsUse;

        [Space(10)]
        [Title("Particles")]
        [Tooltip("A cooldown between particle effects being played.")]
        public float ParticleCooldown;
        [Tooltip("Should particle systems face the direction they are being fired, mirror on the x-axis, or stay the way they were spawned?")]
        public AimOffsetModes ParticleAimMode = AimOffsetModes.AimSpace;
        public ChoiceModes ParticleChoice;
        [Indent]
        [ShowIf("IsIndexedParticle")]
        public int ParticleIndex = 0;
        public ParticleSystem[] Effects;

        bool IsIndexedAudio => AudioChoice == ChoiceModes.Selection;
        bool IsIndexedParticle => ParticleChoice == ChoiceModes.Selection;


        public enum ChoiceModes
        {
            Random,
            All,
            Selection,
        }

        string LastSoundTime;
        string LastPartTime;


        protected override void OnEnable()
        {
            base.OnEnable();
            LastSoundTime = RegisterVar("LastSoundTime");
            LastPartTime = RegisterVar("LastPartTime");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        protected ParticleSystem PlayParticle(ITool tool, Vector3 position, Vector3 forward)
        {
            float t = Time.time;
            var toolTrans = tool.gameObject.transform;

            if (Effects != null && Effects.Length > 0)
            {
                if (ParticleCooldown <= 0 || Time.time - tool.GetInstVar<float>(LastPartTime) >= ParticleCooldown)
                {
                    tool.SetInstVar(LastPartTime, Time.time);

                    if(ParticleChoice == ChoiceModes.All)
                    {
                        for (int i = 0; i < Effects.Length; i++)
                            ParticleHelper(tool, toolTrans, position, forward, Effects[i]);
                    }
                    else
                    {
                        ParticleSystem part = ParticleChoice == ChoiceModes.Random ? Effects[Random.Range(0, Effects.Length)] : Effects[ParticleIndex];
                        ParticleHelper(tool, toolTrans, position, forward, part);
                        return part;
                    }

                }
            }

            return null;
        }

        /// <summary>
        /// Helepr used to play a single instance of a particle effect
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="toolTrans"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        public void ParticleHelper(ITool tool, Transform toolTrans, Vector3 position, Vector3 forward, ParticleSystem effect)
        {
            if (effect != null)
            {
                var go = Lazarus.Instance.Summon(effect.gameObject, false);
                var part = go.GetComponent<ParticleSystem>();
                Vector3 localOffset = tool.MirrorLocalOffset ? new Vector3(-LocalAimOffset.x, LocalAimOffset.y, LocalAimOffset.z) : LocalAimOffset;

                //var toolForward = toolTrans.forward;
                var trans = part.transform;
                if (ParentToSource)
                    trans.SetParent(toolTrans); //making it a child of the user so that it follows if parent moves. Particles can always be made to emit in world space if needed
                if (tool.AimMode == AimOffsetModes.AimSpace)
                {
                    trans.SetPositionAndRotation(MathUtils.ForwardSpaceOffset(position, forward, LocalAimOffset + tool.AimOffset),
                                                 Quaternion.LookRotation(forward, toolTrans.up));
                }
                else if (tool.AimMode == AimOffsetModes.Bilateral)
                {
                    trans.position = position + localOffset + tool.AimOffset;
                    if (ParticleAimMode == AimOffsetModes.AimSpace)
                        trans.rotation = Quaternion.LookRotation(forward, toolTrans.up);
                    else if (ParticleAimMode == AimOffsetModes.Bilateral)
                        trans.rotation = forward.x > 0 ? Quaternion.LookRotation(Vector3.right, trans.up) : Quaternion.LookRotation(Vector3.left, trans.up);
                }
                else
                {
                    trans.position = position + LocalAimOffset + tool.AimOffset;
                }
                part.gameObject.SetActive(true);
                part.Play();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="position"></param>
        protected void PlaySoundOneshot(ITool tool, Vector3 position)
        {

            if (ClipsUse != null && ClipsUse.Length > 0)
            {
                if (SoundCooldown <= 0 || Time.time - tool.GetInstVar<float>(LastSoundTime) >= SoundCooldown)
                {
                    tool.SetInstVar(LastSoundTime, Time.time);

                    
                    
                    if(AudioChoice == ChoiceModes.All)
                    {
                        for(int i = 0; i < ClipsUse.Length; i++)
                        {
                            if (OverrideMixerGroup)
                                TempAudioSourcePlayer.Instance.PlayWithInterruptMode(InterruptMode, AudioIndex, tool.gameObject.GetInstanceID(), ClipsUse[i], MixerGroup, position, SfxVolume);
                            else TempAudioSourcePlayer.Instance.PlayWithInterruptMode(InterruptMode, AudioIndex, tool.gameObject.GetInstanceID(), ClipsUse[i], position, SfxVolume);

                        }
                    }
                    else
                    {
                        AudioClip clip = AudioChoice == ChoiceModes.Random ? ClipsUse[Random.Range(0, ClipsUse.Length)] : ClipsUse[ClipIndex];
                        if (OverrideMixerGroup)
                            TempAudioSourcePlayer.Instance.PlayWithInterruptMode(InterruptMode, AudioIndex, tool.gameObject.GetInstanceID(), clip, MixerGroup, position, SfxVolume);
                        else TempAudioSourcePlayer.Instance.PlayWithInterruptMode(InterruptMode, AudioIndex, tool.gameObject.GetInstanceID(), clip, position, SfxVolume);
                        //OLD VERSION - BEFORE INTERRUPTMODES WERE IMPLEMENTED
                        //if(OverrideMixerGroup)
                        //    TempAudioSourcePlayer.StopLastAndPlayNew(AudioIndex, tool.gameObject.GetInstanceID(), randClip, MixerGroup, position, SfxVolume);
                        //else TempAudioSourcePlayer.StopLastAndPlayNew(AudioIndex, tool.gameObject.GetInstanceID(), randClip, position, SfxVolume);

                    }


                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="source"></param>
        /// <param name="clip"></param>
        protected void PlaySoundDirect(AudioSource source, AudioClip clip, bool loop)
        {
            source.Stop();
            source.clip = clip;
            source.loop = loop;
            source.Play();
        }
    }

}


