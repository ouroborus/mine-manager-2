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
    public class Logger {

      Program Program;
      IMyTextSurface Surface;

      public Logger(Program program, IMyProgrammableBlock me) {
        Program = program;
        Surface = me.GetSurface(0);
      }

      public void ClearScreen() {
        Surface.ContentType = ContentType.TEXT_AND_IMAGE;
        Surface.WriteText("", false);
      }

      public void Info(string s = "") {
        Program.Echo(s);
        Surface.WriteText(s + "\n", true);
      }

      public void Error(string s) {
        Program.Echo(s);
        Surface.WriteText($"ERROR: {s}\n", true);
      }

    }
  }
}
