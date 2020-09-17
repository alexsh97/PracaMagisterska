using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;

/// <summary>
/// Klasa reprezentująca agenta zwięrzęcia
/// </summary>
public class AnimalAgent : Agent
{
    public ForestLevelController level;//Środowisko w którym znajduje się agent
    protected Rigidbody rBody;//Właściwości fizyczne
    protected float health;//poziom życia
    protected float hunger;//poziom głodu
    protected float thirst;//poziom pragnienia
    protected float energy;//poziom energii
    public float speed;//prędkośż poruszania się
    protected float personalTimer;//timer osobisty

    /// <summary>
    /// Inicjalizacja agenta
    /// </summary>
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        personalTimer = 0f;
    }

    /// <summary>
    /// Inicjalizacja wszystkich parametrów odnowa na początku nowego epizodu
    /// Inaczej mówiąc resetacja agenta
    /// </summary>
    public override void OnEpisodeBegin()
    {
        personalTimer = 0;

        health = UnityEngine.Random.Range(4, 11) * 10f;
        hunger = UnityEngine.Random.Range(4, 11) * 10f;
        thirst = UnityEngine.Random.Range(4, 11) * 10f;
        energy = UnityEngine.Random.Range(4, 11) * 10f;

        this.transform.localPosition = level.SpawnNewPosOnLevel();

    }

    /// <summary>
    /// Zbieranie obserwacji agenta, będącymi danymi wejściowymi sieci neuronowej
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((this.health / 100f));
        sensor.AddObservation((this.hunger / 100f));
        sensor.AddObservation((this.hunger / 100f));
        sensor.AddObservation((this.hunger / 100f));

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    /// <summary>
    /// Wykonanie akcji: ruchu, obracania się, ataku
    /// Obniżenie parametrów zwięrzęcia
    /// Jeśli poziom jednego z parametrów poniżej 30, wtedy obniża się poziom życia i agent dostaje karę
    /// W przypadku braku życia kończy epizod z wynikiem negatywnym
    /// </summary>
    /// <param name="vectorAction">Wektor akcji</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        if (health > 0)
        {
            MoveAgent(vectorAction[0]);
            RotateAgent(vectorAction[1]);

            energy -= 0.01f;
            energy = energy <= 0 ? 0 : energy;

            thirst -= 0.01f;
            thirst = thirst <= 0 ? 0 : thirst;

            hunger -= 0.01f;
            hunger = hunger <= 0 ? 0 : hunger;

            if (energy < 30 || thirst < 30 || hunger < 30)
            {
                AddReward(-0.002f);
            }
            else
            {
                AddReward(0.005f);
                health += 0.05f;
                health = health >= 100 ? 100 : health;
            }

            if (energy <= 0 || thirst <= 0 || hunger <= 0)
            {
                health -= 0.01f;
            }
        }
        else
        {
            Debug.Log("DIED");
            //SetReward(-2f);
            //EndEpisode();
        }

    }

    /// <summary>
    /// Poruszanie się agenta w wybranym kierunku
    /// </summary>
    /// <param name="actions">Akcje określający kierunek poruszania się</param>
    protected void MoveAgent(float action)
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

        if (forwardAction == 0 && this.energy < 100)
        {
            this.energy += 1f;
        }

        rBody.AddForce(dirToGo * speed, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Wykonanie akcji obracania się w prawo albo w lewo
    /// </summary>
    /// <param name="action">Akcja określająca kierunek obracania się</param>
    protected void RotateAgent(float action)
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

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
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
    /// Getter i setter parametru żucia
    /// </summary>
    public float Health
    {
        set { health = value; }
        get { return health; }
    }
}
