using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contato
{
    /****************
    Clase Contato

    Informações de contato inseridas pelo usuário para serem exibidas em seu perfil

    Campos:
    -   idUsuario: ID do usuário que criou o objeto Comission
    -   tipoContato: descrição, inserida pelo usuário, de que tipo de informação de contato se trata (por exemplo, se é seu e-mail ou numero de telefone)
    -   valorContato: a informação de contato em si, inserida pelo usuário
    ****************/
    
    string idUsuario, tipoContato, valorContato;

    public Contato(string newIdUsuario, string newTipoContato, string newValorContato)
    {
        idUsuario = newIdUsuario;
        tipoContato = newTipoContato;
        valorContato = newValorContato;
        Debug.Log("Novo contato de usuário criado");
    }
    
    
    /****************
    Função Contato.ToDictionary()
    
    Converte o objeto Contato para Dictionary associando seus campos aos valores adequados.

    Retorno:
    -   Dicionário associando o nome de cada campo da classe Contato a seu respectivo valor
    ****************/
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["tipoContato"] = tipoContato;
        resultado["valorContato"] = valorContato;

        return resultado;
    }

}
