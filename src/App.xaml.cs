/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 3/18/2026 06:28
///
/// REMAINING : FINISHED ( SUBJECT TO UPDATE )

#region GENERAL HEADER

using System.Windows;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Collections;
using LanChat.SubSystem.UserInterface;

#endregion

/// CONTENTS  :
///
/// App^Application  - CLAS [ LCRT : 00-00 ]
/// ^   Focus        - ENUM
/// ^   Runtime      - ENUM
/// ^   ENVIRONMENTS - DICT
/// ^   App      ()  - FUNC [ LCRT : 00-01 ]
/// ^   OnStartup()  - FUNC [ LCRT : 00-02 ]
/// ^   _ENTR_   ()  - FUNC [ LCRT : 00-03 ]
/// ^   _EXIT_   ()  - FUNC [ LCRT : 00-04 ]
///
namespace LanChat.Runtime;

/// <summary>
///     <para><b>ID :</b> [ LCRT : 00-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         App is the entry point for the LanChat application, initializing WPF prefabs and managing window entry and exit.
///     </para>
/// </summary>
public partial class App : Application 
{
    #region INTERNAL  ENUMS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         Focus helps identify how much weight a window has which influences whether exiting it may trigger local shutdown, or application shutdown.
    ///     </para>
    /// </summary>
    internal enum Focus   
    {
        NIL, // No      Focus
        PTL, // Partial Focus
        FUL  // Full    Focus
    }

    /// <summary>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         Runtime helps identify what runtime environment a window occupies whether that be idle, client, local-host, or server.
    ///     </para>
    /// </summary>
    internal enum Runtime 
    {
        IDL, // Idle       runtime;
        CNT, // Client     runtime;
        HST, // Local-Host runtime;
        SRV  // Server     runtime;
    }

    #endregion

    #region INTERNAL  STATIC   FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         Environments routes incoming runtime identification to lambdas which initialize the respective window.
    ///     </para>
    /// </summary>
    internal static Readonly_Collection < Runtime, Func < App, Window > > ENVIRONMENTS = new ( 
        new Dictionary < Runtime, Func < App, Window > >
    {
        { Runtime.IDL, static Window ( App app ) => new InitWindow() },
        { Runtime.CNT, static Window ( App app ) => new MainWindow() },
        { Runtime.HST, static Window ( App app ) => new MainWindow() },
        { Runtime.SRV, static Window ( App app ) => new TermWindow() }
    });

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    ///     <para><b>ID : </b>[ LCRT : 00-01 ]</para>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         The app constructor initializes the app object and the WPF prefabs collection.
    ///     </para>
    /// </summary>
    private App () : base () { Prefabs.Initialize(); SubSystem.Debugging.Debug.AllocConsole(); }

    // FUNCTIONS //

    #region PROTECTED OVERRIDE FUNCTIONS

    /// <summary>
    ///     <para><b>ID : </b>[ LCRT : 00-02 ]</para>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         OnStartup invokes on startup of the application and initializes the default window.
    ///     </para>
    /// </summary>
    protected override void OnStartup ( StartupEventArgs _ ) => this._ENTR_();

    #endregion
    #region INTERNAL  INSTANCE WINDOW    MANAGEMENT

    /// <summary>
    ///     <para><b>ID : </b>[ LCRT : 00-03 ]</para>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         _ENTR_ closes the current window if applicable, and initializes the window associated with the selected runtime identity.
    ///     </para>
    /// </summary>
    /// <param name = "rntm"></param>
    /// <param name = "win" ></param>
    internal void _ENTR_ ( Runtime rntm = Runtime.IDL, Window? win = null ) 
    {
        if ( win != null && rntm == Runtime.IDL && win is InitWindow ) return;

        win?.Close();

        this.MainWindow = ENVIRONMENTS[ rntm ].Invoke( this );

        this.MainWindow.Show();
    }

    /// <summary>
    ///     <para><b>ID : </b>[ LCRT : 00-04 ]</para>
    ///     <para>
    ///         <b>Description :</b>
    ///         
    ///         _EXIT_ closes the current window if its focus is partial, and shutsdown the application if its focus is full.
    ///     </para>
    /// </summary>
    /// <param name = "focs"></param>
    /// <param name = "win" ></param>
    internal void _EXIT_ ( Focus   focs = Focus.NIL  , Window? win = null ) 
    {
        switch ( focs )
        {
            case    Focus.PTL : win?.Close   (); return; // Partial Shutdown;
            case    Focus.FUL : this.Shutdown(); return; // Full    Shutdown;
            default           :                  return; // No      Shutdown;
        }
    }

    #endregion
}
