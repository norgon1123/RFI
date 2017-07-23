using HoloToolkit.Unity;
using System;
using UnityEngine;
using System.Collections.Generic;

public class SoundManager : Singleton<SoundManager>
{
    // A game object that will be used to contain the gesture audio source.
    // This object will be moved to the location of the object responding to the gesture.
//    private GameObject audioSourceContainer;
//
//    private AudioSource audioSource;
//
//    private void Start()
//    {
//        audioSourceContainer = new GameObject("AudioSourceContainer", new Type[] { typeof(AudioSource) });
//        audioSource = audioSourceContainer.GetComponent<AudioSource>();
//
//        audioSource.spatialize = true;
//        audioSource.spatialBlend = 1.0f;
//        audioSource.dopplerLevel = 0.0f;
//        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
//        audioSource.loop = true;
//    }
//
//    public void SetAudio(GameObject focusedObject, bool navigating)
//    {
//        AudioClip audioClip = focusedObject.GetComponent<AudioSource>().clip;
//
//        if (audioClip != null && navigating)
//        {
//            // Move the audio source container to the location of the object so that
//            // the sound is properly spatialized with the object.
//            audioSourceContainer.transform.position = focusedObject.transform.position;
//
//            // Set the AudioSource clip field to the audioClip
//            audioSource.clip = audioClip;
//
//            // Play the AudioSource
//            if (!audioSource.isPlaying)
//            {
//                audioSource.Play();
//            }
//        }
//        else
//        {
//            // Stop the AudioSource
//            audioSource.Stop();
//        }
//    }

	public static SoundManager Instance;
	public Dictionary<string, string> AudioFiles;

	private void Awake(){
		if (Instance == null) {
			Instance = this;
		}
		CreateDictionary ();
	}

	private void CreateDictionary()
	{
		AudioFiles = new Dictionary<string, string>();
		AudioFiles.Add ("Pitch 5sec", Application.dataPath + "/Resources/Audio/440Hz_44100Hz_16bit_05sec.wav");
		AudioFiles.Add ("Pitch 30sec", Application.dataPath + "/Resources/Audio/440Hz_44100Hz_16bit_30sec.wav");
		AudioFiles.Add ("Waypoint Complete", Application.dataPath + "/Resources/Audio/131660__bertrof__game-sound-correct.wav");
		AudioFiles.Add ("click", Application.dataPath + "/Resources/Audio/click.mp3");
		AudioFiles.Add ("Click_Suit_Button", Application.dataPath + "/Resources/Audio/Click_Suit_Button.aif");
		AudioFiles.Add ("Colission", Application.dataPath + "/Resources/Audio/Colission.mp3");
		AudioFiles.Add ("Good for Goodie", Application.dataPath + "/Resources/Audio/Good for Goodie.wav");
		AudioFiles.Add ("PolyHover", Application.dataPath + "/Resources/Audio/PolyHover.wav");
		AudioFiles.Add ("pop_sound_effect", Application.dataPath + "/Resources/Audio/pop_sound_effect.mp3");
		AudioFiles.Add ("Speech_Off", Application.dataPath + "/Resources/Audio/Speech_Off.aif");
		AudioFiles.Add ("Speech_On", Application.dataPath + "/Resources/Audio/Speech_On.aif");
		AudioFiles.Add ("Waypoint", Application.dataPath + "/Resources/Audio/Waypoint.mp3");
		AudioFiles.Add ("Laser Fire", Application.dataPath + "/Resources/Audio/Energy_Laser_Fire.wav");
	}

	public string LookupSound(string name)
	{
		string path;
		if (AudioFiles.TryGetValue (name, out path)) {
			return path;
		} else {
			return Application.dataPath + "/Resources/Audio/pop_sound_effect.mp3";
		}
	}

}