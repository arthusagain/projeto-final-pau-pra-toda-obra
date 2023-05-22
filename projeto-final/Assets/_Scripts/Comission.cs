using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class Comission
{
    /****************
    Clase Comission

    Anúncio de contrato criado por um usuário, seja buscando um trabalhador ou buscando um trabalho.

    Campos:
    -   idUsuario: ID do usuário que criou o objeto Comission
    -   titulo: titulo atribuído à comission em sua criação
    -   descricao: descrição atribuída à comission em sua criação
    -   imagePath: caminho, no banco de dados, para a imagem salva atribuída à comission em sua criação
    -   estado: unidade federativa brasileira onde o serviço é requisitado
    -   cidade: cidade brasileira onde o serviço é requisitado
    -   frequencia: dias da semana em que o serviço deseja ser repetido. Caso seja uma atividade operada uma única vez, este valor será igual a string vazia ("")
    -   tipoContrato: texto que classifica o tipo de contrato buscado/oferecido (por exemplo, serviços de babá ou pedreiro)
    -   precoExigido: valor sugerido pelo criador da Comission pelo serviço prestado/buscado
    -   classeComissao: string que determina se o usuário criador da Comission está buscando um trabalhador (representado como "Comissão") ou um contratante que queria pagar por seu trabalho (representado como "Serviço")
    -   criadoEm: data e hora da criação do objeto, para fins de ordenação
    ****************/
    
    public string idUsuario, titulo, descricao, imagePath, estado, cidade, frequencia, tipoContrato, precoExigido, classeComissao;
    private DateTime criadoEm;

    public Comission(string newIdUsuario, string newTitulo, string newDescricao, string newImagePath, string newEstado, string newCidade, string newFrequencia, string newTipoContrato, string newPrecoExigido, string newClasseComissao){
        idUsuario = newIdUsuario;
        titulo = newTitulo;
        descricao = newDescricao;
        imagePath = newImagePath;
        estado = newEstado;
        cidade = newCidade;
        frequencia = newFrequencia;
        tipoContrato = newTipoContrato;
        precoExigido = newPrecoExigido;
        classeComissao = newClasseComissao;
        criadoEm = DateTime.Now;

        Debug.Log("Nova comissao criada");
    }

    /****************
    Função Comission.ToDictionary()
    
    Converte o objeto Comission para Dictionary associando seus campos aos valores adequados.

    Retorno:
    -   Dicionário associando o nome de cada campo da classe Comission a seu respectivo valor
    ****************/
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["titulo"] = titulo;
        resultado["descricao"] = descricao;
        resultado["imagePath"] = imagePath;
        resultado["estado"] = estado;
        resultado["cidade"] = cidade;
        resultado["frequencia"] = frequencia;
        resultado["tipoContrato"] = tipoContrato;
        resultado["precoExigido"] = precoExigido;
        resultado["classeComissao"] = classeComissao;
        resultado["criadoEm"] = criadoEm.ToString("yyyy/MM/dd HH:mm:ss");

        return resultado;
    }


}
