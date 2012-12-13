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
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Settings;
using MediaPortal.Plugins.SystemStateMenu.Settings;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;

namespace MediaPortal.Plugins.SystemStateMenu.Models
{
  /// <summary>
  /// Workflow model for the sleep timer.
  /// </summary>
  public class SleepTimerModel : IWorkflowModel
  {
    #region Constants

    private const string ASPECT_ID_AUDIO = "435E2E99-1546-444C-AE83-4843A3094533";
    private const string ASPECT_ID_VIDEO = "89B0883D-0CEC-4B5B-8783-C923C59BB87C";

    public const string SLEEP_TIMER_MODEL_ID_STR = "D5513721-92D8-4E45-B988-2C4DBF055B0F";

    private const int ADDITIONAL_TIMEOUT = 1;

    private static readonly int[] NotifyIntervals = new int[] {1, 3, 5, 10, 30};

    #endregion

    #region Private fields

    private List<SystemStateItem> _shutdownItemList = null;
    private ItemsList _timerActions;

    private int _currentActionIndex;

    private AbstractProperty _customTimeoutProperty;
    private AbstractProperty _currentSleepActionProperty;
    private AbstractProperty _currentSleepActionTextProperty;
    private AbstractProperty _sleepTimeProperty;

    protected AbstractProperty _isTimerActiveProperty = new WProperty(typeof(bool), false);

    private Timer _sleepTimer;
    private Timer _notificationTimer;

    #endregion

    #region Private methods

    /// <summary>
    /// Loads shutdown actions from the settings.
    /// </summary>
    private void GetShutdownActionsFromSettings()
    {
      SystemStateDialogSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<SystemStateDialogSettings>();
      _shutdownItemList = settings.ShutdownItemList;

      // set timeout to last one
      CustomTimeout = (int) settings.LastCustomSleepTimeout;

      // set shutdown action to last used one
      _currentActionIndex = _shutdownItemList.FindIndex(si => si.Action == settings.LastCustomSleepAction);
      CurrentSleepAction = _shutdownItemList[_currentActionIndex].Action;

      // if last used shutdownaction has been disabled in the meanwhile, choose next one
      if (!_shutdownItemList[_currentActionIndex].Enabled)
        ToggleSleepAction();
    }

    /// <summary>
    /// Saves currently used values to settings.
    /// </summary>
    private void SaveSettings()
    {
      // save current values
      ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
      SystemStateDialogSettings settings = settingsManager.Load<SystemStateDialogSettings>();

      settings.LastCustomSleepAction = CurrentSleepAction;
      settings.LastCustomSleepTimeout = CustomTimeout;

      settingsManager.Save(settings);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout">Timout in min to execute the SystemStateAction.</param>
    private void PrepareTimer(int timeout)
    {
      SaveSettings();

      // activate shutdown timer
      ServiceRegistration.Get<ILogger>().Debug("SystemStateMenu: PrepareTimer SystemStateAction={0} timeOut={1}",
                                               _shutdownItemList[_currentActionIndex].Action,
                                               timeout);

      _sleepTimer = new Timer(timeout*60*1000);
      _sleepTimer.Elapsed += TimerShutDown_Elapsed;
      _sleepTimer.AutoReset = false;
      _sleepTimer.Start();
      // set property
      SleepTime = DateTime.Now.AddMinutes(timeout);
      // setup notification
      SetupNotificationTimer();
    }

    private void SetupNotificationTimer()
    {
      int remaining = (int)Math.Truncate(SleepTime.Subtract(DateTime.Now).TotalMinutes) + 1;
      int nextNotify = int.MinValue;
      var res = NotifyIntervals.Where(s => s < remaining);
      if (res.Any())
        nextNotify = res.Max();

      if (nextNotify != int.MinValue)
      {
        _notificationTimer = new Timer();
        _notificationTimer.Elapsed += NotificationTimer_Elapsed;
        _notificationTimer.Interval = (remaining - nextNotify)*60*1000;
        _notificationTimer.AutoReset = false;
        _notificationTimer.Start();
      }
    }

    /// <summary>
    /// todo: get media item's remaining duration
    /// </summary>
    /// <returns></returns>
    private int GetRemainingTimeForMediaItem()
    {
      //IPlayerContextManager iPlayerContextManager = ServiceRegistration.Get<IPlayerContextManager>();

      //iPlayerContextManager.PrimaryPlayerContext.CurrentMediaItem.Aspects[vi]

      //timeout = GetRemainingTimeForMediaItem();
      //label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_MEDIA_ITEM, timeout);

      //AddTimerAction(label, timeout, now.AddMinutes(timeout), PrepareMediaItemTimer);

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

    public AbstractProperty CurrentSleepActionTextProperty
    {
      get { return _currentSleepActionTextProperty; }
    }

    public string CurrentSleepActionText
    {
      get { return (string)_currentSleepActionTextProperty.GetValue(); }
      set
      {
        _currentSleepActionTextProperty.SetValue(value);
      }
    }

    public AbstractProperty CurrentSleepActionProperty
    {
      get { return _currentSleepActionProperty; }
    }

    public SystemStateAction CurrentSleepAction
    {
      get { return (SystemStateAction)_currentSleepActionProperty.GetValue(); }
      set
      {
        _currentSleepActionProperty.SetValue(value);
        CurrentSleepActionText = Consts.GetResourceIdentifierForMenuItem(value);
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
    /// Indicates if a sleep timer is active.
    /// </summary>
    public bool IsTimerActive
    {
      get { return (bool)_isTimerActiveProperty.GetValue(); }
      set { _isTimerActiveProperty.SetValue(value); }
    }

    /// <summary>
    /// Exposes the SleepTime property.
    /// </summary>
    public AbstractProperty SleepTimeProperty
    {
      get { return _sleepTimeProperty; }
    }

    /// <summary>
    /// Represents the time, when the system state action will be executed
    /// </summary>
    public DateTime SleepTime
    {
      get { return (DateTime)_sleepTimeProperty.GetValue(); }
      set { _sleepTimeProperty.SetValue(value); }
    }

    #endregion

    #region Public methods (can be used by the GUI)

    public void ToggleSleepAction()
    {
      int oldIndex = _currentActionIndex;

      // go through ordered list of shutdown actions, and choose next one, which is enabled
      do
      {
        if (_currentActionIndex < _shutdownItemList.Count - 1)
          _currentActionIndex++;
        else
          _currentActionIndex = 0;
      } while (!_shutdownItemList[_currentActionIndex].Enabled || _shutdownItemList[_currentActionIndex].Action == SystemStateAction.SleepTimer);

      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: ToggleShutdownAction oldIndex={0}={1} newIndex={2}={3}",
        oldIndex, _shutdownItemList[oldIndex].Action,
        _currentActionIndex, _shutdownItemList[_currentActionIndex].Action);

      CurrentSleepAction = _shutdownItemList[_currentActionIndex].Action;

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
      ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Cancel SleepTimer Action has been executed");
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

      //IPlayerContextManager iPlayerContextManager = ServiceRegistration.Get<IPlayerContextManager>();
      //if (iPlayerContextManager.NumActivePlayerContexts > 0)
      //{
      //  timeout = GetRemainingTimeForMediaItem();
      //  label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_MEDIA_ITEM, timeout);

      //  AddTimerAction(label, timeout, now.AddMinutes(timeout), PrepareMediaItemTimer);

      // todo: check for playback of a playlist (if more than one media item)
      //if (true)
      //{
      //  timeout = GetRemainingTimeForPlaylist();
      //  label = loc.ToString(Consts.RES_SHUTDOWN_AFTER_PLAYLIST, timeout);

      //  AddTimerAction(label, timeout, now.AddMinutes(timeout), PreparePlaylistTimer);
      //}
      //}

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

    void TimerShutDown_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      SystemStateAction action = _shutdownItemList[_currentActionIndex].Action;
      SystemStateModel.DoAction(action);
    }

    void NotificationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      IDialogManager dialogManager = ServiceRegistration.Get<IDialogManager>();
      string header = LocalizationHelper.Translate(Consts.RES_SLEEP_TIMER_NOTIFY_HEADER);
      string text = Consts.GetTimerMessage(CurrentSleepAction, SleepTime);
      dialogManager.ShowDialog(header, text, DialogType.OkDialog, false, DialogButtonType.Ok);
      SetupNotificationTimer();
    }

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return new Guid(SLEEP_TIMER_MODEL_ID_STR); }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      _customTimeoutProperty = new WProperty(typeof(int), 120);
      _currentSleepActionProperty = new WProperty(typeof(SystemStateAction), SystemStateAction.Suspend);
      _currentSleepActionTextProperty = new WProperty(typeof(string), Consts.GetResourceIdentifierForMenuItem(SystemStateAction.Suspend));
      _sleepTimeProperty = new WProperty(typeof(DateTime), DateTime.MinValue);
      _timerActions = new ItemsList();
      // Load settings
      GetShutdownActionsFromSettings();
      UpdateTimerActions();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      //_timerActions.Clear();
      //_timerActions = null;
      //_customTimeoutProperty = null;
      //_currentShutdownActionProperty = null;
      //_currentShutdownActionTextProperty = null;
      //_shutdownTimeProperty = null;
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