using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa określająca środowisko miasta
/// </summary>
public class TownController : LevelController
{
    public GameObject[] parkingPlaces; //miejsca parkingowe
    public GameObject[] civilPlaces; //miejsca docelowe

    /// <summary>
    /// Funkcja wywołana na samym początku po włączeniu gry
    /// Nadaje każdemu miejscu parkingowemu na poziomie Id
    /// </summary>
    void Start()
    {
        for(int p = 0; p < parkingPlaces.Length; p++)
        {
            parkingPlaces[p].GetComponent<ParkingPlaceController>().ID = p+1;
        }
    }

    /// <summary>
    /// Losowo wybierane jest miejsce parkingowe, które w dodatku nie jest już nadane innemu agentowi
    /// </summary>
    /// <returns>Zwraca wybrane miejsce parkingowe</returns>
    public GameObject GetParkingPlace()
    {
        GameObject parkingPlace;
        do
        {
            int parkingIndex = UnityEngine.Random.Range(0, parkingPlaces.Length);
            parkingPlace = parkingPlaces[parkingIndex];

        } while (parkingPlace.GetComponent<ParkingPlaceController>().IsActive);

        return parkingPlace;
    }

    /// <summary>
    /// Losowo wybierane jest miejsce docelowe dla mieszkańca
    /// </summary>
    /// <returns>Zwraca wybrane miejsce parkingowe</returns>
    public GameObject GetPlace()
    {
        int civilIndex = UnityEngine.Random.Range(0, civilPlaces.Length);
        return civilPlaces[civilIndex];
    }

    /// <summary>
    /// Generacja położenia w tym środowisku
    /// </summary>
    /// <returns>Wektor 3-wymiarowy określający pozycje w przestrzni</returns>
    public override Vector3 SpawnNewPosOnLevel()
    {
        Vector3 newPos = new Vector3(UnityEngine.Random.Range(positionToSpawn0.x, positionToSpawn1.x), positionToSpawn1.y, UnityEngine.Random.Range(positionToSpawn0.z, positionToSpawn1.z));
        return newPos;
    }
}
