using System.Collections.Generic;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;

public class LightSourceScript : MonoBehaviour
{

    float startY;
    float startX;
    float disY;

    [SerializeField] private Transform spawnPoint;

    List<RaycastHit2D> results;

    void Start()
    {
        startY = gameObject.transform.position.y;
        results = new List<RaycastHit2D>();
    }

    void Update()
    {
        startX = PlayerMovement2D.i.gameObject.transform.position.x;
        disY = startY - PlayerMovement2D.i.gameObject.transform.position.y;

        Physics2D.Raycast(new Vector2(startX, startY), Vector2.down, ContactFilter2D.noFilter, results, disY + 30);

        if (results.Count > 0 && results[0].collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("SMASKDLŞ");
            PlayerMovement2D.i.gameObject.transform.position = spawnPoint.position;

        }
    }
}
