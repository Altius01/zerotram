﻿using System.Collections.Generic;
using System.Diagnostics;
using Assets;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class HealBonus : PassengerEffectBonus
{
    private float _coef;
    public override GameController.BonusTypes GetBonusType()
    {
        return GameController.BonusTypes.Heal;
    }
    
    protected override void AddEffectAfterCheck(PassengerSM passenger)
    {
        float healValue = passenger.GetInitialLifes() * _coef;
        passenger.AddDamageValue(-healValue);
    }

    protected override void RemoveEffectAfterCheck(PassengerSM passenger)
    {
    }
    
    public HealBonus()
    {
        TTL = 0;
        _coef = ConfigReader.GetConfig().GetField("healBonus").GetField("coef").n;
    }


}