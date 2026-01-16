#region GENERAL HEADER

using System.Windows;

#endregion
#region LANCHAT HEADER

using LanChat.NetworkSystem;

#endregion

namespace LanChat.Runtime;

public partial class App : Application 
{
    protected override void OnStartup ( StartupEventArgs _ )
    {
        MainWindow = new MainWindow(); 
        MainWindow.Show();

        Bridge.Init_Authentication();
    }
}