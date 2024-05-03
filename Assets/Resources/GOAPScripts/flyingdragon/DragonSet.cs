using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Resolver;
using UnityEngine;



 

public class DragonSet : GoapSetFactoryBase
{
    public override IGoapSetConfig Create()
    {
        var builder = new GoapSetBuilder("DragonSet");
        
        // Goals
     //builder.AddGoal<WanderGoal>().AddCondition<IsWandering>(Comparison.GreaterThanOrEqual,1);
        builder.AddGoal<KillPlayerGoal>().AddCondition<hurtsPlayerWorldKey>(Comparison.GreaterThanOrEqual,1);
        builder.AddGoal<preserveLifeGoal>().AddCondition<lowOnHealthKey>(Comparison.GreaterThanOrEqual,1).AddCondition<hurtsPlayerWorldKey>(Comparison.GreaterThanOrEqual,1);
       // builder.AddGoal<strafeGoal>().AddCondition<takenlotsofDamageKey>(Comarison.GreaterThanOrEqual,1);
     //   builder.AddGoal<SummonMonsterGoal>().AddCondition<cansummonWorldKey>(Comparison.GreaterThanOrEqual,1).AddCondition<hurtsPlayerWorldKey>(Comparison.GreaterThanOrEqual,1); 

       // Actions

       //assigning actions 
       //every action has conditions to be executable 
       //every action has goals to accomplish
       //AI chooses which cation to take 
        builder.AddAction<WanderAction>()
            .SetTarget<WanderTarget>()
            .AddEffect<IsWandering>(EffectType.Increase)
            .SetBaseCost(3).SetInRange(0.3f);

    builder.AddAction<fireAction>()
       .SetTarget<PlayerTargetTarget>()
        .SetBaseCost(1)
        .SetInRange(90f)
        //.SetInRange(30f).AddCondition<HasGunCondition>(Comparison.GreaterThanOrEqual,1)
      //  .AddCondition<LOSWorldKey>(Comparison.GreaterThanOrEqual,1)
       .AddEffect<hurtsPlayerWorldKey>(EffectType.Increase);
       
       

    //     builder.AddAction<flankTargetAction>()
    //     .SetTarget<PlayerTargetTarget>()
    //     .SetBaseCost(0)
    //     .SetInRange(30f)
    //    // .AddCondition<HasGunCondition>(Comparison.GreaterThanOrEqual,1)
    //    // .AddCondition<LOSWorldKey>(Comparison.GreaterThanOrEqual,1)
    //    .AddEffect<hurtsPlayerWorldKey>(EffectType.Increase)
    //    .AddCondition<lowOnHealthKey>(Comparison.GreaterThanOrEqual,1);
    //    ;



        //TODO 1/6/2024
        //1. finish up the action to allow this enemy to summon another one 
        //2. do  we want to do enemy amount caps? i can go either way tbh 
        
        //update 1/7 
        //i can actually use the exact same parameters for the summon that i can use for the gun - because the bot doesnt have the gun condition, they cannot shoot 
        //i can use the cansummonworldkey the same way i use the has gun condition
        //i probably need a "can summon condition" script too 

        // // Target Sensors

        
      //  builder.AddTargetSensor<WanderTargetSensor>()
           //  .SetTarget<WanderTarget>();

      builder.AddTargetSensor<GunPickupSensor>().SetTarget<GunPickupTarget>();
      builder.AddTargetSensor<PlayerTargetSensor>().SetTarget<PlayerTargetTarget>();
        builder.AddTargetSensor<flankspotTargetSensor>().SetTarget<flankspotTarget>(); 
        // // World Sensors
        builder.AddWorldSensor<HasGunSensor>().SetKey<HasGunCondition>();
        
        builder.AddWorldSensor<lowOnHealthSensor>().SetKey<lowOnHealthKey>();
        

        return builder.Build();
    }
}