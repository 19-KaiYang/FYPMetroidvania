using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "ProgressionData", menuName = "ProgressionData/New Data", order = 0)]
public class ProgressionData : ScriptableObject
{
    public List<string> rooms = new();
    public string EndingScene;
}
