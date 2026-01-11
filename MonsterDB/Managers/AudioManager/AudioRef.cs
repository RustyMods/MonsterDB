using UnityEngine;

namespace MonsterDB;

public class AudioRef
{
    public string filePath;
    public AudioClip? clip;
    public GameObject? sfx;

    public AudioRef(string filePath)
    {
        this.filePath = filePath;
        clip = AudioManager.ReadAudioFile(filePath);
        if (clip != null)
        {
            Clone clone = new Clone("sfx_Bonemass_idle", clip.name);
            clone.OnCreated += p =>
            {
                if (p.TryGetComponent(out ZSFX component))
                {
                    component.m_captionType = ClosedCaptions.CaptionType.Enemy;
                    component.m_closedCaptionToken = clip.name;
                    component.m_secondaryCaptionToken = "";
                    component.m_audioClips = new AudioClip[] { clip };
                    sfx = p;
                }

                if (p.TryGetComponent(out TimedDestruction timedDestruction))
                {
                    timedDestruction.m_timeout = clip.length;
                }
                MonsterDBPlugin.LogInfo($"Loaded SFX as {p.name}");
            };

            AudioManager.clips[clip.name] = this;
        }
    }
}