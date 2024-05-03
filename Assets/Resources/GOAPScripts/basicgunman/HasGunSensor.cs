using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Sensors;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;

public class HasGunSensor : LocalWorldSensorBase
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
            var gunBehaviour = references.GetCachedComponent<currGunBehaviour>();

            if (gunBehaviour == null)
                return false;

            //different gun logic for each gun potentially? So here we would have IDs for different types of guns
            //sounds complicated. lets have every enemy have color coded array
            //


            //core problem: currgunbehavior
            return gunBehaviour.gunHP > 0;
            //return true; 
        }
}
