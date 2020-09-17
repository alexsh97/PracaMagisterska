using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa zarządzająca drzwiami
/// </summary>
public class DoorController : MonoBehaviour
{
    bool isOpen = false;//stan obiektu
    public GameObject[] door;//lista drzwi do zarządzania
    float closeTimer = 0f;//licznik

    /// <summary>
    /// Aktualizacja obiektu co klatkę
    /// po odliczeniu pewnego czasu kontrolowane drzwi są zamykane
    /// </summary>
    void FixedUpdate()
    {
        closeTimer += Time.deltaTime;
        if(closeTimer > 60)
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Próba otwarcia drzwi, jęsli byli zamknięte
    /// Gdyż drzwi już otwarte, po wywołaniu metody zostaną zamknięte
    /// </summary>
    public void OpenDoor()
    {
        if(!isOpen)
        {
            foreach(GameObject d in door)
            {
                d.transform.Translate(0, 5, 0);
            }
            isOpen = true;
            closeTimer = 0f;
        }
        else
        {
            foreach (GameObject d in door)
            {
                d.transform.Translate(0, -5, 0);
            }
            isOpen = false;
            closeTimer = 0f;
        }
    }



}
