using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
public class localsensorcansummon : LocalWorldSensorBase
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
            var selfsummon = references.GetCachedComponent<summonBehavior>();

            if (selfsummon == null)
                return false;

            return (selfsummon != null); 
            //return true; 
        }
}
