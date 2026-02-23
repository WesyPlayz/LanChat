/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 14:52
/// 
/// REMAINING : FINISHED ( SUBJECT TO UPDATE )

#region GENERAL HEADER

using System.Net;
using System.Net.Sockets;
using System.Text;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Core;
using LanChat.SubSystem.Serialization;

#endregion

namespace LanChat.SubSystem.Network.Runtime;

/// <summary>
/// 
/// </summary>
internal abstract class rtEntity 
{
    #region INTERNAL INSTANCE FIELDS

    internal List< iEntity > _ETTYs_ = [];

    #endregion
    #region PROTECTED INSTANCE FIELDS

    protected UdpClient? _DTGM_ = null         ;
    protected Sync       _SYNC_ = new()        ;

    protected uint       _ATCR_ = uint.MinValue;

    #endregion

    #region INTERNAL  INSTANCE COMPUTED

    /// <summary>
    /// 
    /// </summary>
    internal uint Authenticator => this._ATCR_;

    #endregion

    #region INTERNAL  ABSTRACT FUNCTIONS

    internal abstract void      _AUTH_ ();
    internal abstract void      _STRT_ ();
    internal abstract void      _STOP_ ();
    internal abstract DateTime? _TIME_ ();

    #endregion
}

/// <summary>
/// 
/// </summary>
internal sealed   class rtClient : rtEntity
{
    #region PRIVATE  INSTANCE FIELDS

    private List < string > _PSWDs_ = []               ;

    private TcpClient       _CLNT_  = null!            ;
    private NetworkStream   _STRM_  = null!            ;
    private byte[]          _BFFR_  = new byte[ 1024 ] ;

    private Server?         _SERV_  = null             ;
    private DateTime        _GTIM_  = DateTime.MinValue;

    private List < string > _eQUE_  = []               ;

    private int             _sREQ_  = 0                ;
    private int             _fREQ_  = 0                ;

    #endregion

    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void      _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 1, 10 ) );
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void      _STRT_ () 
    {
        if ( !this._SYNC_.Idle || this._DTGM_ != null ) return;

        this._DCVR_();
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void      _STOP_ () 
    {
        if ( !this._SYNC_.Continue || this._DTGM_ == null ) return;

        this._SYNC_.Close ();
        this._SYNC_.Yield ();
        this._DTGM_?.Close();

        this._DTGM_ = null!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal override DateTime? _TIME_ () 
    {
        if ( this._SERV_ == null ) return null;

        DateTime time = this._GTIM_;

        this._SEND_( Bridge.SND, "TIME" );

        while ( time == this._GTIM_ ) Thread.Sleep( 16 );

        return this._GTIM_;
    }

    #endregion
    #region INTERNAL INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pswd"></param>
    internal       void         _STRT_ ( string pswd ) 
    {
        if ( !this._SYNC_.Idle || this._DTGM_ != null ) return;

        this._DCVR_( pswd );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    /// <returns></returns>
    internal async Task< bool > _CNCT_ ( int    idx  ) 
    {
        Retry rt = new ( 20 );

        if ( !rt.Attempt( () => this._ETTYs_.Count > 0 ) || this._ETTYs_[ idx ].Equals( this._SERV_ ) ) return false;

        this._SYNC_.Start();
        this._CLNT_ = new();

        bool rslt = true;

        try
        {
            if ( !( this._ETTYs_[ idx ] is Server serv ) )
            {
                this._SYNC_.Stop();

                return false;
            }
            this._SERV_ = serv;

            await this._CLNT_.ConnectAsync( this._SERV_.Ip, this._SERV_.Port );

            this._STRM_ = this._CLNT_.GetStream();

            _ = Task.Run( () => this._COMM_() );
        }
        catch { rslt = false; }

        this._SYNC_.Stop();

        return rslt;
    }

    #endregion
    #region INTERNAL INSTANCE COMMUNICATORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cmd" ></param>
    /// <param name = "pyld"></param>
    internal      void _SEND_ ( string   cmd  , string pyld ) 
    {
        Retry rt = new();

        rt.Attempt_Async( () => this._STRM_ != null ).GetAwaiter().GetResult();

        if ( cmd == Bridge.FIL && this._sREQ_ != 0 )
        {
            cmd         = Bridge.NXT;
            this._sREQ_--           ;
        }
        byte[] rqst = Encoding.UTF8.GetBytes( $"{ cmd }{ Serializer.SPLITTER }{ pyld }{ Serializer.TERMINATOR }" );

        this._STRM_.Write( rqst, 0, rqst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "elmts"></param>
    private       void _INVK_ ( string[] elmts              ) 
    {
        if ( elmts.Length <= 1 || !Bridge._OPCDs_.ContainsKey( elmts[ 0 ] ) ) return;

        if ( elmts[ 0 ] == Bridge.FIL && elmts.Length == 2 )
        {
            if ( !int.TryParse( elmts[ 1 ], out int num ) ) return;

            this._eQUE_.Clear();

            this._fREQ_ = num;
        }
        else if ( elmts[ 0 ] == Bridge.NXT && this._fREQ_ > 0 )
        {
            for ( int idx = 1; idx < elmts.Length; idx++ ) this._eQUE_.Add( elmts[ idx ] );

            this._fREQ_--;

            if ( this._fREQ_ == 0 )
            {
                Bridge._OPCDs_[ elmts[ 0 ] ]._cEVNT_?.Invoke( [ .. this._eQUE_ ] );

                this._eQUE_ = [];
            }
        }
        else Bridge._OPCDs_[ elmts[ 0 ] ]._cEVNT_?.Invoke( elmts );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task _COMM_ (                             ) 
    {
        this._SYNC_.Start();

        string rqst = "";
        
        while ( this._SYNC_.Continue )
        {
            int bytes = await this._STRM_.ReadAsync( this._BFFR_, 0, this._BFFR_.Length );

            if ( bytes == 0 ) break;

            rqst += Encoding.UTF8.GetString( this._BFFR_, 0, bytes );

            int rend = default;

            while ( ( rend = rqst.IndexOf( Serializer.TERMINATOR ) ) != -1 )
            {
                this._INVK_( rqst[ ..rend ].Trim().Split( Serializer.SPLITTER ) );

                rqst = rqst[ ( rend + Serializer.TERMINATOR.Length ) .. ];
            }
        }
        this._SYNC_.Stop();
    }

    #endregion
    #region PRIVATE  INSTANCE PROCESSES

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pswd"></param>
    private void _DCVR_ ( string pswd ) 
    {
        if (!this._SYNC_.Continue || this._DTGM_ == null || this._PSWDs_.Contains( pswd ) ) return;

        this._SYNC_.Start(      );
        this._PSWDs_.Add ( pswd );

        byte[] bPsw = Encoding.UTF8.GetBytes( pswd );

        Task.Run( async () => 
        { 
            await this._DTGM_.SendAsync( bPsw, bPsw.Length, IPAddress.Broadcast.ToString(), Bridge._dPRT_ );

            if ( this._DTGM_.Available == 0 ) return;

            var rspc = await this._DTGM_.ReceiveAsync();

            string[] elmts = Encoding.UTF8.GetString( rspc.Buffer ).Split( " : " );

            if ( elmts.Length == 2 )
            {
                Server serv = new ( elmts[ 0 ], int.Parse( elmts[ 1 ] ) );

                if ( !this._ETTYs_.Contains( serv ) ) this._ETTYs_.Add( serv );
            }
            this._SYNC_.Stop();
        });
    }

    /// <summary>
    /// 
    /// </summary>
    private void _DCVR_ (             ) 
    {
        this._SYNC_.Start()                           ;
        this._DTGM_ = new() { EnableBroadcast = true };

        byte[] pswd = Encoding.UTF8.GetBytes( Bridge._DFLT_ );

        Task.Run( async () =>
        {
            while ( this._SYNC_.Continue )
            {
                await this._DTGM_.SendAsync( pswd, pswd.Length, IPAddress.Broadcast.ToString(), Bridge._dPRT_ );

                var timer = Task.Delay( 500 );

                while ( this._SYNC_.Continue && !timer.IsCompleted )
                {
                    if ( this._DTGM_.Available == 0 )
                    {
                        await Task.Delay( 10 );

                        continue;
                    }
                    var rspc = await this._DTGM_.ReceiveAsync();

                    string[] elmts = Encoding.UTF8.GetString( rspc.Buffer ).Split( " : " );

                    if ( elmts.Length == 2 )
                    {
                        Server serv = new ( elmts[ 0 ], int.Parse( elmts[ 1 ] ) );

                        if ( !this._ETTYs_.Contains( serv ) ) this._ETTYs_.Add( serv );
                    }
                }
                await Task.Delay( 500 );
            }
            this._SYNC_.Stop();
        });
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
internal sealed   class rtServer : rtEntity
{
    #region INTERNAL INSTANCE FIELDS

    internal List < Client > _CLNTs_ = [];

    #endregion
    #region PRIVATE  INSTANCE FIELDS

    private TcpListener?    _SERV_ = null         ;
    private int             _PORT_ = 7000         ;
    private string          _PSWD_ = Bridge._DFLT_;

    private List < string > _eQUE_ = []           ;

    private int             _sREQ_ = 0            ;
    private int             _fREQ_ = 0            ;

    #endregion

    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void      _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 4000000000, uint.MaxValue ) );
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void      _STRT_ () 
    {
        if ( !this._SYNC_.Idle || this._SERV_ != null || this._DTGM_ != null ) return;

        this._DCVR_();
        this._PROC_();
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void      _STOP_ () 
    {
        if ( !this._SYNC_.Continue || this._SERV_ == null || this._DTGM_ == null ) return;

        this._SYNC_.Close ();
        this._SYNC_.Yield ();

        this._SERV_?.Stop ();
        this._DTGM_?.Close();

        this._SERV_ = null!;
        this._DTGM_ = null!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal override DateTime? _TIME_ () 
    {
        return DateTime.Now;
    }

    #endregion
    #region INTERNAL INSTANCE COMMUNICATORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cmd" ></param>
    /// <param name = "pyld"></param>
    internal      void _SEND_ ( string    cmd , NetworkStream strm, string pyld  ) 
    {
        if ( cmd == Bridge.FIL && this._sREQ_ != 0 )
        {
            cmd         = Bridge.NXT;
            this._sREQ_--           ;
        }
        byte[] rqst = Encoding.UTF8.GetBytes( $"{ cmd }{ Serializer.SPLITTER }{ pyld }{ Serializer.TERMINATOR }" );

        strm.Write( rqst, 0, rqst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cmd"></param>
    /// <param name = "pyld"></param>
    internal      void _SEND_ ( string    cmd ,                     string pyld  ) 
    {
        if ( cmd == Bridge.FIL && this._sREQ_ != 0 )
        {
            cmd         = Bridge.NXT;
            this._sREQ_--           ;
        }
        byte[] rqst = Encoding.UTF8.GetBytes( $"{ cmd }{ Serializer.SPLITTER }{ pyld }{ Serializer.TERMINATOR }" );

        foreach ( iEntity etty in this._ETTYs_ )
        {
            if ( !( etty is Client clnt ) ) continue;

            clnt._STRM_.Write( rqst, 0, rqst.Length );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "strm" ></param>
    /// <param name = "elmts"></param>
    private       void _INVK_ ( Client    clnt, string[]      elmts              ) 
    {
        if ( elmts.Length <= 1 || !Bridge._OPCDs_.ContainsKey( elmts[ 0 ] ) ) return;

        if ( elmts[ 0 ] == Bridge.FIL && elmts.Length == 2 )
        {
            if ( !int.TryParse( elmts[ 1 ], out int num ) ) return;

            this._eQUE_.Clear();

            this._fREQ_ = num;
        }
        else if ( elmts[ 0 ] == Bridge.NXT && this._fREQ_ > 0 )
        {
            for ( int idx = 1; idx < elmts.Length; idx++ ) this._eQUE_.Add( elmts[ idx ] );

            this._fREQ_--;

            if ( this._fREQ_ == 0 )
            {
                Bridge._OPCDs_[ elmts[ 0 ] ]._sEVNT_?.Invoke( clnt, [ .. this._eQUE_ ] );

                this._eQUE_ = [];
            }
        }
        else Bridge._OPCDs_[ elmts[ 0 ] ]._sEVNT_?.Invoke( clnt, elmts );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cntn"></param>
    /// <returns></returns>
    private async Task _COMM_ ( TcpClient cntn                                   ) 
    {
        this._SYNC_.Start();

        Client        clnt = new ( cntn );
        string        rqst = ""          ;

        this._CLNTs_.Add( clnt );
        
        while ( this._SYNC_.Continue )
        {
            int bytes = await clnt._STRM_.ReadAsync( clnt.bffr, 0, clnt.bffr.Length );

            if ( bytes == 0 ) break;
            rqst += Encoding.UTF8.GetString( clnt.bffr, 0, bytes );

            int rend = default;

            while ( ( rend = rqst.IndexOf( Serializer.TERMINATOR ) ) != -1 )
            {
                this._INVK_( clnt, rqst[ .. rend ].Trim().Split( Serializer.SPLITTER ) );

                rqst = rqst[ ( rend + Serializer.TERMINATOR.Length ) .. ];
            }
        }
        clnt._ACTV_ = false;

        cntn.Close      ();
        this._SYNC_.Stop();
    }

    #endregion
    #region PRIVATE  INSTANCE PROCESSES

    /// <summary>
    /// 
    /// </summary>
    private void _DCVR_ () 
    {
        this._SYNC_.Start (               );
        this._DTGM_ = new ( Bridge._dPRT_ );

        Task.Run( async () =>
        {
            IPEndPoint ePtn = new ( IPAddress.Any, 0 );

            while ( this._SYNC_.Continue )
            {
                if      ( this._DTGM_.Available                                      == 0           ) await Task.Delay( 10 );
                else if ( Encoding.UTF8.GetString( this._DTGM_.Receive( ref ePtn ) ) == this._PSWD_ )
                {
                    byte[] cfrm = Encoding.UTF8.GetBytes( 
                        $"{ Dns.GetHostEntry( Dns.GetHostName() ).AddressList.First( 
                            _ => _.AddressFamily == AddressFamily.InterNetwork 
                        )} : { this._PORT_ }" 
                    );
                    this._DTGM_.Send( cfrm, cfrm.Length, ePtn );
                }
            }
            this._SYNC_.Stop();
        });
    }

    /// <summary>
    /// 
    /// </summary>
    private void _PROC_ () 
    {
        this._SYNC_.Start (                            );
        this._SERV_ = new ( IPAddress.Any, this._PORT_ );
        this._SERV_.Start();

        Task.Run( async () => {
            while ( this._SYNC_.Continue )
            {
                TcpClient clnt = await this._SERV_.AcceptTcpClientAsync();

                _  = this._COMM_( clnt );
            }
            this._SYNC_.Stop();
        });
    }

    #endregion
}
