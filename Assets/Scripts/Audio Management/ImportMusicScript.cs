using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImportMusicScript : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown musicDropdown;
    [NonSerialized] public AudioSource audioSource;

    [Tooltip("Default Resources subfolder to load audio from (e.g. place clips in Assets/Resources/Music)")]
    public string resourcesSubfolder = "Music";
    
    [NonSerialized] public Dictionary<string, Sound> musicMap;

    private void Awake()
    {
        PopulateFromResources();
    }

    // Existing method to populate from a provided map
    private void PopulateFromMap(Dictionary<string, Sound> map)
    {
        if (map == null) return;
        musicMap = map;

        if (musicDropdown == null) return;
        musicDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var key in musicMap) options.Add(new TMP_Dropdown.OptionData(key.Key));
        musicDropdown.AddOptions(options);

        musicDropdown.onValueChanged.RemoveAllListeners();
        musicDropdown.onValueChanged.AddListener(PlaySelected);
    }
    
    public void SetDropDownSelection(int index)
    {
        if (musicDropdown == null) return;
        if (index < 0 || index >= musicDropdown.options.Count) return;
        musicDropdown.value = index;
    }
    
    public void GetBackgroundSounds(out Sound[] backgroundSounds)
    {
        backgroundSounds = musicMap?.Values.ToArray();
    }

    // Runtime: load all AudioClip assets from the specified Resources subfolder
    private void PopulateFromResources(string subfolder = null)
    {
        var folder = string.IsNullOrEmpty(subfolder) ? resourcesSubfolder : subfolder;
        if (string.IsNullOrEmpty(folder)) return;

        var clips = Resources.LoadAll<AudioClip>(folder);
        if (clips == null || clips.Length == 0) return;

        var map = clips.ToDictionary(c => c.name, c => new Sound { name = c.name, clip = c });
        PopulateFromMap(map);
    }

    // Called automatically when dropdown value changes
    private void PlaySelected(int index) 
    {
        if (index < 0 || index >= musicMap.Count) return;
        AudioManager.instance.PlayBackground(index);
    }

    // Useful for wiring up UI Buttons: set button OnClick to call PlayByName with the song key
    public void PlayByName(string name)
    {
        if (musicMap == null || string.IsNullOrEmpty(name)) return;
        if (!musicMap.TryGetValue(name, out var sound) || sound == null) return;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.clip = sound.clip;
        audioSource.Play();
    }

#if UNITY_EDITOR
    // Editor-only: provide one AudioClip reference to locate the folder that contains it,
    // then load all AudioClips from the same folder using AssetDatabase.
    public void PopulateFromReference(AudioClip referenceClip)
    {
        if (referenceClip == null) return;

        var assetPath = AssetDatabase.GetAssetPath(referenceClip);
        if (string.IsNullOrEmpty(assetPath)) return;

        var directory = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(directory)) return;

        // Find all AudioClip assets in that folder
        var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { directory });
        var map = new Dictionary<string, Sound>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null && !map.ContainsKey(clip.name))
                map.Add(clip.name, new Sound { name = clip.name, clip = clip });
        }

        PopulateFromMap(map);
    }
#endif
}
