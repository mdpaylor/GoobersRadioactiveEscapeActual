using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class orientation : MonoBehaviour
{
    public Transform camera;
    public Transform weapon;
    public Vector2 sense;
    private Vector2 xyRotation;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      Vector2 input = new Vector2
      {
        x = Input.GetAxis("Mouse X"),
        y = Input.GetAxis("Mouse Y")
      };
        xyRotation.x -= input.y * sense.y;
        xyRotation.y += input.x * sense.x;

        xyRotation.x = Mathf.Clamp(xyRotation.x, -90f, 90f);

        transform.eulerAngles = new Vector3(0f, xyRotation.y, 0f);

        camera.localEulerAngles = new Vector3(xyRotation.x, 0f, 0f);
        if (weapon != null ) weapon.localEulerAngles = new Vector3(xyRotation.x, weapon.rotation.eulerAngles.y, 0f); // Moves gun along with the player's sight
    }
}
