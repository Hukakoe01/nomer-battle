using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSession
{
    public static bool serverWasStarted = false;
    public static string playerName;
    public static string playerType; // "host" или "guest"
    public static string gameId;
}

