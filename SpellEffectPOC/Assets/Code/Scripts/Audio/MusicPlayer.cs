using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip [] songs;
    [SerializeField] float musicVolume = 0.65f;

    private int currentSongIndex = 0;
    // Start is called before the first frame update


    private void Start()
    {
        source.volume = musicVolume;
    }

    public void Play()
    {
        AudioClip currentSong = songs[currentSongIndex];
        source.Stop();
        if (currentSong == null) return;
        source.PlayOneShot(currentSong);
    }

    public void PlayNextSong()
    {
        currentSongIndex = (currentSongIndex+1) % songs.Length;
        Play();
    }


    public void Stop()
    {
        source.Stop();
    }

    public void LowerMusicVolume()
    {
        source.volume = 0.3f;
    }

    public void IncreaseMusicVolume()
    {
        Invoke("increaseVolume", 2.5f);
    }

    private void increaseVolume()
    {
        source.volume = musicVolume;
    }

}
