using UnityEngine;
using System;
using System.Diagnostics;

public class BeatDetection : MonoBehaviour
{
    //The audisource playing
    AudioSource m_AudioSource;

    //Stores the stream of left and right samples as they are fed in.
    private float[] m_samplesLeft;
    private float[] m_samplesRight;

    public float sensitivity;

    //The amount of samples being taken each update frame.
    public int samplesTaken;

    //The last average energy readings. Size will be 44032 / samplesTaken as this is the closest you can get to 1 second of audio.
    private float[] localHistory;

    //TESTING
    public float m_instantEnergy;
    public float m_averageLocalEnergy;

    private bool wasLastBeat;
    public static bool beat;

    // Use this for initialization
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_samplesLeft = new float[samplesTaken];
        m_samplesRight = new float[samplesTaken];
        localHistory = new float[43]; //Dictated by 44032 / 1024 for 1 second of audio.
        wasLastBeat = false;
        beat = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumData();
        m_instantEnergy = CalculateInstantEnergy(); //Calculate the instant energy based on 1024 samples.
        m_averageLocalEnergy = CalculateAverageLocalEnergy(); //Calculate the local average energy based on 43 instant energy readings.
        localHistory = ShiftHistory(); //Shift the history buffer up one to make room for new values.
        localHistory[0] = m_instantEnergy; //Add the instant energy average to the history.

        if (m_instantEnergy > m_averageLocalEnergy * sensitivity)
        {
            beat = true;
        }
        else { beat = false; }
    }

    void GetSpectrumData()
    {
        //Could make the FFT window a public variable for easier comparison.
        m_AudioSource.GetSpectrumData(m_samplesLeft, 0, FFTWindow.BlackmanHarris);
        m_AudioSource.GetSpectrumData(m_samplesRight, 1, FFTWindow.BlackmanHarris);
    }

    float CalculateInstantEnergy()
    {
        float instantEnergy = 0f;

        for (int i = 0; i < samplesTaken; i++)
        {
            instantEnergy += (float)Math.Pow(m_samplesLeft[i], 2) + (float)Math.Pow(m_samplesRight[i], 2);
        }

        return instantEnergy;
    }

    float CalculateAverageLocalEnergy()
    {
        float averageLocalEnergy = 0f;

        for (int i = 0; i < localHistory.Length; i++)
        {
            averageLocalEnergy += (float)Math.Pow(localHistory[i], 2) / 43;
        }

        return averageLocalEnergy;
    }

    /// <summary>
    /// Shifts the history data to the right by one place to make room for new values.
    /// </summary>
    /// <returns>'result' which is the new array with the values shifted.</returns>
    float[] ShiftHistory()
    {
        float[] result = new float[localHistory.Length];

        for (int i = 1; i < localHistory.Length; i++)
        {
            if (i != localHistory.Length)
            {
                result[i] = localHistory[i - 1];
            }
        }

        return result;
    }
  
    /// <summary>
    /// Not currently used but could be useful in the future.
    /// </summary>
    /// <returns>true if a beat is detected, false otherwise</returns>
    public bool IsBeat()
    {
        if (m_instantEnergy > m_averageLocalEnergy * sensitivity)
        {
            if (!wasLastBeat)
            {
                UnityEngine.Debug.Log("BEAT DETECTED!");
                wasLastBeat = true;              
            }
            return true;              
        }
        wasLastBeat = false;
        return false;
    }
}
