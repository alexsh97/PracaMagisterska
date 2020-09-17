using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using System;


public class DogAgent : Agent
{
    Rigidbody rBody;
    public GameObject ballTarget;
    public GameObject Player;
    bool isHaveBall = false;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isHaveBall)
        {
            ballTarget.transform.localPosition = this.transform.localPosition + this.transform.forward + this.transform.up;
            ballTarget.GetComponent<BallToy>().MakeKinematik();

        }
        if(Vector3.Distance(this.transform.localPosition, Player.transform.localPosition) < 3f && isHaveBall)
        {
            isHaveBall = false;
            //ballTarget.GetComponent<BallToy>().IsThrown = false;
            Debug.Log("Give back the ball!");
            SetReward(2f);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        isHaveBall = false;
        this.transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), 3f, UnityEngine.Random.Range(-8f, 3f));
        Player.transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), 3f, UnityEngine.Random.Range(-8f, 3f));
        ballTarget.GetComponent<BallToy>().Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isHaveBall ? 1 : 0);

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        AddReward(isHaveBall ? -0.0001f : -0.0002f);

        Vector3 dirToGo = transform.forward * Mathf.Clamp(vectorAction[0], -1, 1);
        float rotationAction = vectorAction[1];

        transform.eulerAngles += new Vector3(0, (rotationAction * 90) * 0.02f, 0);
        //transform.Rotate(rotationAction*this.transform.up, Time.deltaTime * 200f);
        rBody.AddForce(dirToGo * speed, ForceMode.VelocityChange);
        Debug.Log("Translate: "+ vectorAction[0]+ " rotation "+ vectorAction[1]);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[0] = Input.GetAxis("Vertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("bench") || collision.gameObject.CompareTag("cola"))
        {
            SetReward(-2f);
            EndEpisode();
        }
        
        if (collision.gameObject.CompareTag("ball") && ballTarget.GetComponent<BallToy>().IsThrown)
        {
            Debug.Log("Get the ball");
            AddReward(0.5f);
            isHaveBall = true;
        }
    }
}
