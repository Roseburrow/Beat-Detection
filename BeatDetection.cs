using UnityEngine;
using System;
using System.Diagnostics;

[RequireComponent(typeof(AudioSource))]
public class BeatDetection : MonoBehaviour
{
    //The audisource playing
    AudioSource m_AudioSource;

    //Stores the stream of left and right samples as they are fed in.
    private float[] m_samplesLeft;
    private float[] m_samplesRight;

    //The last average energy readings. Size will be 44032 / samplesTaken as this is the closest you can get to 1 second of audio.
    private float[] localHistory;

    //Determines what change in amplitude dictates a beat. Variance is used in this calculation.
    public float m_Sensitivity;
    public float m_EnergyVariance;

    //Values required for detecting if there is a beat or not.
    //Don't need to be public.
    public float m_InstantEnergy;
    public float m_AverageLocalEnergy;
    public float averagemodifiedEnergy;

    //The amount of samples being taken each update frame.
    public int samplesTaken;     

    //True if a beat is detected, false otherwise.
    public static bool m_Beat;

    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_samplesLeft = new float[samplesTaken];
        m_samplesRight = new float[samplesTaken];
        localHistory = new float[43]; //Dictated by 44032 / 1024 for 1 second of audio.
        m_Beat = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumData(); //Populate sample arrays.
        m_InstantEnergy = CalculateInstantEnergy(); //Calculate the instant energy based on sample arrays.
        m_AverageLocalEnergy = CalculateAverageLocalEnergy(); //Calculate the local average energy based on 43 instant energy readings.
        m_EnergyVariance = CalculateEnergyVariance();
        m_Sensitivity = CalculateSensitivity();

        localHistory = ShiftHistory(); //Shift the history buffer up one to make room for new values.
        localHistory[0] = m_InstantEnergy; //Add the instant energy average to the history.

        //This is only used for testing.
        averagemodifiedEnergy = m_AverageLocalEnergy * m_Sensitivity;

        m_Beat = IsBeat();
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

    float CalculateEnergyVariance()
    {
        float variance = 0f;

        for (int i = 0; i < localHistory.Length; i++)
        {
            variance += (float)Math.Pow(localHistory[i] - m_AverageLocalEnergy, 2);
        }
        return variance / localHistory.Length;
    }

    float CalculateSensitivity()
    {
        float S = 0f;
        S = (0.0025714f * m_EnergyVariance) + 1.5142857f; //Experiment with these numbers.
        return S;
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
        if (m_InstantEnergy > m_AverageLocalEnergy * m_Sensitivity)
        {
            return true;
        }
        else { return false; }
    }
}
