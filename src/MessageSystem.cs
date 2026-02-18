using static System.Net.Mime.MediaTypeNames;

namespace LanChat.MessageSystem;

/// <summary>
/// Description :
///     An object that contains message data and runtime metadata.
/// </summary>
public sealed class Message
{
    #region INTERNAL PROPERTIES

    internal uint     _ID_   { get; set; }

    internal Message  _NEXT_ { get; set; }
    internal Message  _PREV_ { get; set; }

    #endregion
    #region PUBLIC   PROPERTIES
    
    public        DateTime Time   ;

    public        string   Sender ;
    public        string   Content;

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
        this.Sender  = sender ;
        this.Time    = time   ;
        this.Content = content;

        Batch.Include( this );
    }

    #endregion

    // TODO;
}

/// <summary>
/// Description :
///     An object that contains a doubly linked list of elements which store message runtime ids.
/// </summary>
public sealed class Batch ( Message first )
{
    #region INTERNAL PROPERTIES

    internal static List < Batch > _BATCHES_ { get; private set; } = new();

    internal static int            _bSIZ_    { get;         set; } = 20;
    internal static int            _UNF_     { get;         set; }

    internal        int            Size = 1;

    #endregion

    public Message  First = first;
    public Message  Last  = first;

    public static void Include ( Message message )
    {
        if ( _UNF_ < _BATCHES_.Count )
        {
            message._ID_   = _BATCHES_[ _UNF_ ].Last._ID_ + 1;
            message._PREV_ = _BATCHES_[ _UNF_ ].Last         ;

            _BATCHES_[ _UNF_ ].Last._NEXT_ = message;
            _BATCHES_[ _UNF_ ].Last        = message;

            _BATCHES_[ _UNF_ ].Size++;

            if ( _BATCHES_[ _UNF_ ].Size == _bSIZ_ ) _UNF_++;

            return;
        }
        message._ID_ = 0;

        _BATCHES_.Add( new ( message ));
    }
}