/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 14:55
///
/// REMAINING :
///     Client   CLASS
///     Server   CLASS
///     Renderer CLASS

#region GENERAL HEADER

using System.Net.Sockets;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Network.Runtime;

#endregion

namespace LanChat.SubSystem.Network;

public interface iEntity;

public sealed class Client   : iEntity
{
    #region INTERNAL PROPERTIES

    internal TcpClient _CONNECTION_ { get; private set; }
    internal NetworkStream _STRM_ { get; private set; }
    internal byte[]        bffr = new byte[ 1024 ] ;
    internal bool _ACTV_ = false;

    #endregion

    public bool Active => this._ACTV_;

    internal Client ( TcpClient connection )
    {
        this._CONNECTION_ = connection            ;
        this._STRM_       = connection.GetStream();
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

public sealed class Server   ( string ip, int port ) : iEntity
{
    public string Name { get; private set; } = "Server";
    public string Ip   { get; private set; } = ip      ;
    public int    Port { get; private set; } = port    ;

    public override bool Equals      ( object? obj ) => obj is Server serv && this.Ip == serv.Ip && this.Port == serv.Port;
    public override int  GetHashCode (             ) => HashCode.Combine( this.Ip, this.Port );

    public override string ToString ( ) => $"{ this.Ip } : { this.Port }";
}

/// <summary>
/// 
/// </summary>
public sealed class Bridge   
{
    #region PUBLIC   ENUMS

    public enum Mode
    {
        NMD,
        CNT,
        SRV
    }

    #endregion

    #region PUBLIC   STATIC FIELDS

    public readonly static string SND = "SND";
    public readonly static string FIL = "FIL";
    public readonly static string NXT = "NXT";
    public readonly static string REQ = "REQ";

    #endregion
    #region INTERNAL STATIC FIELDS

    internal readonly static int      _dPRT_  = 6000     ;
    internal readonly static string   _DFLT_  = "LC-SERV";

    internal readonly static Dictionary < 
        string, 
        ( Action < string[] >? _cEVNT_, Action < Client, string[] >? _sEVNT_ ) 
    > _OPCDs_ = new ()
    {
        { SND, ( null, null ) },
        { FIL, ( null, null ) },
        { NXT, ( null, null ) },
        { REQ, ( null, null ) }
    };

    #endregion
    #region PRIVATE  STATIC FIELDS

    private static rtEntity? _RNTM_ = null;

    #endregion

    #region PUBLIC   STATIC COMPUTED

    /// <summary>
    /// 
    /// </summary>
    public static bool Active => _RNTM_ != null;

    #endregion

    #region PUBLIC   STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mode"></param>
    public static void Initialize ( Mode mode ) 
    {
        switch ( mode )
        {
            case    Mode.CNT : _RNTM_  = new rtClient(); break ;
            case    Mode.SRV : _RNTM_  = new rtServer(); break ;
            default          :                           return;
        }
    }

    #endregion
    #region PUBLIC   STATIC RUNTIME MANAGEMENT

    /// <summary>
    /// 
    /// </summary>
    public static void Start () => _RNTM_?._STRT_();

    /// <summary>
    /// 
    /// </summary>
    public static void Stop  () => _RNTM_?._STOP_();

    #endregion
    #region PUBLIC   STATiC ACCESSORS

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static iEntity[]? Get () 
    {
        if ( _RNTM_ == null ) return null;

        return [ .. _RNTM_._ETTYs_ ];
    }

    #endregion
    #region PUBLIC   STATIC MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "opcd"></param>
    /// <param name = "func"></param>
    public static void Bind ( string opcd, Action <         string[] > func ) 
    {
        if ( !_OPCDs_.ContainsKey( opcd ) ) return;

        if ( _RNTM_ is rtClient ) _OPCDs_[ opcd ] = ( func, null );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "opcd"></param>
    /// <param name = "func"></param>
    public static void Bind ( string opcd, Action < Client, string[] > func ) 
    {
        if ( !_OPCDs_.ContainsKey( opcd ) ) return;

        if ( _RNTM_ is rtServer ) _OPCDs_[ opcd ] = ( null, func );
    }

    #endregion
    #region PUBLIC   STATIC COMMUNICATORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Send        (              string pyld ) 
    {
        if ( _RNTM_ is rtClient clnt ) clnt._SEND_( SND, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt"></param>
    /// <param name = "pyld"></param>
    public static void Send        ( Client clnt, string pyld ) 
    {
        if ( _RNTM_ is rtServer serv ) serv._SEND_( SND, clnt._STRM_, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Send_All    (              string pyld ) 
    {
        if ( _RNTM_ is rtServer serv ) serv._SEND_( Bridge.SND, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Fill        (              string pyld ) 
    {
        if ( _RNTM_ is rtClient clnt ) clnt._SEND_( FIL, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt"></param>
    /// <param name = "pyld"></param>
    public static void Fill        ( Client clnt, string pyld ) 
    {
        if ( _RNTM_ is rtServer serv ) serv._SEND_( FIL, clnt._STRM_, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Fill_All    (              string pyld ) 
    {
        if ( _RNTM_ is rtServer serv ) serv._SEND_( Bridge.FIL, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Request     (              string pyld ) 
    {
        if ( _RNTM_ is rtClient clnt ) clnt._SEND_( REQ, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt"></param>
    /// <param name = "pyld"></param>
    public static void Request     ( Client clnt, string pyld ) 
    {
        if ( _RNTM_ is rtServer serv ) serv._SEND_( REQ, clnt._STRM_, pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Request_All (              string pyld )
    {
        if ( _RNTM_ is rtServer serv ) serv._SEND_( Bridge.REQ, pyld );
    }

    #endregion

    #region CLIENT SPECIFIC

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pswd"></param>
    public static void Search   ( string pswd ) 
    {
        if ( _RNTM_ is rtClient clnt ) clnt._STRT_( pswd );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "idx"></param>
    /// <returns></returns>
    public static bool Connect ( int     idx  ) 
    {
        if ( _RNTM_ is rtClient clnt ) return clnt._CNCT_( idx ).GetAwaiter().GetResult();

        return false;
    }

    #endregion
}

public sealed class Renderer 
{

}
