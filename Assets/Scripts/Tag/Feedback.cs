using Academy.HoloToolkit.Unity;
using HoloToolkit.Unity;
using System.Collections;
using UnityEngine;

/// <summary>
/// This keeps track of the various parts of the recording and text display process.
/// </summary>

namespace Tagging
{
    [RequireComponent(typeof(AudioSource), typeof(MicrophoneManager), typeof(KeywordManager))]
    public class Feedback : MonoBehaviour
    {
        [Tooltip("The button to be selected when the user wants to record audio and dictation.")]
        public Button RecordButton;
        [Tooltip("The button to be selected when the user wants to stop recording.")]
        public Button RecordStopButton;
        [Tooltip("The button to be selected when the user wants to play audio.")]
        public Button PlayButton;
        [Tooltip("The button to be selected when the user wants to stop playing.")]
        public Button PlayStopButton;
        [Tooltip("The button to be selected when the user wants to reset the text")]
        public Button EraseTextButton;

        [Tooltip("The sound to be played when the recording session starts.")]
        public AudioClip StartListeningSound;
        [Tooltip("The sound to be played when the recording session ends.")]
        public AudioClip StopListeningSound;

        [Tooltip("The icon to be displayed while recording is happening.")]
        public GameObject MicIcon;

        //[Tooltip("A message to help the user understand what to do next.")]
        //public Renderer MessageUIRenderer;

        //[Tooltip("The waveform animation to be played while the microphone is recording.")]
        //public Transform Waveform;
        //[Tooltip("The meter animation to be played while the microphone is recording.")]
        //public MovieTexturePlayer SoundMeter;

        private AudioSource startAudio;
        private AudioSource stopAudio;
        private float startTime;
        private bool recording;

        public enum Message
        {
            PressMic,
            PressStop,
            SendMessage
        };

        private MicrophoneManager microphoneManager;
        private TagManager tagManager;

        void Start()
        {
            startTime = Time.time;
            recording = false;
            startAudio = gameObject.AddComponent<AudioSource>();
            stopAudio = gameObject.AddComponent<AudioSource>();

            startAudio.playOnAwake = false;
            startAudio.clip = StartListeningSound;
            stopAudio.playOnAwake = false;
            stopAudio.clip = StopListeningSound;

            microphoneManager = GetComponent<MicrophoneManager>();
            tagManager = TagManager.Instance;
        }

        private void Update()
        {
            // Timeout for tagging after 10 seconds
            if (Time.time - startTime > 10 && !recording)
            {
                tagManager.DestroyDiction();
            }
        }

        public void Record()
        {
            if (RecordButton.IsOn())
            {
                recording = true;

                microphoneManager.EraseText();

                // Turn the microphone on, which returns the recorded audio.
                microphoneManager.StartRecording();

                // Set proper UI state and play a sound.
                SetUI(true, Message.PressStop, startAudio);

                //Set button states
                PlayButton.SetActive(false);
                RecordButton.gameObject.SetActive(false);
                RecordStopButton.gameObject.SetActive(true);
                RecordStopButton.SetActive(true);
                EraseTextButton.gameObject.SetActive(true);
            }
        }

        public void RecordStop()
        {
            if (RecordStopButton.IsOn())
            {
                // Turn off the microphone.
                microphoneManager.StopRecording();
                // Restart the PhraseRecognitionSystem and KeywordRecognizer
                microphoneManager.StartCoroutine("RestartSpeechSystem");

                // Set proper UI state and play a sound.
                SetUI(false, Message.SendMessage, stopAudio);

                //Set button states
                PlayButton.SetActive(true);
                RecordStopButton.SetActive(false);
                RecordStopButton.gameObject.SetActive(false);
                RecordButton.SetActive(true);
                RecordButton.gameObject.SetActive(true);
            }
        }

        public void Play()
        {
            if (PlayButton.IsOn())
            {
                if (tagManager.CreateNewTag)
                {
                    tagManager.CreateTag(microphoneManager.DictationDisplay.text);
                }
                else
                {
                    tagManager.NavigateToTag(microphoneManager.DictationDisplay.text);
                }
            }
        }

        public void PlayStop()
        {

        }

        public void ResetText()
        {
            if (EraseTextButton.IsOn())
            {
                microphoneManager.EraseText();
            }
        }

        void ResetAfterTimeout()
        {
            // Set proper UI state and play a sound.
            SetUI(false, Message.PressMic, stopAudio);

            RecordStopButton.gameObject.SetActive(false);
            RecordButton.gameObject.SetActive(true);
        }

        private void SetUI(bool enabled, Message newMessage, AudioSource soundToPlay)
        {
            MicIcon.SetActive(enabled);
            soundToPlay.volume = .75f;
            soundToPlay.Play();
        }
    }
}