using TMPro;
using UnityEngine;

public class SongController : MonoBehaviour
{
    public Board board;

    public AudioClip getLuckyClip;
    public AudioClip bakaMitaiClip;
    public AudioClip galdinQuayClip;

    public TextMeshProUGUI songChosen;

    public AudioSource music;
    private float bpm;

    private float lowVolume = 0.3f; // 30% volume
    private float endTime = 15f; // 15 seconds
    private float fadeDuration = 2f; // 2 seconds

    private bool isMusicOn = true;
    private bool musicHasEnded = false;
    private bool isDemo = false;

    public void SetSong(AudioClip selectedClip, float selectedBPM)
    {
        if (music == null)
            music = gameObject.AddComponent<AudioSource>();

        music.clip = selectedClip;
        bpm = selectedBPM;
        
        music.volume = lowVolume;
        isDemo = true;
        music.Play(); // play demo

        // store player song selection in playerprefs
        PlayerPrefs.SetString("SelectedSong", selectedClip.name);
        PlayerPrefs.SetFloat("SelectedBPM", selectedBPM);
        PlayerPrefs.Save();

        // display demo song chosen in UI
        songChosen.text = "Selected: " + selectedClip.name;
    }

    private void Update() // play only 10 seconds of the song (demo)
    {
        // check if music ended -> go to game over screen
        if (!musicHasEnded && music.time >= music.clip.length)
        {
            musicHasEnded = true;
            board.GameOver();
        }

        if(music != null && isDemo)
            if (music.time >= endTime)
                FadeOutMusic(music);
    }

    public void PlayMusic()
    {
        isDemo = false;

        if (music == null)
            music = gameObject.AddComponent<AudioSource>();

        // get song name from playerprefs
        string selectedSong = PlayerPrefs.GetString("SelectedSong", "");
        float bpm = PlayerPrefs.GetFloat("SelectedBPM", 116f);

        // select AudioClip based on song name saved in player prefs
        switch (selectedSong)
        {
            case "Get Lucky":
                music.clip = getLuckyClip;
                music.volume = 0.3f;
                break;
            case "Baka Mitai":
                music.clip = bakaMitaiClip;
                music.volume = 0.3f;
                break;
            case "Galdin Quay":
                music.clip = galdinQuayClip;
                music.volume = 0.3f;
                break;
            default:
                Debug.LogError("No valid song found in PlayerPrefs.");
                return;
        }

        // display song chosen in UI
        songChosen.text = "Playing: " + selectedSong;

        if (music.clip != null)
            music.Play();
    }


    private void FadeOutMusic(AudioSource music)
    {
        music.volume -= Time.deltaTime / fadeDuration; // lower volume for duration of fadeDuration
        if (music.volume <= 0.0f)
            music.Stop();
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (music != null)
            music.mute = !isMusicOn;
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
}