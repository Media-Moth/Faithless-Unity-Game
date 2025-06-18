using UnityEngine;

public class Jump : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    [SerializeField] float jumpPower = 5.0f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody rigid = transform.GetComponent<Rigidbody>();
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }
}
