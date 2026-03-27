using UnityEngine;

/// <summary>
/// Маркер для объектов, которые могут быть аномалией.
/// AnomalyManager выставляет IsAnomalous = true на одном случайном объекте.
/// </summary>
public class AnomalyObject : MonoBehaviour
{
    public bool IsAnomalous { get; set; }
}
