using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contato
{
    string idUsuario, tipoContato, valorContato;

    //constructor
    public Contato(string newIdUsuario, string newTipoContato, string newValorContato)
    {
        idUsuario = newIdUsuario;
        tipoContato = newTipoContato;
        valorContato = newValorContato;
        Debug.Log("Novo contato de usuário criado");
    }
    
    
    //dicionário para exportar para firebase
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["tipoContato"] = tipoContato;
        resultado["valorContato"] = valorContato;

        return resultado;
    }

}
