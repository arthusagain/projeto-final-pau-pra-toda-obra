using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class Comission
{
    public string idUsuario, titulo, descricao, imagePath, estado, cidade, frequencia, tipoContrato, precoExigido, classeComissao;
    private DateTime criadoEm;

    //constructor
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

    //dicionário para exportar para firebase
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
