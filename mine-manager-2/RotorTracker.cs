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
    class RotorTracker : IUpdate {

      const float PI = 3.14159274f;
      const float PI2 = 6.28318548f;
      const float TODEGREES = 57.29578f;

      public IMyMotorStator Rotor { get; private set; }

      float StartAngle;
      public float StartAngleRad { get { return StartAngle; } }
      public float StartAngleDeg { get { return StartAngle * TODEGREES; } }

      public int Direction { get; private set; }
      public float Velocity { get; private set; }
      public int WrapCount { get; private set; }

      float PrevOffset;
      float ThisOffset;
      public float OffsetRad { get { return ThisOffset; } }
      public float OffsetDeg { get { return ThisOffset * TODEGREES; } }

      public float TravelRad { get { return WrapCount * PI2 + ThisOffset; } }
      public float TravelDeg { get { return TravelRad * TODEGREES; } }

      float PrevAngle;
      float ThisAngle;

      readonly Logger Log;

      public RotorTracker(Program me, IMyMotorStator rotor) {
        Log = me.Log;

        Rotor = rotor;
        Mark();
      }

      public void Mark() {
        ThisAngle = StartAngle = Rotor.Angle;
        ThisOffset = 0;
        WrapCount = 0;
        Update();
      }

      public void Update() {
        Velocity = Rotor.TargetVelocityRPM;
        Direction = Math.Sign(Velocity);
        if(Velocity < 0) Velocity = -Velocity;

        PrevAngle = ThisAngle;
        ThisAngle = Rotor.Angle;
        if(ThisAngle >= PI2) {
          ThisAngle = ThisAngle - PI2;
        } else
        if(ThisAngle < 0) {
          ThisAngle = ThisAngle + PI2;
        }

        PrevOffset = ThisOffset;
        ThisOffset = ThisAngle - StartAngle;
        if(ThisOffset >= PI) {
          ThisOffset = ThisOffset - PI2;
        } else
        if(ThisOffset < -PI) {
          ThisOffset = ThisOffset + PI2;
        }

        var offsetDelta = ThisOffset - PrevOffset;
        if(offsetDelta >= PI) {
          WrapCount--;
        } else
        if(offsetDelta < -PI) {
          WrapCount++;
        }
      }

    }
  }
}
