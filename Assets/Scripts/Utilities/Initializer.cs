using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour
{
    public static bool hasInitialized;
    [SerializeField] private float waitTime;
    void Start()
    {
        if (hasInitialized) Destroy(gameObject);
        else
        {
            NetworkManager.Instance.WebSocketService.ConnectToServer(OnConnectionSuccess, OnConnectionFail);
            hasInitialized = true;
        }

        StartCoroutine(LoadMenu());
    }
    void OnConnectionSuccess()
    {
        NetworkManager.Instance.HealthStatusCheckService.Activate();
        PlayerDataManager.Instance.LoadData();
    }
    void OnConnectionFail()
    {
        Debug.Log("Connection Failed");
    }

    IEnumerator LoadMenu()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Menu");
    }
}
