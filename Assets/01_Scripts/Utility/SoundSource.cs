using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundSource : MonoBehaviour
{
    private AudioSource audioSource;

    public void Play(AudioClip clip, float soundEffectVolume, float soundEffectPitchVariance)
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        CancelInvoke();

        audioSource.clip = clip;
        audioSource.volume = soundEffectVolume;

        audioSource.pitch = 1f + Random.Range(-soundEffectPitchVariance, soundEffectPitchVariance);
        audioSource.Play();

        Invoke(nameof(Disable), clip.length + 0.1f);
    }

    private void Disable()
    {
        GetComponent<AudioSource>().Stop();
        SoundManager.Instance.ReturnSoundSource(this);
    }
}
