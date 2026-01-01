using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AudioSource))]

public class AudioClipDisplay : MonoBehaviour
{
    [SerializeField] int HorizontalMargin = 5;
    [SerializeField] int ChannelSpacing = 5;

    RawImage LinkedImage;
    AudioSource LinkedSource;

    Texture2D DisplayTexture;
    int ImageWidth;
    int ImageHeight;

    void Awake()
    {
        LinkedImage = GetComponent<RawImage>();
        LinkedSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rect ImageDimensions = RectTransformUtility.PixelAdjustRect(LinkedImage.rectTransform, LinkedImage.canvas);
        ImageWidth = Mathf.CeilToInt(ImageDimensions.width);
        ImageHeight = Mathf.CeilToInt(ImageDimensions.height);

        DisplayTexture = new Texture2D(ImageWidth, ImageHeight, TextureFormat.ARGB32, false);
        DisplayTexture.wrapMode = TextureWrapMode.Clamp;
        LinkedImage.texture = DisplayTexture;
    }

    public void OnSetNewAudioClip(AudioClip InNewClip)
    {
        //clear the image
        var ImagePixels = new Color[ImageWidth * ImageHeight];
        for (int PixelIndex = 0; PixelIndex < ImagePixels.Length; PixelIndex++)
        {
            ImagePixels[PixelIndex] = Color.white;
        }
        DisplayTexture.SetPixels(ImagePixels);

        int Size = InNewClip.samples * InNewClip.channels;
        float[] ClipData = new float[Size];

        //attempt to retrieve the clip data
        if (!InNewClip.GetData(ClipData, 0))
        {
            return;
        }

        //figure out current UI heights
        int TotalVerticalPadding = ChannelSpacing * (InNewClip.channels + 1);
        int HeightPerChannel = (ImageHeight - TotalVerticalPadding) / InNewClip.channels;

        if (HeightPerChannel <= 0)
        {
            return;
        }

        int WorkingWidth = ImageWidth - (2 * HorizontalMargin);
        int SamplesPerPixel = InNewClip.samples / WorkingWidth;

        if (SamplesPerPixel == 0)
        {
            return;
        }

        //figure out the bounds 
        float MinValue = float.MaxValue;
        float MaxValue = float.MinValue;
        foreach(var Sample in ClipData)
        {
            float WorkingSample = Mathf.Abs(Sample);

            if (WorkingSample < MinValue)
            {
                MinValue = WorkingSample;
            }
            if (WorkingSample > MaxValue)
            {
                MaxValue = WorkingSample;
            }
        }

        //process by channel
        for (int ChannelIndex = 0; ChannelIndex < InNewClip.channels; ChannelIndex++)
        {
            int ChannelYOffset = ChannelSpacing + (ChannelIndex * (ChannelSpacing + HeightPerChannel));

            //loop through the horizontal axes
            for (int GraphX = 0; GraphX < WorkingWidth; GraphX++)
            {
                float LocalSampleSum = 0;
                int LocalSampleCount = 0;

                //loop through the samples for this pixel
                for (int LocalSampleIndex = 0; LocalSampleIndex < SamplesPerPixel; LocalSampleIndex++)
                {
                    int SampleIndex = LocalSampleIndex + (GraphX * SamplesPerPixel);
                    int DataIndex = ChannelIndex + (SampleIndex * InNewClip.channels);

                    if (DataIndex >= ClipData.Length)
                    {
                        break;
                    }

                    LocalSampleSum += Mathf.Abs(ClipData[DataIndex]);
                    ++LocalSampleCount;
                }

                if (LocalSampleCount == 0)
                {
                    continue;
                }

                float AveragedValue = LocalSampleSum / LocalSampleCount;
                float NormalisedValue = Mathf.InverseLerp(MinValue, MaxValue, AveragedValue);

                int YEnd = ChannelYOffset + Mathf.RoundToInt(NormalisedValue * HeightPerChannel);

                for (int YPos = ChannelYOffset; YPos <= YEnd; ++YPos)
                {
                    DisplayTexture.SetPixel(HorizontalMargin + GraphX, YPos, Color.black);
                }
            }
        }

        DisplayTexture.Apply();

        LinkedSource.clip = InNewClip;
        LinkedSource.Play();
    }
}
