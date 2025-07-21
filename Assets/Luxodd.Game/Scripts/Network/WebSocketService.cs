
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Luxodd.Game.Scripts.HelpersAndUtils;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.CommandHandler;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace Luxodd.Game.Scripts.Network
{
    public class WebSocketService : MonoBehaviour
    {
        public bool IsConnected => _isConnected;
        public string SessionToken => GetSessionToken();

        [SerializeField] private NetworkSettingsDescriptor _settingsDescriptor = null;
        [SerializeField] private WebSocketLibraryWrapper _socketLibraryWrapper = null;

        [SerializeField] private FetchUrlQueryString _fetchUrlQueryString = null;

        private ClientWebSocket _clientWebSocket;
        private bool _isConnected;

        private System.Action<string> _successCallback;
        private System.Action<string> _errorCallback;

        private Action _onConnectedCallback;
        private Action _onConnectionErrorCallback;

        private Dictionary<CommandRequestType, Queue<CommandRequestHandler>> _commandRequestHandlers =
            new Dictionary<CommandRequestType, Queue<CommandRequestHandler>>();

        public void ConnectToServer(Action onSuccessCallback = null, Action onErrorCallback = null)
        {
            _onConnectedCallback = onSuccessCallback;
            _onConnectionErrorCallback = onErrorCallback;
            _ = StartConnectionAsync();
        }

        public void CloseConnection()
        {
            _socketLibraryWrapper.CloseWebSocketConnection();
        }

        public void BackToSystemWithError(string message, string error)
        {
            LoggerHelper.LogError(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(BackToSystemWithError)}] OK, message: {message}, error: {error}");
            _socketLibraryWrapper.NotifySessionEnd();
        }
        
        public void BackToSystem()
        {
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(BackToSystem)}] OK");
            _socketLibraryWrapper.NotifySessionEnd();
        } 

        public void SendCommand(CommandRequestType commandRequestType, string commandRequestJson,
            Action<CommandRequestHandler> onSuccess)
        {
            AddCommandRequestHandler(commandRequestType, onSuccess);
            SendEventInner(commandRequestJson);
        }

        private void Awake()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _socketLibraryWrapper.WebSocketOpenedEvent.AddListener(OnWebSocketConnectedHandler);
            _socketLibraryWrapper.WebSocketConnectionErrorEvent.AddListener(OnWebSocketConnectionErrorHandler);
            _socketLibraryWrapper.WebSocketClosedEvent.AddListener(OnWebSocketClosedHandler);
            _socketLibraryWrapper.MessageReceivedEvent.AddListener(OnMessageReceived);
        }

        private void OnWebSocketConnectionErrorHandler(string error)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketConnectionErrorHandler)}] OK, error:{error}");
            _isConnected = false;
        }

        private void OnWebSocketClosedHandler(int code)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketConnectionErrorHandler)}] OK");
            _isConnected = false;
        }

        private void UnsubscribeFromEvents()
        {
            _socketLibraryWrapper.WebSocketOpenedEvent.RemoveListener(OnWebSocketConnectedHandler);
            _socketLibraryWrapper.WebSocketConnectionErrorEvent.RemoveListener(OnWebSocketConnectionErrorHandler);
            _socketLibraryWrapper.WebSocketClosedEvent.RemoveListener(OnWebSocketClosedHandler);
            _socketLibraryWrapper.MessageReceivedEvent.RemoveListener(OnMessageReceived);
        }
        
        private string GetSessionToken()
        {
            var isDebug = false;
#if UNITY_EDITOR
            isDebug = true;
#endif

            var sessionToken = isDebug == false ? _fetchUrlQueryString.Token : _settingsDescriptor.DeveloperDebugToken;
            return sessionToken;
        }

        private async Task StartConnectionAsync()
        {
            await Task.Yield();

            var isDebug = false;
#if UNITY_EDITOR
            isDebug = true;
#endif

            var serverUrl = isDebug == false
                ? $"{_settingsDescriptor.ServerAddress}?token={_fetchUrlQueryString.Token}"
                : $"{_settingsDescriptor.ServerAddress}?token={_settingsDescriptor.DeveloperDebugToken}"; 

            var websocketUri =
                new Uri(serverUrl);



#if !UNITY_EDITOR
            try
            {
                await Task.Yield();
                LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK, connecting to: {websocketUri}...");
                _socketLibraryWrapper.StartWebSocket(websocketUri.AbsoluteUri);

                _isConnected = true;
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] Error: {ex}");
                Console.WriteLine(ex);
                _isConnected = false;
                throw;
            }
#else
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK, connecting to: {websocketUri.AbsoluteUri}");
            try
            {
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(websocketUri, CancellationToken.None);
                _isConnected = true;
                LoggerHelper.Log(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK, connected to: {websocketUri.AbsoluteUri}");
                OnWebSocketConnectedHandler();
                _ = ReceiveMessage();
            }
            catch (Exception e)
            {
                LoggerHelper.LogError($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] Error: {e}");
                Console.WriteLine(e);
                _isConnected = false;
                throw;
            }

#endif
        }

        private void OnMessageReceived(string message)
        {
            //LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnMessageReceived)}] " + message);
            UnityMainThread.Worker.AddJob(() => HandleMessageReceived(message));
        }

        private async void OnApplicationQuit()
        {
            if (_clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Application closing",
                    CancellationToken.None);
            }
        }

        private async Task ReceiveMessage()
        {
            var buffer = new byte[1024 * 4];
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result =
                        await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                        {
                            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            LoggerHelper.Log(
                                $"[{DateTime.Now}][{GetType().Name}][{nameof(ReceiveMessage)}] OK, received message: {receivedMessage}");
                            OnMessageReceived(receivedMessage);
                            //UnityMainThread.Worker.AddJob(() => _successCallback?.Invoke(receivedMessage));
                            
                            break;
                        }
                        case WebSocketMessageType.Close:
                            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(ReceiveMessage)}] OK, closed");
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
                                CancellationToken.None);
                            break;
                        case WebSocketMessageType.Binary:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"[{GetType().Name}][{nameof(ReceiveMessage)}] OK, error: {ex.Message}");
                    UnityMainThread.Worker.AddJob(() => _errorCallback?.Invoke(ex.Message));
                }
            }
        }

        private async Task SendMessageWebsocket(string message)
        {
            if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
            {
                LoggerHelper.LogError($"[{GetType().Name}][{nameof(SendMessageWebsocket)}] OK, not connected");
                _errorCallback?.Invoke($"[{GetType().Name}][{nameof(SendMessageWebsocket)}] Not connected");
                return;
            }

            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
                CancellationToken.None);
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(SendMessageWebsocket)}] OK, message sent: {message}");
        }

        private async void SendEventInner(string commandRequestRaw)
        {
#if !UNITY_EDITOR
            try
            {
                if (_isConnected == false)
                {
                    LoggerHelper.LogError($"[{GetType().Name}][{nameof(SendEventInner)}] Not connected");
                    return;
                }

                await Task.Yield();

                _socketLibraryWrapper.SendMessageToWebSocket(commandRequestRaw);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"[{GetType().Name}][{nameof(SendEventInner)}] OK, error: {ex.Message}");
            }
#else
            await SendMessageWebsocket(commandRequestRaw);
#endif
        }

        private void OnWebSocketConnectedHandler()
        {
            _onConnectedCallback?.Invoke();
        }

        private void AddCommandRequestHandler(CommandRequestType commandRequestType,
            Action<CommandRequestHandler> onSuccess)
        {
            var commandRequestHandler = new CommandRequestHandler()
            {
                CommandRequestType = commandRequestType,
                Id = 0,
                OnCommandResponseSuccessHandler = onSuccess,
            };

            if (_commandRequestHandlers.TryGetValue(commandRequestType, out var queueOfCommandRequestHandlers))
            {
                commandRequestHandler.Id = queueOfCommandRequestHandlers.Count;
                queueOfCommandRequestHandlers.Enqueue(commandRequestHandler);
            }
            else
            {
                var queue = new Queue<CommandRequestHandler>();
                queue.Enqueue(commandRequestHandler);
                _commandRequestHandlers.Add(commandRequestType, queue);
            }
        }

        private void HandleMessageReceived(string message)
        {
#if NEWTONSOFT_JSON
            //LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(HandleMessageReceived)}] OK, message: {message}");
            var responseJson = JsonConvert.DeserializeObject<CommandResponse>(message);
            if (responseJson == null) return;

            if (responseJson.Type.Contains("_"))
            {
                responseJson.Type = responseJson.Type.ToPascalCaseStyle();
            }

            if (Enum.TryParse<CommandResponseType>(responseJson.Type, out var commandResponseType) == false) return;

            var commandRequestType = commandResponseType.ToCommandRequestType();

            if (_commandRequestHandlers.TryGetValue(commandRequestType, out var queueOfCommandRequestHandlers) ==
                false) return;

            if (queueOfCommandRequestHandlers.Count > 0)
            {
                var commandRequestHandler = queueOfCommandRequestHandlers.Dequeue();
                commandRequestHandler.CommandResponse = responseJson;
                commandRequestHandler.RawResponse = message;
                commandRequestHandler.OnCommandResponseSuccessHandler?.Invoke(commandRequestHandler);
            }
            else
            {
                LoggerHelper.LogError(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(HandleMessageReceived)}] Error: Message: {message}, did not contain any command");
            }
            #endif
        }
    }

    public class CommandRequestJson
    {
#if NEWTONSOFT_JSON
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)] public string Version { get; set; }
        [JsonProperty("payload")] public object Payload { get; set; }
        #else
        public string Type { get; set; }
        public string Version { get; set; }
        public object Payload { get; set; }
        #endif
    }

    public class AmountPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("pin")] public string PinCode { get; set; }
        #else
        public int Amount { get; set; }
        public string PinCode { get; set; }
#endif
    }

    public class CommandResponse
    {
#if NEWTONSOFT_JSON
        [JsonProperty("msgver")] public string MessageVersion { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("ts")] public string TimeStamp { get; set; }
        [JsonProperty("status")] public int StatusCode { get; set; }
        [JsonProperty("payload")] public object Payload { get; set; }
        #else
        public string MessageVersion { get; set; }
        public string Type { get; set; }
        public string TimeStamp { get; set; }
        public int StatusCode { get; set; }
        public object Payload { get; set; }
        #endif
    }

    public class ProfilePayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("username")] public string UserName { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("profile_picture")] public string ProfilePicture { get; set; }
        #else
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ProfilePicture { get; set; }
        #endif
    }

    public class UserBalancePayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("balance")] public float Balance { get; set; }
        #else
        public float Balance { get; set; }
        #endif
    }

    public class LevelStatisticPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("level")]public int Level { get; set; }
        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]public int Score { get; set; }
        [JsonProperty("accuracy", NullValueHandling = NullValueHandling.Ignore)]public int Accuracy { get; set; }
        [JsonProperty("time_taken", NullValueHandling = NullValueHandling.Ignore)]public int TotalSeconds { get; set; }
        [JsonProperty("enemies_killed", NullValueHandling = NullValueHandling.Ignore)]public int EnemiesKilled { get; set; }
        [JsonProperty("completion_percentage", NullValueHandling = NullValueHandling.Ignore)]public int Progress { get; set; }
        #else
        public int Level { get; set; }
        public int Score { get; set; }
        public int Accuracy { get; set; }
        public int TotalSeconds { get; set; }
        public int EnemiesKilled { get; set; }
        public int Progress { get; set; }
        #endif
    }
    public class CommandRequestHandler
    {
        public CommandRequestType CommandRequestType { get; set; }
        public int Id { get; set; }
        public CommandResponse CommandResponse { get; set; }
        public string RawResponse { get; set; }
        public Action<CommandRequestHandler> OnCommandResponseSuccessHandler { get; set; }
    }

    public class UserDataPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("user_data")]public object Data { get; set; }
        #else
        public object Data { get; set; }
        #endif
    }
}
