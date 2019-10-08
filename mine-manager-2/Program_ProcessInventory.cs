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

    bool ProcessInventory() {
      // return false if there is (probably) more work to do
      var processingComplete = true;

      foreach(var item in Drills.GetItems()) {
        //Log.Info($"{item.item.Type}:{item.item.Amount}");
        if(Trash.Contains(item.item.Type)) {
          if(Ejectors.Eject(item)) {
            //Log.Info("Continue: Ejector success");
            continue;
          } else {
            //Log.Info("Continue: Ejector failed");
          }
          processingComplete = false;
        } else {
          if(Refineries.Refine(item)) {
            //Log.Info("Continue: Refinery success");
            continue;
          } else {
            //Log.Info("Continue: Refinery failed");
          }
          processingComplete = false;
        }
        if(!Containers.Store(item)) {
          // TODO: Everything's full and drills are backing up! Panic?
          //Log.Info("End: Storing failed");
          processingComplete = false;
          break;
        }
      }

      foreach(var item in Refineries.GetItems()) {
        if(!Containers.Store(item)) {
          // TODO: Can't unload refineries because containers are full. Panic?
          processingComplete = false;
          break;
        }
      }

      if(Ejectors.IsFull) {
        if(Refineries.IsFull) {
          // TODO: Ejection and refineries are full. Panic?
          processingComplete = false;
        } else {
          foreach(var item in Containers.GetItems()) {
            if(!Trash.Contains(item.item.Type)) {
              if(!Refineries.Refine(item)) {
                processingComplete = false;
                break;
              }
            }
          }
        }
      } else {
        if(Refineries.IsFull) {
          foreach(var item in Containers.GetItems()) {
            if(Trash.Contains(item.item.Type)) {
              if(!Ejectors.Eject(item)) {
                processingComplete = false;
                break;
              }
            }
          }
        } else {
          foreach(var item in Containers.GetItems()) {
            if(Trash.Contains(item.item.Type)) {
              if(!Ejectors.Eject(item) && Refineries.IsFull) {
                processingComplete = false;
                break;
              }
            } else {
              if(!Refineries.Refine(item) && Ejectors.IsFull) {
                processingComplete = false;
                break;
              }
            }
          }
        }
      }

      return processingComplete;
    }

  }
}
