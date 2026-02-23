/// AUTHOR    : Ryan L Harding
/// 
/// UPDATED   : 2/22/2026 23:48
/// 
/// REMAINING : FINISHED ( SUBJECT TO UPDATES )

#region GENERAL HEADER

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.UserInterface;

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
    
    // CONSTRUCTORS //

    /// <summary>
    /// Description :
    ///     Initializes a new Initialization Window and fills its application connection to the given application.
    /// </summary>
    /// <param name = "app"></param>
    internal InitWindow ( App app ) 
    {
        this.InitializeComponent();

        this._APP_                   = app                                ;
        this.Style                   = Prefabs.Get_Style( "Fixed_Window" );
        this.Screen_Space.Background = Prefabs.Get_Brush( "Primary6"     );

        this.Initialize();
    }

    #region PRIVATE  INSTANCE INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    private void Initialize () 
    {
        Border? hedr = ( Border? )Prefabs.Get_Template( "Header"        )?.LoadContent();
        Border? lbdr = ( Border? )Prefabs.Get_Template( "Left_Border"   )?.LoadContent();
        Border? bbdr = ( Border? )Prefabs.Get_Template( "Bottom_Border" )?.LoadContent();
        Border? rbdr = ( Border? )Prefabs.Get_Template( "Right_Border"  )?.LoadContent();

        if ( hedr != null )
        {
            hedr.MouseLeftButtonDown += Drag;

            Grid.SetRowSpan   ( hedr, 2 );
            Grid.SetColumnSpan( hedr, 5 );

            Button       mnbt = ( Button       )hedr.FindName( "Minimize"   );
            ToggleButton fsbt = ( ToggleButton )hedr.FindName( "Fullscreen" );
            Button       etbt = ( Button       )hedr.FindName( "Exit"       );

            if ( fsbt != null )
            {
                Grid parent = ( Grid )fsbt.Parent;

                parent.Children.Remove( fsbt );
            }
            if ( mnbt != null ) mnbt.Click += Minimize  ;
            if ( etbt != null ) etbt.Click += Exit      ;

            this.Screen_Space.Children.Add( hedr );
        }
        if ( lbdr != null )
        {
            Grid.SetRow    ( lbdr, 2 );
            Grid.SetRowSpan( lbdr, 4 );

            this.Screen_Space.Children.Add( lbdr );
        }
        if ( bbdr != null )
        {
            Grid.SetRow       ( bbdr, 5 );
            Grid.SetColumn    ( bbdr, 1 );
            Grid.SetColumnSpan( bbdr, 3 );

            this.Screen_Space.Children.Add( bbdr );
        }
        if ( rbdr != null )
        {
            Grid.SetRow    ( rbdr, 2 );
            Grid.SetRowSpan( rbdr, 4 );
            Grid.SetColumn ( rbdr, 4 );

            this.Screen_Space.Children.Add( rbdr );
        }
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
