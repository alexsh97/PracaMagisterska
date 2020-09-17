using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa opisująca światło drogowe
/// </summary>
public class ColorLightController : MonoBehaviour
{
    float timer = 0.0f;
    public float duration;//czas oczekiwania na zmiane światła

    /// <summary>
    /// Wywoła się na samym początku po łączeniu gry
    /// </summary>
    void Start()
    {
        if(this.CompareTag("greenlight"))
        {
            //this.tag = "redlight";
            //this.transform.Translate(0, -30, 0);
        }
    }

    /// <summary>
    /// Akutalizacja obiektu co klatke
    /// Odliczanie czasu do zmiany światła
    /// </summary>
    void FixedUpdate()
    {
        timer += Time.deltaTime;

        if(this.CompareTag("greenlight") && timer > duration)
        {
            ChangeColor();
            timer = 0.0f;
        }
        if (this.CompareTag("redlight") && timer > 10)
        {
            ChangeColor();
            timer = 0.0f;
        }
    }

    /// <summary>
    /// Zmiana światła z jednego na drugi kolor
    /// </summary>
    void ChangeColor()
    {
        if (this.CompareTag("redlight"))
        {
            this.tag = "greenlight";
            //this.transform.Translate(0,-30,0);
        }
        else
        {
            this.tag = "redlight";
            //this.transform.Translate(0, 30, 0);
        }
    }
}
