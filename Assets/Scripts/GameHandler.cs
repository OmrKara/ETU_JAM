using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{

    private void Awake()
    {
        // Singleton korumasý
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }
    public static bool IsPaused { get; private set; }

    private void Update()
    {
        var input = InputController2D.Current;
        if (input == null) return;

        if (input is InputController2D ic && ic.PausePressed)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        FindFirstObjectByType<PausePanel>(FindObjectsInactive.Include).gameObject.SetActive(IsPaused);
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    public static GameHandler I { get; private set; }
    public HashSet<ItemType> ownedItems = new HashSet<ItemType>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mod)
    {
        SoundManager.Initialize();

        BindAllButtons();
        DeactiveUI();
    }

    public bool HasItem(ItemType item) => ownedItems.Contains(item);

    public bool AddItem(ItemType item)
    {
        // true dönerse yeni eklenmiþtir
        return ownedItems.Add(item);
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
