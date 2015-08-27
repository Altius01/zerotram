﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public class Gnome : Passenger
    {
        public override void Init()
        {
            MoveProbability = 20;
            AttackProbability = 30;
            ChangeStatePeriod = 20;
            AttackDistance = 1;
            AttackReloadPeriod = 2;
            AttackMaxDistance = 2;
            CounterAttackProbability = 70;
            Hp = 600;
            Velocity = 2;
            AttackStrength = 5;
            AttackReactionPeriod = 0.5f;
            CalculateTicket(60);
            base.Init();
        }
    }
}
