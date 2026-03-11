/// AUTHOR    : Ryan L Harding
/// 
/// UPDATED   : 3/03/2026 12:44
/// 
/// REMAINING : ALL ( SUBJECT TO FILL )

namespace Project.SubSystems.Authentication;

// ABSTRACT CLASSES //

/// <summary>
/// 
/// </summary>
public abstract class Account  
{
    #region PROTECTED INSTANCE PROPERTIES

    protected uint _ATCR_ { get; set; }

    #endregion

    #region INTERNAL  INSTANCE COMPUTED

    /// <summary>
    /// 
    /// </summary>
    internal uint Authenticator => this._ATCR_;

    #endregion

    #region INTERNAL  ABSTRACT FUNCTIONS

    internal abstract void _AUTH_();

    #endregion
}

// SEALED CLASSES //

/// <summary>
/// 
/// </summary>
public sealed   class User     : Account
{
    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 1, 10 ) );
    }

    #endregion
}

/// <summary>
/// 
/// </summary>
public sealed   class Admin    : Account
{
    #region INTERNAL OVERRIDE FUNCTIONS

    /// <summary>
    /// 
    /// </summary>
    internal override void _AUTH_ () 
    {
        if ( this._ATCR_ != uint.MinValue ) return;

        this._ATCR_ = ( uint )( new Random().NextInt64( 4000000000, uint.MaxValue ) );
    }

    #endregion
}
