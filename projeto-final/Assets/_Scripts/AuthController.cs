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
    /****************
    Clase AuthController

    Singleton responsável por:
    -   Validar a capacidade do dispositivo de executar o aplicativo
    -   Registrar e autenticar usuários
    -   Exibir mensagens de erro
    -   Retornar informações do usuário atual para as demais classes
    
    Campos:
    -   authInstance: instancia deste singleton
    -   emailInputReg: caixa de texto onde o usuário insere o e-mail com o qual deseja se registrar
    -   passwordInputReg: caixa de texto onde o usuário insere a senha com a qual deseja se registrar
    -   emailInputLog: caixa de texto onde o usuário insere o e-mail com o qual deseja acessar o sistema
    -   passwordInputLog: caixa de texto onde o usuário insere a senha com a qual deseja acessar o sistema
    -   editarPerfilMenu: referência à tela de editar perfil
    -   mainMenu: referência à tela de menu principal
    -   authMenu: referência à tela de autenticação
    -   errorPanel: referência ao painel de exibição de mensagem de erro
    -   errorDisplay: referência ao texto a ser editado no painel de erro
    -   message: buffer para a mensagem de erro a ser exibida
    ****************/
    internal static AuthController authInstance;

    [Header("Registro")]
    [SerializeField]
    private InputField emailInputReg;
    [SerializeField]
    private InputField passwordInputReg;
    [SerializeField]
    private InputField confirmPasswordInputReg;
    [Header("Login")]
    [SerializeField]
    private InputField emailInputLog;
    [SerializeField]
    private InputField passwordInputLog;
    [Header("Referencias de objetos")]
    [SerializeField]
    private GameObject editarPerfilMenu;
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject authMenu;
    public GameObject errorPanel;
    public Text errorDisplay;
    string message;

    /****************
    Método MonoBehaviour.Awake()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre antes de todas as execuções de MonoBehaviour.Start()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   inicializa a instancia do singleton
    -   evita que mais de um GameObject do Unity crie uma instancia do singleton
    ****************/
    void Awake() {
        if (authInstance!= null && authInstance != this)
        {   
            Destroy(gameObject);
        }
        else
        {
            authInstance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    /****************
    Método MonoBehaviour.Start()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre após todas as execuções de MonoBehaviour.Awake()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   Caso o dispositivo executando o aplicativo não atenda aos requisitos para executar um programa que utiliza Google Firebase, bloqueia o uso do resto do aplicativo e exibe erro na tela
    ****************/
    void Start(){
        
        // Verifica ao inicializar se o dispositivo possui dependencias necessarias para a execução do firebase, e tenta corrigir as falhas resultantes caso os requisitos não sejam atendidos
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
                    // Caso não seja possível corrigir as falhas de dependências, uma mensagem de erro é exibida e o usuário é impedido de prosseguir
                    RaiseErrorPanel("Seu dispositivo não tem os requisitos necessários para executar este aplicativo.");
                    UnityMainThread.mainThreadInstance.AddJob(()=>{
                        errorPanel.GetComponent<Button>().interactable=false;
                    });
                }
        });
    }

    /****************
    Método AuthController.Login()

    Responsável por realisar uma tentativa de acessar o sistema com credenciais inseridas
    Em caso de falha, uma mensagem de erro adequada é exibida
    Em caso de sucesso, o usuário acessa o sistema com as credenciais inseridas e é direcionado ao menu principal

    Entrada:
    - Texto preenchido no campo AuthController.emailInputLog
    - Texto preenchido no campo AuthController.passwordInputLog

    Resultados possíveis:
    -   Se um dos campos de texto obrigatórios não for preenchido, cancela operação e notifica o usuário em mensagem de erro
    -   Se os valores preenchidos resultam em um erro, cancela a operação e exibe mensagem de erro de acordo com o método AuthController.GetErrorCodeMessage()
    -   Se os valores preenchidos correspondem a credenciais válidas de um usuário registrado, o acesso ao sistema é permitido e o usuário é levado à tela de menu principal
    ****************/
    public void LogIn(){
        // Verifica se algum dos campos obrigatórios não foi preenchido
        if (emailInputLog.text.Equals("") || passwordInputLog.text.Equals(""))
        {
            RaiseErrorPanel("Insira um email e uma senha previamente registrados para entrar.");
            return;
        }

        // Realiza uma tentativa de entrada no sistema com as credenciais inseridas
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInputLog.text,passwordInputLog.text).ContinueWith((task=>{
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync foi cancelada.");
                return;
            }
            if (task.IsFaulted) {
                // Caso um erro ocorra, converte o código do erro em mensagem adequada para que o usuário se corrija
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0]  as Firebase.FirebaseException;
                
                GetErrorCodeMessage((AuthError)e.ErrorCode);
                RaiseErrorPanel(message);
                return;
            }

            //Caso nenhum erro ocorra, ativa telas do menu principal e desativa telas do menu de autenticação
            Firebase.Auth.FirebaseUser novoUsuario = task.Result;

            UnityMainThread.mainThreadInstance.AddJob(()=>{
            authMenu.SetActive(false);
            mainMenu.SetActive(true);
            });
            
        }));
            

    }
    
    /****************
    Método AuthController.Registrar()

    Responsável por adicionar um novo usuário do sistema com as credenciais inseridas, caso válidas, ou exibir quaisquer erros que as invalidem ou impeçam a operação

    Entrada:
    - Texto preenchido no campo AuthController.emailInputReg
    - Texto preenchido no campo AuthController.passwordInputReg

    Resultados possíveis:
    -   Se um dos campos de texto obrigatórios não for preenchido, cancela operação e notifica o usuário em mensagem de erro
    -   Se os valores preenchidos resultam em um erro, cancela a operação e exibe mensagem de erro de acordo com o Método AuthController.GetErrorCodeMessage()
    -   Se os valores preenchidos correspondem a credenciais válidas e o e-mail não corresponde a um usuário já registrado, o acesso ao sistema é permitido e o usuário é levado à tela de edição de perfil
    
    ****************/
    public void Registrar(){
        // Verifica se algum dos campos obrigatórios não foi preenchido
        if (emailInputReg.text.Equals("") || passwordInputReg.text.Equals(""))
        {
            Debug.LogError("Campos vazios.");
            RaiseErrorPanel("Para se registrar, insira um e-mail válido e uma senha nos campos indicados");
            return;
        }

        // Realiza uma tentativa de registro no sistema com as credenciais inseridas
        Firebase.Auth.FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInputReg.text, passwordInputReg.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync foi cancelada.");
                return;
            }
            if (task.IsFaulted) {
                // Caso um erro ocorra, converte o código do erro em mensagem adequada para que o usuário se corrija
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0]  as Firebase.FirebaseException;
                GetErrorCodeMessage((AuthError)e.ErrorCode);
                RaiseErrorPanel(message);
                return;
            }

            //Caso nenhum erro ocorra, ativa telas do menu de edição de perfil e desativa telas do menu de autenticação
            Firebase.Auth.FirebaseUser novoUsuario = task.Result;
            UnityMainThread.mainThreadInstance.AddJob(()=>{
            // Para manter a organização da funcionalidade de botão de retornar à janela anterior, os valores adequados são adicionados à suas pilhas. Este funcionamento é explicado na classe MenuController
            MenuController.menuControllerInstance.IrNovoTitulo("Criar meu perfil");
            MenuController.menuControllerInstance.IrNovaJanela(editarPerfilMenu);
            MenuController.menuControllerInstance.IrNovoAux("");
            authMenu.SetActive(false);
            editarPerfilMenu.SetActive(true);
            });
        });

            
    }

    /****************
    Método AuthController.LogOut()

    Desconecta o usuário do sistema do aplicativo

    Resultados: Ao fim da execução do Método o usuário não estará mais conectado ao sistema, necessitando autenticar-se novamente para acessar as funcionalidades do aplicativo
    ****************/
    public void LogOut()
    {
        // Se o usuário não está conectado, nada é feito
        if(Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            return;
        }

        // Caso contrario, desconecta o usuário
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
    }

    /****************
    Método AuthController.GetErrorCodeMessage()

    Preenche o campo AuthController.message com um texto descrevendo que problema resultou no erro recebido no parâmetro errorCode

    Parâmetros:
    - AuthError errorCode: o código de um erro do Firebase, gerado na execução das demais funções de AuthController quando algo impede que o usuário se registre ou acesse o sistema

    Resultados: o campo AuthController.message será sobreescrito com uma mensagem de erro adequada conforme o erro recebido
    ****************/
    private void GetErrorCodeMessage(AuthError errorCode)
    {
        // Compara possíveis valores de Firebase.Auth.AuthError com o valor recebido em errorCode
        switch (errorCode)
        {
        case AuthError.AccountExistsWithDifferentCredentials:
            // Para cada valor possível, registra em AuthController.message uma mensagem de erro adequada
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

    /****************
    Função AuthController.GetCurrentUserId()

    Retorna o e-mail com o qual o usuário atualmente utilizando o sistema se registrou

    Entrada:
    - Dados do usuário já autenticado no sistema

    Retorno: 
    - E-mail de registro do usuário atual
    ****************/
    public string GetCurrentUserId()
    {
        return Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email;
    }

    /****************
    Método AuthController.RaiseErrorPanel()

    Habilita painel AuthController.errorPanel para aparecer na frente das demais telas do aplicativo, exibindo a mensagem de erro desejada

    Parâmetros:
    - targetMessage: contem a mensagem a ser exibida

    Resultado: 
    -   o painel AuthController.errorPanel cobre a tela e exibe o valor targetMessage como texto
    ****************/
    public void RaiseErrorPanel(string targetMessage)
    {
        try
        {
            UnityMainThread.mainThreadInstance.AddJob(()=>{
                errorPanel.SetActive(true);
                errorDisplay.text = targetMessage;
            });
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    /****************
    Método AuthController.ToggleShowPassword()

    Habilita ou desabilita a exibição dos caracteres digitados da senha

    Entrada:
    - targetMessage contem a mensagem a ser exibida

    Resultado:
    - se a senha estava escondida, é exibida
    - se a senha estava sendo exibida, é escondida
    ****************/
    public void ToggleShowPassword()
    {
        //se é senha, exibe como texto planio
        if(passwordInputLog.contentType == InputField.ContentType.Password)
        {
            passwordInputLog.contentType = InputField.ContentType.Standard;
            passwordInputReg.contentType = InputField.ContentType.Standard;
            confirmPasswordInputReg.contentType = InputField.ContentType.Standard;

            
            passwordInputLog.ForceLabelUpdate();
            passwordInputReg.ForceLabelUpdate();
            confirmPasswordInputReg.ForceLabelUpdate();
        }
        //se é texto plano, exibe como senha
        else{
            passwordInputLog.contentType = InputField.ContentType.Password;
            passwordInputReg.contentType = InputField.ContentType.Password;
            confirmPasswordInputReg.contentType = InputField.ContentType.Password;

            
            passwordInputLog.ForceLabelUpdate();
            passwordInputReg.ForceLabelUpdate();
            confirmPasswordInputReg.ForceLabelUpdate();

        }

    }


}
