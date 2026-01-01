using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioRecorder : MonoBehaviour
{
    [SerializeField] int MaxDuration = 30;

    [SerializeField] UnityEvent<AudioClip> OnRecordingReady = new();

    public bool IsRecording { get; private set; } = false;
    AudioClip CurrentRecording;
    string CurrentDevice;

    public bool StartRecording(string InDevice, int InSampleRate)
    {
        if (IsRecording)
        {
            return false;
        }

        CurrentRecording = Microphone.Start(InDevice, false, MaxDuration, InSampleRate);
        if (CurrentRecording == null)
        {
            return false;
        }

        CurrentDevice = InDevice;
        IsRecording = true;

        return true;
    }
    
    public bool StopRecording()
    {
        if (!IsRecording)
        {
            return false;
        }

        if ((CurrentRecording == null) || string.IsNullOrEmpty(CurrentDevice))
        {
            return false;
        }

        Microphone.End(CurrentDevice);
        IsRecording = false;
        OnRecordingReady.Invoke(CurrentRecording);
        return true;
    }
}
