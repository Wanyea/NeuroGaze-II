using UnityEngine;

/// <summary>
/// Manages the Emotiv device and what headset is connected
/// </summary>
public class EmotivManager : MonoBehaviour
{
    public static EmotivManager Instance { get; private set; }

    [Header("Emotiv Settings")]
    public string clientId = "n5oqW9xORKP6d0QDA5xoI8d6PERLkDvcTz2p8QYZ";
    public string clientSecret = "ycBwS8rOI1zTJFCZB5VZzj85AqGAMXxRYK4hBQjkQ8BIPD6Wh6qgSbtEbJpcN0mcQSpgMMaLrLIK1ck9oXWm20qlXj15qYVfu4IzL6sLrMfu44c2ndAiorluHg9vJDXI";
    public string appName = "NeuroGazeII";
    public string appVersion = "3.3.0";

    [Header("Profile Information")]
    [HideInInspector] public string[] headsetNames;
    [SerializeField] public int selectedHeadsetIndex = 0;
    [HideInInspector] public string headsetId;
    public string profileName = "wanyea";

    private void Start()
    {
        headsetNames = new string[]
        {
            "EPOCX-3357EFDD",
            "INSIGHT2-A068A3B7",
            "EPOCX-E50208B2",
            "EPOCX-FF138AB1",
            "EPOCX-16EF2451",
            "EPOCX-BFDB334A",
            "INSIGHT2-A3D20368",
            "INSIGHT2-DC3B479A",
            "INSIGHT2-EC3D8746",
            "INSIGHT2-A3D2036F"

        };
    }

    private void Update()
    {
        headsetId = headsetNames[selectedHeadsetIndex];
    }
}

