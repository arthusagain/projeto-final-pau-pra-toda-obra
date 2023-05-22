using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoUsuario
{
    /****************
    Clase InfoUsuario

    Informações de um usuário, exibidas em seu perfil

    Campos:
    -   idUsuario: ID do usuário que criou o objeto Comission, também é seu e-mail usado no registro
    -   nomeUsuario: nome de exibição do usuário
    -   generoUsuario: gênero do usuário
    -   idadeUsuario: idade do usuário
    -   imagePath: endereço da imagem de perfil do usuário no servidor Firebase
    ****************/

    public string idUsuario, nomeUsuario, generoUsuario, idadeUsuario, imagePath, comissionList;

    public InfoUsuario(string newIdUsuario, string newNomeUsuario, string newGeneroUsuario, string newIdadeUsuario, string newImagePath){
        idUsuario = newIdUsuario;
        nomeUsuario = newNomeUsuario;
        generoUsuario = newGeneroUsuario;
        idadeUsuario = newIdadeUsuario;
        imagePath = newImagePath;
        //comissionList = "";
    }
    
    /****************
    Função InfoUsuario.ToDictionary()
    
    Converte o objeto InfoUsuario para Dictionary associando seus campos aos valores adequados.

    Retorno:
    -   Dicionário associando o nome de cada campo da classe InfoUsuario a seu respectivo valor
    ****************/
    public Dictionary<string, System.Object> ToDictionary() {
        Dictionary<string, System.Object> resultado = new Dictionary<string, System.Object>();
        resultado["idUsuario"] = idUsuario;
        resultado["nomeUsuario"] = nomeUsuario;
        resultado["generoUsuario"] = generoUsuario;
        resultado["idadeUsuario"] = idadeUsuario;
        resultado["imagePath"] = imagePath;
        //resultado["comissionList"] = comissionList;

        return resultado;
    }


}
