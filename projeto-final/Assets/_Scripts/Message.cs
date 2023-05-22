using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class Message
{
    /****************
    Clase Message

    Mensagem enviada de um usuário para outro através da funcionalidade de conversa

    Campos:
    -   remetente: identificador do usuário que enviou a mensagem
    -   destino: identificador do usuário que recebeu a mensagem
    -   conteudo: texto da mensagem
    -   criadoEm: momento de envio da mensagem
    ****************/

    public string remetente, destino, conteudo;
    private DateTime criadoEm;

    public Message(string newRemetente, string newDestino, string newConteudo){
        remetente = newRemetente;
        destino = newDestino;
        conteudo = newConteudo;
        criadoEm = DateTime.Now;
        Debug.Log("Nova mensagem criada");
    }

    //dicionário para exportar para firebase
    /****************
    Função Message.ToDictionary()
    
    Converte o objeto Message para Dictionary associando seus campos aos valores.

    Retorno:
    -   Dicionário associando o nome de cada campo da classe Message a seu respectivo valor
    ****************/
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["remetente"] = remetente;
        resultado["destino"] = destino;
        resultado["conteudo"] = conteudo;
        resultado["criadoEm"] = criadoEm.ToString("yyyy/MM/dd HH:mm:ss");

        return resultado;
    }


}
