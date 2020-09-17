using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;
using Barracuda;
using System;

/// <summary>
/// Klasa opisująca agenta - ochroniarza
/// Dziedziczy po klasie Agent
/// Ma implementacje funkcji dla każdego etapu uczenia ze wzmocnieniem
/// oraz funkcje do wykonania akcji, obsługi kolizji, interakcji
/// </summary>
public class GuardAgent : Agent
{
    public int idGuard;//Id agenta
    public NNModel passive; //pasywny model zachowania
    public NNModel agressive; // agresuwny model zachowania
    public LabController level;//środowisko w którym się znajduje
    public GameObject[] guardPosts;// wybrane punkty kontrolne dla ochroniarza
    GameObject target;//obecny cel
    Rigidbody rBody; //właściwości fizyczne
    float health;// poziom życia
    public float speed; //prędkość poruszania się
    int currentMission;//obecne zadanie 
    int typeOfBehavior = 0;// 0 - passive; 1 - agressive
    bool onGround = true;// czy jest na powierzchni czy w powietrzu
    GuardWeapon weapon;
    
    float oldDistanceToTarget, newDistanceToTarget;// stara i nowa odległośc do celu
    float fieldOfView;//pole widzenia
    int post_num;//indeks dostępnego punktu kontrolnego
    int idTarget;//id punktu kontrolnego
    Vector3 playerLastSeen;//położenie w którym ostatnio widziano gracza
    
    /// <summary>
    /// Inicjalizacja agenta
    /// </summary>
    public override void Initialize()
    {
        currentMission = 0;
        rBody = GetComponent<Rigidbody>();
        weapon = GetComponent<GuardWeapon>();
        fieldOfView = 120f;
        post_num = -1;
        playerLastSeen = Vector3.zero;
        health = 10;
    }
    
    /// <summary>
    /// Funkcja wywołana co klatke
    /// Liczemoe timera osobistego
    /// Identyfikacja gracza
    /// Sprawdzenie czy był wywołany alarm, 
    /// aby zmienić model zachowania na agresywny
    /// </summary>
    void FixedUpdate()
    {
        if (level.player != null)
        {
            if (Vector3.Distance(this.transform.localPosition, level.player.transform.localPosition) < 15f )
            {
                IdentifyPlayer(level.player);
            }
        }

        if(level.IsAlarmRaised && typeOfBehavior == 0)
        {
            typeOfBehavior = 1;
            SetModel("GuardBehavior", agressive); //zmiana modelu zachowania
        }
    }

    /// <summary>
    /// Inicjalizacja wszystkich parametrów odnowa na początku nowego epizodu
    /// Inaczej mówiąc resetacja agenta
    /// </summary>
    public override void OnEpisodeBegin()
    {
        foreach (GameObject g in guardPosts)
        {
            g.GetComponent<GuardPost>().Deactivate();
        }

        onGround = true;
        //currentMission =  UnityEngine.Random.Range(0, 2);
        weapon.Ammo = UnityEngine.Random.Range(3, weapon.MaxAmmo);
        weapon.IsAttacking = false;
        //health = UnityEngine.Random.Range(3, 10);
        //this.transform.localPosition = level.SpawnNewPosOnLevel();

        if (typeOfBehavior == 0)
        {
            if(currentMission == 0)
            {
                //Debug.Log("Need post");
                post_num = ++post_num < guardPosts.Length ? post_num : 0;
                target = guardPosts[post_num];
                target.GetComponent<GuardPost>().MakeActive();
                idTarget = target.GetComponent<GuardPost>().id;
            }else if(currentMission == 1)
            {
                //Debug.Log("Need alarm");
                target = level.alarm;
            }
                        
        }
        else 
        {
            target = level.player;
        }

        newDistanceToTarget = Vector3.Distance(this.transform.localPosition, target.transform.localPosition);
        oldDistanceToTarget = 0f;

    }

    /// <summary>
    /// Zbieranie obserwacji agenta, będącymi danymi wejściowymi sieci neuronowej
    /// W zależności od obecnego modelu zachowania obserwacją jest położenie agenta
    /// ze względu na punkt kontrolny (lub alarm) albo ostatnie widoczne połozenie gracza 
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(currentMission);
        
        sensor.AddObservation((int)health);
        sensor.AddObservation(weapon.Ammo > 0f ? 1f : 0f);
        
        if(typeOfBehavior == 0)
        {
            sensor.AddObservation((int)(this.transform.localPosition.x - target.transform.localPosition.x));
            sensor.AddObservation((int)(this.transform.localPosition.y - target.transform.localPosition.y));
            sensor.AddObservation((int)(this.transform.localPosition.z - target.transform.localPosition.z));
        }
        else
        {
            sensor.AddObservation((int)(this.transform.localPosition.x - playerLastSeen.x));
            sensor.AddObservation((int)(this.transform.localPosition.y - playerLastSeen.y));
            sensor.AddObservation((int)(this.transform.localPosition.z - playerLastSeen.z));
        }
        
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    /// <summary>
    /// Ustawienie maski akcji (zakaz wykonanie wybranych akcji)
    /// W przypadku pasywnego modelu zachowania jest zakazane wykonanie skoku, bo wcale nie ma w tym sensu
    /// Też jest zakaz zbędnego strzelania, bo nie ma w kogo.
    /// </summary>
    /// <param name="actionMasker"></param>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        
        if (!onGround || typeOfBehavior==0)
        {
            actionMasker.SetMask(2, new int[1] { 1 });
        }
        if(typeOfBehavior == 0 || !weapon.IsReadyToBeUsed())
        {
            actionMasker.SetMask(3, new int[1] { 1 });
        }
    }

    /// <summary>
    /// Wykonanie akcji: ruchu, obracania się, ataku
    /// Jeśli poziom zycia jest poniżej 5, wtedy agent dostaje kare
    /// W przypadku braku życia kończy epizod z wynikiem negatywnym
    /// </summary>
    /// <param name="vectorAction">Wektor akcji</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        if (health > 0)
        {
            MoveAgent(vectorAction);
            RotateAgent(vectorAction[1]);
            
            if (typeOfBehavior != 0 && Mathf.FloorToInt(vectorAction[3]) == 1)
            {
                Attack();
            }

            if (health < 5)
            {
                AddReward(-0.0001f);
            }
            if(weapon.Ammo <= 0)
            {
                AddReward(-0.0001f);
            }

            CheckDistanceToTarget();

        }
        if (health <= 0 || transform.localPosition.y < -10f)
        {
            SetReward(-2f);
            Debug.Log("Guard death");
            //EndEpisode();
        }
    }

    /// <summary>
    /// Poruszanie się agenta w wybranym kierunku, w tym skoki
    /// </summary>
    /// <param name="actions">Akcje określający kierunek poruszania się</param>
    private void MoveAgent(float[] actions)
    {
        var dirToGo = Vector3.zero;

        var forwardAction = Mathf.FloorToInt(actions[0]);
        var jumpAction = Mathf.FloorToInt(actions[2]);
        
        switch (forwardAction)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                dirToGo = transform.right * 1f;
                break;
            case 4:
                dirToGo = transform.right * -1f;
                break;
        }

        onGround = GroundCheck();
        if (jumpAction == 1 && onGround)
        {
            onGround = false;
            dirToGo *= 0.5f;
            dirToGo.y = 15f;
        }
        else
        {
            dirToGo.y = -0.1f;
        }
        if(!onGround)
        {
            dirToGo.x /= 3f;
            dirToGo.z /= 3f;
            
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
        var rotateDir = Vector3.zero;

        switch (rotationAction)
        {
            case 1:
                rotateDir = transform.up * 1f;
                break;
            case 2:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
    }


    /// <summary>
    /// Wykonanie akcji ataku
    /// Jeśli jest amunicja, wtedy agent strzela
    /// W przeciwnym wypadku walka z bliska
    /// Może trafić w gracza i kotów, zarówno w innego ochroniarza
    /// </summary>
    private void Attack()
    {
        weapon.TryToAttack();
        Vector3 rayPos = transform.position;
        rayPos.y -= 0.5f;
        Vector3 rayDir = transform.forward;
        Ray r = new Ray(rayPos, rayDir);
        float rayLength = weapon.Ammo > 0 ? 15f : 2f;
        RaycastHit hit;
        Debug.DrawRay(rayPos, rayLength * rayDir, Color.red, 0f, true);

        if (Physics.Raycast(r, out hit, rayLength))
        {
            string hittedTag = hit.collider.gameObject.tag;

            if (hittedTag.Equals("Player") || hittedTag.Equals("cat"))
            {
                Debug.Log("see victim");
                currentMission = 1;
                AddReward(0.0001f);
                if (weapon.IsAttacking)
                {
                    Debug.Log("Guard hit "+hittedTag);
                    AddReward(0.1f);
                    CatAgent targetToAttack= hittedTag.Equals("Player") ? 
                        level.player.GetComponent<PlayerController>() : hit.collider.gameObject.GetComponent<CatAgent>();
                    targetToAttack.BeenAttacked(2f);
                    targetToAttack.Freez();

                    if (targetToAttack.Health <= 0)
                    {
                        //level.ResetLevel();
                        SetReward(2f);
                        EndEpisode();
                    }
                }

            }
            else if (hittedTag.Equals("guard"))
            {
                currentMission = 0;
                if (weapon.IsAttacking)
                {
                    Debug.Log("Guard hit guard");
                    AddReward(-0.1f);
                    GuardAgent targetToAttack = hit.collider.gameObject.GetComponent<GuardAgent>();
                    targetToAttack.BeenAttacked(2f);
                }

            }
            else
            {
                currentMission = 0;
                if (weapon.IsAttacking)
                {
                    AddReward(-0.0005f);
                    Debug.Log("MISS");
                }
            }

        }

        weapon.IsAttacking = false;
    }
    /// <summary>
    /// Obliczenie nowej odległości i starej od celu agenta
    /// po każdym kroku
    /// Gdy agent się zbliża do celu - dostaje nagrodę
    /// Gdy się oddala - karę
    /// </summary>
    private void CheckDistanceToTarget()
    {
        if (typeOfBehavior == 0)
        {
            if (oldDistanceToTarget <= newDistanceToTarget)
            {
                AddReward(-0.001f);
            }
            else
            {
                AddReward(0.001f);
                Debug.Log("Good");
            }
        }

        oldDistanceToTarget = newDistanceToTarget;
        newDistanceToTarget = Vector3.Distance(this.transform.localPosition, target.transform.localPosition);
    }
    

    /// <summary>
    /// Sprawdzenie czy jest pod agentem jakaś powieszchnia
    /// </summary>
    /// <returns></returns>
    private bool GroundCheck()
    {
        Vector3 rayPos = transform.position;
        rayPos.y -= 0.5f;
        float rayLength = 0.9f;

        RaycastHit hit;
        Vector3 rayDir = -transform.up + transform.forward/2f;
        Ray ray = new Ray(rayPos, rayDir);
        Debug.DrawRay(rayPos, rayLength * rayDir, Color.red, 0f, true);
        if (Physics.Raycast(ray, out hit, rayLength))
        {
                return true;
        }

        rayDir = -transform.up + transform.forward / 2f;
        ray = new Ray(rayPos, rayDir);
        Debug.DrawRay(rayPos, rayLength * rayDir, Color.red, 0f, true);
        if (Physics.Raycast(ray, out hit, rayLength))
        {
            return true;
        }

        return false;
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
        actionsOut[3] = 0;

        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[0] = 3f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[0] = 4f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2f;
        }


        if (Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[1] = 1f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[1] = 2f;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            actionsOut[2] = 1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            actionsOut[3] = 1f;
        }
    }

    /// <summary>
    /// Obsługa kolizji agenta z obiektami w środowisku
    /// </summary>
    /// <param name="collision">obiekt z którym jest kolizja</param>
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("wall") || (typeOfBehavior == 0 && collision.gameObject.CompareTag("stone")))
        {
            SetReward(-2f);
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("ammo"))
        {
            var ammoKit = collision.gameObject.GetComponent<ResourceGetter>();
            weapon.Ammo += ammoKit.GetValueToAdd();
            Debug.Log("Get Ammo");
        }
        if (collision.gameObject.CompareTag("medecine"))
        {
            var medecineKit = collision.gameObject.GetComponent<ResourceGetter>();
            health += medecineKit.GetValueToAdd();
            health = health >= 10 ? 10 : health;
            Debug.Log("Get first aid");
        }        
        if (typeOfBehavior == 0 && currentMission == 1 && collision.gameObject.CompareTag("alarm"))
        {
            Debug.Log("Guard Alarm");
            //level.ResetLevel();
            level.IsAlarmRaised = true;
            //SetReward(2f);
            //EndEpisode();
        }
    }

    /// <summary>
    /// Obsługa zdarzeń z triggerami
    /// Otwarcie drzwi
    /// Przyjście do punktu kontrolnego
    /// </summary>
    /// <param name="other">trigger z którym jest kolizja</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("door"))
        {
            DoorController door = other.gameObject.GetComponent<DoorController>();
            door.OpenDoor();
        }

        if (typeOfBehavior == 0 && currentMission == 0 && other.gameObject.CompareTag("target") && other.gameObject.GetComponent<GuardPost>().id == idTarget)
        {
            Debug.Log("Guard Post");
            target.GetComponent<GuardPost>().Deactivate();
            SetReward(2f);
            EndEpisode();
        }
    }

    /// <summary>
    /// Odbieranie zadanych uszkodzeń
    /// </summary>
    /// <param name="damage">wartość uszkodzeń</param>
    public void BeenAttacked(float damage)
    {
        health -= damage;
        health = health <= 0 ? 0 : health;
    }


    /// <summary>
    /// Getter and setter pola health
    /// </summary>
    public float Health
    {
        set { health = value; }
        get { return health; }
    }

    /// <summary>
    /// Identyfikacja gracza - czy agent widzi postać gracza
    /// W przypadku widoczności gracza agent dostaje nowe polcenie - włączyć alarm
    /// </summary>
    /// <param name="player"></param>
    public void IdentifyPlayer(GameObject player)
    {
        Vector3 direction = player.transform.localPosition - this.transform.localPosition;
        float angle = Vector3.Angle(direction, transform.forward);
        
        if (angle < fieldOfView*0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction.normalized, out hit, 15f))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    currentMission = 1;
                    Debug.Log("Raise the alarm!");
                    //target = level.alarm;
                    playerLastSeen = player.transform.localPosition - this.transform.localPosition;
                    EndEpisode();
                }
            }
        }
    }
}
