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

    private string _dateFormat;
    private string _timeFormat;
    CultureInfo _culture;

    private int _currentShutdownIndex;

    private AbstractProperty _customTimeoutProperty;
    private AbstractProperty _currentShutdownActionProperty;

    #endregion

    #region Private methods

    /// <summary>
    /// Loads shutdown actions from the settings.
    /// </summary>
    private void GetShutdownActionsFromSettings()
    {
      ShutdownSettings shutdownSettings = ServiceRegistration.Get<ISettingsManager>().Load<ShutdownSettings>();
      _shutdownItemList = shutdownSettings.ShutdownItemList;

      // set timeout to last one
      CustomTimeout = (int) shutdownSettings.LastCustomShutdownTime;

      // set shutdown action to last used one
      _currentShutdownIndex = _shutdownItemList.FindIndex(si => si.Action == shutdownSettings.LastCustomShutdownAction);

      // if last used shutdownaction has been disabled in the meanwhile, choose next one
      if (!_shutdownItemList[_currentShutdownIndex].Enabled)
        ToggleShutdownAction();

      ILocalization localization = ServiceRegistration.Get<ILocalization>();
      _culture = localization.CurrentCulture;

      SkinBaseSettings skinBaseSettings = ServiceRegistration.Get<ISettingsManager>().Load<SkinBaseSettings>();
      _dateFormat = skinBaseSettings.DateFormat;
      _timeFormat = skinBaseSettings.TimeFormat;
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

    private void ExecuteAfterTimeout(int timeOut)
    {
      SaveSettings();

      // activate shutdown timer
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ExecuteAfterTimeout shutdownAction={0} timeOut={1}",
        _shutdownItemList[_currentShutdownIndex].Action,
        timeOut);
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

    #region localization helpers

    /// <summary>
    /// todo: is it necessary to display the date if timeout is > 24hr or only at next day?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    private string GetLocalizedDateOrTime(DateTime dateTime)
    {
      // todo: check if time (without date) is always enough , i.e. if timout ends the next day
      if(true)
        return GetLocalizedTime(dateTime);

      return GetLocalizedDate(dateTime);
    }

    /// <summary>
    /// todo: Is there are already a Utility to return the localized time?
    /// </summary>
    /// <param name="time"></param>
    /// <returns>localized time as string</returns>
    private string GetLocalizedTime(DateTime time)
    {
      return time.ToString(_timeFormat, _culture);
    }

    /// <summary>
    /// todo: Is there are already a Utility to return the localized date?
    /// </summary>
    /// <param name="date"></param>
    /// <returns>localized time as string</returns>
    private string GetLocalizedDate(DateTime date)
    {
      return date.ToString(_dateFormat, _culture);
    }

    #endregion

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

    public void ExecuteAfterCustomTimeout()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ExecuteAfterCustomTimeout");
      ExecuteAfterTimeout(CustomTimeout);
    }

    public void ExecuteAfterMediaItem()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ExecuteAfterMediaItem");
      ExecuteAfterTimeout(GetRemainingTimeForMediaItem());
    }

    public void ExecuteAfterPlaylist()
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ExecuteAfterPlaylist");
      ExecuteAfterTimeout(GetRemainingTimeForPlaylist());
    }

    #endregion

    private void UpdateTimerActions()
    {
      _timerActions.Clear();

      //todo: chefkoch, rework into string file, not sur about the solution:   3 strings per shutdown action? 1 base string per shutdown action?
      ILocalization loc = ServiceRegistration.Get<ILocalization>();
      ListItem newItem;
      int timeout;
      DateTime now = DateTime.Now;
      string label;

      timeout = CustomTimeout;
      label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_CUSTOM_TIMEOUT, timeout);


      newItem = new ListItem(Consts.KEY_NAME, label);
      newItem.AdditionalProperties[Consts.KEY_TIMEOUT] = timeout;
      newItem.AdditionalProperties[Consts.KEY_TIME] = GetLocalizedDateOrTime(now.AddMinutes(timeout));
      newItem.Command = new MethodDelegateCommand(ExecuteAfterCustomTimeout);
      _timerActions.Add(newItem);


      IPlayerContextManager iPlayerContextManager = ServiceRegistration.Get<IPlayerContextManager>();
      // todo: check for playback of a file
      if (true)
      {
        timeout = GetRemainingTimeForMediaItem();
        label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_MEDIA_ITEM, timeout);

        newItem = new ListItem(Consts.KEY_NAME, label);
        newItem.AdditionalProperties[Consts.KEY_TIMEOUT] = timeout;
        newItem.AdditionalProperties[Consts.KEY_TIME] = GetLocalizedDateOrTime(now.AddMinutes(timeout));
        newItem.Command = new MethodDelegateCommand(ExecuteAfterMediaItem);
        _timerActions.Add(newItem);
      }

      // todo: check for playback of a playlist (if more than one media item)
      if (true)
      {
        timeout = GetRemainingTimeForPlaylist();
        label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_PLAYLIST, timeout);

        newItem = new ListItem(Consts.KEY_NAME, label);
        newItem.AdditionalProperties[Consts.KEY_TIMEOUT] = timeout;
        newItem.AdditionalProperties[Consts.KEY_TIME] = GetLocalizedDateOrTime(now.AddMinutes(timeout));
        newItem.Command = new MethodDelegateCommand(ExecuteAfterPlaylist);
        _timerActions.Add(newItem);
      }

      _timerActions.FireChange();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="shutdownAction">shutdown action to execute</param>
    /// <param name="timeOut">timeout in minutes</param>
    public void ExecuteAfterTimeout(string shutdownAction, int timeOut)
    {
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ExecuteAfterTimeout shutdownAction={0} timeOut={1}", shutdownAction, timeOut);
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