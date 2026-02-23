/// AUTHOR    : Ryan L Harding
///
/// UPDATED   : 2/23/2026 1:30
///
/// REMAINING : ALL ( SUBJECT TO FILL )

using System.Windows;

namespace LanChat.Runtime;

/// <summary>
/// 
/// </summary>
public partial class TermWindow : Window
{
    internal App _APP_;
    public TermWindow ( App app )
    {
        InitializeComponent();

        this._APP_ = app;
    }
}
