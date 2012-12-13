using MediaPortal.Common;
using MediaPortal.Common.PluginManager;

#region keybinding instead of workflowstate

//  [16:54]	@morpheus_xx: but for a normal dialog you might not even need a workflow state, you can open dialog from xaml
//  [16:54]	@morpheus_xx: i.e. slimtv fullscreen binds to "enter" to open the miniguide
//  [16:55]	@chefkoch: k, i'll look into it
//  [16:55]	@chefkoch: thx
//---------------------------------------------------------------------------------------
//  [16:56]	@morpheus_xx: <KeyBinding Key="Ok" Command="{Command Source={Service ScreenManager},Path=ShowDialog,Parameters=SlimTVClient-miniguide}" IsEnabled="{Binding !IsOSDVisible}"/>
//  [16:56]	@morpheus_xx: fullscreenContenttv.xaml

#endregion

namespace MediaPortal.Plugins.ShutdownManager
{
  public class ShutdownManagerPlugin : IPluginStateTracker
  {
    #region IPluginStateTracker implementation

    public void Activated(PluginRuntime pluginRuntime)
    {
      //ServiceRegistration.Set<IWeatherCatcher>(new WorldWeatherOnlineCatcher());
    }

    public bool RequestEnd()
    {
      return true;
    }

    public void Stop()
    {
      //ServiceRegistration.Remove<IWeatherCatcher>();
    }

    public void Continue() { }

    void IPluginStateTracker.Shutdown() { }

    #endregion
  }
}