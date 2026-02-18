/// AUTHOR  : Ryan L Harding
/// 
/// UPDATED : 2/17/2026 14:29

#region GENERAL HEADER

using System.Windows;
using System.Windows.Input;

#endregion

namespace LanChat.Runtime;

/// <summary>
/// 
/// </summary>
public partial class InitWindow : Window
{
    #region PRIVATE  INSTANCE PROPERTIES

    private App _APP_ { get; set; }

    #endregion
    #region PRIVATE  STATIC   PROPERTIES

    private static string _cDES_ = "\tClient"    ;
    private static string _lDES_ = "\tLocal Host";
    private static string _sDES_ = "\tServer"    ;

    #endregion
    #region INTERNAL INSTANCE CONSTRUCTOR

    /// <summary>
    /// Description :
    ///     Initializes a new Initialization Window and fills its application connection to the given application.
    /// </summary>
    /// <param name = "app"></param>
    internal InitWindow ( App app ) 
    {
        InitializeComponent();

        this._APP_ = app;
    }

    #endregion
    #region PRIVATE  INSTANCE TRANSFORMERS

    /// <summary>
    /// Description :
    ///     While mouse button is held, the window follows the cursor.
    /// </summary>
    /// <param name = "input"></param>
    private void Drag ( object _, MouseButtonEventArgs input ) 
    { 
        if ( input.ButtonState == MouseButtonState.Pressed ) DragMove(); 
    }

    /// <summary>
    /// Description :
    ///     Minimizes the window.
    /// </summary>
    private void Minimize ( object _, RoutedEventArgs __ ) => this.WindowState = WindowState.Minimized;

    /// <summary>
    /// Description :
    ///     Closes the current window and shutsdown the current application.
    /// </summary>
    private void Exit     ( object _, RoutedEventArgs __ ) => this._APP_._EXIT_( App.Integration.FUL, this );

    #endregion
    #region PRIVATE  BUTTON   EVENTS

    // HOVER EVENTS //

    /// <summary>
    /// Description :
    ///     Displays the description of the client runtime while a button is hovered over.
    /// </summary>
    private void Client_HVR     ( object _, MouseEventArgs __ ) => this.Description.Text = _cDES_;

    /// <summary>
    /// Description :
    ///     Displays the description of the local host runtime while a button is hovered over.
    /// </summary>
    private void Local_Host_HVR ( object _, MouseEventArgs __ ) => this.Description.Text = _lDES_;

    /// <summary>
    /// Description :
    ///     Displays the description of the server runtime while a button is hovered over.
    /// </summary>
    private void Server_HVR     ( object _, MouseEventArgs __ ) => this.Description.Text = _sDES_;

    // PRESS EVENTS //

    /// <summary>
    /// Description :
    ///     Initializes client runtime by switching to the Main Window on button clicked.
    /// </summary>
    private void Client     ( object _, RoutedEventArgs __ ) => this._APP_._MAIN_( this                    );

    /// <summary>
    /// Description :
    ///     Initializes local host runtime by switching to the Main Window with an integrated Terminal on button clicked.
    /// </summary>
    private void Local_Host ( object _, RoutedEventArgs __ ) => this._APP_._MAIN_( this, App.Extention.TER );

    /// <summary>
    /// Description :
    ///     Initializes server runtime by switching to the Terminal Window on button clicked.
    /// </summary>
    private void Server     ( object _, RoutedEventArgs __ ) => this._APP_._TERM_( this                    );

    #endregion
}