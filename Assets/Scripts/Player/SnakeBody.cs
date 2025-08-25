using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    [SerializeField] private GameObject blowPrefab;

    private void OnEnable()
    {
        GameManager.OnGameOver += BOOM;
    }
    private void OnDisable()
    {
        GameManager.OnGameOver -= BOOM;
    }

    void BOOM(int score)
    {
        var pos = new Vector3(transform.position.x, transform.position.y, -4f);
        Instantiate(blowPrefab, pos, Quaternion.identity);
        Destroy(gameObject);
    }
    
}
