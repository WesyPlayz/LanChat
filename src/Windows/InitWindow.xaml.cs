/// AUTHOR    : Ryan L Harding
/// 
/// UPDATED   : 3/18/2026 05:09
/// 
/// REMAINING : FINISHED ( SUBJECT TO UPDATE )

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

/// CONTENTS  :
///
/// InitWindow^Window          - CLAS [ LCRT : 01-00 ]
/// ^   INSTRUCTION            - CNST
/// ^   CLIENT_DESCRIPTION     - CNST
/// ^   LOCAL_HOST_DESCRIPTION - CNST
/// ^   SERVER_DESCRIPTION     - CNST
/// ^   _DCRT_                 - FELD
/// ^   InitWindow()           - FUNC [ LCRT : 01-01 ]
/// ^   _INIT_    ()           - FUNC [ LCRT : 01-02 ]
/// ^   _EXIT_    ()           - FUNC [ LCRT : 01-03 ]
/// ^   _DRAG_    ()           - EVNT [ LCRT : 01-04 ]
/// ^   _MNMZ_    ()           - EVNT [ LCRT : 01-05 ]
/// ^   _EXIT_    ()           - EVNT [ LCRT : 01-06 ]
/// ^   _HOVR_    ()           - EVNT [ LCRT : 01-07 ]
/// ^   _SLCT_    ()           - EVNT [ LCRT : 01-08 ]
///
namespace LanChat.Runtime;

/// <summary>
///     <para><b>ID :</b> [ LCRT : 01-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
///         InitWindow is the default window of the application, used to determine which runtime the user wants between Client, Local-Host, and Server.
///     </para>
/// </summary>
public partial class InitWindow : Window
{
    #region PRIVATE  CONSTANT FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         Instruction is used to instruct the user what to do.
    ///     </para>
    /// </summary>
    private const string     INSTRUCTION        = "Pick a process to run LanChat on.";

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         Client_Description is used to describe the Client runtime environment.
    ///     </para>
    /// </summary>
    private const string     CLIENT_DESCRIPTION = "\tClient"    ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         Local_Host_Description is used to describe the Local-Host runtime environment.
    ///     </para>
    /// </summary>
    private const string LOCAL_HOST_DESCRIPTION = "\tLocal Host";

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         Server_Description is used to describe the Server runtime environment.
    ///     </para>
    /// </summary>
    private const string     SERVER_DESCRIPTION = "\tServer"    ;

    #endregion
    #region PRIVATE  INSTANCE FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _DCRT_ contains the description of a runtime environment.
    ///     </para>
    /// </summary>
    private TextBlock _DCRT_ = null!;

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         The InitWindow constructor initializes the windows contents and styling.
    ///     </para>
    /// </summary>
    /// <param name = "app"></param>
    internal InitWindow () 
    {
        this.InitializeComponent();

        Style?           styl = Prefabs.Get_Style( "Fixed_Window" );

        SolidColorBrush? brsh = Prefabs.Get_Brush( "Primary6"     );

        if ( styl == null || brsh == null ) this._EXIT_();

        this.Tag                     = App.Focus.FUL;
        this.Style                   = styl!        ;
        this.Screen_Space.Background = brsh!        ;

        if ( !this._INIT_() ) this._EXIT_();
    }

    // FUNCTIONS //

    #region PRIVATE  INSTANCE INITIALIZERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _INIT_ initializes the contents styling and event binding.
    ///     </para>
    /// </summary>
    /// <Return></Return>
    private bool _INIT_ () 
    {
        Grid?   spnl = ( Grid?   )this.Screen_Space.FindName( "Selection_Panel"   )               ;

        // HEADER //

        Border? hedr = ( Border? )Prefabs.Get_Template      ( "Header"            )?.LoadContent();
        Border? lbdr = ( Border? )Prefabs.Get_Template      ( "Left_Border"       )?.LoadContent();
        Border? bbdr = ( Border? )Prefabs.Get_Template      ( "Bottom_Border"     )?.LoadContent();
        Border? rbdr = ( Border? )Prefabs.Get_Template      ( "Right_Border"      )?.LoadContent();

        // STYLES //

        Style?  cstl =            Prefabs.Get_Style         ( "Client_Button"     )               ;
        Style?  hstl =            Prefabs.Get_Style         ( "Local_Host_Button" )               ;
        Style?  sstl =            Prefabs.Get_Style         ( "Server_Button"     )               ;
        Style?  dtxt =            Prefabs.Get_Style         ( "Text1"             )               ;
        Style?  itxt =            Prefabs.Get_Style         ( "Text3"             )               ;

        if ( 
            spnl == null || hedr == null || lbdr == null || 
            bbdr == null || rbdr == null || cstl == null || 
            hstl == null || sstl == null || dtxt == null 
        ) return false;

        ToggleButton fsbt = ( ToggleButton )hedr.FindName( "Fullscreen" );

        Button       mnbt = ( Button       )hedr.FindName( "Minimize"   );
        Button       etbt = ( Button       )hedr.FindName( "Exit"       );

        if ( mnbt == null || fsbt == null || etbt == null ) return false;

        Grid? prnt = ( Grid? )fsbt.Parent;

        if ( prnt == null ) return false;

        Button clnt = new()
        {
            Tag    = ( _RNTM_ : App.Runtime.CNT, _DCRT_ : CLIENT_DESCRIPTION ),
            Style  = cstl                                                     ,
            Width  = 80                                                       ,
            Margin = new( 30, 10, 30, 10 )
        };
        Button host = new()
        {
            Tag    = ( _RNTM_ : App.Runtime.CNT, _DCRT_ : LOCAL_HOST_DESCRIPTION ),
            Style  = hstl                                                         ,
            Width  = 80                                                           ,
            Margin = new( 30, 10, 30, 10 )
        };
        Button serv = new()
        {
            Tag    = ( _RNTM_ : App.Runtime.SRV, _DCRT_ : SERVER_DESCRIPTION ),
            Style  = sstl                                                     ,
            Width  = 80                                                       ,
            Margin = new( 30, 10, 30, 10 )
        };
        TextBlock irct = new()
        {
            Style               = itxt                      ,
            Text                = INSTRUCTION               ,
            VerticalAlignment   = VerticalAlignment.Center  ,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new( 25, 25, 25, 25 )
        };
        this._DCRT_ = new()
        {
            Style  = dtxt                 ,
            Margin = new( 25, 25, 25, 25 )
        };
        hedr.MouseLeftButtonDown += _DRAG_;

        clnt.MouseEnter          += _HOVR_;
        host.MouseEnter          += _HOVR_;
        serv.MouseEnter          += _HOVR_;

        mnbt.Click               += _MNMZ_;
        etbt.Click               += _EXIT_;
        clnt.Click               += _SLCT_;
        host.Click               += _SLCT_;
        serv.Click               += _SLCT_;

        Grid.SetRow       ( lbdr       , 2 );
        Grid.SetRow       ( bbdr       , 5 );
        Grid.SetRow       ( rbdr       , 2 );
        Grid.SetRow       ( irct       , 2 );
        Grid.SetRow       ( this._DCRT_, 4 );

        Grid.SetRowSpan   ( hedr       , 2 );
        Grid.SetRowSpan   ( lbdr       , 4 );
        Grid.SetRowSpan   ( rbdr       , 4 );

        Grid.SetColumn    ( bbdr       , 1 );
        Grid.SetColumn    ( rbdr       , 4 );
        Grid.SetColumn    ( host       , 1 );
        Grid.SetColumn    ( serv       , 2 );
        Grid.SetColumn    ( irct       , 2 );
        Grid.SetColumn    ( this._DCRT_, 2 );

        Grid.SetColumnSpan( hedr       , 5 );
        Grid.SetColumnSpan( bbdr       , 3 );

        prnt.Children.Remove          ( fsbt        );

        spnl.Children.Add             ( clnt        );
        spnl.Children.Add             ( host        );
        spnl.Children.Add             ( serv        );

        this.Screen_Space.Children.Add( hedr        );
        this.Screen_Space.Children.Add( lbdr        );
        this.Screen_Space.Children.Add( bbdr        );
        this.Screen_Space.Children.Add( rbdr        );
        this.Screen_Space.Children.Add( irct        );
        this.Screen_Space.Children.Add( this._DCRT_ );

        return true;
    }

    #endregion
    #region PRIVATE  INSTANCE TRANSFORMERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _EXIT_ shutsdown the application.
    ///     </para>
    /// </summary>
    private void _EXIT_ (                                      ) 
    {
        if ( this.Tag is App.Focus fcus )
        {
            ( ( App )Application.Current )._EXIT_( fcus, this );

            return;
        }
        ( ( App )Application.Current )._EXIT_( App.Focus.FUL, this );
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _DRAG_ links the window to the users cursor while holding the sender element.
    ///     </para>
    /// </summary>
    /// <param name = "input"></param>
    private void _DRAG_ ( object _, MouseButtonEventArgs input ) 
    { 
        if ( input.ButtonState == MouseButtonState.Pressed ) DragMove();
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _MNMZ_ minimizes the window.
    ///     </para>
    /// </summary>
    private void _MNMZ_ ( object _, RoutedEventArgs      __    ) 
    {
        this.WindowState = WindowState.Minimized;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-06 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _EXIT_ shutsdown the application.
    ///     </para>
    /// </summary>
    private void _EXIT_ ( object _, RoutedEventArgs      __    ) 
    {
        this._EXIT_();
    }

    #endregion
    #region PRIVATE  INSTANCE EVENTS

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-07 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _HOVR_ binds the tagged description it contains, to the description objects text.
    ///     </para>
    /// </summary>
    /// <param name = "sndr"></param>
    private void _HOVR_ ( object sndr, MouseEventArgs  __ ) 
    {
        if ( 
            sndr     is FrameworkElement          elmt && 
            elmt.Tag is ( App.Runtime _, string dcrt ) 
        ) this._DCRT_.Text = dcrt;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 01-08 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///         
    ///         _SLCT_ requests to enter a runtime environment based on the tagged runtime selection the sender contains.
    ///     </para>
    /// </summary>
    /// <param name = "sndr"></param>
    private void _SLCT_ ( object sndr, RoutedEventArgs __ ) 
    {
        if ( 
            sndr     is FrameworkElement          elmt && 
            elmt.Tag is ( App.Runtime rntm, string _ ) 
        ) ( ( App )Application.Current )._ENTR_( rntm, this );
    }

    #endregion
}
