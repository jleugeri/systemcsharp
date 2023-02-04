namespace SystemCSharp;

public interface IUpdate
{
    ///<summary>
    ///This method is called by the event loop to trigger the state update of an object
    ///</summary>
    public void ApplyUpdate();
}