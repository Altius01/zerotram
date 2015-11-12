﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Assets.Scripts.Math;

public class MovableCharacterSM : StateMachine
{
    [SerializeField] private SpriteRenderer _lifebar;

    public float Velocity;
    public Animator Animator;
    public Rigidbody2D CharacterBody;
    public BoxCollider2D BoxCollider2D;
    protected bool IsDead;
    protected float Hp;
    public float AttackMaxDistance = 3;
    protected float AttackedStartTime;
    private Vector2 _target;
    protected float AttackStrength = 10;
    public MovableCharacterSM AttackTarget;
    protected float AttackReactionPeriod = 0.5f;
    public float AttackReloadPeriod = 0.5f;
    protected float InitialLifes;
    public bool IsGoingAway;
    public float AttackDistance = 1;
    public float TimeSincePreviousClickMade;
    private bool _isFreezeInProgress;
    private bool _isFreezeTemporalyDisabled;

    public const float MaxClickDuration = 0.6f;

    public enum MovableCharacterStates
    {
        Idle = 0,
        Move = 1,
        Drag = 2,
        Attack = 3,
        Attacked = 4,
        Stick = 5,
        FlyingAway = 6,
        Dragged = 7,
        Frozen = 8
    }

    public float GetInitialLifes()
    {
        return InitialLifes;
    }
    
    public Vector2 GetTarget()
    {
        return _target;
    }
    public void SetTarget(Vector2 target)
    {
        _target = target;
        CalculateOrientation(target);
        ActivateState((int)MovableCharacterStates.Move);
    }

    public void AddDamageValue(float damage)
    {
        Hp -= damage;
        float lifesPercent = Hp / (float)InitialLifes;
        float originalValue = _lifebar.bounds.min.x;
        _lifebar.transform.localScale = new Vector3(lifesPercent, 1, 1);
        float newValue = _lifebar.bounds.min.x;
        float difference = newValue - originalValue;
        _lifebar.transform.Translate(new Vector3(-difference, 0, 0));
        if (lifesPercent > 0.5f)
        {
            _lifebar.color = Color.green;
        }
        if (lifesPercent < 0.5f && lifesPercent > 0.1f)
        {
            _lifebar.color = Color.yellow;
        }
        if (lifesPercent < 0.1f)
        {
            _lifebar.color = Color.red;
        }
        if (Hp <= 0)
        {
            Hp = 0;
            IsDead = true;
            GameController.GetInstance().RegisterDeath(this);
            Destroy(this.gameObject);
        }
    }

    public virtual void AddDamage(MovableCharacterSM attacker)
    {
        AttackedStartTime = Time.time;
        ActivateState((int)MovableCharacterStates.Attacked);
        if (attacker.AttackStrength < 0)
            MonobehaviorHandler.GetMonobeharior().GetObject<AudioPlayer>("AudioPlayer").PlayAudioById("heal");
        if (attacker.AttackStrength < 0 && Hp >= InitialLifes)
        {
            Hp = InitialLifes;
            return;
        }
        if (attacker.AttackStrength > 0)
            MonobehaviorHandler.GetMonobeharior().GetObject<AudioPlayer>("AudioPlayer").PlayAudioById("lowkick");
        float currentStrength = attacker.AttackStrength*Randomizer.GetNormalizedRandom();
        AttackTarget = attacker;
        AddDamageValue(currentStrength);
    }

    public bool IsAttackReationFinished()
    {
        return Time.time - AttackedStartTime > AttackReactionPeriod;
    }

    public float GetTargetDistance()
    {
        return ((Vector2) transform.position - GetTarget()).sqrMagnitude;
    }

    public bool IsInAttackRadius(Vector2 pos)
    {
        float sqrRemainingDistance = ((Vector2)transform.position - pos).sqrMagnitude;
        return sqrRemainingDistance <= AttackDistance;
    }

    public void MakeAttack()
    {
        AttackTarget.AddDamage(this);
    }

    public void CalculateOrientation(Vector2 target)
    {
        if (target.x > transform.position.x)
        {
            CharacterBody.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            CharacterBody.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void MakeIdle()
    {
        ActivateState((int)MovableCharacterSM.MovableCharacterStates.Idle);
    }

    public virtual bool CanNotInteract()
    {
        return false;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        TimeSincePreviousClickMade += Time.fixedDeltaTime;
    }

    public virtual void HandleClick()
    {   
    }

    public virtual void HandleDoubleClick()
    {
    }

    public void Freeze()
    {
        _isFreezeInProgress = true;
        ActivateState((int)MovableCharacterStates.Frozen);
    }

    public void UnFreeze()
    {
        _isFreezeInProgress = false;
        _isFreezeTemporalyDisabled = false;
        Animator.enabled = true;
        MakeIdle();
    }

    public void TemporalyUnfreeze()
    {
        _isFreezeInProgress = false;
        _isFreezeTemporalyDisabled = true;
    }

    public bool IsUnfreezeIsTemporary()
    {
        return _isFreezeTemporalyDisabled;
    }

    public bool IsFrozen()
    {
        return _isFreezeInProgress;
    }
}
