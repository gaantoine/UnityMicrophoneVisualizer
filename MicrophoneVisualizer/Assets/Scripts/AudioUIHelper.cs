using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AudioUIHelper : MonoBehaviour
{

    [SerializeField] Button StartStopButton;
    [SerializeField] TextMeshProUGUI StartStopButtonText;
    [SerializeField] TMP_Dropdown SourcePicker;
    [SerializeField] TMP_Dropdown SampleRatePicker;
    [SerializeField] List<int> CandidateSampleRates;

    [SerializeField] AudioRecorder LinkedRecorder;

    List<string> KnownDevices = new();
    Dictionary<string, List<string>> SampleRateLookUp = new();

    string SelectedDevice => KnownDevices[SourcePicker.value];
    int SelectedSampleRate => int.Parse(SampleRateLookUp[SelectedDevice][SampleRatePicker.value]);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        KnownDevices.AddRange(Microphone.devices);

        //no microphones present
        if (KnownDevices.Count == 0)
        {
            StartStopButton.enabled = false;
            SourcePicker.enabled = false;
            SampleRatePicker.enabled = false;

            return;
        }

        //build up device info
        foreach (var DeviceName in KnownDevices)
        {
            int MinFrequency;
            int MaxFrequency;

            Microphone.GetDeviceCaps(DeviceName, out MinFrequency, out MaxFrequency);

            List<int> SampleRates = new();
            SampleRates.Add(MinFrequency);

            //add in candidates after checking validity
            foreach (var CandidateRate in CandidateSampleRates)
            {
                if (CandidateRate < MinFrequency || CandidateRate > MaxFrequency)
                {
                    continue;
                }

                if (SampleRates.Contains(CandidateRate))
                {
                    continue;
                }

                SampleRates.Add(CandidateRate);
            }

            if (!SampleRates.Contains(MaxFrequency))
            {
                SampleRates.Add(MaxFrequency);
            }

            SampleRates.Sort();

            //build string list version of the rates
            List<string> SampleRateStrings = new();
            foreach (var SampleRate in SampleRates)
            {
                SampleRateStrings.Add(SampleRate.ToString());
            }

            SampleRateLookUp[DeviceName] = SampleRateStrings;
        }

        //update the UI
        if (SampleRateLookUp[KnownDevices[0]].Count == 0)
        {
            StartStopButton.enabled = false;
            SampleRatePicker.enabled = false;

            return;
        }

        SourcePicker.ClearOptions();
        SourcePicker.AddOptions(KnownDevices);
        SourcePicker.SetValueWithoutNotify(0);

        SampleRatePicker.ClearOptions();
        SampleRatePicker.AddOptions(SampleRateLookUp[KnownDevices[0]]);
        SampleRatePicker.SetValueWithoutNotify(0);
    }

    public void OnSourceSelected(int InIndex)
    {
        SampleRatePicker.ClearOptions();

        if (SampleRateLookUp[KnownDevices[InIndex]].Count == 0)
        {
            StartStopButton.enabled = false;
            SampleRatePicker.enabled = false;
        }
        else
        {
            SampleRatePicker.AddOptions(SampleRateLookUp[KnownDevices[InIndex]]);
            SampleRatePicker.SetValueWithoutNotify(0);

            StartStopButton.enabled = true;
            SampleRatePicker.enabled = true;
        }
    }

    public void OnStartStopButtonClicked()
    {
        if (LinkedRecorder.IsRecording)
        {
            LinkedRecorder.StopRecording();

            StartStopButtonText.text = "Start Recording";
            SourcePicker.enabled = true;
            SampleRatePicker.enabled = true;
        }
        else
        {
            LinkedRecorder.StartRecording(SelectedDevice, SelectedSampleRate);

            StartStopButtonText.text = "Stop Recording";
            SourcePicker.enabled = false;
            SampleRatePicker.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
