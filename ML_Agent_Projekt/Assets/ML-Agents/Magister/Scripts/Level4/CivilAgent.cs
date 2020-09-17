using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;
using Barracuda;

/// <summary>
/// Klasa reprezentująca mieszkańca miasta
/// </summary>
public class CivilAgent : Agent
{
    public int id;
    public TownController level;//środowisko w którym agent się znajduje
    public NNModel calm; //spokojny model zachowania
    public NNModel afraid; // przestraszony model zachowania
    Rigidbody rBody;//właściwości fizyczne
    public float speed;//prędkość
    public float health;//poziom życia
    public GameObject goal;//punkt docelowy
    public int actualMission;//aktualne zadanie
    bool isAfraid;//określa zachowanie agenta, czy jest spokojny, czy przestraszony
    float oldDistanceToTarget, newDistanceToTarget;//stara i nowa odległość agent od celu

    /// <summary>
    /// Inicjalizacja agenta
    /// </summary>
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        health = 100f;
        isAfraid = false;
        actualMission = 0;
    }

    /// <summary>
    /// Funkcja aktualizacji obiektu co klatkę
    /// Sprawdzenie czy agent jest blisko celu, jeśli ma spokojny typ zachowania
    /// Sprawdzenie poziomu życia w przypadku przestraszonego stanu, aby zmienić model zachowania
    /// </summary>
    void FixedUpdate()
    {
        if(newDistanceToTarget < 1.5f)
        {
            SetReward(2f);
            Debug.Log("Goal");
            actualMission = UnityEngine.Random.Range(0, 4);
            EndEpisode();
        }

        if(isAfraid && health >= 95f)
        {
            isAfraid = false;
            SetModel("CivilBehavior", calm);
            
        }
    }

    /// <summary>
    /// Inicjalizacja wszystkich parametrów na początku nowego epizodu
    /// Nadawanie celu
    /// Inaczej mówiąc resetacja agenta
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Debug.Log("Mission "+actualMission);
        //isAfraid = true;
        //this.transform.localPosition = level.SpawnNewPosOnLevel();
        //this.transform.Translate(0,2,0);
        goal = level.GetPlace();
        //actualMission = UnityEngine.Random.Range(0, 4);
        health = UnityEngine.Random.Range(1, 8) * 10f;

        if (goal != null)
        {
            oldDistanceToTarget = 0;
            newDistanceToTarget = Vector3.Distance(this.transform.localPosition, goal.transform.localPosition);
        }
    }

    /// <summary>
    /// Zbieranie obserwacji agenta jako dane wejściowe sieic neuronowe
    /// </summary>
    /// <param name="sensor">Wektor obserwacji do którego przekazujemy dane</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        if(isAfraid)
        {
            sensor.AddObservation(health / 100f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
        else
        {
            sensor.AddObservation(actualMission);

            if (actualMission <= 1)
            {
                sensor.AddObservation((int)(this.transform.localPosition.x - 
                                                goal.transform.localPosition.x));
                sensor.AddObservation((int)(this.transform.localPosition.z - 
                                                goal.transform.localPosition.z));
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

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
        if (health > 0)
        {
            MoveAgent(vectorAction[0]);
            RotateAgent(vectorAction[1]);

            CheckGround();
            if((actualMission == 0 || actualMission == 1) && goal != null)
            {
                CheckDistanceToTarget();
            }
            else
            {
                AddReward(-0.0005f);
            }

            if(isAfraid)
            {
                AddReward(0.05f); 
                health += 0.05f;
            }
                
        }
        if (transform.localPosition.y < 0 || health <=0 )
        {
            SetReward(-2f);
            Debug.Log("DEAD");
            EndEpisode();
        }
    }

    /// <summary>
    /// Poruszanie się agenta w wybranym kierunku (do przodu, do tyłu)
    /// </summary>
    /// <param name="actions">Akcje określający kierunek poruszania się</param>
    private void MoveAgent(float action)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

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
    /// </summary>
    /// <param name="action">Akcja określająca kierunek obracania się</param>
    private void RotateAgent(float action)
    {
        var rotationAction = Mathf.FloorToInt(action);
        float rotationAct = 0;

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

    /// <summary>
    /// Sprawdzenie czy agent porusza się w dozwolonych miejscach: droga
    /// W przypadku poruszania się pochodzniku, albo przejazd na światło czerwona - agent dostaje karę
    /// </summary>
    private void CheckGround()
    {
        Vector3 rayPos = transform.position;
        rayPos.y -= 2f;
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
                //Debug.Log("Good Ground!");
            }
            if (hit.collider.gameObject.CompareTag("road"))
            {
                AddReward(isAfraid ? -0.001f : -0.05f);
                //Debug.Log(id+" Wrong road!");
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
            AddReward(-0.005f);
        }
        else if(oldDistanceToTarget > newDistanceToTarget)
        {
            AddReward(0.005f);
        }

        oldDistanceToTarget = newDistanceToTarget;
        newDistanceToTarget = Vector3.Distance(this.transform.localPosition, goal.transform.localPosition);
    }

    /// <summary>
    /// Obsługa klawisz klawiatury w celu sterowania agentem w przypadku braku modelu zachowania
    /// </summary>
    /// <param name="actionsOut">wektor akcji</param>
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        actionsOut[1] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 1f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 2f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2f;
        }
    }

    /// <summary>
    /// Obsługa kolizji agenta z obiektami w środowisku
    /// </summary>
    /// <param name="collision">obiekt z którym jest kolizja</param>
    private void OnCollisionEnter(Collision collision)
    {
       if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("car"))
        {
            SetReward(-2f);
            Debug.Log("wall");
            //EndEpisode();
        }

       if(actualMission == 2  && collision.gameObject.CompareTag("bench"))
       {
            SetReward(2f);
            Debug.Log("Bench");
            actualMission = UnityEngine.Random.Range(0, 4);
            EndEpisode();
        }
       if (actualMission == 3 && collision.gameObject.CompareTag("cola"))
       {
            SetReward(2f);
            Debug.Log("Cola");
            actualMission = UnityEngine.Random.Range(0, 4);
            EndEpisode();
        }
    }

    /// <summary>
    /// Obsługa zdarzeń z triggerami
    /// przejście na światło czerwone
    /// </summary>
    /// <param name="other">trigger z którym jest kolizja</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("redlight"))
        {
            AddReward(isAfraid ? -0.001f : -0.05f);
        }
        if (other.CompareTag("greenlight"))
        {
            AddReward(0.001f);
        }
    }

    /// <summary>
    /// Odbieranie zadanych uszkodzeń
    /// </summary>
    /// <param name="damage">wartość uszkodzeń</param>
    public void BeenAttacked(float damage)
    {
        isAfraid = true;
        SetModel("CivilBehavior", afraid);
        health -= damage;
        health = health <= 0 ? 0 : health;
    }

    public float Health
    {
        set { health = value; }
        get { return health; }
    }
}
