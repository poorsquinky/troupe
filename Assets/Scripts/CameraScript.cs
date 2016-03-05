using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    public Transform target = null;

    public float pixelsPerUnit = 16f;
    public int maxUnitsHigh = 30;

    private float smooth = 5;
    private Vector3 virtual_position = new Vector3(0,0,0);

    private Camera cam;

    public float scaleMultiplier = 1f;

    // Use this for initialization
    void Start () {
        virtual_position = transform.position;
        cam = GetComponent<Camera>();

        float o = Screen.height / (2f * pixelsPerUnit);
        while (o > (float)maxUnitsHigh / 2f && scaleMultiplier < 1000f)
        {
            scaleMultiplier += 1f;
            o = Screen.height / (2f * pixelsPerUnit * scaleMultiplier);
        }
        cam.orthographicSize = o;

    }

    void Update () {

        if (target)
        {
            virtual_position = new Vector3(Mathf.Lerp(virtual_position.x,target.position.x,Time.deltaTime*this.smooth), Mathf.Lerp(virtual_position.y,target.position.y,Time.deltaTime*this.smooth), -100);
            Vector3 new_pos = transform.position;
            // The camera position is locked to screen pixels in order to stop moire-type distortion
            new_pos.x = Mathf.Round(virtual_position.x * pixelsPerUnit * scaleMultiplier) / (pixelsPerUnit * scaleMultiplier);
            new_pos.y = Mathf.Round(virtual_position.y * pixelsPerUnit * scaleMultiplier) / (pixelsPerUnit * scaleMultiplier);
            transform.position = new_pos;
        }

    }
}
