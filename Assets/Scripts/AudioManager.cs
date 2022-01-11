using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel {Master,Sfx,Music};
    public float masterVolumePercent { get; private set; } = 1f;
    public float sfxVolumePercent { get; private set; } = 1;
    public float musicVolumePercent { get; private set; } = 1;

    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;
    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    private void Awake()
    {

        if (instance != null) {
            Destroy(gameObject);
        } else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject newSfx2DSource = new GameObject("2D sfx source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;

            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
            audioListener = FindObjectOfType<AudioListener>().transform;

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", masterVolumePercent);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", sfxVolumePercent);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", musicVolumePercent);
            PlayerPrefs.Save();

        }
    }

    private void Update()
    {
        if (playerT != null)
        {
            audioListener.position = new Vector3(playerT.position.x,0,playerT.position.z);
        }
    }

    void OnLevelWasLoaded(int index)
    {
        if (playerT == null)
        {
            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
        }
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void PlaySound(string soundName, Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent);
    }

    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }
}
