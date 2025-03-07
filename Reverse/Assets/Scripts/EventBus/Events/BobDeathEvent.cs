public struct BobDieEvent : IEvent
{

}

/// <summary>
/// To be called whenever bob respawns
/// </summary>
public struct BobRespawnEvent : IEvent
{
    public int killCtr;
}