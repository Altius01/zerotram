﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class Hero : MovableObject
    {
        private BackgroundManager _backgroundManager;
        private Passenger _dragTarget;
        private Vector2 _dragStartPoint;
        private Vector2 _dragOffset;
        private float _maxDragDistance;
        private bool _isInWayoutZone;
        private Text _lifes;
        private AudioPlayer _player;

        public void SetInWayoutZone(bool inZone)
        {
            _isInWayoutZone = inZone;
        }

        public bool IsInWayoutZone()
        {
            return _isInWayoutZone;
        }

        void Awake()
        {
            _maxDragDistance = ConfigReader.GetConfig().GetField("hero").GetField("MaxDragDistance").n;
            Hp = InitialLifes = ConfigReader.GetConfig().GetField("hero").GetField("InitialLifes").n;
            AttackMaxDistance = ConfigReader.GetConfig().GetField("hero").GetField("AttackMaxDistance").n;
            AttackReloadPeriod = ConfigReader.GetConfig().GetField("hero").GetField("AttackReloadPeriod").n;
            Velocity = ConfigReader.GetConfig().GetField("hero").GetField("Velocity").n;
            AttackReactionPeriod = ConfigReader.GetConfig().GetField("hero").GetField("AttackReactionPeriod").n;
            AttackReloadPeriod = ConfigReader.GetConfig().GetField("hero").GetField("AttackReloadPeriod").n;
            _lifes = GameObject.Find("userLifes").GetComponent<Text>();
            _lifes.text = "100%";
            _player = GameObject.Find("AudioPlayer").GetComponent<AudioPlayer>();
        }

        public override void AddDamage(MovableObject attacker)
        {
            base.AddDamage(attacker);
            int lifesPercent = Mathf.RoundToInt(100*(Hp/(float) InitialLifes));
            _lifes.text = lifesPercent + "%";
        }

        void FixedUpdate()
        {
            TimeSinceAttackMade += Time.fixedDeltaTime;
        }
        public void Kick(Passenger obj)
        {
            _player.PlayAudioById("kick");
            CalculateOrientation(obj.GetPosition());
            CurrentState = State.Attack;
            if (obj.IsStick)
            {
                obj.StopStick();
                obj.WasStickWhenFlyAway = true;
            }
            obj.FlyAway();
            TimeSinceAttackMade = 0;
            AttackTarget = null;
            _dragTarget = null;
        }

        protected new void Start()
        {
            base.Start();
            _backgroundManager = GameObject.Find("background").GetComponent<BackgroundManager>();
        }

        public bool IsInAttackRadius(Passenger obj)
        {
            Vector2 position2D = GetPosition();
            Vector2 passengerPosition2D = obj.GetPosition();
            float sqrRemainingDistance = (position2D - passengerPosition2D).sqrMagnitude;
            bool isDistanceCorrect = sqrRemainingDistance <= AttackMaxDistance;
            return isDistanceCorrect;
        }


        public void StartDrag(Passenger obj)
        {
            if (obj.IsStick)
            {
                obj.StopStick();
            }
            CurrentState = State.Drag;
            obj.SetDragged(true);
            _dragTarget = obj;
            _dragStartPoint = _backgroundManager.GetCurrentMousePosition();
            _dragOffset = transform.position - obj.transform.position;
            CalculateOrientation(_dragStartPoint);
        }

        public void UpdatePositionForDrag()
        {
            if (_dragTarget == null || !Input.GetMouseButton(0))
            {
                StopDrag();
                return;
            }
            Vector2 targetPos = _backgroundManager.GetCurrentMousePosition();
            float currentDist = (targetPos - _dragStartPoint).sqrMagnitude;
            if (currentDist > _maxDragDistance)
            {
                StopDrag();
                return;
            }
            CalculateOrientation(targetPos + _dragOffset);
            SetPosition(targetPos + _dragOffset);
            _dragTarget.SetPosition(targetPos);
        }

        public bool IsUnderAttack()
        {
            return CurrentState == State.Attacked;
        }

        public bool IsDragging()
        {
            return CurrentState == State.Drag;
        }

        public void StopDrag()
        {
            if (CurrentState == State.Drag)
            {
                CurrentState = State.Idle;
                if (_dragTarget != null)
                {
                    _dragTarget.SetDragged(false);
                }   
            }
        }

        public void HandleClick()
        {
            if (_dragTarget != null && _dragOffset.sqrMagnitude < AttackMaxDistance)
            {
                _dragTarget.HandleClick();
            }
        }

        public override bool CanBeAttacked()
        {
            return true;
        }
    }
}
