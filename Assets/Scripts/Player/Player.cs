using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    //some variables
    [SerializeField] private float reverseSpeed;
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private Vector2 minMaxSpawnDelay;
    [SerializeField] private GameObject snakeBodyPrefab;
    [SerializeField] private GameObject foodPrefab;
    
    private Quaternion _headRotation;
    private Vector2Int _moveDirection;
    private Vector2Int _foodPosition;
    private Vector2Int _foodStandardPosition;
    
    private List<Vector2Int> _snakeBodyPositions;
    private List<GameObject>  _snakeBodySegments;
    
    private int _yBound;
    private int _xBound;

    private WaitForSeconds _snakeDelay;
    
    private bool _gameOver;

    private GameObject _food;

    public static Action<int, Vector2> UpdateScore;
    public static Action<bool> OnHitAction;
    void Start()
    {
        Init();
    }

    void Init()
    {
        _snakeBodyPositions = new List<Vector2Int>();
        _snakeBodySegments = new List<GameObject>();
        _snakeDelay = new WaitForSeconds(reverseSpeed);

        for (var i = 0; i < 4; i++)
        {
            var position = new Vector2Int(1,1);
            if (i == 0) CreateSnakeBodySegments(position, true);
            else CreateSnakeBodySegments(position);
            _snakeBodyPositions.Add(position);
        }
        
        _headRotation = Quaternion.Euler(0,0,90);
        _moveDirection = Vector2Int.right;
        _foodStandardPosition = new Vector2Int(Mathf.RoundToInt(gridSize.x * 2), Mathf.RoundToInt(gridSize.y * 2));
        _foodPosition = _foodStandardPosition;
        
        _food = Instantiate(foodPrefab, new Vector3(_foodPosition.x, _foodPosition.y), Quaternion.identity);
        
        _xBound = Mathf.RoundToInt(gridSize.x/2);
        _yBound = Mathf.RoundToInt(gridSize.y/2);

        
        StartCoroutine(WaitAndMoveRoutine());
        StartCoroutine(SpawnFoodRoutine());
        
    }
    
    private void Update()
    {
        if (!_gameOver){
            InputHandler();
            UpdateVisual();
        }
    }

    void InputHandler()
    {
        if (!_gameOver){
            var movementInput = InputManager.InputActions.Player.Move.ReadValue<Vector2>();

            if (movementInput.x > 0 && _moveDirection != Vector2Int.left)
            {
                _moveDirection = Vector2Int.right;
                _headRotation =  Quaternion.Euler(0,0, -90);
            }

            if (movementInput.x < 0 && _moveDirection != Vector2Int.right)
            {
                _moveDirection = Vector2Int.left;
                _headRotation =  Quaternion.Euler(0,0,90);
            }

            if (movementInput.y > 0 && _moveDirection != Vector2Int.down)
            {
                _moveDirection = Vector2Int.up;
                _headRotation =  Quaternion.Euler(0,0,0);
            }

            if (movementInput.y < 0 && _moveDirection != Vector2Int.up)
            {
                _moveDirection = Vector2Int.down;
                _headRotation =   Quaternion.Euler(0,0,180);
            }

        }
    }
    void UpdateVisual()
    {
        for (var i = 0; i < _snakeBodySegments.Count; i++)
        {
            _snakeBodySegments[i].transform.position = Vector2.Lerp(_snakeBodySegments[i].transform.position, _snakeBodyPositions[i], 10 * Time.deltaTime);
            if (i==0) _snakeBodySegments[i].transform.rotation = Quaternion.Slerp(_snakeBodySegments[i].transform.rotation, _headRotation, 10 * Time.deltaTime);
        }
    }
    
    void Move()
    {       
        var newHeadPos = _snakeBodyPositions[0] + _moveDirection;

        if (newHeadPos.x > _xBound || newHeadPos.x < -_xBound
                                  || newHeadPos.y > _yBound || newHeadPos.y < -_yBound)
        {
            OnHit();
            OnHitAction?.Invoke(true);
            return;
        }

        

        foreach (var i in _snakeBodyPositions)
        {
            if (newHeadPos == i)
            {
                OnHit();
                OnHitAction?.Invoke(true);
                return;
            }
        }
        
        
        _snakeBodyPositions.Insert(0, newHeadPos);
        
        if (newHeadPos == _foodPosition)
        {
            CreateSnakeBodySegments(_snakeBodyPositions[_snakeBodyPositions.Count - 1]);
            UpdateScore?.Invoke(_snakeBodyPositions.Count, _food.transform.position);
            _foodPosition = _foodStandardPosition;
            StartCoroutine(SpawnFoodRoutine());
        }
        else
            _snakeBodyPositions.RemoveAt(_snakeBodyPositions.Count - 1);
    }
    
    void OnHit()
    {
        StartCoroutine(OnHitRoutine(_moveDirection));
    }

    void OnGameOver()
    {
        _gameOver = true;
    }
    
    void SpawnFood()
    {
        var x = Mathf.RoundToInt(Random.Range((-gridSize.x)/2+1, (gridSize.x)/2-1));
        var y = Mathf.RoundToInt(Random.Range((-gridSize.y)/2+1, (gridSize.y)/2-1));
        _foodPosition = new Vector2Int(x, y);
        _food.transform.position = new Vector3(x,y);
    }
    
    void CreateSnakeBodySegments(Vector2Int pos, bool isHead = false)
    {
        var position = new Vector3(pos.x, pos.y, 0);
        var newSegment = Instantiate(snakeBodyPrefab, position, Quaternion.identity);
        _snakeBodySegments.Add(newSegment);

        if (isHead)
        {
            newSegment.GetComponent<SnakeBody>().SetHead();
        }
    }
    
    IEnumerator WaitAndMoveRoutine()
    {
        while (!_gameOver)
        {
            yield return _snakeDelay;
            Move();
        }
    }

    IEnumerator SpawnFoodRoutine()
    {
        _food.transform.position = new Vector3(_foodStandardPosition.x, foodPrefab.transform.position.y);
        var delay = Random.Range(minMaxSpawnDelay.x, minMaxSpawnDelay.y);
        yield return new WaitForSeconds(delay);
        SpawnFood();     
    }
    IEnumerator OnHitRoutine(Vector2Int direction)
    {
        yield return new WaitForSeconds(0.3f);
        if (_moveDirection == direction)
        {
            GameManager.Instance.GameOver();
            OnGameOver();
        }
        else
        {
            OnHitAction?.Invoke(false);
        }
    }
}
