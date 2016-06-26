﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Assets.Scripts.Client;

public class GameOverHandler : MonoBehaviour, GameStateNotificationListener
{
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private Text _reasonText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Text _countText;
    [SerializeField] private Text _captionText;
	[SerializeField] private Client _client;
    [SerializeField] private TrainingHandler _trainingHandler;
    [SerializeField] private ConductorSM _hero;
    [SerializeField] private MyMessageScript _messages;

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
        if (_stateInfo.IsLevelFinished)
        {
            _reasonText.text = StringResources.GetLocalizedString("GameOverVictory");
            _captionText.text = StringResources.GetLocalizedString("GameOverVictoryCaption");
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
            {
                if (TrainingHandler.IsTrainingFinished())
                {
                    _reasonText.text = StringResources.GetLocalizedString("GameOverHare");
                }
                else
                {
                    GameController.GetInstance().ResetHarePercent();
                    _trainingHandler.TrainingFailHare();
                    gameOverMenu.gameObject.SetActive(false);
                    return;
                }
            }
            if (_stateInfo.RemainKilled < 0)
            {
                if (TrainingHandler.IsTrainingFinished())
                {
                    _reasonText.text = StringResources.GetLocalizedString("GameOverKilledPassengers");
                }
                else
                {
                    GameController.GetInstance().ResetDiedPassengersPercent();
                    _trainingHandler.TrainingFailPassengers();
                    gameOverMenu.gameObject.SetActive(false);
                    return;
                }
            }
            if (_stateInfo.IsConductorDied)
            {
                if (TrainingHandler.IsTrainingFinished())
                {
                    _reasonText.text = StringResources.GetLocalizedString("GameOverDeath");
                }
                else
                {
                    _hero.Resurrect();
                    _trainingHandler.TrainingFailDeath();
                    gameOverMenu.gameObject.SetActive(false);
                    return;
                }
            }
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
            
            if (!response.HasField("error"))
            {
                if (response.HasField("tramLivesCount"))
                {
                    float count = response.GetField("tramLivesCount").n;
                    string message = String.Format(StringResources.GetLocalizedString("remainLivesCount"), count);
                    _messages.AddMessage(message);
                }
            }
		});
		if(_stateInfo.TicketCount > 0) {
			RecordsManager.GetInstance().AddRecord(_stateInfo.TicketCount);
			_client.SendRecord (_stateInfo.TicketCount, false, (result) => {
               
                if (!result.HasField("error"))
                {
                    string message = string.Format(StringResources.GetLocalizedString("newTicketsRecord"),
                            _stateInfo.TicketCount) + Environment.NewLine  + StringResources.GetLocalizedString("reward");
                    MessageSender.SendRewardMessage(result, _messages, message);
                }
			});
		}
		int flyingAwayCount = 0;
		foreach (KeyValuePair<string, int> pair in GameController.GetInstance().GetFlyingAwayDuringGame ()) {
			flyingAwayCount += pair.Value;
		    var pair1 = pair;
		    _client.SendDangerRecord(pair.Value, pair.Key, false, (result) => {
                string message = string.Format(StringResources.GetLocalizedString("newDangerRecord"),
                            StringResources.GetLocalizedString(pair1.Key + "Name"), pair1.Value) + Environment.NewLine + StringResources.GetLocalizedString("reward");
                MessageSender.SendRewardMessage(result, _messages, message);
            });
        }
		int stationNumber = GameController.GetInstance ().GetCurrentStationNumber ();
		if(stationNumber > 0 && flyingAwayCount == 0) {
			_client.SendPacifistRecord (stationNumber, (result) => {
                if (!result.HasField("error"))
                {
                    string message = string.Format(StringResources.GetLocalizedString("newPacifistRecord"),
                            stationNumber);
                    MessageSender.SendRewardMessage(result, _messages, message);
                }
            });
		}
        int antistick = GameController.GetInstance().GetAntiStick();
        if (antistick > 0)
        {
            _client.SendAntiStickRecord(antistick, (result) =>
            {
                if (!result.HasField("error"))
                {
                    string message = string.Format(StringResources.GetLocalizedString("newAntistickRecord"),
                            antistick);
                    MessageSender.SendRewardMessage(result, _messages, message);
                }
            });
        }
		int bigStationsCount = GameController.GetInstance ().GetBigStationsCount ();
		if(bigStationsCount > 0) {
			if(GameController.GetInstance ().GetKilledPassengersCount () == 0) {
				_client.SendLivesaverRecord (bigStationsCount, (result) => {
                    if (!result.HasField("error"))
                    {
                        string message = string.Format(StringResources.GetLocalizedString("newLivesaverRecord"),
                                bigStationsCount);
                        MessageSender.SendRewardMessage(result, _messages, message);
                    }
                });
			}
			if(bigStationsCount > 1) {
				_client.SendTruckerRecord (bigStationsCount, (result) => {
                    if (!result.HasField("error"))
                    {
                        string message = string.Format(StringResources.GetLocalizedString("newTruckerRecord"),
                                bigStationsCount);
                        MessageSender.SendRewardMessage(result, _messages, message);
                    }
                });
			}
		}
    }
}
