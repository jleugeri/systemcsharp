namespace SystemCSharp;

///<summary>
///Output ports share a signal with input ports, which makes it easy to communicate between modules.
///</summary>
public interface IOutPort<DataT> where DataT: IEquatable<DataT>
{
    ///<summary>
    ///Name of the port
    ///</summary>
    string Name { get; }

    ///<summary>
    ///Data present on the port. Write to this property to send data to any connected input ports.
    ///</summary>
    DataT Value { get; set; }

    ///<summary>
    ///Resets the port to its initial state
    ///</summary>
    void Reset();

    ///<summary>
    ///Binds the out-port to a target in-port
    ///</summary>
    void Bind(IInPort<DataT> source);
}
