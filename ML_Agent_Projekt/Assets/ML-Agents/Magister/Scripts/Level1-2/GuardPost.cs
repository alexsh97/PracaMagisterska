using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa reprezentująca punkt kontrolny ochroniarza
/// </summary>
public class GuardPost : MonoBehaviour
{
    public int id;//Id punktu kontrolnego
    bool isActive = false;//Czy jest obecnym celem ochroniarza
    
    /// <summary>
    /// Funkcja wywołana zaraz po włączeniu sceny,
    /// Dezaktywowuje obiekt, przez przemieszczenia go pod poziom
    /// </summary>
    void Start()
    {
        this.transform.Translate(0,-10f,0);
    }

    /// <summary>
    /// Aktywowanie punktu (powrót na poziom)
    /// </summary>
    public void MakeActive()
    {
        if(!isActive)
        {
            this.transform.Translate(0, 10f, 0);
            isActive = true;
        }
            
    }

    /// <summary>
    /// Dezaktywacja punktu, przemieszczając go pod poziom
    /// </summary>
    public void Deactivate()
    {
        if(isActive)
        {
            this.transform.Translate(0, -10f, 0);
            isActive = false;
        }
            
    }

}
