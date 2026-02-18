#region GENERAL HEADER

using System.Net;
using System.Net.Sockets;
using System.Text;

#endregion
#region LANCHAT HEADER

using LanChat.CoreSystem;
using LanChat.SerialSystem;

#endregion

namespace LanChat.NetworkSystem;

/// <summary>
/// 
/// </summary>
internal abstract class Runtime      
{
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
internal sealed class Client_RUNTIME : Runtime
{
    #region PRIVATE  INSTANCE FIELDS

    private List< Server > _SERVs_ = []               ;
    private List< string > _PSWDs_ = []               ;

    private TcpClient      _CLNT_  = null!            ;
    private NetworkStream  _STRM_  = null!            ;
    private byte[]         _BFFR_  = new byte[ 1024 ] ;

    private Server?        _SERV_  = null             ;
    private DateTime       _GTIM_  = DateTime.MinValue;
    private int            _fREQ_  = 0                ;

    private List< string > _eQUE_  = []               ;

    #endregion
    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void     _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 1, 10 ) );
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void     _STRT_ () 
    {
        if ( !this._SYNC_.Idle || this._DTGM_ != null ) return;

        this._DCVR_();
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void     _STOP_ () 
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

        this._SEND_( Bridge._OPCDs_[ 1 ], "TIME" );

        while ( time == this._GTIM_ ) Thread.Sleep( 16 );

        return this._GTIM_;
    }

    #endregion
    #region INTERNAL INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pswd"></param>
    internal void _STRT_ ( string pswd ) 
    {
        if ( !this._SYNC_.Idle || this._DTGM_ != null ) return;

        this._DCVR_( pswd );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    /// <returns></returns>
    internal bool _CNCT_ ( int    idx  ) 
    {
        int atmps = 0;

        while ( this._SERVs_.Count <= 0 )
        {
            Thread.Sleep( 16 );
            atmps++;

            if ( atmps == 20 ) return false;
        }
        if ( this._SERVs_[ idx ].Equals( this._SERV_ ) ) return false;

        this._SYNC_.Start   ();
        this._CLNT_ ??= new ();

        Task.Run( async () => {
            this._SERV_ = this._SERVs_[ idx ];

            await this._CLNT_.ConnectAsync( this._SERV_.ip, this._SERV_.port );

            this._STRM_ = this._CLNT_.GetStream();

            await this._COMM_();

            this._SYNC_.Stop();
        });
        return true;
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
        byte[] rqst = Encoding.UTF8.GetBytes( $"{ cmd } { Serializer.Splitter } { pyld } { Serializer.Terminator }" );

        this._STRM_.Write( rqst, 0, rqst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task _COMM_ (                             ) 
    {
        this._SYNC_.Start();

        string rmng = "";
        
        while ( this._SYNC_.Continue )
        {
            string[] elmts = []  ;
            string   rqst  = rmng;
            
            while ( elmts.Length == 0 )
            {
                int bytes = await this._STRM_.ReadAsync( this._BFFR_, 0, this._BFFR_.Length );

                if ( bytes == 0 ) break;

                rqst += Encoding.UTF8.GetString( this._BFFR_, 0, bytes );

                int rend = rqst.IndexOf( Serializer.Terminator );

                if ( rend != -1 )
                {
                    rmng  = rqst[ ( rend + Serializer.Terminator.Length ).. ]                                    ;
                    elmts = rqst[ ..rend                                    ].Trim().Split( Serializer.Splitter );
                }
            }
            if ( elmts.Length == 0 ) break;

            if ( elmts.Length <= 1 || !Bridge._OPCDs_.Contains( elmts[ 0 ] ) ) continue;

            for ( int idx = 0; idx < Bridge._OPCDs_.Length; idx++ )
            {
                if ( elmts[ 0 ].Equals( Bridge._OPCDs_[ idx ] ) )
                {
                    if ( idx == 1 && elmts.Length == 2 )
                    {
                        if ( !int.TryParse( elmts[ 1 ], out int num ) ) break;

                        this._fREQ_ = num;

                        this._eQUE_.Clear();
                    }
                    else if ( idx == 2 && this._fREQ_ > 0 )
                    {
                        for ( int eIdx = 1; eIdx < elmts.Length; eIdx++ ) this._eQUE_.Add( elmts[ eIdx ] );

                        this._fREQ_--;

                        if ( this._fREQ_ == 0 )
                        {
                            Bridge._COPLs_![ idx ]?.Invoke( [ .. this._eQUE_ ] );

                            this._eQUE_ = [];
                        }
                    }
                    else Bridge._COPLs_![ idx ]?.Invoke( elmts );

                    break;
                }
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
        if ( this._DTGM_ == null || this._PSWDs_.Contains( pswd ) ) return;

        this._SYNC_.Start(      );
        this._PSWDs_.Add ( pswd );

        byte[] bPsw = Encoding.UTF8.GetBytes( pswd );

        Task.Run( async () => 
        { 
            await this._DTGM_.SendAsync( bPsw, bPsw.Length, "255.255.255.255", Bridge._dPRT_ );

            if ( this._DTGM_.Available == 0 ) return;

            var rspc = await this._DTGM_.ReceiveAsync();

            string[] elmts = Encoding.UTF8.GetString( rspc.Buffer ).Split( " : " );

            if ( elmts.Length == 2 )
            {
                Server serv = new ( elmts[ 0 ], int.Parse( elmts[ 1 ] ) );

                if ( !this._SERVs_.Contains( serv ) ) this._SERVs_.Add( serv );
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
                await this._DTGM_.SendAsync( pswd, pswd.Length, "255.255.255.255", Bridge._dPRT_ );

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

                        if ( !this._SERVs_.Contains( serv ) ) this._SERVs_.Add( serv );
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
internal sealed class Server_RUNTIME : Runtime
{
    #region PRIVATE  INSTANCE FIELDS

    private List< Client > _CLNTs_ = []           ;

    private TcpListener?   _SERV_  = null         ;
    private string         _PSWD_  = Bridge._DFLT_;

    private int            _fREQ_  = 0            ;

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
    internal      void _SEND_ ( NetworkStream strm, string   pyld  ) 
    {
        byte[] rqst = Encoding.UTF8.GetBytes( $"{ Bridge._OPCDs_[ 0 ] } { Serializer.Splitter } { pyld } { Serializer.Terminator }" );

        strm.Write( rqst, 0, rqst.Length );
    }

    internal      void _FILL_ ( NetworkStream strm, string   pyld  )
    {
        byte[] rqst = [];
        int    idx  = 1 ;

        if ( this._fREQ_ != 0 )
        {
            idx         = 2;
            this._fREQ_--  ;
        }
        rqst = Encoding.UTF8.GetBytes( $"{ Bridge._OPCDs_[ idx ] } { Serializer.Splitter } { pyld } { Serializer.Terminator }" );

        strm.Write( rqst, 0, rqst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt"></param>
    /// <param name = "id"  ></param>
    /// <returns></returns>
    private async Task _COMM_ ( TcpClient     clnt, uint   id                ) 
    {
        this._SYNC_.Start();

        NetworkStream strm = clnt.GetStream(          );
        Client        wrpr = new           ( id, clnt );
        string        rmng = ""                        ;

        this._CLNTs_.Add( wrpr );
        
        while ( this._SYNC_.Continue )
        {
            string[] elmts = []  ;
            string   rqst  = rmng;
            
            while ( elmts.Length == 0 )
            {
                int bytes = await strm.ReadAsync( wrpr.bffr, 0, wrpr.bffr.Length );

                if ( bytes == 0 ) break;

                rqst += Encoding.UTF8.GetString( wrpr.bffr, 0, bytes );

                int rend = rqst.IndexOf( Serializer.Terminator );

                if ( rend != -1 )
                {
                    rmng  = rqst[ ( rend + Serializer.Terminator.Length ) .. ]                                    ;
                    elmts = rqst[ .. rend                                    ].Trim().Split( Serializer.Splitter );
                }
            }
            if ( elmts.Length == 0 ) break;

            if ( elmts.Length <= 1 || !Bridge._OPCDs_.Contains( elmts[ 0 ] ) ) continue;

            for ( int idx = 0; idx < Bridge._OPCDs_.Length; idx++ )
            {
                if ( elmts[ 0 ].Equals( Bridge._OPCDs_[ idx ] ) )
                {
                    Bridge._SOPLs_![ idx ]?.Invoke( strm, elmts );

                    break;
                }
            }
        }
        this._CLNTs_.Remove( wrpr );
        clnt.Close         (      );
        this._SYNC_.Stop   (      );
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
                        )} : { Bridge._cPRT_ }" 
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
        this._SYNC_.Start (                              );
        this._SERV_ = new ( IPAddress.Any, Bridge._cPRT_ );
        this._SERV_.Start();

        Task.Run( async () => {
            uint id = default;

            while ( this._SYNC_.Continue )
            {
                TcpClient clnt = await this._SERV_.AcceptTcpClientAsync();

                _  = this._COMM_( clnt, id );
                id++                        ;
            }
            this._SYNC_.Stop();
        });
    }

    #endregion
}

internal sealed class Client 
{
    #region INTERNAL PROPERTIES

    internal TcpClient _CONNECTION_ { get; private set; }
    internal byte[]        bffr = new byte[ 1024 ] ;
    internal uint id = 0;

    #endregion

    internal Client ( uint ID,  TcpClient connection )
    {
        this.id = ID;
        this._CONNECTION_ = connection;
    }
    /*
    internal uint      _PASSWORD_   { get; private set; } = uint.MinValue;

    #endregion
    #region PRIVATE  PROPERTIES

    private uint _ROLE_; // TODO;

    #endregion
    #region PUBLIC   PROPERTIES

    public uint Username { get; private set; } = uint.MinValue;
    public int  tOffset  { get;         set; }

    #endregion
    #region INTERNAL CONSTRUCTOR

    /// <summary>
    /// Description :
    ///     Initializes the client with a connection.
    /// </summary>
    /// <param name="iID"></param>
    /// <param name="connection"></param>
    internal Client ( uint iID, TcpClient connection ) 
    {
        if ( iID != Bridge._AUTHENTICATOR_ ) return;

        this._CONNECTION_ = connection ;
    }

    #endregion
    #region PUBLIC   CONSTRUCTOR

    /// <summary>
    /// Description :
    ///     Initializes the client with a username and password.
    /// </summary>
    /// <param name="iID"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public Client ( uint iID, uint username, uint password ) 
    {
        this._CONNECTION_ = null               ;

        this.Username     = username           ;

        this.Set_Password( iID, uint.MinValue, password );
    }

    #endregion
    #region PRIVATE  SECURITY

    /// <summary>
    /// Description :
    ///     Encrypts the given value using a hash operation.
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    private static uint Encrypt ( uint password ) 
    {
        // TODO;

        return uint.MinValue; // TEMP
    }

    #endregion
    #region INTERNAL ACCESSORS

    /// <summary>
    /// Description :
    ///     Sets the password of the client if given the correct authenticator code and password.
    /// </summary>
    /// <param name="iID"></param>
    /// <param name="password"></param>
    /// <param name="npassword"></param>
    internal void Set_Password ( uint iID, uint password, uint npassword ) 
    {
        this._PASSWORD_ = (
            this.Is_Password( iID, password ) ? Encrypt( password ) : 
            this._PASSWORD_ 
        );
    }

    /// <summary>
    /// Description :
    ///     Verifies whether the password is the client password if given the correct authenticator code.
    /// </summary>
    /// <param name="iID"></param>
    /// <returns></returns>
    internal bool Is_Password( uint iID, uint password )
    {
        if (iID != Bridge._AUTHENTICATOR_ || Encrypt( password ) != this._PASSWORD_ ) return false;
        return true;
    }

    /// <summary>
    /// Description :
    ///     Sets the username of the client if given the correct authenticator code and password.
    /// </summary>
    /// <param name = "iID"></param>
    /// <param name = "password"></param>
    /// <param name = "username"></param>
    internal void Set_Username ( uint iID, uint password, uint username  ) 
    {
        if ( iID != Bridge._AUTHENTICATOR_ || password != this._PASSWORD_ ) return;

        this.Username = username;
    }

    #endregion
    */
}

internal sealed class Server ( string ip, int port )
{
    internal string ip   = ip  ;
    internal int    port = port;

    public override bool Equals      ( object? obj ) => obj is Server serv && this.ip == serv.ip && this.port == serv.port;
    public override int  GetHashCode (             ) => HashCode.Combine( this.ip, this.port );
}

/// <summary>
/// 
/// </summary>
public class Bridge 
{
    #region PUBLIC   ENUMS

    public enum Mode
    {
        NMD,
        CNT,
        SRV
    }

    #endregion
    #region INTERNAL STATIC FIELDS

    internal readonly static int      _dPRT_  = 7000     ;
    internal readonly static int      _cPRT_  = 6000     ;

    internal readonly static string   _DFLT_  = "LC-SERV";

    internal readonly static string[] _OPCDs_ = 
    {
        "SND",
        "FIL",
        "NXT",
        "REQ",
        "UPD",
        "DEL"
    };

    #endregion
    #region INTERNAL STATIC PROPERTIES

    internal static List < Action <                string[] >? >? _COPLs_ { get; private set; }
    internal static List < Action < NetworkStream, string[] >? >? _SOPLs_ { get; private set; }

    #endregion
    #region PRIVATE  STATIC FIELDS

    private static Runtime? _RNTM_ = null;

    #endregion

    public static bool Active => _RNTM_ != null;

    #region PUBLIC   STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    public static void Initialize ( Mode mode ) 
    {
        switch ( mode )
        {
            case Mode.CNT : 
                _RNTM_  = new Client_RUNTIME()                  ;
                _COPLs_ = [ null, null, null, null, null, null ];
            break;

            case Mode.SRV : 
                _RNTM_  = new Server_RUNTIME()                  ; 
                _SOPLs_ = [ null, null, null, null, null, null ];
            break;

            default : return;
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public static void Start () => _RNTM_?._STRT_();

    /// <summary>
    /// 
    /// </summary>
    public static void Stop  () => _RNTM_?._STOP_();

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "opcd"></param>
    /// <param name = "func"></param>
    public static void Add ( string opcd, Action < string[] > func )
    {
        if ( !_OPCDs_.Contains( opcd ) ) return;

        if ( _RNTM_ is Client_RUNTIME && _COPLs_ != null ) _COPLs_[ Array.IndexOf( _OPCDs_, opcd ) ] = func;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "opcd"></param>
    /// <param name = "func"></param>
    public static void Add ( string opcd, Action < NetworkStream, string[] > func )
    {
        if ( !_OPCDs_.Contains( opcd ) ) return;

        if ( _RNTM_ is Server_RUNTIME && _SOPLs_ != null ) _SOPLs_[ Array.IndexOf( _OPCDs_, opcd ) ] = func;
    }

    #region CLIENT SPECIFIC

    public static void Search ( string pswd )
    {
        if ( _RNTM_ is Client_RUNTIME rntm ) rntm._STRT_( pswd );
    }

    public static bool Connect ( int idx )
    {
        if ( _RNTM_ is Client_RUNTIME rntm ) return rntm._CNCT_( idx );

        return false;
    }

    public static void Send     ( string pyld ) 
    {
        if ( _RNTM_ is Client_RUNTIME rntm ) rntm._SEND_(_OPCDs_[ 0 ], pyld );
    }

    public static void Send     ( NetworkStream strm, string pyld ) 
    {
        if ( _RNTM_ is Server_RUNTIME rntm ) rntm._SEND_( strm, pyld );
    }

    public static void Fill     ( NetworkStream strm, string pyld )
    {
        if ( _RNTM_ is Server_RUNTIME rntm ) rntm._FILL_( strm, pyld );
    }
    #endregion

    public static void Request ( string pyld )
    {
        if ( _RNTM_ is Client_RUNTIME rntm ) rntm._SEND_(_OPCDs_[ 3 ], pyld );
    }
}