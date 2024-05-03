/* File serves as the "semi-static" manager for the networking involved in multiplayer. This file is less technical
 * in terms of the data being sent, received, etc. It is more focused on being the holder of data that is consistent
 * with the network.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    // Is invoked when the correct time has passed to send data to the server
    public event EventHandler OnFixedNetworkUpdateEvent;

    // Holds the collection of player objects that have moved since the last server update
    private Dictionary<int, NetworkTypes.NetworkPlayerToMove> changedPlayerObjects;

    // Holds the collection of enemy objects that have moved since the last server update
    private List<NetworkTypes.NetworkEnemyToMove> changedEnemyObjects;

    // Holds the collection of objects that have shot since the last update
    private List<NetworkTypes.ObjectThatFiredBullet> objectsThatFiredBullet;

    // Holds the collection of objects to spawn on the network
    private List<NetworkTypes.PrefabSpawnLocal> objectsToSpawn;

    // Holds the collection of objects that have taken damage since the last update
    private NetworkTypes.AIDamagedList aiThatTookDamage;

    // Holds the collection of objects that have changed their animation state
    private NetworkTypes.AIAnimationList aiThatChangedAnimations;

    // Holds the collection of objects that have activated or deactivated particles
    private NetworkTypes.AIParticleList aiThatChangedParticles;

    // Holds the correlation of networkID to gameobject, actual network position, and actual network rotation
    // in the current scene. This enables easy access of gameObjects to move them, access scripts, etc.
    private Dictionary<int, NetworkTypes.NetworkObject> networkObjectMap;

    private int connectedPlayers = 0;

    [SerializedDictionary("Prefab Name", "Prefab Reference")]
    public SerializedDictionary<string, GameObject> dummyPrefabReferences;
    [SerializedDictionary("Prefab Name", "Prefab Reference")]
    public SerializedDictionary<string, GameObject> functionalPrefabReferences;
    public Transform playerSpawnPosition;
    public float passedTime = 0f;
    public float finalNetworkTime = .0166f;

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        changedPlayerObjects = new Dictionary<int, NetworkTypes.NetworkPlayerToMove>();
        changedEnemyObjects = new List<NetworkTypes.NetworkEnemyToMove>();
        objectsThatFiredBullet = new List<NetworkTypes.ObjectThatFiredBullet>();
        objectsToSpawn = new List<NetworkTypes.PrefabSpawnLocal>();
        aiThatTookDamage = new NetworkTypes.AIDamagedList
        {
            list = new List<NetworkTypes.AIThatTookDamage>()
        };
        aiThatChangedAnimations = new NetworkTypes.AIAnimationList
        {
            list = new List<NetworkTypes.AnimationJson>()
        };
        aiThatChangedParticles = new NetworkTypes.AIParticleList
        {
            list = new List<NetworkTypes.ParticleJson>()
        };
        networkObjectMap = new Dictionary<int, NetworkTypes.NetworkObject>();
    }

    void Update()
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected) passedTime += Time.deltaTime;
        if (passedTime >= finalNetworkTime) // Network Update
        {
            OnFixedNetworkUpdateEvent?.Invoke(this, EventArgs.Empty);
            passedTime -= finalNetworkTime;
        }
    }

    // Adds a player object that has changed its position
    public void AddChangedPlayerObject(int id, string type, Vector3 velocityVector, Vector3 positionVector, Vector3 rotationVector, float speed, float weaponRotation)
    {
        if (changedPlayerObjects.ContainsKey(id))
        {
            changedPlayerObjects[id] = new NetworkTypes.NetworkPlayerToMove(id, type, weaponRotation, velocityVector, positionVector, rotationVector, speed);
        }
        else
        {
            changedPlayerObjects.Add(id, new NetworkTypes.NetworkPlayerToMove(id, type, weaponRotation, velocityVector, positionVector, rotationVector, speed));
        }
    }

    // Adds an enemy that has changed its position
    public void AddChangedEnemyObject(int id, int targetId, Vector3 positionVector, Vector3 rotationVector, bool shouldMove)
    {
        changedEnemyObjects.Add(new NetworkTypes.NetworkEnemyToMove(id, targetId, positionVector, rotationVector, shouldMove));
    }

    // Adds an Enemy or Player object that shot a bullet
    public void AddObjectThatFiredBullet(int id, Vector3 shootingDir, string type)
    {
        var shootingVec = new NetworkTypes.DeserializableVector3
        {
            x = shootingDir.x,
            y = shootingDir.y,
            z = shootingDir.z
        };

        objectsThatFiredBullet.Add(new NetworkTypes.ObjectThatFiredBullet
        {
            networkId = id,
            shootingVector = shootingVec,
            type = type
        });
    }

    // Adds an object that needs to be spawned
    public void AddObjectToSpawnOnNetwork(GameObject reference, string prefabName, Vector3 positionVector, Vector3 rotationVector)
    {
        NetworkTypes.DeserializableVector3 position = new NetworkTypes.DeserializableVector3
        {
            x = positionVector.x,
            y = positionVector.y,
            z = positionVector.z
        };
        NetworkTypes.DeserializableVector3 rotation = new NetworkTypes.DeserializableVector3
        {
            x = rotationVector.x,
            y = rotationVector.y,
            z = rotationVector.z
        };

        objectsToSpawn.Add(new NetworkTypes.PrefabSpawnLocal(reference, prefabName, position, rotation));
    }

    // Adds an Enmey object that took damage
    public void AddEnemyThatTookDamage(int id, string weaponString, int playerId)
    {
        aiThatTookDamage.list.Add(new NetworkTypes.AIThatTookDamage
        {
            networkId = id,
            weaponString = weaponString,
            playerId = playerId
        });
    }

    // Adds an Enmey object that changed animations
    public void AddEnemyThatChangedAnimationState(int id, string stateName)
    {
        aiThatChangedAnimations.list.Add(new NetworkTypes.AnimationJson
        {
            networkId = id,
            animationName = stateName
        });
    }

    // Adds an Enmey object that used their particle system
    public void AddEnemyThatUsedParticleSystem(int id, bool condition)
    {
        aiThatChangedParticles.list.Add(new NetworkTypes.ParticleJson
        {
            networkId = id,
            condition = condition
        });
    }

    // Returns the dictionary of player objects that have changed their position
    public Dictionary<int, NetworkTypes.NetworkPlayerToMove> GetChangedPlayerObjects()
    {
        return changedPlayerObjects;
    }

    // Returns the list of enemy objects that have changed their position
    public List<NetworkTypes.NetworkEnemyToMove> GetChangedEnemyObjects()
    {
        return changedEnemyObjects;
    }

    // Returns the list of objects that have fired a bullet
    public List<NetworkTypes.ObjectThatFiredBullet> GetObjectsThatFiredBullet()
    {
        return objectsThatFiredBullet;
    }

    // Returns the list of objects that need to be spawned on the network
    public List<NetworkTypes.PrefabSpawnLocal> GetObjectsToSpawn()
    {
        return objectsToSpawn;
    }

    // Returns the object that holds the list of enemies that have taken damage
    public NetworkTypes.AIDamagedList GetAIThatTookDamage()
    {
        return aiThatTookDamage;
    }

    // Returns the object that holds the list of enemies that have changed animations
    public NetworkTypes.AIAnimationList GetAIThatChangedAnimation()
    {
        return aiThatChangedAnimations;
    }

    // Returns the object that holds the list of enemies that have used their particle system
    public NetworkTypes.AIParticleList GetAIThatUsedParticleSystem()
    {
        return aiThatChangedParticles;
    }

    // Adds a value to the network object map
    public int AddValueToNetworkMap(int id, GameObject networkObject, string type, Vector3 position, Vector3 rotation)
    {
        int newId = id;
        while (networkObjectMap.ContainsKey(newId)) newId++;

        if (newId != id && (id > -1))
        {
            Debug.Log("Correcting Id - newId: " + newId + "; OldId: " + id);

            foreach (var pair in networkObjectMap)
            {
                Debug.Log("'Key: " + pair.Key + "; Value: " + pair.Value);
            }

            PersonalNetworkAgent.Instance.CorrectIdAssignment(id, newId);
        }

        networkObjectMap.Add(newId, new NetworkTypes.NetworkObject(networkObject, type, position, rotation));

        return newId;
    }

    // Returns the gameObject reference that corresponds to a specific ID
    public NetworkTypes.NetworkObject FindGameObjectInNetworkMap(int id)
    {
         NetworkTypes.NetworkObject foundObject = null;

        if (networkObjectMap.ContainsKey(id)) foundObject = networkObjectMap[id];

        return foundObject;
    }

    // Checks to see if a given ID is in the network object map
    public bool HasObjectInMapWithId(int id)
    {
        if (networkObjectMap.ContainsKey(id)) return true;
        return false;
    }

    // Removes an object from the network object map
    public void RemoveObjectFromNetworkMap(int id)
    {
        if (networkObjectMap.ContainsKey(id))
        {
            networkObjectMap.Remove(id);
        }
    }

    // Returns the network object map
    public Dictionary<int, NetworkTypes.NetworkObject> GetNetworkObjectMap()
    {
        return networkObjectMap;
    }

    // Clears the network object map
    public void ClearNetworkMap()
    {
        networkObjectMap.Clear();
    }

    // Clears the list of objects that have shot a bullet
    public void ClearObjectsThatShotList()
    {
        objectsThatFiredBullet.Clear();
    }

    public void ClearObjectsToSpawn()
    {
        objectsToSpawn.Clear();
    }

    // Clears the dictionary of players that have moved locally
    public void ClearChangedPlayerObjects()
    {
        changedPlayerObjects.Clear();
    }

    // Clears the list of enemies that have moved locally
    public void ClearChangedEnemyObjects()
    {
        changedEnemyObjects.Clear();
    }

    // Clears the list of enemies that have taken damage
    public void ClearAIThatTookDamage()
    {
        aiThatTookDamage.list.Clear();
    }

    // Clears the list of enemies that changed animations
    public void ClearAIThatChangedAnimation()
    {
        aiThatChangedAnimations.list.Clear();
    }

    // Clears the list of enemies that have used their particle system
    public void ClearAIThatUsedParticleSystem()
    {
        aiThatChangedParticles.list.Clear();
    }

    public void IncrementConnectedPlayers()
    {
        connectedPlayers++;
    }

    public void DecrementConnectedPlayers()
    {
        connectedPlayers--;
    }

    // Returns the amount of connected players on the network
    public int GetConnectedPlayers()
    {
        return connectedPlayers;
    }

    // Sets the number of connected players on the network
    public void SetConnectedPlayers(int connectedPlayers)
    {
        this.connectedPlayers = connectedPlayers;
    }
}
