using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : NMonoBehaviour
{
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Button _button;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] private Image _image;

    private bool isLevelUnlocked;
    private int currentLevel;
    private SoundManager _soundManager;

    protected override void Awake()
    {
        _button.onClick.AddListener(Clicked);
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

    protected override void OnEnable()
    {
        MainMenuManager.Instance.LevelOpened += LevelOpened;
    }

    protected override void OnDisable()
    {
        MainMenuManager.Instance.LevelOpened -= LevelOpened;
    }

    protected void LevelOpened() //đặt màu sắc, mở khoá level và tự động đặt các cấp level trong unity
    {
        string gameObjectName = gameObject.name;
        string[] parts = gameObjectName.Split('_');
        _levelText.text = parts[parts.Length - 1];
        currentLevel = int.Parse(_levelText.text);
        isLevelUnlocked = GameManager.Instance.IsLevelUnlocked(currentLevel);

        if (isLevelUnlocked)
        {
            _image.color = MainMenuManager.Instance.CurrentColor;
        }
        else
        {
            _image.color = _inactiveColor;
        }
    }

    public void Clicked()
    {
        if (!isLevelUnlocked)
            return;

        _soundManager.PlaySFX(_soundManager.connectClip);
        GameManager.Instance.CurrentLevel = currentLevel;
        GameManager.Instance.GoToGameplay();
    }
}
