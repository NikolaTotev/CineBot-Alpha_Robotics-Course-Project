using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ToolTipManager : MonoBehaviour
{

    private static ToolTipManager m_Instance;

    [SerializeField]
    private Camera m_UiCamera;
    public float ToolTipPadding=4.2f;
    private Text m_ToolTipText;
    private RectTransform m_BackgroundRectTransform;

    private void Awake()
    {
        m_BackgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        m_ToolTipText = transform.Find("Txt_Content").GetComponent<Text>();

        ShowToolTip("This is some text so yeah");

        m_Instance = this;
    }   


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, m_UiCamera, out var localPoint);
        transform.localPosition = localPoint;
    }

    private void ShowToolTip(string toolTipText)
    {
        gameObject.SetActive(true);
        m_ToolTipText.text = toolTipText;
        Vector2 backgroundSize = new Vector2(m_ToolTipText.preferredWidth+ToolTipPadding*2f, m_ToolTipText.preferredHeight+ToolTipPadding*2f);
        m_BackgroundRectTransform.sizeDelta = backgroundSize;

    }

    private void HideToolTip()
    {
        gameObject.SetActive(false);
    }

    public static void Show(string toolTipString)
    {
        m_Instance.ShowToolTip(toolTipString);
    }

    public static void Hide()
    {
        m_Instance.HideToolTip();
    }
}
