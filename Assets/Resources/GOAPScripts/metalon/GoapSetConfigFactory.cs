using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Resolver;
using CrashKonijn.Goap.Enums;

public class GoapSetConfigFactory : GoapSetFactoryBase
{
    public override IGoapSetConfig Create()
    {
        var builder = new GoapSetBuilder("GettingStartedSet");
        
        // Goals
        builder.AddGoal<KillPlayerGoal>()
            .AddCondition<hurtsPlayerWorldKey>(Comparison.GreaterThanOrEqual, 1);

        builder.AddGoal<dropoffBoxesGoal>()
            .AddCondition<boxOutsideZoenCondition>(Comparison.GreaterThan,1);
        builder.AddGoal<WanderGoal>().AddCondition<IsWandering>(Comparison.GreaterThanOrEqual,1);

            

        // Actions
        builder.AddAction<metalonMeleeAction>()
            .SetTarget<PlayerTarget>()
            .AddEffect<hurtsPlayerWorldKey>(EffectType.Increase)
            .SetBaseCost(1)
           // .AddCondition<LOSWorldKey>(Comparison.GreaterThanOrEqual,1)
            .SetInRange(3f);

        builder.AddAction<WanderAction>()
            .SetTarget<WanderTarget>()
            .AddEffect<IsWandering>(EffectType.Increase)
            .SetBaseCost(3).SetInRange(0.3f);

        // Target Sensors
        builder.AddTargetSensor<PlayerTargetSensor>()
            .SetTarget<PlayerTarget>();

        builder.AddTargetSensor<WanderTargetSensor>()
             .SetTarget<WanderTarget>();


        // World Sensors
        //builder.addworld
     //   builder.AddWorldSensor<LosSensor>().SetKey<LOSWorldKey>();
        builder.AddWorldSensor<lowOnHealthSensor>().SetKey<lowOnHealthKey>();


        return builder.Build();
    }
}