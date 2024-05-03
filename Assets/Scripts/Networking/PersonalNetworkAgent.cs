/* File serves as the personal helper for everything involving multiplayer. This file is mostly 
 * client specific in terms of the data that it contains. includes the socket connection and all 
 * of logic that handles sending and receiving data on the socket from the multiplayer server.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Newtonsoft.Json;
using System;

public class PersonalNetworkAgent : MonoBehaviour
{
    public static PersonalNetworkAgent Instance { get; private set; }

    // Event is to allow AI to look at a different enemy when a player object is deleted
    public event EventHandler OnPlayerDisconnectedEvent;
    public event EventHandler OnPlayerConnectedEvent;
    public event EventHandler OnNewConnectionEvent;

    public bool isNetworkHost = false;
    public bool isConnected = false;
    public GameObject playerPrefab;

    [SerializeField] private GameObject aiSpawnPointsReference;
    [SerializeField] private GameObject goapObjectReference;

    private movement playerScript = null;
    private SocketIOUnity currentSocket;
    private int userNetworkId = -1;
    private float socketRefreshTime = 0f;
    private float finalSocketRefreshTime = 270f;
    private bool doHostMigrationToSelf = false;
    private bool isLoadingScene;
    private bool doSceneActivation = false;
    private bool doConnectionEventInvoke = false;

    // Server Links
    //private const string serverUrlLink = "http://localhost:3000"; // Currently used computer
    private const string serverUrlLink = "https://gameserver-bxhk27adgq-ue.a.run.app/"; // Live server - Use this one!
    //private const string serverUrlLink = "https://gameservertest-bxhk27adgq-ue.a.run.app"; // Test Server

    private HashSet<int> omittedIdsThatAreNotInScene;

    // These collections are here because some Unity computations/processes can only be done on the main thread
    private List<NetworkTypes.PlayerToMove> remainingPlayerObjectsToMove;
    private List<NetworkTypes.EnemyToMove> remainingEnemyObjectsToMove;
    private List<NetworkTypes.ObjectToSpawn> remainingObjectsToSpawn;
    private List<NetworkTypes.PlayerToStop> remainingPlayersToStop;
    private Queue<NetworkTypes.ObjectToAssignId> remainingObjectsToAssignIds;
    private Queue<NetworkTypes.ObjectToAssignId> remainingObjectsWithNoId;
    private Queue<GameObject> remainingObjectsToDelete;
    private Queue<NetworkTypes.NetworkObjectReferenceToJump> remainingObjectsToJump;
    private Queue<NetworkTypes.NetworkObjectReferenceToShoot> remainingObjectsToShoot;
    private Queue<NetworkTypes.AIThatTookDamage> remainingEnemiesToDamage;
    private Queue<NetworkTypes.ObjectToChangeAnimation> remainingAnimationsToActivate;
    private Queue<NetworkTypes.ObjectToUseParticles> remainingParticlesToUtilize;
    private Queue<GameObject> remainingPlayersToDamage;
    private Queue<GameObject> remainingPlayersToKill;
    private Queue<NetworkTypes.WeaponSwapJson> remainingPlayersToSwapWeapons;

    private void Awake()
    {
        Instance = this;    
    }

    void Start()
    {
        NetworkManager.Instance.OnFixedNetworkUpdateEvent += NetworkManager_OnFixedNetworkUpdate;

        remainingPlayerObjectsToMove = new List<NetworkTypes.PlayerToMove>();
        remainingEnemyObjectsToMove = new List<NetworkTypes.EnemyToMove>();
        remainingObjectsToSpawn = new List<NetworkTypes.ObjectToSpawn>();
        remainingPlayersToStop = new List<NetworkTypes.PlayerToStop>();
        remainingObjectsToAssignIds = new Queue<NetworkTypes.ObjectToAssignId>();
        remainingObjectsWithNoId = new Queue<NetworkTypes.ObjectToAssignId>();
        remainingObjectsToDelete = new Queue<GameObject>();
        remainingObjectsToJump = new Queue<NetworkTypes.NetworkObjectReferenceToJump>();
        remainingObjectsToShoot = new Queue<NetworkTypes.NetworkObjectReferenceToShoot>();
        remainingEnemiesToDamage = new Queue<NetworkTypes.AIThatTookDamage>();
        remainingAnimationsToActivate = new Queue<NetworkTypes.ObjectToChangeAnimation>();
        remainingParticlesToUtilize = new Queue<NetworkTypes.ObjectToUseParticles>();
        remainingPlayersToDamage = new Queue<GameObject>();
        remainingPlayersToKill = new Queue<GameObject>();
        remainingPlayersToSwapWeapons = new Queue<NetworkTypes.WeaponSwapJson>();

        omittedIdsThatAreNotInScene = new HashSet<int>();

        // Initialize both player and goap in the active scene
        if (NetworkManager.Instance != null && NetworkManager.Instance.playerSpawnPosition != null)
        {
            playerScript = Instantiate(playerPrefab, NetworkManager.Instance.playerSpawnPosition.position, Quaternion.identity).GetComponent<movement>();
        }
        else
        {
            playerScript = Instantiate(playerPrefab).GetComponent<movement>();
        }
        //Instantiate(NetworkManager.Instance.functionalPrefabReferences["GOAP"]);

        // ID is -200 for later retrieval once the server responds with the correct ID
        NetworkManager.Instance.AddValueToNetworkMap(-200, playerScript.gameObject, "Player", playerScript.transform.position, playerScript.transform.rotation.eulerAngles);

        OnPlayerDisconnectedEvent += PersonalNetworkAgent_OnPlayerDisconnected;

        StartNewSocket();
    }

    // Simply edits the player count
    private void PersonalNetworkAgent_OnPlayerDisconnected(object sender, EventArgs e)
    {
        NetworkManager.Instance.DecrementConnectedPlayers();
    }

    // An event from the network manager which is the fixed server update. This sends the objects that have shot
    private void NetworkManager_OnFixedNetworkUpdate(object sender, EventArgs e)
    {
        ReportObjectsThatShot();
    }

    void Update()
    {
        // The "remaining*" collections are used to execute things that can only be done in the main thread
        if (remainingObjectsToAssignIds.Count > 0) SetRemainingNetworkIds();

        if (remainingObjectsToSpawn.Count > 0) SpawnRemainingObjects();

        if (remainingPlayerObjectsToMove.Count > 0) MoveRemainingPlayerObjects();

        if (remainingEnemyObjectsToMove.Count > 0) SetRemainingEnemyObjectsData();

        if (remainingPlayersToStop.Count > 0) StopRemainingPlayerObjects();

        if (remainingObjectsToDelete.Count > 0) DeleteRemainingObjects();

        if (remainingObjectsToJump.Count > 0) EnableRemainingObjectsToJump();

        if (remainingObjectsToShoot.Count > 0) EnableRemainingObjectsToShoot();

        if (remainingEnemiesToDamage.Count > 0) DamageRemainingEnemies();

        if (remainingAnimationsToActivate.Count > 0) ActivateRemainingAnimations();

        if (remainingParticlesToUtilize.Count > 0) UtilizeRemainingParticles();

        if (remainingPlayersToDamage.Count > 0) DamageRemainingPlayers();

        if (remainingPlayersToKill.Count > 0) KillRemainingPlayers();

        if (remainingPlayersToSwapWeapons.Count > 0) SwapRemainingWeapons();

        // Deals with the data that needs to be reported to the server
        if (NetworkManager.Instance.GetChangedPlayerObjects().Count > 0) UpdateNetworkPlayerPositions();

        if (NetworkManager.Instance.GetChangedEnemyObjects().Count > 0) UpdateNetworkEnemyPositions();

        if (NetworkManager.Instance.GetAIThatTookDamage().list.Count > 0) UpdateShotEnemies();

        if (NetworkManager.Instance.GetAIThatChangedAnimation().list.Count > 0) UpdateEnemyAnimations();

        if (NetworkManager.Instance.GetAIThatUsedParticleSystem().list.Count > 0) UpdateEnemyParticles();

        if (NetworkManager.Instance.GetObjectsToSpawn().Count > 0) UpdateObjectsToSpawn();

        // After 5 minutes the connection to the server stops. This is a timer for less than 5 minutes
        socketRefreshTime += Time.deltaTime;
        if (socketRefreshTime >= finalSocketRefreshTime)
        {
            socketRefreshTime -= finalSocketRefreshTime;

            RefreshSocketConnection();
        }

        if (doHostMigrationToSelf) MigrateHostToSelf();

        if (doSceneActivation)
        {
            goapObjectReference.SetActive(true);

            if (isNetworkHost) aiSpawnPointsReference.SetActive(true);

            doSceneActivation = false;
        }

        if (doConnectionEventInvoke)
        {
            doConnectionEventInvoke = false;
            OnNewConnectionEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    // Starts the socket connection to the game server
    private void StartNewSocket()
    {
        try
        {
            Uri uri = new Uri(serverUrlLink);
            currentSocket = new SocketIOUnity(uri);

            currentSocket.OnConnected += (sender, e) =>
            {
                Debug.Log("Connected to socket server");

                if (!isConnected) // Just started instance and is connecting to the server for the first time
                {
                    currentSocket.Emit("connection", "");
                    Debug.Log("Connecting normally");
                }
                else // Has connected before and is refreshing connection to the server to stay connected longer
                {
                    var json = new NetworkTypes.SetPersonalIdJson
                    {
                        userNetworkId = userNetworkId,
                        objectNetworkId = playerScript.networkId
                    };

                    currentSocket.Emit("setPersonalIds", JsonUtility.ToJson(json));

                    Debug.Log("Set personal ID");
                }
            };

            // Executes when the client first connects to the server. The message is sent from the server
            // with the user ID and player object ID
            currentSocket.On("connection", response =>
            {
                Debug.Log("Entered Connection");

                if (userNetworkId == -1)
                {
                    try
                    {
                        var jsonData = JsonConvert.DeserializeObject<IList<NetworkTypes.ConnectionId>>(response.ToString());

                        isNetworkHost = jsonData[0].isHost;

                        userNetworkId = jsonData[0].userNetworkId;

                        playerScript.playerId = userNetworkId;
                        playerScript.networkId = jsonData[0].networkId;
                        playerScript.SetCanControlObject(true);

                        var playerReference = NetworkManager.Instance.FindGameObjectInNetworkMap(-200).reference;

                        // Assign ID the player object in the network map
                        int result = NetworkManager.Instance.AddValueToNetworkMap(jsonData[0].networkId, playerReference, "Player", Vector3.zero, Vector3.zero);
                        NetworkManager.Instance.RemoveObjectFromNetworkMap(-200);

                        // If the ID had to be corrected, assign the proper ID to the player
                        if (result != playerScript.networkId) playerScript.networkId = result;

                        playerScript.SetNetworkPositionReference();

                        NetworkManager.Instance.SetConnectedPlayers(jsonData[0].connectedPlayers);

                        isConnected = true;

                        doSceneActivation = true;

                        doConnectionEventInvoke = true;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error getting network ID:\n" + e.Message);
                    }
                }
            });

            // Spawn Object from the network
            currentSocket.On("spawnObjects", response =>
            {
                Debug.Log("Received!");
                DealWithPrefabSpawnJson(response.ToString());
            });

            // Updates player positions from network
            currentSocket.On("updatePlayerPositions", response =>
            {
                DealWithReceivedPlayerPositionJson(response.ToString());
            });

            // Updates stopped player position from network
            currentSocket.On("reportStoppedPosition", response =>
            {
                DealWithReceivedStopPlayerJson(response.ToString());
            });

            // Updates enemy positions
            currentSocket.On("updateEnemyData", response =>
            {
                DealWithReceivedEnemyDataJson(response.ToString());
            });

            // When entering the game with someone in it already, this spawns in all of
            // the assets already in the network scene
            currentSocket.On("spawnScene", response =>
            {
                LoadNetworkScene(response.ToString());
                Debug.Log("Loaded Scene...");
            });

            // Sets the host of the network scene
            currentSocket.On("setHost", response =>
            {
                Debug.Log("Setting new host...");
                DealWithHostJson(response.ToString());
            });

            // Corrects the local network object map
            currentSocket.On("correctObjectMap", response =>
            {
                Debug.Log("Correcting Object Map");
                DealWithObjectMapCorrection(response.ToString());
            });

            // Corrects the network ID of a certain network object
            currentSocket.On("correctId", response =>
            {
                Debug.Log("Correcting ID");
                DealWithIdCorrectionJson(response.ToString());
            });

            // Corrects the user ID specified
            currentSocket.On("correctUserId", response =>
            {
                Debug.Log("Fixing user id");
                DealWithUserIdCorrectionJson(response.ToString());
            });

            // Receives a jump from the network
            currentSocket.On("sendJump", response =>
            {
                DealWithJumpJson(response.ToString());
            });

            // Receives the objects that shot bullets from the network
            currentSocket.On("sendShootingObjects", response =>
            {
                DealWithShootingObjectsJson(response.ToString());
            });

            // Receives AI enemies that took damage
            currentSocket.On("updateAIThatTookDamage", response =>
            {
                DealWithDamagedEnemiesJson(response.ToString());
            });

            currentSocket.On("updateChangedAnimations", response =>
            {
                DealWithChangedAnimationsJson(response.ToString());
            });

            currentSocket.On("updateChangedParticles", response =>
            {
                DealWithChangedParticlesJson(response.ToString());
            });

            currentSocket.On("sendPlayerDamage", response =>
            {
                DealWithPlayerDamageJson(response.ToString());
            });

            currentSocket.On("killSelfOnNetwork", response =>
            {
                DealWithPlayerKillJson(response.ToString());
            });

            currentSocket.On("changeWeapon", response =>
            {
                DealWithWeaponSwapJson(response.ToString());
            });

            // Deletes network object from network
            currentSocket.On("deleteObject", response =>
            {
                DealWithDeleteObjectFromNetworkJson(response.ToString());
            });

            // Simply invokes the "OnPlayerDisconnected" event
            currentSocket.On("disconnectOtherPlayer", response =>
            {
                Debug.Log("Disconnected Player");
                OnPlayerDisconnectedEvent?.Invoke(this, EventArgs.Empty);
            });

            currentSocket.Connect();
        }
        catch (Exception e)
        {
            Debug.Log("Error on line 270:\n" + e.Message);
        }
    }

    // Refreshes the connection to the server before the current socket automatically discconnects
    private void RefreshSocketConnection()
    {
        try
        {
            Debug.Log("Refreshing Socket Connection");

            SocketIOUnity previousSocket = currentSocket;

            StartNewSocket(); // Assigns currentSocket

            previousSocket.Emit("refreshConnection"); // Removes ID from server socketID map

            var json = new NetworkTypes.SetPersonalIdJson
            {
                userNetworkId = userNetworkId,
                objectNetworkId = playerScript.networkId
            };

            // Make new object in socketID map for proper host configuration
            currentSocket.Emit("setPersonalIds", JsonUtility.ToJson(json));

            Debug.Log("Emitted personal ID setting");
        } catch(Exception e)
        {
            Debug.Log("Error on line 288:\n" + e.Message);
        }
    }

    // Sets the IDs of objects that need to added to the network map. It also set the IDs in the object scripts
    private void SetRemainingNetworkIds()
    {
        while (remainingObjectsToAssignIds.Count > 0)
        {
            try
            {
                var networkObject = remainingObjectsToAssignIds.Dequeue();

                int result = NetworkManager.Instance.AddValueToNetworkMap(networkObject.id, networkObject.reference, networkObject.type, networkObject.reference.transform.position, networkObject.reference.transform.rotation.eulerAngles);


                // Set the ID in the scripts of the objects
                if (networkObject.type == "Player")
                {
                    networkObject.reference.GetComponent<movement>().networkId = result;
                }
                else
                {
                    networkObject.reference.GetComponent<MovingObjectAgentMove>().networkId = result;
                }
            }
            catch (Exception e) 
            {
                Debug.Log("Failed to set id:\n" + e.Message);
            }
        }
    }

    // Spawns the remaining objects that need to be spawned
    private void SpawnRemainingObjects()
    {
        for (int i = remainingObjectsToSpawn.Count-1; i>=0; i--)
        {
            var networkObject = remainingObjectsToSpawn[i];

            remainingObjectsToSpawn.RemoveAt(i);
            
            // Create the object, grab the reference, set the position, and the rotation
            GameObject objectReference = Instantiate(NetworkManager.Instance.dummyPrefabReferences[networkObject.prefabName], networkObject.position, Quaternion.identity);
            objectReference.transform.rotation = Quaternion.Euler(networkObject.rotation);

            NetworkManager.Instance.AddValueToNetworkMap(networkObject.networkId, objectReference, networkObject.prefabName, networkObject.position, networkObject.rotation);

            // Set the id in the script of the spawned object
            if (networkObject.prefabName == "Player")
            {
                
                Debug.Log("Entered Player spawn");

                var playerScript = objectReference.GetComponent<movement>();

                playerScript.networkId = networkObject.networkId;
                playerScript.SetNetworkPositionReference();

                if (NetworkManager.Instance != null && NetworkManager.Instance.playerSpawnPosition != null && !isLoadingScene)
                {
                    playerScript.gameObject.transform.position = NetworkManager.Instance.playerSpawnPosition.position;
                }

                OnPlayerConnectedEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var reference = objectReference.GetComponent<MovingObjectAgentMove>();
                if (reference != null)
                {
                    reference.networkId = networkObject.networkId;
                    reference.SetIsDummyObject(true);

                    if (networkObject.prefabName == "Metalon") reference.ActivateAnimation("Walk Forward In Place");
                }
            }
        }
        isLoadingScene = false;

        Debug.Log("Spawned Objects");
    }

    // Move the player objects that have moved from the network
    private void MoveRemainingPlayerObjects()
    {
        while (remainingPlayerObjectsToMove.Count > 0)
        {
            try
            {
                var objectToMove = remainingPlayerObjectsToMove[0];
                remainingPlayerObjectsToMove.RemoveAt(0);

                movement playerScript = objectToMove.reference.GetComponent<movement>();
                playerScript.SetDataFromNetwork(objectToMove.velocity, objectToMove.speed, objectToMove.rotation, objectToMove.weaponRotation);
                playerScript.isStoppedOnNetwork = false;
            }
            catch (Exception)
            {}
        }
    }

    // Move the remaining enemy objects from the data from network
    private void SetRemainingEnemyObjectsData()
    {
        while (remainingEnemyObjectsToMove.Count > 0)
        {
            try
            {
                var objectToMove = remainingEnemyObjectsToMove[0];
                remainingEnemyObjectsToMove.RemoveAt(0);

                MovingObjectAgentMove enemyScript = objectToMove.reference.GetComponent<MovingObjectAgentMove>();
                enemyScript.SetDataFromNetwork(objectToMove.targetId, objectToMove.shouldMove);
            }
            catch (Exception){}
        }
    }

    // Stop the remainign player objects from the data from the network
    private void StopRemainingPlayerObjects()
    {
        while (remainingPlayersToStop.Count > 0)
        {
            try
            {
                var objectToStop = remainingPlayersToStop[0];
                remainingPlayersToStop.RemoveAt(0);

                movement playerScript = objectToStop.reference.GetComponent<movement>();
                playerScript.StopObject(objectToStop.positionVector, objectToStop.rotationVector);
            }
            catch (Exception)
            {}
        }
    }

    // Destroys the remaining objects that were sent to be deleted from the network
    private void DeleteRemainingObjects()
    {
        // TODO: Add type field for object deletion
        while (remainingObjectsToDelete.Count > 0)
        {
            GameObject objectReference = remainingObjectsToDelete.Dequeue();
            if (objectReference.GetComponent<movement>() != null) // If object is a player (has "movement" script)
            {
                objectReference.GetComponent<movement>().UnsubscribeFromEvents();
            }
            else // Object is not a player and must be an enemy
            {
                objectReference.GetComponent<MovingObjectAgentMove>().UnsubscribeFromEvents();
            }
            Destroy(objectReference);
        }
    }

    // Makes the remaining player objects jump from the network
    private void EnableRemainingObjectsToJump()
    {
        while (remainingObjectsToJump.Count > 0)
        {
            try
            {
                NetworkTypes.NetworkObjectReferenceToJump objectToJump = remainingObjectsToJump.Dequeue();

                objectToJump.networkObject.GetComponent<movement>().EnableJump();
            }
            catch (Exception) {}
        }
    }

    // Tells the remaining player objects to shoot from the network
    private void EnableRemainingObjectsToShoot()
    {
        while (remainingObjectsToShoot.Count > 0)
        {
            try
            {
                NetworkTypes.NetworkObjectReferenceToShoot objectToShoot = remainingObjectsToShoot.Dequeue();

                if (objectToShoot.type == "Player")
                {
                    objectToShoot.networkObject.GetComponent<movement>().FireWeapon(objectToShoot.shootingDirection);

                }
            }
            catch (Exception) {}
        }
    }

    // Damages the remaining enemy objects by ID frpm the network
    private void DamageRemainingEnemies()
    {
        while (remainingEnemiesToDamage.Count > 0)
        {
            try
            {
                NetworkTypes.AIThatTookDamage objectReference = remainingEnemiesToDamage.Dequeue();

                GameObject reference = NetworkManager.Instance.FindGameObjectInNetworkMap(objectReference.networkId).reference;

                if (reference != null)
                {
                    var healthScript = reference.GetComponent<dewAgentHealth>();

                    if (healthScript != null) healthScript.TakeDamageFromNetwork(objectReference.weaponString, objectReference.playerId);
                }
            }
            catch (Exception) {}
        }
    }

    // TODO: validate
    private void ActivateRemainingAnimations()
    {
        while (remainingAnimationsToActivate.Count > 0)
        {
            try
            {
                NetworkTypes.ObjectToChangeAnimation objectReference = remainingAnimationsToActivate.Dequeue();

                if (objectReference.reference != null && objectReference.type != "Player")
                {
                    objectReference.reference.GetComponent<MovingObjectAgentMove>().ActivateAnimation(objectReference.animationName);
                }
            }
            catch (Exception) { }
        }
    }

    // TODO: validate
    private void UtilizeRemainingParticles()
    {
        while (remainingParticlesToUtilize.Count > 0)
        {
            try
            {
                NetworkTypes.ObjectToUseParticles objectReference = remainingParticlesToUtilize.Dequeue();

                if (objectReference.reference != null && objectReference.type != "Player")
                {
                    var movementScript = objectReference.reference.GetComponent<MovingObjectAgentMove>();

                    if (movementScript != null) movementScript.SetParticles(objectReference.condition);

                    Debug.Log("Setting particles");
                }
            }
            catch (Exception) { }
        }
    }

    // Damages the remaining players from the network
    private void DamageRemainingPlayers()
    {
        var playerReference = remainingPlayersToDamage.Dequeue();
        playerHealth playerHealthScript = null;

        if (playerReference != null) playerHealthScript = playerReference.GetComponent<playerHealth>();

        if (playerHealthScript != null)
        {
            playerHealthScript.health--;
            UIStuff.Instance.healthChange(-1);
            Debug.Log("Decreased Health");
        }
    }

    // Kills the remaining players from the network
    private void KillRemainingPlayers()
    {
        var playerReference = remainingPlayersToKill.Dequeue();

        if (playerReference != null) Destroy(playerReference);
    }

    private void SwapRemainingWeapons()
    {
        while (remainingPlayersToSwapWeapons.Count > 0)
        {
            NetworkTypes.WeaponSwapJson weaponSwapObject = remainingPlayersToSwapWeapons.Dequeue();
            var playerReference = NetworkManager.Instance.FindGameObjectInNetworkMap(weaponSwapObject.networkId).reference;

            if (playerReference != null)
            {
                var playerMovementScript = playerReference.GetComponent<movement>();

                if (playerMovementScript != null) playerMovementScript.SwitchWeapon(weaponSwapObject.weapon);
            }
        }
    }

    // Turns all of the network gameobjects into actual objects
    private void MigrateHostToSelf()
    {
        doHostMigrationToSelf = false;
        foreach (var sceneObject in NetworkManager.Instance.GetNetworkObjectMap())
        {
            if (sceneObject.Value.type != "Player")
            {
                var previousReferenceScript = sceneObject.Value.reference.GetComponent<MovingObjectAgentMove>();
                if (previousReferenceScript != null && previousReferenceScript.isDummyObject)
                {
                    //previousReferenceScript.isDummyObject = false;

                    Destroy(previousReferenceScript.gameObject);

                    var newEnemyScript = Instantiate(NetworkManager.Instance.functionalPrefabReferences[sceneObject.Value.type], sceneObject.Value.actualPosition, Quaternion.identity).GetComponent<MovingObjectAgentMove>();
                    newEnemyScript.transform.rotation = Quaternion.Euler(sceneObject.Value.actualRotation);
                    
                    newEnemyScript.networkId = sceneObject.Key;
                    
                    sceneObject.Value.reference = newEnemyScript.gameObject;
                }
            }
        }
    }

    // Prepares and sends the data of the Player objects that have changed their position since the last network update
    private void UpdateNetworkPlayerPositions()
    {
        string json = MakePlayerJsonPositionObject();

        NetworkManager.Instance.ClearChangedPlayerObjects();

        currentSocket.Emit("updatePlayerPositions", json);
    }

    // Prepares and sends the data of the Enemy objects that have changed position since the last network update
    private void UpdateNetworkEnemyPositions()
    {
        string json = MakeEnemyJsonPositionsObject();

        NetworkManager.Instance.ClearChangedEnemyObjects();

        currentSocket.Emit("updateEnemyData", json);
    }

    // Prepares and sends the data of the Enemy objects that have been shot since the last network update
    private void UpdateShotEnemies()
    {
        string json = JsonUtility.ToJson(NetworkManager.Instance.GetAIThatTookDamage());

        NetworkManager.Instance.ClearAIThatTookDamage();

        currentSocket.Emit("updateAIThatTookDamage", json);
    }

    // Prepares and sends the data of the AI that changed their animation state
    private void UpdateEnemyAnimations()
    {
        string json = JsonUtility.ToJson(NetworkManager.Instance.GetAIThatChangedAnimation());

        NetworkManager.Instance.ClearAIThatChangedAnimation();

        currentSocket.Emit("updateChangedAnimations", json);
    }

    // Prepares and sends the data of the AI that used their particle system
    private void UpdateEnemyParticles()
    {
        string json = JsonUtility.ToJson(NetworkManager.Instance.GetAIThatUsedParticleSystem());

        NetworkManager.Instance.ClearAIThatUsedParticleSystem();

        currentSocket.Emit("updateChangedParticles", json);
    }

    private void UpdateObjectsToSpawn()
    {
        List<NetworkTypes.PrefabSpawn> prefabSpawnJsonList = new List<NetworkTypes.PrefabSpawn>();

        foreach (var jsonObject in NetworkManager.Instance.GetObjectsToSpawn())
        {
            NetworkTypes.PrefabSpawn prefabSpawnObject = new NetworkTypes.PrefabSpawn
            {
                prefabReferenceName = jsonObject.prefabReferenceName,
                position = jsonObject.position,
                rotation = jsonObject.rotation
            };

            prefabSpawnJsonList.Add(prefabSpawnObject);

            remainingObjectsWithNoId.Enqueue(new NetworkTypes.ObjectToAssignId(jsonObject.reference, jsonObject.prefabReferenceName, -1));
        }

        string json = JsonUtility.ToJson(new NetworkTypes.PrefabSpawnSenderListJson {
            senderId = userNetworkId,
            list = prefabSpawnJsonList
        });

        NetworkManager.Instance.ClearObjectsToSpawn();

        currentSocket.Emit("spawnObjects", json);

        Debug.Log("Sent Spawning Objects");
    }

    // Creates and returns the JSON object required to send the position data of the Player objects that have changed their position
    private string MakePlayerJsonPositionObject()
    {
        Dictionary<int, NetworkTypes.NetworkPlayerToMove> changedPlayerObjects = NetworkManager.Instance.GetChangedPlayerObjects();

        var serializableData = new List<NetworkTypes.PlayerMovementSerializablePositionEntry>();

        // Makes a list of the changed objects with all of the required data per object
        foreach (var changedObject in changedPlayerObjects)
        {
            var entry = new NetworkTypes.PlayerMovementSerializablePositionEntry
            {
                networkId = changedObject.Key,
                type = changedObject.Value.type,
                speed = changedObject.Value.speed,
                weaponRotation = changedObject.Value.weaponRotation,
                velocity = new NetworkTypes.DeserializableVector3
                {
                    x = changedObject.Value.movementVector.x,
                    y = changedObject.Value.movementVector.y,
                    z = changedObject.Value.movementVector.z
                },
                position = new NetworkTypes.DeserializableVector3
                {
                    x = changedObject.Value.positionVector.x,
                    y = changedObject.Value.positionVector.y,
                    z = changedObject.Value.positionVector.z
                },
                rotation = new NetworkTypes.DeserializableVector3
                {
                    x = changedObject.Value.rotationVector.x,
                    y = changedObject.Value.rotationVector.y,
                    z = changedObject.Value.rotationVector.z
                }
            };

            serializableData.Add(entry);
        }

        var jsonData = new NetworkTypes.PlayerMovementListWrapper
        {
            senderId = userNetworkId,
            objects = serializableData
        };

        return JsonUtility.ToJson(jsonData);
    }

    // Creates and returns the JSON object required to send the position data of the Enemy objects that have changed their position
    private string MakeEnemyJsonPositionsObject()
    {
        List<NetworkTypes.NetworkEnemyToMove> changedEnemyObjects = NetworkManager.Instance.GetChangedEnemyObjects();

        var serializableData = new List<NetworkTypes.EnemyMovementSerializablePositionEntry>();

        // Makes a list of the changed objects with all of the required data per object
        foreach (var changedObject in changedEnemyObjects)
        {
            var entry = new NetworkTypes.EnemyMovementSerializablePositionEntry
            {
                networkId = changedObject.id,
                targetId = changedObject.targetId,
                position = new NetworkTypes.DeserializableVector3
                {
                    x = changedObject.positionVector.x,
                    y = changedObject.positionVector.y,
                    z = changedObject.positionVector.z
                },
                rotation = new NetworkTypes.DeserializableVector3
                {
                    x = changedObject.rotationVector.x,
                    y = changedObject.rotationVector.y,
                    z = changedObject.rotationVector.z
                },
                shouldMove = changedObject.shouldMove
            };

            serializableData.Add(entry);
        }

        var jsonData = new NetworkTypes.EnemyMovementListWrapper
        {
            senderId = userNetworkId,
            objects = serializableData
        };

        return JsonUtility.ToJson(jsonData);
    }

    /* Deals with the prefab spawn JSON sent from another client via the game server. JSON looks like:
     * 
     * {
     *      senderId: int,
     *      networkId: int,
     *      prefabReferenceName: string,
     *      position: {
     *          x: float,
     *          y: float,
     *          z: float
     *      },
     *      rotation: {
     *          x: float,
     *          y: float,
     *          z: float
     *      }
     * }
     * 
     */
    private void DealWithPrefabSpawnJson(string jsonString)
    {
        var jsonData = JsonConvert.DeserializeObject<List<NetworkTypes.PrefabSpawnResponseListJson>>(jsonString);

        bool onlyAssignIds = false;

        int senderId = jsonData[0].senderId;
        if (senderId == userNetworkId) onlyAssignIds = true;

        foreach (var objectToSpawn in jsonData[0].list)
        {
            if (onlyAssignIds)
            {
                AssignObjectIdForType(objectToSpawn.prefabReferenceName, objectToSpawn.networkId);
                continue;
            }

            if (omittedIdsThatAreNotInScene.Contains(objectToSpawn.networkId)) omittedIdsThatAreNotInScene.Remove(objectToSpawn.networkId);

            NetworkTypes.DeserializableVector3 coordinates = objectToSpawn.position;
            NetworkTypes.DeserializableVector3 rotation = objectToSpawn.rotation;
            Vector3 positionVector = new Vector3(coordinates.x, coordinates.y, coordinates.z);
            Vector3 rotationVector = new Vector3(rotation.x, rotation.y, rotation.z);

            remainingObjectsToSpawn.Add(new NetworkTypes.ObjectToSpawn(objectToSpawn.networkId, objectToSpawn.prefabReferenceName, positionVector, rotationVector));

            if (objectToSpawn.prefabReferenceName == "Player") NetworkManager.Instance.IncrementConnectedPlayers();
        }

        Debug.Log("Spawning Objects");
    }

    /* Deserializes and sets up the objects from the JSON received from the server about the updated player positions. JSON:
     * 
     * [{
     *      data: {
     *          senderId: int,
     *          objects: [
     *              {
     *                  networkId: int,
     *                  type: string,
     *                  speed: int,
     *                  velocity: {
     *                      x: float,
     *                      y: float,
     *                      z: float
     *                  },
     *                  position: {
     *                      x: float,
     *                      y: float,
     *                      z: float 
     *                  },
     *                  rotation: {
     *                      x: float,
     *                      y: float,
     *                      z: float
     *                  },
     *                  {
     *                      ...
     *                  }
     *          ]
     *      }
     * }]
     */
    private void DealWithReceivedPlayerPositionJson(string jsonString)
    {
        try
        {
            var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
            var jsonData = JsonConvert.DeserializeObject<NetworkTypes.PlayerMovementListWrapper>(json[0].data);

            int senderId = jsonData.senderId;
            if (senderId == userNetworkId) return;

            List<NetworkTypes.PlayerMovementSerializablePositionEntry> objects = jsonData.objects;
            foreach(var entry in objects)
            {
                int objectId = entry.networkId;

                NetworkTypes.NetworkObject movedObject = NetworkManager.Instance.FindGameObjectInNetworkMap(objectId);

                NetworkTypes.DeserializableVector3 position = entry.position;
                movedObject.actualPosition = new Vector3(position.x, position.y, position.z);

                NetworkTypes.DeserializableVector3 velocity = entry.velocity;
                NetworkTypes.DeserializableVector3 rotation = entry.rotation;
                Vector3 velocityVector = new Vector3(velocity.x, velocity.y, velocity.z);
                Vector3 rotationVector = new Vector3(rotation.x, rotation.y, rotation.z);

                if (velocityVector != null)
                {
                    remainingPlayerObjectsToMove.Add(new NetworkTypes.PlayerToMove(movedObject.reference, entry.type, velocityVector, rotationVector, entry.speed, entry.weaponRotation));
                }
            }
        }
        catch (Exception) {}
    }

    /* When the player stops over the network, this function is referenced when the socket sends the correct message. This
     * function deserializes the data and sets it up for the player to be stopped on the main thread. JSON:
     * 
     * [{
     *      data: {
     *          networkId: int,
     *          type: string,
     *          position: {
     *              x: float,
     *              y: float,
     *              z: float
     *          },
     *          rotation: {
     *              x: float,
     *              y: float,
     *              z: float
     *          },
     *          speed: float
     *      }
     * }]
     * 
     */
    private void DealWithReceivedStopPlayerJson(string jsonString)
    {
        try
        {
            var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
            var jsonData = JsonConvert.DeserializeObject<NetworkTypes.StoppingPlayerSerializablePositionEntry>(json[0].data);

            int objectId = jsonData.networkId;
            NetworkTypes.NetworkObject movedObject = NetworkManager.Instance.FindGameObjectInNetworkMap(jsonData.networkId);

            NetworkTypes.DeserializableVector3 position = jsonData.position;
            NetworkTypes.DeserializableVector3 rotation = jsonData.rotation;
            Vector3 positionVector = new Vector3(position.x, position.y, position.z);
            Vector3 rotationVector = new Vector3(rotation.x, rotation.y, rotation.z);

            movedObject.actualPosition = positionVector;
            movedObject.actualRotation = rotationVector;

            if (movedObject != null && positionVector != null)
            {
                remainingPlayersToStop.Add(new NetworkTypes.PlayerToStop(movedObject.reference, jsonData.type, positionVector, rotationVector));
            }
        }
        catch (Exception) {}
    }

    /* Deals with the JSON sent over the network to move enemy objects. JSON:
     * 
     * [{
     *      data: {
     *          senderId: int,
     *          objects: [
     *              {
     *                  networkId: int,
     *                  targetId: int,
     *                  position: {
     *                      x: float,
     *                      y: float,
     *                      z: float
     *                  },
     *                  rotation: {
     *                      x: float,
     *                      y: float,
     *                      z: float
     *                  },
     *                  shouldMove: bool
     *              }
     *          ]
     *      }
     * ]}
     * 
     */
    private void DealWithReceivedEnemyDataJson(string jsonString)
    {
        try
        {
            var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
            var jsonData = JsonConvert.DeserializeObject<NetworkTypes.EnemyMovementListWrapper>(json[0].data);

            int senderId = jsonData.senderId;
            if (senderId == userNetworkId) return;

            List<NetworkTypes.EnemyMovementSerializablePositionEntry> objects = jsonData.objects;
            foreach (var entry in objects)
            {
                int objectId = entry.networkId;

                NetworkTypes.NetworkObject movedObject = NetworkManager.Instance.FindGameObjectInNetworkMap(objectId);

                NetworkTypes.DeserializableVector3 position = entry.position;
                NetworkTypes.DeserializableVector3 rotation = entry.rotation;
                Vector3 positionVector = new Vector3(position.x, position.y, position.z);
                Vector3 rotationVector = new Vector3(rotation.x, rotation.y, rotation.z);

                movedObject.actualPosition = positionVector;
                movedObject.actualRotation = rotationVector;

                if (movedObject.reference == null)
                {
                    omittedIdsThatAreNotInScene.Add(objectId);
                    if (objectId != -1)
                    {
                        currentSocket.Emit("askToSpawnObject", new NetworkTypes.AskToSpawnSenderJson
                        {
                            senderId = senderId,
                            networkId = objectId
                        });
                    }
                    else
                    {
                        currentSocket.Emit("deleteObject", new NetworkTypes.NetworkObjectToDelete
                        {
                            id = objectId
                        });
                    }
                }
                else if ((positionVector != null) && (rotationVector != null))
                {
                    remainingEnemyObjectsToMove.Add(new NetworkTypes.EnemyToMove(movedObject.reference, entry.targetId, positionVector, rotationVector, entry.shouldMove));
                }
            }
        }
        catch (Exception) { }
    }

    /* When the client enters a new scene that has already been populated, the client receives a dictionary of all of the objecst in the scene with the
     * required data (position, rotation, prefab name, and ID)
     * 
     * ["
     *      {
     *          \"0\":
     *          {
     *              \"prefabName\":\"Player\",
     *              \"positionVector\":
     *              {
     *                  \"x\":8.933124542236328,
     *                  \"y\":0.062114864587783813,
     *                  \"z\":-10.389663696289062
     *              },
     *              \"rotationVector\":
     *              {
     *                  \"x\":0,
     *                  \"y\":245.31942749023438,
     *                  \"z\":0
     *              }
     *          },
     *          \"1\":
     *          {
     *              \"prefabName\":\"Summoner\",
     *              \"positionVector\":
     *              {
     *                  \"x\":0.4699999988079071,
     *                  \"y\":0.27000001072883606,
     *                  \"z\":-12.529999732971191
     *              },
     *              \"rotationVector\":
     *              {
     *                  \"x\":0,
     *                  \"y\":0,
     *                  \"z\":0
     *              }
     *          },
     *          \"2\":
     *          {
     *              \"prefabName\":\"ShooterSphere\",
     *              \"positionVector\":
     *              {
     *                  \"x\":2.5102038383483887,
     *                  \"y\":0.27000001072883606,
     *                  \"z\":-13.488360404968262
     *              },
     *              \"rotationVector\":
     *              {
     *                  \"x\":0,
     *                  \"y\":0,
     *                  \"z\":0
     *              }
     *          },
     *          \"3\":
     *          {
     *              \"prefabName\":\"ShooterSphere\",
     *              \"positionVector\":
     *              {
     *                  \"x\":2.5102038383483887,
     *                  \"y\":0.27000001072883606,
     *                  \"z\":-13.488360404968262
     *              },
     *              \"rotationVector\":
     *              {
     *                  \"x\":0,
     *                  \"y\":0,
     *                  \"z\":0
     *              }
     *          }
     *      }"
     * ]
     */
    private void LoadNetworkScene(string jsonString)
    {
        try
        {
            isLoadingScene = true;

            var json = JsonConvert.DeserializeObject<List<string>>(jsonString);
            var jsonData = JsonConvert.DeserializeObject<Dictionary<int, NetworkTypes.JsonSpawnNetworkObject>>(json[0]);

            foreach (var pair in jsonData)
            {
                NetworkTypes.DeserializableVector3 jsonPositionVector = pair.Value.positionVector;
                NetworkTypes.DeserializableVector3 jsonRotationVector = pair.Value.rotationVector;
                Vector3 positionVector = new Vector3(jsonPositionVector.x, jsonPositionVector.y, jsonPositionVector.z);
                Vector3 rotationVector = new Vector3(jsonRotationVector.x, jsonRotationVector.y, jsonRotationVector.z);

                remainingObjectsToSpawn.Add(new NetworkTypes.ObjectToSpawn(pair.Key, pair.Value.prefabName, positionVector, rotationVector));
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load network scene:\n" + e.Message);
        }
    }

    /* Sets the host of the client with a certain userID JSON:
     * 
     * {
     *      userNetworkId: int,
     *      isHost: bool
     * }
     * 
     */
    private void DealWithHostJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<IList<NetworkTypes.HostJson>>(jsonString);

        if (userNetworkId == json[0].userNetworkId && json[0].isHost)
        {
            isNetworkHost = json[0].isHost;
            Debug.Log("You are now the host.");
        }

        if (isNetworkHost) doHostMigrationToSelf = true;
    }

    /* Deals with the data that deletes an object from the network scene, sent via the server. JSON:
     * 
     * [{
     *      id: int
     * ]}
     * 
     */
    private void DealWithDeleteObjectFromNetworkJson(string jsonString)
    {
        try
        {
            var json = JsonConvert.DeserializeObject<List<NetworkTypes.NetworkObjectToDelete>>(jsonString);

            GameObject objectToDelete = NetworkManager.Instance.FindGameObjectInNetworkMap(json[0].id).reference;
            if (objectToDelete != null) remainingObjectsToDelete.Enqueue(objectToDelete);

            NetworkManager.Instance.RemoveObjectFromNetworkMap(json[0].id);
        }
        catch (Exception){}
    }

    /* If an ID was required to be fixed, the JSON sent is the data sent via the network to correct a certain ID. JSON:
     * 
     * {
     *      previousId: int,
     *      newId: int
     * }
     * 
     */
    private void DealWithIdCorrectionJson(string jsonString)
    {
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.IdCorrectionJson>(jsonString);

        if (NetworkManager.Instance.HasObjectInMapWithId(jsonData.previousId))
        {
            NetworkTypes.NetworkObject reference = NetworkManager.Instance.FindGameObjectInNetworkMap(jsonData.previousId);
            NetworkManager.Instance.AddValueToNetworkMap(jsonData.newId, reference.reference, reference.type, reference.reference.transform.position, reference.reference.transform.rotation.eulerAngles);
            NetworkManager.Instance.RemoveObjectFromNetworkMap(jsonData.previousId);
        }
        else // Send false information to fix current NetworkObjectMap
        {
            var json = new NetworkTypes.IdCorrectionJson
            {
                previousId = -300,
                newId = -300
            };

            currentSocket.Emit("idCorrection", JsonUtility.ToJson(json));
        }
    }

    /* Corrects the ID of a given user network object. JSON: 
     * 
     * {
     *      
     * }
     * 
     */
    private void DealWithUserIdCorrectionJson(string jsonString)
    {
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.UserIdCorrectionJson>(jsonString);

        userNetworkId = jsonData.userId;

        playerScript.playerId = jsonData.userId;
        playerScript.networkId = jsonData.newId;

        NetworkTypes.NetworkObject playerReference = NetworkManager.Instance.FindGameObjectInNetworkMap(jsonData.previousId);
        NetworkManager.Instance.RemoveObjectFromNetworkMap(jsonData.previousId);
        NetworkManager.Instance.AddValueToNetworkMap(jsonData.newId, playerReference.reference, playerReference.type, playerReference.reference.transform.position, playerReference.reference.transform.rotation.eulerAngles);
    }

    /* Deals with correcting the local networkObjectMap by comparing the current one to the server map. JSON: 
     * 
     * [{
     *      id: int {
     *          prefabName: string,
     *          positionVector: {
     *              x: float,
     *              y: float,
     *              z: float
     *          },
     *          rotationVector: {
     *              x: float,
     *              y: float,
     *              z: float
     *          }
     *      },
     *      id: int {
     *          ...
     *      }
     * }]
     * 
     */
    private void DealWithObjectMapCorrection(string jsonString)
    {
        try
        {
            var json = JsonConvert.DeserializeObject<List<string>>(jsonString);
            var jsonData = JsonConvert.DeserializeObject<Dictionary<int, NetworkTypes.JsonSpawnNetworkObject>>(json[0]);

            int objectsToSpawn = NetworkManager.Instance.GetNetworkObjectMap().Count - jsonData.Count;

            foreach (var pair in NetworkManager.Instance.GetNetworkObjectMap())
            {

                if (jsonData.ContainsKey(pair.Key)) continue;
                else // If the network map does not contain a certain ID, remove it from the network map and delete it from the scene
                {
                    if (objectsToSpawn > 0)
                    {
                        remainingObjectsToDelete.Enqueue(pair.Value.reference);
                        NetworkManager.Instance.RemoveObjectFromNetworkMap(pair.Key);
                    }
                }
            }

            if (jsonData.Count != NetworkManager.Instance.GetNetworkObjectMap().Count)
            {
                foreach (var pair in jsonData)
                {
                    if (NetworkManager.Instance.HasObjectInMapWithId(pair.Key)) continue;
                    else // If the value is not found in the local map, spawn it
                    {
                        if (objectsToSpawn < 0)
                        {
                            NetworkTypes.DeserializableVector3 position = pair.Value.positionVector;
                            NetworkTypes.DeserializableVector3 rotation = pair.Value.rotationVector;
                            Vector3 positionVector = new Vector3(position.x, position.y, position.z);
                            Vector3 rotationVector = new Vector3(rotation.x, rotation.y, rotation.z);
                            remainingObjectsToSpawn.Add(new NetworkTypes.ObjectToSpawn(pair.Key, pair.Value.prefabName, positionVector, rotationVector));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to correct network scene:\n" + e.Message);
        }
    }

    /* Deals with the JSON received from the server to make a Player object jump. JSON:
     * 
     * [{
     *      data: {
     *          id: int,
     *          type: string
     *      }
     * }]
     * 
     */
    private void DealWithJumpJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.JumpJson>(json[0].data);

        GameObject objectReference = NetworkManager.Instance.FindGameObjectInNetworkMap(jsonData.id).reference;

        remainingObjectsToJump.Enqueue(new NetworkTypes.NetworkObjectReferenceToJump(objectReference, jsonData.type));
    }

    /* Deals with the JSON of the objects that have shot. JSON:
     * 
     * [{
     *      data: {
     *          objects: [
     *              {
     *                  networkId: int,
     *                  shootingVector: {
     *                      x: float,
     *                      y: float,
     *                      z: float
     *                  },
     *                  type: string
     *              }
     *          ]
     *      }
     * }]
     * 
     */
    private void DealWithShootingObjectsJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.ObjectsThatFiredBullet>(json[0].data);

        foreach (var shootingObject in jsonData.objects)
        {
            GameObject objectReference = NetworkManager.Instance.FindGameObjectInNetworkMap(shootingObject.networkId).reference;

            Vector3 shootingVector = new Vector3(shootingObject.shootingVector.x, shootingObject.shootingVector.y, shootingObject.shootingVector.z);

            remainingObjectsToShoot.Enqueue(new NetworkTypes.NetworkObjectReferenceToShoot(objectReference, shootingVector, shootingObject.type));
        }
    }

    /* Deals with the JSON that signifies which enemies have taken damage over the network. JSON:
     * 
     * [{
     *      data: {
     *          list: [
     *              {
     *                  networkId: int
     *              }
     *          ]
     *      }
     * }]
     * 
     */
    private void DealWithDamagedEnemiesJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.AIDamagedList>(json[0].data);

        foreach (var enemy in jsonData.list)
        {
            remainingEnemiesToDamage.Enqueue(enemy);
        }
    }

    // TODO: validate
    private void DealWithChangedAnimationsJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.AIAnimationList>(json[0].data);

        foreach (var enemy in jsonData.list)
        {
            try
            {
                NetworkTypes.NetworkObject objectReference = NetworkManager.Instance.FindGameObjectInNetworkMap(enemy.networkId);

                remainingAnimationsToActivate.Enqueue(new NetworkTypes.ObjectToChangeAnimation(objectReference.reference, objectReference.type, enemy.animationName));
            }
            catch (Exception) {}
        }
    }

    // TODO: validate
    private void DealWithChangedParticlesJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.AIParticleList>(json[0].data);

        foreach (var enemy in jsonData.list)
        {
            try
            {
                NetworkTypes.NetworkObject objectReference = NetworkManager.Instance.FindGameObjectInNetworkMap(enemy.networkId);

                remainingParticlesToUtilize.Enqueue(new NetworkTypes.ObjectToUseParticles(objectReference.reference, objectReference.type, enemy.condition));
            }
            catch (Exception) {}
        }
    }

    private void DealWithPlayerDamageJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.DamagedPlayerJson>(json[0].data);

        Debug.Log("Player taking damage");

        remainingPlayersToDamage.Enqueue(NetworkManager.Instance.FindGameObjectInNetworkMap(jsonData.networkId).reference);
    }

    private void DealWithPlayerKillJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.KillSelfOnNetworkJson>(json[0].data);

        remainingPlayersToKill.Enqueue(NetworkManager.Instance.FindGameObjectInNetworkMap(jsonData.networkId).reference);
    }

    private void DealWithWeaponSwapJson(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<List<NetworkTypes.DefaultJsonDataWrapper>>(jsonString);
        var jsonData = JsonConvert.DeserializeObject<NetworkTypes.WeaponSwapJson>(json[0].data);

        remainingPlayersToSwapWeapons.Enqueue(jsonData);
    }

    // Sets up the correct object to be assigned an ID. This involves the objects that have been spawned locally and have not received a network ID
    private void AssignObjectIdForType(string prefabName, int id)
    {
        Queue<NetworkTypes.ObjectToAssignId> tempObjectsThatNeedIdsQueue = remainingObjectsWithNoId;
        var holderQueue = new Queue<NetworkTypes.ObjectToAssignId>();

        bool needsMatch = true;
        while (tempObjectsThatNeedIdsQueue.Count > 0)
        {
            NetworkTypes.ObjectToAssignId possibleMatch = tempObjectsThatNeedIdsQueue.Dequeue();
            if ((possibleMatch.type == prefabName) && needsMatch)
            {
                needsMatch = false;

                possibleMatch.id = id;
                remainingObjectsToAssignIds.Enqueue(possibleMatch);
                
            }
            else holderQueue.Enqueue(possibleMatch);
        }

        remainingObjectsWithNoId = holderQueue;
    }

    // Returns the client user network ID
    public int GetUserNetworkId()
    {
        return userNetworkId;
    }

    // Sends data of the stopped player objects over teh network
    public void ReportStoppedChangedPlayerObjectPosition(int id, string type, Vector3 position, Vector3 rotation, float speed)
    {
        var positionVector = new NetworkTypes.DeserializableVector3
        {
            x = position.x,
            y = position.y,
            z = position.z
        };
        var rotationVector = new NetworkTypes.DeserializableVector3
        {
            x = rotation.x,
            y = rotation.y,
            z = rotation.z
        };

        var stoppedObject = new NetworkTypes.StoppingPlayerSerializablePositionEntry
        {
            networkId = id,
            type = type,
            position = positionVector,
            rotation = rotationVector,
            speed = speed
        };

        currentSocket.Emit("reportStoppedPosition", JsonUtility.ToJson(stoppedObject));
    }

    // Corrects the network ID of an object on the network
    public void CorrectIdAssignment(int previousNetworkId, int newNetworkId)
    {
        var jsonObject = new NetworkTypes.IdCorrectionJson
        {
            previousId = previousNetworkId,
            newId = newNetworkId
        };

        currentSocket.Emit("idCorrection", JsonUtility.ToJson(jsonObject));
    }

    // Sends a jump over the network for the specified player object
    public void SendJump(int id, string type)
    {
        var jsonObject = new NetworkTypes.JumpJson
        {
            id = id,
            type = type
        };

        currentSocket.Emit("sendJump", JsonUtility.ToJson(jsonObject));
    }

    // Sends the data of the objects that shot over the network
    public void ReportObjectsThatShot()
    {
        string json = JsonUtility.ToJson(new NetworkTypes.ObjectsThatFiredBullet{
            objects = NetworkManager.Instance.GetObjectsThatFiredBullet()
        });

        currentSocket.Emit("sendShootingObjects", json);

        NetworkManager.Instance.ClearObjectsThatShotList();
    }

    // Deletes and object from everyones network scene
    public void DeleteObjectOnNetwork(int id)
    {
        NetworkManager.Instance.RemoveObjectFromNetworkMap(id);

        currentSocket.Emit("deleteObject", JsonUtility.ToJson(new NetworkTypes.NetworkObjectToDelete
        {
            id = id
        }));
    }

    // Sends the data that a player has taken damage over the network
    public void DamagePlayerOnNetwork(int id)
    {
        currentSocket.Emit("sendPlayerDamage", JsonUtility.ToJson(new NetworkTypes.DamagedPlayerJson
        {
            networkId = id
        }));
    }

    // Kills self on the network
    public void KillSelfOnNetwork()
    {
        currentSocket.Emit("killSelfOnNetwork", JsonUtility.ToJson(new NetworkTypes.KillSelfOnNetworkJson
        {
            networkId = playerScript.networkId
        }));

        NetworkManager.Instance.RemoveObjectFromNetworkMap(playerScript.networkId);
    }

    // TOUSE
    public void SwapWeapon(int networkId, string weapon)
    {
        currentSocket.Emit("changeWeapon", JsonUtility.ToJson(new NetworkTypes.WeaponSwapJson
        {
            networkId = networkId,
            weapon = weapon
        }));
    }

    // Disconnects the client formally
    public void Disconnect()
    {
        Debug.Log("Disconnecting...");

        var structuredJson = new NetworkTypes.DisconnectionJson
        {
            senderId = userNetworkId,
            objectNetworkId = playerScript.networkId
        };

        string jsonString = JsonUtility.ToJson(structuredJson);

        currentSocket.Emit("disconnectClient", jsonString); // Run proper disconnection code on server before leaving game

        currentSocket.Disconnect();
        Debug.Log("Disconnected from server");
    }
}
