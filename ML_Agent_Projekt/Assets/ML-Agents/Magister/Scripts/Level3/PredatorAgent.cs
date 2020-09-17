using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;

/// <summary>
/// Klasa opisująca agenta - drapieżnika
/// Dziedziczy po klasie Agent
/// Ma implementacje funkcji dla każdego etapu uczenia ze wzmocnieniem
/// oraz funkcje do wykonania akcji, obsługi kolizji, interakcji
/// </summary>
public class PredatorAgent : AnimalAgent
{
    float attackTimer;//timer ataku
    public float rayLength;//odległość ataku

    /// <summary>
    /// Aktualizacja agenta co klatkę
    /// oczekiwania do następnego ataku
    /// Jest odliczany czas do końca dnia dla agenta (w czasie rozrywki, a nie uczenia)
    /// Gdy dzień się skończy startuje nowy epizod
    /// </summary>
    void FixedUpdate()
    {

        if (personalTimer > level.dayDuration)
        {
            Debug.Log("Day ended for predator");
            personalTimer = 0f;
            EndEpisode();
        }

        personalTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;
       
    }

    /// <summary>
    /// Maskowanie akcji (zakaz wykonania) w przypadku śmieric agenta
    /// </summary>
    /// <param name="actionMasker"></param>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        if(attackTimer < 1)
        {
            actionMasker.SetMask(2, new int[1] { 1 });
        }
        if(health <= 0)
        {
            actionMasker.SetMask(0, new int[2] { 1,2 });
            actionMasker.SetMask(1, new int[2] { 1, 2 });
            actionMasker.SetMask(2, new int[1] { 1 });
        }
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

            if (Mathf.FloorToInt(vectorAction[2]) == 1 && attackTimer % 60 > 1)
            {
                Attack();
                energy -= 1f;
                attackTimer = 0;
            }
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
            Debug.Log("Pred DIED");
            SetReward(-2f);
            EndEpisode();
        }
        
    }
    
    /// <summary>
    /// Wykonanie akcji ataku
    /// walka z bliska
    /// Może trafić w gracza i roślinożerców, zarówno jak w innego drapieżnika
    /// </summary>
    private void Attack()
    {
        Vector3 rayPos = transform.position;
        rayPos.y -= transform.localScale.y/4f;
        Vector3 rayDir = transform.forward;
        Ray r = new Ray(rayPos, rayDir);
        RaycastHit hit;
        Debug.DrawRay(rayPos, rayLength * rayDir, Color.red, 0f, true);

        if (Physics.Raycast(r, out hit, rayLength))
        {
            Debug.Log("predator hit herb");
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                AddReward(0.05f);
                Debug.Log("predator hit player");
                CatAgent targetToAttack = 
                        hit.collider.gameObject.GetComponent<PlayerController>();
                targetToAttack.BeenAttacked(5f);
                if (targetToAttack.Health <= 0)
                {
                    hunger += 50;
                    hunger = hunger > 100 ? 100 : hunger;
                }
            }
            else if (hit.collider.gameObject.CompareTag("herbivorous"))
            {
                AddReward(0.05f);
                Debug.Log("predator hit herb");
                HerbivorousAgent targetToAttack = 
                        hit.collider.gameObject.GetComponent<HerbivorousAgent>();
                targetToAttack.BeenAttacked(60);
                if(targetToAttack.Health <= 0)
                {
                    hunger += 50;
                    hunger = hunger > 100 ? 100 : hunger;
                }
            }
            else if (hit.collider.gameObject.CompareTag("predator"))
            {
                AddReward(-0.05f);
                Debug.Log("Friendly fire");
                PredatorAgent targetToAttack = 
                        hit.collider.gameObject.GetComponent<PredatorAgent>();
                targetToAttack.BeenAttacked(40);
                if (targetToAttack.Health <= 0)
                {
                    Debug.Log("cannibalism");
                    hunger += 50;
                    hunger = hunger > 100 ? 100 : hunger;
                }
            }
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
        actionsOut[2] = 0;

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
        if (Input.GetKey(KeyCode.E))
        {
            actionsOut[2] = 1f;
        }
    }

    /// <summary>
    /// Obsługa kolizji agenta z obiektami w środowisku
    /// </summary>
    /// <param name="collision">obiekt z którym jest kolizja</param>
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("water"))
        {
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
        if (collision.gameObject.CompareTag("home"))
        {
            SetReward(-2f);
            //EndEpisode();
        }
                
    }
    
}
