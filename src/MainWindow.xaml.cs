/// AUTHOR  : Ryan L Harding
///
/// UPDATED : 

#region GENERAL HEADER

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion
#region LANCHAT HEADER

using LanChat.InputSystem;
using LanChat.MessageSystem;

#endregion

namespace LanChat.Runtime;

public partial class MainWindow : Window
{
    private App _APP_ { get; set; }

    private Chat_Page    _CTPG_;
    private Listener     _LSNR_;

    private DataTemplate _mTMP_;
    private DataTemplate _cTMP_;

    private bool         Cycling = false;

    public MainWindow ( App app )
    {
        InitializeComponent();

        this._APP_  = app;
        this._mTMP_ = ( DataTemplate )FindResource( "Message"   );
        this._cTMP_ = ( DataTemplate )FindResource( "Chat_Page" );

        this._LSNR_ = new ([
            ( Key.Enter, [ this.Send ] ),
            ( Key.Tab  , [ this.Tab  ] )
        ]);
        Runtime_CTPG();
    }

    public void Runtime_CTPG ()
    {
        FrameworkElement ctpg = ( FrameworkElement )_cTMP_.LoadContent();

        this.Screen_Space.Child = null        ;
        this.Screen_Space.Child = ctpg        ;
        this._CTPG_             = new ( ctpg );
    }

    #region PRIVATE  INSTANCE TRANSFORMERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "input"></param>
    private void Drag     ( object _, MouseButtonEventArgs input ) 
    { 
        if ( input.ButtonState == MouseButtonState.Pressed ) DragMove(); 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "input"></param>
    private void Dampener ( object _, MouseWheelEventArgs  input ) 
    {
        this._CTPG_._mPNL_.ScrollToVerticalOffset( this._CTPG_._mPNL_.VerticalOffset - input.Delta * 0.25 );

        input.Handled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Minimize   ( object _, RoutedEventArgs __ ) => this.WindowState = WindowState.Minimized;
    
    /// <summary>
    /// 
    /// </summary>
    private void Fullscreen ( object _, RoutedEventArgs __ ) => this.WindowState = (
        this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized
    );
    
    /// <summary>
    /// 
    /// </summary>
    private void Exit       ( object _, RoutedEventArgs __ ) => this._APP_._EXIT_( App.Integration.PTL, this );

    #endregion
    #region PRIVATE  INSTANCE EVENTS

    private void Tab ()
    {
        int pos = this._CTPG_._INPT_.CaretIndex;

        this._CTPG_._INPT_.Text       = this._CTPG_._INPT_.Text.Insert( pos, "\t" );
        this._CTPG_._INPT_.CaretIndex = pos + 1                                    ;
    }

    private void Invoke( object _, KeyEventArgs inpt )
    {
        if ( !_APP_.canRUN ) return;

        this._LSNR_.Invoke( inpt );
    }

    /// <summary>
    /// 
    /// </summary>
    private void Send  ( object _, RoutedEventArgs        __ ) 
    {
        this.Send();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Send  (                                     ) 
    {
        if ( !_APP_.canRUN || this.Cycling || string.IsNullOrWhiteSpace( this._CTPG_._INPT_.Text )) return;

        this.Cycling = true;

        Messager.Send           ( this._CTPG_._INPT_.Text );
        this._CTPG_._INPT_.Clear(                         );
        Renderer.Refresh        ( 
            false             , 
            this._CTPG_._mPNL_, 
            this._CTPG_._MSGS_, 
            this._mTMP_ 
        );
        this.Cycling = false;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Cycle ( object _, ScrollChangedEventArgs __ ) 
    {
        if ( !_APP_.canRUN || this.Cycling ) return;

        this.Cycling = true;

        Renderer.Render_Upward  ( this._CTPG_._mPNL_, this._CTPG_._MSGS_, this._mTMP_, 20 );
        Renderer.Render_Downward( this._CTPG_._mPNL_, this._CTPG_._MSGS_, this._mTMP_, 20 );
        
        this.Cycling = false;
    }

    #endregion

    private struct Chat_Page
    {
        internal TextBox      _INPT_ { get; private set; }
        internal ScrollViewer _mPNL_ { get; private set; }
        internal StackPanel   _MSGS_ { get; private set; }
        internal StackPanel   _cACV_ { get; private set; }
        internal StackPanel   _cNAV_ { get; private set; }

        internal Chat_Page ( FrameworkElement ctpg ) 
        {
            this._INPT_ = ( TextBox      )ctpg.FindName( "Input"            );
            this._mPNL_ = ( ScrollViewer )ctpg.FindName( "Message_Panel"    );
            this._MSGS_ = ( StackPanel   )ctpg.FindName( "Messages"         );
            this._cACV_ = ( StackPanel   )ctpg.FindName( "Active_Clients"   );
            this._cNAV_ = ( StackPanel   )ctpg.FindName( "Inactive_Clients" );
        }
    }
}