using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;

public class WanderTargetSensor : LocalTargetSensorBase
{
    // Called when the class is created.
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
        var random = this.GetRandomPosition(agent);
        

        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //  cube.transform.position = random; 
       // var box = cube.GetComponent<BoxCollider>();
  //      box.enabled = false;
        return new PositionTarget(random);

        
    }

    private Vector3 GetRandomPosition(IMonoAgent agent)
    {
        var random =  Random.insideUnitCircle * 2f;
        var position = agent.transform.position + new Vector3(random.x, 0f, random.y);
        
        if(SomethingInTheWay(agent, position)){
            return tryThisAgain(agent);
        }else{
        return position;
        }
    }
    private Vector3 tryThisAgain(IMonoAgent agent)
    {
    
       // var random =  Random.insideUnitCircle * 1f;
        Vector3 position = agent.transform.position;
    

        return position;
    }

    bool SomethingInTheWay(IMonoAgent agent, Vector3 dest){

        Vector3 source = agent.transform.position; 
           
    //bool obj_hit = false;

    Ray ry = new Ray ();
    ry.origin = source;
    ry.direction = dest;

    if (Physics.Raycast(ry, 5f)){
            //Debug.Log("There is something in front of the object!");
            return true;
    }else{
        return false;
    }
    

    /**
        if((Raycast hit = Physics.Raycast(ry, dest.magnitude))){
        //Debug.DrawRay(source, dest, Color.cyan, 4.0f);
             return true; 
        }else{
             return false;
        }
       
    
       return false;
    */
    
}
}
