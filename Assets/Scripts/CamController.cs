using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [Range(0,1)]
    public float smoothTime;

    public Transform playerTransform;

    public void Spawn(Vector3 pos)
    {
        transform.position = pos;
    }

    public void FixedUpdate()
    {
        Vector2 pos = GetComponent<Transform>().position;

        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);
          
        GetComponent<Transform>().position = pos;
    }
}
