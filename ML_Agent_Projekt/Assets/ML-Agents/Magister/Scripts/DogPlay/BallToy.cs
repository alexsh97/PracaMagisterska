using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallToy : MonoBehaviour
{
    Rigidbody rBody;
    bool isThrown = false;
    
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void GetPushed(Vector3 direction, float speed, Transform pushedBy)
    {
        
        Vector3 dirToGo = direction;
        dirToGo.y = 15f;
        //this.pushedBy = pushedBy;
        isThrown = true;
        rBody.AddForce(dirToGo * speed);

        Debug.Log("Thrown the ball");
    }

    public bool IsThrown
    {
        set { isThrown = value; }
        get { return isThrown; }
    }


    public void MakeKinematik()
    {
        rBody.isKinematic = true;
    }

    public void Reset()
    {
        isThrown = true;
        rBody.isKinematic = false;
        this.transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), 3f, UnityEngine.Random.Range(-8f, 3f));
    }
}
