using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingPlaceController : MonoBehaviour
{
    public int id;
    bool isActive;//Czy jest celem agenta, czy nie

    void Start()
    {
        this.transform.Translate(0,-10f,0);  
    }

    public void Activate()
    {
        if (!isActive)
        {
            isActive = true;
            this.transform.Translate(0, 10f, 0);
        }
    }

    public void Deactivate()
    {
        if (isActive)
        {
            isActive = false;
            this.transform.Translate(0, -10f, 0);
        }
    }

    public int ID
    {
        set { id = value; }
        get { return id; }
    }

    public bool IsActive
    {
        set { isActive = value; }
        get { return isActive; }
    }

}

