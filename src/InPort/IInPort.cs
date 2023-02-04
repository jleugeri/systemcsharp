namespace SystemCSharp;

///<summary>
///Input ports share a signal with output ports, which makes it easy to communicate between modules.
///</summary>
public interface IInPort<DataT> where DataT: IEquatable<DataT>
{
    ///<summary>
    ///Name of the port
    ///</summary>
    string Name { get; }

    ///<summary>
    ///Gives access to the internally bound signal.
    ///Note: this property can only be accessed after the port has been bound to an output port!
    ///</summary>
    Signal<DataT> Signal { get; }

    ///<summary>
    ///This event is called whenever the internal signal is updated, regardless of whether the value changed.
    ///Note: this property can only be accessed after the port has been bound to an output port!
    ///</summary>
    IEvent Updated { get; }

    ///<summary>
    ///This event is called whenever the internal signal is changed.
    ///Note: this property can only be accessed after the port has been bound to an output port!
    ///</summary>
    IEvent Changed { get; }

    ///<summary>
    ///This flag is set whenever the internal signal is updated, regardless of whether the value changed.
    ///Note: this property can only be accessed after the port has been bound to an output port!
    ///</summary>
    bool WasUpdated { get; set; }

    ///<summary>
    ///This flag is set whenever whenever the internal signal is changed.
    ///Note: this property can only be accessed after the port has been bound to an output port!
    ///</summary>
    bool WasChanged { get; set; }

    ///<summary>
    ///The value of the internal signal. This is tied to the values sent by any connected output port.
    ///Note: this property can only be accessed after the port has been bound to an output port!
    ///</summary>
    DataT Value { get; }

    ///<summary>
    ///Resets the port to its initial state
    ///</summary>
    void Reset();

    ///<summary>
    ///Binds the in-port to a source out-port
    ///</summary>
    void Bind(IOutPort<DataT> source);

    ///<summary>
    ///Binds the in-port directly to source signal
    ///</summary>
    void Bind(Signal<DataT> source);

    ///<summary>
    ///Unbinds the in-port form the source signal, if any
    ///</summary>
    void Unbind();
}
