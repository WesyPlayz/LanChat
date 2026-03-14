/// AUTHOR    : Ryan L Harding
/// 
/// UPDATED   : 3/11/2026 22:41
/// 
/// REMAINING : ALL ( SUBJECT TO FILL )

#region GENERAL HEADER

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LanChat.SubSystem.Messaging;
using LanChat.SubSystem.Network;
using LanChat.SubSystem.Scheduling;



#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Serialization;
using LanChat.SubSystem.UserInterface;

#endregion

namespace LanChat.SubSystem.Authentication;

// STATIC CLASSES //

/// <summary>
/// 
/// </summary>
public static class Registry 
{
    private static Dictionary < string, User > _LIST_ = []   ;

    private static Action < User[] >?          _ADD_  = null ;

    private static Account                     _ACC_  = null!;

    private static bool actv = false;

    #region PUBLIC   STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "mode"></param>
    public static void Initialize ( Bridge.Mode mode ) 
    {
        if ( !Bridge.Active ) return;

        switch ( mode )
        {
            case Bridge.Mode.CNT :
                _ACC_ = new User();

                Bridge.Bind( Bridge.NXT, _NEXT_CLIENT_ );
            break;

            case Bridge.Mode.SRV :
                Bridge.Bind( Bridge.SND, _SEND_SERVER_ );
                Bridge.Bind( Bridge.REQ, _RQST_SERVER_ );
            break;
        }
    }

    #endregion

    public static void Start () 
    {
        if ( actv ) return;

        actv = true;

        Task.Run( async () =>
        {
            while ( actv )
            {
                Request( "CLIENTS" );
                foreach (User user in _LIST_.Values) Console.WriteLine(user.ToString());
                if ( _LIST_.Count != 0 ) _ADD_?.Invoke( [ .. _LIST_.Values ] );

                await Task.Delay( 500 );
            }
        });
    }

    public static void Stop ()
    {
        actv = false;
    }

    #region PUBLIC   STATIC VALIDATION

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "urnm"></param>
    /// <param name = "pswd"></param>
    /// <returns></returns>
    public static bool Authenticate ( string urnm, string pswd ) 
    {
        if ( _LIST_.ContainsKey( urnm ) )
        {
            if ( !_LIST_[ urnm ].Authenticate( pswd ) ) return false;

            _ACC_.Active = true;

            Send( _ACC_.ToString() );
        }
        if ( _ACC_ == null ) return false;

        _ACC_.Username = urnm;
        _ACC_.Set( pswd );
        _ACC_.Active = true;

        Send( _ACC_.ToString() );

        return true;
    }

    public static void Leave ( )
    {
        _ACC_.Active = false;

        Send( _ACC_.ToString() );
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
    #region PUBLIC   STATIC MODIFIERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "func"></param>
    public static void Bind   ( Action < User[] > func ) => _ADD_ = func;

    /// <summary>
    /// 
    /// </summary>
    public static void Unbind (                        ) => _ADD_ = null;

    #endregion
    #region PRIVATE  STATIC EVENTS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "elmts"></param>
    private static void _NEXT_CLIENT_ (              string[] elmts ) 
    {
        if ( elmts.Length == 1 && elmts[ 0 ] == "NULL" ) return;
        else if ( elmts.Length < 4 ) return;

        if ( elmts[ 0 ] != "LIST" ) return;

        Console.WriteLine("L");

        _LIST_.Clear();
        
        for ( int idx = 1; idx < elmts.Length; idx += 4 )
        {
            if ( !uint.TryParse( elmts[ idx + 1 ], out uint pswd ) || !uint.TryParse( elmts[ idx + 2 ], out uint auth ) || !bool.TryParse( elmts[ idx + 3 ], out bool active ) ) continue;
            
            User user = new ()
            {
                _ATCR_ = auth,
                Username = elmts[ idx ],
                _PSWD_ = pswd,
                Active = active
            };
            _LIST_[ elmts[ idx ] ] = user;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt" ></param>
    /// <param name = "elmts"></param>
    private static void _SEND_SERVER_ ( Client clnt, string[] elmts ) 
    {
        if ( elmts.Length != 4 || !uint.TryParse( elmts[ 1 ], out uint pswd ) || !uint.TryParse( elmts[ 2 ], out uint auth ) || !bool.TryParse( elmts[ 3 ], out bool active ) ) return;

        if ( !_LIST_.ContainsKey(elmts[ 0 ] ) )
        {
            User user = new ()
            {
                _ATCR_ = auth,
                Username = elmts[ 0 ],
                _PSWD_ = pswd,
                Active = active
            };
            _LIST_[ elmts[ 0 ] ] = user;
        }
        else
        {
            _LIST_[elmts[ 0 ] ].Active = active;
        }
    }

    private static bool _COLL_ = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "clnt" ></param>
    /// <param name = "elmts"></param>
    private static void _RQST_SERVER_ ( Client clnt, string[] elmts ) 
    {
        if ( _COLL_ || elmts.Length != 1 ) return;

        _COLL_ = true;

        switch ( elmts[ 0 ] )
        {
            case "CLIENTS" :
                Console.WriteLine(_LIST_.Count);
                if ( _LIST_.Count == 0 )
                {
                    Bridge.Fill( clnt, "1"   , Messager._NAME_, "RQSTSERVER" );
                    Bridge.Fill( clnt, "NULL", Messager._NAME_, "RQSTSERVER" );

                    break;
                }
                Bridge.Fill( clnt, ( _LIST_.Count + 1 ).ToString(), Messager._NAME_, "RQSTSERVER" );
                Bridge.Fill( clnt, "LIST"                         , Messager._NAME_, "RQSTSERVER" );
                
                foreach ( User user in _LIST_.Values ) Bridge.Fill( clnt, user.ToString(), Messager._NAME_, "RQSTSERVER" );
            break;

            default : break;
        }
        _COLL_ = false;
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public static class Renderer  
{
    #region PRIVATE STATIC FIELDS

    private static User[]       _LIST_ = []   ;

    private static ScrollViewer _SCRL_ = null!;
    private static StackPanel   _ACTV_ = null!;
    private static StackPanel   _INAC_ = null!;
    private static DataTemplate _uTMP_ = null!;

    #endregion

    #region PUBLIC  STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
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
    /// 
    /// </summary>
    /// <param name = "usrs"></param>
    private static void _LOD_ ( User[] usrs ) 
    {
        _LIST_ = usrs;

        _DSPY_();
    }

    #endregion
    #region PUBLIC  STATIC FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    public static void Start () => Registry.Bind  ( _LOD_ );

    /// <summary>
    /// 
    /// </summary>
    public static void Stop  () => Registry.Unbind(       );

    #endregion
    #region PRIVATE STATIC FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    private static void _DSPY_ () 
    {
        Console.WriteLine("E");
        if ( _SCRL_ == null || _ACTV_ == null || _INAC_ == null || _uTMP_ == null ) return;
        Console.WriteLine("F");

        if ( _LIST_ != null )
        {
            Application.Current.Dispatcher.Invoke( () =>
            {
                _ACTV_.Children.Clear();
                _INAC_.Children.Clear();

                foreach ( User user in _LIST_ )
                {
                    Console.WriteLine( user.ToString() );

                    FrameworkElement elmt = ( FrameworkElement )_uTMP_.LoadContent();

                    Border?    stts = ( Border?    )elmt.FindName ( "Status" );
                    TextBlock? labl = ( TextBlock? )stts?.FindName( "Label"  );

                    elmt.DataContext = user;

                    if ( user.Active )
                    {
                        if ( stts != null && labl != null )
                        {
                            stts.Background = Prefabs.Get_Brush( "Secondary1" );
                            labl.Text       = "Active";
                        }
                        _ACTV_.Children.Add( elmt );
                    }
                    else
                    {
                        if ( stts != null && labl != null )
                        {
                            stts.Background = Prefabs.Get_Brush( "Secondary2" );
                            labl.Text       = "Inactive";
                        }
                        _INAC_.Children.Add( elmt );
                    }

                    _SCRL_.UpdateLayout();
                }
            });
        }
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public static class Storage   
{
    // TODO;
}

// ABSTRACT CLASSES //

/// <summary>
/// 
/// </summary>
public abstract class Account  
{
    #region PROTECTED INSTANCE FIELDS

    internal uint _ATCR_;
    internal uint _PSWD_ { get; set; }

    #endregion
    #region PUBLIC    INSTANCE PROPERTIES

    public string Username { get; set; } = ""   ;

    public bool   Active { get; set; } = false;

    #endregion

    #region INTERNAL  INSTANCE COMPUTED

    /// <summary>
    /// 
    /// </summary>
    internal uint Authenticator => this._ATCR_;

    #endregion

    #region INTERNAL  ABSTRACT FUNCTIONS

    internal abstract void _AUTH_ ();

    #endregion
    #region PUBLIC    OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"{ this.Username }{ Serializer.SPLITTER }{ this._PSWD_ }{ Serializer.SPLITTER }{ this._ATCR_ }{ Serializer.SPLITTER }{ this.Active }";

    #endregion
    #region PUBLIC INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pswd"></param>
    public void Set( string pswd )
    {
        this._PSWD_ = Hash( pswd ) * this._ATCR_;
    }

    #endregion
    #region PROTECTED INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal void _iACV_ ()
    {

    }

    #endregion

    internal uint Hash(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        byte[] bytes  = Encoding.UTF8.GetBytes(input);
        byte[] hash   = sha.ComputeHash(bytes);

        return BitConverter.ToUInt32(hash, 0);
    }
}

// SEALED CLASSES //

/// <summary>
/// 
/// </summary>
public sealed   class User     : Account
{
    // CONSTRUCTORS //

    /// <summary>
    /// 
    /// </summary>
    public User () => this._AUTH_();

    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 1, 10 ) );
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "pswd"></param>
    /// <returns></returns>
    public bool Authenticate ( string pswd ) 
    {
        if ( Hash( pswd ) * _ATCR_ == _PSWD_ ) return true;

        return false;
    }
}

/// <summary>
/// 
/// </summary>
public sealed   class Admin    : Account
{
    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 4000000000, uint.MaxValue ) );
    }

    #endregion
}
