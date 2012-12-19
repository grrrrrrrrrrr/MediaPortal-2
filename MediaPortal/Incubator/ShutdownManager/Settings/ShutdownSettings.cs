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
using MediaPortal.Common.Settings;

namespace MediaPortal.Plugins.ShutdownManager.Settings
{
  /// <summary>
  /// Shutdown settings class.
  /// </summary>
  public class ShutdownSettings
  {
    private List<ShutdownItem> _shutdownItemList;

    /// <summary>
    /// Constructor
    /// </summary>
    public ShutdownSettings()
    {
      ShutdownItemList = new List<ShutdownItem>();
    }

    [Setting(SettingScope.User, 60)]
    public int? LastCustomShutdownTime { get; set; }

    [Setting(SettingScope.User, ShutdownAction.Suspend)]
    public ShutdownAction? LastCustomShutdownAction { get; set; }

    [Setting(SettingScope.User, null)]
    public List<ShutdownItem> ShutdownItemList
    {
      get
      {
        if (_shutdownItemList == null)
          CreateDefaultShutdownMenu();

        return _shutdownItemList;
      }
      set
      {
        _shutdownItemList = value;
      }
    }

    private void CreateDefaultShutdownMenu()
    {
      _shutdownItemList = new List<ShutdownItem>
                            {
                              new ShutdownItem(ShutdownAction.Suspend, true),
                              new ShutdownItem(ShutdownAction.ShutdownTimer, true),
                              new ShutdownItem(ShutdownAction.Shutdown, true),
                              new ShutdownItem(ShutdownAction.Restart, true),
                              new ShutdownItem(ShutdownAction.CloseMP, true),
                              new ShutdownItem(ShutdownAction.Hibernate, false),
                              new ShutdownItem(ShutdownAction.RestartMP, false),
                              new ShutdownItem(ShutdownAction.MinimizeMP, false),
                              new ShutdownItem(ShutdownAction.Logoff, false)
                            };
    }
  }
}