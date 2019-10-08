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
    public class RefineryManager : IUpdate {

      public List<IMyRefinery> Refineries { get; private set; }

      public bool IsFull { get; private set; } // If we've filled all the refinery inputs during this update period

      int InputIndex;

      readonly Logger Log;

      public RefineryManager(Program me, List<IMyRefinery> refineries) {
        Log = me.Log;

        Refineries = refineries;
        foreach(var refinery in refineries) {
          refinery.UseConveyorSystem = false;
          refinery.Enabled = true;
        }
        IsFull = false;
        InputIndex = 0;
      }

      public void Update() {
        IsFull = false;
        InputIndex = 0;
      }

      // Push items into refinery inputs
      public bool Refine(Item item) {
        return Refine(item.inv, item.item);
      }

      public bool Refine(IMyInventory inv, MyInventoryItem item) {
        if(IsFull) return false;
        var dst = Refineries[InputIndex].InputInventory;
        do {
          while(dst.IsFull) {
            InputIndex++;
            if(InputIndex >= Refineries.Count) {
              IsFull = true;
              return false;
            }
          }
          dst = Refineries[InputIndex].InputInventory;
        } while(!inv.TransferItemTo(Refineries[InputIndex].InputInventory, item));
        return true;
      }

      // Get items from refinery outputs
      public IEnumerable<Item> GetItems() {
        foreach(var refinery in Refineries) {
          var inv = refinery.OutputInventory;
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
