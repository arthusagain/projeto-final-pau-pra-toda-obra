using System;
using System.Collections.Generic;
using UnityEngine;

internal class UnityMainThread : MonoBehaviour
{
    /****************
    Clase UnityMainThread

    Singleton responsável por garantir a execução de métodos e funções na thread principal

    Campos:
    -   wkr: instancia do singleton
    -   jobs: fila de tarefas a serem executadas na thread principal
    ****************/

    internal static UnityMainThread wkr;
    Queue<Action> jobs = new Queue<Action>();

    /****************
    Método MonoBehaviour.Awake()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre antes de todas as execuções de MonoBehaviour.Start()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   inicializa a instancia do singleton
    ****************/
    void Awake() {
        wkr = this;
    }

    /****************
    Método AddJob()

    Adiciona uma tarefa à fila para ser executada na thread principal do Unity

    Parâmetros:
    -   newJob: tarefa a ser executada

    Resultado: 
    -   a tarefa newJob é adicionada ao fim da fila
    ****************/
    internal void AddJob(Action newJob) {
        jobs.Enqueue(newJob);
    }
    
    /****************
    Método MonoBehaviour.Update()

    Método executada 60 vezes por segundo.
    Sobrecarregada para executar operações associadas a atualizar valores na tela

    Resultado: 
    -   se houver uma tarefa na fila, executa-a na thread principal
    ****************/
    void Update() {
        while (jobs.Count > 0) 
            jobs.Dequeue().Invoke();
    }

}
