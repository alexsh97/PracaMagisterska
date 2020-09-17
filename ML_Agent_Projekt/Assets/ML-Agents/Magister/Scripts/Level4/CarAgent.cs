using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

/// <summary>
/// Klasa reprezentująca samochód
/// </summary>
public class CarAgent : Agent
{
    public int idCar;//id samochodu
    public TownController level;//środowisko w którym agent się znajduje
    Rigidbody rBody;//właściwości fizyczne
    public float speed;//prędkość
    GameObject parkingPlace;//docelowe miejsce parkingowe
    public int parkingPlaceId;
    bool isStop;
    bool isOk;
    float oldDistanceToTarget, newDistanceToTarget;//stara i nowa odległość agent od celu
    float parkingTimer;
    /// <summary>
    /// Inicjalizacja agenta
    /// </summary>
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        parkingTimer = 0f;
    }

    void FixedUpdate()
    {
        parkingTimer += Time.deltaTime;
        if(parkingTimer > 5f)
        {
            isStop = false;
            
        }
    }
    /// <summary>
    /// Inicjalizacja wszystkich parametrów na początku nowego epizodu
    /// Nadawanie celu
    /// Inaczej mówiąc resetacja agenta
    /// </summary>
    public override void OnEpisodeBegin()
    {
        
        //this.transform.localPosition = level.SpawnNewPosOnLevel();
        if(this.parkingPlace != null)
        {
            this.parkingPlace.GetComponent<ParkingPlaceController>().Deactivate();
        }
        this.parkingPlace = level.GetParkingPlace();
        this.parkingPlace.GetComponent<ParkingPlaceController>().Activate();
        this.parkingPlaceId = this.parkingPlace.GetComponent<ParkingPlaceController>().ID;

        oldDistanceToTarget = 0;
        newDistanceToTarget = Vector3.Distance(this.transform.localPosition, this.parkingPlace.transform.localPosition);
    }

    /// <summary>
    /// Zbieranie obserwacji agenta, będącymi danymi wejściowymi sieci neuronowej
    /// W zależności od obecnego modelu zachowania obserwacją jest położenie agenta
    /// ze względu na miejsce parkingowe
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
       
        sensor.AddObservation(isOk ? 0 : 1);

        sensor.AddObservation((int)(this.transform.localPosition.x - this.parkingPlace.transform.localPosition.x));

        sensor.AddObservation((int)(this.transform.localPosition.z - this.parkingPlace.transform.localPosition.z));

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

    }

    /// <summary>
    /// Wykonanie akcji: ruchu, obracania się
    /// Sprawdzenie odległości agenta od celu
    /// Sprawdzenie czy porusza się w dozwolonych miejscach
    /// </summary>
    /// <param name="vectorAction">Wektor akcji</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        //MoveAgent(vectorAction);
        MoveDiscrete(vectorAction[0]);
        RotateDiscrete(vectorAction[1]);
        
        CheckGround();
        CheckDistanceToTarget();

        if (transform.localPosition.y < -1)
        {
            SetReward(-2f);
            EndEpisode();
        }
    }

    /// <summary>
    /// Poruszanie się agenta w wybranym kierunku oraz obraca się 
    /// Przypadek ciągłej przestrzeni akcji
    /// </summary>
    /// <param name="actions">Akcje określający kierunek poruszania się oraz obrotu</param>
    public void MoveAgent(float[] vectorAction)
    {
        var dirForward = Vector3.zero;
        var dirBackward = Vector3.zero;
        var dirToGo = Vector3.zero;
        float rotationAct;

        dirForward = transform.forward * Mathf.Clamp(vectorAction[0], 0, 1);
        dirBackward = transform.forward * Mathf.Clamp(vectorAction[1], 0, 1);
        dirToGo = dirForward - dirBackward;

        rotationAct = Mathf.Clamp(vectorAction[2], -1, 1);

        transform.eulerAngles += new Vector3(0, (rotationAct * 90) * 0.02f, 0);
        rBody.AddForce(dirToGo * speed, ForceMode.VelocityChange);
  
    }

    /// <summary>
    /// Poruszanie się agenta w wybranym kierunku (do przodu, do tyłu)
    /// Przypadek dyskretnej przestrzeni akcji
    /// </summary>
    /// <param name="actions">Akcje określający kierunek poruszania się</param>
    public void MoveDiscrete(float action)
    {
        var dirToGo = Vector3.zero;
        var forwardAction = Mathf.FloorToInt(action);

        switch (forwardAction)
        {
            case 1:
                dirToGo = transform.forward * 1f;

                break;
            case 2:
                dirToGo = transform.forward * -1f;

                break;
        }
        
        rBody.AddForce(dirToGo * speed, ForceMode.VelocityChange);

    }

    /// <summary>
    /// Wykonanie akcji obracania się w prawo albo w lewo
    /// Przypadek dyskretnej przestrzeni akcji
    /// </summary>
    /// <param name="action">Akcja określająca kierunek obracania się</param>
    public void RotateDiscrete(float action)
    {
        float rotationAct = 0;
        var rotationAction = Mathf.FloorToInt(action);

        switch (rotationAction)
        {
            case 1:
                rotationAct = 1;
                break;
            case 2:
                rotationAct = -1;
                break;
        }

        transform.eulerAngles += new Vector3(0, (rotationAct * 90) * 0.02f, 0);
    }
    
    private void CheckMarkup()
    {
        isOk = true;
        isStop = false;
        Vector3 rayPos = transform.position + transform.forward*1.5f;
        Vector3 rayDir = transform.right;
        float rayLength = 2f;
        //rayPos.z += 3;
        Ray r = new Ray(rayPos, rayDir);
        RaycastHit hit;
        Debug.DrawRay(rayPos, rayLength*rayDir, Color.red, 0f, true);
        if (Physics.Raycast(r, out hit, rayLength))
        {
            if (hit.collider.gameObject.CompareTag("markup"))
            {
                isOk = false;
                AddReward(-0.0005f);
                Debug.Log("WRONG SIDE!");
            }
        }
    }

    /// <summary>
    /// Sprawdzenie czy agent porusza się w dozwolonych miejscach: droga
    /// W przypadku poruszania się pochodzniku, albo przejazd na światło czerwona - agent dostaje karę
    /// </summary>
    private void CheckGround()
    {
        isOk = false;
        Vector3 rayPos = transform.position + transform.forward;
        Vector3 rayDir = -transform.up;
        float rayLength = 1f;
        rayPos.y += 2;
        Ray r = new Ray(rayPos, rayDir);
        RaycastHit hit;
        Debug.DrawRay(rayPos, rayLength * rayDir, Color.red, 0f, true);

        if (Physics.Raycast(r, out hit, rayLength))
        {
            if (hit.collider.gameObject.CompareTag("ground"))
            {
                AddReward(-0.005f);
                //Debug.Log("WRONG Ground!");
            }
            if (hit.collider.gameObject.CompareTag("road"))
            {
                //AddReward(0.0005f);
                //Debug.Log("Good road!");
            }
        }
    }

    /// <summary>
    /// Obliczenie nowej odległości i starej od celu agenta
    /// po każdym kroku
    /// Gdy agent się zbliża do celu - dostaje nagrodę
    /// Gdy się oddala - karę
    /// </summary>
    private void CheckDistanceToTarget()
    {
        if (oldDistanceToTarget < newDistanceToTarget)
        {
            AddReward(-0.0005f);
        }
        else if (oldDistanceToTarget > newDistanceToTarget)
        {
            AddReward(0.0005f);
        }

        oldDistanceToTarget = newDistanceToTarget;
        newDistanceToTarget = Vector3.Distance(this.transform.localPosition, this.parkingPlace.transform.localPosition);
    }

    /// <summary>
    /// Obsługa klawisz klawiatury w celu sterowania agentem w przypadku braku modelu zachowania
    /// </summary>
    /// <param name="actionsOut">wektor akcji</param>
    public override void Heuristic(float[] actionsOut)
    {
        
        actionsOut[0] = 0;
        actionsOut[1] = 0;
        actionsOut[2] = 0;

        if (!isStop)
        {
            if (Input.GetKey(KeyCode.D))
            {
                actionsOut[2] = 1f;
                //actionsOut[1] = 1f;
            }
            if (Input.GetKey(KeyCode.W))
            {
                actionsOut[0] = 10f;
                //actionsOut[0] = 1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                actionsOut[2] = -1f;
                //actionsOut[1] = 2f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                actionsOut[1] = 0.5f;
                //actionsOut[0] = 2f;
            }
        }
    }

    /// <summary>
    /// Obsługa kolizji agenta z obiektami w środowisku
    /// </summary>
    /// <param name="collision">obiekt z którym jest kolizja</param>
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("bench") || collision.gameObject.CompareTag("cola") || collision.gameObject.CompareTag("npc"))
        {
            SetReward(-2f);
            //EndEpisode();
        }
               
    }

    /// <summary>
    /// Obsługa zdarzeń z triggerami
    /// Kolizja ze ścianą
    /// Przyjście do miejsca parkingowego
    /// </summary>
    /// <param name="other">trigger z którym jest kolizja</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("parking") && 
            this.parkingPlaceId == other.GetComponent<ParkingPlaceController>().ID)
        {

            Debug.Log("PARKING ");
            parkingTimer = 0;
            isStop = true;
            SetReward(2f);
            EndEpisode();
            
        }

        if (other.CompareTag("greenlight"))
        {
            Debug.Log("Wrong light");
            AddReward(-0.005f);
        }
    }

    
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        if (isStop)
        {
            actionMasker.SetMask(0, new int[2] { 1, 2 });
            actionMasker.SetMask(1, new int[2] { 1, 2 });
        }
    }
}
