using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public Vector3 target = new Vector3(0f, 0f, 0f);
    public float speed = 2f;

    private void Update() {
        transform.position = Vector2.MoveTowards(transform.position, target, Time.deltaTime * speed);
        if(transform.position == target) {
            Destroy(this);
        }
    }
}
