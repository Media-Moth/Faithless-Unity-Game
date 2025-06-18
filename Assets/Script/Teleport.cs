using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
    }
    Camera cam;
    [SerializeField] RawImage renderTexture;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // screen to ray doesn't work because of rendertexture
            Vector3 mousePos = Input.mousePosition;
            mousePos.x /= Screen.width;
            mousePos.y /= Screen.height;
            Ray ray = cam.ViewportPointToRay(mousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitPos = hit.point; 
                transform.position = new Vector3(hitPos.x, transform.position.y, hitPos.z);
            }

            
        }
    }
}
