/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 3/13/2026 21:05
///
/// REMAINING : FINISHED ( SUBJECT TO UPDATE )

#region GENERAL HEADER

using System.Runtime.InteropServices;

#endregion

namespace LanChat.SubSystem.Debugging;

// STATIC CLASSES //

/// <summary>
/// 
/// </summary>
public static class Debug 
{
    private const string INVALID_TYPE = "Invalid type for object.";
    private const string NULL_VALUE   = "Object is null."         ;

    private const string EXP          = "Exp : "                  ;
    private const string GOT          = "Got : "                  ;

    [ DllImport( "kernel32.dll" ) ]
    public static extern bool AllocConsole ();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name = "TYPE"        ></typeparam>
    /// <param     name = "obj"         ></param>
    /// <exception cref = "Invalid Type"></exception>
    public static void Is_Type_Exception < TYPE >( string loc, object    obj                 ) 
    {
        if ( obj is not TYPE ) throw new Exception( $"{ loc } { INVALID_TYPE } { EXP } < { typeof( TYPE ) } > { GOT } < { obj.GetType() } >" );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param     name = "loc"       ></param>
    /// <param     name = "obj"       ></param>
    /// <exception cref = "Null Value"></exception>
    public static void Is_Null_Exception         ( string loc, object?   obj                 ) 
    {
        if ( obj == null ) throw new Exception( $"{ loc } { NULL_VALUE }" );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param     name = "loc"       ></param>
    /// <param     name = "objs"      ></param>
    /// <exception cref = "Null Value"></exception>
    public static void All_Is_Null_Exception     ( string loc, object?[] objs                ) 
    {
        foreach ( object? obj in objs ) if ( obj == null ) throw new Exception( $"{ loc } { NULL_VALUE }" );
    }
}
