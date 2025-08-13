using System;
using System.Collections;
using UnityEngine;

public class InactivityDetector : MonoBehaviour
{
    [SerializeField] private bool isInMainMenu;
    private readonly int _menuWaitTime = 30;
    private readonly int _forceStartTime = 60;
    private bool _invokedEvent;
    
    public static Action Quit;
    public static Action ForceStart;
    public static Action<int> UpdateTimers;
    void Start()
    {
        StartCoroutine(UpdateTimer());
    }
    IEnumerator UpdateTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (isInMainMenu)
            {
                var timeToSend = Mathf.RoundToInt(_forceStartTime - Time.time);
                UpdateTimers?.Invoke(timeToSend);
                if (timeToSend < 0) ForceStart?.Invoke();
            }
            else
            {
                var timeToSend = Mathf.RoundToInt(_menuWaitTime - Time.time);
                UpdateTimers?.Invoke(timeToSend);
                if (timeToSend < 0) Quit?.Invoke();
            }
        }
    }
}
