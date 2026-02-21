#region GENERAL HEADER

using System.Net.Sockets;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Network.Runtime;

#endregion

namespace LanChat.SubSystem.Network;

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

public sealed class Server ( string ip, int port )
{
    public string Name { get; private set; } = "Server";
    public string Ip   { get; private set; } = ip      ;
    public int    Port { get; private set; } = port    ;

    public override bool Equals      ( object? obj ) => obj is Server serv && this.Ip == serv.Ip && this.Port == serv.Port;
    public override int  GetHashCode (             ) => HashCode.Combine( this.Ip, this.Port );

    public override string ToString ( ) => $"{this.Ip} : {this.Port}";
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

    private static Runtime_Entity? _RNTM_ = null;

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

    public static Server[]? Get_Servers()
    {
        if (_RNTM_ is Client_RUNTIME rntm)
        {
            foreach (Server s in rntm._SERVs_) Console.WriteLine(s.ToString());
            return [.. rntm._SERVs_];
        }
        return null;
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