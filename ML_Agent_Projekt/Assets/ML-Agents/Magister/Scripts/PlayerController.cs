using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Klasa opisująca gracza
/// Dziedziczy po klasie CatAgent, bo jest też kotem, tylko sterowanym przez gracza
/// </summary>
public class PlayerController :  CatAgent
{
    public string currentLevelGoal; //scena do której musi gracz sie przekierować

    /// <summary>
    /// Funkcja wywołana jest co klatke do aktualizacji obiektu
    /// Sprawdza czy poziom życia gracza równa się 0, jeśli tak,
    /// wówczas gracz przerzucany jest do menu głównego
    /// </summary>
    void FixedUpdate()
    {
        personalTimer += Time.deltaTime;

        if(health <= 0)
        {
            Debug.Log("DEAD Player");
            SceneManager.LoadScene("MainMenu"); 
        }
    }

    /// <summary>
    /// Obsługa kolizji agenta z obiektami w środowisku
    /// </summary>
    /// <param name="collision">obiekt z którym jest kolizja</param>
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("scenetravel")) //zmiana sceny
        {
            SceneManager.LoadScene(currentLevelGoal);
        }

        if (collision.gameObject.CompareTag("medecine"))
        {
            var medecineKit = collision.gameObject.GetComponent<ResourceGetter>();
            health += medecineKit.GetValueToAdd();
            health = health >= 10 ? 10 : health;
            Debug.Log("Get first aid");
        }
    }

    
}
