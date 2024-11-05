using UnityEngine;

public class SelectSongController : MonoBehaviour
{
    public AudioClip getLuckyClip;
    public AudioClip bakaMitaiClip;
    public AudioClip galdinQuayClip;

    public AudioSource music;

    public float bpm;
    public float allowedWindow = 1f;    // allowed time frame around the beat
    public float timePerBeat;
    public float timeSinceLastBeat;

    private bool isMusicOn = true;

    public void Initialize()
    {
        this.timePerBeat = 60f / bpm;    // in seconds
        this.timeSinceLastBeat = 0f;     // start with no time passed since last beat
    }

    public void SetSong(AudioClip selectedClip, float selectedBPM)
    {
        if (music == null)
            music = gameObject.AddComponent<AudioSource>();

        music.clip = selectedClip;
        bpm = selectedBPM;
        
        Initialize();   // recalculate beat timings
        music.Play();   // play demo

        // store player song selection in playerprefs
        PlayerPrefs.SetString("SelectedSong", selectedClip.name);
        PlayerPrefs.SetFloat("SelectedBPM", selectedBPM);
        PlayerPrefs.Save();
    }

    public void PlayGetLucky()
    {
        SetSong(getLuckyClip, 116f);
    }

    public void PlayBakaMitai()
    {
        SetSong(bakaMitaiClip, 116f);
    }

    public void PlayGaldinQuay()
    {
        SetSong(galdinQuayClip, 121f);
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (music != null)
            music.mute = !isMusicOn;
    }
}