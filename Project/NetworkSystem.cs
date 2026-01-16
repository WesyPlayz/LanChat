#region GENERAL HEADER

using System.Net.Sockets;
using System.Runtime.InteropServices;

using System.Windows;

#endregion

namespace LanChat.NetworkSystem;

public enum Mode { IDL  , SRV , HST   }
public enum Role { NOMAD, USER, ADMIN }

/// <summary>
/// Description :
///     Manages access to and storage of the data and connection of a client.
/// </summary>
public sealed class Client 
{
    #region INTERNAL PROPERTIES

    internal TcpClient _CONNECTION_ { get; private set; }
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
}

/// <summary>
/// Description :
///     Manages the bridge between client-server/localhost communication during runtime.
/// </summary>
public static class Bridge 
{
    #region INTERNAL PROPERTIES

    internal static uint _AUTHENTICATOR_ { get; private set; } = uint.MinValue;

    #endregion
    #region PRIVATE  PROPERTIES

    private static Mode _RUNTIME_ = Mode.IDL;

    #endregion
    #region PRIVATE  DLLIMPORTS

    /// <summary>
    /// Description :
    ///     Initializes a console interface window.
    /// </summary>
    /// <returns></returns>
    [ DllImport( "kernel32.dll" )]
    private static extern bool AllocConsole ();

    #endregion
    #region PUBLIC   INITIALIZATION

    /// <summary>
    /// Description :
    ///     Initializes the authenticator code used for data security authenticity.
    /// </summary>
    public static void Init_Authentication (           ) 
    {
        if (_AUTHENTICATOR_ != uint.MinValue) return; 
        
        _AUTHENTICATOR_ = ( uint )( new Random().NextInt64( 4000000000, uint.MaxValue ));
    }

    /// <summary>
    /// Description :
    ///     Initializes the given network mode.
    /// </summary>
    /// <param name = "mode"></param>
    public static void Init_Network        ( Mode mode ) 
    {
        switch ( mode )
        {
            case Mode.SRV :  
                if ( _RUNTIME_ == Mode.SRV ) break;

                _RUNTIME_ = Mode.SRV;
                Runtime_SRV();
                Runtime_LAN();
            break;

            case Mode.HST : 
                if ( _RUNTIME_ == Mode.HST ) break;

                _RUNTIME_ = Mode.HST;
                Runtime_HST();
                Runtime_LAN();
            break;

            case Mode.IDL : 
                if ( _RUNTIME_ == Mode.IDL ) break;

                _RUNTIME_ = Mode.IDL;
            break;

            default : break;
        }
    }

    #endregion
    #region PRIVATE  RUNTIME

    /// <summary>
    /// Description :
    ///     Initializes a server runtime environment utilizing a 
    /// console interface window to retrieve text input commands.
    /// </summary>
    private static void Runtime_SRV () 
    {
        Application.Current.MainWindow.Hide();
        AllocConsole();
    }

    /// <summary>
    /// Description :
    ///     Initializes a localhost runtime environment alongside 
    /// the window application to resolve client-localhost communication.
    /// </summary>
    private static void Runtime_HST () 
    {
        // TODO;
    }

    /// <summary>
    /// Description :
    ///     Initializes a lan runtime environment alongside the 
    /// window application to catch any connecting clients.
    /// </summary>
    private static void Runtime_LAN () 
    {
        // TODO;
    }

    #endregion
}