/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 01:13
/// 
/// REMAINING :
///     Renderer CLASS

#region GENERAL HEADER

using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Core;
using LanChat.SubSystem.Network;
using LanChat.SubSystem.Serialization;

#endregion

namespace LanChat.SubSystem.Messaging;

/// <summary>
/// 
/// </summary>
public static class Messager 
{
    #region PUBLIC   STATIC FIELDS

    public static string TIME  = "TIME" ;
    public static string COUNT = "COUNT";

    #endregion
    #region PRIVATE  STATIC FIELDS

    private static DateTime                  _gTIM_ = DateTime.MinValue; // Global Time

    private static Action < int    , Batch > _bEVT_ = null!            ; // Batch Event
    private static Action < Message        > _mEVT_ = null!            ; // Message Event
    private static Action < DateTime       > _tEVT_ = null!            ; // Time Event
    private static Action < int            > _cEVT_ = null!            ; // Count Event

    #endregion
    #region INTERNAL STATIC PROPERTIES

    internal static string   _NAME_ { get; private set; } = "SERVER"; // Name of Messager
    internal static TimeSpan _OFST_ { get; private set; }             // Time Offset from Global Time

    #endregion

    #region PUBLIC   STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "name"></param>
    public static void Initialize ( Bridge.Mode mode, string name ) 
    {
        if ( !Bridge.Active ) return;

        switch ( mode )
        {
            case Bridge.Mode.CNT :
                Bridge.Bind( Bridge.SND, _SEND_CLIENT_ );
                Bridge.Bind( Bridge.NXT, _NEXT_CLIENT_ );
            break;

            case Bridge.Mode.SRV :
                Bridge.Bind( Bridge.SND, _SEND_SERVER_ );
                Bridge.Bind( Bridge.REQ, _RQST_SERVER_ );
            break;
        }
        if ( mode == Bridge.Mode.CNT )
        {
            Bridge.Request( "TIME" );

            Retry rt = new();

            rt.Attempt_Async( () => _gTIM_ != DateTime.MinValue ).GetAwaiter().GetResult();
        }
        else
        {
            Batch.Initialize( 20 );

            _gTIM_ = DateTime.Now;
        }
        _NAME_ = name;
        _OFST_ = _gTIM_ - DateTime.Now;
    }

    #endregion
    #region PUBLIC   STATIC COMMUNICATORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    public static void Send    ( int    idx  ) 
    {
        Bridge.Send( idx.ToString() );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cntt"></param>
    public static void Send    ( string cntt ) 
    {
        Message msg = new ( DateTime.Now + _OFST_, cntt );

        Bridge.Send( msg.ToString() );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "rqst"></param>
    public static void Request ( string rqst ) 
    {
        if      ( rqst == TIME  ) Bridge.Request( TIME  );
        else if ( rqst == COUNT ) Bridge.Request( COUNT );
    }

    #endregion
    #region PUBLIC   STATIC MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "func"></param>
    public static void Bind ( Action < int    , Batch > func ) => _bEVT_ = func;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "func"></param>
    public static void Bind ( Action < Message        > func ) => _mEVT_ = func;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "func"></param>
    public static void Bind ( Action < DateTime       > func ) => _tEVT_ = func;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "func"></param>
    public static void Bind ( Action < int            > func ) => _cEVT_ = func;

    #endregion
    #region PRIVATE  STATIC EVENTS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "elmts"></param>
    private static void _SEND_CLIENT_ (              string[] elmts ) 
    {
        if ( elmts.Length == 4 )
        {
            if ( !DateTime.TryParse( elmts[ 2 ], out DateTime time ) ) return;

            _mEVT_?.Invoke( new ( elmts[ 1 ], time - _OFST_, elmts[ 3 ] ) );
        }
        else if ( elmts.Length == 2 )
        {
            if      ( DateTime.TryParse( elmts[ 1 ], out DateTime time  ) )
            {
                _gTIM_ = time;

                _tEVT_?.Invoke( time );
            }
            else if ( int.TryParse     ( elmts[ 1 ], out int      count ) ) _cEVT_?.Invoke( count );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "elmts"></param>
    private static void _NEXT_CLIENT_ (              string[] elmts ) 
    {
        if ( !int.TryParse( elmts[ 1 ], out int bIdx ) ) return;

        if      ( elmts[ 0 ] == "BATCH"   )
        {
            Batch btch = new ();

            for ( int idx = 2; idx < elmts.Length; idx += 3 )
            {
                if ( !DateTime.TryParse( elmts[ idx + 1 ], out DateTime time ) ) return;

                btch._INCD_( new ( elmts[ idx ], time - _OFST_, elmts[ idx + 2 ] ) );
            }
            _bEVT_?.Invoke( bIdx, btch );
        }
        else if ( elmts[ 0 ] == "MESSAGE" )
        {
            if ( !DateTime.TryParse( elmts[ 2 ], out DateTime time ) ) return;

            _mEVT_?.Invoke( new( elmts[ 1 ], time - _OFST_, elmts[ 3 ] ) );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt" ></param>
    /// <param name = "elmts"></param>
    private static void _SEND_SERVER_ ( Client clnt, string[] elmts ) 
    {
        if ( elmts.Length == 4 )
        {
            if ( !DateTime.TryParse( elmts[ 2 ], out DateTime time ) ) return;

            Message msg = new ( elmts[ 1 ], time, elmts[ 3 ] );

            Batch.Include( msg );

            Bridge.Fill_All( $"{ 3 }"               );
            Bridge.Fill_All( "MESSAGE"              );
            Bridge.Fill_All( Batch.Count.ToString() );
            Bridge.Fill_All( msg.ToString()         );
        }
        else if ( elmts.Length == 2 )
        {
            if ( !int.TryParse( elmts[ 1 ], out int idx ) ) return;

            Batch btch = Batch.Get( idx );

            Bridge.Fill( clnt, $"{ btch.Size + 2 }" );
            Bridge.Fill( clnt, "BATCH"              );
            Bridge.Fill( clnt, idx.ToString()       );

            Message? cur = btch.First;

            while ( cur != null )
            {
                Bridge.Fill( clnt, cur.ToString() );

                cur = cur._NEXT_;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt" ></param>
    /// <param name = "elmts"></param>
    private static void _RQST_SERVER_ ( Client clnt, string[] elmts ) 
    {
        if ( elmts.Length != 2 ) return;

        switch ( elmts[ 1 ] )
        {
            case    "TIME"  : Bridge.Send( clnt, DateTime.Now.ToString() ); break ;
            case    "COUNT" : Bridge.Send( clnt, Batch.Count.ToString()  ); break ;
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
    #region PUBLIC   INSTANCE PROPERTIES

    public string   Sender  { get; private set; }
    public DateTime Time    { get; private set; }
    public string   Content { get; private set; }

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
    public static void Initialize ( int bSiz ) {
        if ( _BATCHES_.Count > 0 && bSiz > _sBND_ ) return;

        _bSIZ_ = bSiz;

        Include( new ( "Welcome Ryan!" ) );
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
    #region PRIVATE  INSTANCE FIELDS

    private static int    _uPRV_ = -1     ; // The Previous Upper Renderable Batch Index.
    private static int    _lPRV_ =  0     ; // The Previous Lower Renderable Batch Index.

    private static int    _uIDX_ = -1     ; // The Upper Renderable Batch Index.
    private static int    _lIDX_ =  0     ; // The Lower Renderable Batch Index.

    private static Batch? _uBTC_ = null!  ; // The Upper Renderable Batch Object.
    private static Batch? _lBTC_ = null!  ; // The Lower Renderable Batch Object.

    private static bool   _RFSH_ = false  ;
    private static int    _LOAD_ = default;

    #endregion

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
        DataTemplate tMsg
    ) {
        Messager.Bind( Batch   );
        Messager.Bind( Message );

        Refresh( mPnl, msgs, tMsg );
    }

    #endregion
    #region PRIVATE  STATIC   MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    /// <param name = "btch"></param>
    private static void Batch   ( int     idx, Batch btch ) 
    {
        if      ( idx == _uIDX_ ) 
        {
            _uBTC_ = btch  ;
            _uPRV_ = _uIDX_;
        }
        else if ( idx == _lIDX_ )
        {
            _lBTC_ = btch;
            _lPRV_ = _lIDX_;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "msg"></param>
    private static void Message ( Message msg             )
    {

    }

    #endregion
    #region PUBLIC   STATIC   FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    public static void Render_Next     (
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg 
    ) {
        if ( Messager.Count == 0 || _lIDX_ != Messager.Size - 1 ) return;

        FrameworkElement elmt = ( FrameworkElement )tMsg.LoadContent();

        Message msg = Messager.Deferred_Fetch_Message();

        elmt.DataContext = msg;

        msgs.Children.Add( elmt );
        mPnl.UpdateLayout(      );

        if ( msg.Sender == Messager._NAME_ ) mPnl.ScrollToEnd();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mPnl"></param>
    /// <param name = "msgs"></param>
    /// <param name = "tMsg"></param>
    /// <returns></returns>
    public static bool Refresh         (
        ScrollViewer mPnl, 
        StackPanel   msgs, 
        DataTemplate tMsg 
    ) {
        if ( !_RFSH_ ) return false;

        _RFSH_ = true;

        int size = Messager.Size;

        if ( _lIDX_ >= size - 1 ) return false;

        bool rslt = false;

        msgs.Children.Clear();

        _uIDX_ = size - 2;
        _lIDX_ = size - 1;

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
            rslt = true;
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

            rslt = true;
        }
        _RFSH_ = false;

        return rslt;
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
        if ( !( _RFSH_ || _LOAD_ == 1 ) || mPnl.VerticalOffset > 0 || _lBTC_ == null ) return;

        _LOAD_ = 1;

        int size = Messager.Size;

        if ( size == 0 || _uIDX_ <= 0 )
        {
            _LOAD_ = 0;

            return;
        }
        for ( int i = 0; i < _lBTC_.Size; i++ ) msgs.Children.RemoveAt( msgs.Children.Count - 1 );

        _lPRV_ = _lIDX_;
        _lIDX_ = _uIDX_;

        _uPRV_ = _uIDX_;
        _uIDX_--       ;

        _lBTC_ = _uBTC_;

        Messager.Send( _uIDX_ );

        Retry rt = new ();

        rt.Attempt_Async( () => _uPRV_ == _uIDX_ ).GetAwaiter().GetResult();

        if ( _uBTC_ == null )
        {
            _LOAD_ = 0;

            return;
        }
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

        _LOAD_ = 0;
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
        if ( !( _RFSH_ || _LOAD_ == -1 ) || mPnl.VerticalOffset < mPnl.ScrollableHeight || _uBTC_ == null ) return;

        _LOAD_ = -1;

        int size = Messager.Size;

        if ( size == 0 || _lIDX_ >= size - 1 )
        {
            _LOAD_ = 0;

            return;
        }
        double height = _uBTC_.Size * pdng;

        for ( int i = 0; i < _uBTC_.Size; i++ )
        {
            FrameworkElement elmt = ( FrameworkElement )msgs.Children[ 0 ];

            height += elmt.ActualHeight;

            msgs.Children.RemoveAt( 0 );
        }
        _uPRV_ = _uIDX_;
        _uIDX_ = _lIDX_;

        _lPRV_ = _lIDX_;
        _lIDX_++       ;

        _uBTC_ = _lBTC_;

        Messager.Send( _lIDX_ );

        Retry rt = new ();

        rt.Attempt_Async( () => _lPRV_ == _lIDX_ ).GetAwaiter().GetResult();

        if ( _lBTC_ == null )
        {
            _LOAD_ = 0;

            return;
        }
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

        _LOAD_ = 0;
    }

    #endregion
}
