using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    [SerializeField] GameObject player;
    [SerializeField] float speed = 2.0f;
    // Update is called once per frame
    void Update()
    {
        Vector3 Offset = new Vector3(0, 4.92f, -8.65f);
        transform.position = Vector3.MoveTowards(transform.position, Offset + player.transform.position, speed);

    }
}
