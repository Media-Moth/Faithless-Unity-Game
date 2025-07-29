using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Movement : MonoBehaviour
{
    void Start()
    {

        

    }

    [SerializeField] private float speed = 1.0f;
     // Update is called once per frame
    void Update()
    { // MOVEMENT VERY HARD ISN'T IT (ITS NOT)
        

        float deltatime = Time.deltaTime;

        float horizontal = Input.GetAxis("Horizontal") * deltatime * speed;
        float vertical = Input.GetAxis("Vertical") * deltatime * speed;

        Vector3 moveVector = Quaternion.Euler(0,-45,0) * new Vector3(horizontal, 0, vertical) ;

        transform.LookAt(transform.position + moveVector);
        transform.Translate(moveVector, Space.World);

    }
}
