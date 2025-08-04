using UnityEngine;

public class HideObstructingWalls : MonoBehaviour
{
    // Update is called once per frame
    void Start()
    {
        cam = Camera.main;
    }
    Camera cam;
    [SerializeField] GameObject player;

    void Update()
    {
        Vector3 position = player.transform.position;
        Ray ray = new Ray(position, transform.position);
        RaycastHit hit;
        GameObject lastWall = null;
        Debug.DrawLine(ray.origin, ray.direction * 100, Color.red);
        if (Physics.Raycast(ray, out hit))
        {
            //Debug.Log("a");
            if (hit.collider && hit.collider.gameObject != lastWall)
            {
                if (lastWall != null)
                {
                    Debug.Log("b");
                    lastWall.SetActive(true);
                }
                Debug.Log("hit");
                lastWall = hit.collider.gameObject;
                lastWall.SetActive(false);// .GetComponent<MeshRenderer>.enabled = false;
            } 
        }
    }
}
