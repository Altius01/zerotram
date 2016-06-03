﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrainingHandler : MonoBehaviour
{
    [SerializeField]
    private ConductorWindow _fullConductorWindow;
    [SerializeField]
    private ConductorWindow _shortConductorWindow;

    [SerializeField] private GameObject _bonusSelectWindow;
    [SerializeField] private DoorsAnimationController[] _doors;
    [SerializeField] private GameObject _doorsTimer;
    [SerializeField]
    private GameObject _ticketsCounter;
    [SerializeField]
    private GameObject _haresCounter;
    [SerializeField]
    private GameObject _killedCounter;
    [SerializeField]
    private GameObject _lifes;
    [SerializeField]
    private GameObject _arrowPrefab;

    [SerializeField] private GameObject _centralWayout;
    [SerializeField] private GameObject _centralWayoutSprite;
    [SerializeField] private DoorsTimer _doorsTimerController;
    [SerializeField] private Floor _floor;

    private GameObject _activeArrow;

    private const string TrainingKey = "TrainingFinished";

    private int _currentStep;
    private bool _isRefreshInProgress;
    private bool _isPassengerClickAllowed;
    private Bird _birdPassenger;
    private Gnome _gnomePassenger;
    private Cat _catPassenger;
    private Granny _grannyPassenger;
    private int _lastSavedStep;
    private int _goAwayDoorIndex;
    private ConductorSM _hero;
    private GameObject _benches;
    private bool _isBonusDropEnabled;
    private bool _isGnomeSurvived;

    public void SetIsGnomeSurvived(bool val)
    {
        _isGnomeSurvived = val;
    }

    public static bool IsTrainingFinished()
    {
        return PlayerPrefs.HasKey(TrainingKey);
    }

    public bool IsBonusDropEnabled()
    {
        return _isBonusDropEnabled;
    }
    
    public bool IsPassengerClickAllowed()
    {
        return _isPassengerClickAllowed;
    }
    
    // Use this for initialization
    void Start () {
        ShowTrainingStep(0);
    }
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void ShowNext()
    {
        if(_isRefreshInProgress)
            return;
        _isRefreshInProgress = true;
        _currentStep++;
        ShowTrainingStep(_currentStep);
    }

    private void ShowTrainingStep(int step)
    {
        switch (step)
        {
            case 0:
                _benches = GameObject.Find("benches");
                _benches.SetActive(false);
                _isBonusDropEnabled = false;
                _doorsTimer.SetActive(false);
                _ticketsCounter.SetActive(false);
                _haresCounter.SetActive(false);
                _killedCounter.SetActive(false);
                _lifes.SetActive(false);
                _bonusSelectWindow.SetActive(false);
                _isPassengerClickAllowed = false;
                _centralWayout.SetActive(false);
                _centralWayoutSprite.SetActive(false);
                Time.timeScale = 0;
                _doorsTimerController.SetMoveAndStopDuration(3, 1);
                _doorsTimerController.SetMovementLocked(true);
                _fullConductorWindow.DisplayText(StringResources.GetLocalizedString("Training1"), false);
                break;
            case 1:
                _fullConductorWindow.DisplayTextWithImage(StringResources.GetLocalizedString("Training2"), Resources.Load<Sprite>("Sprites/training/training1"), false);
                break;
            case 2:
                _fullConductorWindow.DisplayText(StringResources.GetLocalizedString("Training3"), true);
                break;
            case 3:
                Time.timeScale = 1;
                SpawnPassengerFromRandomDoor("gnome", Spawner.TicketAdditionMode.WithTicket);
                StartCoroutine(WaitAndMoveNext(2));
                break;
            case 4:
                Time.timeScale = 0;
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training4"), true);
                GameObject gnomeObject = GameObject.Find("gnome(Clone)");
                _gnomePassenger = gnomeObject.GetComponent<Gnome>();
                _gnomePassenger.SetAttackEnabled(false);
                _gnomePassenger.SetFlyAwayDenied(true);
                _gnomePassenger.SetDragDenied(true);
                DisplayArrowForPassenger(_gnomePassenger);
                _isPassengerClickAllowed = true;
                break;
            case 5:
                Time.timeScale = 1;
                _ticketsCounter.SetActive(true);
                break;
            case 6:
                Time.timeScale = 0;
                Destroy(_activeArrow);
                _doorsTimer.SetActive(true);
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training5"), true);
                break;
            case 7:
                Time.timeScale = 1;
                _doorsTimerController.SetMovementLocked(false);
                StartCoroutine(WaitAndMoveNext(2));
                break;
            case 8:
                SpawnPassengerFromRandomDoor("bird", Spawner.TicketAdditionMode.WithoutTicket);
                GameObject bird = GameObject.Find("bird(Clone)");
                _birdPassenger = bird.GetComponent<Bird>();
                _birdPassenger.SetFlyAwayDenied(true);
                _birdPassenger.SetAttackEnabled(false);
                _birdPassenger.SetRunawayDenied(true);
                DisplayArrowForPassenger(_birdPassenger);
                _doorsTimerController.SetMovementLocked(true);
                break;
            case 9:
                Time.timeScale = 0;
                Destroy(_activeArrow);
                _doorsTimer.SetActive(true);
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training6"), true);
                break;
            case 10:
                _centralWayout.SetActive(true);
                _centralWayoutSprite.SetActive(true);
                DisplayArrow(_centralWayout);
                Time.timeScale = 1;
                _floor.AddDragCenterListner(_birdPassenger.name);
                break;
            case 11:
                Time.timeScale = 0;
                Destroy(_activeArrow);
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training7"), true);
                break;
            case 12:
                Time.timeScale = 1;
                _birdPassenger.SetRunawayDenied(false);
                _birdPassenger.SetFlyAwayDenied(false);
                _birdPassenger.ActivateFlyAwayListener();
                break;
            case 13:
                Time.timeScale = 0;
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training8"), true);
                break;
            case 14:
                _doorsTimerController.SetMoveAndStopDuration(3, 5);
                Time.timeScale = 1;
                _doorsTimerController.SetMovementLocked(false);
                _goAwayDoorIndex = Randomizer.GetInRange(0, _doors.Length);
                _gnomePassenger.SetAlwaysStickForTraining();
                _gnomePassenger.StartGoAway();
                _lastSavedStep = 14;
                StartCoroutine(WaitAndMoveNext(2.9f));
                break;
            case 15:
                _doors[(_goAwayDoorIndex)].Open(false);
                break;
            case 16:
                Time.timeScale = 0;
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training9"), true);
                break;
            case 17:
                Time.timeScale = 1;
                break;
            case 18:
                Time.timeScale = 0;
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString(_isGnomeSurvived ? "Training10" : "Training11"), true);
                break;
            case 19:
                Time.timeScale = 1;
                _doors[(_goAwayDoorIndex)].Close();
                _doorsTimerController.Unstick();
                StartCoroutine(WaitAndMoveNext(_doorsTimerController.GetCurrentRemainingTime() + 3));
                break;
            case 20:
                _lastSavedStep = 20;
                _goAwayDoorIndex = Randomizer.GetInRange(0, _doors.Length);
                _doorsTimerController.OpenDoors();

                int index = Randomizer.GetInRange(0, _doors.Length);
                _doors[index].OpenAndSpawnByName("granny", Spawner.TicketAdditionMode.WithTicket);
                index = Randomizer.GetInRange(0, _doors.Length);
                _doors[index].OpenAndSpawnByName("cat", Spawner.TicketAdditionMode.WithoutTicket);
                StartCoroutine(WaitAndMoveNext(0.1f));
                break;
            case 21:
                _doorsTimerController.SetMovementLocked(true);
                GameObject grannyObject = GameObject.Find("granny(Clone)");
                _grannyPassenger = grannyObject.GetComponent<Granny>();
                GameObject catObject = GameObject.Find("cat(Clone)");

                _catPassenger = catObject.GetComponent<Cat>();
                _catPassenger.SetMaximumAttackProbabilityForTraining();
                _grannyPassenger.SetMaximumAttackProbabilityForTraining();
                _grannyPassenger.SetConductorAttackDenied(true);
                _catPassenger.SetConductorAttackDenied(true);
                _catPassenger.SetFlyAwayDenied(true);
                _grannyPassenger.SetFlyAwayDenied(true);
                break;
            case 22:
                StartCoroutine(WaitAndMoveNext(2));
                break;
            case 23:
                Time.timeScale = 0;
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training12"), false);
                break;
            case 24:
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training13"), false);
                break;
            case 25:
                _killedCounter.SetActive(true);
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training14"), true);
                break;
            case 26:
                Time.timeScale = 1;
                _grannyPassenger.SetConductorAttackDenied(false);
                _catPassenger.SetConductorAttackDenied(false);
                _hero = GameObject.Find("hero").GetComponent<ConductorSM>();
                _hero.SetAttackListenerActivated();
                break;
            case 27:
                StartCoroutine(WaitAndMoveNext(4));
                break;
            case 28:
                Time.timeScale = 0;
                _lifes.SetActive(true);
                _shortConductorWindow.DisplayText(StringResources.GetLocalizedString("Training15"), true);
                break;
            case 29:
                Time.timeScale = 1;
                break;
        }
        _isRefreshInProgress = false;
    }

    private void TrainingFail(string textId)
    {
        Time.timeScale = 0;
        _currentStep = _lastSavedStep - 1;
        _shortConductorWindow.DisplayText(StringResources.GetLocalizedString(textId), true);
    }

    public void TrainingFailPassengers()
    {
        TrainingFail("TrainingFailPassengers");
    }

    public void TrainingFailHare()
    {
        TrainingFail("TrainingFailHare");
    }

    public void TrainingFailDeath()
    {
        TrainingFail("TrainingFailDeath");
    }

    private IEnumerator WaitAndMoveNext(float amount)
    {
        yield return new WaitForSeconds(amount);
        ShowNext();
    }

    private void DisplayArrowForPassenger(PassengerSM passenger)
    {
        DisplayArrow(passenger.gameObject);
        passenger.EnableTrainingClick();
    }

    private void DisplayArrow(GameObject go)
    {
        GameObject arrow = (GameObject)Instantiate(_arrowPrefab, go.gameObject.transform.position, Quaternion.identity);
        arrow.transform.parent = go.gameObject.transform;
        arrow.transform.localPosition = new Vector3(1, -1.4f, -8);
        _activeArrow = arrow;
    }

    private void SpawnPassengerFromRandomDoor(string passengerName, Spawner.TicketAdditionMode mode)
    {
        _doorsTimerController.OpenDoors();
        int index = Randomizer.GetInRange(0, _doors.Length);
        _doors[index].OpenAndSpawnByName(passengerName, mode);
    }
}
