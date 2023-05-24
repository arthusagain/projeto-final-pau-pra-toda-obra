using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeArea : MonoBehaviour
{
    [SerializeField]
    Canvas targetCanvas;
    RectTransform transformSafeArea;

    Rect currentSafeArea = new Rect();
    ScreenOrientation currentOrientation = ScreenOrientation.Portrait;


    void Start()
    {
        transformSafeArea = GetComponent<RectTransform>();

        currentOrientation = Screen.orientation;
        currentSafeArea = Screen.safeArea;
        
    }

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

    // Update is called once per frame
    void Update()
    {
        if (currentOrientation!=Screen.orientation || currentSafeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }
}
