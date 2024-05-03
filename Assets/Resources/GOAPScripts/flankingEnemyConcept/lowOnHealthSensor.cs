using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Sensors;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;

public class lowOnHealthSensor : LocalWorldSensorBase
{
   public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
        {
            // References are cached by the agent.
            var personalHP = references.GetCachedComponent<dewAgentHealth>();

            if (personalHP == null)
                return false;
            
            return personalHP.hp <= 2;
            //return true; 
        }
}
