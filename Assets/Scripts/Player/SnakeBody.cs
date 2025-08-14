using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    [SerializeField] private GameObject blowPrefab;
    private Animator _animator;
    private bool _isHead;

    private void OnEnable()
    {
        Player.OnHitAction += OnHit;
        GameManager.OnGameOver += BOOM;
    }
    private void OnDisable()
    {
        GameManager.OnGameOver -= BOOM;
        Player.OnHitAction -= OnHit;
    }

    void BOOM(int score)
    {
        var pos = new Vector3(transform.position.x, transform.position.y, -4f);
        Instantiate(blowPrefab, pos, Quaternion.identity);
        Destroy(gameObject);
    }
    void OnHit(bool isHit)
    {
       if (_isHead) _animator.SetBool("OnHit", isHit);
    }
    public void SetHead()
    {
        _isHead = true;
        _animator = GetComponent<Animator>();
        _animator.enabled = true;
    }

    public void SetBody()
    {
        _isHead = false;
        _animator.enabled = false;
    }
    
}
