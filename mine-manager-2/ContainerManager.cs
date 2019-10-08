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
    public class ContainerManager : IUpdate {

      public List<IMyCargoContainer> Containers { get; private set; }

      public bool IsFull { get; private set; }

      int Index;

      public ContainerManager(Program me, List<IMyCargoContainer> containers) {
        Containers = containers;
        Index = 0;
        IsFull = false;
      }

      public void Update() {
        Index = 0;
        IsFull = false;
      }

      public IEnumerable<Item> GetItems() {
        for(var i = Containers.Count - 1; i >= 0; --i) {
          var inv = Containers[i].GetInventory(0);
          var items = new List<MyInventoryItem>();
          inv.GetItems(items);
          foreach(var item in items) {
            yield return new Item() { inv = inv, item = item };
          }
        }
      }

      public bool Store(Item item) {
        return Store(item.inv, item.item);
      }

      public bool Store(IMyInventory inventory, MyInventoryItem item) {
        if(IsFull) return false;
        var dst = Containers[Index].GetInventory(0);
        do {
          while(dst.IsFull) {
            Index++;
            if(Index >= Containers.Count) {
              IsFull = true;
              return false;
            }
          }
          dst = Containers[Index].GetInventory(0);
        } while(!inventory.TransferItemTo(dst, item));
        return true;
      }

    }
  }
}
