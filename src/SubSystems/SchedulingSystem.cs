/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 16:28
/// 
/// REMAINING : FINISHED ( SUBJECT TO UPDATE )

namespace LanChat.SubSystem.Scheduling;

/// <summary>
/// 
/// </summary>
public static class  Time  
{
    #region PUBLIC STATIC PROPERTIES

    public static int Tick { get; private set; } = 1;

    #endregion

    #region PUBLIC STATIC INITIALIZERS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "tick"></param>
    public static void Initialize ( int tick ) => Tick = tick;

    #endregion
}

/// <summary>
/// 
/// </summary>
/// <param name = "proc"></param>
public        struct Sync  ( uint proc = 0, int mult = 1 )
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

    private Status _STAT_ = Status.IDL         ;
    private int    _MULT_ = mult > 0 ? mult : 1;

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
    public                void          Start       () 
    {
        if ( this._STAT_ == Status.END ) return;

        this.Processes++           ;
        this._STAT_ = Status.RUN;
    }

    /// <summary>
    /// 
    /// </summary>
    public                void          Close       () 
    {
        if ( this._STAT_ != Status.RUN ) return;

        this._STAT_ = Status.END;
    }

    /// <summary>
    /// 
    /// </summary>
    public                void          Stop        () 
    {
        if ( this._STAT_ == Status.IDL ) return;

        this.Processes = Math.Max( 0, this.Processes - 1 );

        if ( this.Processes <= 0 ) this._STAT_ = Status.IDL;
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly       void          Yield       () 
    {
        while ( this._STAT_ != Status.IDL ) Thread.Sleep( Time.Tick * this._MULT_ );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public readonly async Task < bool > Yield_Async () 
    {
        while ( this._STAT_ != Status.IDL ) await Task.Delay( Time.Tick * this._MULT_ ).ConfigureAwait( false );
        
        return true;
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
/// <param name = "trys"></param>
public        struct Retry ( uint trys = 0, int mult = 1 )
{
    #region PRIVATE INSTANCE FIELDS

    private uint _TRYS_ = trys               ;
    private int  _MULT_ = mult > 0 ? mult : 1;

    #endregion

    #region PUBLIC  INSTANCE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cndt"></param>
    public readonly bool          Attempt       ( Func < bool > cndt ) 
    {
        uint cur = this._TRYS_ == 0 ? 1 : this._TRYS_;

        while ( cur > 0 )
        {
            if ( cndt() ) return true;

            Thread.Sleep( Time.Tick * this._MULT_ );

            if ( this._TRYS_ > 0 ) cur--;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "cndt"></param>
    /// <returns></returns>
    public readonly async Task < bool > Attempt_Async ( Func < bool > cndt ) 
    {
        uint cur = this._TRYS_ == 0 ? 1 : this._TRYS_;

        while ( cur > 0 )
        {
            if ( cndt() ) return true;

            await Task.Delay( Time.Tick * this._MULT_ ).ConfigureAwait( false );

            if ( this._TRYS_ > 0 ) cur--;
        }
        return false;
    }

    #endregion
}
