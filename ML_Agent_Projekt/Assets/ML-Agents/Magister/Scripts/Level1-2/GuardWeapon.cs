using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa GuardWeapon reprezentuje broń, której używa GuardAgent
/// </summary>
public class GuardWeapon : MonoBehaviour
{
    float shootingPause;//czas oczekiwania na kolejną możliwość ataku
    int maxAmmo;//maksymalna amunicja
    int actualAmmo;//amunicja
    float reloadTimer;//timer do nowego ataku
    bool isAttacking;//czy jest próba ataku
    SpriteRenderer gunFireSprites;//efekt strzału
    
    /// <summary>
    /// Inicjalizacja obiektu po włączeniu gry
    /// </summary>
    void Start()
    {
        maxAmmo = 20;
        shootingPause = 2;
        reloadTimer = 0f;
        gunFireSprites = this.GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Aktualizacja obiektu co klatkę
    /// Odliczanie timera do następnego ataku
    /// </summary>
    void FixedUpdate()
    {
        reloadTimer += Time.deltaTime;

        if (reloadTimer > 0.3)
        {
            gunFireSprites.enabled = false;
        }
    }

    /// <summary>
    /// Mówi, czy można już używać broń
    /// </summary>
    /// <returns>Zwraca true, jeśli można</returns>
    public bool IsReadyToBeUsed()
    {
        return reloadTimer > shootingPause ? true : false;
    }

    /// <summary>
    /// Próba dokonać ataku
    /// Jeśli był strzał, to z magazynku będzie o jeden mniej pocisków
    /// </summary>
    public void TryToAttack()
    {
        if (IsReadyToBeUsed())
        {
            isAttacking = true;
            reloadTimer = 0f;
            actualAmmo = actualAmmo > 0 ? (actualAmmo - 1) : actualAmmo;
            if (actualAmmo <= 0)
            {
                shootingPause = 1;
            }
            else
            {
                shootingPause = 2;
                gunFireSprites.enabled = true;
            }
        }
    }

    /// <summary>
    /// Getter i setter amunicji
    /// </summary>
    public int Ammo
    {
        set{ actualAmmo = value >= maxAmmo ? maxAmmo : value; }
        get { return actualAmmo; }
    }

    public int MaxAmmo
    {
        get { return maxAmmo; }
    }
    /// <summary>
    /// Getter i setter parametru mówiącego czy spust był pociągnięty
    /// </summary>
    public bool IsAttacking
    {
        set { isAttacking = value; }
        get { return isAttacking; }
    }
}
