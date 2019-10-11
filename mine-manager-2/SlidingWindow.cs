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

    // Derived from: https://www.nayuki.io/page/sliding-window-minimum-maximum-algorithm
    public class SlidingWindow {

      readonly int Length;
      Queue<int> Values;
      LinkedList<int> MinLList;
      LinkedList<int> MaxLList;

      public long Sum { get; private set; }
      public int Min { get { return MinLList.First.Value; } }
      public int Max { get { return MaxLList.First.Value; } }
      public int Average { get; private set; }

      public SlidingWindow(int length) {
        Length = length;
        Values = new Queue<int>(length);
        MinLList = new LinkedList<int>();
        MaxLList = new LinkedList<int>();
        Sum = 0;
      }

      public void Add(int value) {
        if(Values.Count >= Length) {
          var old = Values.Dequeue();
          RemoveHead(old);
          Sum -= old;
        }
        Values.Enqueue(value);
        AddTail(value);
        Sum += value;

        long n = Sum * 2 / Values.Count;
        Average = (int)((n + (n < 0 ? -1 : 1)) / 2);
      }

      void AddTail(int value) {
        while(MinLList.Count > 0 && value < MinLList.Last.Value) {
          MinLList.RemoveLast();
        }
        MinLList.AddLast(value);

        while(MaxLList.Count > 0 && value > MaxLList.Last.Value) {
          MaxLList.RemoveLast();
        }
        MaxLList.AddLast(value);
      }

      void RemoveHead(int value) {
        if(value < MinLList.First.Value) {
          throw new Exception("Wrong value");
        }
        if(value == MinLList.First.Value) {
          MinLList.RemoveFirst();
        }

        if(value > MaxLList.First.Value) {
          throw new Exception("Wrong value");
        }
        if(value == MaxLList.First.Value) {
          MaxLList.RemoveFirst();
        }
      }

    }
  }
}
