using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq;
public enum TabletOrientation
{
    None, 
    Horizontal,
    Vertical,
    Both
}
public class InteractiveTablet : MonoBehaviour
{
    [Header("Components")]
    public RawImage documentDisplayIMG;
    public Button nextPageBTN;
    public Button prevPageBTN;
    public Button activePageBTN;
    public TextMeshProUGUI pageIndexText;
    
    [Header("Tablet Settings")]
    public TabletOrientation orientation;
    
    [Header("Documents Settings")]
    public CustomDocument document;
    
    private int pageIndex = 0;
    private void Awake()
    {
        if (nextPageBTN)
            nextPageBTN.onClick.AddListener(OnNextPage);
        if (prevPageBTN)
            prevPageBTN.onClick.AddListener(OnPrevPage);
        if (activePageBTN)
            activePageBTN.onClick.AddListener(GetAndDisplayActivePage);
    }
    void Start()
    {
        SetNewPage(pageIndex);
        SetNewPageIndexText(pageIndex + 1);
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (orientation == TabletOrientation.Horizontal)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GetAndDisplayActivePage();
            }
        }
#endif
    }
    public void GetAndDisplayActivePage()
    {
        pageIndex = SmartUnivManager.instance.PageIndex;

        SetNewPage(pageIndex);
        SetNewPageIndexText(pageIndex + 1);
    }

    public void OnNextPage()
    {
        pageIndex++;
        
        if (pageIndex > document.documentPages.Length)
        {
            pageIndex = document.documentPages.Length;
        }
        else
        {
            SetNewPage(pageIndex);
            SetNewPageIndexText(pageIndex + 1);
        }
    }
    public void OnPrevPage()
    {
        pageIndex--;
        
        if (pageIndex < 0)
        {
            pageIndex = 0;
        }
        else
        {
            SetNewPage(pageIndex);
            SetNewPageIndexText(pageIndex + 1);
        }
    }

    private void SetNewPage(int pageIndex)
    {
        if (document.documentPages[pageIndex])
            documentDisplayIMG.texture = document.documentPages[pageIndex];
    }
    private void SetNewPageIndexText(int index)
    {
        pageIndexText.text = index.ToString();
    }
}
