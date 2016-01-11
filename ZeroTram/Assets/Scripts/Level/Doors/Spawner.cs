﻿using System;
using System.Collections.Generic;
using Assets;
using Assets.Scripts.Math;
using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> unitPrefabs;
    [SerializeField] private BonusTimer _bonusTimer;
    [SerializeField] private DoorsTimer _doorsTimer;
    private float _maxPassengers;

    public static float StickYOffset = 0.8f;

    void Awake()
    {
        //PlayerPrefs.DeleteAll();

        _maxPassengers = ConfigReader.GetConfig().GetField("tram").GetField("MaxPassengers").n;
        GameController.GetInstance().StartNewGame();
    }

    public PassengerSM SpawnAlternativePassenger(Vector3 position, string previousClass)
    {
        string newPassengerClass = MapManager.GetInstance().GetRandomCharacterWithExcludedIndex(previousClass);
        return InstantiateNPC(newPassengerClass, position, false);
    }

    private PassengerSM InstantiateNPC(string className, Vector3 position, bool register)
    {
        int randomIndex = PassengerIndex(className);
        if (randomIndex < 0)
            return null;
        GameObject randomNPC = unitPrefabs[randomIndex];
        GameObject instantiated =
                    (GameObject)Instantiate(randomNPC, position, Quaternion.identity);
        PassengerSM ps = instantiated.GetComponent<PassengerSM>();
        ps.Init(register);
        return ps;
    }

    public void Spawn(GameObject spawnPoint)
    {
        if(GameController.GetInstance().IsGameFinished)
            return;
        int maxCount = GameController.GetInstance().GetCurrentSpawnCount();
        int realCount = Randomizer.GetInRange(1, maxCount);
        
        for (int i = 0; i < realCount; i++)
        {
            if(GameController.GetInstance().GetPassengersCount() > _maxPassengers)
                return;
            string passengerString = MapManager.GetInstance().GetRandomCharacter();
            PassengerSM ps = InstantiateNPC(passengerString, spawnPoint.transform.position, true);
            if(ps == null)
                return;
            _bonusTimer.AddBonusEffectToSpawnedPassenger(ps);
            if (ps.IsStick())
            {
                _doorsTimer.SetPaused(true);
                return;
            }
            ps.CalculateRandomTarget(true);
        } 
    }

    private int PassengerIndex(string stringRepresentation)
    {
        switch (stringRepresentation)
        {
            case "alien":
                return 0;
            case "bird":
                return 1;
			case "cat" :
                return 2;
			case "gnome" :
                return 3;
			case "granny" :
                return 4;
        }
        return -1;
    }
}
