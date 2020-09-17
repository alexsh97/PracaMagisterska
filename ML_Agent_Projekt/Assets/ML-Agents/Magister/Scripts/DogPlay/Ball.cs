using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    float speed = 0f;
    Vector3 dirToGo = Vector3.zero;
    Rigidbody rBody;
    bool isThrown = false;
    Transform pushedBy;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rBody.AddForce(dirToGo * speed);
        dirToGo = Vector3.zero;
    }

    public void GetPushed(Vector3 direction, float speed, Transform pushedBy)
    {
        this.speed = speed;
        this.dirToGo = direction;
        this.dirToGo.y = 15f;
        this.pushedBy = pushedBy;
    }

    public void Spawn()
    {
        this.transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), 3f, UnityEngine.Random.Range(-8f, 3f));
    }

    public bool IsThrown
    {
        set { isThrown = value; }
        get { return isThrown; }
    }

    public Transform PushedBy
    {
        set { PushedBy = value; }
        get { return PushedBy; }
    }
}
