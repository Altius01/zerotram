﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public class Gnome : Passenger
    {
        protected override void Init()
        {
            MoveProbability = 20;
            AttackProbability = 30;
            ChangeStatePeriod = 20;
            AttackDistance = 0.5f;
            AttackReloadPeriod = 2;
            AttackMaxDistance = 2;
            CounterAttackProbability = 70;
            Hp = 200;
            Velocity = 2;
            AttackStrength = 10;
            AttackReactionPeriod = 1;
            CalculateTicket(60);
        }
    }
}
