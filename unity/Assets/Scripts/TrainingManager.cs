using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;

[RequireComponent(typeof(TrainingSystem))]
public class TrainingManager : MonoBehaviour
{
    [Header("Training Interface")]
    public EmotivManager emotivManager;
    private TrainingSystem _trainingSystem;
    [SerializeField] private int _rounds = 2;

    [Header("Training Trigger")]
    public bool startTraining = false;

    [Header("Mental Command Training Log")]
    [TextArea]
    public string mentalCommandsTrainingLog;

    private string logFilePath;

    private void Start()
    {
        MentalCommands mentalCommands = emotivManager.GetComponent<MentalCommands>();
        _trainingSystem = GetComponent<TrainingSystem>();
        logFilePath = Path.Combine(Application.persistentDataPath, "TrainingManagerLog.txt");
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }
    }

    void Update()
    {
        if (startTraining && !_trainingSystem.IsTrainingInProgress)
        {
            startTraining = false;
            StartCoroutine(StartTrainingLoop());
        }
    }

    public void SetStartTraining()
    {
        startTraining = true;
    }

    private void Log(string message)
    {
        File.AppendAllText(logFilePath, message + "\n");
        Debug.Log(message);
    }

    private IEnumerator StartTrainingLoop()
    {
        _trainingSystem.IsTrainingInProgress = true;

        // Separate training loops for neutral and pull
        yield return StartCoroutine(TrainAction("neutral"));
        // yield return StartCoroutine(TrainAction("pull"));

        Log("Training for all actions completed.");
        _trainingSystem.IsTrainingInProgress = false;
        _trainingSystem.EndSession();
        Log("Session ended.");
    }

    private IEnumerator TrainAction(string action)
    {
        int successfulAttempts = 0;
        while (successfulAttempts < _rounds)
        {
            Log($"Rounds left: {_rounds - successfulAttempts}");
            Log($"Training for action: {action}, Attempt: {successfulAttempts + 1}");
            _trainingSystem.selectedAction = action;
            _trainingSystem.mentalCommandResponse = "";

            yield return TrainingSequence(action);

            if (!_trainingSystem.mentalCommandResponse.Contains("Failed"))
            {
                successfulAttempts++;
            }
            else
            {
                Log($"Training attempt for {action} failed, retrying...");
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator TrainingSequence(string mentalCommand)
    {
        Log("Starting Training Round for " + mentalCommand);

        yield return StartCoroutine(CreateSession());
        yield return StartCoroutine(CreateProfile());
        yield return StartCoroutine(LoadProfile());

        // Ensure profile is loaded
        yield return new WaitUntil(() => _trainingSystem.IsProfileLoaded());

        yield return StartCoroutine(Subscribe());
        yield return StartCoroutine(StartMCTraining(mentalCommand));

        // Wait for training response
        yield return new WaitUntil(() => !string.IsNullOrEmpty(_trainingSystem.mentalCommandResponse));

        if (_trainingSystem.mentalCommandResponse.Contains("Succeeded"))
        {
            yield return StartCoroutine(AcceptMCTraining());
            yield return StartCoroutine(SaveProfile());
        }
        else
        {
            Log("Training failed for " + mentalCommand);
        }

        Log("Training Round Completed for " + mentalCommand);
    }

    private IEnumerator CreateSession()
    {
        bool completed = false;
        _trainingSystem.CreateSession(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Session created.");
    }

    private IEnumerator CreateProfile()
    {
        bool completed = false;
        _trainingSystem.CreateProfile(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Profile created.");
    }

    private IEnumerator LoadProfile()
    {
        bool completed = false;
        _trainingSystem.LoadProfile(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Profile loaded.");
    }

    private IEnumerator Subscribe()
    {
        bool completed = false;
        _trainingSystem.Subscribe(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Subscribed to data streams.");
    }

    private IEnumerator StartMCTraining(string mentalCommand)
    {
        bool completed = false;
        _trainingSystem.StartMCTraining(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Mental command training started for " + mentalCommand);
    }

    private IEnumerator AcceptMCTraining()
    {
        bool completed = false;
        _trainingSystem.AcceptMCTraining(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Mental command training accepted.");
    }

    private IEnumerator SaveProfile()
    {
        bool completed = false;
        _trainingSystem.SaveProfile(() => completed = true);
        yield return new WaitUntil(() => completed);
        Log("Profile saved.");
    }
}
