using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    /****************
    Clase MenuController

    Singleton responsável por:
    -   Empilhar janelas visitadas, seus títulos e os valores auxiliares necessários para replicá-las
    -   Permitir retornar à janela visitada anteriormente, até retornar ao menu inicial
    -   Retornar valores auxiliares da janela atual à outras classes do sistema
    -   Atualizar texto do título da página
    
    Campos:
    -   menuControllerInstance: instancia do singleton
    -   textoTituloPagina: texto para exibir no título da página
    -   tituloPagina: objeto texto na interface onde o título é exibido
    -   janela Inicio: referência ao objeto de janela de menu inicial
    -   bottomMenu: referência ao painel de menu inferior
    -   topMenu: referência ao painel de menu superior
    -   perfilMenu: referência ao painel de menu de perfil
    -   editarPerfilMenu: referência ao painel de menu de editar perfil
    -   contratosMenu: referência ao painel de menu de contratos
    -   mensagensMenu: referência ao painel de menu de mensagens
    -   sTitulos: pilha de títulos de janelas visitadas
    -   sJanelas: pilha de referências de janelas visitadas
    -   sAux: pilha de valores auxiliares de janelas visitadas
    ****************/

    internal static MenuController menuControllerInstance;

    string textoTituloPagina = "";

    [SerializeField]
    private GameObject tituloPagina;
    [SerializeField]
    private GameObject janelaInicio;
    /*[SerializeField]
    private GameObject bottomMenu;*/
    [SerializeField]
    private GameObject topMenu;
    [SerializeField]
    private GameObject perfilMenu;
    [SerializeField]
    private GameObject editarPerfilMenu;
    [SerializeField]
    private GameObject contratosMenu;
    [SerializeField]
    private GameObject mensagensMenu;
    /*
    [SerializeField]
    private GameObject databridgeObj;
    private DataBridge databridge;*/

    Stack<string> sTitulos = new Stack<string>();
    Stack<GameObject> sJanelas = new Stack<GameObject>();
    Stack<string> sAux = new Stack<string>();
    
    
    /****************
    Método MonoBehaviour.Awake()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre antes de todas as execuções de MonoBehaviour.Start()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   inicializa a instancia do singleton
    -   evita que mais de um GameObject do Unity crie uma instancia do singleton
    ****************/
    void Awake() {
        if (menuControllerInstance!= null && menuControllerInstance != this)
        {   
            Destroy(menuControllerInstance);
        }
        else
        {
            menuControllerInstance = this;
        }
        DontDestroyOnLoad(menuControllerInstance);
    }

    /****************
    Método MonoBehaviour.Start()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre após todas as execuções de MonoBehaviour.Awake()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   inicializa a pilha com as informações para retornar à janela inicial
    ****************/
    void Start()
    {
        //databridge = databridgeObj.GetComponent<DataBridge>();
        textoTituloPagina = "Menu Principal";
        sTitulos.Push("Menu Principal");
        sJanelas.Push(janelaInicio);
        sAux.Push("");
    }
    
    /****************
    Método MonoBehaviour.Update()

    Método executada 60 vezes por segundo.
    Sobrecarregada para executar operações associadas a atualizar valores na tela

    Resultado: 
    -   o título da página se mantém atualizado de acordo com o conteúdo de textoTituloPagina
    -   se o botão "anterior" nativo do celular for pressionado, retorna à tela anterior
    ****************/
    void Update()
    {
        //manter título atualizado
        tituloPagina.GetComponent<Text>().text = textoTituloPagina;

        //ativar retorno caso botão nativo do celular seja precionado
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            IrJanelaAnterior();
        }
    }
    
    /****************
    Método IrNovoAux()

    Empilha novo valor auxiliar na pilha de valores auxiliares à navegação

    Parâmetros:
    -   novoAux: novo valor a ser empilhado

    Resultado: 
    -   o valor é registrado na pilha. alternativamente, se o valor for -2, copia e empilha o valor inserido imediatamente antes
    ****************/
    public void IrNovoAux(string novoAux)
    {
        //se auxiliar for -2, é sinal para usar mesmo auxiliar de página anterior
        if (novoAux.Equals("-2"))
        {
            Debug.Log("aux = -2. empilhando anterior = "+sAux.Peek());
            sAux.Push(sAux.Peek());
        }
        else {
            sAux.Push(novoAux);
        }
    }

    /****************
    Método IrNovoTitulo()

    Empilha novo titulo na pilha de titulos de janelas

    Parâmetros:
    -   novoTitulo: novo valor a ser empilhado

    Resultado: 
    -   o valor é registrado na pilha
    ****************/
    public void IrNovoTitulo(string novoTitulo)
    {
        textoTituloPagina = novoTitulo;
        sTitulos.Push(novoTitulo);
    }

    /****************
    Método IrJanelaAnterior()

    Remove valor do topo de cada pilha (sAux, sJanelas e sTitulos) e os utiliza para navegar para janela anterior carregando os dados apropriados
    Executa quaisquer métodos e funções relevantes para recarregar a página

    Resultado: 
    -   se já na página inicial, nada ocorre e cancela a execução do método
    -   se saindo da janela de conversa, pausa a atualização de novas mensagens
    -   a tela vista imediatamente antes da atual volta a ser exibida, exceto telas que se usou este método para sair
    -   se neste processo a tela inicial for acessada novamente, as pilhas são resetadas
    ****************/
    public void IrJanelaAnterior()
    {
        //não tem página anterior a atual se já estiver no menu inicial
        if (sJanelas.Peek()==janelaInicio)
        {
            ResetNav(0);
            return;
        }
        //se está saindo da janela de troca de mensagens, para de ouvir por atualizações na lista de mensagens
        if(sJanelas.Peek()==mensagensMenu)
        {
            //databridge.RemoveListener(sAux.Peek());
            DataBridge.dataBridgeInstance.RemoveListener(sAux.Peek());
        }

        //desempilha as 3 pilhas de navegação, desativando tela atual e reativando a anterior
        sTitulos.Pop();
        sJanelas.Pop().SetActive(false);
        sAux.Pop();
        textoTituloPagina = sTitulos.Peek();
        sJanelas.Peek().SetActive(true);

        //dependendo da janela carregada, carrega os dados necessários
        switch(sTitulos.Peek())
        {
            case "Contratos deste usuário":
                //databridge.RefreshComissionUsuarioList();
                DataBridge.dataBridgeInstance.RefreshComissionUsuarioList();
                break;
            case "Meu Perfil":
                //databridge.CarregarPerfil(sAux.Peek());
                DataBridge.dataBridgeInstance.CarregarPerfil(sAux.Peek());
                break;
            case "Avaliações":
                //databridge.RefreshAvaliacaoList();
                DataBridge.dataBridgeInstance.RefreshAvaliacaoList();
                break;
            case "Serviços Disponíveis":
                //databridge.RefreshComissionList("Serviço");
                DataBridge.dataBridgeInstance.RefreshComissionList("Serviço");
                Debug.Log("Voltando para lista de serviços");
                break;
            case "Comissões Disponíveis":
                //databridge.RefreshComissionList("Comissão");
                DataBridge.dataBridgeInstance.RefreshComissionList("Comissão");
                Debug.Log("Voltando para lista de comissões");
                break;
            case "Minhas Conversas":
                //databridge.RefreshConversasList();
                DataBridge.dataBridgeInstance.RefreshConversasList();
                break;
            default://se auxiliar começa com '-', é uma chave única de informações de contrato
                if(sAux.Peek().Length > 0 && sAux.Peek()[0].Equals("-"))
                {
                    //databridge.CarregaInfoContrato(sAux.Peek(), true);
                    DataBridge.dataBridgeInstance.CarregaInfoContrato(sAux.Peek(), true);
                }
                break;
        }

        //se voltou ao menu principal, zera a pilha
        if(string.Equals(sTitulos.Peek(),"Menu Principal"))
        {
            UnityMainThread.mainThreadInstance.AddJob(()=>{
            topMenu.SetActive(false);
            });
            ResetNav(0);
        }
    }

    /****************
    Método IrNovaJanela()

    Empilha nova janela na pilha de janelas visitadas

    Parâmetros:
    -   novaJanela: novo valor a ser empilhado

    Resultado: 
    -   uma referencia à nova janela é adicionada ao topo da pilha
    ****************/
    public void IrNovaJanela(GameObject novaJanela)
    {
        sJanelas.Push(novaJanela);
    }

    /****************
    Método ResetNav()

    Restaura os valores iniciais das pilhas

    Parâmetros:
    -   novoMenu: [DESCONTINUADO]

    Resultado: 
    -   as pilhas sTitulos, sJanelas e SAux possuem apenas os valores associados ao menu inicial ao fim da execução do método
    ****************/
    public void ResetNav(int novoMenu)
    {
        sTitulos.Clear();
        sJanelas.Clear();
        sAux.Clear();

        /* funcionalidade de menu de navegação rápida entre telas de perfil, serviços, comissões e conversas. 
        descartado por apresentar problemas com resto da navegação para janela anterior, e por estar fora do escopo

        switch (novoMenu)
        {
            case 1:
            {
                IrNovoTitulo("Meu Perfil");
                IrNovoAux("-1");
                IrNovaJanela(perfilMenu);
            }
            break;
            case 2:
            {
                IrNovoTitulo("Comissões Disponíveis");
                IrNovoAux("");
                IrNovaJanela(contratosMenu);
            }

            break;
            case 3:
            {
                IrNovoTitulo("Contratos Disponíveis");
                IrNovoAux("");
                IrNovaJanela(contratosMenu);
            }

            break;
            case 4:
            {
                IrNovoTitulo("Minhas Conversas");
                IrNovoAux("-1");
                IrNovaJanela(perfilMenu);
            }

            break;
            default:
            {
                IrNovoTitulo("Menu Principal");
                IrNovoAux("");
                IrNovaJanela(janelaInicio);
            }

            break;
        }*/
        
        IrNovoTitulo("Menu Principal");
        IrNovoAux("");
        IrNovaJanela(janelaInicio);

    }

    /****************
    Função GetTitulo()

    Retorna o título da tela atual para uso em outras classes

    Resultado: 
    -   retorna o título da tela atual
    ****************/
    public string GetTitulo()
    {
        return sTitulos.Peek();
    }

    /****************
    Funçãoi GetTitulo()

    Retorna o valor auxiliar atual para uso em outras classes

    Resultado: 
    -   retorna o valor auxiliar atual
    ****************/
    public string GetAux()
    {
        return sAux.Peek();
    }
}
