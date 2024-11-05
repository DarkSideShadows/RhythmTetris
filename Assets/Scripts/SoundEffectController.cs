using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SoundEffectController : MonoBehaviour
{
    public AudioSource lockSound;
    public AudioSource beatSound;
    public AudioSource lineBreakSound;
    public AudioSource gameOverSound;

    public void PlayLockSound()
    {
        lockSound.Play();
    }

    public void PlayBeatSound()
    {
        beatSound.Play();
    }

    public void PlaylineBreakSound()
    {
        lineBreakSound.Play();
    }

    public void PlayGameOverSound()
    {
        gameOverSound.Play();
    }
}