/// AUTHOR  : Ryan L Harding
///
/// UPDATED : 2/20/2026 21:07

#region GENERAL HEADER

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Input;
using LanChat.SubSystem.Messaging;
using LanChat.SubSystem.UserInterface;

#endregion

namespace LanChat.Runtime;

/// <summary>
/// 
/// </summary>
public partial class MainWindow : Window
{
    #region PRIVATE INSTANCE FIELDS

    private App          _APP_         ;
    private Listener?    _LSNR_ = null ;

    private uiPage?      _PAGE_ = null ;

    private DataTemplate _lTMP_        ;
    private DataTemplate _cTMP_        ;

    private bool         _CYCL_ = false;

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "app"></param>
    internal MainWindow ( App app ) 
    {
        InitializeComponent();

        Style?           styl = Prefabs.Get_Style            ( "Flexible_Window" );
        SolidColorBrush? bkgd = Prefabs.Get_Brush            ( "Primary6"        );

        DataTemplate?    sTmp = Prefabs.Get_Template         ( "Server"          );
        DataTemplate?    mTmp = Prefabs.Get_Template         ( "Message"         );
        DataTemplate?    lTmp = ( DataTemplate? )FindResource( "Login_Page"      );
        DataTemplate?    cTmp = ( DataTemplate? )FindResource( "Chat_Page"       );

        if ( sTmp == null || mTmp == null || lTmp == null || cTmp == null ) app._EXIT_( App.Integration.FUL, this );

        this._APP_                   = app  ;
        this.Style                   = styl!;
        this.Screen_Space.Background = bkgd!;

        this._lTMP_                  = lTmp!;
        this._cTMP_                  = cTmp!;

        Login_Page.Server = sTmp!;
        Chat_Page.Message = mTmp!;

        this._lINIT_(      );
    }

    #region PRIVATE  INSTANCE INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    private void _lINIT_ () 
    {
        this.Screen_Space.Child = null;

        Login_Page flip = new                                                                ();  

        Grid       lipg = ( Grid    )this._lTMP_.LoadContent                                    ();
        Border?    hedr = ( Border? )Prefabs.Get_Template( "Header"               )?.LoadContent();
        Border?    lbdr = ( Border? )Prefabs.Get_Template( "Left_Border"          )?.LoadContent();
        Border?    bbdr = ( Border? )Prefabs.Get_Template( "Bottom_Border"        )?.LoadContent();
        Border?    lipl = ( Border? )Prefabs.Get_Template( "Panel"                )?.LoadContent();
        Grid?      svwd = ( Grid?   )Prefabs.Get_Template( "Extra_Element_Window" )?.LoadContent();

        if ( hedr != null )
        {
            hedr.MouseLeftButtonDown += Drag;

            Grid.SetRowSpan   ( hedr, 2 );
            Grid.SetColumnSpan( hedr, 6 );

            Button       mnbt = ( Button       )hedr.FindName( "Minimize"   );
            ToggleButton fsbt = ( ToggleButton )hedr.FindName( "Fullscreen" );
            Button       etbt = ( Button       )hedr.FindName( "Exit"       );

            if ( mnbt != null ) mnbt.Click += Minimize  ;
            if ( fsbt != null ) fsbt.Click += Fullscreen;
            if ( etbt != null ) etbt.Click += Exit      ;

            lipg.Children.Add( hedr );
        }
        if ( lbdr != null )
        {
            Grid.SetRow    ( lbdr, 2 );
            Grid.SetRowSpan( lbdr, 4 );

            lipg.Children.Add( lbdr );
        }
        if ( bbdr != null )
        {
            Grid.SetRow       ( bbdr, 5 );
            Grid.SetColumn    ( bbdr, 1 );
            Grid.SetColumnSpan( bbdr, 4 );

            lipg.Children.Add( bbdr );
        }
        if ( lipl != null )
        {
            Grid.SetRow       ( lipl, 2 );
            Grid.SetRowSpan   ( lipl, 4 );
            Grid.SetColumn    ( lipl, 4 );
            Grid.SetColumnSpan( lipl, 2 );

            lipl.Padding = new                                   ( 0            , 0, 5, 5 );
            lipl.Effect  = Prefabs.Get_Effect< DropShadowEffect >( "Shadow_Left"          );

            lipg.Children.Add( lipl );
        }
        if ( svwd != null )
        {
            Grid.SetRow   ( svwd, 3 );
            Grid.SetColumn( svwd, 2 );

            TextBlock?    ttle = ( TextBlock?    )svwd.FindName                                            ( "Title"        );
            ScrollViewer? scrl = ( ScrollViewer? )svwd.FindName                                            ( "Scroll_Panel" );
            Border?       ippl = ( Border?       )Prefabs.Get_Template( "Single_Input_Panel" )?.LoadContent(                );

            if ( scrl != null )
            {
                flip.Scroll_Space       = scrl    ;
                scrl.PreviewMouseWheel += Dampener;

                StackPanel?   srvs = ( StackPanel? )svwd.FindName( "Elements" ) ;

                if ( srvs != null ) flip.Servers = srvs;
            }
            if ( ttle != null ) ttle!.Text = "Servers";
            if ( ippl != null )
            {
                Grid.SetRow   ( ippl, 2 );
                Grid.SetColumn( ippl, 1 );

                TextBox? inpt = ( TextBox? )ippl.FindName( "Input" );
                Button?  entr = ( Button?  )ippl.FindName( "Enter" );

                if ( inpt != null ) 
                {
                    flip.Input           = inpt  ;
                    inpt.PreviewKeyDown += Invoke;
                }
                if ( entr != null ) entr.Click += Discover;

                svwd.Children.Add( ippl );
            }
            lipg.Children.Add( svwd );
        }
        this.Screen_Space.Child = lipg;
        this._PAGE_             = flip;
        this._LSNR_             = new ([
            ( Key.Enter, [ this.Discover ] ),
            ( Key.Tab  , [ this.Tab      ] )
        ]);
        this._SRCH_ ( true );
    }

    /// <summary>
    /// 
    /// </summary>
    internal void _cINIT_ () 
    {
        this._SRCH_ ( false );

        this.Screen_Space.Child = null;
        
        Chat_Page     fctp = new                                                                         ();

        Grid          ctpg = ( Grid          )this._cTMP_.LoadContent                                    ();
        Border?       hedr = ( Border?       )Prefabs.Get_Template( "Header"               )?.LoadContent();
        Border?       lbdr = ( Border?       )Prefabs.Get_Template( "Left_Border"          )?.LoadContent();
        Border?       bbdr = ( Border?       )Prefabs.Get_Template( "Bottom_Border"        )?.LoadContent();
        ScrollViewer? blpl = ( ScrollViewer? )Prefabs.Get_Template( "Boolean_Panel"        )?.LoadContent();
        Grid?         mgwd = ( Grid?         )Prefabs.Get_Template( "Extra_Element_Window" )?.LoadContent();

        if ( hedr != null )
        {
            hedr.MouseLeftButtonDown += Drag;

            Grid.SetRowSpan   ( hedr, 2 );
            Grid.SetColumnSpan( hedr, 6 );

            Button       mnbt = ( Button       )hedr.FindName( "Minimize"   );
            ToggleButton fsbt = ( ToggleButton )hedr.FindName( "Fullscreen" );
            Button       etbt = ( Button       )hedr.FindName( "Exit"       );

            if ( mnbt != null ) mnbt.Click += Minimize  ;
            if ( fsbt != null ) fsbt.Click += Fullscreen;
            if ( etbt != null ) etbt.Click += Exit      ;

            ctpg.Children.Add( hedr );
        }
        if ( lbdr != null )
        {
            Grid.SetRow    ( lbdr, 2 );
            Grid.SetRowSpan( lbdr, 4 );

            ctpg.Children.Add( lbdr );
        }
        if ( bbdr != null )
        {
            Grid.SetRow       ( bbdr, 5 );
            Grid.SetColumn    ( bbdr, 1 );
            Grid.SetColumnSpan( bbdr, 3 );

            ctpg.Children.Add( bbdr );
        }
        if ( blpl != null )
        {
            blpl.PreviewMouseWheel += Dampener                                                        ;
            blpl.Padding            = new                                   ( 0            , 0, 5, 5 );
            blpl.Effect             = Prefabs.Get_Effect< DropShadowEffect >( "Shadow_Left"          );

            Grid.SetRow       ( blpl, 2 );
            Grid.SetRowSpan   ( blpl, 4 );
            Grid.SetColumn    ( blpl, 4 );
            Grid.SetColumnSpan( blpl, 2 );

            StackPanel? actv = ( StackPanel? )blpl.FindName( "True"  );
            StackPanel? inac = ( StackPanel? )blpl.FindName( "False" );

            if ( actv != null ) fctp.Active_Clients   = actv;
            if ( inac != null ) fctp.Inactive_Clients = inac;

            ctpg.Children.Add( blpl );
        }
        if ( mgwd != null )
        {
            Grid.SetRow   ( mgwd, 3 );
            Grid.SetColumn( mgwd, 2 );

            TextBlock?    ttle = ( TextBlock?    )mgwd.FindName                                           ( "Title"        );
            ScrollViewer? scrl = ( ScrollViewer? )mgwd.FindName                                           ( "Scroll_Panel" );
            StackPanel?   msgs = ( StackPanel?   )mgwd.FindName                                           ( "Elements"     );
            Border?       ippl = ( Border?       )Prefabs.Get_Template( "Multi_Input_Panel" )?.LoadContent(                );

            if ( ttle != null ) ttle!.Text = "Chat";
            if ( scrl != null )
            {
                fctp.Scroll_Space       = scrl    ;
                scrl.PreviewMouseWheel += Dampener;
                scrl.ScrollChanged     += Cycle   ;
            }
            if ( msgs != null ) fctp.Messages = msgs;
            if ( ippl != null )
            {
                Grid.SetRow   ( ippl, 2 );
                Grid.SetColumn( ippl, 1 );

                TextBox? inpt = ( TextBox? )ippl.FindName( "Input" );
                Button?  entr = ( Button?  )ippl.FindName( "Enter" );

                if ( inpt != null )
                {
                    fctp.Input           = inpt  ;
                    inpt.PreviewKeyDown += Invoke;
                    inpt.MaxHeight       = 300   ;
                }
                if ( entr != null ) entr.Click += Send;

                mgwd.Children.Add( ippl );
            }
            ctpg.Children.Add( mgwd );
        }
        this.Screen_Space.Child = ctpg;
        this._PAGE_             = fctp;
        this._LSNR_             = new ([
            ( Key.Enter, [ this.Send ] ),
            ( Key.Tab  , [ this.Tab  ] )
        ]);
    }

    #endregion
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
        //this._CTPG_._mPNL_.ScrollToVerticalOffset( this._CTPG_._mPNL_.VerticalOffset - input.Delta * 0.25 );

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "inpt"></param>
    private void Invoke   ( object _, KeyEventArgs           inpt ) 
    {
        if ( this._LSNR_ == null ) return;

        this._LSNR_.Invoke( inpt );
    }

    /// <summary>
    /// 
    /// </summary>
    private void Tab      (                                       ) 
    {
        if ( this._PAGE_ is Chat_Page ctpg )
        {
            int pos = ctpg.Input.CaretIndex;

            ctpg.Input.Text       = ctpg.Input.Text.Insert( pos, "\t" );
            ctpg.Input.CaretIndex = pos + 1                            ;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Send     ( object _, RoutedEventArgs        __   ) 
    {
        this.Send();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Send     (                                       ) 
    {
        if ( !this._CYCL_ && this._PAGE_ is Chat_Page ctpg && !string.IsNullOrWhiteSpace( ctpg.Input.Text ) )
        {
            this._CYCL_ = true;

            Messager.Send   ( ctpg.Input.Text );
            ctpg.Input.Clear(                 );

            if ( !Renderer.Refresh( false, ctpg.Scroll_Space, ctpg.Messages, Chat_Page.Message ) )
            {
                Renderer.Render_Next( ctpg.Scroll_Space, ctpg.Messages, Chat_Page.Message );
            }
            this._CYCL_ = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Discover ( object _, RoutedEventArgs        __   ) 
    {
        this.Discover();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Discover (                                       ) 
    {
        // TODO;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Cycle    ( object _, ScrollChangedEventArgs __   ) 
    {
        if ( !this._CYCL_ && this._PAGE_ is Chat_Page ctpg )
        {
            this._CYCL_ = true;

            Renderer.Render_Upward  ( ctpg.Scroll_Space, ctpg.Messages, Chat_Page.Message, 20 );
            Renderer.Render_Downward( ctpg.Scroll_Space, ctpg.Messages, Chat_Page.Message, 20 );
        
            this._CYCL_ = false;
        }
    }

    #endregion
    #region PRIVATE  INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "stat"></param>
    private void _SRCH_ ( bool stat ) 
    {
        if   ( stat && this._PAGE_ is Login_Page lgpg ) this._APP_._SRCH_( lgpg.Scroll_Space, lgpg.Servers, Login_Page.Server );
        else                                            this._APP_._SRCH_(                                                    );
    }

    #endregion
}
