using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image ItemIcon;
    [SerializeField] private TextMeshProUGUI txtCurrentStack;

    public void Setup(ConsumableDataSO data, int currentStack)
    {
        if (data == null || currentStack <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (ItemIcon != null)
            ItemIcon.sprite = data.icon;

        UpdateStackText(currentStack);
    }

    public void UpdateStackText(int stackamount)
    {
        if (txtCurrentStack != null)
        {
            txtCurrentStack.text = stackamount.ToString();
            txtCurrentStack.gameObject.SetActive(stackamount > 0);
        }
    }
}