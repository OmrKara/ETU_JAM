using System.Collections.Generic;
using UnityEngine;

public class PointScript : MonoBehaviour
{


    float startY;
    float startX;

    float endX;
    float endY;

    float disY;

    [SerializeField] private Transform spawnPoint;

    List<RaycastHit2D> results;


    void Start()

    {
        startX = gameObject.transform.position.x;
        startY = gameObject.transform.position.y;
        results = new List<RaycastHit2D>();
    }

    void Update()
    {
        endX = PlayerMovement2D.i.gameObject.transform.position.x;
        endY = PlayerMovement2D.i.gameObject.transform.position.y;

        disY = startY - PlayerMovement2D.i.gameObject.transform.position.y;

        Physics2D.Raycast(new Vector2(startX, startY), new Vector2(endX - startX, endY - startY), ContactFilter2D.noFilter, results, disY + 30);

        if (results.Count > 0 && results[0].collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("SMASKDLŞ");
            PlayerMovement2D.i.gameObject.transform.position = spawnPoint.position;

        }
    }
}
