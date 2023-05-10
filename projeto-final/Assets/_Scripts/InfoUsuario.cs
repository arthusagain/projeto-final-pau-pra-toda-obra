using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoUsuario
{
    public string idUsuario, nomeUsuario, generoUsuario, idadeUsuario, imagePath, comissionList;

    //constructor
    public InfoUsuario(string newIdUsuario, string newNomeUsuario, string newGeneroUsuario, string newIdadeUsuario, string newImagePath){
        idUsuario = newIdUsuario;
        nomeUsuario = newNomeUsuario;
        generoUsuario = newGeneroUsuario;
        idadeUsuario = newIdadeUsuario;
        imagePath = newImagePath;
        comissionList = "";
    }
    
    //dicionário para exportar para firebase
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["nomeUsuario"] = nomeUsuario;
        resultado["generoUsuario"] = generoUsuario;
        resultado["idadeUsuario"] = idadeUsuario;
        resultado["imagePath"] = imagePath;
        resultado["comissionList"] = comissionList;

        return resultado;
    }


}
