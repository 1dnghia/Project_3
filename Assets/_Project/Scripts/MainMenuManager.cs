using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuManager : NMonoBehaviour
{
    private static MainMenuManager _instance;
    public static MainMenuManager Instance => _instance;

    [SerializeField] private GameObject _titlePanel;
    [SerializeField] private GameObject _stagePanel;
    [SerializeField] private GameObject _levelPanel;

    [SerializeField] private TMP_Text _levelTitleText;
    [SerializeField] private Image _levelTitleImage;

    [HideInInspector] public Color CurrentColor;

    public UnityAction LevelOpened;
    private SoundManager _soundManager;

    protected override void Awake()
    {
        _instance = this;
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();

        _titlePanel.SetActive(true);
        _stagePanel.SetActive(false);
        _levelPanel.SetActive(false);
    }

    public void ClickedPlay()
    {
        _soundManager.PlaySFX(_soundManager.connectClip);
        _titlePanel.SetActive(false);
        _stagePanel.SetActive(true);
    }
    public void ClickedBackToTitle()
    {
        _soundManager.PlaySFX(_soundManager.connectClip);
        _stagePanel.SetActive(false);
        _titlePanel.SetActive(true);
    }
    public void ClickedBackToStage()
    {
        _soundManager.PlaySFX(_soundManager.connectClip);
        _levelPanel.SetActive(false);
        _stagePanel.SetActive(true);
    }

    public void ClickedStage(string stageName, Color stageColor)
    {
        _stagePanel.SetActive(false);
        _levelPanel.SetActive(true);
        CurrentColor = stageColor;
        _levelTitleText.text = stageName;
        _levelTitleImage.color = CurrentColor;
        LevelOpened?.Invoke();
    }
}