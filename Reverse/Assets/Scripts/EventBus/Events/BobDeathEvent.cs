public struct BobDieEvent : IEvent
{
    public int killCtr;
}

/// <summary>
/// To be called whenever bob respawns
/// </summary>
public struct BobRespawnEvent : IEvent
{

}