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

    void ProcessCommandStart(string argument, UpdateType updateSource) {
      switch(RunState) {
        case State.Initialized:
          RunState = State.Starting;
          Runtime.UpdateFrequency = UpdateFrequency.Update1;
          break;
        default:
          Log.Error($"Invalid state {RunState}, recompile to initialize");
          break;
      }
    }

    void ProcessCommandStop(string argument, UpdateType updateSource) {
      switch(RunState) {
        case State.Starting:
        case State.Cutting:
        case State.Advancing:
          Rotor.Rotor.RotorLock = true;
          Drills.Stop();
          RunState = State.Stopping;
          break;
        default:
          Log.Error($"Invalid state {RunState}, not running");
          break;
      }
    }

    void ProcessCommandNext(string argument, UpdateType updateSource) {
      switch(RunState) {
        case State.Cutting:
          TransitionCuttingAdvancing();
          break;
        default:
          Log.Error($"Invalid state {RunState}, not cutting");
          break;
      }
    }

  }
}
