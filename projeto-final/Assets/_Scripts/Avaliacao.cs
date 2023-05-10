using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avaliacao
{
    string idUsuario, valorAval;

    //constructor
    public Avaliacao(string newIdUsuario, string newValorAval)
    {
        idUsuario = newIdUsuario;
        valorAval = newValorAval;
        Debug.Log("Nova avaliação de usuário criada");
    }
    
    //dicionário para exportar para firebase
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["valorAval"] = valorAval;

        return resultado;
    }
}
