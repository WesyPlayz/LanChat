/// AUTHOR : Ryan L Harding
///
///

#region GENERAL HEADER

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;

#endregion

namespace LanChat.SubSystem.UserInterface;

/// <summary>
/// 
/// </summary>
public static class Prefabs    
{
    #region PRIVAATE STATIC PROPERTIES

    private static Dictionary< string, SolidColorBrush > _BRSHs_ = [];
    private static Dictionary< string, Style           > _STYLs_ = [];
    private static Dictionary< string, DataTemplate    > _TEMPs_ = [];
    private static Dictionary< string, Effect          > _EFCTs_ = [];

    private static bool                                  _FILD_  = false;

    #endregion
    #region PUBLIC   STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    public static void Initialize () 
    {
        if ( _FILD_ ) return;

        _FILD_ = true;

        ResourceDictionary rsrcs = new ()
        {
            Source = new Uri ( "/Assets/Prefabs.xaml", UriKind.Relative )
        };
        foreach ( DictionaryEntry etry in rsrcs )
        {
            string name = etry.Key.ToString()!;
            object val  = etry.Value!         ;

            if      ( val is SolidColorBrush  brsh ) _BRSHs_[ name ] = brsh;
            else if ( val is Style            styl ) _STYLs_[ name ] = styl;
            else if ( val is DataTemplate     temp ) _TEMPs_[ name ] = temp;
            else if ( val is Effect           efct ) _EFCTs_[ name ] = efct;
        }
    }

    #endregion
    #region PUBLIC   STATIC ACCESSORS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "name"></param>
    /// <returns></returns>
    public static SolidColorBrush? Get_Brush       ( string name ) 
    {
        if ( _BRSHs_ == null || !_BRSHs_.TryGetValue( name, out SolidColorBrush? val ) ) return null;

        return val;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "name"></param>
    /// <returns></returns>
    public static Style?           Get_Style       ( string name ) 
    {
        if ( _STYLs_ == null || !_STYLs_.TryGetValue( name, out Style? val ) ) return null;

        return val;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "name"></param>
    /// <returns></returns>
    public static DataTemplate?    Get_Template    ( string name ) 
    {
        if ( _TEMPs_ == null || !_TEMPs_.TryGetValue( name, out DataTemplate? val ) ) return null;

        return val;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name = "T"   ></typeparam>
    /// <param     name = "name"></param>
    /// <returns></returns>
    public static T?               Get_Effect < T >( string name ) where T : Effect
    {
        if ( _EFCTs_ == null || !_EFCTs_.TryGetValue( name, out Effect? val ) ) return null;

        return val.Clone() as T;
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public interface uiPage {}

/// <summary>
/// 
/// </summary>
/// <param name = "inpt"></param>
/// <param name = "scrl"></param>
/// <param name = "srvs"></param>
public sealed class Login_Page (
    TextBox      inpt = null!,
    ScrollViewer scrl = null!,
    StackPanel   srvs = null!

) : uiPage {
    public TextBox      Input        = inpt;
    public ScrollViewer Scroll_Space = scrl;
    public StackPanel   Servers      = srvs;
}

/// <summary>
/// 
/// </summary>
/// <param name = "inpt"></param>
/// <param name = "scrl"></param>
/// <param name = "msgs"></param>
/// <param name = "actv"></param>
/// <param name = "inac"></param>
public sealed class Chat_Page  (
    TextBox      inpt = null!,
    ScrollViewer scrl = null!,
    StackPanel   msgs = null!,
    StackPanel   actv = null!,
    StackPanel   inac = null!
) : uiPage {
    public TextBox      Input            = inpt;
    public ScrollViewer Scroll_Space     = scrl;
    public StackPanel   Messages         = msgs;
    public StackPanel   Active_Clients   = actv;
    public StackPanel   Inactive_Clients = inac;
}
