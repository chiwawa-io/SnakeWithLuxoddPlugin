using System;
using System.Collections;
using UnityEngine;

public class ItemLifeSpan : MonoBehaviour
{
    [SerializeField] private float lifeSpan;

    public static Action<Vector2Int> OnItemDestroyed;

    private void OnEnable()
    {
        Player.OnPreciousFoodEaten += Destroy;
    }

    private void OnDisable()
    {
        Player.OnPreciousFoodEaten -= Destroy;
    }

    void Destroy()
    {
        StartCoroutine(DestroyAfterTime());
    } 
        
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeSpan);
        OnItemDestroyed?.Invoke(Vector2Int.RoundToInt(this.transform.position));
        Destroy(gameObject);
    }
}
