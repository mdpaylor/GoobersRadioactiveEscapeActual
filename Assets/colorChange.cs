using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colorChange : MonoBehaviour
{
    [SerializeField]
    Material material1;

    [SerializeField]
    Material material2;


    float duration = 2.0f;
    Renderer rend;

    void Start () {
      // At start, use the first material
      rend = GetComponent<Renderer>();
      rend.material = material1;
    }

    void Update () {
      // ping-pong between the materials over the duration
      float lerp = Mathf.PingPong (Time.time, duration) / duration;
      rend.material.Lerp (material1, material2, lerp);
    }
}
