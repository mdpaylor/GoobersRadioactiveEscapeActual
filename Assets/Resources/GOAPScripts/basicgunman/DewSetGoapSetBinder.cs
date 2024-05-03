using CrashKonijn.Goap.Behaviours;
using UnityEngine;

public class DewSetGoapSetBinder : MonoBehaviour {

        public string v = "SETS SHOOTER GOALS";
    public void Awake() {
        var runner = FindObjectOfType<GoapRunnerBehaviour>();
        var agent = GetComponent<AgentBehaviour>();
        agent.GoapSet = runner.GetGoapSet("DewSet");
    }
}