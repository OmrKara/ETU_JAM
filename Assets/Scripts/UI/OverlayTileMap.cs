using UnityEngine;
using UnityEngine.Tilemaps;


public class OverlayTileMap : MonoBehaviour
{
    public Tilemap tileMap1, tilemap2, tilemap3, tilemap4, tilemap5, tilemap6;
    public void Mask1Overlay()
    {
        LevelManager.ActivateTilemapWithoutCollision(tileMap1);
    }
    public void Mask2Overlay()
    {
        LevelManager.ActivateTilemapWithoutCollision(tilemap2);
    }
    public void Mask3Overlay()
    {
        LevelManager.ActivateTilemapWithoutCollision(tilemap3);
    }
    public void Mask4Overlay()
    {
        LevelManager.ActivateTilemapWithoutCollision(tilemap4);
    }
    public void Mask5Overlay()
    {
        LevelManager.ActivateTilemapWithoutCollision(tilemap5);
    }
    public void Mask6Overlay()
    {
        LevelManager.ActivateTilemapWithoutCollision(tilemap6);
    }


    public void Mask1OverlayBack()
    {
        LevelManager.RestoreTilemap(tileMap1);
    }
    public void Mask2OverlayBack()
    {
        LevelManager.RestoreTilemap(tilemap2);
    }
    public void Mask3OverlayBack()
    {
        LevelManager.RestoreTilemap(tilemap3);
    }
    public void Mask4OverlayBack()
    {
        LevelManager.RestoreTilemap(tilemap4);
    }
    public void Mask5OverlayBack()
    {
        LevelManager.RestoreTilemap(tilemap5);
    }
    public void Mask6OverlayBack()
    {
        LevelManager.RestoreTilemap(tilemap6);
    }
}
