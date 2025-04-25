using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
        Debug.Log("Acción agregada al hilo principal.");
    }

    void Update()
    {
        while (_executionQueue.Count > 0)
        {
            Action action = null;
            lock (_executionQueue)
            {
                action = _executionQueue.Dequeue();
            }
            Debug.Log("Ejecutando acción en el hilo principal.");
            action?.Invoke();
        }
    }
}