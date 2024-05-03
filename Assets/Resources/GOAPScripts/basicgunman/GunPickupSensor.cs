using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrashKonijn.Goap.Sensors;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;

public class GunPickupSensor : LocalTargetSensorBase
{
  // Called when the class is created.

    private GameObject gunPickup = null;
    
    public override void Created()
    {
    }

    // Called each frame. This can be used to gather data from the world before the sense method is called.
    // This can be used to gather 'base data' that is the same for all agents, and otherwise would be performed multiple times during the Sense method.
    public override void Update()
    {
    }

    // Called when the sensor needs to sense a target for a specific agent.
    public override ITarget Sense(IMonoAgent agent, IComponentReference references)
    {
        //var random = this.GetRandomPosition(agent);
        //return new PositionTarget(random);
        var pc = this.getGunPickupPos();
        return new PositionTarget(pc);
    }


    private Vector3 getGunPickupPos(){
        if(gunPickup == null){
            gunPickup = GameObject.FindWithTag("GunPickup");
        }
        return gunPickup.transform.position;  
    }
    }

