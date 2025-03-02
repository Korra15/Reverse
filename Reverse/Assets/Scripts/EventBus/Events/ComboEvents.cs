/// <summary>
/// To be called whenever string is added to the combo
/// </summary>
public struct AddingToCombo : IEvent
{
    public string comboToAdd;
}

/// <summary>
/// Called whenever the active combo text needs to be cleared
/// </summary>
public struct ClearCombo : IEvent
{

}