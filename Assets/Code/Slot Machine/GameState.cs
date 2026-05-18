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
    private static GamePhase _phase = GamePhase.Menu;
    public static GamePhase PreviousPhase { get; private set; } = GamePhase.Menu;

    public static GamePhase Phase
    {
        get => _phase;
        set
        {
            PreviousPhase = _phase;
            _phase = value;
        }
    }
}
