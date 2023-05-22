using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avaliacao
{
    /****************
    Clase Avaliacao

    Nota, representada como valor de 1 a 5 estrelas, que um usuário pode dar a outro usuário para registrar sua opinião sobre seus serviços prestados ou recebidos
    Na base de dados, as avaliações de um usuário de id 'usuárioA' são registradas sob o caminho 'avaliacao/usuárioA/', portanto não é necessário registrar o destinatário no objeto Avaliacao

    Campos:
    -   idUsuario: identificador do usuário que deixou a avaliação
    -   valorAval: nota atribuida pelo usuário.
    ****************/
    
    string idUsuario, valorAval;

    public Avaliacao(string newIdUsuario, string newValorAval)
    {
        idUsuario = newIdUsuario;
        valorAval = newValorAval;
        Debug.Log("Nova avaliação de usuário criada");
    }
    
    
    /****************
    Função Avaliacao.ToDictionary()
    
    Converte o objeto Avaliacao para Dictionary associando seus campos aos valores.

    Retorno:
    -   Dicionário associando o nome de cada campo da classe Avaliacao a seu respectivo valor
    ****************/
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["valorAval"] = valorAval;

        return resultado;
    }
}
