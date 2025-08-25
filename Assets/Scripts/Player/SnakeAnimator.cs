using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class SnakeAnimator : MonoBehaviour
{
    [SerializeField] private List<InGameSkin> skinSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    private string _currentSkinName;
    private InGameSkin _currentSkin;
    
    private bool _isHead;
    private Coroutine _idleCoroutine;

    private readonly string _animatorBoolName = "OnHit";

    void OnEnable()
    {
        Player.OnHitAction += OnHit;

        _currentSkinName = PlayerDataManager.Instance.GetCurrentSkin();
        
        foreach (var skin in skinSprites)
        {
            if (skin.skinName == _currentSkinName) 
            {
                _currentSkin = skin;
            }
        }
    }

    void OnDisable()
    {
        Player.OnHitAction -= OnHit;
    }

    public void SetHead()
    {
        _isHead = true;
        _idleCoroutine = StartCoroutine(IdleAnimation());
    }

    void OnHit(bool a)
    {
        if (_isHead)
        {
            StopCoroutine(_idleCoroutine);
            StartCoroutine(OnHitAnimation());
        }
    }

    IEnumerator IdleAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            foreach (var sprite in _currentSkin.idleState)
            {
                spriteRenderer.sprite = sprite;               
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator OnHitAnimation()
    {
        animator.enabled = true;
        animator.SetBool(_animatorBoolName, true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool(_animatorBoolName, false);
        animator.enabled = false;
        _idleCoroutine = StartCoroutine(IdleAnimation());
    }
}
