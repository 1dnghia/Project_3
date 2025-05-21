using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class GameManager : NMonoBehaviour
    {
        [HideInInspector] public int CurrentStage;
        [HideInInspector] public int CurrentLevel;
        [HideInInspector] public string StageName;
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

        protected void Init()
        {
            CurrentStage = 1;
            CurrentLevel = 1;

                        
        }
    }
