using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa abstrakcyjna, potrzebna dla polymorfizmu
/// </summary>
public abstract class LevelController : MonoBehaviour
{
    public GameObject player;//Gracz
    public Vector3 positionToSpawn0, positionToSpawn1;//granice w których może pojawić się postać
    /// <summary>
    /// Generacja położenia w środowisku
    /// </summary>
    /// <returns>Wektor 3-wymiarowy określający pozycje w przestrzni</returns>
    public abstract Vector3 SpawnNewPosOnLevel();
    
}
