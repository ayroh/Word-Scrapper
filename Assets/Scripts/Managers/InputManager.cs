using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Camera cam;
    private RaycastHit[] hit = new RaycastHit[1];

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (PuzzleManager.gameState != GameState.Started)
            return;

        if (Input.GetMouseButtonDown(0) && Physics.RaycastNonAlloc(cam.ScreenPointToRay(Input.mousePosition), hit) == 1)
        {
            PuzzleManager.instance.HitTile(hit[0].transform.GetComponent<Tile>());
        }
    }
}
