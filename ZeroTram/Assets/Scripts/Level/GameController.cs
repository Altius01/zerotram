﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Scripts.Level;
using UnityEngine;

namespace Assets
{
    public class GameController
    {
        private static GameController _instance;

        private int _incomingPassengers;
        private int _killedPassengers;
        private int _totalHares;

        private List<Passenger> _passengers;
        private List<GameStateNotificationListener> _listeners;

        private int _killedPercent;
        private int _haresPercent;

        private const int InitialSpawnCount = 5;
        private const int SpawnIncrementCount = 1;

        private int _currentSpawnCount;
        private int _currentStationNumber;

        public static GameController GetInstance()
        {
            if(_instance == null)
                _instance = new GameController();
            return _instance;
        }

        GameController()
        {
            _passengers = new List<Passenger>();
            _listeners = new List<GameStateNotificationListener>();
            _totalHares = 0;
            _incomingPassengers = 0;
            _currentSpawnCount = InitialSpawnCount;
            _currentStationNumber = 0;
        }

        public int GetCurrentStationNumber()
        {
            return _currentStationNumber;
        }

        public int GetCurrentSpawnCount()
        {
            return _currentSpawnCount;
        }

        public int GetPassengersCount()
        {
            return _passengers.Count();
        }

        public void AddListener(GameStateNotificationListener listener)
        {
            _listeners.Add(listener);
        }

        public void RegisterPassenger(Passenger ps)
        {
            if (ps.HasTicket())
                _incomingPassengers++;
            else
                _totalHares++;
            _passengers.Add(ps);
        }

        public void RegisterDeath(MovableObject obj)
        {
            if (obj as Hero != null)
            {
                GameOver();
            }
            else
            {
                Passenger ps = obj as Passenger;
                if (ps != null)
                {
                    if (ps.HasTicket())
                    {
                        _killedPassengers++;
                        CheckStats();
                        foreach (var gameStateNotificationListener in _listeners)
                        {
                            gameStateNotificationListener.UpdatePercentage(_killedPercent, _haresPercent);
                        }
                    }
                    else
                    {
                        _totalHares--;
                    }
                }
                if(_passengers.Contains(ps))
                    _passengers.Remove(ps);
            }
        }

        public void UndragAll()
        {
            foreach (var passenger in _passengers)
            {
                passenger.SetDragged(false);
            }
        }

        public void GameOver()
        {
            Debug.Log("game over");
            Time.timeScale = 0;
            foreach (var gameStateNotificationListener in _listeners)
            {
                gameStateNotificationListener.GameOver();
            }
        }

        private void UpdateStats()
        {
            float haresPercent = _totalHares / (float)_passengers.Count;
            _haresPercent = Mathf.RoundToInt(haresPercent * 100);
            float killedPercent = _killedPassengers / (float)_incomingPassengers;
            _killedPercent = Mathf.RoundToInt(killedPercent * 100);
        }

        public void CheckStats()
        {
            UpdateStats();
            if (_haresPercent > 50 || _killedPercent > 50)
            {
                Debug.Log("caused by stats");
                GameOver();
            }
            else
            {
                _currentStationNumber++;
                _currentSpawnCount += SpawnIncrementCount;
            }
        }
    }
}
