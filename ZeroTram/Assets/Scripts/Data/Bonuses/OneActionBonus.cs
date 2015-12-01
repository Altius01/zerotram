﻿using System.Collections.Generic;
using System.Diagnostics;
using Assets;
using UnityEngine;

public abstract class OneActionBonus : PointBonus
{
    protected float Distance;
    
    public override void HandleTouchUp(Vector2 point)
    {
        if (IsFired)
            return;
        IsFired = true;
        StartPoint = point;
        if (IsPassengersAffected)
        {
            List<PassengerSM> passengersNear = GameController.GetInstance().AllPassengersInDist(point, Distance);
            foreach (var passengerSm in passengersNear)
            {
                AddEffectToPassenger(passengerSm);
            }
        }
        if (IsConductorAffected)
        {
            ConductorSM hero = MonobehaviorHandler.GetMonobeharior().GetObject<Floor>("Floor").GetHero();
            float dist =
                ((Vector2)hero.transform.position -
                 point).sqrMagnitude;
            if (dist <= Distance)
            {
                AddEffectToConductor(hero);
            }
        }
        Deactivate();
    }

    public override void Activate()
    {
    }

    protected virtual void AddEffectToPassenger(PassengerSM passenger)
    {

    }

    protected override void AddEffectAfterCheck(PassengerSM passenger)
    {
        
    }

    protected override void RemoveEffectAfterCheck(PassengerSM passenger)
    {
        
    }
}