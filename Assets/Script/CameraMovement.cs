using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Vector3 initialPosition;
    [SerializeField] GameObject player;
    [SerializeField] float speed = 2.0f;
    float pixelsPerUnit = 16f;
    float unitsPerPixelX;
    float unitsPerPixelY;
    private Camera cam;
    [SerializeField] RenderTexture RenderTexture;
    void Start()
    {
        cam = GetComponent<Camera>();
        initialPosition = transform.position;

        cam.orthographicSize = RenderTexture.height / (2f * pixelsPerUnit);
    }
    //    Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0,0,0));
    //    Vector3 above = cam.ScreenToWorldPoint(new Vector3(0,1,0));
    //    Vector3 right = cam.ScreenToWorldPoint(new Vector3(1,0,0));

    //    unitsPerPixelX = Vector3.Distance(bottomLeft, right);
    //    unitsPerPixelY = Vector3.Distance(bottomLeft, above);
    //}

    //// Update is called once per frame
    //void SnapToPixelPerfect()
    //{
    //    Vector3 position = transform.position;
    //    Vector3 right = transform.right;
    //    Vector3 up = transform.up;
    //    Vector3 forward = transform.forward;

    //    float rightDot = Vector3.Dot(position, right);
    //    float upDot = Vector3.Dot(position, up);
    //    float forwardDot = Vector3.Dot(position, forward);

    //    float rightSnapped = Mathf.Round(rightDot / unitsPerPixelX) * unitsPerPixelX;
    //    float upSnapped = Mathf.Round(upDot / unitsPerPixelY) * unitsPerPixelY;

    //    Vector3 snappedPosition =
    //        right * rightSnapped +
    //        up * upSnapped +
    //        forwardDot * forward;
    //    transform.position = snappedPosition;
    //}
    void Update()
    {
        Vector3 Offset = initialPosition;
        transform.position = Vector3.MoveTowards( transform.position, Offset + player.transform.position, speed );
        //transform.position = new Vector3(
        //    Mathf.Round(transform.position.x / unitsPerPixelX) * unitsPerPixelX,
        //    transform.position.y,
        //    transform.position.z
        //);
        //SnapToPixelPerfect();
    }
}
