/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 3/18/2026 06:12
/// 
/// REMAINING : FINISHED ( SUBJECT TO UPDATE )

#region GENERAL HEADER

using System.Windows;

using System.Windows.Input;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using System.Windows.Media;
using System.Windows.Media.Effects;

#endregion
#region LANCHAT HEADER

using LanChat.SubSystem.Network;
using LanChat.SubSystem.Authentication;
using LanChat.SubSystem.Messaging;
using LanChat.SubSystem.Input;
using LanChat.SubSystem.UserInterface;

#endregion

/// CONTENTS  :
/// 
/// MainWindow^Window - CLAS [ LCRT : 02-00 ]
/// ^   _lPGE_        - FELD
/// ^   _cPGE_        - FELD
/// ^   _lSPC_        - FELD
/// ^   _cSPC_        - FELD
/// ^   _LSNR_        - FELD
/// ^   _PAGE_        - FELD
/// ^   _CYCL_        - FELD
/// ^   MainWindow()  - FUNC [ LCRT : 02-01 ]
/// ^   _INIT_    ()  - FUNC [ LCRT : 02-02 ]
/// ^   _SWCH_    ()  - FUNC [ LCRT : 02-03 ]
/// ^   _EXIT_    ()  - FUNC [ LCRT : 02-04 ]
/// ^   _DRAG_    ()  - EVNT [ LCRT : 02-05 ]
/// ^   _MNMZ_    ()  - EVNT [ LCRT : 02-06 ]
/// ^   _FULL_    ()  - EVNT [ LCRT : 02-07 ]
/// ^   _EXIT_    ()  - EVNT [ LCRT : 02-08 ]
/// ^   _INVK_    ()  - EVNT [ LCRT : 02-09 ]
/// ^   _TAB_     ()  - FUNC [ LCRT : 02-10 ]
/// ^   _ENTR_    ()  - FUNC [ LCRT : 02-11 ]
/// ^   _ENTR_    ()  - EVNT [ LCRT : 02-12 ]
/// ^   _SLCT_    ()  - EVNT [ LCRT : 02-13 ]
/// ^   _CNCT_    ()  - EVNT [ LCRT : 02-14 ]
/// ^   _DNCT_    ()  - EVNT [ LCRT : 02-15 ]
/// ^   _LGIN_    ()  - EVNT [ LCRT : 02-16 ]
/// ^   _DAMP_    ()  - EVNT [ LCRT : 02-17 ]
/// ^   _SCRL_    ()  - EVNT [ LCRT : 02-18 ]
/// 
namespace LanChat.Runtime;

/// <summary>
///     <para><b>ID :</b> [ LCRT : 02-00 ]</para>
///     <para>
///         <b>Description :</b> 
///     
/// 
///     </para>
/// </summary>
public partial class MainWindow : Window
{
    #region PRIVATE ENUMS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private enum Page
    {
        NIL,
        LOG,
        CHT
    }

    #endregion

    #region PRIVATE INSTANCE FIELDS

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private (
        DataTemplate _SERV_ ,

        ScrollViewer _SCRL_ ,
        StackPanel   _SERVs_,
        TextBox      _INPT_ ,

        StackPanel   _PANL_ ,
        
        Grid         _LGIN_ ,

        TextBox      _URNM_ ,
        TextBox      _PSWD_ 
    ) _lPGE_;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private (
        DataTemplate _MSG_ ,
        DataTemplate _CLNT_,

        ScrollViewer _SCRL_,
        StackPanel   _MSGs_,
        TextBox      _INPT_,

        ScrollViewer _ACRL_,
        StackPanel   _ACTV_,
        StackPanel   _INAC_
    ) _cPGE_;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private Grid     _lSPC_           ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private Grid     _cSPC_           ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private Listener _LSNR_ = null!   ;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private Page     _PAGE_ = Page.NIL;

    /// <summary>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private bool     _CYCL_ = false   ;

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-01 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    internal MainWindow () 
    {
        InitializeComponent();

        Style?           styl = Prefabs.Get_Style            ( "Flexible_Window" );

        SolidColorBrush? brsh = Prefabs.Get_Brush            ( "Primary6"        );

        DataTemplate?    sTmp = Prefabs.Get_Template         ( "Server"          );
        DataTemplate?    uTmp = Prefabs.Get_Template         ( "Client"          );
        DataTemplate?    mTmp = Prefabs.Get_Template         ( "Message"         );

        DataTemplate?    lTmp = ( DataTemplate? )FindResource( "Login_Page"      );
        DataTemplate?    cTmp = ( DataTemplate? )FindResource( "Chat_Page"       );

        if (
            styl == null || brsh == null || sTmp == null ||
            uTmp == null || mTmp == null || lTmp == null ||
            cTmp == null
        ) this._EXIT_();

        this.Tag                     = App.Focus.FUL;
        this.Style                   = styl         ;
        this.Screen_Space.Background = brsh         ;

        this._lPGE_._SERV_           = sTmp!        ;
        this._cPGE_._CLNT_           = uTmp!        ;
        this._cPGE_._MSG_            = mTmp!        ;

        Grid? lisp = ( Grid? )lTmp!.LoadContent();
        Grid? ctsp = ( Grid? )cTmp!.LoadContent();

        if ( lisp == null || ctsp == null ) this._EXIT_();

        this._lSPC_ = lisp!;
        this._cSPC_ = ctsp!;

        if ( !this._INIT_() ) this._EXIT_();

        this._SWCH_();

        Bridge.Initialize( Bridge.Mode.CNT );
        Bridge.Start     (                 );
    }

    // FUNCTIONS //

    #region PRIVATE INSTANCE INITIALIZERS
    
    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-02 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    /// <returns></returns>
    private bool _INIT_  () 
    {
        // EFFECTS //

        DropShadowEffect? lsdw = Prefabs.Get_Effect< DropShadowEffect >( "Shadow_Left"          )               ;

        // HEADER //

        Border?           hedr = ( Border?       )Prefabs.Get_Template ( "Header"               )?.LoadContent();
        Border?           lbdr = ( Border?       )Prefabs.Get_Template ( "Left_Border"          )?.LoadContent();
        Border?           bbdr = ( Border?       )Prefabs.Get_Template ( "Bottom_Border"        )?.LoadContent();

        // SECTIONS //

        Border?           lipn = ( Border?       )Prefabs.Get_Template ( "Panel"                )?.LoadContent();

        ScrollViewer?     acpn = ( ScrollViewer? )Prefabs.Get_Template ( "Boolean_Panel"        )?.LoadContent();

        // MINI-WINDOWS //

        Grid?             svwd = ( Grid?         )Prefabs.Get_Template ( "Extra_Element_Window" )?.LoadContent();
        Grid?             ctwd = ( Grid?         )Prefabs.Get_Template ( "Extra_Element_Window" )?.LoadContent();

        // INPUT //

        Border?           sipp = ( Border?       )Prefabs.Get_Template ( "Single_Input_Panel"   )?.LoadContent();
        Border?           mipp = ( Border?       )Prefabs.Get_Template ( "Multi_Input_Panel"    )?.LoadContent();

        // LOGIN INTERFACE //

        Grid?             cnct = ( Grid?         )Prefabs.Get_Template ( "Text_Button"          )?.LoadContent();
        Grid?             lgin = ( Grid?         )Prefabs.Get_Template ( "Login_Panel"          )?.LoadContent();

        if (
            lsdw == null || hedr == null || lbdr == null || 
            bbdr == null || lipn == null || acpn == null || 
            svwd == null || ctwd == null || sipp == null ||
            mipp == null || cnct == null || lgin == null
        ) return false;

        // HEADER //

        ToggleButton? fsbt  = ( ToggleButton? )hedr.FindName( "Fullscreen"   );

        Button?       mnbt  = ( Button?       )hedr.FindName( "Minimize"     );
        Button?       etbt  = ( Button?       )hedr.FindName( "Exit"         );

        // ENTER //

        Button?       sven  = ( Button?       )sipp.FindName( "Enter"        );
        Button?       cten  = ( Button?       )mipp.FindName( "Enter"        );

        // LOGIN BUTTONS //

        Button?       cbtn  = ( Button?       )cnct.FindName( "Button"       );
        Button?       jbtn  = ( Button?       )lgin.FindName( "Join"         );

        // TITLES //

        TextBlock?    svtl  = ( TextBlock?    )svwd.FindName( "Title"        );
        TextBlock?    cttl  = ( TextBlock?    )ctwd.FindName( "Title"        );

        TextBlock?    labl  = ( TextBlock?    )cnct.FindName( "Label"        );

        // INPUT //

        TextBox?      svip  = ( TextBox?      )sipp.FindName( "Input"        );
        TextBox?      ctip  = ( TextBox?      )mipp.FindName( "Input"        );

        TextBox?      urnm  = ( TextBox?      )lgin.FindName( "Username"     );
        TextBox?      pswd  = ( TextBox?      )lgin.FindName( "Password"     );

        // SECTIONS //

        StackPanel?   servs = ( StackPanel?   )svwd.FindName( "Elements"     );
        StackPanel?   msgs  = ( StackPanel?   )ctwd.FindName( "Elements"     );

        StackPanel?   pnsp  = ( StackPanel?   )lipn.FindName( "Panel_Space"  );
        StackPanel?   actv  = ( StackPanel?   )acpn.FindName( "True"         );
        StackPanel?   inac  = ( StackPanel?   )acpn.FindName( "False"        );

        // SCROLL //

        ScrollViewer? svsl  = ( ScrollViewer? )svwd.FindName( "Scroll_Panel" );
        ScrollViewer? ctsl  = ( ScrollViewer? )ctwd.FindName( "Scroll_Panel" );

        if ( 
            fsbt == null || mnbt == null || etbt  == null ||
            sven == null || cten == null || cbtn  == null ||
            jbtn == null || svtl == null || cttl  == null || 
            labl == null || svip == null || ctip  == null ||
            urnm == null || pswd == null || servs == null || 
            msgs == null || pnsp == null || actv  == null || 
            inac == null || svsl == null || ctsl  == null
        ) return false;

        hedr.MouseLeftButtonDown += this._DRAG_;

        acpn.PreviewMouseWheel   += this._DAMP_;
        svsl.PreviewMouseWheel   += this._DAMP_;
        ctsl.PreviewMouseWheel   += this._DAMP_;

        ctsl.ScrollChanged       += this._SCRL_;

        fsbt.Click               += this._FULL_;

        mnbt.Click               += this._MNMZ_;
        etbt.Click               += this._EXIT_;
        sven.Click               += this._ENTR_;
        cten.Click               += this._ENTR_;
        cbtn.Click               += this._CNCT_;
        jbtn.Click               += this._LGIN_;

        svip.PreviewKeyDown      += this._INVK_;
        ctip.PreviewKeyDown      += this._INVK_;

        Grid.SetRow       ( lbdr       , 1 );
        Grid.SetRow       ( bbdr       , 3 );
        Grid.SetRow       ( this._lSPC_, 2 );
        Grid.SetRow       ( this._cSPC_, 2 );
        Grid.SetRow       ( svwd       , 1 );
        Grid.SetRow       ( ctwd       , 1 );
        Grid.SetRow       ( sipp       , 2 );
        Grid.SetRow       ( mipp       , 2 );

        Grid.SetRowSpan   ( hedr       , 2 );
        Grid.SetRowSpan   ( lbdr       , 3 );
        Grid.SetRowSpan   ( lipn       , 3 );
        Grid.SetRowSpan   ( acpn       , 3 );

        Grid.SetColumn    ( bbdr       , 1 );
        Grid.SetColumn    ( this._lSPC_, 1 );
        Grid.SetColumn    ( this._cSPC_, 1 );
        Grid.SetColumn    ( lipn       , 3 );
        Grid.SetColumn    ( acpn       , 3 );
        Grid.SetColumn    ( svwd       , 1 );
        Grid.SetColumn    ( ctwd       , 1 );
        Grid.SetColumn    ( sipp       , 1 );
        Grid.SetColumn    ( mipp       , 1 );

        Grid.SetColumnSpan( hedr       , 3 );
        Grid.SetColumnSpan( this._lSPC_, 2 );
        Grid.SetColumnSpan( this._cSPC_, 2 );
        Grid.SetColumnSpan( lipn       , 2 );
        Grid.SetColumnSpan( acpn       , 2 );

        lipn.Padding   = new( 0  , 0 , 5  , 5  );
        lipn.Effect    = lsdw                   ;

        acpn.Padding   = new( 0  , 0 , 5  , 5  );
        acpn.Effect    = lsdw                   ;

        cbtn.Margin    = new( 0  , 25, 0  , 25 );

        lgin.Margin    = new( 100, 10, 100, 10 );

        labl.Text      = "Connect"              ;
        labl.Margin    = new( 0  , 25, 0  , 25 );

        ctip.MaxHeight = 300                    ;

        svtl.Text      = "Servers"              ;
        cttl.Text      = "Chat"                 ;

        this._lPGE_._SCRL_  = svsl ;
        this._lPGE_._SERVs_ = servs;
        this._lPGE_._INPT_  = svip ;
        this._lPGE_._PANL_  = pnsp ;
        this._lPGE_._LGIN_  = lgin ;
        this._lPGE_._URNM_  = urnm ;
        this._lPGE_._PSWD_  = pswd ;
        this._cPGE_._SCRL_  = ctsl ;
        this._cPGE_._MSGs_  = msgs ;
        this._cPGE_._INPT_  = ctip ;
        this._cPGE_._ACRL_  = acpn ;
        this._cPGE_._ACTV_  = actv ;
        this._cPGE_._INAC_  = inac ;

        svwd.Children.Add             ( sipp        );
        ctwd.Children.Add             ( mipp        );
        pnsp.Children.Add             ( cnct        );

        this._lSPC_.Children.Add      ( lipn        );
        this._lSPC_.Children.Add      ( svwd        );
        this._cSPC_.Children.Add      ( acpn        );
        this._cSPC_.Children.Add      ( ctwd        );

        this.Screen_Space.Children.Add( hedr        );
        this.Screen_Space.Children.Add( lbdr        );
        this.Screen_Space.Children.Add( bbdr        );

        this._LSNR_ = new ([
            ( Key.Enter, [ this._ENTR_ ] ),
            ( Key.Tab  , [ this._TAB_  ] )
        ]);
        return true;
    }

    #endregion
    #region PRIVATE INSTANCE FUNCTIONS

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-03 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    ///     
    ///     </para>
    /// </summary>
    private void _SWCH_ () 
    {
        if      ( this._PAGE_ != Page.LOG )
        {
            if ( this.Screen_Space.Children.Contains( this._cSPC_ ) ) this.Screen_Space.Children.Remove( this._cSPC_ );

            this.Screen_Space.Children.Add       ( this._lSPC_                                                 );
            SubSystem.Network.Renderer.Bind      ( this._SLCT_                                                 );
            SubSystem.Network.Renderer.Initialize( this._lPGE_._SCRL_, this._lPGE_._SERVs_, this._lPGE_._SERV_ );
            SubSystem.Network.Renderer.Start     (                                                             );

            this._PAGE_ = Page.LOG;
        }
        else if ( this._PAGE_ != Page.CHT )
        {
            if ( this.Screen_Space.Children.Contains( this._lSPC_ ) ) this.Screen_Space.Children.Remove( this._lSPC_ );

            this.Screen_Space.Children.Add              ( this._cSPC_                                                                    );
            SubSystem.Authentication.Renderer.Initialize( this._cPGE_._SCRL_, this._cPGE_._ACTV_, this._cPGE_._INAC_, this._cPGE_._CLNT_ );
            SubSystem.Messaging.Renderer.Initialize     ( this._cPGE_._SCRL_, this._cPGE_._MSGs_, this._cPGE_._MSG_ , 20                 );
            SubSystem.Authentication.Renderer.Start     (                                                                                );
            Registry.Start                              (                                                                                );

            this._PAGE_ = Page.CHT;
        }
    }

    #endregion
    #region PRIVATE INSTANCE TRANSFORMERS

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-04 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _EXIT_ (                                        ) 
    {
        if ( this._PAGE_ == Page.CHT ) Registry.Logout();

        if ( this.Tag is App.Focus fcus )
        {
            ( ( App )Application.Current )._EXIT_( fcus, this );

            return;
        }
        ( ( App )Application.Current )._EXIT_( App.Focus.FUL, this );
    }

    // BUTTON EVENTS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-05 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    /// <param name = "inpt"></param>
    private void _DRAG_ ( object _   , MouseButtonEventArgs inpt ) 
    { 
        if ( inpt.ButtonState == MouseButtonState.Pressed ) this.DragMove(); 
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-06 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _MNMZ_ ( object _   , RoutedEventArgs      __   ) 
    {
        this.WindowState = WindowState.Minimized;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-07 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _FULL_ ( object _   , RoutedEventArgs      __   ) 
    {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-08 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _EXIT_ ( object _   , RoutedEventArgs      __   ) 
    {
        this._EXIT_();
    }

    #endregion
    #region PRIVATE INSTANCE EVENTS

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-09 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    /// <param name = "inpt"></param>
    private void _INVK_ ( object _   , KeyEventArgs       inpt ) 
    {
        if ( this._LSNR_ == null ) return;

        this._LSNR_.Invoke( inpt );
    }

    // TEXT BOX EVENTS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-10 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _TAB_  (                                      ) 
    {
        if      ( this._PAGE_ == Page.LOG )
        {
            int pos = this._lPGE_._INPT_.CaretIndex;

            this._lPGE_._INPT_.Text       = this._lPGE_._INPT_.Text.Insert( pos, "\t" );
            this._lPGE_._INPT_.CaretIndex = pos + 1                                    ;
        }
        else if ( this._PAGE_ == Page.CHT )
        {
            int pos = this._cPGE_._INPT_.CaretIndex;

            this._cPGE_._INPT_.Text       = this._cPGE_._INPT_.Text.Insert( pos, "\t" );
            this._cPGE_._INPT_.CaretIndex = pos + 1                                    ;
        }
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-11 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _ENTR_ (                                      ) 
    {
        if      ( this._PAGE_ == Page.LOG && !string.IsNullOrWhiteSpace( this._lPGE_._INPT_.Text ) )
        {
            Bridge.Discover         ( this._lPGE_._INPT_.Text );
            this._lPGE_._INPT_.Clear(                         );
        }
        else if ( this._PAGE_ == Page.CHT && !string.IsNullOrWhiteSpace( this._cPGE_._INPT_.Text ) )
        {
            Messager.Send           ( this._cPGE_._INPT_.Text );
            this._cPGE_._INPT_.Clear(                         );
        }
    }

    // BUTTON EVENTS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-12 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _ENTR_ ( object _   , RoutedEventArgs    __   ) 
    {
        this._ENTR_();
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-13 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _SLCT_ ( object sndr, RoutedEventArgs    __   ) 
    {
        if ( sndr is not Button bttn || bttn.Tag is not int idx ) return;

        SubSystem.Network.Renderer.Select( idx, bttn );
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-14 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _CNCT_ ( object _   , RoutedEventArgs    __   ) 
    {
        if ( this._PAGE_ != Page.LOG ) return;
        
        Bridge.Disconnect();

        if ( SubSystem.Network.Renderer.Selected < 0 || !Bridge.Connect( SubSystem.Network.Renderer.Selected ) ) return;
        
        if ( this._lPGE_._LGIN_ == null || this._lPGE_._URNM_ == null || this._lPGE_._PSWD_ == null ) return;
        
        this._lPGE_._PANL_.Children.Add( this._lPGE_._LGIN_ );
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-15 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _DNCT_ ( object _   , RoutedEventArgs    __   ) 
    {
        Bridge.Disconnect();
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-16 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _LGIN_ ( object _   , RoutedEventArgs    __   ) 
    {
        if (
            this._PAGE_        != Page.LOG                  || 
            this._lPGE_._URNM_ == null                      ||
            this._lPGE_._PSWD_ == null                      ||
            string.IsNullOrWhiteSpace( _lPGE_._URNM_.Text ) ||
            string.IsNullOrWhiteSpace( _lPGE_._PSWD_.Text )
        ) return;

        Registry.Initialize( Bridge.Mode.CNT, _lPGE_._URNM_.Text );

        if ( !Registry.Login( _lPGE_._URNM_.Text, _lPGE_._PSWD_.Text ) ) return;

        this._SWCH_        (                                     );
        Messager.Initialize( Bridge.Mode.CNT, _lPGE_._URNM_.Text );
        Messager.Request   ( Messager.ALL                        );
    }

    // SCROLL EVENTS //

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-17 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    /// <param name = "inpt"></param>
    private void _DAMP_ ( object sndr, MouseWheelEventArgs    inpt ) 
    {
        if ( sndr is not ScrollViewer scrl ) return;

        scrl.ScrollToVerticalOffset( scrl.VerticalOffset - inpt.Delta * 0.25 );

        inpt.Handled = true;
    }

    /// <summary>
    ///     <para><b>ID :</b> [ LCRT : 02-18 ]</para>
    ///     <para>
    ///         <b>Description :</b> 
    ///     
    /// 
    ///     </para>
    /// </summary>
    private void _SCRL_ ( object _   , ScrollChangedEventArgs __   ) 
    {
        if ( !this._CYCL_ && this._PAGE_ == Page.CHT )
        {
            this._CYCL_ = true;

            //if ( ctpg.Scroll_Space.VerticalOffset <= 0                                  ) _ = Task.Run( SubSystem.Messaging.Renderer.Render_Upward   );
            //if ( ctpg.Scroll_Space.VerticalOffset == ctpg.Scroll_Space.ScrollableHeight ) _ = Task.Run( SubSystem.Messaging.Renderer.Render_Downward );
        
            this._CYCL_ = false;
        }
    }

    #endregion
}
