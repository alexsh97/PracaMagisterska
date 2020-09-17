using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;

/// <summary>
/// Klasa opisująca agenta - roślinożerce
/// Dziedziczy po klasie Agent
/// Ma implementacje funkcji dla każdego etapu uczenia ze wzmocnieniem
/// oraz funkcje do wykonania akcji, obsługi kolizji, interakcji
/// </summary>
public class HerbivorousAgent : AnimalAgent
{
    
    /// <summary>
    /// Aktualizacja obiektu co klatkę
    /// Jest odliczany czas do końca dnia dla agenta (w czasie rozrywki, a nie uczenia)
    /// Gdy dzień się skończy startuje nowy epizod
    /// </summary>
    void FixedUpdate()
    {
        if (personalTimer > level.dayDuration)
        {
            Debug.Log("Day ended for herb");
            personalTimer = 0;
            EndEpisode();
        }

        personalTimer += Time.deltaTime;
    }
    
    /// <summary>
    /// Maskowanie akcji (zakaz wykonania) w przypadku śmieric agenta
    /// </summary>
    /// <param name="actionMasker"></param>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        if(health <= 0)
        {
            actionMasker.SetMask(0, new int[2] { 1,2 });
            actionMasker.SetMask(1, new int[2] { 1, 2 });
        }
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
        if (collision.gameObject.CompareTag("food"))
        {
            hunger += collision.gameObject.GetComponent<FoodController>().GiveFood();
            Debug.Log("Herb eat!");
            if (hunger > 100)
            {
                AddReward(-0.001f);
                hunger = 100;
            }
            energy -= 5f;
            energy = energy <= 0 ? 0 : energy;

        }
        if (collision.gameObject.CompareTag("home"))
        {
            Debug.Log("Herb rest!");
            energy += 40f;
            energy = energy > 100 ? 100 : energy;

            thirst -= 5f;
            thirst = thirst <= 0 ? 0 : thirst;

            hunger -= 5f;
            hunger = hunger <= 0 ? 0 : hunger;

        }
        if (collision.gameObject.CompareTag("water"))
        {
            Debug.Log("Herb drink!");
            thirst += 40f;
            if (thirst > 100)
            {
                AddReward(-0.001f);
                thirst = 100;
            }
            energy -= 5f;
            energy = energy <= 0 ? 0 : energy;

        }
        if (collision.gameObject.CompareTag("wall"))
        {
            SetReward(-2f);
            //EndEpisode();
        }
        
    }
    
}
