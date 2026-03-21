namespace LanChat.SubSystem.Collections;

public sealed class Readonly_Collection < KEY, VAL > where KEY : notnull
{
    /// <summary>
    /// 
    /// </summary>
    private readonly ( KEY[] _KEYs_, VAL[] _VALs_ ) _PAIRs_;

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "dict"></param>
    public Readonly_Collection ( Dictionary < KEY, VAL > dict )
    {
        _PAIRs_._KEYs_ = [ .. dict.Keys   ];
        _PAIRs_._VALs_ = [ .. dict.Values ];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name = "key"></param>
    /// <returns></returns>
    public VAL this[ KEY key ] => _PAIRs_._VALs_[ Array.IndexOf( _PAIRs_._KEYs_, key ) ];
}
