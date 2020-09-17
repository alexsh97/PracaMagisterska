using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa reprezentująca zasoby, dające wartości
/// </summary>
public class ResourceGetter : MonoBehaviour
{
    bool isEmpty = false;//Czy jest pusty
    public int valueToGive;//Wartość 
    public string nameFull;//Tag gdy jest pełny
    public string nameEmpty;//Tag gdy zasób jest pusty
    
    /// <summary>
    /// Resetowanie obiektu
    /// </summary>
    public void ResetResource()
    {
        isEmpty = false;
        tag = nameFull;
    }

    /// <summary>
    /// Dawanie zasobu
    /// zmiana stanu na pusty
    /// </summary>
    /// <returns>wartość zasobu</returns>
    public int GetValueToAdd()
    {
        isEmpty = true;
        tag = nameEmpty;
        return valueToGive;
    }

    /// <summary>
    /// Aktualny stan zasobu
    /// </summary>
    /// <returns>pusty czy pełny</returns>
    public bool GetStatus()
    {
        return isEmpty;
    }
}
