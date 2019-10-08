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
    public class DrillManager : IUpdate {

      public List<IMyShipDrill> Drills { get; private set; }

      public enum State {
        Unknown = 0,
        Starting,
        Running,
        Stopped,
      }

      public State MyState { get; private set; }

      public Stack<IMyShipDrill> Startup;

      readonly Logger Log;

      public DrillManager(Program me, List<IMyShipDrill> drills) {
        Log = me.Log;

        this.Drills = drills;
        foreach(var drill in drills) {
          drill.Enabled = false;
          drill.UseConveyorSystem = false;
        }
        MyState = State.Stopped;
      }

      // Return true if all drills are enabled
      public bool IsReady() {
        return MyState == State.Running;
      }

      public void Update() {
        switch(MyState) {
          case State.Starting:
            Startup.Pop().Enabled = true;
            if(Startup.Count == 0) {
              Startup = null;
              MyState = State.Running;
            }
            break;
        }
      }

      public void Start() {
        switch(MyState) {
          case State.Starting:
          case State.Running:
            break;
          case State.Stopped:
            // Initialize startup
            Startup = new Stack<IMyShipDrill>(Drills);
            MyState = State.Starting;
            break;
          default:
            throw new Exception($"Unexpected drill state: {MyState}");
        }
      }

      public void Stop() {
        foreach(var drill in Drills) {
          drill.Enabled = false;
        }
        MyState = State.Stopped;
      }

      public IEnumerable<Item> GetItems() {
        foreach(var drill in Drills) {
          var inv = drill.GetInventory(0);
          var items = new List<MyInventoryItem>();
          inv.GetItems(items);
          foreach(var item in items) {
            yield return new Item() { inv = inv, item = item };
          }
        }
      }

    }
  }
}
