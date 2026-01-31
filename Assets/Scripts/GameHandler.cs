using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public static GameHandler I { get; private set; }
    public HashSet<ItemType> ownedItems = new HashSet<ItemType>();

    public enum ItemType
    {
        Turkuaz, GreenMaskShard1, GreenMaskShard2,
        // ihtiyacýna göre ekle
    }
    public void DeactiveUI()
    {
        MaskeUI[] list = FindObjectsOfType<MaskeUI>(true); // inactive olanlarý da alýr
        foreach (MaskeUI maske in list)
        {
            maske.gameObject.SetActive(false);

        }
    }

    private void Awake()
    {
        SoundManager.Initialize();
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasItem(ItemType item) => ownedItems.Contains(item);

    public bool AddItem(ItemType item)
    {
        // true dönerse yeni eklenmiþtir
        return ownedItems.Add(item);
    }


void Start()
    {
        BindAllButtons();
        DeactiveUI();
    }

    void BindAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true); // inactive olanlarý da alýr

        foreach (Button button in buttons)
        {
            // ON CLICK
            button.onClick.AddListener(() => OnAnyButtonClicked(button));

            // ON MOUSE ENTER (UI için PointerEnter)
            AddPointerEnter(button.gameObject);
        }
    }

    void OnAnyButtonClicked(Button button)
    {
        SoundManager.PlaySound(SoundManager.Sound.ButtonClick);
        // tek satýrda istediðin kod
    }

    void OnAnyButtonHover(GameObject go)
    {
        SoundManager.PlaySound(SoundManager.Sound.ButtonOver);
        // tek satýrda istediðin kod
    }

    void AddPointerEnter(GameObject go)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = go.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };

        entry.callback.AddListener((data) => OnAnyButtonHover(go));
        trigger.triggers.Add(entry);
    }


}
