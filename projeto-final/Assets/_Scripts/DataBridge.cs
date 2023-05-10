using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Networking;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Storage;
using SimpleFileBrowser;

public class DataBridge : MonoBehaviour
{
    string path;
    private bool listenHandlerCooldown = true;
 
    //prefabs de dados de objetos a serem criados
    private Comission dataComission;
    private InfoUsuario dataPerfil;
    private Message dataMessage;
    private Contato dataContato;
    private Avaliacao dataAval;

    //listas de prefabs instanciados em listas
    private List<GameObject> listaContatosInstanciados= new List<GameObject>();
    private List<GameObject> listaComissionsUsuarioInstanciadas= new List<GameObject>();
    private List<GameObject> listaComissionsInstanciadas= new List<GameObject>();
    private List<GameObject> listaMensagensInstanciadas = new List<GameObject>();
    private List<GameObject> listaConversasInstanciadas = new List<GameObject>();
    private List<GameObject> listaAvaliacoesInstanciadas= new List<GameObject>();

    //dictionary para guardar nomes que podem ser carregados nessa sessão
    private Dictionary<string,string> dicNomesUsuarios = new Dictionary<string, string>();

    [Header("Referencias Firebase")]
    public DatabaseReference dbRef;
    public FirebaseStorage storage;
    public StorageReference storageRef;

    [Header("Outros controladores")]
    [SerializeField]
    private GameObject globalMenu;
    [SerializeField]
    private GameObject authControllerObj;
    
    [Header("Perfil")]
    [SerializeField]
    private GameObject perfilPanel;
    [SerializeField]
    private Text perfNomeUser;
    [SerializeField]
    private GameObject butPerfEditar;
    [SerializeField]
    private GameObject butPerfConversar;
    [SerializeField]
    private GameObject perfIdade;
    [SerializeField]
    private GameObject perfGenero;
    [SerializeField]
    private RawImage perfImagem;

    [Header("Editar Perfil")]
    [SerializeField]
    private InputField perfURLImagem;
    [SerializeField]
    private InputField perfEditarNome;
    [SerializeField]
    private InputField perfEditarIdade;
    [SerializeField]
    private InputField perfEditarGenero;
    [SerializeField]
    private RawImage perfEditarImagem;

    [Header("Contato")]
    [SerializeField]
    private GameObject contListaPanel;
    [SerializeField]
    private Text contButtonText;    
    [SerializeField]
    private InputField contEditarTipo;
    [SerializeField]
    private InputField contEditarValor;
    [SerializeField]
    private GameObject contNovoPainel;
    [SerializeField]
    private GameObject contatoTemplate;

    [Header("Mensagem")]
    [SerializeField]
    private GameObject msgListaPanel;
    [SerializeField]
    private GameObject conversaListaPanel;
    [SerializeField]
    private GameObject messageTemplate;
    [SerializeField]
    private GameObject conversaTemplate;
    [SerializeField]
    public InputField newMensagem;

    [Header("Avaliação")]
    [SerializeField]
    private Slider avalSlider;
    [SerializeField]
    private GameObject avalButton;
    [SerializeField]
    private Text avalDisplay;
    [SerializeField]
    private GameObject avalTemplate;

    [Header("Comissões")]
    [SerializeField]
    private GameObject comissionTemplate;
    [SerializeField]
    private GameObject comissionUsuarioTemplate;
    [SerializeField]
    private GameObject comissionListPanel;
    [SerializeField]
    private GameObject comissionListUsuarioPanel;
    [SerializeField]
    private InputField newTitulo;
    [SerializeField]
    private InputField newDescricao;
    [SerializeField]
    private Toggle newClasse;
    [SerializeField]
    private Dropdown newEstado;
    [SerializeField]
    private Dropdown newCidade;
    [SerializeField]
    private Toggle newFrequenciaUnico;
    [SerializeField]
    private Toggle newFrequenciaDom;
    [SerializeField]
    private Toggle newFrequenciaSeg;
    [SerializeField]
    private Toggle newFrequenciaTer;
    [SerializeField]
    private Toggle newFrequenciaQua;
    [SerializeField]
    private Toggle newFrequenciaQui;
    [SerializeField]
    private Toggle newFrequenciaSex;
    [SerializeField]
    private Toggle newFrequenciaSab;
    [SerializeField]
    private Toggle newTipoObra;
    [SerializeField]
    private Toggle newTipoFaxina;
    [SerializeField]
    private Toggle newTipoBaba;
    [SerializeField]
    private Toggle newTipoOutro;
    [SerializeField]
    private InputField newPreco;
    [SerializeField]
    private InputField newURLImagem;
    [SerializeField]
    private RawImage newImagem;

    [Header("Info Comissões")]
    [SerializeField]
    private GameObject infoPanel;
    [SerializeField]
    private Text infoEstado;
    [SerializeField]
    private Text infoCidade;
    [SerializeField]
    private Text infoFrequencia;
    [SerializeField]
    private Text infoTipo;
    [SerializeField]
    private Text infoPreco;
    [SerializeField]
    private Text infoClasse;
    [SerializeField]
    private Text infoDescricao;
    [SerializeField]
    private Text infoCriador;
    [SerializeField]
    private RawImage infoImagem;

    [Header("Filtros")]
    [SerializeField]
    private Dropdown filterSort;
    private bool filterClasse;
    [SerializeField]
    private Dropdown filterEstado;
    [SerializeField]
    private Dropdown filterCidade;
    [SerializeField]
    private Toggle filterFrequenciaUnico;
    [SerializeField]
    private Toggle filterFrequenciaSemanal;
    [SerializeField]
    private Toggle filterFrequenciaDom;
    [SerializeField]
    private Toggle filterFrequenciaSeg;
    [SerializeField]
    private Toggle filterFrequenciaTer;
    [SerializeField]
    private Toggle filterFrequenciaQua;
    [SerializeField]
    private Toggle filterFrequenciaQui;
    [SerializeField]
    private Toggle filterFrequenciaSex;
    [SerializeField]
    private Toggle filterFrequenciaSab;
    [SerializeField]
    private Toggle filterTipoObra;
    [SerializeField]
    private Toggle filterTipoFaxina;
    [SerializeField]
    private Toggle filterTipoBaba;
    [SerializeField]
    private Toggle filterTipoOutro;
    [SerializeField]
    private InputField filterPrecoMin;
    [SerializeField]
    private InputField filterPrecoMax;

    private void Start() {
        //prepara referencias de endereços dos arquivos do aplicativo no servidor do firebase
        dbRef=FirebaseDatabase.DefaultInstance.RootReference;
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://pau-pra-toda-obra.appspot.com/");

        //prepara dicionário 
        PrepareDicNomes();

        //prepara navegador de arquivos para exibir apenas imagens .png
        //descartado por ter problemas no android. mantido como comentario para possivel melhoria em versões futuras
        /*FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".png"));
		FileBrowser.SetDefaultFilter( ".png" );
		FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe", ".jpg", ".jpeg", ".txt", ".pdf" );
        */
    }   

    private void Update() {
        //manutenção dos radiobuttons de frequencia na tela de novo contrato
        if(newFrequenciaUnico.isOn)
        {
            newFrequenciaDom.isOn=false;
            newFrequenciaSeg.isOn=false;
            newFrequenciaTer.isOn=false;
            newFrequenciaQua.isOn=false;
            newFrequenciaQui.isOn=false;
            newFrequenciaSex.isOn=false;
            newFrequenciaSab.isOn=false;
        }
        if(!(newFrequenciaDom.isOn||newFrequenciaSeg.isOn||newFrequenciaTer.isOn||newFrequenciaQua.isOn||newFrequenciaQui.isOn||newFrequenciaSex.isOn||newFrequenciaSab.isOn))
        {
            newFrequenciaUnico.isOn = true;
        }
        //atualização do valor exibido de nova avaliação na tela de avaliações
        avalDisplay.text = avalSlider.value.ToString("F1");
    }

    //prepara dicionário com emails e nomes de usuários para uso nas demais funções sem demandar constantes acessos ao banco de dados para recuperar nomes
    public void PrepareDicNomes()
    {
        List<DataSnapshot> listaPerfis = new List<DataSnapshot>();
        //carrega dados do perfil dos usuários
        dbRef.Child("infoUsuario").GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao preparar cache de nomes de de usuarios");
            }
            else if (tarefa.IsCompleted)
            {
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){
                    listaPerfis = snapshot.Children.ToList();
                    for(int i=0; i<listaPerfis.Count;i++)
                    {
                        dicNomesUsuarios[listaPerfis[i].Child("idUsuario").GetValue(false).ToString()] = listaPerfis[i].Child("nomeUsuario").GetValue(false).ToString();
                    }
                }
                else{
                    Debug.Log("Nenhum usuário encontrado");
                }
            }
        });
    }

    //métodos para salvar dados
    //verifica se campos obrigatórios de novo contrato estão preenchidos, e salva no banco caso sim
    public void SalvarComissao()
    {
        if(newTitulo.text.Equals("")||newEstado.value == 0||newCidade.value == 0||newPreco.text.Equals("")||StringToFloat(newPreco.text)<=0)
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Alguns campos obrigatórios não foram preenchidos");
            return;
        }
        StartCoroutine(CreateComission());
    }

    //salva avaliação no banco 
    public void SalvarAvaliacao()
    {
        StartCoroutine(CreateAval(globalMenu.GetComponent<MenuController>().GetAux()));
    }
    
    //verifica se campos obrigatórios de novo perfil estão preenchidos, e salva no banco caso sim
    public void SalvarPerfil()
    {
        if(perfEditarNome.text.Equals(""))
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Nome obrigatório!");
            return;
        }
        StartCoroutine(CreateInfoUsuario(perfEditarNome.text, perfEditarIdade.text, perfEditarGenero.text));
    }

    //verifica se campos obrigatórios de novo contato estão preenchidos, e salva no banco caso sim
    public void SalvarContato()
    {
        if(contEditarTipo.text.Equals(""))
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Tipo de contato obrigatório!");
            return;
        }
        if(contEditarValor.text.Equals(""))
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Informação de contato obrigatória!");
            return;
        }
        StartCoroutine(CreateContato(contEditarTipo.text, contEditarValor.text));
    }

    //verifica se mensagem a ser enviada não está vazia, salvando no banco caso tenha conteúdo válido
    public void EnviarMensagem()
    {
        if (newMensagem.text.Equals(""))
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("A mensagem não pode ser vazia!");
            return;
        }
        StartCoroutine(CreateMensagem(globalMenu.GetComponent<MenuController>().GetAux(), newMensagem.text));
        

    }

    //métodos de atualzação de dados exibidos
    //atualiza lista de contatos do usuário idUsuario
    public void RefreshContatoList(string idUsuario)
    {
        //atualiza dicionário de nomes caso nome de algum usuário tenha sido modificado desde o inicio da sessão
        PrepareDicNomes();
        
        //se idUsuário == -2, copia idUsuario utilizado para recuperar dados da tela anterior
        if(idUsuario.Equals("-2"))
        {
            idUsuario=globalMenu.GetComponent<MenuController>().GetAux();
        }

        //remove dados instanciados na ultima chamada da função
        for (int i = 0; i<listaContatosInstanciados.Count;)
        {
            GameObject currentObj = listaContatosInstanciados[i];
            listaContatosInstanciados.Remove(currentObj);
            Destroy(currentObj);
        }

        //carrega dados do servidor
        dbRef.Child("contato/"+MakeTokensValid(idUsuario)).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar contatos. Tente novamente mais tarde.");
                globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            }
            else if (tarefa.IsCompleted){
                //verifica se encontrou dados
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){
                    //para cada contato encontrado deste usuário, instancia um elemento para exibir seus valores
                    List<DataSnapshot> listaContatos = snapshot.Children.ToList();
                    for (int i = 0; i<=listaContatos.Count;i++)
                    {
                        int j = i;
                        //copia template de contato e altera valores a serem exibidos de acordo
                        GameObject contatoInstance = Instantiate(contatoTemplate) as GameObject;
                        listaContatosInstanciados.Add(contatoInstance);
                        contatoInstance.SetActive(true);

                        contatoInstance.transform.Find("TextTipo").GetComponent<Text>().text = listaContatos[i].Child("tipoContato").GetValue(false).ToString();
                        contatoInstance.transform.Find("TextInfo").GetComponent<Text>().text = listaContatos[i].Child("valorContato").GetValue(false).ToString();

                        //perpara botão para permitir dono do contato apagá-lo de sua lista
                        contatoInstance.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(delegate { 
                            RemoveContato(MakeTokensValid(idUsuario)+"/"+listaContatos[j].Key);
                            Destroy(contatoInstance);});

                        //apenas exibe botão de remover contato caso usuário esteja acessando a própria lista de contatos
                        if(idUsuario.Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
                        {
                            contatoInstance.transform.Find("DeleteButton").gameObject.SetActive(true);
                        }
                        else{
                            contatoInstance.transform.Find("DeleteButton").gameObject.SetActive(false);
                        }

                        //garante que contato será instanciado na posição correta na hierarquia de objetos
                        contatoInstance.transform.SetParent(contatoTemplate.transform.parent,false);
                    }
                }
            }
        });
    }

    //atualiza lista de contratos dependendo se está buscando "Serviço" ou "Comissão"
    public void RefreshComissionList(string classeComissaoRefreshed)
    {
        //atualiza dicionário de nomes caso nome de algum usuário tenha sido modificado desde o inicio da sessão
        PrepareDicNomes();
        
        //se classeComissaoRefreshed == -2, copia classe de contrato utilizada na ultima chamada da função.
        if(classeComissaoRefreshed.Equals("-2"))
        {
            if (filterClasse)
            {
                classeComissaoRefreshed = "Serviço";
            }
            else{
                classeComissaoRefreshed = "Comissão";
            }
        }
        else
        {
            filterClasse = classeComissaoRefreshed.Equals("Serviço");
        }

        //remove dados instanciados na ultima chamada da função
        for (int i = 0; i<listaComissionsInstanciadas.Count;)
        {
            GameObject currentObj = listaComissionsInstanciadas[i];
            listaComissionsInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //carrega dados do servidor
        dbRef.Child("comission").GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar lista de contratos. Tente novamente mais tarde.");
                globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            }
            else if (tarefa.IsCompleted)
            {
                //verifica se lista de contratos está vazia
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren)
                {
                    List<DataSnapshot> listaComissions = snapshot.Children.ToList();

                    //preparação de comparações para filtros
                    string filterFrequenciaStr = "";
                    if (filterFrequenciaDom.isOn)
                    {
                        filterFrequenciaStr += "Dom ";
                    }
                    if (filterFrequenciaSeg.isOn)
                    {
                        filterFrequenciaStr += "Seg ";
                    }
                    if (filterFrequenciaTer.isOn)
                    {
                        filterFrequenciaStr += "Ter ";
                    }
                    if (filterFrequenciaQua.isOn)
                    {
                        filterFrequenciaStr += "Qua ";
                    }
                    if (filterFrequenciaQui.isOn)
                    {
                        filterFrequenciaStr += "Qui ";
                    }
                    if (filterFrequenciaSex.isOn)
                    {
                        filterFrequenciaStr += "Sex ";
                    }
                    if (filterFrequenciaSab.isOn)
                    {
                        filterFrequenciaStr += "Sab";
                    }
                    
                    string filterTiposStr = "";
                    if (filterTipoBaba.isOn)
                    {
                        filterTiposStr += "Babá ";
                    }
                    if (filterTipoObra.isOn)
                    {
                        filterTiposStr += "Obra ";
                    }
                    if (filterTipoFaxina.isOn)
                    {
                        filterTiposStr += "Faxina ";
                    }
                    if (filterTipoBaba.isOn)
                    {
                        filterTiposStr += "Outro ";
                    }


                    //aplicar filtros
                    for(int indexIter = 0; indexIter < listaComissions.Count;)
                    {
                        //filtra por classe de contrato (Serviço ou Comissão)
                        if (filterClasse && listaComissions[indexIter].Child("classeComissao").GetValue(false).ToString().Equals("Comissão"))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        else if (!filterClasse && listaComissions[indexIter].Child("classeComissao").GetValue(false).ToString().Equals("Serviço"))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        //filtro por cidade e estado
                        if (filterEstado.value > 0 && !(filterEstado.captionText.text == listaComissions[indexIter].Child("estado").GetValue(false).ToString()))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        if (filterEstado.value > 0 && filterCidade.value > 0 && !(filterCidade.captionText.text == listaComissions[indexIter].Child("cidade").GetValue(false).ToString()))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        //filtro por frequencia (Único ou Semanal)
                        Debug.Log("Feito!. Por frequencia:");
                        if(!filterFrequenciaUnico.isOn && listaComissions[indexIter].Child("frequencia").GetValue(false).ToString().Equals(""))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        if(!filterFrequenciaSemanal.isOn && !listaComissions[indexIter].Child("frequencia").GetValue(false).ToString().Equals(""))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        if(filterFrequenciaSemanal.isOn)
                        {
                            //Filtra contratos de frequencia Semanal que cobram/prestam serviço em dia da semana não selecionado pelo usuário
                            string[] frequenciaList = listaComissions[indexIter].Child("frequencia").GetValue(false).ToString().Split(' ');
                            bool comissionRemoved = false;
                            for(int j = 0; j < frequenciaList.Length; j++)
                            {
                                Debug.Log("Dia da semana: "+frequenciaList[j]);
                                if (!filterFrequenciaStr.Contains(frequenciaList[j]))
                                {
                                    listaComissions.Remove(listaComissions[indexIter]);
                                    comissionRemoved = true;
                                    break;
                                }
                            }
                            if(comissionRemoved)
                            {
                                continue;
                            }
                        }

                        //filtro por tipos (Faxina, Babá, Obra ou Outro)
                        if(!filterTiposStr.Contains(listaComissions[indexIter].Child("tipoContrato").GetValue(false).ToString()))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        //filtro por preço dentro do intervalo
                        if(StringToFloat(listaComissions[indexIter].Child("precoExigido").GetValue(false).ToString()) < StringToFloat(filterPrecoMin.text))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        if(!filterPrecoMax.text.Equals("") && StringToFloat(listaComissions[indexIter].Child("precoExigido").GetValue(false).ToString()) > StringToFloat(filterPrecoMax.text))
                        {
                            listaComissions.Remove(listaComissions[indexIter]);
                            continue;
                        }
                        indexIter++;
                    }
                    
                    //ordena  de acordo com opção selecionada
                    switch (filterSort.value)
                    {
                        case 1: //mais novo primeiro
                            listaComissions.Sort((p1,p2)=>
                            -1 * System.DateTime.ParseExact(p1.Child("criadoEm").GetValue(false).ToString(),"yyyy/MM/dd HH:mm:ss",null).CompareTo(
                            System.DateTime.ParseExact(p2.Child("criadoEm").GetValue(false).ToString(),"yyyy/MM/dd HH:mm:ss",null)
                            ));
                        break;
                        case 2: //maior preco
                            listaComissions.Sort((p1,p2)=> 
                            -1 * ComparePrecos(StringToFloat(p1.Child("precoExigido").GetValue(false).ToString()),
                            StringToFloat(p2.Child("precoExigido").GetValue(false).ToString()))
                            );
                        break;  
                        case 3: //menor preco
                            listaComissions.Sort((p1,p2)=> 
                            ComparePrecos(StringToFloat(p1.Child("precoExigido").GetValue(false).ToString()),
                            StringToFloat(p2.Child("precoExigido").GetValue(false).ToString()))
                            );
                        break;
                        default: //mais antigo primeiro
                            listaComissions.Sort((p1,p2)=>
                            System.DateTime.ParseExact(p1.Child("criadoEm").GetValue(false).ToString(),"yyyy/MM/dd HH:mm:ss",null).CompareTo(
                            System.DateTime.ParseExact(p2.Child("criadoEm").GetValue(false).ToString(),"yyyy/MM/dd HH:mm:ss",null)
                            ));
                        break;
                    }

                    //para cada dado que sobrou após filtros e ordenação, copia template de contratos e altera valores para exibir
                    for (int i = 0; i<=listaComissions.Count;i++)
                    {
                        int j = i;
                        GameObject comissionInstance = Instantiate(comissionTemplate) as GameObject;
                        listaComissionsInstanciadas.Add(comissionInstance);
                        
                        comissionInstance.SetActive(true);

                        comissionInstance.transform.Find("TituloComission").GetComponent<Text>().text = listaComissions[i].Child("titulo").GetValue(false).ToString();
                        comissionInstance.transform.Find("EstadoComission").GetComponent<Text>().text = listaComissions[i].Child("estado").GetValue(false).ToString();
                        comissionInstance.transform.Find("CidadeComission").GetComponent<Text>().text = listaComissions[i].Child("cidade").GetValue(false).ToString();
                        string diasSemana = listaComissions[i].Child("frequencia").GetValue(false).ToString();
                        if(diasSemana.Equals("Dom Seg Ter Qua Qui Sex Sab"))
                        {
                            comissionInstance.transform.Find("DiasComission").GetComponent<Text>().text = "A semana inteira";
                        }
                        else if (diasSemana.Equals(""))
                        {
                            comissionInstance.transform.Find("DiasComission").GetComponent<Text>().text = "Serviço único";
                        }
                        else
                        {
                            comissionInstance.transform.Find("DiasComission").GetComponent<Text>().text = diasSemana;
                        }
                        comissionInstance.transform.Find("PrecoComission").GetComponent<Text>().text = listaComissions[i].Child("precoExigido").GetValue(false).ToString();
                        comissionInstance.transform.Find("TipoComission").GetComponent<Text>().text = listaComissions[i].Child("tipoContrato").GetValue(false).ToString();
                        comissionInstance.transform.Find("ClasseComission").GetComponent<Text>().text = listaComissions[i].Child("classeComissao").GetValue(false).ToString();
                        //prepara botão de informações do contrato para carregar dados do contrato selecionado
                        comissionInstance.transform.Find("ButtonInfo").GetComponent<Button>().onClick.AddListener(delegate { 
                            CarregaInfoContrato(listaComissions[j].Key, false);
                            comissionListPanel.SetActive(false);
                            infoPanel.SetActive(true);
                        });
                        //desabilita exibição do botão deletar
                        comissionInstance.transform.GetChild(14).gameObject.SetActive(false);
                        
                        //garante que nova instancia está posicionada corretamente na hierarquia de objetos do Unity
                        comissionInstance.transform.SetParent(comissionTemplate.transform.parent,false);
                        
                        //carrega imagem salva na criação do contrato, caso tenha
                        string comissionImage = listaComissions[i].Child("imagePath").GetValue(false).ToString();
                        if(!comissionImage.Equals("") && comissionImage != null)
                        {
                            storageRef.Child(comissionImage).GetDownloadUrlAsync().ContinueWithOnMainThread(subtarefa =>
                            {
                                if(subtarefa.IsFaulted || subtarefa.IsCanceled)
                                {
                                    Debug.Log(subtarefa.Exception);
                                }
                                else{
                                    RawImage comissionImagem = comissionInstance.transform.Find("RawImage").GetComponent<RawImage>();
                                    StartCoroutine(LoadImagemUrl(subtarefa.Result.ToString(),comissionInstance.transform.Find("RawImage").GetComponent<RawImage>()));
                                }
                            });
                        }
                    }
                }
            }
        });
    }

    //atualiza lista de contratos atualmente publicados do último perfil acessado
    public void RefreshComissionUsuarioList()
    {
        //atualiza dicionário de nomes caso nome de algum usuário tenha sido modificado desde o inicio da sessão
        PrepareDicNomes();
        string idUsuario = globalMenu.GetComponent<MenuController>().GetAux();
        
        //remove dados instanciados na ultima chamada da função
        for (int i = 0; i<listaComissionsUsuarioInstanciadas.Count;)
        {
            GameObject currentObj = listaComissionsUsuarioInstanciadas[i];
            listaComissionsUsuarioInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //carrega dados do banco
        dbRef.Child("comission").GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar lista de contratos deste usuário. Tente novamente mais tarde.");
                globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            }
            else if (tarefa.IsCompleted){
                //verifica se encontrou dados
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){

                    List<DataSnapshot> listaComissions = snapshot.Children.ToList();
                    //para cada contrato encontrado, ignora os que não foram criados pelo usuário buscado
                    for (int i = 0; i<=listaComissions.Count;i++)
                    {
                        if(!idUsuario.Equals(listaComissions[i].Child("idUsuario").GetValue(false).ToString()))
                        {
                            continue;
                        }

                        //cria copia de template de contratos e altera valores para exibir dados carregados
                        int j = i;
                        GameObject comissionInstance = Instantiate(comissionUsuarioTemplate) as GameObject;
                        listaComissionsUsuarioInstanciadas.Add(comissionInstance);
                        
                        comissionInstance.SetActive(true);

                        comissionInstance.transform.Find("TituloComission").GetComponent<Text>().text = listaComissions[i].Child("titulo").GetValue(false).ToString();
                        comissionInstance.transform.Find("EstadoComission").GetComponent<Text>().text = listaComissions[i].Child("estado").GetValue(false).ToString();
                        comissionInstance.transform.Find("CidadeComission").GetComponent<Text>().text = listaComissions[i].Child("cidade").GetValue(false).ToString();
                        string diasSemana = listaComissions[i].Child("frequencia").GetValue(false).ToString();
                        if(diasSemana.Equals("Dom Seg Ter Qua Qui Sex Sab "))
                        {
                            comissionInstance.transform.Find("DiasComission").GetComponent<Text>().text = "A semana inteira";
                        }
                        else if (diasSemana.Equals(""))
                        {
                            comissionInstance.transform.Find("DiasComission").GetComponent<Text>().text = "Serviço único";
                        }
                        else
                        {
                            comissionInstance.transform.Find("DiasComission").GetComponent<Text>().text = diasSemana;
                        }
                        comissionInstance.transform.Find("PrecoComission").GetComponent<Text>().text = listaComissions[i].Child("precoExigido").GetValue(false).ToString();
                        comissionInstance.transform.Find("TipoComission").GetComponent<Text>().text = listaComissions[i].Child("tipoContrato").GetValue(false).ToString();
                        comissionInstance.transform.Find("ClasseComission").GetComponent<Text>().text = listaComissions[i].Child("classeComissao").GetValue(false).ToString();
                        //prepara botão para deletar contrato
                        comissionInstance.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(delegate { 
                            RemoveComission(listaComissions[j].Key);
                            Destroy(comissionInstance);
                        });
                        //prepara botão para exibir mais informações de contrato
                        comissionInstance.transform.Find("ButtonInfo").GetComponent<Button>().onClick.AddListener(delegate { 
                            CarregaInfoContrato(listaComissions[j].Key, false);
                            comissionListUsuarioPanel.SetActive(false);
                            infoPanel.SetActive(true);
                        });

                        //só exibe botão de deletar contrato caso esteja olhando a própria lista de contratos
                        if(idUsuario.Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
                        {
                            comissionInstance.transform.GetChild(14).gameObject.SetActive(true);
                        }
                        else{
                            comissionInstance.transform.GetChild(14).gameObject.SetActive(false);
                        }

                        //garante que contrato instanciado está posicionado corretamente na hierarquia de objetos do Unity
                        comissionInstance.transform.SetParent(comissionUsuarioTemplate.transform.parent,false);
                        
                        //carrega imagem de contrat
                        string comissionImage = listaComissions[i].Child("imagePath").GetValue(false).ToString();
                        if(!comissionImage.Equals("") && comissionImage != null)
                        {

                            storageRef.Child(comissionImage).GetDownloadUrlAsync().ContinueWithOnMainThread(subtarefa =>
                            {
                                if(subtarefa.IsFaulted || subtarefa.IsCanceled)
                                {
                                    Debug.Log(subtarefa.Exception);
                                }
                                else{
                                    RawImage comissionImagem = comissionInstance.transform.Find("RawImage").GetComponent<RawImage>();
                                    StartCoroutine(LoadImagemUrl(subtarefa.Result.ToString(),comissionInstance.transform.Find("RawImage").GetComponent<RawImage>()));
                                }
                            });
                        }
                    }
                }
            }
        });
    }

    //atualiza lista de avaliações do usuáro do último perfil acessado
    public void RefreshAvaliacaoList()
    {
        //atualiza dicionário de nomes, caso algum usuário tenha alterado seu nome desde o início da sessão
        PrepareDicNomes();

        //se estiver olhando a própria lista de avaliações, esconde a opção de publicar sua avaliação. exibe caso contrário
        string idUsuario = globalMenu.GetComponent<MenuController>().GetAux();
        if (idUsuario.Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
        {
            avalButton.SetActive(false);
        }
        else{
            avalButton.SetActive(true);
        }
        
        //limpa dados instanciados na última execução desta função
        for (int i = 0; i<listaAvaliacoesInstanciadas.Count;)
        {
            GameObject currentObj = listaAvaliacoesInstanciadas[i];
            listaAvaliacoesInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //carrega dados do banco
        dbRef.Child("avaliacao/"+MakeTokensValid(idUsuario)).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar avaliações. Tente novamente mais tarde");
                globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            }
            else if (tarefa.IsCompleted){
                //verifica se tem dados
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){
                    List<DataSnapshot> listaAvaliacoes = snapshot.Children.ToList();
                    //para cada avaliação, instancia um painel com nome do avaliador e número de estrelas igual ao valor da avaliação
                    for (int i = 0; i<=listaAvaliacoes.Count;i++)
                    {
                        GameObject avaliacaoInstance = Instantiate(avalTemplate) as GameObject;
                        listaAvaliacoesInstanciadas.Add(avaliacaoInstance);
                        avaliacaoInstance.SetActive(true);

                        avaliacaoInstance.transform.Find("Autor").GetComponent<Text>().text = dicNomesUsuarios[listaAvaliacoes[i].Child("idUsuario").GetValue(false).ToString()];
                        avaliacaoInstance.transform.Find("Stars").GetComponent<RectTransform>().sizeDelta = new Vector2((float)61.2 * StringToFloat(listaAvaliacoes[i].Child("valorAval").GetValue(false).ToString()),avaliacaoInstance.transform.Find("Stars").GetComponent<RectTransform>().sizeDelta.y);
                        
                        avaliacaoInstance.transform.SetParent(avalTemplate.transform.parent,false);

                    }
                }
            }
        });
    }

    //atualiza lista de mensagens trocadas entre o usuário atual e o usuário destinatarioId
    public void RefreshMensagemList(string destinatarioId)
    {
        //atualiza dicionário de nomes, caso algum usuário tenha mudado de nome durante a sessão atual
        PrepareDicNomes();

        //se destinatárioId for -2, carrega id de último usuário acessado e empilha no MenuController os valores adequados para preparar a tela de troca de mensagem 
        if(destinatarioId.Equals("-2"))
        {
            destinatarioId = globalMenu.GetComponent<MenuController>().GetAux();
            
            globalMenu.GetComponent<MenuController>().IrNovoAux(globalMenu.GetComponent<MenuController>().GetAux());
            globalMenu.GetComponent<MenuController>().IrNovoTitulo("Conversa com "+dicNomesUsuarios[destinatarioId]);
            globalMenu.GetComponent<MenuController>().IrNovaJanela(msgListaPanel);

        }

        //remove mensagens instanciadas na última execução desta função
        for (int i = 0; i<listaMensagensInstanciadas.Count;)
        {
            GameObject currentObj = listaMensagensInstanciadas[i];
            listaMensagensInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //carrega dados do servidor
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), destinatarioId))).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                Debug.Log("Erro ao ler lista messages");
            }
            else if (tarefa.IsCompleted){
                //verifica se há dados
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){
                    List<DataSnapshot> listaMessagesConversa = snapshot.Children.ToList();

                    //ordena lista de mensagens por ordem de criação
                    listaMessagesConversa.Sort((p1,p2)=>
                    System.DateTime.ParseExact(p1.Child("criadoEm").GetValue(false).ToString(),"yyyy/MM/dd HH:mm:ss",null).CompareTo(
                    System.DateTime.ParseExact(p2.Child("criadoEm").GetValue(false).ToString(),"yyyy/MM/dd HH:mm:ss",null)
                    ));

                    //para cada mensagem carregada, cria uma instancia e modifica 
                    for (int i = 0; i<listaMessagesConversa.Count;i++)
                    {
                        GameObject messageInstance = Instantiate(messageTemplate) as GameObject;
                        listaMensagensInstanciadas.Add(messageInstance);
                        messageInstance.SetActive(true);

                        messageInstance.transform.GetChild(0).GetComponent<Text>().text = listaMessagesConversa[i].Child("remetente").GetValue(false).ToString();
                        messageInstance.transform.GetChild(1).GetComponent<Text>().text = listaMessagesConversa[i].Child("conteudo").GetValue(false).ToString();
                        
                        //garante que instancia de mensagem está posicionada corretamente na hierarquia de objetos do Unity
                        messageInstance.transform.SetParent(messageTemplate.transform.parent,false);
                    }
                }
            }
        });
    }

    //atualiza lista de usuários com quem o usuário atual já começou uma conversa, recebendo ou enviando qualquer mensagem
    public void RefreshConversasList()
    {
        //atualiza dicionário de nomes de usuários, para caso algum usuário tenha modificado seu nome durante a sessão atual
        PrepareDicNomes();

        //remove instancias criadas na última execução desta função
        for (int i = 0; i<listaConversasInstanciadas.Count;)
        {
            GameObject currentObj = listaConversasInstanciadas[i];
            listaConversasInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //recebe dados do banco
        dbRef.Child("infoUsuario/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())+"/conversas").GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar lista conversas. Tente novamente mais tarde");
                globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            }
            else if (tarefa.IsCompleted){
                //verifica se há dados
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){
                    List<DataSnapshot> listaConversa = snapshot.Children.ToList();

                    //para cada dado recebido, instancia um botão com nome de um usuário com quem o usuário atual já começou conversa
                    for (int i = 0; i<listaConversa.Count;i++)
                    {
                        int j = i;
                        GameObject conversaInstance = Instantiate(conversaTemplate) as GameObject;
                        listaConversasInstanciadas.Add(conversaInstance);
                        conversaInstance.SetActive(true);

                        conversaInstance.transform.GetChild(1).GetComponent<Text>().text = dicNomesUsuarios[listaConversa[i].GetValue(false).ToString()];
                        //botão instanciado leva para tela de conversa com usuário 
                        conversaInstance.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { 
                            IrMensagemList(listaConversa[j].GetValue(false).ToString());
                            msgListaPanel.SetActive(true);
                            conversaListaPanel.SetActive(false);
                        });
                        
                        //garante que instancia criada está posicionada corretamente na hierarquia de objetos do Unity
                        conversaInstance.transform.SetParent(conversaTemplate.transform.parent,false);
                    }
                }
            }
        });
    }
    
    //carrega dados do contrato keyContrato
    public void CarregaInfoContrato(string keyContrato, bool isRetorno)
    {
        //atualiza dicionário de nomes, caso algum usuário tenha atualizado seu nome durante a sessão atual
        PrepareDicNomes();

        //empilha valor auxiliar e objeto da tela atual. titulo é empilhado posteriormente pois é o titulo do contrato buscado
        if(!isRetorno)
        {
            globalMenu.GetComponent<MenuController>().IrNovoAux(keyContrato);
            globalMenu.GetComponent<MenuController>().IrNovaJanela(infoPanel);
        }
        //carrega dados do banco
        dbRef.Child("comission/"+keyContrato).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar dados do contrato. Tente novamente mais tarde");
                if(!isRetorno)
                {   //empilha titulo placeholder antes de sair da tela para manter sincronia das pilha de navegação
                    globalMenu.GetComponent<MenuController>().IrNovoTitulo("");
                }
                globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            }
            else if (tarefa.IsCompleted)
            {
                //verifica se encontrou dados
                DataSnapshot snapshot = tarefa.Result;
                if (snapshot.Exists && snapshot.HasChildren){
                    //carrega imagem do contrato
                    string contratoImage = snapshot.Child("imagePath").GetValue(false).ToString();
                    if(!contratoImage.Equals("") && contratoImage != null)
                    {

                        storageRef.Child(contratoImage).GetDownloadUrlAsync().ContinueWithOnMainThread(tarefa =>
                        {
                            if(tarefa.IsFaulted || tarefa.IsCanceled)
                            {
                                Debug.Log(tarefa.Exception);
                            }
                            else{
                                StartCoroutine(LoadImagemUrl(tarefa.Result.ToString(),infoImagem));
                            }
                        });

                    }

                    //para poder exibir o titulo do contrato como titulo da janela, este só é empilhado após o carregamento dos dados
                    if(!isRetorno)
                    {
                        globalMenu.GetComponent<MenuController>().IrNovoTitulo(snapshot.Child("titulo").GetValue(false).ToString());
                    }

                    //substitui texto dos campos correspondentes pelo valor do contrato sendo acessado
                    infoEstado.GetComponent<Text>().text=snapshot.Child("estado").GetValue(false).ToString();
                    infoCidade.GetComponent<Text>().text=snapshot.Child("cidade").GetValue(false).ToString();
                    string diasSemana = snapshot.Child("frequencia").GetValue(false).ToString();
                    if(diasSemana.Equals("Dom Seg Ter Qua Qui Sex Sab"))
                        {
                            infoFrequencia.GetComponent<Text>().text = "A semana inteira";
                        }
                        else if (diasSemana.Equals(""))
                        {
                            infoFrequencia.GetComponent<Text>().text = "Serviço único";
                        }
                        else
                        {
                            infoFrequencia.GetComponent<Text>().text = diasSemana;
                        }
                    infoPreco.GetComponent<Text>().text=snapshot.Child("precoExigido").GetValue(false).ToString();
                    infoTipo.GetComponent<Text>().text=snapshot.Child("tipoContrato").GetValue(false).ToString();
                    infoClasse.GetComponent<Text>().text=snapshot.Child("classeComissao").GetValue(false).ToString();
                    infoDescricao.GetComponent<Text>().text=snapshot.Child("descricao").GetValue(false).ToString();
                    infoCriador.GetComponent<Text>().text="Publicado por: "+dicNomesUsuarios[snapshot.Child("idUsuario").GetValue(false).ToString()];

                    //prepara botão para ir para conversa com criador da publicação
                    infoPanel.transform.GetChild(0).transform.GetChild(0).transform.Find("ButtonMensagem").GetComponent<Button>().onClick.AddListener(delegate { 
                        IrConversaContrato(snapshot.Child("idUsuario").GetValue(false).ToString());
                    });
                    //prepara botão para ir para perfil do criador da publicação
                    infoPanel.transform.GetChild(0).transform.GetChild(0).transform.Find("ButtonPerfil").GetComponent<Button>().onClick.AddListener(delegate { 
                        IrPerfilContrato(snapshot.Child("idUsuario").GetValue(false).ToString());
                    });
                }
            }
        });
    }

    //carrega dados do perfil de idUsuario
    public void CarregarPerfil(string idUsuario)
    {
        //se idUsuario = -1, carrega dados do perfil do usuário atual e registra na pilha de navegação
        if (idUsuario.Equals("-1"))
        {
            idUsuario = authControllerObj.GetComponent<AuthController>().GetCurrentUserId();
            globalMenu.GetComponent<MenuController>().IrNovoTitulo("Meu Perfil");
            globalMenu.GetComponent<MenuController>().IrNovoAux(idUsuario);
        }

        //busca perfil do usuario no sistema
        dbRef.Child("infoUsuario").Child(MakeTokensValid(idUsuario)).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar o perfil deste usuário. Tente novamente mais tarde");

            }
            else if (tarefa.IsCompleted)
            {
                DataSnapshot snapshot = tarefa.Result;
                //verifica se encontrou dados

                if (snapshot.Exists && snapshot.HasChildren){
                    
                    //carrega imagem associada ao perfil
                    string perfilImage = snapshot.Child("imagePath").GetValue(false).ToString();
                    if(!perfilImage.Equals("") && perfilImage != null)
                    {
                        storageRef.Child(perfilImage).GetDownloadUrlAsync().ContinueWithOnMainThread(tarefa =>
                        {
                            if(tarefa.IsFaulted || tarefa.IsCanceled)
                            {
                                Debug.Log(tarefa.Exception);
                            }
                            else{
                                StartCoroutine(LoadImagemUrl(tarefa.Result.ToString(),perfImagem));
                                StartCoroutine(LoadImagemUrl(tarefa.Result.ToString(),perfEditarImagem));
                            }
                        });

                    }
                    //set outros dados do usuario
                    perfNomeUser.GetComponent<Text>().text=snapshot.Child("nomeUsuario").GetValue(false).ToString();
                    perfGenero.GetComponent<Text>().text="Gênero: "+snapshot.Child("generoUsuario").GetValue(false).ToString();
                    perfIdade.GetComponent<Text>().text="Idade: "+snapshot.Child("idadeUsuario").GetValue(false).ToString()+" anos";
                    perfEditarNome.text = snapshot.Child("nomeUsuario").GetValue(false).ToString(); 
                    perfEditarIdade.text = snapshot.Child("idadeUsuario").GetValue(false).ToString();
                    perfEditarGenero.text = snapshot.Child("generoUsuario").GetValue(false).ToString();

                    if(globalMenu.GetComponent<MenuController>().GetTitulo().Equals("Meu Perfil"))
                    {
                        //perfil deste usuario. habilita botao para editar
                        butPerfEditar.SetActive(true);
                        butPerfConversar.SetActive(false);
                    }
                    else{
                        //perfil de outro usuario. habilita botão para ir para conversa
                        butPerfEditar.SetActive(false);
                        butPerfConversar.SetActive(true);
                    }
                }
            }
        });

    }

    //corrotinas
    //espera 0.1s entre tentativas de atualizar lista de mensagens atraves do listener
    IEnumerator CooldownHandler()
    {
        yield return new WaitForSeconds((float)0.1);
        listenHandlerCooldown = true;
    }

    //abre caixa de dialogo para navegar nos arquivos do dispositivo e escolher imagem. carrega imagem escolhida como textura de targetRawImage
    /*IEnumerator ShowLoadDialogCoroutine(RawImage targetRawImage)
    {//função não usada pois estava dando problemas com android. mantida no comentário para possiveis melhorias em versões futuras do app
        yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.FilesAndFolders, false, null, null, "Escolha uma imagem para inserir", "Selecionar" );
        
        if( FileBrowser.Success )
		{
            //salva caminho do arquivo selecionado.
            path = FileBrowser.Result[0];

            //carrega imagem selecionada no display de exemplo
            Texture2D placeholderTexture = new Texture2D(200,200);
            placeholderTexture.LoadImage(FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]));
            targetRawImage.texture = placeholderTexture as Texture;

		}
        else{
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("A seleção de imagem foi cancelada.");
        }
    }*/

    //carrega imagem armazenada no servidor e insere como textura na imagem alvo targetImage
    IEnumerator LoadImagemUrl (string mediaUrl, RawImage targetImage)
    {
        //carrega imagem do endereço web
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar imagem: "+request.error);
        }
        else
        {
            //passa imagem carregada como textura para imagem alvo
            Texture myTexture =((DownloadHandlerTexture)request.downloadHandler).texture;
            targetImage.texture = myTexture;
        }
    }

    //salva contrato e sua imagem associada no banco
    IEnumerator CreateComission()
    {
        //salva imagem
        string newImagePath = "";
        if (path != "" && path != null && path.Substring(path.Length-4).Equals(".png"))
        {
            //usa horário atual com 4 casas decimais de precisão como nome efetivamente único para o arquivo da imagem, e tenta enviar para o servidor
            newImagePath = DateTime.Now.ToString("yyyyMMddHHmmssffff")+".png";
            storageRef.Child(newImagePath).PutBytesAsync(((Texture2D)newImagem.texture).EncodeToPNG()).ContinueWithOnMainThread(tarefa => {
                if(tarefa.IsFaulted || tarefa.IsCanceled){
                    //se falhar a publicar a imagem, salva contrato sem imagem
                    authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha no upload da imagem. Tente novamente mais tarde. Contrato será registrado sem imagem.");
                    newImagePath = "";
                }
            });
        }
        else 
        {
            //path vazio ou não png, indicando que usuário não carregou nenhuma imagem png na tela atual
            newImagePath = "";
        }

        //prepara string listando dias da semana selecionados na tela de criação. se nenhum selecionado a frequencia é único
        string thisFrequencia = "";
        if (newFrequenciaDom.isOn==true)
        {
            thisFrequencia += "Dom ";
        }
        if (newFrequenciaSeg.isOn==true)
        {
            thisFrequencia += "Seg ";
        }
        if (newFrequenciaTer.isOn==true)
        {
            thisFrequencia += "Ter ";
        }
        if (newFrequenciaQua.isOn)
        {
            thisFrequencia += "Qua ";
        }
        if (newFrequenciaQui.isOn)
        {
            thisFrequencia += "Qui ";
        }
        if (newFrequenciaSex.isOn)
        {
            thisFrequencia += "Sex ";
        }
        if (newFrequenciaSab.isOn)
        {
            thisFrequencia += "Sab";
        }

        //prepara string com tipo de contrato selecionado. se  de alguma forma nenhum está selecionado, o tipo é outro
        string thisTipo = "";
        if(newTipoObra.isOn){
            thisTipo= "Obra";
        }
        else if(newTipoFaxina.isOn){
            thisTipo= "Faxina";
        }
        else if(newTipoBaba.isOn){
            thisTipo= "Babá";
        } else{
            thisTipo= "Outro";
        }

        //registra se o contrato sendo publicado é um Serviço ou uma Comissão
        string thisClasse= "";
        if (newClasse.isOn){
            thisClasse = "Serviço";
        }
        else{
            thisClasse = "Comissão";
        }

        //cria objeto Comission com dados definidos na tela, transforma em dicionário e publica para caminho adequado no servidor firebase
        dataComission = new Comission(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), newTitulo.text, newDescricao.text, newImagePath, newEstado.captionText.text, newCidade.captionText.text, thisFrequencia, thisTipo, newPreco.text, thisClasse);
        Dictionary<string,System.Object> dicComissao = dataComission.ToDictionary();
        var TarefaDB = dbRef.Child("comission").Push().SetValueAsync(dicComissao);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar novo contrato: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //contrato publicado com sucesso. volta a lista de contratos anterior
            RefreshComissionList("-2");
            ClearImageField();

        }
    }

    //salva mensagem trocada entre usuários e registra o início da conversa da dupla caso ainda não tenha sido iniciada
    IEnumerator CreateMensagem(string destino, string conteudo)
    {
        //cria objeto Message com valores inseridos e converte para dicionário
        dataMessage = new Message(dicNomesUsuarios[authControllerObj.GetComponent<AuthController>().GetCurrentUserId()],dicNomesUsuarios[destino],conteudo);
        Dictionary<string,System.Object> dicMessage = dataMessage.ToDictionary();

        //registra inicio de conversa para usuário atual
        var TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())+"/conversas/"+MakeTokensValid(destino)).SetValueAsync(destino);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        //registra inicio de conversa para usuário recebendo a mensagem
        TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(destino)+"/conversas/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).SetValueAsync(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        //registra mensagem no servidor firebase na lista de mensagens trocadas na conversa destes dois usuários
        TarefaDB = dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), destino))).Push().SetValueAsync(dicMessage);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar nova mensagem: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //sucesso publicando nova mensagem. atualiza lista de exibição
            RefreshMensagemList(globalMenu.GetComponent<MenuController>().GetAux());
        }

        //limpa caixa de texto de mensagem
        newMensagem.text = "";
    }

    //salva perfil de usuário e sua imagem associada no banco
    IEnumerator CreateInfoUsuario(string newPerfNome, string newPerfIdade, string newPerfGenero)
    {
        //salva imagem
        string newImagePath = "";
        if (path != "" && path != null && path.Substring(path.Length-4).Equals(".png"))
        {
            //usa horário atual com 4 casas decimais de precisão como nome efetivamente único para o arquivo da imagem, e tenta enviar para o servidor
            newImagePath = DateTime.Now.ToString("yyyyMMddHHmmssffff")+".png";
            storageRef.Child(newImagePath).PutBytesAsync(((Texture2D)perfEditarImagem.texture).EncodeToPNG()).ContinueWithOnMainThread(tarefa => {
                if(tarefa.IsFaulted || tarefa.IsCanceled){
                    //se falhar a publicar a imagem, salva perfil sem imagem
                    authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha no upload da imagem. Tente novamente mais tarde. Perfil será registrado sem imagem.");
                    newImagePath = "";
                }
            });
        }
        else 
        {
            newImagePath = "";
        }
        //cria novo objeto InfoUsuario, converte para dicionário e publica no caminho adequado no servidor firebase
        dataPerfil = new InfoUsuario(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(),newPerfNome,newPerfGenero,newPerfIdade,newImagePath);
        Dictionary<string,System.Object> dicPerfil = dataPerfil.ToDictionary();
        var TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).SetValueAsync(dicPerfil);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar perfil: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //perfil salvo com sucesso. atualiza tela de perfil e dicionário de nomes, e limpando path da image para uploads futuros
            CarregarPerfil(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
            ClearImageField();
            globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            PrepareDicNomes();
        }
    }
    
    //salva contato no banco
    IEnumerator CreateContato(string newContatoTipo, string newContatoValor)
    {
        //cria objeto Contato com valores inseridos, converte em dicionário e publica no caminho adequado no servidor firebase
        dataContato = new Contato(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(),newContatoTipo,newContatoValor);
        Dictionary<string,System.Object> dicContato = dataContato.ToDictionary();
        var TarefaDB = dbRef.Child("contato/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).Push().SetValueAsync(dicContato);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar novo contato: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //contato salvo com suceso. atualiza lista sendo exibida
            RefreshContatoList(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
        }
    }

    //salva avaliação do usuário atual sobre usuário avaliadoId no banco
    IEnumerator CreateAval(string avaliadoId)
    {
        //cria objeto Avaliacao com valor inserido e usuários associados, converte para dicionário e salva no local adequado no servidor 
        dataAval = new Avaliacao(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(),avalSlider.value.ToString("F1"));
        Dictionary<string,System.Object> dicAvaliacao = dataAval.ToDictionary();
        var TarefaDB = dbRef.Child("avaliacao/"+MakeTokensValid(avaliadoId)+"/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).SetValueAsync(dicAvaliacao);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);
        if (TarefaDB.Exception != null) 
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar nova avaliação: "+(TarefaDB.Exception)+" Tente novamente mais tarde");
        }
        else{
            //avaliação registrada com sucesso. atualiza lista de exibição
            RefreshAvaliacaoList();
        }
    }

    //métodos utilitários
    //adapta botão da página de contatos para exibir texto correto dependendo se a lista pertence ao usuário atual ou a outro usuário
    public void SetContatoButton()
    {
        if(globalMenu.GetComponent<MenuController>().GetAux().Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
        {
            contButtonText.text = "Adicionar forma de contato";
        }
        else{
            contButtonText.text = "Ir para conversa";
        }
    }

    //habilita adição de novo contato, se na própria lista de contatos, ou vai para conversa, se na lista de outro usuário
    public void ContatoButtonHandler()
    {
        if(globalMenu.GetComponent<MenuController>().GetAux().Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
        {
            //abrir painel de criar contato
            contNovoPainel.SetActive(true);
        }
        else{
            //registra navegação na pila
            globalMenu.GetComponent<MenuController>().IrNovoAux(globalMenu.GetComponent<MenuController>().GetAux());
            globalMenu.GetComponent<MenuController>().IrNovoTitulo("Conversa com "+dicNomesUsuarios[globalMenu.GetComponent<MenuController>().GetAux()]);
            globalMenu.GetComponent<MenuController>().IrNovaJanela(msgListaPanel);
            //sai da janela de contatos janela contatos
            contListaPanel.SetActive(false);
            //vai para janela de conversa
            msgListaPanel.SetActive(true);
        }
    }

    //atualiza checkboxes da janela de filtros, desativando caixas de dias da semana se a opção for desselecionada e reabilitando quando marcada novamente
    public void ReactivateFilterHandler()
    {
        filterFrequenciaDom.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaDom.interactable = filterFrequenciaSemanal.isOn;
        filterFrequenciaSeg.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaSeg.interactable = filterFrequenciaSemanal.isOn;
        filterFrequenciaTer.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaTer.interactable = filterFrequenciaSemanal.isOn;
        filterFrequenciaQua.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaQua.interactable = filterFrequenciaSemanal.isOn;
        filterFrequenciaQui.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaQui.interactable = filterFrequenciaSemanal.isOn;
        filterFrequenciaSex.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaSex.interactable = filterFrequenciaSemanal.isOn;
        filterFrequenciaSab.isOn=filterFrequenciaSemanal.isOn;
        filterFrequenciaSab.interactable = filterFrequenciaSemanal.isOn;
    }

    //listener espera por atualizações na lista de mensagens enquanto usuário está nela
    public void ConversaListenHandler(object o, ChildChangedEventArgs args)
    {
        //aguarda 0.1s entre atualizações para evitar acessos excessivos ao banco
        if (listenHandlerCooldown)
        {
            RefreshMensagemList(globalMenu.GetComponent<MenuController>().GetAux());
            Debug.Log("Nova mensagem. Refresh conversa com "+globalMenu.GetComponent<MenuController>().GetAux());
            listenHandlerCooldown = false;
            StartCoroutine(CooldownHandler());
        }
    }
    //abre explorador de arquivos para selecionar imagem a ser salva no sistema
    /*public void OpenFileExplorer(RawImage targetRawImage)
    {//função não usada pois estava dando problemas com android. mantida no comentário para possiveis melhorias em versões futuras do app
        StartCoroutine(ShowLoadDialogCoroutine(targetRawImage));
        return;
    }*/

    //carrega imagem da url inserida na caixa de texto da página idpagina e insere conteudo na rawimage da pagina. idpagina 1 = perfil; 2= contrato
    public void SalvarImagemHandler(int idPagina)
    {
        switch (idPagina)
        {
            case 1:
                if(perfURLImagem.text.Substring(perfURLImagem.text.Length-4).Equals(".png"))
                {
                    path = perfURLImagem.text;
                    StartCoroutine(LoadImagemUrl(perfURLImagem.text,perfEditarImagem));
                }
                else{
                    authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("A imagem deve ser um arquivo .png");
                }
                break;
            case 2:
                if(newURLImagem.text.Substring(newURLImagem.text.Length-4).Equals(".png"))
                {
                    path = newURLImagem.text;
                    StartCoroutine(LoadImagemUrl(newURLImagem.text,newImagem));
                }
                else{
                    authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("A imagem deve ser um arquivo .png");
                }
                break;
            default:
                authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Opa, você não devia ter acesso ao upload de imagem nessa página.");
            break;
        }
    }

    //limpa path para evitar sujeira nas publicações de arquivos
    public void ClearImageField()
    {
        path = "";
    }
    
    //limpa filtros de exibição de contrato para exibir todos. mantem apenas classe como "Comissão" ou "Serviço" pois telas são separadas
    public void ClearFiltros(string filterClasseContrato)
    {
        filterCidade.value = 0;
        filterCidade.RefreshShownValue();
        filterEstado.value = 0;
        filterEstado.RefreshShownValue();
        filterFrequenciaUnico.isOn = true;
        filterFrequenciaDom.isOn = true;
        filterFrequenciaSeg.isOn = true;
        filterFrequenciaTer.isOn = true;
        filterFrequenciaQua.isOn = true;
        filterFrequenciaQui.isOn = true;
        filterFrequenciaSex.isOn = true;
        filterFrequenciaSab.isOn = true;
        filterPrecoMax.text = "";
        filterPrecoMin.text = "";
        filterTipoBaba.isOn = true;
        filterTipoObra.isOn = true;
        filterTipoFaxina.isOn = true;
        filterTipoOutro.isOn = true;
        RefreshComissionList(filterClasseContrato);
    }

    //converte uma string para um float equivalente, usado nas avaliações e na comparação de preços
    private float StringToFloat(string inputString)
    {
        if(inputString.Equals(""))
        {
            return 0;
        }
        float result = float.Parse(inputString,System.Globalization.CultureInfo.InvariantCulture);
        return result/10;
    }

    //compara preços p1 e p2, retornando 0 se iguais, 
    private int ComparePrecos(float p1, float p2)
    {
        if (p1 == p2)
        {
        return 0;
        }
        if(p1 > p2)
        {
            return 1;
        }
        return -1;
    }

    //verifica se preço mínimo inserido no filtro é maior que máximo ou se algum é menor que 0, tratando como erro e resetando valores
    public void ValidarFiltrosPrecos()
    {
        if((filterPrecoMin.text!="" && StringToFloat(filterPrecoMin.text)<=0) || (filterPrecoMax.text != "" && StringToFloat(filterPrecoMax.text)<=0) || (filterPrecoMin.text!="" && filterPrecoMax.text!="" &&StringToFloat(filterPrecoMin.text)>StringToFloat(filterPrecoMax.text)))
        {
            authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("O preço mínimo deve ser menor que o máximo, e ambos devem ser maiores que 0");
            filterPrecoMin.text = "";
            filterPrecoMax.text = "";
        }
    }

    //converte caracter '.', inválido como endereço no firebase, em '"', que é válido para firebase e pode ser revertido por ser inválido em email
    private string MakeTokensValid(string inputIdString)
    {
        string validTokensId = "";
        for(int i=0; i<inputIdString.Length;i++)
        {
            if(inputIdString[i] == '.')
            {
                validTokensId += '"';
            }
            else validTokensId += inputIdString[i];
        }
        return validTokensId;
    }

    //retorna string "input1 input2" onde input1 é o primeiro input em ordem alfabética entre inputA e inputB, e input2 é o restante
    private string SortStringsAlpha(string inputA, string inputB)
    {
        List<string> result = new List<string> {inputA,inputB};
        result.Sort();
        return result[0]+" "+result[1];
    }

    //navega para janela de conversa a partir da janela de contratos
    public void IrConversaContrato(string idUsuario)
    {
        //registra navegação na pilha para permitir voltar depois
        MenuController menu = globalMenu.GetComponent<MenuController>();
        menu.IrNovaJanela(msgListaPanel);
        menu.IrNovoAux(idUsuario);
        menu.IrNovoTitulo("Conversa com "+dicNomesUsuarios[idUsuario]);
        //carrega mensagens trocadas entre este usuário e o outro
        RefreshMensagemList(idUsuario);
        //habilita listener para atualizar lista ao receber nova mensagem
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), idUsuario))).ChildAdded += ConversaListenHandler;
    }

    //navega para janela de perfil do usuário a partir da janela de informações do contrato
    public void IrPerfilContrato(string idUsuario)
    {
        //registra navegação na pilha para permitir voltar depois
        MenuController menu = globalMenu.GetComponent<MenuController>();
        menu.IrNovaJanela(perfilPanel);
        menu.IrNovoAux(idUsuario);
        menu.IrNovoTitulo("Perfil de "+dicNomesUsuarios[idUsuario]);
        //carrega informações do perfil
        CarregarPerfil(idUsuario);
    }

    //navega para janela de conversa a partir da janela de contratos
    public void IrMensagemList(string idUsuario)
    {
        //registra navegação na pilha para permitir voltar depois
        MenuController menu = globalMenu.GetComponent<MenuController>();
        menu.IrNovaJanela(msgListaPanel);
        menu.IrNovoAux(idUsuario);
        menu.IrNovoTitulo("Conversa com "+dicNomesUsuarios[idUsuario]);
        //carrega lista de mensagens trocadas entre usuários
        RefreshMensagemList(idUsuario);
        //habilita listener para atualizar lista ao receber nova mensagem
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), idUsuario))).ChildAdded += ConversaListenHandler;
    }

    //exclusão de dados
    //exclui contrato
    public void RemoveComission(string comissionKey)
    {
        dbRef.Child("comission/"+comissionKey).RemoveValueAsync();
    }

    //exclui contato
    public void RemoveContato(string contatoKey)
    {
        dbRef.Child("contato/"+contatoKey).RemoveValueAsync();
        RefreshContatoList(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
    }

    //deshabilita listener, parando de atualizar lista de mensagens até abrir a janela novamente
    public void RemoveListener(string idUsuario)
    {
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), idUsuario))).ChildAdded -= ConversaListenHandler;
        
    }
}
