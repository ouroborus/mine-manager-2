using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript {
  partial class Program {

    bool Init() {

      var connectors = new List<IMyShipConnector>();
      var containers = new List<IMyCargoContainer>();
      var drills = new List<IMyShipDrill>();
      var ejectors = new List<IMyShipConnector>();
      var pistons = new List<IMyPistonBase>();
      var refineries = new List<IMyRefinery>();
      var sorters = new List<IMyConveyorSorter>();
      IMyMotorStator rotor = null;

      try {
        GridTerminalSystem.GetBlocksOfType((List<IMyTerminalBlock>)null, block => {
          if(!Me.IsSameConstructAs(block)) {
            return false;
          }

          var drill = block as IMyShipDrill;
          if(drill != null) {
            drills.Add(drill);
            return false;
          }
          var sorter = block as IMyConveyorSorter;
          if(sorter != null) {
            sorters.Add(sorter);
            return false;
          }
          var piston = block as IMyPistonBase;
          if(piston != null) {
            pistons.Add(piston);
            return false;
          }
          var connector = block as IMyShipConnector;
          if(connector != null) {
            (connector.ThrowOut ? ejectors : connectors).Add(connector);
            return false;
          }
          var refinery = block as IMyRefinery;
          if(refinery != null) {
            refineries.Add(refinery);
            return false;
          }
          var container = block as IMyCargoContainer;
          if(container != null) {
            containers.Add(container);
            return false;
          }

          var _rotor = block as IMyMotorStator;
          if(rotor == null) {
            rotor = _rotor;
            return false;
          } else if(_rotor != null) {
            throw new ConstraintException("Structure must include a single rotor");
          }

          return false;
        });
      }
      catch(ConstraintException ex) {
        Log.Error(ex.Message);
        return false;
      }
      if(drills.Count == 0) {
        Log.Error("Structure must include drills");
        return false;
      }
      if(pistons.Count == 0) {
        Log.Error("Structure must include pistons");
        return false;
      }
      if(ejectors.Count == 0) {
        Log.Error("Structure must include connectors with \"Throw out\" enabled");
        return false;
      }
      if(refineries.Count == 0) {
        Log.Error("Structure must include refineries");
        return false;
      }
      if(containers.Count == 0) {
        Log.Error("Structure must include containers");
        return false;
      }

      Ejectors = new EjectionManager(this, ejectors);
      Containers = new ContainerManager(this, containers);
      Drills = new DrillManager(this, drills);
      Pistons = new PistonManager(this, pistons);
      Refineries = new RefineryManager(this, refineries);

      Rotor = new RotorTracker(this, rotor);
      rotor.RotorLock = true;
      rotor.TargetVelocityRPM = RotorSpeed;

      if(!IsFullyConnected()) {
        Log.Error("Inventories must be fully connected");
        return false;
      }

      //foreach(var sorter in sorters) {
      //  sorter.DrainAll = false;
      //}

      Updater = new UpdateManager(this) {
        Ejectors,
        Containers,
        Drills,
        Pistons,
        Refineries,
        Rotor
      };

      float capTotal = 0;
      float capUsed = 0;
      foreach(var container in containers) {
        var inv = container.GetInventory(0);
        capTotal += (float)inv.MaxVolume;
        capUsed += (float)inv.CurrentVolume;
      }
      capTotal *= 1000;
      capUsed *= 1000;
      float capAvail = capTotal - capUsed;

      var width = (new[] {
        drills.Count,
        pistons.Count,
        ejectors.Count,
        refineries.Count,
        containers.Count,
      }).Max().ToString().Length;

      Log.Info("Found:\n"
        + $"  {drills.Count.ToString().PadLeft(width)} {(drills.Count == 1 ? "drill" : "drills")}\n"
        + $"  {pistons.Count.ToString().PadLeft(width)} {(pistons.Count == 1 ? "piston" : "pistons")}\n"
        + $"  {"1".PadLeft(width)} rotor\n"
        + $"  {ejectors.Count.ToString().PadLeft(width)} {(ejectors.Count == 1 ? "ejector" : "ejectors")}\n"
        + $"  {refineries.Count.ToString().PadLeft(width)} {(refineries.Count == 1 ? "refinery" : "refineries")}\n"
        + $"  {containers.Count.ToString().PadLeft(width)} {(containers.Count == 1 ? "container" : "containers")} "
          + $"({capAvail * 100f / capTotal:N2}% of {capTotal:N2}L available)\n"
        + "Initialization complete");

      return true;
    }

    bool IsFullyConnected() {
      var src = Containers.Containers[0];
      var srcInv = src.GetInventory(0);

      foreach(var dst in Containers.Containers) {
        var dstInv = dst.GetInventory(0);
        if(dst != src && (!srcInv.IsConnectedTo(dstInv) || !dstInv.IsConnectedTo(srcInv))) {
          return false;
        }
      }

      // Not really fully connected. Only checking one direction here.
      foreach(var dst in Drills.Drills) {
        var dstInv = dst.GetInventory(0);
        if(!dstInv.IsConnectedTo(srcInv)) {
          return false;
        }
      }

      foreach(var dst in Ejectors.Ejectors) {
        var dstInv = dst.GetInventory(0);
        if(!srcInv.IsConnectedTo(dstInv) || !dstInv.IsConnectedTo(srcInv)) {
          return false;
        }
      }

      foreach(var dst in Refineries.Refineries) {
        if(!srcInv.IsConnectedTo(dst.InputInventory) || !dst.InputInventory.IsConnectedTo(srcInv)
          || !srcInv.IsConnectedTo(dst.OutputInventory) || !dst.OutputInventory.IsConnectedTo(srcInv)
        ) {
          return false;
        }
      }

      return true;
    }

  }
}
