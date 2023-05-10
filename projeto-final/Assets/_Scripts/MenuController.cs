using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    string textoTituloPagina = "";

    [SerializeField]
    private GameObject tituloPagina;
    [SerializeField]
    private GameObject janelaInicio;
    [SerializeField]
    private GameObject bottomMenu;
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
    [SerializeField]
    private GameObject databridgeObj;
    private DataBridge databridge;

    Stack<string> sTitulos = new Stack<string>();
    Stack<GameObject> sJanelas = new Stack<GameObject>();
    Stack<string> sAux = new Stack<string>();
    
    void Start()
    {
        databridge = databridgeObj.GetComponent<DataBridge>();
        textoTituloPagina = "Menu Principal";
        sTitulos.Push("Menu Principal");
        sJanelas.Push(janelaInicio);
        sAux.Push("");
    }
    
    void Update()
    {
        //manter título atualizado
        tituloPagina.GetComponent<Text>().text = textoTituloPagina;
        
    }
    
    //carrega valor auxiliar da nova página na pilha
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

    //carrega texto a ser exibido no título da nova página na pilha
    public void IrNovoTitulo(string novoTitulo)
    {
        textoTituloPagina = novoTitulo;
        sTitulos.Push(novoTitulo);
    }

    //descarta topo da pilha e acessa valores da página anteriormente acessada
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
            databridge.RemoveListener(sAux.Peek());
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
                databridge.RefreshComissionUsuarioList();
                break;
            case "Meu Perfil":
                databridge.CarregarPerfil(sAux.Peek());
                break;
            case "Avaliações":
                databridge.RefreshAvaliacaoList();
                break;
            case "Serviços Disponíveis":
                databridge.RefreshComissionList("Serviço");
                Debug.Log("Voltando para lista de serviços");
                break;
            case "Comissões Disponíveis":
                databridge.RefreshComissionList("Comissão");
                Debug.Log("Voltando para lista de comissões");
                break;
            case "Minhas Conversas":
                databridge.RefreshConversasList();
                break;
            default://se auxiliar começa com '-', é uma chave única de informações de contrato
                if(sAux.Peek().Length > 0 && sAux.Peek()[0].Equals("-"))
                {
                    databridge.CarregaInfoContrato(sAux.Peek(), true);
                }
                break;
        }

        //se voltou ao menu principal, zera a pilha
        if(string.Equals(sTitulos.Peek(),"Menu Principal"))
        {
            UnityMainThread.wkr.AddJob(()=>{
            topMenu.SetActive(false);
            });
            ResetNav(0);
        }
    }

    public void IrNovaJanela(GameObject novaJanela)
    {
        sJanelas.Push(novaJanela);
    }

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

    //retorna título atualmente exibido para ser usado em outras classes
    public string GetTitulo()
    {
        return sTitulos.Peek();
    }

    //retorna valor auxiliar do topo da pilha para ser usado em outras classes
    public string GetAux()
    {
        return sAux.Peek();
    }
}
