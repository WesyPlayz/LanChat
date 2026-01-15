#region GENERAL HEADER

using System.Net.Sockets;
using System.Runtime.InteropServices;

using System.Windows;

#endregion

namespace LanChat.NetworkSystem;

public enum Mode { IDL  , SRV , HST   }
public enum Role { NOMAD, USER, ADMIN }

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

    internal Client ( uint iID, TcpClient connection ) 
    {
        if ( iID != Bridge._AUTHENTICATOR_ ) return;

        this._CONNECTION_ = connection ;
    }

    #endregion
    #region PUBLIC   CONSTRUCTOR

    public Client ( uint iID, uint username, uint password ) 
    {
        this._CONNECTION_ = null               ;

        this.Username     = username           ;

        this.Set_Password( iID, uint.MinValue, password );
    }

    #endregion
    #region PRIVATE  SECURITY

    private static uint Encrypt ( uint password ) 
    {
        // TODO;

        return uint.MinValue; // TEMP
    }
    private static uint Decrypt ( uint password ) 
    {
        // TODO;

        return uint.MinValue; // TEMP
    }

    #endregion
    #region INTERNAL ACCESSORS

    internal void Set_Password ( uint iID, uint password, uint npassword ) 
    {
        if ( iID != Bridge._AUTHENTICATOR_ || password != this._PASSWORD_ ) return;

        this._PASSWORD_ = Encrypt( npassword );
    }
    internal uint Get_Password ( uint iID                                ) 
    {
        if ( iID != Bridge._AUTHENTICATOR_ ) return uint.MinValue;

        return Decrypt( this._PASSWORD_ );
    }

    internal void Set_Username ( uint iID, uint password, uint username  ) 
    {
        if ( iID != Bridge._AUTHENTICATOR_ || password != this._PASSWORD_ ) return;

        this.Username = username;
    }

    #endregion
}

public static class Bridge
{
    #region INTERNAL PROPERTIES

    internal static uint _AUTHENTICATOR_ { get; private set; } = uint.MinValue;

    #endregion
    #region PRIVATE  PROPERTIES

    private static Mode _RUNTIME_ = Mode.IDL;

    #endregion
    #region PRIVATE  DLLIMPORTS

    [ DllImport( "kernel32.dll" )]
    private static extern bool AllocConsole ();

    #endregion
    #region PUBLIC   INITIALIZATION

    public static void Init_Authentication (           ) 
    {
        if (_AUTHENTICATOR_ != uint.MinValue) return; 
        
        _AUTHENTICATOR_ = ( uint )( new Random().NextInt64( 4000000000, uint.MaxValue ));
    }
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

    private static void Runtime_SRV () 
    {
        Application.Current.MainWindow.Hide();
        AllocConsole();
    }
    private static void Runtime_HST () 
    {
        // TODO;
    }
    private static void Runtime_LAN () 
    {
        // TODO;
    }

    #endregion
}