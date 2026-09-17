using UnityEngine;

public class AspectRatioHandler : MonoBehaviour
{
    private Camera cam;

    private float minAspectWidth = 4;
    private float minAspectHeight = 3;

    private float maxAspectWidth = 22;
    private float maxAspectHeight = 9;

    private float minAspectRatio;
    private float maxAspectRatio;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        minAspectRatio = minAspectWidth / minAspectHeight;
        maxAspectRatio = maxAspectWidth / maxAspectHeight;

        UpdateAspect();
    }

    void Update()
    {
        UpdateAspect();
    }

    void UpdateAspect()
    {
        Rect rect = new Rect(0, 0, 1, 1);

        float currentAspectRatio = (float)Screen.width / (float)Screen.height;

        if (currentAspectRatio < minAspectRatio)
        {
            float scaleRatio = currentAspectRatio / minAspectRatio;

            rect.width = 1;
            rect.height = scaleRatio;
            rect.x = 0;
            rect.y = (1 - scaleRatio) / 2;
        }
        else if (currentAspectRatio > maxAspectRatio)
        {
            float scaleRatio = currentAspectRatio / maxAspectRatio;

            rect.width = 1 / scaleRatio;
            rect.height = 1;
            rect.x = (1 - rect.width) / 2;
            rect.y = 0;
        }

        cam.rect = rect;
    }
}