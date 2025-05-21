using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SO/Level")]
public class LevelData : ScriptableObject
{
   public string levelName;
   public List<Edge> Edgs;
}

[System.Serializable]
public struct Edge
{
   
}