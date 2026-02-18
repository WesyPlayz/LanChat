/// AUTHOR  : Ryan L Harding
///
/// UPDATED : 2/17/2026 15:59

#region GENERAL HEADER

using System.Windows.Input;
using static LanChat.InputSystem.Listener;

#endregion

namespace LanChat.InputSystem;

/// <summary>
/// 
/// </summary>
public class Listener 
{
    #region PRIVATE INSTANCE FIELDS

    private List < Action >?[]  _FUNCs_ = [];
    private Key             []? _KEYs_  = [];

    #endregion

    // CONSTRUCTORS //

    /// <summary>
    /// 
    /// </summary>
    public Listener (                                     ) 
    {
        this._FUNCs_ = new List< Action >[ Enum.GetValues< Key >().Length ];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "keys"></param>
    public Listener ( Key[]                         keys  ) 
    {
        this._FUNCs_ = new List< Action >[ keys.Length ];
        this._KEYs_  = keys                             ;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "lsnrs"></param>
    public Listener ( ( Key key, Action[] funcs )[] lsnrs ) 
    {
        List < List< Action >? > funcs = [];
        List < Key             > keys  = [];

        foreach ( (Key key, Action[] funcs ) lsnr in lsnrs )
        {
            if ( !keys.Contains( lsnr.key ) ) keys.Add( lsnr.key );

            int idx = keys.IndexOf( lsnr.key );

            while ( funcs.Count <= idx ) funcs.Add( null );

            funcs[ idx ] ??= [];

            foreach ( Action func in lsnr.funcs ) funcs[ idx ]!.Add( func );
        }
        this._KEYs_  = [ .. keys  ];
        this._FUNCs_ = [ .. funcs ];
    }

    #region PUBLIC  INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "key" ></param>
    /// <param name = "func"></param>
    public void Add    ( Key          key , Action func ) 
    {
        int idx = this._KEYs_ == null ? ( int )key : Array.IndexOf( this._KEYs_, key );

        if ( idx < 0 ) return;

        if ( this._FUNCs_[ idx ] == null ) this._FUNCs_[ idx ] = [];

        this._FUNCs_[ idx ]!.Add( func );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "inpt"></param>
    public void Invoke ( KeyEventArgs inpt              ) 
    {
        if ( Keyboard.Modifiers == ModifierKeys.None )
        {
            int idx = this._KEYs_ == null ? ( int )inpt.Key : Array.IndexOf( this._KEYs_, inpt.Key );

            if ( idx < 0 || this._FUNCs_[ idx ] == null ) return;

            foreach ( Action func in this._FUNCs_[ idx ]! ) func.Invoke();

            inpt.Handled = true;
        }
    }

    #endregion
}
