using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemUI : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Info")]
    public string itemName;
    [TextArea(3, 5)]
    public string itemDescription;
    
    [Header("References")]
    public GameObject descriptionPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    private void Start()
    {
        HideDescription();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescription();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideDescription();
    }

    private void ShowDescription()
    {
        nameText.text = itemName;
        descText.text = itemDescription;
        descriptionPanel.SetActive(true);
    }

    private void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }
}