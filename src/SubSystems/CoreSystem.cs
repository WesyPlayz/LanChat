/// AUTHOR  : Ryan L Harding
///
/// UPDATED : 2/17/2026 17:08

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

    private Status State = Status.IDL;

    #endregion
    #region PUBLIC  INSTANCE PROPERTIES

    public uint Processes { get; private set; } = proc;

    #endregion
    #region PUBLIC  INSTANCE COMPUTED

    /// <summary>
    /// 
    /// </summary>
    public readonly bool Idle     => this.State == Status.IDL;

    /// <summary>
    /// 
    /// </summary>
    public readonly bool Continue => this.State == Status.RUN;

    #endregion
    #region PUBLIC  INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    public          void Start () 
    {
        if ( this.State == Status.END ) return;

        this.Processes++           ;
        this.State     = Status.RUN;
    }

    /// <summary>
    /// 
    /// </summary>
    public          void Close () 
    {
        if ( this.State != Status.RUN ) return;

        this.State = Status.END;
    }

    /// <summary>
    /// 
    /// </summary>
    public          void Stop  () 
    {
        if ( this.State == Status.IDL ) return;

        this.Processes = Math.Max( 0, this.Processes - 1 );

        if ( this.Processes <= 0 ) this.State = Status.IDL;
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly void Yield () 
    {
        while ( this.State != Status.IDL ) Thread.Sleep( 16 );
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
