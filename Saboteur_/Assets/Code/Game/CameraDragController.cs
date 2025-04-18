using UnityEngine;

public class CameraDragController : MonoBehaviour
{
    public float dragSpeed = 2f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;
    public Transform boundRoot; // 绑定Back父物体

    private Vector3 dragOrigin;
    private Vector2 minBound;
    private Vector2 maxBound;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (boundRoot != null)
        {
            Bounds totalBounds = new Bounds(boundRoot.position, Vector3.zero);
            foreach (Renderer r in boundRoot.GetComponentsInChildren<Renderer>())
            {
                totalBounds.Encapsulate(r.bounds);
            }

            minBound = totalBounds.min;
            maxBound = totalBounds.max;
        }
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = cam.transform.position + difference;

            // 限制相机位置在背景范围内
            newPos.x = Mathf.Clamp(newPos.x, minBound.x, maxBound.x);
            newPos.y = Mathf.Clamp(newPos.y, minBound.y, maxBound.y);
            newPos.z = cam.transform.position.z;

            cam.transform.position = newPos;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}
