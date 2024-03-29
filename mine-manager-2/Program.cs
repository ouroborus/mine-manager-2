﻿using Sandbox.Game.EntityComponents;
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
  public partial class Program : MyGridProgram {

    // CONFIG BEGIN
    const float PistonStep = 2f; // meters for each layer
    const float PistonSpeed = 0.25f; // piston speed in m/s
    const float RotorStep = 180f; // degrees for each layer
    const float RotorSpeed = 0.075f; // rotor speed in RPM; sign controls direction
    // [circumference linear velocity in m/s] = [# drills] * [drill width in meters] * [RPM] * 0.05236
    const float RotorTorque = 50000000f; // red > 33599988; rotor torque only has 6-7 digits of precision
    // CONFIG END

    #region mdk macros
    const string Deployment = "$MDK_DATE$ $MDK_TIME$";
    #endregion

    readonly HashSet<MyItemType> Trash = new HashSet<MyItemType>() {
      MyItemType.MakeOre("Stone"),
      MyItemType.MakeIngot("Gravel"),
    };

    enum State {
      Unknown = 0,
      Initialized,
      Starting,
      Cutting,
      Advancing,
      // Holding,
      Stopping,
      Stopped,
    }

    State RunState;

    readonly Dictionary<string, Action<string, UpdateType>> Commands;
    readonly Dictionary<State, Action<string, UpdateType>> Ticks;

    readonly Logger Log;

    UpdateManager Updater;

    EjectionManager Ejectors;
    ContainerManager Containers;
    DrillManager Drills;
    PistonManager Pistons;
    RefineryManager Refineries;

    RotorTracker Rotor;

    int Cycles = 0;

    SlidingWindow Average = new SlidingWindow(60);

    public Program() {
      //if(Runtime != null) throw new Exception();

      Log = new Logger(this, Me);

      Commands = new Dictionary<string, Action<string, UpdateType>> {
        //{ "", ProcessCommandHelp },
        //{ "HELP", ProcessCommandHelp },
        { "START", ProcessCommandStart },
        { "STOP", ProcessCommandStop },
        //{ "PAUSE", ProcessCommandPause },
        //{ "RESUME", ProcessCommandResume },
        { "NEXT", ProcessCommandNext },
      };

      Ticks = new Dictionary<State, Action<string, UpdateType>> {
        { State.Initialized, null },
        { State.Starting, ProcessTickStarting },
        { State.Cutting, ProcessTickCutting },
        { State.Advancing, ProcessTickAdvancing },
        //{ State.Holding, null },
        { State.Stopping, ProcessTickStopping },
        { State.Stopped, ProcessTickStopped },

      };

      Log.ClearScreen();
      Log.Info($"Version: {Deployment}");
      Log.Info();

      if(!Init()) {
        RunState = State.Unknown;
        Log.Error("Initialization failed");
      } else {
        RunState = State.Initialized;
      }

      Log.Info();
      Log.Info($"Instructions: {Runtime.CurrentInstructionCount}/{Runtime.MaxInstructionCount}");
    }

    public void Save() {
    }

    public void Main(string argument, UpdateType updateSource) {
      Log.ClearScreen();
      Cycles++;
      Log.Info($"Cycles: {Cycles}");
      Log.Info($"State: {RunState}");
      Log.Info();

      switch(updateSource) {
        case UpdateType.Terminal:
          argument = argument.ToUpper();
          if(Commands.ContainsKey(argument)) {
            Commands[argument](argument, updateSource);
          } else {
            // TODO: Unhandled argument. Panic?
          }
          break;
        case UpdateType.Update1:
          if(Updater != null) Updater.Update();
          if(Ticks.ContainsKey(RunState)) {
            Ticks[RunState]?.Invoke(argument, updateSource);
          } else {
            throw new Exception($"Unhandled state during dispatch: {RunState}");
          }
          break;
        default:
          throw new Exception($"Unhandled update source: {updateSource}");
      }

      Log.Info();
      Log.Info($"Last run time: {Runtime.LastRunTimeMs:N4}ms");
      Average.Add((int)(Runtime.LastRunTimeMs * 10000));
      Log.Info($"Min: {Average.Min / 10000f:N4}, Avg: {Average.Average / 10000f:N4}, Max: {Average.Max / 10000f:N4}");
      Log.Info($"Instructions: {Runtime.CurrentInstructionCount}/{Runtime.MaxInstructionCount}");
    }

  }
}
