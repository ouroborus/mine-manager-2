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
    class EjectionManager : IUpdate {

      public List<IMyShipConnector> Ejectors { get; private set; } // Ejectors are connectors set ThrowOut = true

      int Index; // Where we are in the rotation
      int Used; // How many connectors we've filled during this update period

      public bool IsFull { get; private set; } // If we've filled all the connectors during this update period

      readonly Logger Log;

      // Initialize ejectors
      // Behavior is undefined if list size is zero
      public EjectionManager(Program me, List<IMyShipConnector> ejectors) {
        Log = me.Log;

        Ejectors = ejectors;
        Index = 0;
        Used = 0;
        IsFull = false;
        foreach(var ejector in ejectors) {
          ejector.PullStrength = 0f;
          ejector.Disconnect();
          ejector.CollectAll = false;
          ejector.ThrowOut = true;
          ejector.Enabled = true;
        }
      }

      // Reset for the next update cycle
      public void Update() {
        Used = 0;
        IsFull = false;
      }

      // Dump the supplied item stack into ejectors until one of them accepts (the remainder of) the stack or we run out of connectors to try.
      // Partial dumps can happen but return false and we can't check remainder without re-requesting the inventory so we blindly try one after another until one succeeds.
      // Returns true if dump completed successfully. Returns false if we ran out of connectors before we were able to complete the dump.
      public bool Eject(Item item) {
        return Eject(item.inv, item.item);
      }
      
      public bool Eject(IMyInventory inventory, MyInventoryItem item) {
        //Log.Info($"IfFull: {IsFull}");
        if(IsFull) return false;
        var dst = Ejectors[Index].GetInventory(0);
        do {
          while(dst.IsFull) {
            Index++;
            Used++;
            //Log.Info($"Index:{Index}, Used:{Used}");
            if(Index >= Ejectors.Count) {
              //Log.Info("resetting Index");
              Index = 0;
            }
            if(Used >= Ejectors.Count) {
              //Log.Info("setting IsFull");
              IsFull = true;
              return false;
            }
            dst = Ejectors[Index].GetInventory(0);
          }
        } while(!inventory.TransferItemTo(dst, item));
        return true;
      }

    }
  }
}
