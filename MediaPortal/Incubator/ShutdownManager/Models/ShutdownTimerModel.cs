#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Runtime;
using MediaPortal.Common.Settings;
using MediaPortal.Plugins.ShutdownManager.Settings;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UiComponents.SkinBase.Settings;

namespace MediaPortal.Plugins.ShutdownManager.Models
{
  /// <summary>
  /// Workflow model for the shutdown timer.
  /// </summary>
  public class ShutdownTimerModel : IWorkflowModel
  {
    #region Constants

    public const string SHUTDOWN_TIMER_MODEL_ID_STR = "D5513721-92D8-4E45-B988-2C4DBF055B0F";

    private const int ADDITIONAL_TIMEOUT = 1;

    #endregion

    #region Private fields

    private List<ShutdownItem> _shutdownItemList = null;
    private ItemsList _timerActions;

    private int _currentShutdownIndex;

    private AbstractProperty _customTimeoutProperty;
    private AbstractProperty _currentShutdownActionProperty;

    protected AbstractProperty _isTimerActiveProperty = new WProperty(typeof(bool), false);

    #endregion

    #region Private methods

    /// <summary>
    /// Loads shutdown actions from the settings.
    /// </summary>
    private void GetShutdownActionsFromSettings()
    {
      ShutdownSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<ShutdownSettings>();
      _shutdownItemList = settings.ShutdownItemList;

      // set timeout to last one
      CustomTimeout = (int) settings.LastCustomShutdownTime;

      // set shutdown action to last used one
      _currentShutdownIndex = _shutdownItemList.FindIndex(si => si.Action == settings.LastCustomShutdownAction);

      // if last used shutdownaction has been disabled in the meanwhile, choose next one
      if (!_shutdownItemList[_currentShutdownIndex].Enabled)
        ToggleShutdownAction();
    }

    /// <summary>
    /// Saves currently used values to settings.
    /// </summary>
    private void SaveSettings()
    {
      // save current values
      ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
      ShutdownSettings settings = settingsManager.Load<ShutdownSettings>();

      settings.LastCustomShutdownAction = _shutdownItemList[_currentShutdownIndex].Action;
      settings.LastCustomShutdownTime = CustomTimeout;

      settingsManager.Save(settings);
    }

    private void PrepareTimer(int timeout)
    {
      SaveSettings();

      // activate shutdown timer
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: PrepareTimer shutdownAction={0} timeOut={1}",
        _shutdownItemList[_currentShutdownIndex].Action,
        timeout);
    }

    /// <summary>
    /// todo: get media item's remaining duration
    /// </summary>
    /// <returns></returns>
    private int GetRemainingTimeForMediaItem()
    {
      return 60;
    }

    /// <summary>
    /// todo: get playlist's remaining duration
    /// </summary>
    /// <returns></returns>
    private int GetRemainingTimeForPlaylist()
    {
      return 600;
    }

    #endregion

    #region Public properties (can be used by the GUI)

    public ItemsList TimerActions
    {
      get { return _timerActions; }
    }

    public AbstractProperty CustomTimeoutProperty
    {
      get { return _customTimeoutProperty; }
    }

    public int CustomTimeout
    {
      get { return (int) _customTimeoutProperty.GetValue(); }
      set
      {
        _customTimeoutProperty.SetValue(value);
        UpdateTimerActions();
      }
    }

    public AbstractProperty CurrentShutdownActionProperty
    {
      get { return _currentShutdownActionProperty; }
    }

    public string CurrentShutdownAction
    {
      get { return (string)_currentShutdownActionProperty.GetValue(); }
      set
      {
        _currentShutdownActionProperty.SetValue(value);
        UpdateTimerActions();
      }
    }

    /// <summary>
    /// Exposes the IsTimerActive property.
    /// </summary>
    public AbstractProperty IsTimerActiveProperty
    {
      get { return _isTimerActiveProperty; }
    }

    /// <summary>
    /// Indicates if a shutdown timer is active.
    /// </summary>
    public bool IsTimerActive
    {
      get { return (bool)_isTimerActiveProperty.GetValue(); }
      set { _isTimerActiveProperty.SetValue(value); }
    }

    #endregion

    #region Public methods (can be used by the GUI)

    public void ToggleShutdownAction()
    {
      int oldIndex = _currentShutdownIndex;

      // go through ordered list of shutdown actions, and choose next one, which is enabled
      do
      {
        if (_currentShutdownIndex < _shutdownItemList.Count - 1)
          _currentShutdownIndex++;
        else
          _currentShutdownIndex = 0;
      } while (!_shutdownItemList[_currentShutdownIndex].Enabled || _shutdownItemList[_currentShutdownIndex].Action == ShutdownAction.ShutdownTimer);

      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ToggleShutdownAction oldIndex={0}={1} newIndex={2}={3}",
        oldIndex, _shutdownItemList[oldIndex].Action,
        _currentShutdownIndex, _shutdownItemList[_currentShutdownIndex].Action);

      CurrentShutdownAction = Consts.GetResourceIdentifierForMenuItem(_shutdownItemList[_currentShutdownIndex].Action);

      // done when setting current shutdownaction
      //UpdateTimerActions();
    }

    public void PrepareCustomTimer()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: PrepareCustomTimer");

      int timeout = CustomTimeout;
      PrepareTimer(timeout);
    }

    public void PrepareMediaItemTimer()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: PrepareMediaItemTimer");

      int timeout = GetRemainingTimeForMediaItem();
      PrepareTimer(timeout);
    }

    public void PreparePlaylistTimer()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: PreparePlaylistTimer");

      int timeout = GetRemainingTimeForPlaylist();
      PrepareTimer(timeout);
    }

    public void CancelTimer()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Cancel ShutdownTimer Action has been executed");
    }

    #endregion

    private void UpdateTimerActions()
    {
      _timerActions.Clear();

      //todo: chefkoch, rework into string file, not sure about the solution:   3 strings per shutdown action? 1 base string per shutdown action?
      ILocalization loc = ServiceRegistration.Get<ILocalization>();
      DateTime now = DateTime.Now;
      int timeout;
      string label;

      timeout = CustomTimeout;
      label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_CUSTOM_TIMEOUT, timeout);

      AddTimerAction(label, timeout, now.AddMinutes(timeout), PrepareCustomTimer);

      IPlayerContextManager iPlayerContextManager = ServiceRegistration.Get<IPlayerContextManager>();
      // todo: check for playback of a file
      if (true)
      {
        timeout = GetRemainingTimeForMediaItem();
        label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_MEDIA_ITEM, timeout);

        AddTimerAction(label, timeout, now.AddMinutes(timeout), PrepareMediaItemTimer);
      }

      // todo: check for playback of a playlist (if more than one media item)
      if (true)
      {
        timeout = GetRemainingTimeForPlaylist();
        label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_PLAYLIST, timeout);

        AddTimerAction(label, timeout, now.AddMinutes(timeout), PreparePlaylistTimer);
      }

      _timerActions.FireChange();
    }

    private void AddTimerAction(string label, int timeout, DateTime timeoutTime, ParameterlessMethod command)
    {
      ListItem newItem;
      newItem = new ListItem(Consts.KEY_NAME, label);
      newItem.AdditionalProperties[Consts.KEY_TIMEOUT] = timeout;
      newItem.AdditionalProperties[Consts.KEY_TIME] = timeoutTime;
      newItem.Command = new MethodDelegateCommand(command);
      _timerActions.Add(newItem);
    }

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return new Guid(SHUTDOWN_TIMER_MODEL_ID_STR); }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      _customTimeoutProperty = new WProperty(typeof(int), 120);
      _currentShutdownActionProperty = new WProperty(typeof(string), Consts.GetResourceIdentifierForMenuItem(ShutdownAction.Suspend));
      _timerActions = new ItemsList();
      // Load settings
      GetShutdownActionsFromSettings();
      UpdateTimerActions();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      _timerActions.Clear();
      _timerActions = null;
      _customTimeoutProperty = null;
      _currentShutdownActionProperty = null;
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
      // TODO
    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
    }

    public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
    }

    public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
    {
      // Nothing to do here
    }

    public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
    {
      return ScreenUpdateMode.AutoWorkflowManager;
    }

    #endregion
  }
}