﻿using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking.NetworkSystem;

public class PassengerSM : MovableCharacterSM
{
    public bool IsNearBench;

    private int _tramStopCount;
    private int _currentTramStopCount;
    public float AttackProbability = 50;
    public float ChangeStatePeriod = 10;
    public float DragChangeStatePeriod = 10;
    protected float CounterAttackProbability = 50;
    protected float StickProbability = 0;
    protected float TicketProbability;
    public float BonusProbability = 0;
    private bool _hasTicket;
    private float _maxStopCount;
    private bool _isMagnetTurnedOn;
    private float _magnetDistance;
    private float _attackingDenyPeriod;
    private bool _isMagnetActivated;

    public bool IsAttackingAllowed;
    private Bench _currentBench;

    private bool _isFlyingAwayListenerActivated;

    public Dictionary<GameController.BonusTypes, float> BonusProbabilities; 

    private float _savedStickProbability;

    public List<GameController.BonusTypes> ActiveBonuses;

    private bool _isDragRunawayDeniedByTraining;
    private bool _isFlyAwayDenied;
    private bool _isDragDenied;
    private bool _isDragListenerActivated;
    private bool _isSitListenerActivated;
    private MovableCharacterSM _pursuer;

    [SerializeField]
    private Sprite _question;
    [SerializeField]
    private Sprite _ticket;
    [SerializeField]
    private Sprite _hare;
    [SerializeField]
    private Sprite _stick;
    [SerializeField]
    protected SpriteRenderer Indicator;
    
    private bool _isTrainingEnabled;

    private bool _attackDenyedByTraining;
    private bool _isStickModifiedForTraining;
    private bool _isConductorAttackDenied;
    private bool _isPassengerAttackDenied;
    private bool _isGoAwayVelocityIncreased;
    private float _calculatedAttackTargetDistance = -1;

    void Awake()
    {
        ActiveBonuses = new List<GameController.BonusTypes>();
        PassengerIdleState idleState = new PassengerIdleState(this);
        PassengerMoveState moveState = new PassengerMoveState(this);
        PassengerAttackState attackState = new PassengerAttackState(this);
        PassengerAttackedState attackedState = new PassengerAttackedState(this);
        PassengerStickState stickState = new PassengerStickState(this);
        PassengerFlyingAwayState flyingAwayState = new PassengerFlyingAwayState(this);
        PassengerDraggedState draggedState = new PassengerDraggedState(this);
        FrozenState frozenState = new FrozenState(this);
        PassengerSitState sitState = new PassengerSitState(this);
        PassengerHuntState huntState = new PassengerHuntState(this);
        PassengerEscapeState escapeState = new PassengerEscapeState(this);
        Dictionary<int, State> stateMap = new Dictionary<int, State>
        {
            {(int) MovableCharacterStates.Idle, idleState},
            {(int) MovableCharacterStates.Move, moveState},
            {(int) MovableCharacterStates.Attack, attackState},
            {(int) MovableCharacterStates.Attacked, attackedState},
            {(int) MovableCharacterStates.Stick, stickState},
            {(int) MovableCharacterStates.FlyingAway, flyingAwayState},
            {(int) MovableCharacterStates.Dragged, draggedState},
            {(int) MovableCharacterStates.Frozen, frozenState},
            {(int) MovableCharacterStates.Sit, sitState},
            {(int) MovableCharacterStates.Hunt, huntState},
            {(int) MovableCharacterStates.Escape, escapeState},
        };
        InitWithStates(stateMap, (int)MovableCharacterStates.Idle);
    }

    public MovableCharacterSM GetPursuer()
    {
        return _pursuer;
    }

    public void BeginEscape(MovableCharacterSM pursuer)
    {
        _pursuer = pursuer;
        ActivateState((int)MovableCharacterStates.Escape);
    }

    public void StopEscape()
    {
        _pursuer = null;
    }

    public void SetFlyAwayDenied(bool denied)
    {
        _isFlyAwayDenied = denied;
    }

    public void SetSitListenerActivated(bool activated)
    {
        _isSitListenerActivated = activated;
    }

    public void SetConductorAttackDenied(bool value)
    {
        _isConductorAttackDenied = value;
    }

    public void IncreaseGoAwayVelocity()
    {
        _isGoAwayVelocityIncreased = true;
    }

    public void SetPassengerAttackDenied(bool value)
    {
        _isPassengerAttackDenied = value;
    }

    public void SetDragDenied(bool denied)
    {
        _isDragDenied = denied;
    }

    public void SetDragListenerEnabled(bool value)
    {
        _isDragListenerActivated = value;
    }

    public void SetAttackEnabled(bool isEnabled)
    {
        _attackDenyedByTraining = !isEnabled;
    }
    
    public void AttackIfPossible()
    {
        if (IsAttackingAllowed && !_attackDenyedByTraining)
            ActivateState((int)MovableCharacterSM.MovableCharacterStates.Attack);
    }

    public void SetRunawayDenied(bool denied)
    {
        _isDragRunawayDeniedByTraining = denied;
    }

    public bool IsRunawayDenied()
    {
        return _isDragRunawayDeniedByTraining;
    }

    public void ActivateFlyAwayListener()
    {
        _isFlyingAwayListenerActivated = true;
    }

    public bool IsFlyAwayListenerActivated()
    {
        return _isFlyingAwayListenerActivated;
    }

    public void RecalculateTicketProbability(float coef, bool onlyForInvisible)
    {
        TicketProbability *= coef;
        if(onlyForInvisible && _isVisibleToHero)
            return;
        _hasTicket = Randomizer.GetPercentageBasedBoolean((int)TicketProbability);
    }

    public void EnableTrainingClick()
    {
        _isTrainingEnabled = true;
    }

    public virtual void Init(bool register, bool unstickable = false)
    {
        AttackProbability = ConfigReader.GetConfig().GetField(GetClassName()).GetField("AttackProbability").n;
        DragChangeStatePeriod = ConfigReader.GetConfig().GetField(GetClassName()).GetField("DragChangeStatePeriod").n;
        ChangeStatePeriod = ConfigReader.GetConfig().GetField(GetClassName()).GetField("ChangeStatePeriod").n;
        AttackDistance = ConfigReader.GetConfig().GetField(GetClassName()).GetField("AttackDistance").n;
        AttackReloadPeriod = ConfigReader.GetConfig().GetField(GetClassName()).GetField("AttackReloadPeriod").n;
        AttackMaxDistance = ConfigReader.GetConfig().GetField(GetClassName()).GetField("AttackMaxDistance").n;
        CounterAttackProbability = ConfigReader.GetConfig().GetField(GetClassName()).GetField("CounterAttackProbability").n;
        Hp = InitialLifes = ConfigReader.GetConfig().GetField(GetClassName()).GetField("InitialLifes").n;
        Velocity = ConfigReader.GetConfig().GetField(GetClassName()).GetField("Velocity").n;
        AttackStrength = ConfigReader.GetConfig().GetField(GetClassName()).GetField("AttackStrength").n;
        AttackReactionPeriod = ConfigReader.GetConfig().GetField(GetClassName()).GetField("AttackReactionPeriod").n;
        TicketProbability = ConfigReader.GetConfig().GetField(GetClassName()).GetField("TicketProbability").n;
        StickProbability = ConfigReader.GetConfig().GetField(GetClassName()).GetField("StickProbability").n;
        BonusProbability = ConfigReader.GetConfig().GetField(GetClassName()).GetField("BonusProbability").n;
        _attackingDenyPeriod = ConfigReader.GetConfig().GetField("tram").GetField("AttackDenyPeriod").n;
        ParseBonusMap();
        _hasTicket = Randomizer.GetPercentageBasedBoolean((int)TicketProbability);
        if(!unstickable)
            CalculateStick();
        _maxStopCount = ConfigReader.GetConfig().GetField("tram").GetField("MaxStopCount").n;
        int stopCount = Randomizer.GetInRange(1, (int)_maxStopCount);
        _tramStopCount = stopCount;
        if(register)
            GameController.GetInstance().RegisterPassenger(this);
    }

    public void IncreaseBonusProbability()
    {
        BonusProbability = 100;
    }

    private float AttackTargetDistance()
    {
        if (AttackTarget == null)
            return float.MaxValue;
        Vector2 position2D = transform.position;
        Vector2 attackTargetPosition2D = AttackTarget.BoxCollider2D.bounds.ClosestPoint(transform.position);
        _calculatedAttackTargetDistance = (position2D - attackTargetPosition2D).sqrMagnitude;
        return _calculatedAttackTargetDistance;
    }

    public float CalculatedAttackTargetDistance()
    {
        if (_calculatedAttackTargetDistance == -1)
        {
            _calculatedAttackTargetDistance = AttackTargetDistance();
        }
        return _calculatedAttackTargetDistance;
    }

    public bool HasTicket()
    {
        return _hasTicket;
    }

    public void SetTicketAndVisibility(bool hasTicket, bool isVisible)
    {
        _hasTicket = hasTicket;
        _isVisibleToHero = isVisible;
    }

    public void TurnOnMagnet(float dist)
    {
        if(_hasTicket && _isVisibleToHero)
            return;
        _isMagnetTurnedOn = true;
        _magnetDistance = dist;
    }

    public void TurnOffMagnet()
    {
        _isMagnetTurnedOn = false;
    }

    public bool IsStick()
    {
        return GetActiveState().Equals((int) MovableCharacterStates.Stick);
    }

    public bool WasStickWhenFlyAway { get; set; }

    private bool _isVisibleToHero;

    public bool IsVisibleToHero()
    {
        return _isVisibleToHero;
    }
    
    public void StartDrag()
    {
        if (_isDragListenerActivated)
        {
            ShowNextTrainingMessage();
            _isDragListenerActivated = false;
        }
        if (IsFrozen())
        {
            TemporalyUnfreeze();
        }
        ActivateState((int)MovableCharacterStates.Dragged);
    }

    public void StartGoAway()
    {
        _currentTramStopCount = int.MaxValue - 1;
    }

    public void SetAlwaysStickForTraining()
    {
        StickProbability = 100;
        _isStickModifiedForTraining = true;
    }

    public void SetStickProbability(float probability)
    {
        StickProbability = probability;
    }

    public void SetCounterAttackProbability(float probability)
    {
        CounterAttackProbability = probability;
    }

    public void IncrementStationCount()
    {
        _currentTramStopCount++;
        if (_currentTramStopCount > _tramStopCount && !IsGoingAway)
        {
            DoorsAnimationController door1 =
                    MonobehaviorHandler.GetMonobeharior().GetObject<DoorsAnimationController>("door1");
            DoorsAnimationController door2 =
                MonobehaviorHandler.GetMonobeharior().GetObject<DoorsAnimationController>("door2");
            DoorsAnimationController door3 =
                MonobehaviorHandler.GetMonobeharior().GetObject<DoorsAnimationController>("door3");
            DoorsAnimationController door4 =
                MonobehaviorHandler.GetMonobeharior().GetObject<DoorsAnimationController>("door4");
            List<DoorsAnimationController> selected = new List<DoorsAnimationController>();
            if (door1.IsOpened())
                selected.Add(door1);
            if (door2.IsOpened())
                selected.Add(door2);
            if (door3.IsOpened())
                selected.Add(door3);
            if (door4.IsOpened())
                selected.Add(door4);
            if (selected.Count == 0)
                return;
            int randomPercent = Randomizer.GetRandomPercent();
            int step = 100 / selected.Count;
            int currentStep = 0;
            int i = 0;
            for (i = 0; i < selected.Count - 1; i++)
            {
                if (currentStep > randomPercent)
                {
                    break;
                }
                currentStep += step;
            }
            BoxCollider2D collider = selected[i].GetComponent<BoxCollider2D>();
            Vector2 target = new Vector2(selected[i].gameObject.transform.position.x, selected[i].gameObject.transform.position.y) + collider.offset;
            Velocity *= 2;
            if (_isGoAwayVelocityIncreased)
            {
                Velocity *= 2;
            }
            IsGoingAway = true;
            base.SetTarget(target);
        }
    }
    
    public void CalculateStick()
    {
        if(GameController.GetInstance().IsAnybodyStick())
            return;
        bool stick = Randomizer.GetPercentageBasedBoolean((int)StickProbability);
        if (stick && MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").IsPassengerNearDoors(this))
        {
            if (_isStickModifiedForTraining)
            {
                ShowNextTrainingMessage();
            }
            ActivateState((int)MovableCharacterStates.Stick);
            Indicator.sprite = _stick;
        }
    }

    private void ShowNextTrainingMessage()
    {
        TrainingHandler handler =
                    MonobehaviorHandler.GetMonobeharior().GetObject<TrainingHandler>("TrainingHandler");
        handler.ShowNext();
    }

    public override bool CanNotInteract()
    {
        int activeStateCode = GetActiveState();
        return (activeStateCode == (int) MovableCharacterStates.FlyingAway || IsGoingAway ||
                IsStick() || activeStateCode == (int) MovableCharacterStates.Dragged);
    }

    //will be replaced with current skin config

    public int GetStandPossibility()
    {
        return (int)ConfigReader.GetConfig().GetField("tram").GetField("StandPossibility").n;
    }

    public float GetStopStandPeriod()
    {
        return ConfigReader.GetConfig().GetField("tram").GetField("StopStandCheckPeriod").n; 
    }

    public void HandleStandUp()
    {
        if (_currentBench != null)
        {
            _currentBench = null;
            CalculateRandomTarget();
        }
    }

    public bool IsOnTheBench()
    {
        return GetActiveState() == (int)MovableCharacterStates.Sit;
    }

    public void HandleTriggerEnter(Collider2D other)
    {
        if (CanNotInteract())
            return;
        MovableCharacterSM movable = other.gameObject.GetComponentInParent<MovableCharacterSM>();
        if (movable != null)
        {
            TryAttackMovable(movable);
        }
    }

    public bool IsSitListenerActivated()
    {
        return _isSitListenerActivated;
    }

    public void ActivateSitListener()
    {
        if (_isSitListenerActivated)
        {
            ShowNextTrainingMessage();
            _isSitListenerActivated = false;
        }
    }

    public void HandleSitdown(Bench bench)
    {
        if(IsGoingAway)
            return;
        _currentBench = bench;
        AttackTarget = null;
        transform.position = new Vector3(bench.transform.position.x, bench.transform.position.y, transform.position.z);
        ActivateState((int) MovableCharacterStates.Sit);
    }

    public void TryAttackMovable(MovableCharacterSM movable)
    {
        if(_attackDenyedByTraining)
            return;
        float currentAttackProbability = AttackProbability;
        var sm = movable as PassengerSM;
        if (sm != null)
        {
            if(_isPassengerAttackDenied)
                return;
            PassengerSM passenger = sm;
            if (passenger.IsOnTheBench())
            {
                currentAttackProbability *=
                     ConfigReader.GetConfig().GetField("tram").GetField("SitAggressionIncrement").n;
            }
        }
        if (Randomizer.GetPercentageBasedBoolean((int)currentAttackProbability))
        {
            if (_isConductorAttackDenied)
            {
                ConductorSM conductor = movable as ConductorSM;
                if (conductor != null)
                {
                    return;
                }
            }
            AttackTarget = movable;
            AttackIfPossible();
        }
        else
        {
            MakeIdle();
        }
    }


    public override void MakeIdle()
    {
        if(IsAttackingAllowed)
            base.MakeIdle();
    }

    public void BeginHunt()
    {
        if (AttackTarget == null)
            return;
        Vector2 result = AttackTarget.BoxCollider2D.bounds.ClosestPoint(transform.position);
        Target = result;
        CalculateOrientation(result);
        ActivateState((int)MovableCharacterStates.Hunt);
    }

    public void SetEscapeTarget(Vector2 target)
    {
        Target = target;
        CalculateOrientation(target);
    }
    
    public override void SetTarget(Vector2 target)
    {
        if (IsGoingAway)
        {
            CalculateOrientation(GetTarget());
            ActivateState((int) MovableCharacterStates.Move);
        }
        else
        {
            base.SetTarget(target);
        }
    }

    public void CalculateRandomTarget(bool force = false)
    {
        if (IsGoingAway)
        {
            SetTarget(GetTarget());
            return;
        }
        if(AttackTarget != null && !force)
            return;
        Vector2 target = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").GetRandomPosition();
        if (target != default(Vector2))
            SetTarget(target);
    }

    public void SetMaximumAttackProbabilityForTraining()
    {
        AttackProbability = 100;
        ChangeStatePeriod = 1f;
        IsAttackListenerActivated = true;
    }

    public void DisableAttackListener()
    {
        IsAttackListenerActivated = false;
    }

    public void CalculateAttackReaction()
    {
        if(_attackDenyedByTraining)
            return;
        bool willCounterAttack = Randomizer.GetPercentageBasedBoolean((int)CounterAttackProbability);
        if (willCounterAttack)
        {
            if (AttackTarget != null)
                AttackIfPossible();
            else
                MakeIdle();
        }
        else
        {
            MovableCharacterSM pursuer = AttackTarget;
            AttackTarget = null;
            BeginEscape(pursuer);
        }
    }

    public bool IsStickModifiedForTraining()
    {
        return _isStickModifiedForTraining;
    }

    public void StopStick()
    {
        if (!IsGoingAway)
        {
            CalculateRandomTarget();
        }
        else
        {
            if (_isStickModifiedForTraining)
            {
                ShowNextTrainingMessage();
            }
            Destroy(gameObject);       
        }
        MonobehaviorHandler.GetMonobeharior().GetObject<DoorsTimer>("DoorsTimer").SetPaused(false);
        GameController.GetInstance().IncreaseAntiStick();
    }
    
    public void FlyAway()
    {
        ActivateState((int)MovableCharacterStates.FlyingAway);
        AttackTarget = null;
        GameController.GetInstance().RegisterDeath(this);
    }

    public void StartUnstick(ConductorSM hero)
    {
        TimeSincePreviousClickMade = MaxClickDuration;
        hero.SetTarget(transform.position);
        hero.StartSaveStickPassenger(this);
    }

    public override void HandleClick()
    {
        if (_isTrainingEnabled)
        {
            ShowNextTrainingMessage();
            _isTrainingEnabled = false;
        }
        ConductorSM hero = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").GetHero();
        if (!_isVisibleToHero)
        {
            if (IsStick())
            {
                StartUnstick(hero);
            }
            else
            {
                _isVisibleToHero = true;
                GameController.GetInstance().UpdatePassenger(this);
                if (!_hasTicket)
                {
                    if (!_isConductorAttackDenied)
                    {
                        AttackTarget = hero;
                        CalculateAttackReaction();
                    }
                }
                else
                {
                    MonobehaviorHandler.GetMonobeharior().GetObject<AudioPlayer>("AudioPlayer").PlayAudioById("coins");
                }
            }
            return;
        }
        if (IsStick())
        {
            StartUnstick(hero);
            return;
        }
        hero.StartDrag(this);
    }

    public bool IsDragDenied()
    {
        return _isDragDenied;
    }

    public override void HandleDoubleClick()
    {
        ConductorSM hero = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").GetHero();
        if (hero.CanKick(this))
        {
            if (_isFlyAwayDenied)
                return;
            hero.Kick(this);
            return;
        }
        hero.StartDrag(this);
    }

    private void CalculateIndicator()
    {
        if (IsStick())
        {
            Indicator.sprite = _stick;
            return;
        }
        if (!_isVisibleToHero)
        {
            Indicator.sprite = _question;
            return;
        }
        if (_hasTicket)
            Indicator.sprite = _ticket;
        else
            Indicator.sprite = _hare;
    }

    public void StopDrag(bool attack)
    {
        if(GetActiveState() != (int)MovableCharacterStates.Dragged)
            return;
        Floor floor = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor");
        floor.OnMouseUp();
        ConductorSM conductor = floor.GetHero();
        if (conductor.CanKick(this))
        {
            if (!floor.IsPassengerNearDoors(this))
            {
                if (!HasTicket())
                {
                    BeginEscape(conductor);
                }
            }
        }
        else
        {
            if (attack)
            {
                if (_isConductorAttackDenied)
                {
                    MakeIdle();
                    return;
                }
                AttackTarget = conductor;
                AttackIfPossible();
            }
            else
            {
                MakeIdle();
            }
        }
    }
    
    public void CalculateMagnet()
    {
        if (_isMagnetActivated)
        {
            if (_hasTicket && _isVisibleToHero)
            {
                _isMagnetActivated = false;
                _isMagnetTurnedOn = false;
            }
        }
        ConductorSM hero = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").GetHero();
        Vector2 heroPosition = hero.transform.position;
        float heroDist = ((Vector2)transform.position - heroPosition).sqrMagnitude;
        if (heroDist < _magnetDistance || _isMagnetActivated)
        {
            _isMagnetActivated = true;
            if (heroDist < 0.1f)
                return;
            if (!GetTarget().Equals(heroPosition))
                SetTarget(hero.transform.position);
        }
    }

    public bool IsMagnetTurnedOn()
    {
        return _isMagnetTurnedOn;
    }

    public void AddVortexEffect(Vector2 point, float dist)
    {
        float randomAngleInDegrees = Randomizer.GetInRange(0, 360);
        float radians = randomAngleInDegrees*Mathf.Deg2Rad;
        float finalDist = Mathf.Min(dist*0.5f, Randomizer.GetNormalizedRandom() * dist);
        float xOffset = finalDist * Mathf.Cos(radians);
        float yOffset = finalDist * Mathf.Sin(radians);
        Vector3 oldPos = transform.position;
        Vector3 newPos = new Vector3(oldPos.x + xOffset, oldPos.y + yOffset, oldPos.z);
        MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").NormalizePosition(ref newPos, true);
        StopStick();
        MakeIdle();
        transform.position = newPos;
    }

    protected override void Update()
    {
        base.Update();
        ConductorSM hero = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").GetHero();
        if (hero == null)
            return;
        CalculateIndicator();
        if(!hero.IsDragging())
            StopDrag(false);
        if (AttackTarget != null)
        {
            if (AttackTarget.CanNotInteract())
            {
                AttackTarget = null;
            }
        }
        _attackingDenyPeriod -= Time.deltaTime;
        if (_attackingDenyPeriod <= 0)
        {
            IsAttackingAllowed = true;
        }
        if (AttackTarget != null)
        {
            float dist = AttackTargetDistance();
            if (dist > AttackMaxDistance)
            {
                AttackTarget = null;
            }
        }
    }

    public void ApplyWrenchBonus(bool add)
    {
        if(!_hasTicket)
            return;
        if (add)
        {
            _savedStickProbability = StickProbability;
            StickProbability = 0;
            MakeIdle();
        }
        else
        {
            StickProbability = _savedStickProbability;
        }
    }

    protected void ParseBonusMap()
    {
        BonusProbabilities = new Dictionary<GameController.BonusTypes, float>();
        JSONObject unparsedMap = ConfigReader.GetConfig().GetField(GetClassName()).GetField("BonusMap");
        foreach (var bonus in Enum.GetValues(typeof(GameController.BonusTypes)))
        {
            string representation = bonus.ToString();
            if (unparsedMap.HasField(representation))
            {
                BonusProbabilities.Add((GameController.BonusTypes)bonus, unparsedMap.GetField(representation).n);
            }
        }
    }

    public virtual string GetClassName()
    {
        return string.Empty;
    }

}
