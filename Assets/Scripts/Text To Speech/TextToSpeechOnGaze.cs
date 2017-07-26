// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;
using HoloToolkit.Unity;
using System;
using HoloToolkit.Unity.InputModule;

public class TextToSpeechOnGaze : MonoBehaviour, IInputClickHandler
{
	public TextToSpeechManager textToSpeechManager;

    bool silentMode;

	// Use this for initialization
	void Start ()
	{
        silentMode = false;
	}

	public void GazeEntered()
	{
        if (silentMode)
        { return; }

		// Try and get a TTS Manager
		TextToSpeechManager tts = GetComponent<TextToSpeechManager>();

		// If we have a text to speech manager on the target object, say something.
		// This voice will appear to emanate from the object.
		if (tts != null && !tts.IsSpeaking())
		{
			// Get the name
			var voiceName = Enum.GetName(typeof(TextToSpeechVoice), tts.Voice);

			// Create message
			GameObject tagName = transform.FindChild("Tag Name").gameObject;
			var msg = string.Format(tagName.GetComponent<TextMesh> ().text, voiceName);
			//var msg = string.Format("This is the {0} voice. It should sound like it's coming from the object you clicked. Feel free to walk around and listen from different angles.", voiceName);

			// Speak message
			tts.SpeakText(msg);
		}
		else if (tts.IsSpeaking())
		{
			//tts.StopSpeaking();
			return;
		}
	}

    public void OnInputClicked(InputClickedEventData eventData)
    {
        silentMode = !silentMode;
    }
}
