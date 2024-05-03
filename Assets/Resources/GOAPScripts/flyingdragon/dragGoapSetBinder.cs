using CrashKonijn.Goap.Behaviours;
using UnityEngine;

public class dragGoapSetBinder : MonoBehaviour {

        public string v = "SETS DRAGON GOALS";
    public void Awake() {
        var runner = FindObjectOfType<GoapRunnerBehaviour>();
        var agent = GetComponent<AgentBehaviour>();
        agent.GoapSet = runner.GetGoapSet("DragonSet");
    }
}