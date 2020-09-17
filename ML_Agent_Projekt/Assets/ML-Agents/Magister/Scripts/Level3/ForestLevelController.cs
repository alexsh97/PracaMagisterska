using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa określający poziom środowsika lasu
/// </summary>
public class ForestLevelController : LevelController
{
    float dayTimer = 0f;//timer dnia
    public float dayDuration;//czas trwania jednego dnia
    
    /// <summary>
    /// Liczenie timera, aż dzień się nie skończy
    /// </summary>
    void FixedUpdate()
    {
        dayTimer += Time.deltaTime;
        if (dayTimer > dayDuration)
        {
            Debug.Log("End of day"+ dayTimer);
            dayTimer = 0f;
        }
        
    }

    /// <summary>
    /// Generacja położenia w tym środowisku
    /// </summary>
    /// <returns>Wektor 3-wymiarowy określający pozycje w przestrzni</returns>
    public override Vector3 SpawnNewPosOnLevel()
    {
        Vector3 newPos = new Vector3(UnityEngine.Random.Range(positionToSpawn0.x, positionToSpawn1.x), 
            positionToSpawn1.y, UnityEngine.Random.Range(positionToSpawn0.z, positionToSpawn1.z));
        return newPos;
    }

    
    public float GetSeconds()
    {
        return dayTimer;
    }
}
