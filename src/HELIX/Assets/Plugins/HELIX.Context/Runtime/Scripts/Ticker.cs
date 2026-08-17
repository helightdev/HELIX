using System;

namespace HELIX.Context {
  public class Ticker {

  }

  [AttributeUsage(AttributeTargets.Method)]
  [MixinExpression(@"

")]
  public class TickerAttribute : Attribute {

    public TickerProvider provider;

  }

  public enum TickerProvider {
    Update = 0,
    LateUpdate = 1,
    FixedUpdate = 2
  }
}