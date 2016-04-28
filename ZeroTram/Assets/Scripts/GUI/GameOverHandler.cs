﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverHandler : MonoBehaviour, GameStateNotificationListener
{
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private Text _reasonText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Text _countText;
    [SerializeField] private Text _captionText;
    [SerializeField] private GameObject _stickCaption;
	[SerializeField] private Client _client;

    private const string DeathReason = "Кондуктор погиб";
    private const string HareReason = "Слишком много зайцев";
    private const string KilledPassengersReason = "Слишком много погибших";
    private const string VictoryReason = "Вы достигли следующей станции!";

    private const int ZeroCount = 6;

    private GameController.StateInformation _stateInfo;

    void Awake()
    {
        _restartButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("main");
        });
        _exitButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
			SceneManager.LoadScene("MainMenu");
        });
        GameController.GetInstance().AddListener(this);
    }

    void OnDestroy()
    {
        GameController.GetInstance().RemoveListener(this);
    }

    public void UpdateInfo(GameController.StateInformation information)
    {
        _stateInfo = information;
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        _stickCaption.SetActive(false);
        if (_stateInfo.IsLevelFinished)
        {
            _reasonText.text = VictoryReason;
            _captionText.text = "победа!";
            _restartButton.onClick.RemoveAllListeners();
            _exitButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1;
                string nextStationId = MapManager.GetInstance().GetLastOpenedStationId();
                if (!MapManager.GetInstance().IsNewWorldAnimationNeedToBePlayed())
                {
                    MapManager.GetInstance().SetCurrentStation(nextStationId);
					SceneManager.LoadScene("main");
                }
                else
                {
					SceneManager.LoadScene("Map");
                }
            });
            _exitButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1;
				SceneManager.LoadScene("Map");
            });
        }
        else
        {
            if (_stateInfo.Hares > GameController.GetInstance().MaxHaresPercent)
                _reasonText.text = HareReason;
            if (_stateInfo.RemainKilled < 0)
                _reasonText.text = KilledPassengersReason;
            if (_stateInfo.IsConductorDied)
                _reasonText.text = DeathReason;
        }
        
        int leadingZeroCount = ZeroCount - _stateInfo.TicketCount.ToString().Length;
        string countText = string.Empty;
        for (int i = 0; i < leadingZeroCount; i++)
        {
            countText += "0";
        }
        countText += _stateInfo.TicketCount;
        countText = countText.Insert(3, " ");
        _countText.text = countText;
        gameOverMenu.SetActive(true);
		_client.DecreaseTramLives ((response) => {
			Debug.Log (response);
			if(_stateInfo.TicketCount > 0) {
				RecordsManager.GetInstance().AddRecord(_stateInfo.TicketCount);
				_client.SaveRecord (_stateInfo.TicketCount, (result) => {
					Debug.Log (result);
				});
			}
		});
    }
}
