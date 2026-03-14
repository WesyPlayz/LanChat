/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 15:16
///
/// REMAINING : ALL ( SUBJECT TO FILL )

using System.Windows;

using System.Windows.Input;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using LanChat.SubSystem.UserInterface;
using LanChat.SubSystem.Messaging;
using LanChat.SubSystem.Network;
using LanChat.SubSystem.Input;
using LanChat.SubSystem.Authentication;

namespace LanChat.Runtime;

/// <summary>
/// 
/// </summary>
public partial class TermWindow : Window
{
    internal App _APP_;
    private Listener?    _LSNR_ = null ;

    internal Terminal_Page _TMPG_;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "app"></param>
    public TermWindow ( App app ) 
    {
        InitializeComponent();

        this._APP_ = app;

        this._tINIT_();
    }

    /// <summary>
    /// 
    /// </summary>
    private void _tINIT_ ()
    {
        this._TMPG_ = new Terminal_Page();

        Border? hedr = ( Border? )Prefabs.Get_Template( "Header" )?.LoadContent();
        Border? lbdr = (Border?)Prefabs.Get_Template("Left_Border")?.LoadContent();
        Border? bbdr = (Border?)Prefabs.Get_Template("Bottom_Border")?.LoadContent();
        Border? rbdr = (Border?)Prefabs.Get_Template("Right_Border")?.LoadContent();
        Grid? mgwd = (Grid?)Prefabs.Get_Template("Extra_Element_Window")?.LoadContent();

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

            Screen_Space.Children.Add( hedr );
        }
        if ( lbdr != null )
        {
            Grid.SetRow    ( lbdr, 2 );
            Grid.SetRowSpan( lbdr, 2 );

            Screen_Space.Children.Add( lbdr );
        }
        if ( bbdr != null )
        {
            Grid.SetRow       ( bbdr, 3 );
            Grid.SetColumn    ( bbdr, 1 );

            Screen_Space.Children.Add( bbdr );
        }
        if ( rbdr != null )
        {
            Grid.SetRow    ( rbdr, 2 );
            Grid.SetRowSpan( rbdr, 2 );
            Grid.SetColumn ( rbdr, 2 );

            Screen_Space.Children.Add( rbdr );
        }
        if ( mgwd != null )
        {
            Grid.SetRow   ( mgwd, 2 );
            Grid.SetColumn( mgwd, 1 );

            TextBlock?    ttle = ( TextBlock?    )mgwd.FindName                                           ( "Title"        );
            ScrollViewer? scrl = ( ScrollViewer? )mgwd.FindName                                           ( "Scroll_Panel" );
            StackPanel?   cmds = ( StackPanel?   )mgwd.FindName                                           ( "Elements"     );
            Border?       ippl = ( Border?       )Prefabs.Get_Template( "Single_Input_Panel" )?.LoadContent(                );

            if ( ttle != null ) ttle!.Text = "Terminal";
            if ( scrl != null )
            {
                this._TMPG_.Scroll_Space       = scrl    ;
                scrl.PreviewMouseWheel += Dampener;
            }
            if ( cmds != null ) this._TMPG_.Commands = cmds;
            if ( ippl != null )
            {
                Grid.SetRow   ( ippl, 2 );
                Grid.SetColumn( ippl, 1 );

                TextBox? inpt = ( TextBox? )ippl.FindName( "Input" );
                Button?  entr = ( Button?  )ippl.FindName( "Enter" );

                if ( inpt != null )
                {
                    this._TMPG_.Input           = inpt  ;
                    inpt.PreviewKeyDown += Invoke;
                    inpt.MaxHeight       = 300   ;
                }
                if ( entr != null ) entr.Click += Prompt;

                mgwd.Children.Add( ippl );
            }
            Screen_Space.Children.Add( mgwd );

            this._LSNR_             = new ([
                ( Key.Enter, [ this.Prompt] )
            ]);
            this._TMPG_.Commands.Children.Add( new TextBlock() 
            { 
                Text = "Prompts: " +
                "\n INIT < name > < port > < password > -> Creates a new server" +
                "\n END -> Ends the current server",
                Foreground = Prefabs.Get_Brush( "Primary0" )
            });
            this._TMPG_.Scroll_Space.UpdateLayout();
            this._TMPG_.Scroll_Space.ScrollToEnd();
        }
    }

    #region PRIVATE INSTANCE TRANSFORMERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "input"></param>
    private void Drag       ( object _, MouseButtonEventArgs input ) 
    { 
        if ( input.ButtonState == MouseButtonState.Pressed ) DragMove(); 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "input"></param>
    private void Dampener   ( object _, MouseWheelEventArgs  input ) 
    {
        //this._CTPG_._mPNL_.ScrollToVerticalOffset( this._CTPG_._mPNL_.VerticalOffset - input.Delta * 0.25 );

        input.Handled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Minimize   ( object _, RoutedEventArgs      __    ) 
    {
        this.WindowState = WindowState.Minimized;
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void Fullscreen ( object _, RoutedEventArgs      __    ) 
    {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void Exit       ( object _, RoutedEventArgs      __    ) 
    {
        this._APP_._EXIT_( App.Integration.FUL, this );
    }

    #endregion
    #region PRIVATE INSTANCE EVENTS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "inpt"></param>
    private void Invoke     ( object _   , KeyEventArgs           inpt ) 
    {
        if ( this._LSNR_ == null ) return;

        this._LSNR_.Invoke( inpt );
    }

    // TEXT BOX EVENTS //

    /// <summary>
    /// 
    /// </summary>
    private void Prompt       (                                          ) 
    {
        if ( !string.IsNullOrWhiteSpace( this._TMPG_.Input.Text ) )
        {
            string[] elmts = this._TMPG_.Input.Text.Split(' ');
            string result = this._TMPG_.Input.Text;
            
            if (elmts[0] == "INIT" && elmts.Length == 4 && int.TryParse(elmts[2], out int port ))
            {
                if ( Bridge.Active ) result = "Cannot create a server while one is in progress.";
                else
                {
                    Bridge.Initialize   ( Bridge.Mode.SRV           );
                    Bridge.Start(port, elmts[3]);

                    Registry.Initialize ( Bridge.Mode.SRV           );
                    Messager.Initialize(Bridge.Mode.SRV, elmts[1]);
                }
            }
            else if (elmts[0] == "END" && elmts.Length == 1 )
            {
                if (!Bridge.Active) result = "Cannot end a server when none are in progress";
                Bridge.Stop();
            }
            this._TMPG_.Commands.Children.Add(new TextBlock()
            {
                Text = result,
                Foreground = Prefabs.Get_Brush("Primary0")
            });
            this._TMPG_.Input.Clear(                 );
        }
    }

    // BUTTON EVENTS //

    /// <summary>
    /// 
    /// </summary>
    private void Prompt       ( object _   , RoutedEventArgs        __   ) 
    {
        this.Prompt();
    }

    #endregion
}
