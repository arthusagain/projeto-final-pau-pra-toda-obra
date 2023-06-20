using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Storage;

public class DataBridge : MonoBehaviour
{
    /****************
    Clase DataBridge

    Singleton responsável por:
    -   Ler, escrever, filtrar e remover dados do banco de dados
    -   Atualizar telas com informações recebidas do banco
    -   Adaptar entrada recebida do usuário para formato aceito pelo Firebase  
    
    Campos:
    -   dataBridgeInstance: instancia deste singleton
    * Variáveis internas para realizar operações das funções:  
    -   path: armazena temporariamente o endereço da imagem relevante para a tela atual, seja URL da imagem sendo publicada ou caminho para a imagem armazenada no banco de dados do Firebase
    -   listenHandlerCooldown: booleano que sinaliza quando o sistema deve tentar atualizar mensagens de uma conversa, atualizado a cada alguns segundos
    * Variáveis que armazenam dados dos objetos a serem publicados no banco
    -   dataComission: dados sobre contrato
    -   dataPerfil: dados sobre Perfil
    -   ataMessage: dados sobre Message
    -   dataContato: dados sobre Contato
    -   dataAval: dados sobre Avaliacao
    * Listas de paineis atualmente instanciados na tela
    -   listaContatosInstanciados: lista dos contatos instanciados, na tela de contatos
    -   listaComissionsUsuarioInstanciadas: lista dos contratos do usuário instanciadas, na tela de contratos do usuário X
    -   listaComissionsInstanciadas: lista dos contratos instanciados, na tela de todos os contratos
    -   listaMensagensInstanciadas: lista de mensagens, na tela de conversa com usuário X
    -   listaConversasInstanciadas: lista de conversas, na tela de conversas ativas
    -   listaAvaliacoesInstanciadas: lista de avaliações, na tela de avaliações
    * Dicionários utilitários
    -   dicNomesUsuarios: Relação e-mail->nome de usuários, evitando acessos excessivos ao banco de dados
    * Referências básicas para uso do Firebase:
    -   dbRef: referencia para raiz do banco de dados Firebase
    -   storageRef: referencia para armazenamento de imagens do Firebase
    * Referencias de GameObjects do Unity:
    -   permissionPanel: painel que exibe explicação para o usuário sobre necessidade da permissão de acesso aos arquivos do sistema
    * Referencias dos elementos da tela de perfil
    -   perfilPanel: a tela de exibição de perfil
    -   perfNomeUser: texto que exibe nome do usuário dono do perfil
    -   butPerfEditar: botão para navegar para tela de edição de perfil
    -   butPerfConversar: botão para navbegar para tela de conversa com o dono deste perfil
    -   perfIdade: texto que exibe idade do usuário dono do perfil
    -   perfGenero: texto que exibe gênero do usuário dono do perfil
    -   perfImagem: texto que exibe imagem do usuário dono do perfil
    * Referencias dos elementos da tela de editar perfil
    -   perfEditarNome: campo de texto para editar nome de exibição do usuário
    -   perfEditarIdade: campo de texto para editar idade do usuário a ser exibida no perfil
    -   perfEditarGenero: campo de texto para editar gênero do usuário a ser exibido no perfil
    -   perfEditarImagem: imagem de prévia sendo exibida na tela
    * Referencias dos elementos da tela de contatos
    -   contListaPanel: a tela de exibição da lista de formas de contato
    -   contButtonText: botão para navegação. No perfil do próprio usuário abre tela de criação de novo contato, enquanto no perfil de outros usuários navega para tela de conversa
    -   contEditarTipo: campo de texto para editar tipo de contato
    -   contEditarValor: campo de texto para editar informações de contato
    -   contNovoPainel: a tela de criação de novo contato
    -   contatoTemplate: template de contato a ser copiado para a lista de contatos
    * Referencias dos elementos das telas de conversas
    -   msgListaPanel: a tela de conversa, onde os usuários trocam mensagens
    -   conversaListaPanel: a tela de lista de conversas, onde são exibidos usuários com quem alguma mensagem ja foi trocada
    -   messageTemplate: template de mensagem a ser copiado para a lista de mensagens
    -   conversaTemplate: template de contato a ser copiado para a lista de conversas
    -   newMensage: caixa de texto onde o usuário insere o texto da mensagem que deseja enviar
    * Referencias dos elementos da tela de avaliação
    -   avalSlider: slider usado pelo usuário para deixar sua avaliação
    -   avalButton: botão para publicar a avaliação selecionada
    -   avalDisplay: texto exibindo o valor atual selecionado da avaliação
    -   avalTemplate: template de avaliação a ser copiado para a lista de avaliações
    * Referencias dos elementos das telas de comissão e criação de comissões
    -   comissionTemplate: template de contrato a ser copiado para a lista de contratos
    -   comissionUsuarioTemplate: template de contrato a ser copiado para a lista de contratos do usuário
    -   comissionListPanel: tela da lista de contratos
    -   comissionListUsuarioPanel: tela da lista de contratos do usuário
    -   newTitulo: campo de texto para titulo de novo contrato
    -   newDescricao: campo de texto para descrição de novo contrato
    -   newClasse: radiobuttons para selecionar se o contrato é uma "comissão" ou um "serviço"
    -   newEstado: dropdown para selecionar o estado onde o contrato está sendo oferecida
    -   newCidade: dropdown para selecionar a cidade onde o contrato está sendo oferecida, atualizado conforme estado
    -   newFrequenciaUnico: radiobuttons para selecionar se o contrato ocorrerá apenas 1 vez ou se deseja-se repetir a operação semanalmente
    -   newFrequenciaDom: checkbox para sinalizar que o contrato repetirá aos domingos
    -   newFrequenciaSeg: checkbox para sinalizar que o contrato repetirá às segundas-feiras
    -   newFrequenciaTer: checkbox para sinalizar que o contrato repetirá às terças-feiras
    -   newFrequenciaQua: checkbox para sinalizar que o contrato repetirá às quartas-feiras
    -   newFrequenciaQui: checkbox para sinalizar que o contrato repetirá às quintas-feiras
    -   newFrequenciaSex: checkbox para sinalizar que o contrato repetirá às sextas-feiras
    -   newFrequenciaSab: checkbox para sinalizar que o contrato repetirá aos sábados
    -   newTipoObra: radiobuttons para selecionar se o contrato é um serviço de obra
    -   newTipoFaxina: radiobuttons para selecionar se o contrato é um servi~ço de faxina
    -   newTipoBaba: radiobuttons para selecionar se o contrato é um serviço de babá
    -   newTipoOutro: radiobuttons para selecionar se o contrato é um outro tipo de serviço que as demais opções não cobrem
    -   newPreco: campo de texto para inserir valor a ser oferecido ou cobrado pelo serviço
    -   newURLImagem: URL da imagem que se deseja associar à comsision
    -   newImagem: cimagem exibindo uma prévia baseado na URL inserida
    * Referencias dos elementos das telas de informações aprofundadas do contrato
    -   infoPanel: tela de informações sobre contrato
    -   infoEstado: texto que exibe o estado onde o contrato está sendo oferecida
    -   infoCidade: texto que exibe a cidade onde o contrato está sendo oferecida
    -   infoFrequencia: texto que exibe a frequência com que se deseja realizar o contrato
    -   infoTipo: texto que exibe o tipo do contrato
    -   infoPreco: texto que exibe o valor cobrado ou oferecido pelo contrato
    -   infoClasse: texto que exibe se o contrato é uma "comissão" ou um "serviço"
    -   infoDescricao: texto que exibe a descrição do contrato
    -   infoCriador: texto que exibe o nome do autor do contrato
    -   infoImagem: imagem associada à contrato em sua criação
    * Referencias dos elementos da tela de seleção de filtros de contratos
    -   filterSort: dropdown para selecionar de que forma a lista será ordenada
    -   filterClasse: booleano que sinaliza se as contratos a serem carregadas serão da classe "serviço" (True) ou "comissão" (false)
    -   filterEstado: dropdown para selecionar o estado em que se tem interesse nas contrato
    -   filterCidade: dropdown para selecionar a cidade em que se tem interesse nas contrato
    -   filterFrequenciaUnico: checkbox para selecionar se desejar ver contrato de serviço único, não repetido semanalmente
    -   filterFrequenciaSemanal: checkbox para selecionar se desejar ver contrato de serviço semanal
    -   filterFrequenciaDom: checkbox para selecionar se desejar ver contrato repetidas aos domingos
    -   filterFrequenciaSeg: checkbox para selecionar se desejar ver contrato repetidas às segundas-feiras
    -   filterFrequenciaTer: checkbox para selecionar se desejar ver contrato repetidas às terças-feiras
    -   filterFrequenciaQua: checkbox para selecionar se desejar ver contrato repetidas às quartas-feiras
    -   filterFrequenciaQui: checkbox para selecionar se desejar ver contrato repetidas às quintas-feiras
    -   filterFrequenciaSex: checkbox para selecionar se desejar ver contrato repetidas às sextas-feiras
    -   filterFrequenciaSab: checkbox para selecionar se desejar ver contrato repetidas aos sábados
    -   filterTipoObra: checkbox para selecionar se deseja-se ver contrato do tipo obra
    -   filterTipoFaxina: checkbox para selecionar se deseja-se ver contrato do tipo faxina
    -   filterTipoBaba: checkbox para selecionar se deseja-se ver contrato do tipo babá
    -   filterTipoOutro: checkbox para selecionar se deseja-se ver contrato do tipo outros
    -   filterPrecoMin: caixa de texto para inserir o menor preço, oferecido ou cobrado, que se aceita
    -   filterPrecoMax: caixa de texto para inserir o maior preço, oferecido ou cobrado, que se aceita
    ****************/

    internal static DataBridge dataBridgeInstance;

    string path;
    private bool listenHandlerCooldown = true;
 
    private Comission dataComission;
    private InfoUsuario dataPerfil;
    private Message dataMessage;
    private Contato dataContato;
    private Avaliacao dataAval;

    private List<GameObject> listaContatosInstanciados= new List<GameObject>();
    private List<GameObject> listaComissionsUsuarioInstanciadas= new List<GameObject>();
    private List<GameObject> listaComissionsInstanciadas= new List<GameObject>();
    private List<GameObject> listaMensagensInstanciadas = new List<GameObject>();
    private List<GameObject> listaConversasInstanciadas = new List<GameObject>();
    private List<GameObject> listaAvaliacoesInstanciadas= new List<GameObject>();

    private Dictionary<string,string> dicNomesUsuarios = new Dictionary<string, string>();

    [Header("Referencias Firebase")]
    public DatabaseReference dbRef;
    public StorageReference storageRef;

    [Header("Permissões")]
    [SerializeField]
    private GameObject PermissionPanel;
    
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

    /****************
    Método MonoBehaviour.Awake()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre antes de todas as execuções de MonoBehaviour.Start()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   inicializa a instancia do singleton
    -   evita que mais de um GameObject do Unity crie uma instancia do singleton
    ****************/
    void Awake() {
        if (dataBridgeInstance!= null && dataBridgeInstance != this)
        {   
            Destroy(DataBridge.dataBridgeInstance);
        }
        else
        {
            dataBridgeInstance = this;
        }
        DontDestroyOnLoad(dataBridgeInstance);
    }

    /****************
    Método MonoBehaviour.Start()

    Método executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre após todas as execuções de MonoBehaviour.Awake()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   referências do firebase são inicializadas nas variáveis correspondetes
    -   dicionário de nomes é inicializado com os nomes correntes dos usuários
    ****************/
    private void Start() {
        //prepara referencias de endereços dos arquivos do aplicativo no servidor do firebase
        dbRef=FirebaseDatabase.DefaultInstance.RootReference;
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://pau-pra-toda-obra.appspot.com/");

        //prepara dicionário 
        PrepareDicNomes();

        //prepara navegador de arquivos para exibir apenas imagens .png
        //descartado por ter problemas no android. mantido como comentario para possivel melhoria em versões futuras
        /*FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".png"));
		FileBrowser.SetDefaultFilter( ".png" );
		FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe", ".jpg", ".jpeg", ".txt", ".pdf" );*/
        
    }   

    /****************
    Método MonoBehaviour.Update()

    Método executada 60 vezes por segundo.
    Sobrecarregada para executar operações associadas a atualizar valores na tela

    Resultado: 
    -   na tela de novo contrato, se frequência = único estiver selecionado, desseleciona e desabilita a possibilidade de selecionar dias da semana
    -   na tela de novo contrato, se nenhum dia da semana for selecionado, automaticamente seleciona único
    -   na tela de avaliação, atualiza o texto para exibir o valor selecionado no slider
    ****************/
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

    /****************
    Método PrepareDicNomes()

    Atualiza o dicionário dicNomes para armazenar uma relação e-mail -> nome de exibição do usuário

    Resultado: 
    -   dicNomes passa a armazenar um dicionário tal que cada entrada relaciona e-mail -> nome de exibição
    ****************/
    public void PrepareDicNomes()
    {
        List<DataSnapshot> listaPerfis = new List<DataSnapshot>();
        //carrega dados do perfil dos usuários
        dbRef.Child("infoUsuario").GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao preparar cache de nomes de de usuarios");
                AuthController.authInstance.RaiseErrorPanel("Erro ao preparar cache de nomes de de usuarios");
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

    //Métodos para salvar dados
    /****************
    Método SalvarComissao()

    Verifica se os valores inseridos são válidos. Se sim, salva dados no banco como novo contrato

    Entrada:
    -   newTitulo: titulo dado à novo contrato
    -   newEstado: estado onde novo contrato se localiza
    -   newCidade: cidade onde novo contrato se localiza
    -   newPreco: preço inicialmente oferecido ou cobrado pelo serviço anunciado no contrato

    Resultado: 
    -   se algum dos dados do contrato estiver vazio, aborta a operação e exibe mensagem "Alguns campos obrigatórios não foram preenchidos"
    -   se os dados inseridos forem válidos, executa a corrotina CreateComission(), que salva os dados inseridos em novo contrato no banco de dados
    ****************/
    public void SalvarComissao()
    {
        if(newTitulo.text.Equals("")||newEstado.value == 0||newCidade.value == 0||newPreco.text.Equals("")||StringToFloat(newPreco.text)<=0)
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Alguns campos obrigatórios não foram preenchidos");
            AuthController.authInstance.RaiseErrorPanel("Alguns campos obrigatórios não foram preenchidos");
            return;
        }
        StartCoroutine(CreateComission());
    }

    /****************
    Método SalvarAvaliacao()
mainThreadInstance
    Executa corrotina para salvar avaliação no banco de dados

    Entrada:
    -   avalSlider: slider em que o usuário selecionou o valor da avaliação que deseja dar ao outro usuário

    Resultado: 
    -   executa a corrotina CreateAval(), que salva o valor inserido em um objeto avaliacao no banco de dados
    ****************/
    public void SalvarAvaliacao()
    {
        //StartCoroutine(CreateAval(globalMenu.GetComponent<MenuController>().GetAux()));
        StartCoroutine(CreateAval(MenuController.menuControllerInstance.GetAux()));
        
    }
    
    /****************
    Método SalvarPerfil()

    Verifica se um nome foi inserido na edição de perfil. Se sim, cria e salva seu perfil no sistema, caso seja um novo usuário, ou apenas atualiza seus dados caso já tenha perfil

    Entrada:
    -   perfEditarNome: caixa de texto em que o usuário escreve o nome de exibição desejado
    -   perfEditarIdade: caixa de texto em que o usuário escreve a idade
    -   perfEditarGenero: caixa de texto em que o usuário escreve o ênero

    Resultado: 
    -   se o nome não for preenchido, exibe o erro "Nome obrigatório!" na tela
    -   se os dados inseridos forem válidos, executa a corrotina CreateInfoUsuario(), que salva os dados inseridos em um objeto perfil no banco de dados
    
    ****************/
    public void SalvarPerfil()
    {
        if(perfEditarNome.text.Equals(""))
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Nome obrigatório!");
            AuthController.authInstance.RaiseErrorPanel("Nome obrigatório!");
            return;
        }
        StartCoroutine(CreateInfoUsuario(perfEditarNome.text, perfEditarIdade.text, perfEditarGenero.text));
    }

    /****************
    Método SalvarContato()

    Verifica se tipo e informações de contato fram inseridos. Se sim, salva objeto contato no banco de dados. Senão, exibe erro adequado

    Entrada:
    -   contEditarTipo: caixa de texto em que o usuário escreve o tipo de contato sendo adicionado
    -   contEditarValor: caixa de texto em que o usuário escreve as informações do contato sendo adicionado

    Resultado: 
    -   se o tipo de contato não foi preenchido, exibe "Tipo de contato obrigatório!" na tela
    -   se as informações de contato não foram preenchidas, exibe "Informação de contato obrigatória!" na tela
    -   se os dados inseridos forem válidos, executa a corrotina CreateContato(), que salva os dados inseridos em um objeto contato no banco de dados
    
    ****************/
    public void SalvarContato()
    {
        if(contEditarTipo.text.Equals(""))
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Tipo de contato obrigatório!");
            AuthController.authInstance.RaiseErrorPanel("Tipo de contato obrigatório!");
            return;
        }
        if(contEditarValor.text.Equals(""))
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Informação de contato obrigatória!");
            AuthController.authInstance.RaiseErrorPanel("Informação de contato obrigatória!");
            return;
        }
        StartCoroutine(CreateContato(contEditarTipo.text, contEditarValor.text));
    }

    /****************
    Método EnviarMensagem()

    Verifica se a caixa de nova mensagem foi preenchida. Se sim, cria nova mensagem na conversa entre usuário atual e seu destinatário

    Entrada:
    -   newMensagem: caixa de texto em que o usuário escreve a mensagem que deseja enviar

    Resultado: 
    -   se a caixa de mensagem estiver vazia, exibe o erro "A mensagem não pode ser vazia!"
    -   caso contrário, executa a corrotina CreateMensagem(), que salva no banco de dados um objeto mensagem na conversa entre o usuário e seu destinatário
    ****************/
    public void EnviarMensagem()
    {
        if (newMensagem.text.Equals(""))
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("A mensagem não pode ser vazia!");
            AuthController.authInstance.RaiseErrorPanel("A mensagem não pode ser vazia!");
            return;
        }
        //StartCoroutine(CreateMensagem(globalMenu.GetComponent<MenuController>().GetAux(), newMensagem.text));
        StartCoroutine(CreateMensagem(MenuController.menuControllerInstance.GetAux(), newMensagem.text));
        

    }

    //Métodos de atualzação de dados exibidos
    /****************
    Método RefreshContatoList()

    Executado na tela de lista de formas de contato de um usuário
    Lê, no banco de dados, as informações de contato que o usuário publicou no sistema, e instancia cada elemento na interface como um cartão em uma lista
    Adapta conteudo dos elementos instanciado, exibindo a opção de deletar caso o usuário atual seja o autor

    Parâmetros:
    -   idUsuário: identificador do usuário cujas informações de contato se deseja carregar. Se for -2, carrega o usuário atual no sistema

    Resultado: 
    -   Para cada contato que o usuário de id = idUsuario (ou usuário atual, caso idUsuario = -2), um cartão será exibido na lista ao centro da tela contendo suas informações 
    -   Se o usuário atual for autor dos contatos, cada cartão tem um botão associado para deletar seu respectivo contato do sistema
   ****************/
    public void RefreshContatoList(string idUsuario)
    {
        //atualiza dicionário de nomes caso nome de algum usuário tenha sido modificado desde o inicio da sessão
        PrepareDicNomes();
        
        //se idUsuário == -2, copia idUsuario utilizado para recuperar dados da tela anterior
        if(idUsuario.Equals("-2"))
        {
            //idUsuario=globalMenu.GetComponent<MenuController>().GetAux();
            idUsuario=MenuController.menuControllerInstance.GetAux();
        }

        //remove dados instanciados na ultima chamada do método
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
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar contatos. Tente novamente mais tarde.");
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar contatos. Tente novamente mais tarde.");
                //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
                MenuController.menuControllerInstance.IrJanelaAnterior();
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
                        //if(idUsuario.Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
                        if(idUsuario.Equals(AuthController.authInstance.GetCurrentUserId()))
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

    /****************
    Método RefreshComissionList()

    Executado na tela de lista de contratos
    Lê, no banco de dados, as informações de contratos publicados no sistema, e instancia cada elemento na interface como um cartão em uma lista

    Parâmetros:
    -   classeComissaoRefreshed: string que identifica a classe de contrato a ser filtrada ("Servico", "Comissão", ou "-2"). Se -2, usa o utl

    Entrada:
    -   filterEstado: filtro selecionado no dropdown de filtrar por estado. Por default, exibe todos
    -   filterCidade: filtro selecionado no dropdown de filtrar por cidade. Por default, exibe todos
    -   filterClasse: filtro booleano, sinaliza "True" quando filtrando contratos da classe "Serviço", e "False" quando filtrando por "Comissão". O valor adequado é sempre atualizado externamente antes de se fazer a chamada do método RefreshComissionList()
    -   filterFrequenciaUnico: filtro booleano, sinalizando se deseja exibir contratos que não se repetem. Default True
    -   filterFrequenciaSemanal: filtro booleano, sinalizando se deseja exibir contratos com frequencia semanal. Default True
    -   filterFrequenciaDom: filtro booleano, sinalizando se deseja exibir contratos repetidas aos domingos. Default True
    -   filterFrequenciaSeg: filtro booleano, sinalizando se deseja exibir contratos repetidas às segundas-feiras. Default True
    -   filterFrequenciaTer: filtro booleano, sinalizando se deseja exibir contratos repetidas às terças-feiras. Default True
    -   filterFrequenciaQua: filtro booleano, sinalizando se deseja exibir contratos repetidas às quartas-feiras. Default True
    -   filterFrequenciaQui: filtro booleano, sinalizando se deseja exibir contratos repetidas às quintas-feiras. Default True
    -   filterFrequenciaSex: filtro booleano, sinalizando se deseja exibir contratos repetidas às sextas-feiras. Default True
    -   filterFrequenciaSab: filtro booleano, sinalizando se deseja exibir contratos repetidas aos sábados. Default True
    -   filterPrecoMax: filtro digitado no campo para filtrar valor máximo cobrado/oferecido no contrato. Por default não limita valor máximo
    -   filterPrecoMin: filtro digitado no campo para filtrar valor mínimo cobrado/oferecido no contrato. Por default é 0
    -   filterTipoBaba: filtro booleando, sinalizando se deseja exibir contratos do tipo "babá"
    -   filterTipoFaxina: filtro booleando, sinalizando se deseja exibir contratos do tipo "faxina"
    -   filterTipoObra: filtro booleando, sinalizando se deseja exibir contratos do tipo "obra"
    -   filterTipoOutro: filtro booleando, sinalizando se deseja exibir contratos do tipo "outro"
    -   filterSort: filtro selecionado no dropdown para ordenação de exibição dos dados carregados

    Resultado: 
    -   Para cada contrato correspondente aos filtros, um cartão será exibido na lista ao centro da tela contendo suas informações 
    ****************/
    public void RefreshComissionList(string classeComissaoRefreshed)
    {
        //atualiza dicionário de nomes caso nome de algum usuário tenha sido modificado desde o inicio da sessão
        PrepareDicNomes();
        
        //se classeComissaoRefreshed = -2, copia classe de contrato utilizada na ultima chamada do método.
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

        //remove dados instanciados na ultima chamada do método
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
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar lista de contratos. Tente novamente mais tarde.");
                //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar lista de contratos. Tente novamente mais tarde.");
                MenuController.menuControllerInstance.IrJanelaAnterior();
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

    /****************
    Método RefreshComissionList()

    Executado na tela de lista de contratos publicadas por um usuário especifico, em seu perfil
    Lê, no banco de dados, as informações de de contratos publicadas no sistema pelo usuário dono do perfil sendo acessado, e instancia cada elemento na interface como um cartão em uma lista
    Também torna possivel um usuário excluir contrato de sua autoria

    Resultado: 
    -   Para cada contrato criada pelo usuário (dono do perfil sendo acessado), um cartão será exibido na lista ao centro da tela contendo suas informações 
    -   Se o dono do perfil sendo visualizado é o usuário atual, um botão é disponibilizado em cada cartão para excluir o contrato associada
    ****************/
    public void RefreshComissionUsuarioList()
    {
        //atualiza dicionário de nomes caso nome de algum usuário tenha sido modificado desde o inicio da sessão
        PrepareDicNomes();
        //string idUsuario = globalMenu.GetComponent<MenuController>().GetAux();
        string idUsuario = MenuController.menuControllerInstance.GetAux();
        
        //remove dados instanciados na ultima chamada do método
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
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar lista de contratos deste usuário. Tente novamente mais tarde.");
                //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar lista de contratos deste usuário. Tente novamente mais tarde.");
                MenuController.menuControllerInstance.IrJanelaAnterior();
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
                        //if(idUsuario.Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
                        if(idUsuario.Equals(AuthController.authInstance.GetCurrentUserId()))
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

    /****************
    Método RefreshAvaliacaoList()

    Executado na tela de lista de avaliações publicadas sobre um usuário especifico, em seu perfil
    Lê, no banco de dados, as informações de avaliações direcionadas ao usuário dono do perfil sendo acessado, e instancia cada elemento na interface como um cartão em uma lista
    Se o dono do perfil sendo visualizado não é o usuário atual, o método também habilita o painel que permite deixar uma avaliação

    Resultado: 
    -   Para cada avaliação feita ao serviço do usuário(dono do perfil sendo acessado), um cartão será exibido na lista ao centro da tela contendo suas informações 
    -   Se o usuário atual não for dono do perfil sendo acessado, um painel será exibido na porção inferior da tela, disponibilizando o botão e o slider 
    ****************/
    public void RefreshAvaliacaoList()
    {
        //atualiza dicionário de nomes, caso algum usuário tenha alterado seu nome desde o início da sessão
        PrepareDicNomes();

        //se estiver olhando a própria lista de avaliações, esconde a opção de publicar sua avaliação. exibe caso contrário
        //string idUsuario = globalMenu.GetComponent<MenuController>().GetAux();
        string idUsuario = MenuController.menuControllerInstance.GetAux();
        //if (idUsuario.Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
        if (idUsuario.Equals(AuthController.authInstance.GetCurrentUserId()))
        {
            avalButton.SetActive(false);
        }
        else{
            avalButton.SetActive(true);
        }
        
        //limpa dados instanciados na última execução deste método
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
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar avaliações. Tente novamente mais tarde");
                //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar avaliações. Tente novamente mais tarde");
                MenuController.menuControllerInstance.IrJanelaAnterior();
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

    /****************
    Método RefreshMensagemList()

    Executado na tela de conversa
    Lê, no banco de dados, as mensagens trocadas entre o usuário atual e o usuário alvo (identificado por destinatarioId) e instancia cada elemento na interface como um cartão em uma lista
 
    Parâmetros:
    -   destinatarioId: identificador do usuário destinatário

    Resultado: 
    -   Para cada mensagem trocada entre os usuários, um cartão será exibido na lista ao centro da tela contendo suas informações
    ****************/
    public void RefreshMensagemList(string destinatarioId)
    {
        //atualiza dicionário de nomes, caso algum usuário tenha mudado de nome durante a sessão atual
        PrepareDicNomes();

        //se destinatárioId for -2, carrega id de último usuário acessado e empilha no MenuController os valores adequados para preparar a tela de troca de mensagem 
        if(destinatarioId.Equals("-2"))
        {
            //destinatarioId = globalMenu.GetComponent<MenuController>().GetAux();
            destinatarioId = MenuController.menuControllerInstance.GetAux();
            
            /*implementação antiga sem singleton
            globalMenu.GetComponent<MenuController>().IrNovoAux(globalMenu.GetComponent<MenuController>().GetAux());
            globalMenu.GetComponent<MenuController>().IrNovoTitulo("Conversa com "+dicNomesUsuarios[destinatarioId]);
            globalMenu.GetComponent<MenuController>().IrNovaJanela(msgListaPanel);*/
            MenuController.menuControllerInstance.IrNovoAux(MenuController.menuControllerInstance.GetAux());
            MenuController.menuControllerInstance.IrNovoTitulo("Conversa com "+dicNomesUsuarios[destinatarioId]);
            MenuController.menuControllerInstance.IrNovaJanela(msgListaPanel);

        }

        //remove mensagens instanciadas na última execução deste método
        for (int i = 0; i<listaMensagensInstanciadas.Count;)
        {
            GameObject currentObj = listaMensagensInstanciadas[i];
            listaMensagensInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //carrega dados do servidor
        //dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), destinatarioId))).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(AuthController.authInstance.GetCurrentUserId(), destinatarioId))).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
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

    /****************
    Método RefreshConversasList()

    Executado na tela de lista de conversas
    Lê, no banco de dados, com que usuários o usuário atual já iniciou uma conversa, e instancia cada elemento na interface como um cartão em uma lista
 
    Resultado: 
    -   Para cada conversa iniciada contendo o usuário, um cartão será exibido na lista ao centro da tela contendo o nome do interlocutor
    ****************/
    public void RefreshConversasList()
    {
        //atualiza dicionário de nomes de usuários, para caso algum usuário tenha modificado seu nome durante a sessão atual
        PrepareDicNomes();

        //remove instancias criadas na última execução deste método
        for (int i = 0; i<listaConversasInstanciadas.Count;)
        {
            GameObject currentObj = listaConversasInstanciadas[i];
            listaConversasInstanciadas.Remove(currentObj);
            Destroy(currentObj);
        }

        //recebe dados do banco
        //dbRef.Child("infoUsuario/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())+"/conversas").GetValueAsync().ContinueWithOnMainThread(tarefa =>{
        dbRef.Child("infoUsuario/"+MakeTokensValid(AuthController.authInstance. GetCurrentUserId())+"/conversas").GetValueAsync().ContinueWithOnMainThread(tarefa =>{   
            if(tarefa.IsFaulted)
            {
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar lista conversas. Tente novamente mais tarde");
                //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar lista conversas. Tente novamente mais tarde"); 
                MenuController.menuControllerInstance.IrJanelaAnterior();
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
    
    /****************
    Método CarregaInfoContrato()

    Executado na tela de informações de contrato.
    Carrega os dados inseridos na criação do contrato para serem exibidos na tela

    Parâmetros:
    -   keyContrato: chave única indentificadora do contrato cujas informações se deve carregar
    -   isRetorno: flag booleana. False se o método for executado a partir da lista de contratos ou da lista de contratos do usuário. True caso contrario, implicando que a página está sendo revisitada a partir da funcionalidade de voltar para tela anterior

    Resultado: 
    -   As informações do contrato são carregadas nos respectivos campos
    ****************/
    public void CarregaInfoContrato(string keyContrato, bool isRetorno)
    {
        //atualiza dicionário de nomes, caso algum usuário tenha atualizado seu nome durante a sessão atual
        PrepareDicNomes();

        //empilha valor auxiliar e objeto da tela atual. titulo é empilhado posteriormente pois é o titulo do contrato buscado
        if(!isRetorno)
        {
            //globalMenu.GetComponent<MenuController>().IrNovoAux(keyContrato);
            //globalMenu.GetComponent<MenuController>().IrNovaJanela(infoPanel);
            MenuController.menuControllerInstance.IrNovoAux(keyContrato);
            MenuController.menuControllerInstance.IrNovaJanela(infoPanel);
        }
        //carrega dados do banco
        dbRef.Child("comission/"+keyContrato).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar dados do contrato. Tente novamente mais tarde");
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar dados do contrato. Tente novamente mais tarde");
                if(!isRetorno)
                {   //empilha titulo placeholder antes de sair da tela para manter sincronia das pilha de navegação
                    //globalMenu.GetComponent<MenuController>().IrNovoTitulo("");
                    MenuController.menuControllerInstance.IrNovoTitulo("");
                }
                //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
                MenuController.menuControllerInstance.IrJanelaAnterior();
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
                        //globalMenu.GetComponent<MenuController>().IrNovoTitulo(snapshot.Child("titulo").GetValue(false).ToString());
                        MenuController.menuControllerInstance.IrNovoTitulo(snapshot.Child("titulo").GetValue(false).ToString());
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

    /****************
    Método CarregarPerfil()

    Executado na tela de informações de perfil.
    Carrega os dados inseridos na criação do perfil para serem exibidos na tela

    Parâmetros:
    -   idUsuario: chave única indentificadora do usuário cujas informações se deve carregar

    Resultado: 
    -   As informações do perfil são carregadas nos respectivos campos
    ****************/
    public void CarregarPerfil(string idUsuario)
    {
        //se idUsuario = -1, carrega dados do perfil do usuário atual e registra na pilha de navegação
        if (idUsuario.Equals("-1"))
        {
            //idUsuario = authControllerObj.GetComponent<AuthController>().GetCurrentUserId();
            //globalMenu.GetComponent<MenuController>().IrNovoTitulo("Meu Perfil");
            //globalMenu.GetComponent<MenuController>().IrNovoAux(idUsuario);
            idUsuario = AuthController.authInstance.GetCurrentUserId();
            MenuController.menuControllerInstance.IrNovoTitulo("Meu Perfil");
            MenuController.menuControllerInstance.IrNovoAux(idUsuario);
        }

        //busca perfil do usuario no sistema
        dbRef.Child("infoUsuario").Child(MakeTokensValid(idUsuario)).GetValueAsync().ContinueWithOnMainThread(tarefa =>{
            if(tarefa.IsFaulted)
            {
                //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar o perfil deste usuário. Tente novamente mais tarde");
                AuthController.authInstance.RaiseErrorPanel("Erro ao carregar o perfil deste usuário. Tente novamente mais tarde");
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

                    //if(globalMenu.GetComponent<MenuController>().GetTitulo().Equals("Meu Perfil"))
                    if(MenuController.menuControllerInstance.GetTitulo().Equals("MeuPerfil"))
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

    //Corrotinas
    /****************
    Corrotina CooldownHandler()

    Espera 0.1s antes de tentar atualizar lista de mensagens

    Entrada:
    -   listenHandlerCooldown: flag usada pelo método ConversaListenHandler() para atrasar a atualização da lista de mensagens para não ser atualizada mais que 1 vez a cada 0.1s.

    Resultado: 
    -   Após o atraso, atualiza lista de mensagens da conversa atual
    ****************/
    IEnumerator CooldownHandler()
    {
        yield return new WaitForSeconds((float)0.1);

        //atualiza lista de mensagens após o atraso
        listenHandlerCooldown = true;
        //RefreshMensagemList(globalMenu.GetComponent<MenuController>().GetAux());
        RefreshMensagemList(MenuController.menuControllerInstance.GetAux());
    }

    

    /****************
    Corrotina LoadImagemUrl()

    Carrega imagem de uma url e insere como textura na imagem alvo targetImage

    Parâmetros:
    -   mediaUrl: url da imagem a ser carregada
    -   targetImage: objeto RawImage exibido na tela, onde se deseja carregar a imagem

    Resultado: 
    -   targetImage passa a exibir a imagem indicada na url inserida
    -   Se ocorrer uma falha no acesso à imagem, um erro é exibido e a operação é cancelada
    ****************/
    IEnumerator LoadImagemUrl (string mediaUrl, RawImage targetImage)
    {
        //carrega imagem do endereço web
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Erro ao carregar imagem: "+request.error);
            AuthController.authInstance.RaiseErrorPanel("Erro ao carregar imagem: "+request.error);
        }
        else
        {
            //passa imagem carregada como textura para imagem alvo
            Texture myTexture =((DownloadHandlerTexture)request.downloadHandler).texture;
            targetImage.texture = myTexture;
        }
    }

    /****************
    Corrotina CreateComission()

    Salva informações de um contrato válido e sua imagem associada no banco de dados

    Entrada:
    -   newTitulo: titulo dado ao novo contrato
    -   newDescricao: descrição dada ao novo contrato
    -   newEstado: estado onde novo contrato se localiza
    -   newCidade: cidade onde novo contrato se localiza
    -   newPreco: preço inicialmente oferecido ou cobrado pelo serviço anunciado no contrato
    -   newFrequenciaDom: booleano True se o contrato se repete aos domingos
    -   newFrequenciaSeg: booleano True se o contrato se repete às segundas-feiras
    -   newFrequenciaTer: booleano True se o contrato se repete às terças-feiras
    -   newFrequenciaQua: booleano True se o contratose repete às quartas-feiras
    -   newFrequenciaQui: booleano True se o contrato se repete às quintas-feiras
    -   newFrequenciaSex: booleano True se o contrato se repete às sextas-feiras
    -   newFrequenciaSab: booleano True se o contrato se repete aos sábados
    -   newTipoObra: booleano True se o contrato for referênte a um serviço de obra
    -   newTipoFaxina: booleano True se o contrato for referênte a um serviço de obra
    -   newTipoBaba: booleano True se o contrato for referênte a um serviço de obra
    -   newImagem: RawImage exibindo a imagem carregada pelo usuário

    Resultado: 
    -   Uma cópia da imagem newImagem passa a existir no banco de dados, caso sua transferencia tenha sucesso. Em caso de falha, uma mensagem de erro é exibida ao usuário e o contrato é salvo sem imagem.
    -   Salva as informações inseridas como um novo contrato no banco de dados. Se houver erro na transferencia da imagem, salva o contrato como sem imagem
    ****************/
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
                    //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha no upload da imagem. Tente novamente mais tarde. Contrato será registrado sem imagem.");
                    AuthController.authInstance.RaiseErrorPanel("Falha no upload da imagem. Tente novamente mais tarde. Contrato será registrado sem imagem.");
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
        //dataComission = new Comission(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), newTitulo.text, newDescricao.text, newImagePath, newEstado.captionText.text, newCidade.captionText.text, thisFrequencia, thisTipo, newPreco.text, thisClasse);
        dataComission = new Comission(AuthController.authInstance.GetCurrentUserId(), newTitulo.text, newDescricao.text, newImagePath, newEstado.captionText.text, newCidade.captionText.text, thisFrequencia, thisTipo, newPreco.text, thisClasse);
        Dictionary<string,System.Object> dicComissao = dataComission.ToDictionary();
        var TarefaDB = dbRef.Child("comission").Push().SetValueAsync(dicComissao);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar novo contrato: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
            AuthController.authInstance.RaiseErrorPanel("Falha ao registrar novo contrato: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //contrato publicado com sucesso. volta a lista de contratos anterior
            RefreshComissionList("-2");
            path = "";

        }
    }

    /****************
    Corrotina CreateMensagem()

    Salva nova mensagem enviada no banco de dados como nova mensagem 
    Se o remetente e o destinatário nunca tiverem conversado pelo aplicativo anteriormente, também salva a nova conversa

    Parâmetros:
    -   destino: identificador do usuário destinatário da mensagem
    -   conteudo: o texto do conteudo da mensagem

    Resultado: 
    -   a mensagem é salva na conversa entre os dois usuários
    -   a lista de mensagens é atualizada para exibir a mensagem enviada
    -   o campo de texto é esvaziado para evitar reenvio da mesma mensagem multiplas vezes
    ****************/
    IEnumerator CreateMensagem(string destino, string conteudo)
    {
        //cria objeto Message com valores inseridos e converte para dicionário
        //dataMessage = new Message(dicNomesUsuarios[authControllerObj.GetComponent<AuthController>().GetCurrentUserId()],dicNomesUsuarios[destino],conteudo);
        dataMessage = new Message(dicNomesUsuarios[AuthController.authInstance.GetCurrentUserId()],dicNomesUsuarios[destino],conteudo);
        Dictionary<string,System.Object> dicMessage = dataMessage.ToDictionary();

        //registra inicio de conversa para usuário atual
        //var TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())+"/conversas/"+MakeTokensValid(destino)).SetValueAsync(destino);
        var TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(AuthController.authInstance.GetCurrentUserId())+"/conversas/"+MakeTokensValid(destino)).SetValueAsync(destino);
        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        //registra inicio de conversa para usuário recebendo a mensagem
        //TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(destino)+"/conversas/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).SetValueAsync(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
        TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(destino)+"/conversas/"+MakeTokensValid(AuthController.authInstance.GetCurrentUserId())).SetValueAsync(AuthController.authInstance.GetCurrentUserId());

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        //registra mensagem no servidor firebase na lista de mensagens trocadas na conversa destes dois usuários
        //TarefaDB = dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), destino))).Push().SetValueAsync(dicMessage);
        TarefaDB = dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(AuthController.authInstance.GetCurrentUserId(), destino))).Push().SetValueAsync(dicMessage);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar nova mensagem: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");]
            AuthController.authInstance.RaiseErrorPanel("Falha ao registrar nova mensagem: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //sucesso publicando nova mensagem. atualiza lista de exibição
            //RefreshMensagemList(globalMenu.GetComponent<MenuController>().GetAux());
            RefreshMensagemList(MenuController.menuControllerInstance.GetAux());
        }

        //limpa caixa de texto de mensagem
        newMensagem.text = "";
    }

    /****************
    Corrotina CreateInfoUsuario()

    Salva perfil de usuário e sua imagem associada no banco de dados, atualizando suas informações caso ja exista

    Parâmetros:
    -   newPerfNome: texto inserido como novo nome de exibição do usuário
    -   newPerfIdade: texto inserido como nova idade do usuário
    -   newPerfGenero: texto inserido como novo gênero do usuário
    -   perfEditarImagem: RawImage contendo a imagem carregada que se deseja associar ao perfil

    Resultado: 
    -   se uma imagem foi carregada, ela será salva no banco de dados e associada ao perfil. se ocorrer falha nessa operação um erro é exibido na tela e o perfil é salvo sem imagem
    -   se o usuário não tinha um perfil antes, salva seu perfil no banco de dados
    -   se o usuário já tinha um perfil, atualiza suas informações no perfil salvo no banco de dados
    -   se ocorrer uma falha nessa operção, uma mensagem de erro é exibida e a operação é cancelada
    ****************/
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
                    //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha no upload da imagem. Tente novamente mais tarde. Perfil será registrado sem imagem.");
                    AuthController.authInstance.RaiseErrorPanel("Falha no upload da imagem. Tente novamente mais tarde. Perfil será registrado sem imagem.");
                    newImagePath = "";
                }
            });
        }
        else 
        {
            newImagePath = "";
        }
        //cria novo objeto InfoUsuario, converte para dicionário e publica no caminho adequado no servidor firebase
        //dataPerfil = new InfoUsuario(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(),newPerfNome,newPerfGenero,newPerfIdade,newImagePath);
        dataPerfil = new InfoUsuario(AuthController.authInstance.GetCurrentUserId(),newPerfNome,newPerfGenero,newPerfIdade,newImagePath);
                    
        Dictionary<string,System.Object> dicPerfil = dataPerfil.ToDictionary();
        //var TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).SetValueAsync(dicPerfil);
        var TarefaDB = dbRef.Child("infoUsuario/"+MakeTokensValid(AuthController.authInstance.GetCurrentUserId())).SetValueAsync(dicPerfil);

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar perfil: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
            AuthController.authInstance.RaiseErrorPanel("Falha ao registrar perfil: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //perfil salvo com sucesso. atualiza tela de perfil e dicionário de nomes, e limpando path da image para uploads futuros
            //CarregarPerfil(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
            CarregarPerfil(AuthController.authInstance.GetCurrentUserId());

            path = "";
            //globalMenu.GetComponent<MenuController>().IrJanelaAnterior();
            MenuController.menuControllerInstance.IrJanelaAnterior();
            PrepareDicNomes();
        }
    }
    
    /****************
    Corrotina CreateContato()

    Salva contato no banco de dados

    Parâmetros:
    -   newContatoTipo: tipo do novo contato, digitado pelo usuário
    -   newContatoValor: informações do novo contato, digitado pelo usuário

    Resultado: 
    -   um novo contato será salvo no banco de dados com as informações inseridas
    -   se ocorrer um erro, uma mensagem é exibida e a operação é cancelada
    ****************/
    IEnumerator CreateContato(string newContatoTipo, string newContatoValor)
    {
        //cria objeto Contato com valores inseridos, converte em dicionário e publica no caminho adequado no servidor firebase
        //dataContato = new Contato(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(),newContatoTipo,newContatoValor);
        dataContato = new Contato(AuthController.authInstance.GetCurrentUserId(),newContatoTipo,newContatoValor);
        Dictionary<string,System.Object> dicContato = dataContato.ToDictionary();
        //var TarefaDB = dbRef.Child("contato/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).Push().SetValueAsync(dicContato);
        var TarefaDB = dbRef.Child("contato/"+MakeTokensValid(AuthController.authInstance.GetCurrentUserId())).Push().SetValueAsync(dicContato);
        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);

        if (TarefaDB.Exception != null)
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar novo contato: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
            AuthController.authInstance.RaiseErrorPanel("Falha ao registrar novo contato: "+(TarefaDB.Exception)+" Tente novamente mais tarde.");
        }
        else{
            //contato salvo com suceso. atualiza lista sendo exibida
            //RefreshContatoList(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
            RefreshContatoList(AuthController.authInstance.GetCurrentUserId());
        }
    }

    /****************
    Corrotina CreateAval()

    Salva avaliação no banco de dados

    Parâmetros:
    -   avaliadoId: identificador unico do usuário sendo avaliado

    Resultado: 
    -   uma nova avaliação sobre o usuário alvo é salva no banco de dados
    -   se ocorrer um erro, uma mensagem é exibida e a operação é cancelada
    ****************/
    IEnumerator CreateAval(string avaliadoId)
    {
        //cria objeto Avaliacao com valor inserido e usuários associados, converte para dicionário e salva no local adequado no servidor 
        //dataAval = new Avaliacao(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(),avalSlider.value.ToString("F1"));
        dataAval = new Avaliacao(AuthController.authInstance.GetCurrentUserId(),avalSlider.value.ToString("F1"));
        Dictionary<string,System.Object> dicAvaliacao = dataAval.ToDictionary();
        //var TarefaDB = dbRef.Child("avaliacao/"+MakeTokensValid(avaliadoId)+"/"+MakeTokensValid(authControllerObj.GetComponent<AuthController>().GetCurrentUserId())).SetValueAsync(dicAvaliacao);
        var TarefaDB = dbRef.Child("avaliacao/"+MakeTokensValid(avaliadoId)+"/"+MakeTokensValid(AuthController.authInstance.GetCurrentUserId())).SetValueAsync(dicAvaliacao);
        

        yield return new WaitUntil(predicate: ()=> TarefaDB.IsCompleted);
        if (TarefaDB.Exception != null) 
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("Falha ao registrar nova avaliação: "+(TarefaDB.Exception)+" Tente novamente mais tarde");
            AuthController.authInstance.RaiseErrorPanel("Falha ao registrar nova avaliação: "+(TarefaDB.Exception)+" Tente novamente mais tarde");
        }
        else{
            //avaliação registrada com sucesso. atualiza lista de exibição
            RefreshAvaliacaoList();
        }
    }


    //Métodos e funções utilitários
    /****************
    Método AttemptReadFile()

    Abre a seleção de arquivos para que o usuário selecione uma imagem para fazer o upload, se tiver concedido permissão, e armazena copia da imagem selecionada no objeto RawImage da tela atual
    Caso contrario, abre painel explicando a necessidade de conceder permissão ao aplicativo

    Entrada:
    -   PermissionPanel: painel com explicação da necessidade de conceder permissão de acesso aos arquivos do dispostivo

    Resultado: 
    -   se acesso foi concedido, será exibida uma seleção para o usuário escolher o arquivo de imagem .png a ser utilizado
    -   se um arquivo valido foi selecinado, a imagem é copiada para o objeto RawImage de prévia da tela atual
    -   se permissão ainda não foi concedida ao aplicativo, um painel é exibido, com texto explicando a necessidade de conceder permissão
    ****************/
    public void AttemptReadFile()
    {
        if(NativeFilePicker.CheckPermission()!=NativeFilePicker.Permission.Granted)
        {
            PermissionPanel.SetActive(true);
            return;
        }
        if( NativeFilePicker.IsFilePickerBusy() )
			return;
        NativeFilePicker.PickFile( (filePath)=>
        {
            if(filePath == "")
            {
                Debug.Log("Seleção de imagem cancelada");
                return;
            }

            RawImage targetRawImage;
            if(MenuController.menuControllerInstance.GetTitulo() == "Editar Meu Perfil")
            {
                //imagem a ser transferida como nova imagem de perfil
                targetRawImage = perfEditarImagem;
            }
            else{
                //imagem a ser transferida como nova imagem de contrato
                targetRawImage = newImagem;
            }

            Texture2D placeholderTexture = new Texture2D(2,2);
            placeholderTexture.LoadImage(File.ReadAllBytes(filePath));
            targetRawImage.texture = placeholderTexture as Texture;

            //redimensiona nova imagem para manter proporção da original
            targetRawImage.rectTransform.sizeDelta= new Vector2(
                targetRawImage.texture.width * 200 / Mathf.Max(targetRawImage.texture.width,targetRawImage.texture.height),
                targetRawImage.texture.height * 200 / Mathf.Max(targetRawImage.texture.width,targetRawImage.texture.height));
            path = filePath;
        }
        ,"image/png");
    }

    /****************
    Método PermissionRequestHandler()

    Se o usuário negou acesso aos arquivos do dispositivo permanentemente, redireciona-o à tela de configurações de permissões do celular
    Caso contrário, requisita as permissões de acesso

    Entrada:
    -   PermissionPanel: painel com explicação da necessidade de conceder permissão de acesso aos arquivos do dispostivo

    Resultado: 
    -   se usuário anteriormente selecionou "não perguntar novamente" quando permissão foi pedida, redireciona-o para as configurações do celular de permissoes do aplicativo
    -   
    ****************/
    public void PermissionRequestHandler()
    {
        if(NativeFilePicker.CheckPermission()==NativeFilePicker.Permission.Denied)
        {
            NativeFilePicker.OpenSettings();
        }
        else
        {
            if (NativeFilePicker.RequestPermission() == NativeFilePicker.Permission.Granted)
            {
                PermissionPanel.SetActive(false);
                AttemptReadFile();
            }
        }
    }
    /****************
    Método SetContatoButton()

    Adapta texto do botão da página de contatos dependendo se a lista pertence ao usuário atual ou a outro usuário

    Resultado: 
    -   se a lista de contatos sendo vista pertence ao usuário atual, o botão passa a exibir o texto "Adicionar forma de contato"
    -   se a lista de contatos sendo vista pertence a outro usuário que não o usuário atual, o botão passa a exibir o texto "Ir para conversa"
    ****************/
    public void SetContatoButton()
    {
        //if(globalMenu.GetComponent<MenuController>().GetAux().Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
        if(MenuController.menuControllerInstance.GetAux().Equals(AuthController.authInstance.GetCurrentUserId()))
        {
            contButtonText.text = "Adicionar forma de contato";
        }
        else{
            contButtonText.text = "Ir para conversa";
        }
    }

    /****************
    Método ContatoButtonHandler()

    Adapta função do botão da página de contatos dependendo se a lista pertence ao usuário atual ou a outro usuário

    Resultado: 
    -   se a lista de contatos sendo vista pertence ao usuário atual, pressionar o botão passa a habilitar o painel de adicionar novo contato
    -   se a lista de contatos sendo vista pertence a outro usuário que não o usuário atual, pressionar o botão passa a navegar para a tela de conversa com ele
    ****************/
    public void ContatoButtonHandler()
    {
        //if(globalMenu.GetComponent<MenuController>().GetAux().Equals(authControllerObj.GetComponent<AuthController>().GetCurrentUserId()))
        if(MenuController.menuControllerInstance.GetAux().Equals(AuthController.authInstance.GetCurrentUserId()))
        {
            //abrir painel de criar contato
            contNovoPainel.SetActive(true);
        }
        else{
            //registra navegação na pila
            /*implementação antiga sem singleton
            globalMenu.GetComponent<MenuController>().IrNovoAux(globalMenu.GetComponent<MenuController>().GetAux());
            globalMenu.GetComponent<MenuController>().IrNovoTitulo("Conversa com "+dicNomesUsuarios[globalMenu.GetComponent<MenuController>().GetAux()]);
            globalMenu.GetComponent<MenuController>().IrNovaJanela(msgListaPanel);*/
            MenuController.menuControllerInstance.IrNovoAux(MenuController.menuControllerInstance.GetAux());
            MenuController.menuControllerInstance.IrNovoTitulo("Conversa com "+dicNomesUsuarios[MenuController.menuControllerInstance.GetAux()]);
            MenuController.menuControllerInstance.IrNovaJanela(msgListaPanel);

            //sai da janela de contatos janela contatos
            contListaPanel.SetActive(false);
            //vai para janela de conversa
            msgListaPanel.SetActive(true);
        }
    }

    

    /****************
    Método ReactivateFilterHandler()

    Atuali1za checkboxes da janela de filtros, desativando caixas de dias da semana se a opção for desselecionada e reabilitando quando marcada novamente

    Entrada:
    -   filterFrequenciaSemanal: valor da checkbox que filtra contratos que se repetem semanalmente 
    -   filterFrequenciaDom: valor da checkbox que filtra contratos que se repetem aos domingos
    -   filterFrequenciaSeg: valor da checkbox que filtra contratos que se repetem às segundas-feiras
    -   filterFrequenciaTer: valor da checkbox que filtra contratos que se repetem às terças-feiras
    -   filterFrequenciaQua: valor da checkbox que filtra contratos que se repetem às quartas-feiras
    -   filterFrequenciaQui: valor da checkbox que filtra contratos que se repetem às quintas-feiras
    -   filterFrequenciaSex: valor da checkbox que filtra contratos que se repetem às sextas-feiras
    -   filterFrequenciaSab: valor da checkbox que filtra contratos que se repetem aos sábados

    Resultado: 
    -   quando a caixa filterFrequenciaSemanal for habilitada, habilita todas as checkboxes de filtros por dias da semana e torna seus valores True
    -   quando a caixa filterFrequenciaSemanal for desabilitada, desabilita todas as checkboxes de filtros por dias da semana e torna seus valores False
    ****************/
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

    /****************
    Método ConversaListenHandler()

    Método executado em resposta à adição de nova mensagem na conversa no banco de dados

    Parâmetros:
    -   o:  Parâmetro herdado do evento Firebase "ChildAdded". Não utilizado nessa aplicação.
    -   args:  Parâmetro herdado do evento Firebase "ChildAdded". Não utilizado nessa aplicação.

    Entrada:
    -   listenHandlerCooldown: booleano que sinaliza True quando se passaram 0.1s desde a última execução do método

    Resultado: 
    -   se uma nova mensagem foi criada a mais que 0.1s na conversa atual, aguarda 0.1s antes de atualizar a lista de mensagens
    ****************/
    public void ConversaListenHandler(object o, ChildChangedEventArgs args)
    {
        //só atualiza se tiverem passado 0.1s entre atualizações para evitar acessos excessivos ao banco
        if (listenHandlerCooldown)
        {
            //RefreshMensagemList(globalMenu.GetComponent<MenuController>().GetAux());
            Debug.Log("Nova mensagem. Refresh conversa com "+MenuController.menuControllerInstance.GetAux());
            listenHandlerCooldown = false;
            StartCoroutine(CooldownHandler());
        }
    }

    /****************
    Método SalvarImagemHandler()

    Verifica se a URL usada para um upload de imagem é válida, carregando a prévia no objeto RawImage associado da tela caso positivo

    Parâmetros:
    -   idPagina: numeração usada para identificar página em que o upload da imagem está sendo feito. 1= edição de perfil. 2= edição de contrato. Qualquer outro valor exibe erro e cancela a operação

    Entrada:
    -   newURLImagem: caixa de texto onde é inserida a url da imagem a ser transferida para contrato

    Resultado: 
    -   se a url endereça uma imagem .png, o objeto rawImage da tela atual passa a exibir a imagem na url.
    ****************/
    public void SalvarImagemHandler()
    {
        StartCoroutine(LoadImagemUrl(path,perfEditarImagem));
    }

    
    /****************
    Método ClearImageField()

    Limpa todas as imagens de prévia, retornando à imagem em branco padrão, e limpa a memória do caminho para imagem a ser transferida

    Entrada:
    -   path: caixa de texto onde é inserida a url da imagem a ser transferida para perfil
    -   perfEditarImagem: prévia da imagem na tela de criação de novo perfil
    -   newImagem: prévia da imagem na tela de criação de novo contrato

    Resultado: 
    -   as imagens de prévia voltam a ser um quadrado branco. O caminho para a imagem a ser transferida é apagado
    ****************/
    public void ClearImageField()
    {
        path = "";
        perfEditarImagem.texture = new Texture2D(200,200) as Texture;
        newImagem.texture = new Texture2D(200,200) as Texture;
    }
    
    //limpa filtros de exibição de contrato para exibir todos. mantem apenas classe como "Comissão" ou "Serviço" pois telas são separadas
    /****************
    Método ClearFiltros()

    Restaura todos os filtros para seus valores padrâo, permitindo novamente a exibição de todos os contratos do sistema

    Entrada:
    -    filterCidade: valor selecionado de filtro de cidade
    -    filterEstado: valor selecionado de filtro de estado
    -    filterFrequenciaUnico: valor selecionado de filtro de frequencia única
    -    filterFrequenciaDom: valor selecionado de filtro de contratos repetidos aos domingos
    -    filterFrequenciaSeg: valor selecionado de filtro de contratos repetidos às segundas-feiras
    -    filterFrequenciaTer: valor selecionado de filtro de contratos repetidos às terças-feiras
    -    filterFrequenciaQua: valor selecionado de filtro de contratos repetidos às quartas-feiras
    -    filterFrequenciaQui: valor selecionado de filtro de contratos repetidos às quintas-feiras
    -    filterFrequenciaSex: valor selecionado de filtro de contratos repetidos às sextas-feiras
    -    filterFrequenciaSab: valor selecionado de filtro de contratos repetidos aos sábados
    -    filterPrecoMax: valor selecionado de filtro de maior preço
    -    filterPrecoMin: valor selecionado de filtro de menor preço
    -    filterTipoBaba: valor selecionado de filtro de contratos de tipo "baba"
    -    filterTipoObra: valor selecionado de filtro de contratos de tipo "obra"
    -    filterTipoFaxina: valor selecionado de filtro de contratos de tipo "faxina"
    -    filterTipoOutro: valor selecionado de filtro de contratos de tipo "outro"

    Resultado: 
    -   todos os filtros voltam aos valores default, salvo o filtro por classe de contrato, que é determinado pela tela. todos os contratos da classe atual voltam a ser exibidos
    ****************/
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

    /****************
    Função StringToFloat()

    Converte a string recebida como parâmetro em valor float equivalente ao digitado

    Parâmetros:
    -   inputString: string a ser convertida
    
    Resultado: 
    -   um float contendo o valor numérico representado pela string dada como parametro
    ****************/
    private float StringToFloat(string inputString)
    {
        if(inputString.Equals(""))
        {
            return 0;
        }
        float result = float.Parse(inputString,System.Globalization.CultureInfo.InvariantCulture);
        return result/10;
    }

    /****************
    Função ComparePrecos()

    Compara dois floats, retornando um int indicativo do resultado da comparação

    Parâmetros:
    -   p1: primeiro float da comparação
    -   p2: segundo float da comparação
    
    Resultado: 
    -   se p1 é maior que p2, retorna 1
    -   se p2 é maior que p1, retorna -1
    -   se p1 e p2 tem o mesmo valor, retorna 0
    ****************/
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

    /****************
    Método ValidarFiltrosPrecos()

    Valida os valores inseridos como filtros de preço, evitando que o menor seja inferior a 0 ou superior ao maior e que qualquer um seja inferior a 0

    Entrada:
    -   filterPrecoMin: valor inserido como preço mínimo do filtro
    -   filterPrecoMax: valor inserido como preço máximo do filtro
    
    Resultado: 
    -   se filterPrecoMin for maior que filterPrecoMax, ou se algum dos dois for menor que 0, uma mensagem de erro é exibida
    ****************/
    public void ValidarFiltrosPrecos()
    {
        if((filterPrecoMin.text!="" && StringToFloat(filterPrecoMin.text)<=0) || (filterPrecoMax.text != "" && StringToFloat(filterPrecoMax.text)<=0) || (filterPrecoMin.text!="" && filterPrecoMax.text!="" &&StringToFloat(filterPrecoMin.text)>StringToFloat(filterPrecoMax.text)))
        {
            //authControllerObj.GetComponent<AuthController>().RaiseErrorPanel("O preço mínimo deve ser menor que o máximo, e ambos devem ser maiores que 0");
            AuthController.authInstance.RaiseErrorPanel("O preço mínimo deve ser menor que o máximo, e ambos devem ser maiores que 0");
            filterPrecoMin.text = "";
            filterPrecoMax.text = "";
        }
    }

    /****************
    Função MakeTokensValid()

    Alguns caracteres não são válidos para nomear endereços do banco de dados Firebase. O caractére '.' (ponto) é um deles
    O email é a chave identificadora de um usuário, que sempre possui '.' mas por sua vez não aceita '"' (aspas duplas) como caractere
    Esta função adapta quaisquer caracteres '.' em uma string para caracteres '"', permitindo usar e-mails como endereço firebase mas ainda converter de volta para e-mail sem ambiguidade quando necessário

    Parâmetros:
    -   inputIdString: e-mail a ser tornado válido 
    
    Resultado: 
    -   uma versão modificada do e-mail é retornada, substituindo todos '.' por '"'
    ****************/
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

    /****************
    Função SortStringsAlpha()

    Concatena ambas strings dadas como parâmetro em uma única string, na qual a primeira em ordem alfabética é a primeira digitada no resultado

    Parâmetros:
    -   inputA: primeira string da concatenação
    -   inputB: segundo string da concatenação
    
    Resultado: 
    -   se inputA é a primeira em ordem alfabética ou igual a inputB, retorna inputA + " " + inputB
    -   se inputB é a primeira em ordem alfabética, retorna inputB + " " + inputA
    ****************/
    private string SortStringsAlpha(string inputA, string inputB)
    {
        List<string> result = new List<string> {inputA,inputB};
        result.Sort();
        return result[0]+" "+result[1];
    }

    /****************
    Métodos IrConversaContrato()

    Executado a partir da janela de informações de um contrato
    Registra uma visitação da janela de conversa na pilha de navegação da classe MenuController, permitindo a ela ao voltar para tela anterior posteriormente

    Parâmetros:
    -   idUsuario: identificador único do usuário destinatário

    Resultado: 
    -   visitação à tela de conversa é registrada na pilha de navegação de MenuController
    -   atualiza lista de mensagens com o usuário destinatário
    -   habilita listener que aguarda novas mensagens para atualizar lista de mensagens com usuário destinatário
    ****************/
    public void IrConversaContrato(string idUsuario)
    {
        //registra navegação na pilha para permitir voltar depois
        //MenuController menu = globalMenu.GetComponent<MenuController>();

        MenuController.menuControllerInstance.IrNovaJanela(msgListaPanel);
        MenuController.menuControllerInstance.IrNovoAux(idUsuario);
        MenuController.menuControllerInstance.IrNovoTitulo("Conversa com "+dicNomesUsuarios[idUsuario]);
        //carrega mensagens trocadas entre este usuário e o outro
        RefreshMensagemList(idUsuario);
        //habilita listener para atualizar lista ao receber nova mensagem
        //dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), idUsuario))).ChildAdded += ConversaListenHandler;
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(AuthController.authInstance.GetCurrentUserId(), idUsuario))).ChildAdded += ConversaListenHandler;
    }

    /****************
    Métodos IrPerfilContrato()

    Registra uma visitação da janela de perfil na pilha de navegação da classe MenuController, permitindo a ela ao voltar para tela anterior posteriormente

    Parâmetros:
    -   idUsuario: identificador único do usuário destinatário

    Resultado: 
    -   visitação à tela de perfil é registrada na pilha de navegação de MenuController
    -   atualiza informações de perfil do usuário destinatário
    ****************/
    public void IrPerfilContrato(string idUsuario)
    {
        //registra navegação na pilha para permitir voltar depois
        //MenuController menu = globalMenu.GetComponent<MenuController>();

        MenuController.menuControllerInstance.IrNovaJanela(perfilPanel);
        MenuController.menuControllerInstance.IrNovoAux(idUsuario);
        MenuController.menuControllerInstance.IrNovoTitulo("Perfil de "+dicNomesUsuarios[idUsuario]);
        //carrega informações do perfil
        CarregarPerfil(idUsuario);
    }

    /****************
    Métodos IrMensagemList()

    Registra uma visitação da janela de conversa na pilha de navegação da classe MenuController, permitindo a ela ao voltar para tela anterior posteriormente

    Parâmetros:
    -   idUsuario: identificador único do usuário destinatário

    Resultado: 
    -   visitação à tela de conversa é registrada na pilha de navegação de MenuController
    -   atualiza lista de mensagens com o usuário destinatário
    -   habilita listener que aguarda novas mensagens para atualizar lista de mensagens com usuário destinatário
    ****************/
    public void IrMensagemList(string idUsuario)
    {
        //registra navegação na pilha para permitir voltar depois
        //MenuController menu = globalMenu.GetComponent<MenuController>();
        MenuController.menuControllerInstance.IrNovaJanela(msgListaPanel);
        MenuController.menuControllerInstance.IrNovoAux(idUsuario);
        MenuController.menuControllerInstance.IrNovoTitulo("Conversa com "+dicNomesUsuarios[idUsuario]);
        //carrega lista de mensagens trocadas entre usuários
        RefreshMensagemList(idUsuario);
        //habilita listener para atualizar lista ao receber nova mensagem
        //dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), idUsuario))).ChildAdded += ConversaListenHandler;
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(AuthController.authInstance.GetCurrentUserId(), idUsuario))).ChildAdded += ConversaListenHandler;
    }

    //Métodos de exclusão de dados
    /****************
    Métodos RemoveComission()

    Exclui um contrato do banco de dados

    Parâmetros:
    -   comissionKey: identificador único do contrato a ser excluido

    Resultado: 
    -   o contrato é removido permanentemente do banco de dados
    ****************/
    public void RemoveComission(string comissionKey)
    {
        dbRef.Child("comission/"+comissionKey).RemoveValueAsync();
    }

    /****************
    Métodos RemoveContato()

    Exclui um contato do banco de dados

    Parâmetros:
    -   contatoKey: identificador único do contato a ser excluido

    Resultado: 
    -   o contato é removido permanentemente do banco de dados
    ****************/
    public void RemoveContato(string contatoKey)
    {
        dbRef.Child("contato/"+contatoKey).RemoveValueAsync();
        //RefreshContatoList(authControllerObj.GetComponent<AuthController>().GetCurrentUserId());
        RefreshContatoList(AuthController.authInstance.GetCurrentUserId());
    }

    /****************
    Métodos RemoveListener()

    Ao sair da tela de conversa, desabilita o listener responsável por manter a conversa atualizada

    Parâmetros:
    -   idUsuario: identificador único do usuário destinatário da conversa

    Resultado: 
    -   para de atualizar a conversa até o usuário voltar à tela de conversa
    ****************/
    public void RemoveListener(string idUsuario)
    {
        //dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(authControllerObj.GetComponent<AuthController>().GetCurrentUserId(), idUsuario))).ChildAdded -= ConversaListenHandler;
        dbRef.Child("conversas/"+MakeTokensValid(SortStringsAlpha(AuthController.authInstance.GetCurrentUserId(), idUsuario))).ChildAdded -= ConversaListenHandler;
    }
}
