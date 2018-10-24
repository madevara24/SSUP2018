using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleUIAR : MonoBehaviour {

    public GameObject objectUI, UIscore;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void Hidden()
    {
        objectUI.SetActive(false);
    }

    public void UnHidden()
    {
        objectUI.SetActive(true);
    }

    public void ScoreActive()
    {
        UIscore.SetActive(true);
        objectUI.SetActive(false);
    }
}
