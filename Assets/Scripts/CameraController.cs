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
        float Yspeed = Input.GetAxisRaw("Vertical") * Time.fixedDeltaTime;
        float Xspeed = Input.GetAxisRaw("Horizontal") * Time.fixedDeltaTime;
        float Zspeed = Input.mouseScrollDelta.y * 1;

        transform.position = new Vector3(
            transform.position.x + Xspeed,
            transform.position.y + Yspeed,
            transform.position.z
        );
        Camera.main.orthographicSize -= Zspeed;
    }
}
