using System;
using System.Collections.Generic;
using UnityEngine;
using EmotivUnityPlugin;
using TMPro;
using System.Collections;

/// <summary>
/// Handles setting up the EEG stream and getting the current mental command
/// </summary>
public class MentalCommands : MonoBehaviour
{
    public EmotivUnityItf _emotivUnityltf = new EmotivUnityItf();
    EmotivManager emotivManager;

    bool mentalCmdRcvd = false;
    string mentalCommand;
    List<string> dataStreamList = new List<string>() { DataStreamName.MentalCommands, DataStreamName.SysEvents };
    public string profileName = "";
    public string headsetId = "";
    private string lastMentalCommand = "";

    // Delegate for mental command changes
    public delegate void OnMentalCommandChanged(string newCommand);
    public event OnMentalCommandChanged MentalCommandChanged;

    public bool startEEGStream = false;

    void Start()
    {
        emotivManager = GameObject.Find("Emotiv Manager").GetComponent<EmotivManager>();
    }

    void Update()
    {
        profileName = emotivManager.profileName;
        headsetId = emotivManager.headsetId;

        if (startEEGStream)
        {
            _emotivUnityltf.Init(emotivManager.clientId, emotivManager.clientSecret, emotivManager.appName, emotivManager.appVersion, false);
            _emotivUnityltf.Start();
            startEEGStream = false;
            StartCoroutine(StartEEGStream());
        }

        if (mentalCmdRcvd)
        {
            string currentCommand = _emotivUnityltf.mentalCmdIs();
            Debug.Log($"Current Mental Command is: {currentCommand}");
            if (currentCommand != lastMentalCommand)
            {
                mentalCommand = currentCommand;
                lastMentalCommand = currentCommand;

                // Trigger the event
                MentalCommandChanged?.Invoke(mentalCommand);
            }
        }
    }

    public void CheckEEGStream()
    {
        startEEGStream = true;
    }

    private IEnumerator CreateSessionWithHeadsetCoroutine(string headsetId)
    {
        _emotivUnityltf.CreateSessionWithHeadset(headsetId);
        // Wait for 3 seconds after creating the session
        yield return new WaitForSeconds(0.5f);
        UnityEngine.Debug.Log("Session created with: " + headsetId);
    }

    private IEnumerator UnLoadProfileCoroutine(string profileName)
    {
        _emotivUnityltf.UnLoadProfile(profileName);
        // Wait for 3 seconds after loading the profile
        yield return new WaitForSeconds(0.5f);
        UnityEngine.Debug.Log("Profile unloaded: " + profileName);
    }


    private IEnumerator LoadProfileCoroutine(string profileName)
    {
        _emotivUnityltf.LoadProfile(profileName);
        // Wait for 3 seconds after loading the profile
        yield return new WaitForSeconds(0.5f);
        UnityEngine.Debug.Log("Profile loaded: " + profileName);
    }

    private IEnumerator SubscribeDataCoroutine(List<string> dataStreamList)
    {
        _emotivUnityltf.SubscribeData(dataStreamList);
        // Assuming a different wait time is not specifically required here;
        // otherwise, adjust the duration as needed.
        yield return new WaitForSeconds(0.5f);
        UnityEngine.Debug.Log("Subscribed to DataStream");
    }

    private IEnumerator StartEEGStream()
    {
        /// Establish connection with Emotiv EPOC X or Insight II with correct headsetId
        yield return StartCoroutine(CreateSessionWithHeadsetCoroutine(headsetId));

        yield return StartCoroutine(UnLoadProfileCoroutine(profileName));

        /// Load profile from EmotivBCI with same training profile name as profileName
        yield return StartCoroutine(LoadProfileCoroutine(profileName));

        /// Subscribe to datastream from EmotivBCI to get mental commands from Insight II
        yield return StartCoroutine(SubscribeDataCoroutine(dataStreamList));
        mentalCmdRcvd = true;
    }


    public string GetMentalCommand()
    {
        return mentalCommand;
    }

    private void OnDestroy()
    {
        _emotivUnityltf.UnLoadProfile(profileName);
        _emotivUnityltf.UnSubscribeData(dataStreamList);
        _emotivUnityltf.Stop();
    }

    /// <summary>
    /// End session with EEG headset when game closes
    /// </summary>
    void OnApplicationQuit()
    {
        _emotivUnityltf.UnLoadProfile(profileName);
        _emotivUnityltf.UnSubscribeData(dataStreamList);
        _emotivUnityltf.Stop();
    }
}
