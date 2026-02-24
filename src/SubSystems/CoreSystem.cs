/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 15:29
/// 
/// REMAINING : PARTIAL ( SUBJECT TO FILL )

namespace LanChat.SubSystem.Core;

/// <summary>
/// 
/// </summary>
/// <param name = "proc"></param>
public struct Sync  ( uint proc = 0 )
{
    #region PUBLIC  ENUM

    public enum Status
    {
        IDL,
        RUN,
        END
    }

    #endregion

    #region PRIVATE INSTANCE FIELDS

    private Status _STAT_ = Status.IDL;

    #endregion
    #region PUBLIC  INSTANCE PROPERTIES

    public uint Processes { get; private set; } = proc;

    #endregion

    #region PUBLIC  INSTANCE COMPUTED

    /// <summary>
    /// 
    /// </summary>
    public readonly bool Idle     => this._STAT_ == Status.IDL;

    /// <summary>
    /// 
    /// </summary>
    public readonly bool Continue => this._STAT_ == Status.RUN;

    #endregion

    #region PUBLIC  INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    public          void Start () 
    {
        if ( this._STAT_ == Status.END ) return;

        this.Processes++           ;
        this._STAT_ = Status.RUN;
    }

    /// <summary>
    /// 
    /// </summary>
    public          void Close () 
    {
        if ( this._STAT_ != Status.RUN ) return;

        this._STAT_ = Status.END;
    }

    /// <summary>
    /// 
    /// </summary>
    public          void Stop  () 
    {
        if ( this._STAT_ == Status.IDL ) return;

        this.Processes = Math.Max( 0, this.Processes - 1 );

        if ( this.Processes <= 0 ) this._STAT_ = Status.IDL;
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly void Yield () 
    {
        while ( this._STAT_ != Status.IDL ) Thread.Sleep( 16 );
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
/// <param name = "trys"></param>
public struct Retry ( uint trys = 0 )
{
    private uint _TRYS_ = trys;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cndt"></param>
    public       bool          Attempt       ( Func < bool > cndt ) 
    {
        uint cur = this._TRYS_ == 0 ? 1 : this._TRYS_;

        while ( cur > 0 )
        {
            if ( cndt() ) return true;

            Thread.Sleep( 16 );

            if ( this._TRYS_ > 0 ) cur--;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cndt"></param>
    /// <returns></returns>
    public async Task < bool > Attempt_Async ( Func < bool > cndt ) 
    {
        uint cur = this._TRYS_ == 0 ? 1 : this._TRYS_;

        while ( cur > 0 )
        {
            if ( cndt() ) return true;

            await Task.Delay( 1000 ).ConfigureAwait( false );

            if ( this._TRYS_ > 0 ) cur--;
        }
        return false;
    }
}
