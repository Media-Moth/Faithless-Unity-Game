using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        

    }
    [SerializeField] private float speed = 1.0f;
    // Update is called once per frame
    void Update()
    { // MOVEMENT VERY HARD ISN'T IT (ITS NOT)


        // this gets time amount of time from the last frame to this frame
        float deltatime = Time.deltaTime;

        float horizontal = Input.GetAxis("Horizontal") * deltatime * speed;
        float vertical = Input.GetAxis("Vertical") * deltatime * speed;

        Vector3 moveVector = new Vector3(horizontal, 0, vertical);

        transform.LookAt(transform.position + moveVector);
        transform.Translate(moveVector, Space.World);

    }
}
