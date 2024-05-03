using UnityEngine;
using TMPro;
using System;

public class movement : MonoBehaviour
{
    [SerializeField]
    private float maximumSpeed;

    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private float jumpSpeed;

    [SerializeField]
    private float jumpButtonGracePeriod;

    [SerializeField]
    private TextMeshProUGUI scoreTextReference;

    [SerializeField]
    private TextMeshProUGUI ammoTextReference;

    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private CurrentGun currentGunScript;

    //private Animator animator;
    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;

    // Network attributes
    public int playerId = -1;
    public int networkId = -1;
    public bool isStoppedOnNetwork = true;
    public bool isDummyObject = false;

    private NetworkTypes.NetworkObject networkPositionReference;
    private bool canControlObject = false;
    private bool canMovedOnNetwork = false;
    private float timeBeforeCanMoveOnNetwork = 5f;
    private float passedTimeForMovement = 0f;

    private Vector3 previousPosition;
    private Vector3 previousRotation;
    private Vector3 velocity;
    private Vector2 rotation;
    private float speed;
    private float weaponRotation = 0f;
    private bool isJumping = false;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;

        if (!isDummyObject)
        {
            PlayerScore.Instance.playerScoreText = scoreTextReference;
            AmmoManager.Instance.ammoDisplay = ammoTextReference;
        }

        previousPosition = transform.position;
        previousRotation = transform.rotation.eulerAngles;

        if (!isDummyObject && NetworkManager.Instance != null) NetworkManager.Instance.OnFixedNetworkUpdateEvent += NetworkManager_OnFixedNetworkUpdate;
    }

    private void NetworkManager_OnFixedNetworkUpdate(object sender, System.EventArgs e)
    {
        Vector3 position = transform.position;
        Vector3 rotation = transform.rotation.eulerAngles;
        if ((previousPosition != position) || (previousRotation != rotation))
        {
            NetworkManager.Instance.AddChangedPlayerObject(networkId, "Player", velocity, position, rotation, speed, currentGunScript.GetCurrentGun().transform.rotation.eulerAngles.x);
            previousPosition = position;
            previousRotation = rotation;
            isStoppedOnNetwork = false;
        }
        else {
            PersonalNetworkAgent.Instance.ReportStoppedChangedPlayerObjectPosition(networkId, "Player", position, rotation, speed);
            previousPosition = position;
            previousRotation = rotation;
            isStoppedOnNetwork = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected) {
            if (!canControlObject)
            {
                if (!isStoppedOnNetwork) MoveObjectFromNetworkUpdate();
                return;
            }

            if (!canMovedOnNetwork)
            {
                passedTimeForMovement += Time.deltaTime;
                if (passedTimeForMovement >= timeBeforeCanMoveOnNetwork)
                {
                    canMovedOnNetwork = true;
                }
                else return;
            }
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            inputMagnitude /= 2;
        }

        //animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);

        speed = inputMagnitude * maximumSpeed;
        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
        movementDirection.Normalize();

        ySpeed += Physics.gravity.y* Time.deltaTime;

        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpButtonPressedTime = Time.time;
            if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected) PersonalNetworkAgent.Instance.SendJump(networkId, "Player");
        }

        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            characterController.stepOffset = 0;
        }

        velocity = movementDirection * speed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            //animator.SetBool("isMoving", true);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
          //animator.SetBool("isMoving", false);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void MoveObjectFromNetworkUpdate()
    {
        //animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);

        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        if (isJumping)
        {
            jumpButtonPressedTime = Time.time;
            isJumping = false;
        }

        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            characterController.stepOffset = 0;
        }

        Quaternion targetQuaternion = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation.y, transform.rotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime);

        GameObject weaponReference = currentGunScript.GetCurrentGun();

        Vector3 weaponLookVector = new Vector3(weaponRotation, weaponReference.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        if (weaponReference != null) weaponReference.transform.rotation = Quaternion.Slerp(weaponReference.transform.rotation, Quaternion.Euler(weaponLookVector), rotationSpeed * Time.deltaTime);

        velocity.y = ySpeed;

        if (networkPositionReference == null)
        {
            networkPositionReference = NetworkManager.Instance.FindGameObjectInNetworkMap(networkId);
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    public void SetDataFromNetwork(Vector3 networkVelocity, float networkSpeed, Vector3 networkRotation, float networkWeaponRotation)
    {
        try
        {
            velocity = networkVelocity;
            speed = networkSpeed;
            rotation = networkRotation;
            weaponRotation = networkWeaponRotation;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void StopObject(Vector3 position, Vector3 rotation)
    {
        velocity = Vector3.zero;
        isStoppedOnNetwork = true;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
    }

    public void EnableJump()
    {
        isJumping = true;
    }

    public void FireWeapon(Vector3 shootingDirection)
    {
        currentGunScript.GetCurrentGun().GetComponent<Weapon>().FireNetworkWeapon(shootingDirection);
    }

    public void SetNetworkPositionReference()
    {
        networkPositionReference = NetworkManager.Instance.FindGameObjectInNetworkMap(networkId);
    }

    public void SetCanControlObject(bool condition)
    {
        canControlObject = condition;
    }

    public bool CanControlObject()
    {
        return canControlObject;
    }

    public void SwitchWeapon(string weaponString)
    {
        Debug.Log("Started Switching Weapon");
        currentGunScript.SetGunFromNetwork(weaponString);
    }

    public void UnsubscribeFromEvents()
    {
        if (!isDummyObject) NetworkManager.Instance.OnFixedNetworkUpdateEvent -= NetworkManager_OnFixedNetworkUpdate;
    }
}
