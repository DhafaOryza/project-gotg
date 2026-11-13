using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Notification : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI message;

    [Header("Animation Settings")]
    [SerializeField] private float offScreenY = 700f;
    [SerializeField] private float targetY = 450f;
    [SerializeField] private float animationSpeed = 10f;

    private RectTransform panelRectTransform;
    private bool isShowing = false;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        if (warningPanel != null)
        {
            panelRectTransform = warningPanel.GetComponent<RectTransform>();
            warningPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isShowing && panelRectTransform != null)
        {
            Vector2 anchoredPos = panelRectTransform.anchoredPosition;
            anchoredPos.y = Mathf.Lerp(anchoredPos.y, targetY, Time.unscaledDeltaTime * animationSpeed);
            panelRectTransform.anchoredPosition = anchoredPos;

            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                HideWarning();
            }
        }
    }

    public void ShowWarning(string warningMessage)
    {
        if (warningPanel == null) return;

        if (message != null)
        {
            message.text = warningMessage;
        }

        warningPanel.SetActive(true);
        isShowing = true;

        if (panelRectTransform != null)
        {
            Vector2 anchoredPos = panelRectTransform.anchoredPosition;
            anchoredPos.y = offScreenY;
            panelRectTransform.anchoredPosition = anchoredPos;
        }
    }

    public void HideWarning()
    {
        if (warningPanel == null) return;

        isShowing = false;
        warningPanel.SetActive(false);
    }
}
