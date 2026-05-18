using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Menu,
    Shooting,
    Scoring,
    Results
}

public static class GameState
{
    public static GamePhase Phase = GamePhase.Menu;
}
