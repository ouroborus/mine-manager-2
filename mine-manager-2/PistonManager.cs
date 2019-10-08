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
    public class PistonManager : IUpdate {

      const float EPSILON = 0.0001f;

      public List<IMyPistonBase> Pistons { get; private set; }

      public float CurrentPosition { get; private set; }
      public float LowestPosition { get; private set; }
      public float HighestPosition { get; private set; }
      public float MaxVelocity { get; private set; }

      public PistonStatus State { get; private set; }

      public float MarkedPosition { get; private set; }
      public float TargetPosition { get; private set; }
      public float Velocity { get; private set; }

      readonly Logger Log;

      public PistonManager(Program me, List<IMyPistonBase> pistons) {
        Log = me.Log;

        Pistons = pistons;
        CurrentPosition = 0;
        LowestPosition = 0;
        HighestPosition = 0;
        MaxVelocity = 0;
        foreach(var piston in pistons) {
          CurrentPosition += piston.MinLimit = piston.MaxLimit = piston.CurrentPosition;
          piston.Velocity = 0;
          LowestPosition += piston.LowestPosition;
          HighestPosition += piston.HighestPosition;
          if(MaxVelocity == 0 || MaxVelocity > piston.MaxVelocity) {
            MaxVelocity = piston.MaxVelocity;
          }
          piston.Enabled = true;
        }
        State = PistonStatus.Stopped;
      }

      public void Update() {
        switch(State) {
          case PistonStatus.Extending:
          case PistonStatus.Retracting:
            if(!UpdateMovement()) {
              State = PistonStatus.Stopped;
            }
            break;
          case PistonStatus.Stopped:
          case PistonStatus.Extended:
          case PistonStatus.Retracted:
            // Nothing to do
            break;
        }
      }

      // Return false if pistons are already at target total extension
      bool UpdateMovement() {
        CurrentPosition = 0;
        foreach(var piston in Pistons) {
          CurrentPosition += piston.MinLimit = piston.MaxLimit = piston.CurrentPosition;
          piston.Velocity = 0;
        }

        var neededTravel = TargetPosition - CurrentPosition;
        if(neededTravel < EPSILON && neededTravel > -EPSILON) {
          if(CurrentPosition >= HighestPosition - EPSILON) {
            State = PistonStatus.Extended;
          } else
          if(CurrentPosition <= LowestPosition + EPSILON) {
            State = PistonStatus.Retracted;
          } else {
            State = PistonStatus.Stopped;
          }
          return false;
        }

        foreach(var piston in Pistons) {
          if(neededTravel > 0) {
            if(piston.CurrentPosition < piston.HighestPosition) {
              var availTravel = piston.HighestPosition - piston.CurrentPosition; // positive
              if(availTravel > neededTravel) {
                piston.MaxLimit = piston.CurrentPosition + neededTravel;
              } else {
                piston.MaxLimit = piston.HighestPosition;
              }
              piston.Velocity = Velocity;
              State = PistonStatus.Extending;
              return true;
            }
          } else {
            if(piston.CurrentPosition > piston.LowestPosition) {
              var availTravel = piston.LowestPosition - piston.CurrentPosition; // negative
              if(availTravel > neededTravel) {
                piston.MinLimit = piston.CurrentPosition + neededTravel;
              } else {
                piston.MinLimit = piston.LowestPosition;
              }
              piston.Velocity = -Velocity;
              State = PistonStatus.Retracting;
              return true;
            }
          }
        }

        throw new Exception("Unexpected state");
      }

      public void Mark() {
        MarkedPosition = CurrentPosition;
      }

      public void Stop() {
        CurrentPosition = 0;
        foreach(var piston in Pistons) {
          CurrentPosition += piston.MinLimit = piston.MaxLimit = piston.CurrentPosition;
          piston.Velocity = 0;
        }
        if(CurrentPosition >= HighestPosition - EPSILON) {
          State = PistonStatus.Extended;
        } else
        if(CurrentPosition <= LowestPosition + EPSILON) {
          State = PistonStatus.Retracted;
        } else {
          State = PistonStatus.Stopped;
        }
      }

      public void MoveTo(float position, float velocity) {
        if(velocity <= 0)
          throw new ConstraintException($"Target velocity ({velocity:N4}) must be greater than zero");
        if(velocity > MaxVelocity)
          throw new ConstraintException($"Target velocity ({velocity:N4}) greater than MaxVelocity ({MaxVelocity:N4})");
        if(position > HighestPosition)
          throw new ConstraintException($"Target position ({position:N4}) greater than HighestPosition ({HighestPosition:N4})");
        if(position < LowestPosition)
          throw new ConstraintException($"Target position ({position:N4}) less than LowestPosition ({LowestPosition:N4})");

        TargetPosition = position;
        Velocity = velocity;
        State = velocity > 0 ? PistonStatus.Extending : PistonStatus.Retracting;
        if(!UpdateMovement()) {
          throw new Exception("Unexpected state");
        }
      }

    }
  }
}
