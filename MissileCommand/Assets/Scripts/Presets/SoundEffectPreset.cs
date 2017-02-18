using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New SoundEffectPreset.asset", menuName = "ScriptableObject/SoundEffectPreset")]
public class SoundEffectPreset : ScriptableObject
{
    public AudioMixerGroup m_mixerGroup;
    public AudioClip[] m_clips;
    public bool m_loop;
    public int m_loopCount = 2;
    public bool m_randomizeVolume = false;
    public float m_volumeDefault = 1f;
    public float m_volumeMin = 0.9f;
    public float m_volumeMax = 1.1f;
    public float m_rangeMin = 3f;
    public float m_rangeMax = 20f;
    public AudioRolloffMode m_rollOff = AudioRolloffMode.Linear;
    public bool m_randomizePitch = false;
    public float m_pitchDefault = 1f;
    public float m_pitchMin = 0.9f;
    public float m_pitchMax = 1.1f;

    public void SetupAudioSource(AudioSource source)
    {
        SetupAudioSource(source, true);
    }

    public void SetupAudioSource(AudioSource source, bool setVolume)
    {
        source.loop = m_loop;
        source.minDistance = m_rangeMin;
        source.maxDistance = m_rangeMax;
        source.rolloffMode = m_rollOff;
        source.clip = GetClip();
        source.pitch = GetPitch();
        source.outputAudioMixerGroup = m_mixerGroup;

        if (setVolume)
            source.volume = GetVolume();
    }

    public AudioClip GetClip()
    {
        return m_clips[Random.Range(0, m_clips.Length)];
    }

    public AudioClip GetClip(int index)
    {
        if (index > 0 && index < m_clips.Length)
            return m_clips[index];

        return null;
    }

    public float GetPitch()
    {
        if (m_randomizePitch)
            return Random.Range(m_pitchMin, m_pitchMax);

        return m_pitchDefault;
    }

    public float GetVolume()
    {
        if (m_randomizeVolume)
            return Random.Range(m_volumeMin, m_volumeMax);

        return m_volumeDefault;
    }

    public void PlayAt(Vector3 position)
    {
        GameObject go = new GameObject("AudioSource_" + name);
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        PlayOnSource(source);
        
        Destroy(go, m_loop ? source.clip.length * m_loopCount : source.clip.length + 0.5f);
    }

    public void PlayOnSource(AudioSource audioSource)
    {
        SetupAudioSource(audioSource);
        audioSource.Play();
    }
}
