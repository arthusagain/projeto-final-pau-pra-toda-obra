using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeArea : MonoBehaviour
{
    /****************
    Clase SafeArea

    Mantem a area ocupada da tela dentro da area segura determinada pelo modelo do aparelho do usuário

    Campos:
    -   targetCanvas: tela em que as mudanças devem ser aplicadas
    -   transformSafeArea: dimensões da area segura da tela
    -   currentSafeArea: area segura atualmente delimitada
    -   currentOrientation: orientação atual da tela do usuário. Deve permanecer no modo retrato
    ****************/
    [SerializeField]
    Canvas targetCanvas;
    RectTransform transformSafeArea;

    Rect currentSafeArea = new Rect();
    ScreenOrientation currentOrientation = ScreenOrientation.Portrait;


    /****************
    Método MonoBehaviour.Awake()

    Método herdado, executada no primeiro frame em que o objeto contendo o script atual estiver ativo, sempre antes de todas as execuções de MonoBehaviour.Start()
    Sobrecarregada para executar as operações desejadas para o preparo inicial do objeto

    Resultado: 
    -   registra area segura atual, suas dimensoes, e a orientação da tela
    ****************/
    void Start()
    {
        transformSafeArea = GetComponent<RectTransform>();

        currentOrientation = Screen.orientation;
        currentSafeArea = Screen.safeArea;

        ApplySafeArea();
    }


    /****************
    Método SafeArea.ApplySafeArea()

    Altera as dimensões da tela para permanecer dentro da area segura delimitada

    Resultado: 
    -   tamanho da tela é contido dentro da área segura
    ****************/
    void ApplySafeArea()
    {
        if (transformSafeArea == null)
        {
            return;
        }

        Rect safeAreaRect = Screen.safeArea;
        Vector2 anchorMin = safeAreaRect.position;
        Vector2 anchorMax = safeAreaRect.position - safeAreaRect.size;

        anchorMin.x /= targetCanvas.pixelRect.width;
        anchorMin.y /= targetCanvas.pixelRect.height;

        anchorMax.x /= targetCanvas.pixelRect.width;
        anchorMax.y /= targetCanvas.pixelRect.height;

        transformSafeArea.anchorMin = anchorMin;
        transformSafeArea.anchorMax = anchorMax;

        currentOrientation = Screen.orientation;
        currentSafeArea = Screen.safeArea;
    }

    /****************
    Método MonoBehaviour.Update()

    Método executada 60 vezes por segundo.
    Sobrecarregada para executar operações associadas a atualizar valores na tela

    Resultado: 
    -   se oriebtação mudou, recalcula area segura e a aplica novamente
    ****************/
    void Update()
    {
        if (currentOrientation!=Screen.orientation || currentSafeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }
}
