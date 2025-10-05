using UnityEngine;
using UnityEngine.UI;

public class ResponsiveUILayout : MonoBehaviour
{
    [Header("UI Containers")]
    [SerializeField] private RectTransform dartIconsContainer;
    [SerializeField] private RectTransform player1Container;
    [SerializeField] private RectTransform player2Container;
    
    [Header("Responsive Settings")]
    [SerializeField] private float dartIconSize = 60f;
    [SerializeField] private float dartIconSpacing = 10f;
    [SerializeField] private Vector2 padding = new Vector2(20f, 20f);
    
    private Vector2 lastScreenSize;
    
    void Start()
    {
        SetupResponsiveLayout();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }
    
    void Update()
    {
        // Check if screen size changed
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            SetupResponsiveLayout();
            lastScreenSize = currentScreenSize;
        }
    }
    
    private void SetupResponsiveLayout()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float screenWidth = canvasRect.rect.width;
        float screenHeight = canvasRect.rect.height;
        
        // Setup Dart Icons Container (Top Right)
        if (dartIconsContainer != null)
        {
            dartIconsContainer.anchorMin = new Vector2(1, 1);
            dartIconsContainer.anchorMax = new Vector2(1, 1);
            dartIconsContainer.pivot = new Vector2(1, 1);
            
            float totalWidth = (dartIconSize * 3) + (dartIconSpacing * 2);
            dartIconsContainer.sizeDelta = new Vector2(totalWidth, dartIconSize);
            dartIconsContainer.anchoredPosition = new Vector2(-padding.x, -padding.y);
        }
        
        // Setup Player 1 Container (Bottom Right)
        if (player1Container != null)
        {
            player1Container.anchorMin = new Vector2(1, 0);
            player1Container.anchorMax = new Vector2(1, 0);
            player1Container.pivot = new Vector2(1, 0);
            
            float containerWidth = Mathf.Min(screenWidth * 0.25f, 300f);
            float containerHeight = Mathf.Min(screenHeight * 0.15f, 150f);
            player1Container.sizeDelta = new Vector2(containerWidth, containerHeight);
            player1Container.anchoredPosition = new Vector2(-padding.x, padding.y);
        }
        
        // Setup Player 2 Container (Bottom Left)
        if (player2Container != null)
        {
            player2Container.anchorMin = new Vector2(0, 0);
            player2Container.anchorMax = new Vector2(0, 0);
            player2Container.pivot = new Vector2(0, 0);
            
            float containerWidth = Mathf.Min(screenWidth * 0.25f, 300f);
            float containerHeight = Mathf.Min(screenHeight * 0.15f, 150f);
            player2Container.sizeDelta = new Vector2(containerWidth, containerHeight);
            player2Container.anchoredPosition = new Vector2(padding.x, padding.y);
        }
    }
}