using UnityEngine;

public interface IPlayerInput2D
{
    Vector2 Move { get; }
    bool JumpPressed { get; } // sadece bu frame
    bool JumpHeld { get; }    // basýlý tutuluyor mu

    bool ShiftPressed {  get; }
    bool ShiftHeld { get; }

    bool ControlPressed { get; }
    bool ControlHeld { get; }

}