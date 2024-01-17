using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        float Yspeed = Input.GetAxisRaw("Vertical") * 1;
        float Xspeed = Input.GetAxisRaw("Horizontal") * 1;
        float Zspeed = Input.mouseScrollDelta.y * 1;

        transform.position = new Vector3(
            transform.position.x + Xspeed,
            transform.position.y + Yspeed,
            transform.position.z
        );
        Camera.main.orthographicSize -= Zspeed;
    }
}
