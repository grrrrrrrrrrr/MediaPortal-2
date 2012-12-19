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

namespace MediaPortal.Plugins.ShutdownManager
{
  public class Consts
  {
    public const string STR_WF_STATE_ID_SHUTDOWN_MENU = "BBFA7DB7-5055-48D5-A904-0F0C79849369";
    public const string STR_WF_STATE_ID_SHUTDOWN_TIMER = "90FB6BC8-6038-4261-A00F-53774ED11B1A";
    public const string STR_WF_STATE_ID_SHUTDOWN_CONFIGURATION = "F499DC76-2BCE-4126-AF4E-7FEB9DB88E80";

    public static readonly Guid WF_STATE_ID_SHUTDOWN_MENU = new Guid(STR_WF_STATE_ID_SHUTDOWN_MENU);
    public static readonly Guid WF_STATE_ID_SHUTDOWN_TIMER = new Guid(STR_WF_STATE_ID_SHUTDOWN_TIMER);
    public static readonly Guid WF_STATE_ID_SHUTDOWN_CONFIGURATION = new Guid(STR_WF_STATE_ID_SHUTDOWN_CONFIGURATION);

    // Localization resource identifiers
    public const string RES_SHUTDOWN_TIMER_SETUP_MENU_ITEM = "[ShutdownMenu.TimerSetup]";
    public const string RES_SHUTDOWN_TIMER_CANCEL_MENU_ITEM = "[ShutdownMenu.TimerCancel]";

    public const string RES_SYSTEM_HIBERNATE_MENU_ITEM = "[ShutdownMenu.Hibernate]";
    public const string RES_SYSTEM_SHUTDOWN_MENU_ITEM = "[ShutdownMenu.Shutdown]";
    public const string RES_SYSTEM_SUSPEND_MENU_ITEM = "[ShutdownMenu.Suspend]";
    public const string RES_SYSTEM_RESTART_MENU_ITEM = "[ShutdownMenu.Restart]";
    public const string RES_SYSTEM_LOGOFF_MENU_ITEM = "[ShutdownMenu.Logoff]";

    public const string RES_MEDIAPORTAL_MINIMIZE_MENU_ITEM = "[ShutdownMenu.MinimizeMP]";
    public const string RES_MEDIAPORTAL_RESTART_MENU_ITEM = "[ShutdownMenu.RestartMP]";
    public const string RES_MEDIAPORTAL_SHUTDOWN_MENU_ITEM = "[ShutdownMenu.ShutdownMP]";

    // Accessor keys for GUI communication
    public const string KEY_NAME = "Name";
    public const string KEY_INDEX = "Sort-Index";

    public const string KEY_IS_CHECKED = "IsChecked";

    public const string KEY_IS_DOWN_BUTTON_FOCUSED = "IsDownButtonFocused";
    public const string KEY_IS_UP_BUTTON_FOCUSED = "IsUpButtonFocused";

    public static string GetResourceIdentifierForMenuItem(ShutdownAction shutdownAction, bool timerActive = false)
    {
      switch (shutdownAction)
      {
        case ShutdownAction.Suspend:
          return RES_SYSTEM_SUSPEND_MENU_ITEM;
        case ShutdownAction.Shutdown:
          return RES_SYSTEM_SHUTDOWN_MENU_ITEM;
        case ShutdownAction.Hibernate:
          return RES_SYSTEM_HIBERNATE_MENU_ITEM;
        case ShutdownAction.Logoff:
          return RES_SYSTEM_LOGOFF_MENU_ITEM;
        case ShutdownAction.Restart:
          return RES_SYSTEM_RESTART_MENU_ITEM;

        case ShutdownAction.CloseMP:
          return RES_MEDIAPORTAL_SHUTDOWN_MENU_ITEM;
        case ShutdownAction.MinimizeMP:
          return RES_MEDIAPORTAL_MINIMIZE_MENU_ITEM;
        case ShutdownAction.RestartMP:
          return RES_MEDIAPORTAL_RESTART_MENU_ITEM;

        case ShutdownAction.ShutdownTimer:
          return timerActive ? RES_SHUTDOWN_TIMER_CANCEL_MENU_ITEM : RES_SHUTDOWN_TIMER_SETUP_MENU_ITEM;

        default:
          return string.Empty;
      }
    }
  }
}
