using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.Mathematics;

public class AISpawns : MonoBehaviour
{

    [SerializeField] private List<GameObject> emlist;
    [SerializeField] private List<Transform> spawnPositions;
    [SerializeField] private GameObject summonerPrefabReference;

    [SerializeField] private GameObject shooterwithmodel; 

    [SerializeField] private GameObject metalonspawn;
    [SerializeField] private GameObject dragonenemy;
    [SerializeField] private bool doMassSpawn = true;

   public  int runthismanytimes = 4;
private void  Start(){
    //emlist = new GameObject[100];
    if (doMassSpawn) StartCoroutine(SpawnEveryThirtySeconds());


}

IEnumerator SpawnEveryThirtySeconds(){
    UnityEngine.Debug.Log("Running spawn logic");
    if(runthismanytimes == -1){
      yield  return new WaitForSeconds(10000);
    }else{
        runEnemySpawnWave();
        yield return new WaitForSeconds(40);
         runthismanytimes -=1;
    }
    StartCoroutine(SpawnEveryThirtySeconds());

}

    private void runEnemySpawnWave(){
        if(runthismanytimes == 4){
            //spawn 9 metalons , 3 shooters, 2 summoners 
            for(int i = 0; i<10; i++) {
                int spawnspot = UnityEngine.Random.Range(0, spawnPositions.Count);
                GameObject reference = Instantiate(metalonspawn, spawnPositions[spawnspot].position, Quaternion.identity);
                emlist.Add(reference);

                SpawnOnNetwork(reference, "Metalon", spawnPositions[spawnspot].position, reference.transform.rotation.eulerAngles);
            }

            for(int i = 0; i<3; i++) {
                int spawnspot = 4;
                GameObject reference = Instantiate(shooterwithmodel, (spawnPositions[spawnspot].position + Vector3.up), Quaternion.identity);
                emlist.Add(reference);

                SpawnOnNetwork(reference, "Shooter", (spawnPositions[spawnspot].position + Vector3.up), reference.transform.rotation.eulerAngles);
            }

            int sp2 = UnityEngine.Random.Range(0, spawnPositions.Count);
            GameObject rr = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr);

            SpawnOnNetwork(rr, "Summoner", spawnPositions[sp2].position, rr.transform.rotation.eulerAngles);

            GameObject rr2 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr2);

            SpawnOnNetwork(rr2, "Summoner", spawnPositions[sp2].position, rr2.transform.rotation.eulerAngles);

        } else if(runthismanytimes == 3){
        //spawn 9 metalons , 3 shooters, 2 summoners 
            for(int i = 0; i<20; i++) {
                int spawnspot = UnityEngine.Random.Range(0, spawnPositions.Count);
                GameObject reference = Instantiate(metalonspawn, spawnPositions[spawnspot].position, Quaternion.identity);
                emlist.Add(reference);

                SpawnOnNetwork(reference, "Metalon", spawnPositions[spawnspot].position, reference.transform.rotation.eulerAngles);
            }

            for(int i = 0; i<6; i++) {
                int spawnspot = 4;
                GameObject reference = Instantiate(shooterwithmodel, spawnPositions[spawnspot].position, Quaternion.identity);
                emlist.Add(reference);

                SpawnOnNetwork(reference, "Shooter", spawnPositions[spawnspot].position, reference.transform.rotation.eulerAngles);
            }

            int sp2 = UnityEngine.Random.Range(0, spawnPositions.Count);
            GameObject rr = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr);
            SpawnOnNetwork(rr, "Summoner", spawnPositions[sp2].position, rr.transform.rotation.eulerAngles);

            GameObject rr3 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr3);
            SpawnOnNetwork(rr3, "Summoner", spawnPositions[sp2].position, rr3.transform.rotation.eulerAngles);

            GameObject rr2 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr2);
            SpawnOnNetwork(rr2, "Summoner", spawnPositions[sp2].position, rr2.transform.rotation.eulerAngles);

            GameObject rr4 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr4);
            SpawnOnNetwork(rr4, "Summoner", spawnPositions[sp2].position, rr4.transform.rotation.eulerAngles);
        }
        else if(runthismanytimes == 2){
        //spawn 9 metalons , 3 shooters, 2 summoners 
            for(int i = 0; i<20; i++) {
                int spawnspot = UnityEngine.Random.Range(0, spawnPositions.Count);
                GameObject reference = Instantiate(metalonspawn, spawnPositions[spawnspot].position, Quaternion.identity);
                emlist.Add(reference);

                SpawnOnNetwork(reference, "Metalon", spawnPositions[spawnspot].position, reference.transform.rotation.eulerAngles);
            }

            for(int i = 0; i<6; i++) {
                int spawnspot = 4;
                GameObject reference = Instantiate(shooterwithmodel, spawnPositions[spawnspot].position, Quaternion.identity);
                emlist.Add(reference);

                SpawnOnNetwork(reference, "Shooter", spawnPositions[spawnspot].position, reference.transform.rotation.eulerAngles);
            }

            int sp2 = UnityEngine.Random.Range(0, spawnPositions.Count);
            GameObject rr = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr);
            SpawnOnNetwork(rr, "Summoner", spawnPositions[sp2].position, rr.transform.rotation.eulerAngles);

            GameObject rr3 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr3);
            SpawnOnNetwork(rr3, "Summoner", spawnPositions[sp2].position, rr3.transform.rotation.eulerAngles);

            GameObject rr2 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr2);
            SpawnOnNetwork(rr2, "Summoner", spawnPositions[sp2].position, rr2.transform.rotation.eulerAngles);

            GameObject rr4 = Instantiate(summonerPrefabReference, spawnPositions[sp2].position, Quaternion.identity);
            emlist.Add(rr4);
            SpawnOnNetwork(rr4, "Summoner", spawnPositions[sp2].position, rr4.transform.rotation.eulerAngles);
        }
        else if(runthismanytimes == 1){

            UnityEngine.Debug.Log("Despawning time");
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach(GameObject emy in enemies){
                DestroyOnNetwork(emy.gameObject);
                Destroy(emy.gameObject,1);
            }
        }
        else if(runthismanytimes== 0){
           GameObject drag = Instantiate(dragonenemy, (new Vector3(87.7f,15f,0) ), quaternion.identity );
         SpawnOnNetwork(drag,"Dragon", new Vector3(87.7f,15f,0), drag.transform.rotation.eulerAngles);
         UnityEngine.Debug.Log("Dragon spawn");
        }

        //in this else if statement, ERASE ALL ENEMIES
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            int spawnpot = UnityEngine.Random.Range(0, spawnPositions.Count);
            if(spawnpot == 4 && doMassSpawn){
             GameObject reference = Instantiate(shooterwithmodel, spawnPositions[4].position, Quaternion.identity);
            //GameObject reference = Instantiate(summonerPrefabReference, spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)].position, Quaternion.identity);

            Transform referenceTransform = reference.transform;

            if (PersonalNetworkAgent.Instance != null) NetworkManager.Instance.AddObjectToSpawnOnNetwork(reference, "Shooter", reference.transform.position, referenceTransform.rotation.eulerAngles);

            }else{
            GameObject reference = Instantiate(summonerPrefabReference, spawnPositions[spawnpot].position, Quaternion.identity);
            //GameObject reference = Instantiate(summonerPrefabReference, spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)].position, Quaternion.identity);

            Transform referenceTransform = reference.transform;

            if (PersonalNetworkAgent.Instance != null) NetworkManager.Instance.AddObjectToSpawnOnNetwork(reference, "Summoner", reference.transform.position, referenceTransform.rotation.eulerAngles);
            }
        }
    }

    private void SpawnOnNetwork(GameObject reference, string type, Vector3 position, Vector3 rotation)
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            NetworkManager.Instance.AddObjectToSpawnOnNetwork(reference, type, position, rotation);
        }
    }

    private void DestroyOnNetwork(GameObject reference)
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            var movementScript = reference.GetComponent<MovingObjectAgentMove>();

            if (movementScript != null)
            {
                movementScript.UnsubscribeFromEvents();
                PersonalNetworkAgent.Instance.DeleteObjectOnNetwork(movementScript.networkId);
            }
        }
    }
}
