// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;
using HoloToolkit.Unity;
using System;
using HoloToolkit.Unity.InputModule;

public class TextToSpeechOnDiscover : MonoBehaviour
{
	public TextToSpeechManager textToSpeechManager;

	// Use this for initialization
	void Start ()
	{
	}

	/// <summary>
	/// Send caption from cognitive API to text to speech manager
	/// </summary>
	/// <param name="caption">Caption.</param>
	public void SayCaption(string caption)
	{
		// Try and get a TTS Manager
		TextToSpeechManager tts = GetComponent<TextToSpeechManager>();

		// If we have a text to speech manager on the target object, say something.
		// This voice will appear to emanate from the object.
		if (tts != null && !tts.IsSpeaking())
		{
			// Get the name
			var voiceName = Enum.GetName(typeof(TextToSpeechVoice), tts.Voice);

			// Create message
			var msg = string.Format(caption, voiceName);
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
}
