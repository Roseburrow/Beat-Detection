﻿using UnityEngine;

public class ChangeColour : MonoBehaviour {

    Material m_Material;
    Color m_BeatColour;
    public float smoothnessChange;

	// Use this for initialization
	void Start () 
	{
        m_Material = GetComponent<MeshRenderer>().materials[0];
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (BeatDetection.m_Beat)
        {
            m_BeatColour = Color.green;
            m_Material.SetColor("_EmissionColor", m_BeatColour);
        }
        else
        {
            m_BeatColour = Color.Lerp(m_BeatColour, Color.black, smoothnessChange * Time.deltaTime); ;
            m_Material.SetColor("_EmissionColor", m_BeatColour);
        }
	}

    private void LateUpdate()
    {
        
    }
}
