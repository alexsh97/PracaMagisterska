using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa określająca poziomy środowsika laboratorium
/// </summary>
public class LabController : LevelController
{
    public ResourceGetter[] resources;//Lista wszystkich zasobów na poziomie
    public GameObject alarm;
    bool isAlarmRaised;//czy alarm był wywołany
    
    /// <summary>
    /// Resetowanie wszystkich zasobów w tym środowisku
    /// </summary>
    private void ResetResource()
    {
        foreach(ResourceGetter r in resources)
        {
            r.ResetResource();
        }
    }

    /// <summary>
    /// Resetowanie całego poziomu
    /// </summary>
    public void ResetLevel()
    {
        if(player != null)
        {
            player.transform.localPosition = SpawnNewPosOnLevel();
            Debug.Log("New player pos");
        }

        ResetResource();
       
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

    /// <summary>
    /// Getter i setter parametru, mówiącego czy alarm jest wywołany
    /// </summary>
    public bool IsAlarmRaised
    {
        set { isAlarmRaised = value; }
        get { return isAlarmRaised; }
    }
 }
