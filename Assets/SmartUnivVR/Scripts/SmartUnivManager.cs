using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SmartUnivManager : MonoBehaviour
{
    public static SmartUnivManager instance;

    public GameObject verticalTablet;
    public GameObject horizontalTablet;

    public CustomDocument activeDocument;

    public RawImage presentationDisplayIMG;
    public TextMeshProUGUI pageIndexText;

    private int pageIndex = 0;
    public GameObject virtualClassroom;
    public GameObject virtualClassroom360;
    public int PageIndex {  get { return pageIndex; } }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        SetNewPage(pageIndex);
        SetNewPageIndexText(pageIndex + 1);

        virtualClassroom.SetActive(true);
        virtualClassroom360.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnNextPage();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnPrevPage();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            virtualClassroom.SetActive(!virtualClassroom.activeInHierarchy);
            virtualClassroom360.SetActive(!virtualClassroom360.activeInHierarchy);

        }
    }

    public void OnNextPage()
    {
        pageIndex++;

        if (pageIndex > activeDocument.documentPages.Length)
        {
            pageIndex = activeDocument.documentPages.Length;
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
        if (activeDocument.documentPages[pageIndex])
            presentationDisplayIMG.texture = activeDocument.documentPages[pageIndex];
    }
    private void SetNewPageIndexText(int index)
    {
        if (pageIndexText)
            pageIndexText.text = index.ToString();
    }
}
