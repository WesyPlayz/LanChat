/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/18/2026 15:14
/// 
/// REMAINING : FINISHED ( SUBJECT TO UPDATES )

#region GENERAL HEADER

using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;

#endregion
#region LANCHAT HEADER

using LanChat.NetworkSystem;
using LanChat.SerialSystem;

#endregion

namespace LanChat.MessageSystem;

/// <summary>
/// 
/// </summary>
public static class Messager 
{
    #region PRIVATE  STATIC FIELDS

    private static Queue < Batch   > _bQUE_ = []               ;
    private static Queue < Message > _mQUE_ = []               ;

    private static DateTime          _gTIM_ = DateTime.MinValue;
    private static int               _SIZE_ = -1               ;

    #endregion
    #region INTERNAL STATIC PROPERTIES

    internal static string   _NAME_ { get; private set; } = "SERVER";
    internal static TimeSpan _OFST_ { get; private set; }

    #endregion
    #region PUBLIC   STATIC COMPUTED

    /// <summary>
    /// 
    /// </summary>
    public static int Count 
    {
        get => _mQUE_.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    public static int Size  
    {
        get
        {
            _SIZE_ = -1;

            Bridge.Request( "COUNT" );

            while ( _SIZE_ < 0 ) Thread.Sleep( 16 );

            return _SIZE_;
        }
    }

    #endregion
    #region PUBLIC   STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "name"></param>
    public static void Initialize ( Bridge.Mode mode, string name ) 
    {
        if ( !Bridge.Active ) return;

        Bridge.Request( "TIME" );

        while ( _gTIM_ ==  DateTime.MinValue ) Thread.Sleep( 16 );

        _NAME_ = name                 ;
        _OFST_ = _gTIM_ - DateTime.Now;

        switch ( mode )
        {
            case Bridge.Mode.CNT :
                Bridge.Add( "SND", _SEND_CLIENT_ );
                Bridge.Add( "NXT", _NEXT_CLIENT_ );
            break;

            case Bridge.Mode.SRV :
                Bridge.Add( "SND", _SEND_SERVER_ );
                Bridge.Add( "REQ", _RQST_SERVER_ );
            break;
        }
    }

    #endregion
    #region PUBLIC   STATIC COMMUNICATORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    public static void Send  ( int    idx  ) 
    {
        Bridge.Send( idx.ToString() );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cntt"></param>
    public static void Send  ( string cntt ) 
    {
        Message msg = new ( cntt );

        _mQUE_.Enqueue( msg            );
        Bridge.Send   ( msg.ToString() );
    }

    #endregion
    #region PUBLIC   STATIC MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Batch   Fetch_Batch            () 
    {
        return _bQUE_.Dequeue();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Batch   Deferred_Fetch_Batch   () 
    {
        while ( _bQUE_.Count <= 0 ) Thread.Sleep( 16 );

        return _bQUE_.Dequeue();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void    Clear_Batches          () 
    {
        _bQUE_.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Message Fetch_Message          () 
    {
        return _mQUE_.Dequeue();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Message Deferred_Fetch_Message () 
    {
        while ( _mQUE_.Count <= 0 ) Thread.Sleep( 16 );

        return _mQUE_.Dequeue();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void    Clear_Messages         () 
    {
        _mQUE_.Clear();
    }

    #endregion
    #region PRIVATE  STATIC EVENTS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "elmts"></param>
    private static void _SEND_CLIENT_ (                     string[] elmts ) 
    {
        if ( elmts.Length == 4 )
        {
            if ( !DateTime.TryParse( elmts[ 2 ], out DateTime time ) ) return;

            _mQUE_.Enqueue( new ( elmts[ 1 ], time, elmts[ 3 ] ) );
        }
        else if ( elmts.Length == 2 )
        {
            if      ( DateTime.TryParse( elmts[ 1 ], out DateTime time ) ) _gTIM_ = time;
            else if ( int.TryParse     ( elmts[ 1 ], out int      size ) ) _SIZE_ = size;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "elmts"></param>
    private static void _NEXT_CLIENT_ (                     string[] elmts ) 
    {
        Batch btch = new ();

        for ( int idx = 0; idx < elmts.Length; idx += 3 )
        {
            if ( !DateTime.TryParse( elmts[ idx + 1 ], out DateTime time ) ) return;

            btch._INCD_( new ( elmts[ idx ], time, elmts[ idx + 2 ] ) );
        }
        _bQUE_.Enqueue( btch );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "strm" ></param>
    /// <param name = "elmts"></param>
    private static void _SEND_SERVER_ ( NetworkStream strm, string[] elmts ) 
    {
        if ( elmts.Length == 4 )
        {
            if ( !DateTime.TryParse( elmts[ 2 ], out DateTime time ) ) return;

            Batch.Include( new ( elmts[ 1 ], time, elmts[ 3 ] ) );
        }
        else if ( elmts.Length == 2 )
        {
            if ( !int.TryParse( elmts[ 1 ], out int idx ) ) return;

            Batch btch = Batch.Get( idx );

            Bridge.Fill( strm, btch.Size.ToString() );

            Message? cur = btch.First;

            while ( cur != null )
            {
                Bridge.Fill( strm, cur.ToString() );

                cur = cur._NEXT_;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "strm" ></param>
    /// <param name = "elmts"></param>
    private static void _RQST_SERVER_ ( NetworkStream strm, string[] elmts ) 
    {
        if ( elmts.Length != 2 ) return;

        switch ( elmts[ 1 ] )
        {
            case    "TIME"  : Bridge.Send( strm, DateTime.Now.ToString() ); break ;
            case    "COUNT" : Bridge.Send( strm, Batch.Count.ToString()  ); break ;
            default         :                                               return;
        }
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public sealed class Message  
{
    #region INTERNAL INSTANCE FIELDS

    internal Message? _NEXT_ = null;
    internal Message? _PREV_ = null;

    #endregion
    #region PUBLIC   INSTANCE FIELDS

    public readonly string   Sender;
    public readonly DateTime Time  ;

    #endregion
    #region PUBLIC   INSTANCE PROPERTIES

    public string Content { get; private set; }

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cntt"></param>
    internal Message (                             string cntt ) 
    {
        this.Sender  = Messager._NAME_               ;
        this.Time    = DateTime.Now - Messager._OFST_;
        this.Content = cntt                          ;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "time"></param>
    /// <param name = "cntt"></param>
    internal Message (              DateTime time, string cntt ) 
    {
        this.Sender  = Messager._NAME_;
        this.Time    = time           ;
        this.Content = cntt           ;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "sndr"></param>
    /// <param name = "time"></param>
    /// <param name = "cntt"></param>
    internal Message ( string sndr, DateTime time, string cntt ) 
    {
        this.Sender  = sndr;
        this.Time    = time;
        this.Content = cntt;
    }

    #region PUBLIC   OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() => (
        $"{ this.Sender } { Serializer.Splitter } { this.Time } { Serializer.Splitter } { this.Content }"
    );

    #endregion
}

/// <summary>
/// 
/// </summary>
public sealed class Batch    
{
    #region PRIVATE  STATIC   FIELDS

    private static List < Batch > _BATCHES_ = []     ;

    private static int            _bSIZ_    = default; // The Maximum Size Of Each Batch.
    private static int            _sBND_    = 5      ;

    #endregion
    #region PUBLIC   INSTANCE PROPERTIES

    public int      Size  { get; private set; } = 1;

    public Message? First { get; private set; }
    public Message? Last  { get; private set; }

    #endregion
    #region PUBLIC   STATIC   COMPUTED

    /// <summary>
    /// 
    /// </summary>
    public static int Count => _BATCHES_.Count;

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "msg"></param>
    internal Batch ( Message msg = null! ) 
    {
        if ( msg == null ) this.Size = 0;

        this.First = msg;
        this.Last  = msg;
    }

    #region PUBLIC   STATIC   INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    public static void Initialize ( 
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg,
        int          bSiz
    ) {
        if ( _BATCHES_.Count > 0 && bSiz > _sBND_ ) return;

        _bSIZ_ = bSiz;

        Include         ( new ( "Welcome Ryan!" )                  );
        Renderer.Refresh( true                  , mPnl, msgs, tMsg );
    }

    #endregion
    #region PUBLIC   STATIC   ACCESSORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    /// <returns></returns>
    public static Batch Get ( int idx ) 
    {
        if ( idx < 0 || idx >= _BATCHES_.Count ) return null!;

        return _BATCHES_[ idx ];
    }

    #endregion
    #region PUBLIC   INSTANCE MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "msg"></param>
    internal void     _INCD_ ( Message msg ) 
    {
        if ( this.Size  == _bSIZ_ ) return;
        if ( this.First != null   )
        {
            msg._PREV_ = this.Last;

            this.Last!._NEXT_ = msg;
            this.Last         = msg;
            this.Size        ++    ;

            return;
        }
        this.First = msg;
        this.Last  = msg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal Message? _EXLD_ (             ) 
    {
        Message? msg = null;

        if ( this.Size == 1 )
        {
            msg = this.First;

            this.First = null;
            this.Last  = null;
        }
        else if ( this.Size > 1 )
        {
            msg                = this.First         ;

            this.First         = this.First!._NEXT_!;
            this.First!._PREV_ = null               ;

            msg!._NEXT_        = null               ;
        }
        return msg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    internal void     _DELT_ ( int     idx ) 
    {
        if ( idx < 0 || idx >= this.Size ) return;

        if ( idx == 0 )
        {
            this.First        = this.First!._NEXT_!;
            this.First._PREV_ = null               ;
        }
        else if ( idx == this.Size - 1 )
        {
            this.Last          = this.Last!._PREV_!;
            this.Last._NEXT_ ??= null              ;
        }
        else
        {
            Message cur = this.First!;

            for ( int cidx = 0; cidx < idx; cidx++ ) cur = cur._NEXT_!;

            cur._PREV_!._NEXT_ = cur._NEXT_;

            if ( cur._NEXT_ != null ) cur._NEXT_._PREV_ = cur._PREV_;
        }
    }

    #endregion
    #region PUBLIC   STATIC   MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "msg"></param>
    public static void     Include ( Message msg            ) 
    {
        if (_BATCHES_.Count > 0 && _BATCHES_[ ^1 ].Size < _bSIZ_ )
        {
            msg._PREV_ = _BATCHES_[ ^1 ].Last;

            _BATCHES_[ ^1 ].Last!._NEXT_ = msg;
            _BATCHES_[ ^1 ].Last         = msg;
            _BATCHES_[ ^1 ].Size        ++    ;

            return;
        }
        Batch btch = new ( msg );

        _BATCHES_.Add( btch );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "bIdx"></param>
    /// <returns></returns>
    public static Message? Exclude ( int     bIdx           ) 
    {
        if ( bIdx < 0 || bIdx >= _BATCHES_.Count || _BATCHES_[ bIdx ].Size == 0 ) return null;

        Message? msg = _BATCHES_[ bIdx ].First        ;
            
        if ( _BATCHES_[ bIdx ].Size == 1 )
        {
            _BATCHES_[ bIdx ].First = null;
            _BATCHES_[ bIdx ].Last  = null;

            _BATCHES_[ bIdx ].Size--;
        }
        else
        {
            _BATCHES_[ bIdx ].First         = _BATCHES_[ bIdx ].First!._NEXT_;
            _BATCHES_[ bIdx ].First!._PREV_ = null                           ;
            msg!._NEXT_                     = null                           ;

            if ( _BATCHES_[ bIdx ].Size == _bSIZ_ )
            {
                Message? nMsg = Exclude( bIdx + 1 );

                if ( nMsg == null ) _BATCHES_[ bIdx ].Size--;
                else
                {
                    nMsg._PREV_                    = _BATCHES_[ bIdx ].Last;
                    _BATCHES_[ bIdx ].Last!._NEXT_ = nMsg                  ;
                    _BATCHES_[ bIdx ].Last         = nMsg                  ;
                }
            }
            else _BATCHES_[ bIdx ].Size--;
        }
        if ( _BATCHES_[ bIdx ].Size == 0 ) _BATCHES_.RemoveAt( bIdx );

        return msg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "bIdx"></param>
    /// <param name = "mIdx"></param>
    public static void     Delete  ( int     bIdx, int mIdx ) 
    {
        if ( 
            bIdx <  0               || 
            bIdx >= _BATCHES_.Count || 
            mIdx <  0               || 
            ( 
                bIdx >= 0                      && 
                bIdx <  _BATCHES_.Count        && 
                mIdx >= _BATCHES_[ bIdx ].Size 
            ) 
        ) return;

        Message? msg;

        if ( mIdx == 0 )
        {
            if ( mIdx == _BATCHES_[ bIdx ].Size - 1 )
            {
                msg                     = Exclude( bIdx + 1 );
                _BATCHES_[ bIdx ].First = msg!               ;
                _BATCHES_[ bIdx ].Last  = msg!               ;

                if ( msg == null ) _BATCHES_[ bIdx ].Size--;

                if ( _BATCHES_[ bIdx ].Size == 0 ) _BATCHES_.RemoveAt( bIdx );

                return;
            }
            _BATCHES_[ bIdx ].First = _BATCHES_[ bIdx ].First!._NEXT_;

            if ( _BATCHES_[ bIdx ].First != null ) _BATCHES_[ bIdx ].First!._PREV_ = null;
        }
        else if ( mIdx == _BATCHES_[ bIdx ].Size - 1 )
        {
            _BATCHES_[ bIdx ].Last = _BATCHES_[ bIdx ].Last!._PREV_;

            if ( _BATCHES_[ bIdx ].Last != null ) _BATCHES_[ bIdx ].Last!._NEXT_ = null;
        }
        else
        {
            Message cur = _BATCHES_[ bIdx ].First!;

            for ( int idx = 0; idx < mIdx; idx++ ) cur = cur._NEXT_!;

            if ( cur._PREV_ != null ) cur._PREV_._NEXT_ = cur._NEXT_;
            if ( cur._NEXT_ != null ) cur._NEXT_._PREV_ = cur._PREV_;
        }
        msg = Exclude( bIdx + 1 );

        if   ( msg == null ) _BATCHES_[ bIdx ].Size--;
        else
        {
            Message prev = _BATCHES_[ bIdx ].Last!;

            prev._NEXT_                    = msg ;
            _BATCHES_[ bIdx ].Last         = msg!;
            _BATCHES_[ bIdx ].Last!._PREV_ = prev;

            return;
        }
        if ( _BATCHES_[ bIdx ].Size == 0 ) _BATCHES_.RemoveAt( bIdx );
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public static class Renderer 
{
    #region PUBLIC   INSTANCE FIELDS

    private static int uIDX = -1; // The Upper Renderable Batch Index.
    private static int lIDX =  0; // The Lower Renderable Batch Index.

    #endregion
    #region PRIVATE  INSTANCE FIELDS

    private static Batch? _uBTC_ = null!;
    private static Batch? _lBTC_ = null!;

    #endregion
    #region PUBLIC   STATIC   RENDERERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    public static void Render_Next (
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg 
    ) {
        if ( Messager.Count == 0 || lIDX != Messager.Size - 1 ) return;

        FrameworkElement elmt = ( FrameworkElement )tMsg.LoadContent();

        elmt.DataContext = Messager.Deferred_Fetch_Message();

        msgs.Children.Add( elmt );
        mPnl.UpdateLayout(      );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "srtu"></param>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    /// <returns></returns>
    public static bool Refresh          (
        bool         srtu,
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg 
    ) {
        if ( !srtu && lIDX >= Messager.Size - 1 ) return false;

        msgs.Children.Clear    ();
        Messager.Clear_Messages();

        uIDX = Messager.Size - 2;
        lIDX = Messager.Size - 1;

        Messager.Send( uIDX );

        _uBTC_ = Messager.Deferred_Fetch_Batch();

        Messager.Send( lIDX );

        _lBTC_ = Messager.Deferred_Fetch_Batch();

        if ( _uBTC_ != null )
        {
            Message? cur = _uBTC_.First;

            while ( cur != null )
            {
                FrameworkElement elmt = ( FrameworkElement )tMsg.LoadContent();

                elmt.DataContext = cur;

                msgs.Children.Add( elmt );
                mPnl.UpdateLayout(      );

                cur = cur._NEXT_;
            }
            return true;
        }
        if ( _lBTC_ != null )
        {
            Message? cur = _lBTC_.First;

            while ( cur != null )
            {
                FrameworkElement elmt = ( FrameworkElement )tMsg.LoadContent();

                elmt.DataContext = cur;

                msgs.Children.Add( elmt );
                mPnl.UpdateLayout(      );

                cur = cur._NEXT_;
            }
            mPnl.ScrollToEnd();

            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    /// <param name = "pdng"></param>
    public static void Render_Upward   ( 
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg,
        int          pdng
    ) {
        if ( 
            Messager.Size       == 0    || 
            _lBTC_              == null || 
            uIDX                <= 0    || 
            mPnl.VerticalOffset >  0 
        ) return;

        for ( int i = 0; i < _lBTC_.Size; i++ ) 
            msgs.Children.RemoveAt( msgs.Children.Count - 1 );
        
        lIDX = uIDX;
        uIDX--     ;

        _lBTC_ = _uBTC_;

        Messager.Send( uIDX );

        _uBTC_ = Messager.Deferred_Fetch_Batch();

        if ( _uBTC_ == null ) return;

        Message? cur = _uBTC_.Last;

        double height = _uBTC_.Size * pdng;

        while ( cur != null )
        {
            FrameworkElement elmt = ( FrameworkElement )tMsg.LoadContent();

            elmt.DataContext = cur;

            msgs.Children.Insert( 0, elmt );
            mPnl.UpdateLayout   (         );

            height += elmt.ActualHeight;
            cur     = cur._PREV_       ;
        }
        mPnl.ScrollToVerticalOffset( mPnl.VerticalOffset + height );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    /// <param name = "pdng"></param>
    public static void Render_Downward ( 
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg,
        int          pdng
    ) {
        if (
            Messager.Size       == 0                     ||
            _uBTC_              == null                  ||
            lIDX                >= Messager.Size - 1     ||
            mPnl.VerticalOffset <  mPnl.ScrollableHeight 
        ) return;

        double height = _uBTC_.Size * pdng;

        for ( int i = 0; i < _uBTC_.Size; i++ )
        {
            FrameworkElement elmt = ( FrameworkElement )msgs.Children[ 0 ];

            height += elmt.ActualHeight;

            msgs.Children.RemoveAt( 0 );
        }
        uIDX = lIDX;
        lIDX++     ;

        _uBTC_ = _lBTC_;

        Messager.Send( lIDX );

        _lBTC_ = Messager.Deferred_Fetch_Batch();

        if ( _lBTC_ == null ) return;

        Message? cur = _lBTC_.First;

        while ( cur != null )
        {
            FrameworkElement elmt = ( FrameworkElement )tMsg.LoadContent();

            elmt.DataContext = cur;

            msgs.Children.Add( elmt );
            mPnl.UpdateLayout(      );

            cur = cur._NEXT_;
        }
        mPnl.ScrollToVerticalOffset( mPnl.VerticalOffset - height );
    }

    #endregion
}
