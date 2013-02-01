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
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Runtime;
using MediaPortal.Common.Settings;
using MediaPortal.Plugins.ShutdownManager.Settings;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;

namespace MediaPortal.Plugins.ShutdownManager.Models
{
  /// <summary>
  /// Workflow model for the shutdown menu.
  /// </summary>
  public class ShutdownMenuModel : IWorkflowModel
  {
    public const string SHUTDOWN_MENU_MODEL_ID_STR = "25F16911-ED0D-4439-9858-5E69C970C037";

    #region Private fields

    private ItemsList _shutdownItems = null;

    public ShutdownMenuModel()
    {
      ShutdownItemList = null;
    }

    #endregion

    private List<ShutdownItem> ShutdownItemList { get; set; }

    #region Private members

    private void GetShutdownActionsFromSettings()
    {
      ShutdownSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<ShutdownSettings>();
      ShutdownItemList = settings.ShutdownItemList;
    }

    private bool TryGetAction(ListItem item, out ShutdownAction action)
    {
      action = ShutdownAction.Suspend;
      if (item == null)
        return false;

      object oIndex;
      if (item.AdditionalProperties.TryGetValue(Consts.KEY_INDEX, out oIndex))
      {
        int? i = oIndex as int?;
        if (i.HasValue)
        {
          action = ShutdownItemList[i.Value].Action;
          return true;
        }
      }
      return false;
    }

    private void UpdateShutdownItems()
    {
      _shutdownItems.Clear();
      if (ShutdownItemList != null)
      {
        for (int i = 0; i < ShutdownItemList.Count; i++)
        {
          ShutdownItem si = ShutdownItemList[i];

          // hide disabled items
          if (!si.Enabled)
            continue;

          ListItem item = new ListItem();
          item.SetLabel(Consts.KEY_NAME, Consts.GetResourceIdentifierForMenuItem(si.Action));

          item.AdditionalProperties[Consts.KEY_INDEX] = i;
          _shutdownItems.Add(item);
        }
      }
      _shutdownItems.FireChange();
    }

    #endregion

    #region Public properties (can be used by the GUI)

    public ItemsList ShutdownItems
    {
      get { return _shutdownItems; }
    }

    #endregion

    public static void DoAction(ShutdownAction action)
    {
      switch (action)
      {
        case ShutdownAction.Suspend:
          //ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Suspend Action has been executed");
          ServiceRegistration.Get<ISystemStateService>().Suspend();
          return;

        case ShutdownAction.Shutdown:
          //ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Shutdown Action has been executed");
          ServiceRegistration.Get<ISystemStateService>().Shutdown();
          return;

        case ShutdownAction.Restart:
          //ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Restart Action has been executed");
          ServiceRegistration.Get<ISystemStateService>().Restart();
          return;

        case ShutdownAction.Logoff:
          //ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Logoff Action has been executed");
          ServiceRegistration.Get<ISystemStateService>().Logoff();
          return;


        case ShutdownAction.CloseMP:
          //ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Close MediaPortal Action has been executed");
          ServiceRegistration.Get<IScreenControl>().Shutdown();
          return;

        case ShutdownAction.MinimizeMP:
          //ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Minimize MediaPortal Action has been executed");
          ServiceRegistration.Get<IScreenControl>().Minimize();
          return;

        case ShutdownAction.RestartMP:
          ServiceRegistration.Get<ILogger>().Debug("ShutdownManager: Restart MediaPortal Action has been executed");
          // todo: chefkoch, 2013-01-31: not implemented, yet
          //ServiceRegistration.Get<IScreenControl>().Restart();
          return;


        case ShutdownAction.ShutdownTimer:
          // todo: chefkoch, 2013-01-01: check from ShutdownTimerModel if timer is active
          //if (IsTimerActive)
          //  CancelTimer();
          //else
          ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Consts.WF_STATE_ID_SHUTDOWN_TIMER);
          return;
      }
    }

    #region Public methods (can be used by the GUI)

    /// <summary>
    /// Provides a callable method for the skin to execute the given shutdown <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The choosen item.</param>
    public void Select(ListItem item)
    {
      if (item == null)
        return;

      ShutdownAction action;
      if (!TryGetAction(item, out action))
        return;

      // todo: chefkoch, 2012-12-15: should this be done by the skin file?
      ServiceRegistration.Get<IWorkflowManager>().NavigatePop(1);

      DoAction(action);
    }

    #endregion

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return new Guid(SHUTDOWN_MENU_MODEL_ID_STR); }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      ShutdownItemList = new List<ShutdownItem>();
      _shutdownItems = new ItemsList();
      //// Load settings
      GetShutdownActionsFromSettings();
      UpdateShutdownItems();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      _shutdownItems.Clear();
      _shutdownItems = null;
      //_isTimerActiveProperty = null;
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