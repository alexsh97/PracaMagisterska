using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour
{
    public ForestLevelController level;//środowsiko
    bool isEaten = false;//czy było jedzenie zjedzone
    public float value;//wartość nasycenia
    float personalTimer = 0f;//timer osobisty
    
    /// <summary>
    /// Gdy dzień się skończy a jedzenie było zjedzone, wówczas jedzenie będzie przewrócone
    /// </summary>
    void FixedUpdate()
    {
        personalTimer += Time.deltaTime;

        if (personalTimer > level.dayDuration && isEaten)
        {
            isEaten = false;
            personalTimer = 0f;
            transform.Translate(0, 50, 0);
        }
    }

    /// <summary>
    /// Karmi agenta, który weźmie
    /// </summary>
    /// <returns></returns>
    public float GiveFood()
    {
        isEaten = true;
        personalTimer = 0f;
        transform.Translate(0,-50,0);
        return value;
    }
}
