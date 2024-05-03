using CrashKonijn.Goap.Behaviours;
using UnityEngine;

public class GoapSetBinder : MonoBehaviour {

    public string v = "SETS METALON GOALS";
    public void Awake() {
        var runner = FindObjectOfType<GoapRunnerBehaviour>();
        var agent = GetComponent<AgentBehaviour>();
        agent.GoapSet = runner.GetGoapSet("GettingStartedSet");
    }
}