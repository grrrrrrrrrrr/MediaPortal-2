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

namespace MediaPortal.Plugins.SystemStateMenu
{
  public class Consts
  {
    public const string STR_WF_STATE_ID_SYSTEM_STATE_DIALOG = "BBFA7DB7-5055-48D5-A904-0F0C79849369";
    public const string STR_WF_STATE_ID_SYSTEM_STATE_CONFIGURATION_DIALOG = "F499DC76-2BCE-4126-AF4E-7FEB9DB88E80";
    public const string STR_WF_STATE_ID_SLEEPTIMER_DIALOG = "90FB6BC8-6038-4261-A00F-53774ED11B1A";

    public static readonly Guid WF_STATE_ID_SYSTEM_STATE_DIALOG = new Guid(STR_WF_STATE_ID_SYSTEM_STATE_DIALOG);
    public static readonly Guid WF_STATE_ID_SYSTEM_STATE_CONFIGURATION_DIALOG = new Guid(STR_WF_STATE_ID_SYSTEM_STATE_CONFIGURATION_DIALOG);
    public static readonly Guid WF_STATE_ID_SLEEPTIMER_DIALOG = new Guid(STR_WF_STATE_ID_SLEEPTIMER_DIALOG);

    // Localization resource identifiers
    public const string RES_SLEEP_TIMER_SETUP_MENU_ITEM = "[SystemState.SetupSleepTimer]";
    public const string RES_SLEEP_TIMER_CANCEL_MENU_ITEM = "[SystemState.CancelSleepTimer]";

    public const string RES_SYSTEM_HIBERNATE_MENU_ITEM = "[SystemState.Hibernate]";
    public const string RES_SYSTEM_SHUTDOWN_MENU_ITEM = "[SystemState.Shutdown]";
    public const string RES_SYSTEM_SUSPEND_MENU_ITEM = "[SystemState.Suspend]";
    public const string RES_SYSTEM_RESTART_MENU_ITEM = "[SystemState.Restart]";
    public const string RES_SYSTEM_LOGOFF_MENU_ITEM = "[SystemState.Logoff]";

    public const string RES_MEDIAPORTAL_MINIMIZE_MENU_ITEM = "[SystemState.MinimizeMP]";
    public const string RES_MEDIAPORTAL_RESTART_MENU_ITEM = "[SystemState.RestartMP]";
    public const string RES_MEDIAPORTAL_SHUTDOWN_MENU_ITEM = "[SystemState.ShutdownMP]";

    public const string RES_SHUTDOWN_AFTER_CUSTOM_TIMEOUT = "[SleepTimer.AfterCustomTimer]";
    public const string RES_SHUTDOWN_AFTER_MEDIA_ITEM = "[SleepTimer.AfterMediaItem]";
    public const string RES_SHUTDOWN_AFTER_PLAYLIST = "[SleepTimer.AfterPlaylist]";

    public const string RES_SLEEP_TIMER_NOTIFY_HEADER = "[SleepTimer.Notify.Header]";

    // Accessor keys for GUI communication
    public const string KEY_NAME = "Name";
    public const string KEY_INDEX = "Sort-Index";

    // ShutdownTimerModel
    public const string KEY_TIMEOUT = "Timeout";
    public const string KEY_TIME = "Time";

    // ShutdownConfigurationDialogModel
    public const string KEY_IS_CHECKED = "IsChecked";
    public const string KEY_IS_DOWN_BUTTON_FOCUSED = "IsDownButtonFocused";
    public const string KEY_IS_UP_BUTTON_FOCUSED = "IsUpButtonFocused";

    public static string GetResourceIdentifierForMenuItem(SystemStateAction systemStateAction, bool timerActive = false)
    {
      switch (systemStateAction)
      {
        case SystemStateAction.Suspend:
          return RES_SYSTEM_SUSPEND_MENU_ITEM;
        case SystemStateAction.Hibernate:
          return RES_SYSTEM_HIBERNATE_MENU_ITEM;
        case SystemStateAction.Shutdown:
          return RES_SYSTEM_SHUTDOWN_MENU_ITEM;
        case SystemStateAction.Logoff:
          return RES_SYSTEM_LOGOFF_MENU_ITEM;
        case SystemStateAction.Restart:
          return RES_SYSTEM_RESTART_MENU_ITEM;

        case SystemStateAction.CloseMP:
          return RES_MEDIAPORTAL_SHUTDOWN_MENU_ITEM;
        case SystemStateAction.MinimizeMP:
          return RES_MEDIAPORTAL_MINIMIZE_MENU_ITEM;
        case SystemStateAction.RestartMP:
          return RES_MEDIAPORTAL_RESTART_MENU_ITEM;

        case SystemStateAction.SleepTimer:
          return timerActive ? RES_SLEEP_TIMER_CANCEL_MENU_ITEM : RES_SLEEP_TIMER_SETUP_MENU_ITEM;

        default:
          return string.Empty;
      }
    }

    public static string GetTimerMessage(SystemStateAction systemStateAction, DateTime shutdownTime)
    {
      return String.Format("{0} in {1} min", systemStateAction, shutdownTime.Subtract(DateTime.Now).TotalMinutes);
    }
  }
}
