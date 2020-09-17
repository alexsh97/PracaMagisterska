using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

/// <summary>
/// Klasa opisująca agenta - kota
/// Dziedziczy po klasie Agent
/// Ma implementacje funkcji dla każdego etapu uczenia ze wzmocnieniem
/// oraz funkcje do wykonania akcji, obsługi kolizji, interakcji
/// </summary>
public class CatAgent : Agent
{
    public int idCat;//id kota
    Rigidbody rBody;//właściwości fizyczne
    public LevelController levelSpawner;
    protected float health;//poziom życia
    public float speed;//prędkość poruszania się
    protected bool isShooting;//czy jest możliwość wykonania ataku
    protected float personalTimer;//timer osobisty
    protected bool onGround;//czy znajduje się agent na powierzchni
    protected bool isFreez;//czy jest agent zatrzymany
    protected float freezTimer;//czas zatrzymania agenta

    /// <summary>
    /// Inicjalizacja agenta
    /// </summary>
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        isShooting = false;
        personalTimer = 0f;
        isFreez = false;
        freezTimer = 0f;
        onGround = true;
        health = 10f;
    }

    /// <summary>
    /// Aktualizacja stanu obiektu co klatkę
    /// Odliczanie czasu do kolejnej możliwości ataku,
    /// Osobne odliczanie czasu do "rozmrażania"
    /// </summary>
    void FixedUpdate()
    {
        personalTimer += Time.deltaTime;
      
        if(isFreez)
        {
            freezTimer += Time.deltaTime;
            if(freezTimer > 1)
            {
                freezTimer = 0f;
                isFreez = false;
            }
        }
    }

    /// <summary>
    /// Inicjalizacja wszystkich parametrów odnowa na początku nowego epizodu
    /// Inaczej mówiąc resetacja agenta
    /// </summary>
    public override void OnEpisodeBegin()
    {
        isShooting = false;
        onGround = true;
        //health = UnityEngine.Random.Range(6, 10);
        //this.transform.localPosition = levelSpawner.SpawnNewPosOnLevel();
        //this.transform.Translate(0,2,0);
    }

    /// <summary>
    /// Zbieranie obserwacji agenta jako dane wejściowe sieic neuronowe
    /// </summary>
    /// <param name="sensor">Wektor obserwacji do którego przekazujemy dane</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((int)health);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    /// <summary>
    /// Ustawienie maski akcji (zakaz wykonanie wybranych akcji)
    /// </summary>
    /// <param name="actionMasker"></param>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        actionMasker.SetMask(2, new int[1] { 1 });
        if (!onGround)
        {
            actionMasker.SetMask(2, new int[1] { 1 });
        }
        if (personalTimer <= 3)
        {
            actionMasker.SetMask(3, new int[1] { 1 });
        }
        if(isFreez)
        {
            actionMasker.SetMask(0, new int[4] { 1,2,3,4 });
            actionMasker.SetMask(1, new int[2] { 1,2 });
        }
    }

    /// <summary>
    /// Wykonanie akcji: ruchu, obracania się, ataku
    /// W przypadku braku życia kończy epizod z wynikiem negatywnym
    /// </summary>
    /// <param name="vectorAction">Wektor akcji</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        if(health > 0)
        {
            MoveAgent(vectorAction);
            RotateAgent(vectorAction[1]);

            if(Mathf.FloorToInt(vectorAction[3]) == 1)
            {
               Attack(vectorAction);
            }
        }
        if (health <= 0 || transform.localPosition.y < -10f)
        {
            this.transform.Rotate(0, 0,-90);
            //SetReward(-2f);
            //EndEpisode();
        }
    }

    /// <summary>
    /// Poruszanie się agenta w wybranym kierunku, w tym skoki
    /// </summary>
    /// <param name="actions">Akcje określający kierunek poruszania się</param>
    private void MoveAgent(float[] actions)
    {
        AddReward(-0.0002f);

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
            dirToGo.y = 12f;
        }
        if (!onGround)
        {
            dirToGo.x /= 3f;
            dirToGo.z /= 3f;
        }
        if (!isFreez)
        {
            rBody.AddForce(dirToGo * speed, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// Wykonanie akcji obracania się w prawo albo w lewo
    /// </summary>
    /// <param name="action">Akcja określająca kierunek obracania się</param>
    private void RotateAgent(float action)
    {
        var rotateDir = Vector3.zero;
        var rotationAction = Mathf.FloorToInt(action);

        switch (rotationAction)
        {
            case 1:
                rotateDir = transform.up * 1f;
                break;
            case 2:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 200f);
    }

    /// <summary>
    /// Sprawdzenie czy jest pod agentem jakaś powieszchnia
    /// </summary>
    /// <returns></returns>
    private bool GroundCheck()
    {
        Vector3 rayPos = transform.position;
        rayPos.y -= 0.5f;
        float rayLength = 0.5f;

        RaycastHit hit;
        Vector3 rayDir = -transform.up + transform.forward / 2f;
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
    /// Wykonanie akcji ataku
    /// walka z bliska
    /// Może trafić w ochroniarza
    /// </summary>
    protected void Attack(float[] actions)
    {
       
        if (personalTimer > 2)
        {
            isShooting = true;
            personalTimer = 0;
        }

        Vector3 rayPos = transform.position;
        rayPos.y -= 0.5f;
        Vector3 rayDir = transform.forward;
        Ray r = new Ray(rayPos, rayDir);
        float rayLength = 3f;
        RaycastHit hit;
        Debug.DrawRay(rayPos, rayLength * rayDir, Color.red, 0f, true);
        if (isShooting)
        {
            if (Physics.Raycast(r, out hit, rayLength))
            {
                if (hit.collider.gameObject.CompareTag("guard"))
                {
                    Debug.Log("cat hit guard");
                    GuardAgent targetToAttack = 
                                hit.collider.gameObject.GetComponent<GuardAgent>();
                    targetToAttack.BeenAttacked(3f);
                    if (targetToAttack.Health <= 0)
                    {
                        AddReward(0.5f);
                    }
                }
                if (hit.collider.gameObject.CompareTag("npc"))
                {
                    Debug.Log("cat hit civil");
                    AddReward(1f);
                    CivilAgent targetToAttack =
                                hit.collider.gameObject.GetComponent<CivilAgent>();
                    targetToAttack.BeenAttacked(40f);
                    if (targetToAttack.Health <= 0)
                    {
                        SetReward(2f);
                    }
                }

            }
        }

        isShooting = false;
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
        if (collision.gameObject.CompareTag("wall"))
        {
            //SetReward(-1f);
            //EndEpisode();
        }
        if (collision.gameObject.CompareTag("scenetravel"))
        {
            //SetReward(2f);
            Debug.Log("ESCAPED");
            this.enabled = false;
            //Destroy(this);
            //EndEpisode();
        }

        if (collision.gameObject.CompareTag("medecine"))
        {
            var medecineKit = collision.gameObject.GetComponent<ResourceGetter>();
            health += medecineKit.GetValueToAdd();
            health = health >= 10 ? 10 : health;
            Debug.Log("Get first aid");
        }

    }

    /// <summary>
    /// Obsługa zdarzeń z triggerami
    /// Otwarcie drzwi
    /// </summary>
    /// <param name="other">trigger z którym jest kolizja</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("door"))
        {
            DoorController door = other.gameObject.GetComponent<DoorController>();
            door.OpenDoor();
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

    public float Health
    {
        set { health = value; }
        get { return health; }
    }
    
    /// <summary>
    /// W przypadku uzyskania jakichkolwiek obrażeń 
    /// agent będzie chwilowo zatrzymany w miejscu
    /// </summary>
    public void Freez()
    {
        isFreez = true;
        freezTimer = 0f;
    }

    
}
