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

    void ProcessTickStarting(string argument, UpdateType updateSource) {
      if(Drills.MyState == DrillManager.State.Stopped) {
        Log.Info("Starting drills...");
        Drills.Start();
        return;
      }
      ProcessInventory();
      if(Drills.MyState == DrillManager.State.Starting) {
        Log.Info($"Starting drills: {Drills.Startup.Count}/{Drills.Drills.Count} remaining");
        Log.Info($"Progress: {(Drills.Drills.Count - Drills.Startup.Count) * 100f / Drills.Drills.Count:N2}%");
        return;
      }
      Rotor.Mark();
      Rotor.Rotor.RotorLock = false;
      RunState = State.Cutting;
    }

    void ProcessTickCutting(string argument, UpdateType updateSource) {
      ProcessInventory();
      var travelDeg = Rotor.TravelDeg;
      if(travelDeg < 0) travelDeg = -travelDeg;
      Log.Info($"Mark: {Rotor.StartAngleDeg:N2}°");
      Log.Info($"Travel: {Rotor.TravelDeg:N2}°");
      Log.Info($"Progress: {travelDeg * 100f / RotorStep:N2}%");
      if(travelDeg >= RotorStep) {
        TransitionCuttingAdvancing();
      }
    }

    void TransitionCuttingAdvancing() {
      if(Pistons.State == PistonStatus.Extended) {
        Rotor.Rotor.RotorLock = true;
        Drills.Stop();
        RunState = State.Stopping;
        return;
      }
      var targetPosition = Pistons.CurrentPosition + PistonStep;
      if(targetPosition > Pistons.HighestPosition) {
        targetPosition = Pistons.HighestPosition;
      }
      Pistons.Mark();
      Pistons.MoveTo(targetPosition, PistonSpeed);
      RunState = State.Advancing;
    }

    void ProcessTickAdvancing(string argument, UpdateType updateSource) {
      ProcessInventory();
      Log.Info($"Target: {Pistons.TargetPosition:N2}m");
      Log.Info($"Current: {Pistons.CurrentPosition:N2}m");
      Log.Info($"Progress: {(Pistons.CurrentPosition - Pistons.MarkedPosition) * 100f / (Pistons.TargetPosition - Pistons.MarkedPosition):N2}%");
      if(Pistons.State == PistonStatus.Extending) {
        return;
      }
      Rotor.Mark();
      RunState = State.Cutting;
    }

    void ProcessTickStopping(string argument, UpdateType updateSource) {
      Log.Info("Finishing inventory processing...");
      if(ProcessInventory()) {
        RunState = State.Stopped;
      }
    }

    void ProcessTickStopped(string argument, UpdateType updateSource) {
      Log.Info("Completed.");
      Runtime.UpdateFrequency = UpdateFrequency.None;
    }

  }
}
