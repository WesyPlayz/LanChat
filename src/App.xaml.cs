#region GENERAL HEADER

using System.Runtime.InteropServices;

using System.Windows;
using System.Windows.Controls;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Core;
using LanChat.SubSystem.Network;
using LanChat.SubSystem.Messaging;
using LanChat.SubSystem.UserInterface;

#endregion

namespace LanChat.Runtime;

public partial class App : Application 
{
    #region INTERNAL  ENUMS

    internal enum Integration 
    {
        NIL,
        PTL,
        FUL
    }
    internal enum Extention   
    {
        NIL,
        TER
    }

    #endregion
    #region INTERNAL  INSTANCE PROPERTIES

    internal InitWindow InitWindow { get; set; }
    internal TermWindow TermWindow { get; set; }

    #endregion
    #region PRIVATE   STATIC   PROPERTIES

    public bool canRUN = false;

    #endregion

    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();


    // CONSTRUCTORS //

    /// <summary>
    /// Description :
    ///     Initializes a new application filling it's Initialization Window and Terminal Window with null.
    /// </summary>
    private App () : base () 
    { 
        this.InitWindow = null!; 
        this.TermWindow = null!;

        Prefabs.Initialize();
    }

    // FUNCTIONS //

    #region PROTECTED OVERRIDE FUNCTIONS

    /// <summary>
    /// Description :
    ///     Initializes the current application after all resources are loaded.
    /// </summary>
    protected override void OnStartup ( StartupEventArgs _ ) => this._INIT_();

    #endregion
    #region INTERNAL  INSTANCE WINDOW  MANAGEMENT

    /// <summary>
    /// Description :
    ///     Closes the current window ( if any ) and opens a new instance of the Initialization Window.
    /// </summary>
    /// <param name = "win"></param>
    internal void _INIT_ ( Window? win = null                                ) 
    {
        if ( win != null && win is InitWindow ) return;

        win?.Close();

        AllocConsole();

        this.InitWindow = new ( this );

        this.InitWindow.Show();
    }

    /// <summary>
    /// Description :
    ///     Closes the current window ( if any ) and opens a new instance of the Terminal Window.
    /// </summary>
    /// <param name = "win"></param>
    internal void _TERM_ ( Window? win = null                                ) 
    {
        if ( win != null && win is TermWindow ) return;

        win?.Close();

        this.TermWindow = new TermWindow( this );

        this.TermWindow.Show();

        Bridge.Initialize( Bridge.Mode.SRV );
        Bridge.Start(); // TEMP
        Messager.Initialize( Bridge.Mode.SRV, "SERVER" );
    }

    /// <summary>
    /// Description :
    ///     Closes the current window ( if any ) and opens a new instance of the Main Window.
    /// </summary>
    /// <param name = "win"></param>
    internal void _MAIN_ ( Window? win = null, Extention ext = Extention.NIL ) 
    {
        if ( win != null && win is MainWindow ) return;

        win?.Close();

        this.MainWindow = new MainWindow( this );

        this.MainWindow.Show();

        Bridge.Initialize( ext == Extention.NIL ? Bridge.Mode.CNT : Bridge.Mode.SRV );
        Bridge.Start     (                                                          );
        if ( Bridge.Connect( 0 ) && this.MainWindow is MainWindow mw ) mw._cINIT_();
        Messager.Initialize( Bridge.Mode.CNT, "Ryan" );
    }

    /// <summary>
    /// Description :
    ///     If given Integration.PTL - the given window will be closed.
    ///     If Given Integration.FUL - the given window and the current application will close and shutdown.
    /// </summary>
    /// <param name = "typ"></param>
    /// <param name = "win"></param>
    internal void _EXIT_ ( Integration typ   , Window    win                 ) 
    {
        switch ( typ )
        {
            case    Integration.PTL : win.Close    (); break; // FOR TEST PURPOSES
            case    Integration.FUL : this.Shutdown(); break;
            default                 :                  break;
        }
    }

    #endregion
    #region INTERNAL  INSTANCE BRIDGE  MANAGEMENT

    /*
    internal Server   _DCVR_  ()
    {

    }
    */

    private Sync _SYNC_ = new();

    internal void _SRCH_()
    {
        _SYNC_.Close();
    }

    internal void _SRCH_(ScrollViewer scrl, StackPanel srvs, DataTemplate serv)
    {
        _SYNC_.Start();
        
        Task.Run(async () =>
        {
            while ( _SYNC_.Continue )
            {
                iEntity[]? cServs = Bridge.Get();

                srvs.Dispatcher.Invoke(() =>
                {
                    srvs.Children.Clear();

                    if (cServs != null)
                    {
                        foreach (Server iServ in cServs)
                        {
                            Console.WriteLine(iServ.ToString());
                            FrameworkElement s = (FrameworkElement)serv.LoadContent();

                            s.DataContext = iServ;

                            srvs.Children.Add(s);
                            scrl.UpdateLayout();
                        }
                    }
                });

                await Task.Delay(5000);
            }
            _SYNC_.Stop();
        });
    }


    #endregion
}