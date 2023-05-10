using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks; 
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Storage;
using Firebase.Auth;

public class AuthController : MonoBehaviour
{
    [Header("Registro")]
    [SerializeField]
    private InputField emailInputReg;
    [SerializeField]
    private InputField passwordInputReg;
    [Header("Login")]
    [SerializeField]
    private InputField emailInputLog;
    [SerializeField]
    private InputField passwordInputLog;
    [Header("Referencias de objetos")]
    [SerializeField]
    private GameObject globalMenu;
    [SerializeField]
    private GameObject editarPerfilMenu;
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject authMenu;
    public GameObject errorPanel;
    public Text errorDisplay;
    string message;

    void Start(){
        
        //verifica ao inicializar se o dispositivo possui dependencias necessarias para a execução do firebase, e tenta corrigir caso não tenha
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(checkTask =>
        {
            Firebase.DependencyStatus status = checkTask.Result;
            if (status != Firebase.DependencyStatus.Available) {
                return Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(t => {
                    return Firebase.FirebaseApp.CheckDependenciesAsync();
                }).Unwrap();
            } else {
                return checkTask;
            }
        }).Unwrap().ContinueWith(task => {
            var dependencyStatus = task.Result;
                if (dependencyStatus != Firebase.DependencyStatus.Available) {
                    RaiseErrorPanel("Seu dispositivo não tem os requisitos necessários para executar este aplicativo.");
                    UnityMainThread.wkr.AddJob(()=>{
                        errorPanel.GetComponent<Button>().interactable=false;
                    });
                }
        });
    }

    //tenta acessar sistema com credenciais inseridas
    public void LogIn(){
        if (emailInputLog.text.Equals("") || passwordInputLog.text.Equals(""))
        {
            RaiseErrorPanel("Insira um email e uma senha previamente registrados para entrar.");
            return;
        }

        Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInputLog.text,passwordInputLog.text).ContinueWith((task=>{
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync foi cancelada.");
                return;
            }
            if (task.IsFaulted) {
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0]  as Firebase.FirebaseException;
                
                GetErrorCodeMessage((AuthError)e.ErrorCode);
                RaiseErrorPanel(message);
                return;
            }

            Firebase.Auth.FirebaseUser novoUsuario = task.Result;

            UnityMainThread.wkr.AddJob(()=>{
            authMenu.SetActive(false);
            mainMenu.SetActive(true);
            });
            
        }));
            

    }

    //tenta criar um novo usuário do sistema com as credenciais inseridas
    public void Registrar(){
        if (emailInputReg.text.Equals("") || passwordInputReg.text.Equals(""))
        {
            Debug.LogError("Campos vazios.");
            RaiseErrorPanel("Para se registrar, insira um e-mail válido e uma senha nos campos indicados");
            return;
        }

        Firebase.Auth.FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInputReg.text, passwordInputReg.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync foi cancelada.");
                return;
            }
            if (task.IsFaulted) {
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0]  as Firebase.FirebaseException;
                GetErrorCodeMessage((AuthError)e.ErrorCode);
                RaiseErrorPanel(message);
                return;
            }

            Firebase.Auth.FirebaseUser novoUsuario = task.Result;
            UnityMainThread.wkr.AddJob(()=>{
            globalMenu.GetComponent<MenuController>().IrNovoTitulo("Criar meu perfil");
            globalMenu.GetComponent<MenuController>().IrNovaJanela(editarPerfilMenu);
            globalMenu.GetComponent<MenuController>().IrNovoAux("");
            authMenu.SetActive(false);
            editarPerfilMenu.SetActive(true);
            });
        });

            
    }

    //tenta desconectar do sistema
    public void LogOut()
    {
        if(Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            return;
        }
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
    }

    //converte código de erro do firebase em mensagem para exibir para usuário
    private void GetErrorCodeMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
        case AuthError.AccountExistsWithDifferentCredentials:
            message = "A conta já existe usando outras credenciais";
            break;
        case AuthError.MissingPassword:
            message = "Insira uma senha";
            break;
        case AuthError.WeakPassword:
            message = "A senha inserida é fraca";
            break;
        case AuthError.WrongPassword:
            message = "Senha incorreta";
            break;
        case AuthError.EmailAlreadyInUse:
            message = "Já existe uma conta com este e-mail";
            break;
        case AuthError.InvalidEmail:
            message = "O e-mail inserido não é válido";
            break;
        case AuthError.MissingEmail:
            message = "Insira um e-mail";
            break;
        default:
            message = "Ocorreu um erro. Verifique suas credenciais e sua conexão e tente novamente.";
            break;
        }
        return;
    }

    //retorna email único do usuário atualmente conectado para uso em outras classes
    public string GetCurrentUserId()
    {
        return Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email;
    }

    //habilita painel exibindo mensagem de erro
    public void RaiseErrorPanel(string targetMessage)
    {
        try
        {
            UnityMainThread.wkr.AddJob(()=>{
                errorPanel.SetActive(true);
                errorDisplay.text = targetMessage;
            });
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

}
