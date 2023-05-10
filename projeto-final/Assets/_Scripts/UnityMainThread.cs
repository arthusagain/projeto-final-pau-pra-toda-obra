using System;
using System.Collections.Generic;
using UnityEngine;

internal class UnityMainThread : MonoBehaviour
{
    internal static UnityMainThread wkr;
    Queue<Action> jobs = new Queue<Action>();

    //singleton
    void Awake() {
        wkr = this;
    }

    //adiciona funções à fila de execução da thread principal
    internal void AddJob(Action newJob) {
        jobs.Enqueue(newJob);
    }
    
    //executa funções na fila na thread principal
    void Update() {
        while (jobs.Count > 0) 
            jobs.Dequeue().Invoke();
    }

}
