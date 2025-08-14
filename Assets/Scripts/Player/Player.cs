using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActiveItem
{
    public GameItem Data { get; }
    public GameObject Instance { get; }
    public ActiveItem(GameItem data, GameObject instance)
    {
        Data = data;
        Instance = instance;
    }
}

public class Player : MonoBehaviour
{
    #region Configuration
    
    [Header("Snake Settings")]
    [SerializeField] private float frequency;
    [SerializeField] private float powerUpSpeed;
    [SerializeField] private float lifeAfterHitTime;
    [SerializeField] private float portalSpawnInterval = 15f;
    [SerializeField] private int maxHp;
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private GameObject snakeBodyPrefab;

    [Header("Item Data")] 
    [SerializeField] private List<GameItem> itemTypes;
    
    [Header("Reactive Spawning Logic")] 
    [SerializeField] private List<SpawnPointMap> spawnPointMap = new();
    [Tooltip("The simple food pattern to start the game with.")]
    [SerializeField] private SpawnPattern startingFoodPattern;
    [Tooltip("The simple speed Up pattern to spawn during the game")]
    [SerializeField] private SpawnPattern speedUpPattern;
    [Tooltip("The invulnerability pattern to spawn during the game")]
    [SerializeField] private SpawnPattern invulnerablityPattern;
    [Tooltip("The pattern for regular food spawns.")]
    [SerializeField] private SpawnPattern regularFoodPattern;
    [Tooltip("The pattern for Portal spawns.")]
    [SerializeField] private SpawnPattern portalsPattern;
    [Tooltip("The harder pattern that appears sometimes.")]
    [SerializeField] private List<SpawnPattern> challengePattern = new();
    [SerializeField, Range(0, 1)] private float challengeChance = 0.2f;

    [Header("SFX")] 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip foodCollect;
    [SerializeField] private AudioClip rockHit;
    [SerializeField] private AudioClip explode;
    [SerializeField] private AudioClip speedUp;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip speedDown;
    #endregion
    
    #region State Variables
    
    //spawn
    private readonly Dictionary<Vector2Int, ActiveItem> _activeItems =  new();
    private readonly Dictionary<string, int> _lastSpawnPointIndex = new Dictionary<string, int>();
    
    //snake movement 
    private readonly List<Vector2Int> _snakeBodyPositions = new();
    private readonly List<GameObject>  _snakeBodySegments = new();

    //portals
    private readonly Dictionary<Vector2Int, Vector2Int>  _portalPairs = new();
    private readonly List<GameObject>  _portalInstances = new();
    private readonly Queue<GameObject> _disabledSegments = new();
    
    
    //movement input
    private Vector2 _moveInput;
    private Vector2Int _moveDirection;
    private Vector2Int _nextMoveDirection;
    private Vector2Int _newHeadPos;
    private Quaternion _headRotation;
    
    private bool _gameOver;
    
    //teleport 
    private bool _isTeleporting;
    private int _segmentsLeftToTeleport;
    
    //Coroutines
    private WaitForSeconds _snakeDelay;
    private Coroutine _speedUpCoroutine;
    
    //boundaries
    private int _xBound;
    private int _yBound;
    
    //current session patterns
    private SpawnPointMap _spawnPointMap;
    private SpawnPattern _challengePattern;
    
    //achievements
    private int _gemsCollected;
    private int _speedUpCollected;
    
    //current effects
    private bool _isInvulnerable;
    #endregion
    
    #region Actions
    public static Action<int, Vector2> UpdateScore;
    public static Action<int> UpdateLife;
    public static Action<string, Vector2> EventMessages;
    public static Action<bool> OnHitAction;
    public static Action OnPreciousFoodEaten;
    public static Action<string> OnAchieved;
    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        ItemLifeSpan.OnItemDestroyed += OnItemDestroyed;
    }

    void Start()
    {
        ChoosePattern();

        SpeedChanger(frequency);

        for (var i = 0; i < 4; i++)
        {
            var position = new Vector2Int(1,1);
            CreateSnakeBodySegments(position, i==0);
            _snakeBodyPositions.Add(position);
        }
        
        _headRotation = Quaternion.Euler(0,0,90);
        _moveDirection = Vector2Int.left;
        _nextMoveDirection = _moveDirection;
        
        _xBound = Mathf.RoundToInt(gridSize.x/2);
        _yBound = Mathf.RoundToInt(gridSize.y/2);

        
        StartCoroutine(WaitAndMoveRoutine());
        StartCoroutine(SpawnPortalCoroutine());
        StartCoroutine(SpawnSpeedUpCoroutine());
        StartCoroutine(SpawnInvulnerabilityCoroutine());
        
        
        if (startingFoodPattern != null)
        {
            SpawnPattern(startingFoodPattern);
        }

    }
    
    private void Update()
    {
        if (!_gameOver){
            InputHandler();
            UpdateVisual();
        }
    }

    private void OnDisable()
    {
        ItemLifeSpan.OnItemDestroyed -= OnItemDestroyed;
    }

    #endregion

    #region Movement & Core Logic 
    
    void InputHandler()
    {
        if (_gameOver) return;
        
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (_moveInput.x != 0)
        {
            if (_moveInput.x > 0 && _moveDirection != Vector2Int.left)
            {
                _nextMoveDirection = Vector2Int.right;
                _headRotation = Quaternion.Euler(0, 0, -90); 
            }
            else if (_moveInput.x < 0 && _moveDirection != Vector2Int.right)
            {
                _nextMoveDirection = Vector2Int.left;
                _headRotation = Quaternion.Euler(0, 0, 90);
            }
        }
        else if (_moveInput.y != 0)
        {
            if (_moveInput.y > 0 && _moveDirection != Vector2Int.down)
            {
                _nextMoveDirection = Vector2Int.up;
                _headRotation = Quaternion.Euler(0, 0, 0); 
            }
            else if (_moveInput.y < 0 && _moveDirection != Vector2Int.up)
            {
                _nextMoveDirection = Vector2Int.down;
                _headRotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }

    void Move()
    {
        if (_isTeleporting)
        {
            _moveDirection = _nextMoveDirection;
            _newHeadPos = _snakeBodyPositions[0] + _moveDirection;

            _snakeBodyPositions.Insert(0, _newHeadPos);

            _snakeBodyPositions.RemoveAt(_snakeBodyPositions.Count - 1);

            //visuals
            
            var segmentToDisable = _snakeBodySegments[^1];
            _snakeBodySegments.RemoveAt(_snakeBodySegments.Count - 1);
        
            segmentToDisable.SetActive(false);
            _disabledSegments.Enqueue(segmentToDisable);
        
            var segmentToEnable = _disabledSegments.Dequeue();
        
            segmentToEnable.transform.position = (Vector2)_snakeBodyPositions[1]; 
            segmentToEnable.SetActive(true);
        
            _snakeBodySegments.Insert(1, segmentToEnable);
            //end visual part
            
            _segmentsLeftToTeleport--;
            if (_segmentsLeftToTeleport <= 0)
            {
                _isTeleporting = false;
            }
        }
        else
        {
            _moveDirection = _nextMoveDirection;

            _newHeadPos = _snakeBodyPositions[0] + _moveDirection;
        }

        if (_newHeadPos.x > _xBound || _newHeadPos.x < -_xBound || _newHeadPos.y > _yBound || _newHeadPos.y < -_yBound)
        {
            LifeCheck();
            OnHit(lifeAfterHitTime);
            return;
        }

        // Self collision
        for (int i = 1; i < _snakeBodyPositions.Count; i++)
        {
            if (_newHeadPos == _snakeBodyPositions[i])
            {
                LifeCheck();
                OnHit(lifeAfterHitTime);
                return;
            }
        }

        bool snakeGrow = false;
        if (_activeItems.TryGetValue(_newHeadPos, out ActiveItem activeItem))
        {
            if (activeItem.Data.isPortal)
            {
                if (_portalPairs.TryGetValue(_newHeadPos, out var exitPortal))
                {
                    _isTeleporting = true;
                    _segmentsLeftToTeleport = _snakeBodyPositions.Count;

                    _snakeBodyPositions[0] = exitPortal;
            
                    return;
                }
            }
            else
            {
                snakeGrow = HandleItemInteraction(activeItem);
            }
        }

        _snakeBodyPositions.Insert(0, _newHeadPos);

        if (!snakeGrow)
        {
            _snakeBodyPositions.RemoveAt(_snakeBodyPositions.Count - 1);
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

    void LifeCheck()
    {
        maxHp--;
        if (maxHp <= 0)
        {
            OnHit(0f);
        }
        UpdateLife(maxHp);
    }
    #endregion

    #region Collision & Interaction

    private bool HandleItemInteraction(ActiveItem item)
    {
        bool shouldGrow = false;
        GameItem data = item.Data;
        
        _activeItems.Remove (Vector2Int.RoundToInt(item.Instance.transform.position));
        Destroy(item.Instance);

        if (data.name == "PreciousFood") OnPreciousFoodEaten?.Invoke();
        
        if (data.isCollectible)
        {
            _gemsCollected++;
            if (_gemsCollected > 15) OnAchieved?.Invoke("FoodC_m");
            audioSource.PlayOneShot(foodCollect);
            
            UpdateScore?.Invoke(data.scoreValue * _snakeBodySegments.Count, item.Instance.transform.position);
            CreateSnakeBodySegments(_snakeBodyPositions[^1]);
            shouldGrow = true;
            
            if (Random.value < challengeChance)
            {
                SpawnPattern(_challengePattern);
            }
            else
            {
                SpawnPattern(regularFoodPattern);
            }
        }
        else if (data.isObstacle)
        {
            if (_isInvulnerable)
            {
                shouldGrow = false;
            }
            else
            {
                if (data.isInstaKill)
                {
                    OnHit(0);
                    audioSource.PlayOneShot(explode);
                }
                else
                {
                    audioSource.PlayOneShot(rockHit);
                    int segmentsToDestroy = _snakeBodyPositions.Count / 2;
                
                    UpdateScore?.Invoke(-_snakeBodyPositions.Count, _snakeBodyPositions[0]);
                    for (int i = 0; i < segmentsToDestroy; i++)
                    {
                        if (_snakeBodyPositions.Count == 0 || _snakeBodySegments.Count == 0) break;
                    
                        _snakeBodyPositions.RemoveAt(_snakeBodyPositions.Count - 1);
                    
                        var segment = _snakeBodySegments[^1];
                        _snakeBodySegments.RemoveAt(_snakeBodySegments.Count - 1);
                        Destroy(segment);
                    
                        if (_snakeBodySegments.Count < 2) OnAchieved?.Invoke("Size_m"); 
                    }
                }
            }
        }
        else if (data.isPowerUp)
        {
            ApplyPowerUps(data.effectType, data.effectDuration);
            
            if (data.effectType.ToString() == "SpeedUp")
            {
                audioSource.PlayOneShot(speedUp);
                _speedUpCollected++;
                if (_speedUpCollected > 3) OnAchieved?.Invoke("SpeedC_m");
            }
        }
        
        return shouldGrow;
    }
    void OnHit(float duration)
    {
        StartCoroutine(OnHitRoutine(_moveDirection, duration));
        OnHitAction?.Invoke(true);
    }

    void OnGameOver()
    {
        _gameOver = true;
        audioSource.PlayOneShot(gameOver);
    }

    void OnItemDestroyed(Vector2Int itemPos)
    {
        if (_activeItems.ContainsKey(itemPos))
        {
            _activeItems.Remove(itemPos);
        }
    }

    #endregion

    #region Item & Snake Management
    
    void SpawnItem(string itemName, Vector2Int spawnPos)
    {
        GameItem itemData = itemTypes.FirstOrDefault(i => i.objName == itemName);
        if (itemData == null)
        {
            Debug.LogError($"Item '{itemName}' not found in Item Types list.");
            return;
        }

        var instance = Instantiate(itemData.prefab, (Vector2)spawnPos, Quaternion.identity); 
        var newActiveItem = new ActiveItem(itemData, instance);
        _activeItems[spawnPos] = newActiveItem;
        
        if (itemData.isPortal)
        {
            _portalInstances.Add(instance);
        }
    }
    
    void SpawnPattern(SpawnPattern pattern)
    {
        if (pattern == null || _spawnPointMap == null || pattern.items.Count == 0) return;

        string primaryItemName = pattern.items[0].itemName;
        List<Vector2Int> validPoints = _spawnPointMap.GetPointsFor(primaryItemName);

        if (validPoints == null || validPoints.Count == 0)
        {
            Debug.LogWarning($"No spawn points defined in SpawnPointMap for '{primaryItemName}'.");
            return;
        }

        if (!_lastSpawnPointIndex.ContainsKey(primaryItemName))
        {
            _lastSpawnPointIndex[primaryItemName] = -1;
        }

        for (int i = 0; i < validPoints.Count; i++)
        {
            int currentIndex = (_lastSpawnPointIndex[primaryItemName] + 1 + i) % validPoints.Count;
            Vector2Int anchorPos = validPoints[currentIndex];

            if (IsPatternValid(pattern, anchorPos))
            {
                foreach (var item in pattern.items)
                {
                    SpawnItem(item.itemName, anchorPos + item.relativePosition);
                }
                
                _lastSpawnPointIndex[primaryItemName] = currentIndex;
                return;
            }
        }
        
        Debug.LogWarning($"Could not place pattern '{pattern.name}' on any pre-defined points. All were blocked.");
    }

    void ChoosePattern()
    {
        T PickRandom<T>(List<T> pool)
        {
            if (pool?.Count > 0)
            {
                return pool[Random.Range(0, pool.Count)];
            }
            return default(T); 
        }

        _spawnPointMap    = PickRandom(spawnPointMap);
        _challengePattern = PickRandom(challengePattern);
    }
    
    void ClearPortals()
    {
        foreach (var portalInstance in _portalInstances)
        {
            _activeItems.Remove(Vector2Int.RoundToInt(portalInstance.transform.position));
            Destroy(portalInstance);
        }
        _portalInstances.Clear();
        _portalPairs.Clear();
    }

    void SpawnPortalPattern()
    {
        if (portalsPattern == null || _spawnPointMap == null) return;
    
        ClearPortals();

        List<Vector2Int> allPoints = _spawnPointMap.GetPointsFor("Portal");
        if (allPoints.Count == 0) return;

        for (int i = 0; i < 10; i++)
        {
            Vector2Int anchorPos = allPoints[Random.Range(0, allPoints.Count)];
            if (IsPatternValid(portalsPattern, anchorPos))
            {
                var portalPositions = new List<Vector2Int>();
                foreach (var item in portalsPattern.items)
                {
                    Vector2Int spawnPos = anchorPos + item.relativePosition;
                    SpawnItem(item.itemName, spawnPos);
                    portalPositions.Add(spawnPos);
                }

                if(portalPositions.Count >= 2)
                {
                    _portalPairs[portalPositions[0]] = portalPositions[1];
                    _portalPairs[portalPositions[1]] = portalPositions[0];
                }
                return;
            }
        }
    }

    
    bool IsPatternValid(SpawnPattern pattern, Vector2Int anchorPos)
    {
        foreach (var item in pattern.items)
        {
            Vector2Int pos = anchorPos + item.relativePosition;
            if (pos.x >= _xBound || pos.x <= -_xBound || pos.y >= _yBound || pos.y <= -_yBound) return false;
            if (_snakeBodyPositions.Contains(pos) || _activeItems.ContainsKey(pos)) return false;
        }
        return true;
    }
    
    void CreateSnakeBodySegments(Vector2Int pos, bool isHead = false)
    {
        var position = new Vector3(pos.x, pos.y, 0);
        var newSegment = Instantiate(snakeBodyPrefab, position, Quaternion.identity);
        _snakeBodySegments.Add(newSegment);
        if (isHead) newSegment.GetComponent<SnakeBody>().SetHead();
    }

    void ApplyPowerUps(PowerUpEffectType effectType, float duration)
    {
        switch (effectType)
        {
            case PowerUpEffectType.SpeedUp:
                if (_speedUpCoroutine != null) StopCoroutine(_speedUpCoroutine);
                _speedUpCoroutine = StartCoroutine(SpeedUpRoutine(duration));
                break;
            case PowerUpEffectType.Invulnerable:
                StartCoroutine(InvulnerabilityRoutine());
                break;
        }
    }
    void SpeedChanger(float frequencyV) { _snakeDelay = new WaitForSeconds(frequencyV); }
    #endregion
    
    #region Coroutines
    
    IEnumerator WaitAndMoveRoutine()
    {
        while (!_gameOver)
        {
            yield return _snakeDelay;
            Move();
        }
    }

    IEnumerator SpeedUpRoutine(float duration)
    {
        EventMessages?.Invoke("Speed Up", _snakeBodyPositions[0]);
        SpeedChanger(powerUpSpeed);
        yield return new WaitForSeconds(duration); 
        audioSource.PlayOneShot(speedDown);
        EventMessages?.Invoke("Speed Down", _snakeBodyPositions[0]);
        SpeedChanger(frequency);
        _speedUpCoroutine = null;
    }

    IEnumerator InvulnerabilityRoutine()
    {
        EventMessages?.Invoke("Invulnerable", _snakeBodyPositions[0]);
        _isInvulnerable = true;
        yield return new WaitForSeconds(10f);
        _isInvulnerable = false;
        EventMessages?.Invoke("Not Invulnerable", _snakeBodyPositions[0]);
    }
    
    IEnumerator SpawnSpeedUpCoroutine()
    {
        yield return new WaitForSeconds(7f);
        while (!_gameOver)
        {
            SpawnPattern(speedUpPattern);
            yield return new WaitForSeconds(14f);
        }
    }
    
    IEnumerator SpawnPortalCoroutine()
    {
        yield return new WaitForSeconds(portalSpawnInterval/2);
        while (!_gameOver)
        {
            SpawnPortalPattern();
            yield return new WaitForSeconds(portalSpawnInterval);
        }
    }

    IEnumerator SpawnInvulnerabilityCoroutine()
    {
        yield return new WaitForSeconds(5f);
        while (!_gameOver)
        {
            SpawnPattern(invulnerablityPattern);
            yield return new WaitForSeconds(25f);
        }
    }

    IEnumerator OnHitRoutine(Vector2Int direction, float duration)
    {
        yield return new WaitForSeconds(duration);
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
   #endregion
}