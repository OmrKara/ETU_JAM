using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollisionManager : MonoBehaviour
{
    public GameObject[] gameobj;
    public TextMeshProUGUI textBar;
    public Dialogues dialogues;

    public void Start()
    {
        for (int i = 0 ; i < gameobj.Length; i++)
        {
            CollisionScript gameo = gameobj[i].GetComponent<CollisionScript>();
            gameo.text = dialogues.dialogues[i];
            gameo.textBar = textBar;
        }
    }

}
