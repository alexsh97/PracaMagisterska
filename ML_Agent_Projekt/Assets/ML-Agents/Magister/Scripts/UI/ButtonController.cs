using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa reprezentująca przycisk
/// </summary>
public class ButtonController : MonoBehaviour
{
    bool isPressed = false;//czy jest naciśnięty
    float pressedTimer = 0f;//licznik 

    public Material pressed;//materiał w stanie naciśniętym
    public Material notPressed;//materiał w stanie nie naciśniętym
    
    /// <summary>
    /// Funkcja jest wywołana zaraz po włączeniu sceny
    /// Obiektwi ustawia się domyślny materiał
    /// </summary>
    void Start()
    {
        this.GetComponent<MeshRenderer>().material = notPressed;
    }

    /// <summary>
    /// Aktualizacja obiektu co klatkę
    /// odliczenie czasu, jeśli przycisk był naciśnięty do powrótu do stanu nie naciśnięty
    /// </summary>
    void FixedUpdate()
    {
        pressedTimer += Time.deltaTime;

        if(pressedTimer >  60 && isPressed)
        {
            isPressed = false;
            this.GetComponent<MeshRenderer>().material = notPressed;
        }
    }

    /// <summary>
    /// Obsługa kolizji
    /// Jeśli zdarzyła się kolizja z graczem, 
    /// wtedy przycisk jest uważany za naciśnięty
    /// co oznacza zmiane materiału oraz otwieranie drzwi klatek
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            pressedTimer = 0f;
            this.GetComponent<MeshRenderer>().material = pressed;
            isPressed = true;
            this.GetComponent<DoorController>().OpenDoor();
        }
    }
}

