using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class Message
{
    public string remetente, destino, conteudo;
    private DateTime criadoEm;

    //constructor
    public Message(string newRemetente, string newDestino, string newConteudo){
        remetente = newRemetente;
        destino = newDestino;
        conteudo = newConteudo;
        criadoEm = DateTime.Now;
        Debug.Log("Nova mensagem criada");
    }

    //dicionário para exportar para firebase
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["remetente"] = remetente;
        resultado["destino"] = destino;
        resultado["conteudo"] = conteudo;
        resultado["criadoEm"] = criadoEm.ToString("yyyy/MM/dd HH:mm:ss");

        return resultado;
    }


}
