using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelList", menuName = "SO/LevelList")]
public class LevelList : ScriptableObject
{
    public List<LevelData> Levels;
}
