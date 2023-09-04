using Peg;
using Peg.Lazarus;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "ContinuousFx", menuName = "Assets/Useable Tools/Continuous Fx")]
    public class ContinuousFx : AbstractFx
    {
        public bool LoopSound;

        string PartSys;
        string AudioSource;
        string Started;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            AudioSource = RegisterVar("AudioSrc");
            Started = RegisterVar("Started");
            PartSys = RegisterVar("PartSys");
        }

        protected override void OnDisable()
        {

        }

        protected override void OnDestroy()
        {

        }

        public override void ToolEnabled(ITool tool)
        {
            var audio = tool.gameObject.AddComponent<AudioSource>();
            audio.outputAudioMixerGroup = MixerGroup;
            audio.playOnAwake = false;
            audio.Stop();
            tool.SetInstVar(AudioSource, audio);
        }

        public override void ToolDisabled(ITool tool)
        {
            Destroy(tool.GetInstVar<AudioSource>(AudioSource));
        }

        public override void Use(ITool tool)
        {
            if (tool.GetInstVar<bool>(Started))
                return;

            tool.SetInstVar(Started, true);
            var trans = tool.gameObject.transform;
            tool.SetInstVar(PartSys, PlayParticle(tool, trans.position, trans.forward));

            if (ClipsUse != null && ClipsUse.Length > 0)
            {
                AudioClip clip;
                clip = ClipsUse[Random.Range(0, ClipsUse.Length)];
                PlaySoundDirect(tool.GetInstVar<AudioSource>(AudioSource), clip, LoopSound);
            }
        }

        public override void EndUse(ITool tool)
        {
            var ps = tool.GetInstVar<ParticleSystem>(PartSys);
            if (ps != null)
            {
                ps.Stop();
                Lazarus.Instance.RelenquishToPool(ps.gameObject);
            }

            var src = tool.GetInstVar<AudioSource>(AudioSource);
            src.Stop();

            //only difference between failed and ending
            tool.SetInstVar(Started, false);
        }

        public override void UseFailed(ITool tool)
        {
            var ps = tool.GetInstVar<ParticleSystem>(PartSys);
            if (ps != null)
            {
                ps.Stop();
                Lazarus.Instance.RelenquishToPool(ps.gameObject);
            }

            var src = tool.GetInstVar<AudioSource>(AudioSource);
            src.Stop();
        }


    }
}
