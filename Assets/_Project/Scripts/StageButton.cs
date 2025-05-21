using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

    public class StageButton : NMonoBehaviour
    {
        [SerializeField] protected string _stageName;
        [SerializeField] protected Color _stageColor;
        [SerializeField] protected int _stageNumber;
        [SerializeField] protected Button _stageButton;

        protected override void Awake()
        {
            _stageButton.onClick.AddListener(ClickedButton);
        }

        protected void ClickedButton()
        {
            GameManager.Instance.CurrentStage = _stageNumber;
            GameManager.Instance.StageName = _stageName;
            //
    }
    }
