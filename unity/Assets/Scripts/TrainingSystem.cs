using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EmotivUnityPlugin;
using System;
using TMPro;

public class TrainingSystem : MonoBehaviour
{
    private EmotivManager _emotivManager;
    public string mentalCommandResponse { get; set; }
    public bool IsTrainingInProgress { get; set; }

    public bool EEGToggle, MOTToggle, PMToggle, CQToggle, POWToggle, EQToggle, COMToggle, FEToggle, SYSToggle = true;
    public bool createSession, startRecord, subscribe, unsubscribe, loadProfile, unloadProfile, saveProfile, startMCTraining, acceptMCTraining, rejectMCTraining, eraseMCTraining, stopRecord, injectMarker;

    [Space(10)]
    [TextArea]
    public string messageLog;

    private EmotivUnityItf _eItf = EmotivUnityItf.Instance;
    private float _timerDataUpdate = 0;
    const float TIME_UPDATE_DATA = 1f;

    private string _profileName, _headsetId;
    [HideInInspector] public string selectedAction;
    [HideInInspector] public bool trainingActionCompleted = false;

    private bool _countdownActive = false;
    private float _countdownDuration = 8;

    private string logFilePath;

    void Start()
    {
        _emotivManager = GetComponent<TrainingManager>().emotivManager;
        Debug.Log($"{_emotivManager.clientId}, {_emotivManager.clientSecret}, {_emotivManager.appName}, {_emotivManager.appVersion}");

        _eItf.Init(_emotivManager.clientId, _emotivManager.clientSecret, _emotivManager.appName, _emotivManager.appVersion, false);
        _eItf.Start();

        logFilePath = Path.Combine(Application.persistentDataPath, "TrainingSystemLog.txt");
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }
    }

    void Update()
    {
        _timerDataUpdate += Time.deltaTime;
        if (_timerDataUpdate >= TIME_UPDATE_DATA)
        {
            _timerDataUpdate -= TIME_UPDATE_DATA;
            UpdateLogs();
            PerformActionsBasedOnToggles();

            if (_eItf.MentalCommandTrainingResponse.Contains("Succeeded") || _eItf.MentalCommandTrainingResponse.Contains("Failed"))
            {
                mentalCommandResponse = _eItf.MentalCommandTrainingResponse;
                Log("Mental command training response: " + mentalCommandResponse);
            }
        }

        _profileName = _emotivManager.profileName;
        _headsetId = _emotivManager.headsetId;

        if (_countdownActive)
        {
            Log("Seconds left: " + Mathf.Floor(_countdownDuration).ToString());
            _countdownDuration -= Time.deltaTime;

            if (_countdownDuration <= 0f)
            {
                _countdownActive = false;
                _countdownDuration = 8;
                Log("Countdown finished for " + selectedAction);
            }
        }
    }

    private void Log(string message)
    {
        File.AppendAllText(logFilePath, message + "\n");
        Debug.Log(message);
    }

    void PerformActionsBasedOnToggles()
    {
        if (createSession && !_eItf.IsSessionCreated)
        {
            Log("Creating session...");
            _eItf.CreateSessionWithHeadset(_emotivManager.headsetId);
            createSession = false;
            trainingActionCompleted = true;
            Log("Session created.");
        }

        if (loadProfile && !_eItf.IsProfileLoaded)
        {
            Log("Loading profile...");
            _eItf.LoadProfile(_emotivManager.profileName);
            loadProfile = false;
            trainingActionCompleted = true;
            Log("Profile loaded.");
        }

        if (unloadProfile && _eItf.IsProfileLoaded)
        {
            Log("Unloading profile...");
            _eItf.UnLoadProfile(_emotivManager.profileName);
            unloadProfile = false;
            trainingActionCompleted = true;
            Log("Profile unloaded.");
        }

        if (saveProfile && _eItf.IsProfileLoaded)
        {
            Log("Saving profile...");
            _eItf.SaveProfile(_emotivManager.profileName);
            saveProfile = false;
            trainingActionCompleted = true;
            Log("Profile saved.");
        }

        if (subscribe && _eItf.IsSessionCreated)
        {
            Log("Subscribing to data streams...");
            _eItf.SubscribeData(GetStreamsList());
            subscribe = false;
            trainingActionCompleted = true;
            Log("Subscribed to data streams.");
        }

        if (unsubscribe && _eItf.IsSessionCreated)
        {
            Log("Unsubscribing from data streams...");
            _eItf.UnSubscribeData(GetStreamsList());
            unsubscribe = false;
            trainingActionCompleted = true;
            Log("Unsubscribed from data streams.");
        }

        if (startMCTraining && _eItf.IsProfileLoaded)
        {
            Log("Starting mental command training for " + selectedAction);
            _countdownActive = true;
            _eItf.StartMCTraining(selectedAction);
            startMCTraining = false;
            trainingActionCompleted = true;
            Log("Mental command training started for " + selectedAction);
        }

        if (acceptMCTraining && _eItf.IsProfileLoaded)
        {
            Log("Accepting mental command training...");
            _eItf.AcceptMCTraining();
            acceptMCTraining = false;
            trainingActionCompleted = true;
            Log("Mental command training accepted.");
        }

        if (rejectMCTraining && _eItf.IsProfileLoaded)
        {
            Log("Rejecting mental command training...");
            _eItf.RejectMCTraining();
            rejectMCTraining = false;
            trainingActionCompleted = true;
            Log("Mental command training rejected.");
        }

        if (eraseMCTraining && _eItf.IsProfileLoaded)
        {
            Log("Erasing mental command training for " + selectedAction);
            _eItf.EraseMCTraining(selectedAction);
            eraseMCTraining = false;
            trainingActionCompleted = true;
            Log("Mental command training erased for " + selectedAction);
        }

        if (stopRecord && _eItf.IsRecording)
        {
            Log("Stopping recording...");
            _eItf.StopRecord();
            stopRecord = false;
            trainingActionCompleted = true;
            Log("Recording stopped.");
        }
    }

    public void CreateSession(Action onComplete)
    {
        string headsetId = _emotivManager.headsetId;
        Log("Creating session with headset ID: " + headsetId);
        _eItf.CreateSessionWithHeadset(headsetId);
        onComplete?.Invoke();
    }

    public void CreateProfile(Action onComplete)
    {
        Log("Creating profile...");
        if (!_eItf.IsProfileLoaded)
        {
            _eItf.LoadProfile(_emotivManager.profileName);
            if (!_eItf.IsProfileLoaded)
            {
                _eItf.SaveProfile(_emotivManager.profileName);
                Log("Profile created and saved.");
            }
        }
        onComplete?.Invoke();
    }

    public void LoadProfile(Action onComplete)
    {
        Log("Loading profile...");
        _eItf.LoadProfile(_emotivManager.profileName);
        Log("Profile loaded.");
        onComplete?.Invoke();
    }

    public void Subscribe(Action onComplete)
    {
        Log("Subscribing to data streams...");
        _eItf.SubscribeData(GetStreamsList());
        onComplete?.Invoke();
    }

    public void StartMCTraining(Action onComplete)
    {
        if (_eItf.IsProfileLoaded)
        {
            Log("Starting mental command training for " + selectedAction);
            _countdownActive = true;
            _eItf.StartMCTraining(selectedAction);
            onComplete?.Invoke();
        }
        else
        {
            Log("Profile not loaded. Cannot start training.");
        }
    }

    public void AcceptMCTraining(Action onComplete)
    {
        Log("Accepting mental command training...");
        _eItf.AcceptMCTraining();
        onComplete?.Invoke();
    }

    public void RejectMCTraining(Action onComplete)
    {
        Log("Rejecting mental command training...");
        _eItf.RejectMCTraining();
        onComplete?.Invoke();
    }

    public void EraseMCTraining(Action onComplete)
    {
        Log("Erasing mental command training for " + selectedAction);
        _eItf.EraseMCTraining(selectedAction);
        onComplete?.Invoke();
    }

    public void SaveProfile(Action onComplete)
    {
        Log("Saving profile...");
        _eItf.SaveProfile(_emotivManager.profileName);
        onComplete?.Invoke();
    }

    public bool IsProfileLoaded()
    {
        return _eItf.IsProfileLoaded;
    }

    public void EndSession()
    {
        Log("Ending session...");
        _eItf.Stop();
    }

    private List<string> GetStreamsList()
    {
        List<string> streams = new List<string>();
        if (EEGToggle) streams.Add("eeg");
        if (MOTToggle) streams.Add("mot");
        if (PMToggle) streams.Add("met");
        if (CQToggle) streams.Add("dev");
        if (POWToggle) streams.Add("pow");
        if (EQToggle) streams.Add("eq");
        if (COMToggle) streams.Add("com");
        if (FEToggle) streams.Add("fac");
        if (SYSToggle) streams.Add("sys");
        return streams;
    }

    void UpdateLogs()
    {
        if (_eItf.MessageLog.Contains("Get Error:"))
        {
            messageLog = "Error: " + _eItf.MessageLog;
        }
        else
        {
            messageLog = _eItf.MessageLog;
        }
    }

    void OnApplicationQuit()
    {
        _eItf.Stop();
    }
}
