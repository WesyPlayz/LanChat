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