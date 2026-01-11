using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public Sound[] backgroundSounds;
    
    [SerializeField] private AudioSource _backgroundAudioSource;

    public static AudioManager instance;

    [SerializeField] private ImportMusicScript importMusicScript;
    [SerializeField] private AudioSliderUpdate audioSlider;
    [SerializeField] private GameObject muteButton;
    [SerializeField] private  GameObject unmuteButton;

    public bool themePlaying;

    private bool mutedSounds;

    public float[] resetVolume;

    private int _currentBackgroundIndex = -1;
    private Coroutine _backgroundWatcher;
    private float _backgroundEndTime;
    private float? _backgroundPausedSince;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        resetVolume = new float[sounds.Length];

        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].source = gameObject.AddComponent<AudioSource>();
            sounds[i].source.clip = sounds[i].clip;

            sounds[i].source.volume = sounds[i].volume;
            sounds[i].source.pitch = sounds[i].pitch;
            sounds[i].source.loop = sounds[i].loop;

            resetVolume[i] = sounds[i].source.volume;

        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        
        importMusicScript.audioSource = _backgroundAudioSource;
        importMusicScript.GetBackgroundSounds(out backgroundSounds);
        if (backgroundSounds.Length > 0) PlayBackground(0);
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        s.source.Stop();
    }

    public void FadeOut(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        float ogVol = s.source.volume;

        if (s.source.volume > 0)
        {
            s.source.volume -= 0.07f * Time.deltaTime;
        }
        else
        {
            if (!mutedSounds) s.source.volume = ogVol;
            Stop(name);
        }
    }

    public void ResetVolume(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        int i = Array.IndexOf(sounds, s);
        s.source.volume = resetVolume[i];
    }

    private void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }

    public void MuteSounds()
    {
        muteButton.SetActive(false);
        unmuteButton.SetActive(true);
        StopAllSounds();
        StopBackground();
        ChangeVolume(0);
        mutedSounds = true;
    }
    
    public void UnmuteSounds()
    {
        if (mutedSounds)
        {
            muteButton.SetActive(true);
            unmuteButton.SetActive(false);
            mutedSounds = false;
        }
        ContinueBackground();
        ChangeVolume((int)audioSlider.audioSlider.value);
    }

    public void ChangeVolume(int volume)
    {
        if (mutedSounds) UnmuteSounds();
        var value = volume / 100f;
        foreach (Sound s in sounds)
        {
            s.source.volume = value;
        }
        
        _backgroundAudioSource.volume = value;
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        StopAllSounds();
    }

    public void PlayBackground(int index)
    {
        if (backgroundSounds == null || backgroundSounds.Length == 0) return;
        index = Mathf.Clamp(index, 0, backgroundSounds.Length - 1);
        _currentBackgroundIndex = index;

        Sound s = backgroundSounds[index];
        if (s == null || s.clip == null) return;
        
        if (_backgroundWatcher != null)
        {
            StopCoroutine(_backgroundWatcher);
            _backgroundWatcher = null;
        }

        _backgroundAudioSource.clip = s.clip;
        _backgroundAudioSource.Play();
        
        // calculate realtime end time (remaining length from current play position)
        _backgroundEndTime = Time.realtimeSinceStartup + (s.clip.length - _backgroundAudioSource.time);
        _backgroundPausedSince = null;

        _backgroundWatcher = StartCoroutine(BackgroundWatcher());
        importMusicScript.SetDropDownSelection(index);
    }

    // Updated BackgroundWatcher (uses realtime end time and extends it while paused)
    private IEnumerator BackgroundWatcher()
    {
        if (_backgroundAudioSource == null || _backgroundAudioSource.clip == null)
        {
            _backgroundWatcher = null;
            yield break;
        }

        AudioClip clip = _backgroundAudioSource.clip;

        while (Time.realtimeSinceStartup < _backgroundEndTime)
        {
            if (_backgroundAudioSource == null || _backgroundAudioSource.clip != clip)
            {
                // clip changed or source gone
                _backgroundWatcher = null;
                yield break;
            }

            if (!_backgroundAudioSource.isPlaying)
            {
                // start pause timer if not already
                if (_backgroundPausedSince == null)
                    _backgroundPausedSince = Time.realtimeSinceStartup;
            }
            else
            {
                // if we were paused, extend end time by paused duration
                if (_backgroundPausedSince != null)
                {
                    float pausedDuration = Time.realtimeSinceStartup - _backgroundPausedSince.Value;
                    _backgroundEndTime += pausedDuration;
                    _backgroundPausedSince = null;
                }
            }

            yield return null;
        }

        // final safety: ensure clip didn't change
        if (_backgroundAudioSource == null || _backgroundAudioSource.clip != clip)
        {
            Debug.Log("BackgroundWatcher: clip changed or source gone at end");
            _backgroundWatcher = null;
            yield break;
        }

        // Wait until playback actually stops (in case of rounding)
        while (_backgroundAudioSource != null && _backgroundAudioSource.isPlaying)
        {
            yield return new WaitForFixedUpdate();
            Debug.Log("BackgroundWatcher: waiting for playback to stop");
        }

        _backgroundWatcher = null;
        PlayNextBackground();
    }

    private void PlayNextBackground()
    {
        if (backgroundSounds == null || backgroundSounds.Length == 0) return;
        int next = (_currentBackgroundIndex + 1) % backgroundSounds.Length;
        PlayBackground(next);
    }
    
    private void ContinueBackground()
    {
        if (_backgroundAudioSource == null || _backgroundAudioSource.isPlaying) return;
        _backgroundAudioSource.Play();
        _backgroundWatcher ??= StartCoroutine(BackgroundWatcher());
    }

    private void StopBackground()
    {
        if (_backgroundWatcher != null)
        {
            StopCoroutine(_backgroundWatcher);
            _backgroundWatcher = null;
        }
        if (_backgroundAudioSource != null) _backgroundAudioSource.Stop();
    }
}
