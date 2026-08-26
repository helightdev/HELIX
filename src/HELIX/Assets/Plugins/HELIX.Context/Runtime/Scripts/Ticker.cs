using System;
using UnityEngine;

namespace HELIX.Context {
  public struct FrameTimeTicker {
    public float rate;
    public float lastTick;
    public bool enabled;
    public bool running;

    public FrameTimeTicker(float rate) : this() {
      this.rate = rate;
      lastTick = -rate;
      enabled = true;
    }

    public bool Tick() {
      if (!enabled || running) return false;
      var current = Time.time;
      if (current - lastTick < rate) return false;
      lastTick = current;
      return true;
    }

  }

  public struct FrameCountTicker {
    public int rate;
    public int current;
    public bool enabled;
    public bool running;

    public FrameCountTicker(int rate) : this() {
      this.rate = rate;
      enabled = true;
    }

    public bool Tick() {
      if (!enabled || running) return false;
      if (rate == 0) return true;
      return current++ % rate == 0;
    }
  }


  [AttributeUsage(AttributeTargets.Method)]
  [MixinImport(typeof(ContextMixinLibrary))]
  [MixinExpression("@CALL<TickerImpl>")]
  public class TickerAttribute : Attribute {
    public TickerAttribute(string time = "tick", string target = MixinOn.MonoUpdate, string condition = null, int order = 10) {  }

    public TickerAttribute(float time, string target = MixinOn.MonoUpdate, string condition = null, int order = 10) {}
  }
}
