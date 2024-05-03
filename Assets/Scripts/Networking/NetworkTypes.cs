using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NetworkTypes : MonoBehaviour
{
    [Serializable]
    public class DefaultJsonDataWrapper
    {
        public string data;
    }

    [Serializable]
    public class PlayerMovementListWrapper
    {
        public int senderId;
        public List<PlayerMovementSerializablePositionEntry> objects;
    }

    [Serializable]
    public class EnemyMovementListWrapper
    {
        public int senderId;
        public List<EnemyMovementSerializablePositionEntry> objects;
    }

    [Serializable]
    public class AIThatTookDamage
    {
        public int networkId;
        public string weaponString;
        public int playerId;
    }

    [Serializable]
    public class AIDamagedList
    {
        public List<AIThatTookDamage> list;
    }

    [Serializable]
    public class PlayerMovementSerializablePositionEntry
    {
        public int networkId;
        public string type;
        public float speed;
        public float weaponRotation;
        public DeserializableVector3 velocity;
        public DeserializableVector3 position;
        public DeserializableVector3 rotation;
    }

    [Serializable]
    public class EnemyMovementSerializablePositionEntry
    {
        public int networkId;
        public int targetId;
        public DeserializableVector3 position;
        public DeserializableVector3 rotation;
        public bool shouldMove;
    }

    [Serializable]
    public class StoppingPlayerSerializablePositionEntry
    {
        public int networkId;
        public string type;
        public DeserializableVector3 position;
        public DeserializableVector3 rotation;
        public float speed;
    }

    [Serializable]
    public class ConnectionId
    {
        public int userNetworkId;
        public int networkId;
        public int connectedPlayers;
        public bool isHost;
    }

    [Serializable]
    public class HostJson
    {
        public int userNetworkId;
        public bool isHost;
    }

    [Serializable]
    public class PrefabSpawnSenderListJson
    {
        public int senderId;
        public List<PrefabSpawn> list;
    }

    [Serializable]
    public class PrefabSpawnResponseListJson
    {
        public int senderId;
        public List<PrefabSpawnResponse> list;
    }

    [Serializable]
    public class PrefabSpawn
    {
        public string prefabReferenceName;
        public DeserializableVector3 position;
        public DeserializableVector3 rotation;
    }

    [Serializable]
    public class PrefabSpawnResponse
    {
        public int networkId;
        public string prefabReferenceName;
        public DeserializableVector3 position;
        public DeserializableVector3 rotation;
    }

    [Serializable]
    public class DeserializableVector3
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class JsonSpawnNetworkObject
    {
        public string prefabName;
        public DeserializableVector3 positionVector;
        public DeserializableVector3 rotationVector;
    }

    [Serializable]
    public class DisconnectionJson
    {
        public int senderId;
        public int objectNetworkId;
    }

    [Serializable]
    public class NetworkObjectToDelete
    {
        public int id;
    }

    [Serializable]
    public class RespawnSelfJson
    {
        public int senderId;
        public int objectId;
        public int objectsInMap;
        public DeserializableVector3 positionVector;
        public DeserializableVector3 rotationVector;
    }

    [Serializable]
    public class IdCorrectionJson
    {
        public int previousId;
        public int newId;
    }

    [Serializable]
    public class UserIdCorrectionJson
    {
        public int userId;
        public int previousId;
        public int newId;
    }

    [Serializable]
    public class JumpJson
    {
        public int id;
        public string type;
    }

    [Serializable]
    public class AskToSpawnSenderJson
    {
        public int senderId;
        public int networkId;
    }

    [Serializable]
    public class ObjectThatFiredBullet
    {
        public int networkId;
        public DeserializableVector3 shootingVector;
        public string type;
    }

    [Serializable]
    public class ObjectsThatFiredBullet
    {
        public List<ObjectThatFiredBullet> objects;
    }

    [Serializable]
    public class SetPersonalIdJson
    {
        public int userNetworkId;
        public int objectNetworkId;
    }

    [Serializable]
    public class AIAnimationList
    {
        public List<AnimationJson> list;
    }

    [Serializable]
    public class AIParticleList
    {
        public List<ParticleJson> list;
    }

    [Serializable]
    public class AnimationJson
    {
        public int networkId;
        public string animationName;
    }

    [Serializable]
    public class ParticleJson
    {
        public int networkId;
        public bool condition;
    }

    [Serializable]
    public class DamagedPlayerJson
    {
        public int networkId;
    }

    [Serializable]
    public class KillSelfOnNetworkJson
    {
        public int networkId;
    }

    [Serializable]
    public class WeaponSwapJson
    {
        public int networkId;
        public string weapon;
    }

    public class NetworkObject
    {
        public GameObject reference;
        public string type;
        public Vector3 actualPosition;
        public Vector3 actualRotation;

        public NetworkObject(GameObject reference, string type, Vector3 position, Vector3 rotation)
        {
            this.reference = reference;
            this.type = type;
            this.actualPosition = position;
            this.actualRotation = rotation;
        }
    }

    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class NetworkObjectReferenceToJump
    {
        public GameObject networkObject;
        public string type;

        public NetworkObjectReferenceToJump(GameObject networkObject, string type)
        {
            this.networkObject = networkObject;
            this.type = type;
        }
    }

    public class PrefabSpawnLocal
    {
        public GameObject reference;
        public string prefabReferenceName;
        public DeserializableVector3 position;
        public DeserializableVector3 rotation;

        public PrefabSpawnLocal(GameObject reference, string prefabReferenceName, DeserializableVector3 position, DeserializableVector3 rotation)
        {
            this.reference = reference;
            this.prefabReferenceName = prefabReferenceName;
            this.position = position;
            this.rotation = rotation;
        }
    }

    public class NetworkObjectReferenceToShoot
    {
        public GameObject networkObject;
        public Vector3 shootingDirection;
        public string type;

        public NetworkObjectReferenceToShoot(GameObject networkObject, Vector3 shootingDirection, string type)
        {
            this.networkObject = networkObject;
            this.shootingDirection = shootingDirection;
            this.type = type;
        }
    }

    public class NetworkPlayerToMove
    {
        public int id;
        public string type;
        public float weaponRotation;
        public SerializableVector3 movementVector;
        public SerializableVector3 positionVector;
        public SerializableVector3 rotationVector;
        public float speed;

        public NetworkPlayerToMove(int id, string type, float weaponRotation, Vector3 movement, Vector3 position, Vector3 rotation, float speed)
        {
            this.id = id;
            this.type = type;
            this.weaponRotation = weaponRotation;
            this.movementVector = new SerializableVector3(movement);
            this.positionVector = new SerializableVector3(position);
            this.rotationVector = new SerializableVector3(rotation);
            this.speed = speed;
        }
    }

    public class NetworkEnemyToMove
    {
        public int id;
        public int targetId;
        public Vector3 positionVector;
        public Vector3 rotationVector;
        public bool shouldMove;

        public NetworkEnemyToMove(int id, int targetId, Vector3 position, Vector3 rotation, bool shouldMove)
        {
            this.id = id;
            this.targetId = targetId;
            this.positionVector = position;
            this.rotationVector = rotation;
            this.shouldMove = shouldMove;
        }
    }

    public class PlayerToStop
    {
        public GameObject reference;
        public Vector3 positionVector;
        public Vector3 rotationVector;
        public string type;

        public PlayerToStop(GameObject reference, string type, Vector3 position, Vector3 rotation)
        {
            this.reference = reference;
            this.positionVector = position;
            this.rotationVector = rotation;
            this.type = type;
        }
    }

    public class EnemyToStop
    {
        public GameObject reference;
        public Vector3 positionVector;
        public Vector3 rotationVector;
        public string type;

        public EnemyToStop(GameObject reference, string type, Vector3 position, Vector3 rotation)
        {
            this.reference = reference;
            this.positionVector = position;
            this.rotationVector = rotation;
            this.type = type;
        }
    }

    public class PlayerToMove
    {
        public GameObject reference;
        public string type;
        public Vector3 velocity;
        public Vector3 rotation;
        public float speed;
        public float weaponRotation;

        public PlayerToMove(GameObject reference, string type, Vector3 velocity, Vector3 rotation, float speed, float weaponRotation)
        {
            this.reference = reference;
            this.type = type;
            this.velocity = velocity;
            this.rotation = rotation;
            this.speed = speed;
            this.weaponRotation = weaponRotation;
        }
    }

    public class EnemyToMove
    {
        public GameObject reference;
        public int targetId;
        public Vector3 position;
        public Vector3 rotation;
        public bool shouldMove;

        public EnemyToMove(GameObject reference, int targetId, Vector3 position, Vector3 rotation, bool shouldMove)
        {
            this.reference = reference;
            this.targetId = targetId;
            this.position = position;
            this.rotation = rotation;
            this.shouldMove = shouldMove;
        }
    }

    public class ObjectToSpawn
    {
        public int networkId;
        public string prefabName;
        public Vector3 position;
        public Vector3 rotation;

        public ObjectToSpawn(int id, string name, Vector3 position, Vector3 rotation)
        {
            this.networkId = id;
            this.prefabName = name;
            this.position = position;
            this.rotation = rotation;
        }
    }

    public class ObjectToAssignId
    {
        public GameObject reference;
        public string type;
        public int id;

        public ObjectToAssignId(GameObject reference, string type, int id)
        {
            this.reference = reference;
            this.type = type;
            this.id = id;
        }
    }

    public class ObjectToChangeAnimation
    {
        public GameObject reference;
        public string type;
        public string animationName;

        public ObjectToChangeAnimation(GameObject reference, string type, string animationName)
        {
            this.reference = reference;
            this.type = type;
            this.animationName = animationName;
        }
    }

    public class ObjectToUseParticles
    {
        public GameObject reference;
        public string type;
        public bool condition;

        public ObjectToUseParticles(GameObject reference, string type, bool condition)
        {
            this.reference = reference;
            this.type = type;
            this.condition = condition;
        }
    }

    public class ObjectToChangeTarget
    {
        public GameObject reference;
        public int targetId;

        public ObjectToChangeTarget(GameObject reference, int id)
        {
            this.reference = reference;
            targetId = id;
        }
    }
}
