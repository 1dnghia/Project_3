using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NMonoBehaviour
{
    [HideInInspector] public int CurrentStage;
    [HideInInspector] public int CurrentLevel;
    [HideInInspector] public string StageName;

    [SerializeField] private LevelData DefaultLevel;
    [SerializeField] private LevelList _allLevels;

    private Dictionary<string, LevelData> Levels;

    private const string MainMenu = "MainMenu";
    private const string Gameplay = "Gameplay";
    public static GameManager Instance => _instance;
    private static GameManager _instance;

    protected override void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            Init();
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected void Init() //khoi tao
    {
        CurrentStage = 1;
        CurrentLevel = 1;

        Levels = new Dictionary<string, LevelData>();// thêm hết tên level từ _allLevels.Levels vào thư viện Levels
        foreach (var item in _allLevels.Levels)
        {
            Levels[item.LevelName] = item;
        }
    }

    public bool IsLevelUnlocked(int level)// kiểm tra xem một level được mở khóa chưa
    {
        string levelName = "Level" + CurrentStage.ToString() + level.ToString();
        if (level == 1)
        {
            PlayerPrefs.SetInt(levelName, 1);
            return true;
        }

        if (PlayerPrefs.HasKey(levelName))
        {
            return PlayerPrefs.GetInt(levelName) == 1;
        }
        PlayerPrefs.SetInt(levelName, 0);
        return false;
    }

    public void UnlockLevel()//mở khoá level tiếp theo
    {
        CurrentLevel++;
        if (CurrentLevel == 51)
        {
            CurrentLevel = 1;
            CurrentStage++;

            if (CurrentStage == 8)
            {
                CurrentStage = 1;
                GoToMainMenu();
            }
        }

        string levelName = "Level" + CurrentStage.ToString() + CurrentLevel.ToString();
        PlayerPrefs.SetInt(levelName, 1);
    }

    public LevelData GetLevel() //lấy dữ liệu level hiện tại
    {
        string levelName = "Level" + CurrentStage.ToString() + CurrentLevel.ToString();
        // Nếu là level 1, ưu tiên lấy đúng file LevelX1
        if (CurrentLevel == 1)
        {
            if (Levels.ContainsKey(levelName))
            {
                return Levels[levelName];
            }
        }
        // Các level khác giữ nguyên
        if (Levels.ContainsKey(levelName))
        {
            return Levels[levelName];
        }
        return DefaultLevel;
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenu);
    }

    public void GoToGameplay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(Gameplay);
    }
}