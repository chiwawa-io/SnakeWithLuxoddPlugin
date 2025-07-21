using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance {get; private set; }
    
    [SerializeField] private WebSocketService websocketService;
    [SerializeField] private HealthStatusCheckService healthStatusCheckService;
    [SerializeField] private WebSocketCommandHandler webSocketCommandHandler;
    [SerializeField] private ReconnectService reconnectService;
    
    
    public WebSocketService WebSocketService => websocketService;
    public HealthStatusCheckService HealthStatusCheckService => healthStatusCheckService;
    public WebSocketCommandHandler WebSocketCommandHandler => webSocketCommandHandler;
    public ReconnectService ReconnectService => reconnectService;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
    
}