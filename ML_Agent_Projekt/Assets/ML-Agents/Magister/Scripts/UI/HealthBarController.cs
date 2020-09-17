using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Klasa reprezentująca część UI - pasek życia
/// </summary>
public class HealthBarController : MonoBehaviour
{
    private Image healthBar;//obrazek paska życia
    public float currentHealth;//aktualna wartość
    private float maxHealth = 10f;//maksymalna wartośc
    public PlayerController player;//Gracz

    /// <summary>
    /// Funkcja wywołana zaraz po włączeniu gry
    /// Pobierany jest obrazek paska życia
    /// </summary>
    void Start()
    {
        healthBar = GetComponent<Image>();
    }

    /// <summary>
    /// Funkcja aktualizuje stan obiektu co klatke
    /// pobierana jest aktualna wartość poziomu gry
    /// Wartość przesyłana do paska życia (elementu UI)
    /// </summary>
    void FixedUpdate()
    {
        currentHealth = player.Health;
        healthBar.fillAmount = currentHealth / maxHealth;
    }
}
