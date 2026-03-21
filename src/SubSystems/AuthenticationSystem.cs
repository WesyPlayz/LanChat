/// AUTHOR    : Ryan L Harding
/// 
/// UPDATED   : 3/11/2026 22:41
/// 
/// REMAINING : ALL ( SUBJECT TO FILL )

#region GENERAL HEADER

using System.Text;

using System.Security.Cryptography;

using System.Windows;

using System.Windows.Controls;
using System.Windows.Media;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Serialization;
using LanChat.SubSystem.UserInterface;
using LanChat.SubSystem.Messaging;
using LanChat.SubSystem.Network;

#endregion

// CONTENTS :
//
//
//
namespace LanChat.SubSystem.Authentication;

// STATIC CLASSES //

/// <summary>
///     <para><b>ID :</b> [ LCAU : 00-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         
///     </para>
/// </summary>
public static class Registry 
{
    private static bool _COLL_ = false;

    #region INTERNAL CONSTANT FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    internal const string _DFLT_ = "User";

    #endregion
    #region PRIVATE  STATIC   FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static Dictionary < string, Account >  _ACCTs_ = []   ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static Action     < Account[]       >? _ADD_   = null ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static Account                         _ACC_   = null!;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static bool                            _ACTV_  = false;

    #endregion

    // FUNCTIONS //

    #region PUBLIC   STATIC   INITIALIZERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "mode"></param>
    public static void Initialize ( Bridge.Mode mode, string urnm ) 
    {
        if ( !Bridge.Active ) return;

        switch ( mode )
        {
            case Bridge.Mode.CNT :
                _ACC_ = new User( urnm );

                Bridge.Bind( Bridge.NXT, _nCLNT_ );
            break;

            case Bridge.Mode.SRV :
                _ACC_ = new Admin( urnm );

                Bridge.Bind( Bridge.SND, _sSERV_ );
                Bridge.Bind( Bridge.REQ, _rSERV_ );
            break;
        }
    }

    #endregion
    #region PUBLIC   STATIC   MODIFIERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static void Start  (                           ) 
    {
        if ( _ACTV_ ) return;

        _ACTV_ = true;

        Task.Run( async () =>
        {
            while ( _ACTV_ )
            {
                Request( "ACCOUNTS" );

                if ( _ACCTs_.Count != 0 ) _ADD_?.Invoke( [ .. _ACCTs_.Values ] );

                foreach ( Account acct in _ACCTs_.Values ) Console.WriteLine( acct );

                await Task.Delay( 500 );
            }
        });
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static void Stop   (                           ) 
    {
        _ACTV_ = false;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "func"></param>
    public static void Bind   ( Action < Account[] > func ) 
    {
        _ADD_ = func;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static void Unbind (                           ) 
    {
        _ADD_ = null;
    }

    #endregion
    #region PUBLIC   STATIC VALIDATION

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static bool Login  ( string urnm, string pswd ) 
    {
        if ( _ACCTs_.ContainsKey( urnm ) )
        {
            if ( !_ACCTs_[ urnm ].Authenticate( pswd ) ) return false;

            _ACC_.Set( true                                                                        );
            Send     ( $"{ ( _ACC_ is User ? "USER" : "ADMIN" ) }{ Serializer.SPLITTER }{ _ACC_ }" );
        }
        if ( _ACC_ == null ) return false;

        _ACC_.Set( pswd                                                                        );
        _ACC_.Set( true                                                                        );
        Send     ( $"{ ( _ACC_ is User ? "USER" : "ADMIN" ) }{ Serializer.SPLITTER }{ _ACC_ }" );

        return true;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static void Logout (                          ) 
    {
        _ACC_.Set( false                                                                       );
        Send     ( $"{ ( _ACC_ is User ? "USER" : "ADMIN" ) }{ Serializer.SPLITTER }{ _ACC_ }" );
    }

    #endregion
    #region PUBLIC   STATIC COMMUNICATORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pyld"></param>
    public static void Send ( string pyld )
    {
        Bridge.Send( pyld );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "rqst"></param>
    public static void Request ( string rqst ) 
    {
        Bridge.Request( rqst );
    }

    #endregion
    #region PRIVATE  STATIC EVENTS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "elmts"></param>
    private static void _nCLNT_ (              string[] elmts ) 
    {
        if ( elmts[ 0 ] != "LIST" || ( elmts.Length == 1 && elmts[ 0 ] == "NULL" ) || elmts.Length < 5 ) return;

        _ACCTs_.Clear();
        
        for ( int idx = 1; idx < elmts.Length; idx += 5 )
        {
            if ( 
                !uint.TryParse( elmts[ idx + 2 ], out uint pswd ) || 
                !uint.TryParse( elmts[ idx + 3 ], out uint auth ) || 
                !bool.TryParse( elmts[ idx + 4 ], out bool actv ) 
            ) continue;

            Account acct = (
                elmts[ idx ] == "USER"  ? new User () : 
                elmts[ idx ] == "ADMIN" ? new Admin() :
                null!
            );
            if ( acct == null ) continue;

            string urnm = elmts[ idx + 1 ];

            acct._RECN_( urnm, pswd, auth, actv );

            _ACCTs_[ urnm ] = acct;
        }
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "elmts"></param>
    /// <param name = "clnt" ></param>
    /// <param name = "elmts"></param>
    private static void _sSERV_ ( Client clnt, string[] elmts ) 
    {
        if ( 
            elmts.Length != 5                           || 
            !uint.TryParse( elmts[ 2 ], out uint pswd ) || 
            !uint.TryParse( elmts[ 3 ], out uint auth ) || 
            !bool.TryParse( elmts[ 4 ], out bool actv ) 
        ) return;

        string urnm = elmts[ 1 ];

        if ( !_ACCTs_.ContainsKey( urnm ) )
        {
            Account acct = (
                elmts[ 0 ] == "USER"  ? new User () : 
                elmts[ 0 ] == "ADMIN" ? new Admin() :
                null!
            );
            if ( acct == null ) return;

            acct._RECN_( urnm, pswd, auth, actv );

            _ACCTs_[ urnm ] = acct;
        }
        else _ACCTs_[ urnm ].Set( actv );
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "elmts"></param>
    /// <param name = "clnt" ></param>
    /// <param name = "elmts"></param>
    private static void _rSERV_ ( Client clnt, string[] elmts ) 
    {
        if ( _COLL_ || elmts.Length != 1 ) return;

        _COLL_ = true;

        switch ( elmts[ 0 ] )
        {
            case "ACCOUNTS" :
                if ( _ACCTs_.Count == 0 )
                {
                    Bridge.Fill( clnt, "1"   , Messager._NAME_, "RRQSTSERVER" );
                    Bridge.Fill( clnt, "NULL", Messager._NAME_, "RRQSTSERVER" );

                    break;
                }
                Bridge.Fill( clnt, ( _ACCTs_.Count + 1 ).ToString(), Messager._NAME_, "RRQSTSERVER" );
                Bridge.Fill( clnt, "LIST"                          , Messager._NAME_, "RRQSTSERVER" );
                
                foreach ( Account user in _ACCTs_.Values ) Bridge.Fill( 
                    clnt                                                                                , 
                    $"{ ( user is User ? "USER" : "ADMIN" ) }{ Serializer.SPLITTER }{ user.ToString() }", 
                    Messager._NAME_                                                                     , 
                    "RRQSTSERVER" 
                );
            break;

            default : break;
        }
        _COLL_ = false;
    }

    #endregion

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 00-00 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "inpt"></param>
    /// <returns></returns>
    internal static uint Hash ( string inpt ) 
    {
        return BitConverter.ToUInt32( SHA256.HashData( Encoding.UTF8.GetBytes( inpt ) ), 0 );
    }
}

/// <summary>
///     <para><b>ID :</b> [ LCAU : 01-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         
///     </para>
/// </summary>
public static   class Renderer 
{
    #region PRIVATE STATIC FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static Account[]    _ACCTs_ = []   ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static ScrollViewer _SCRL_  = null!;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static StackPanel   _ACTV_  = null!;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static StackPanel   _INAC_  = null!;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static DataTemplate _uTMP_  = null!;

    #endregion

    // FUNCTIONS //

    #region PUBLIC  STATIC INITIALIZERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 01-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "scrl"></param>
    /// <param name = "actv"></param>
    /// <param name = "inac"></param>
    /// <param name = "uTmp"></param>
    public static void Initialize ( ScrollViewer scrl, StackPanel actv, StackPanel inac, DataTemplate uTmp ) 
    {
        _SCRL_ = scrl;
        _ACTV_ = actv;
        _INAC_ = inac;
        _uTMP_ = uTmp;
    }

    #endregion
    #region PRIVATE STATIC MODIFIERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 01-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "accts"></param>
    private static void _LOAD_ ( Account[] accts ) 
    {
        _ACCTs_ = accts;

        _DSPY_();
    }

    #endregion
    #region PUBLIC  STATIC FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 01-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static void Start () 
    {
        if ( _SCRL_ == null || _ACTV_ == null || _INAC_ == null || _uTMP_ == null ) return;

        Registry.Bind( _LOAD_ );
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 01-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public static void Stop  () 
    {
        Registry.Unbind();
    }

    #endregion
    #region PRIVATE STATIC FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 01-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    private static void _DSPY_ () 
    {
        Application.Current.Dispatcher.Invoke( () =>
        {
            _ACTV_.Children.Clear();
            _INAC_.Children.Clear();

            if ( _ACCTs_.Length == 0 ) return;

            foreach ( Account user in _ACCTs_ )
            {
                FrameworkElement elmt = ( FrameworkElement )_uTMP_.LoadContent();

                SolidColorBrush? brsh =               Prefabs.Get_Brush( user.Active ? "Secondary1" : "Secondary2" );

                Border?          stts = ( Border?    )elmt.FindName    ( "Status"                                  );
                TextBlock?       labl = ( TextBlock? )stts?.FindName   ( "Label"                                   );

                if ( brsh == null || stts == null || labl == null ) break;

                elmt.DataContext = user                               ;
                stts.Background  = brsh                               ;
                labl.Text        = user.Active ? "Inactive" : "Active";

                if ( user.Active ) _ACTV_.Children.Add( elmt );
                else               _INAC_.Children.Add( elmt );

                _SCRL_.UpdateLayout();
            }
        });
    }

    #endregion
}

/// <summary>
///     <para><b>ID :</b> [ LCAU : 02-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         
///     </para>
/// </summary>
public static   class Storage  
{
    // TODO;
}

// ABSTRACT CLASSES //

/// <summary>
///     <para><b>ID :</b> [ LCAU : 03-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         
///     </para>
/// </summary>
public abstract class Account  ( string urnm = Registry._DFLT_ )
{
    #region PROTECTED INSTANCE FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    protected uint _ATCR_;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    protected uint _PSWD_;

    #endregion
    #region PUBLIC    INSTANCE PROPERTIES

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public string Username { get; private set; } = urnm ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public bool   Active   { get; private set; } = false;

    #endregion

    #region PROTECTED ABSTRACT FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    protected abstract void _AUTH_ ();

    #endregion
    #region PUBLIC    ABSTRACT FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    public abstract bool Authenticate ( string pswd );

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "pswd"></param>
    public abstract void Set          ( string pswd );

    #endregion
    #region PUBLIC    OVERRIDE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => (
        $"{ this.Username }"                      +
        $"{ Serializer.SPLITTER }{ this._PSWD_ }" +
        $"{ Serializer.SPLITTER }{ this._ATCR_ }" +
        $"{ Serializer.SPLITTER }{ this.Active }"
    );

    #endregion
    #region PUBLIC    INSTANCE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "actv"></param>
    public void Set ( bool actv ) 
    {
        this.Active = actv;
    }

    #endregion
    #region INTERNAL  INSTANCE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-06 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <returns></returns>
    /// <param name = "urnm"></param>
    /// <param name = "pswd"></param>
    /// <param name = "auth"></param>
    /// <param name = "actv"></param>
    internal void _RECN_ ( string urnm, uint pswd           , bool actv ) 
    {
        this.Username = urnm;
        this._PSWD_   = pswd;
        this.Active   = actv;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 03-07 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <returns></returns>
    /// <param name = "urnm"></param>
    /// <param name = "pswd"></param>
    /// <param name = "auth"></param>
    /// <param name = "actv"></param>
    internal void _RECN_ ( string urnm, uint pswd, uint auth, bool actv ) 
    {
        this.Username = urnm;
        this._PSWD_   = pswd;
        this._ATCR_   = auth;
        this.Active   = actv;
    }

    #endregion
}

// SEALED CLASSES //

/// <summary>
///     <para><b>ID :</b> [ LCAU : 04-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         
///     </para>
/// </summary>
public sealed   class User     : Account
{
    // CONSTRUCTORS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 04-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "urnm"></param>
    public User ( string urnm = Registry._DFLT_ ) : base ( urnm ) => this._AUTH_();

    // FUNCTIONS //

    #region PROTECTED OVERRIDE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 04-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    protected override void _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 1, 10 ) );
    }

    #endregion
    #region PUBLIC    OVERRIDE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 04-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "pswd"></param>
    /// <returns></returns>
    public override bool Authenticate ( string pswd ) 
    {
        if ( Registry.Hash( pswd ) * this._ATCR_ == this._PSWD_ ) return true;

        return false;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 04-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "pswd"></param>
    public override void Set          ( string pswd ) 
    {
        this._PSWD_ = Registry.Hash( pswd ) * this._ATCR_;
    }

    #endregion
}

/// <summary>
///     <para><b>ID :</b> [ LCAU : 05-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         
///     </para>
/// </summary>
public sealed   class Admin    : Account
{
    // CONSTRUCTORS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 05-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "urnm"></param>
    public Admin ( string urnm = Registry._DFLT_ ) : base ( urnm ) => this._AUTH_();

    // FUNCTIONS //

    #region PROTECTED OVERRIDE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 05-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    protected override void _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )new Random().NextInt64( 4000000000, uint.MaxValue );
    }

    #endregion
    #region PUBLIC    OVERRIDE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 05-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "pswd"></param>
    /// <returns></returns>
    public override bool Authenticate ( string pswd ) 
    {
        if ( Registry.Hash( pswd ) == this._PSWD_ ) return true;

        return false;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCAU : 05-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///         
    ///     </para>
    /// </summary>
    /// <param name = "pswd"></param>
    public override void Set          ( string pswd ) 
    {
        this._PSWD_ = Registry.Hash( pswd );
    }

    #endregion
}
