﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    public class Hero : MovableObject
    {
        private BackgroundManager _backgroundManager;
        private Passenger _dragTarget;
        private Vector2 _dragStartPoint;
        private const float MaxDragDistance = 6f;
        private bool _isInWayoutZone;

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
            Hp = 500;
            AttackMaxDistance = 1;
            AttackReloadPeriod = 0.5f;
        }

        void FixedUpdate()
        {
            TimeSinceAttackMade += Time.fixedDeltaTime;
        }
        public void Kick(Passenger obj)
        {
            CurrentState = State.Attack;
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

        public bool IsInAttackRadius(MovableObject obj)
        {
            float sqrRemainingDistance = (transform.position - obj.transform.position).sqrMagnitude;
            bool isDistanceCorrect = sqrRemainingDistance <= AttackMaxDistance;
            Passenger ps = obj.GetComponent<Passenger>();
            if (ps != null)
            {
                if (ps.IsGoingAway)
                    return false;
            }
            return isDistanceCorrect;
        }


        public void StartDrag(Passenger obj)
        {
            if(obj.IsAlreadyDragged())
                return;
            if (obj.IsStick)
            {
                obj.StopStick();
            }
            CurrentState = State.Drag;
            obj.SetDragged(true);
            _dragTarget = obj;
            _dragStartPoint = _backgroundManager.GetCurrentMousePosition();
            CalculateOrientation(_dragStartPoint);
        }

        public void UpdatePositionForDrag()
        {
            if (_dragTarget == null)
            {
                StopDrag();
                return;
            }
            Vector2 targetPos = _backgroundManager.GetCurrentMousePosition();
            Vector2 position2D = transform.position;
            float dist = (position2D - targetPos).sqrMagnitude;
            if(dist > 0.001f)
                CalculateOrientation(targetPos);
            float currentDist = (targetPos - _dragStartPoint).sqrMagnitude;
            if (currentDist > MaxDragDistance)
            {
                StopDrag();
                return;
            }
            transform.position = targetPos;
            _dragTarget.transform.position = targetPos;
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

        void OnMouseUp()
        {
            StopDrag();
        }
    }
}
