namespace LanChat.MessageSystem;

/// <summary>
/// Description :
///     An object that contains message data and runtime metadata.
/// </summary>
public sealed class Message  
{
    #region INTERNAL PROPERTIES

    internal static List < Message > _MESSAGES_ { get; private set; } = new();

    internal uint _ID_ { get; private set; }

    #endregion
    #region PUBLIC   PROPERTIES

    public static Batch    Batch   = new();

    public        string   Sender         ;
    public        DateTime Time           ;
    public        string   Content        ;

    #endregion
    #region PUBLIC   CONSTRUCTOR

    /// <summary>
    /// Description :
    ///     Initializes a message with a runtime id, sender, time it was sent, and its contents.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="time"></param>
    /// <param name="content"></param>
    public Message ( string sender, DateTime time, string content ) 
    {
        _MESSAGES_.Add( this );

        this._ID_ = ( uint )_MESSAGES_.Count - 1;

        this.Sender  = sender ;
        this.Time    = time   ;
        this.Content = content;
    }

    #endregion

    // TODO;
}

/// <summary>
/// Description :
///     An object that represents an element in the batch, stores the runtime id of a message.
/// </summary>
public sealed class eMessage 
{
    public uint     ID  ;

    public eMessage Next;
    public eMessage Prev;

    // TODO;
}

/// <summary>
/// Description :
///     An object that contains a doubly linked list of elements which store message runtime ids.
/// </summary>
public sealed class Batch    
{
    public eMessage First;
    public eMessage Last ;

    // TODO;
}