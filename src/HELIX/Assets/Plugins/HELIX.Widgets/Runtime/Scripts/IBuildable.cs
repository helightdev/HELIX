namespace HELIX.Widgets {
  public delegate Widget BuildFunction(BuildContext ctx);

  public delegate Widget BuildFunction<in T>(BuildContext ctx, T arg);

  public delegate Widget BuildFunction<in T1, in T2>(BuildContext ctx, T1 arg0, T2 arg1);

  public interface IBuildable {
    /// <summary>
    /// Builds the widget in the given context.
    /// </summary>
    /// <param name="context">The context in which to build the widget. This will be provided by
    /// the framework when building the widget tree.</param>
    Widget Build(BuildContext context);
  }

  public readonly struct FunctionBuildable : IBuildable {
    private readonly BuildFunction _func;

    public FunctionBuildable(BuildFunction func) {
      _func = func;
    }

    public Widget Build(BuildContext context) {
      return _func(context);
    }
  }

  public readonly struct ParameterizedFunctionBuildable<T> : IBuildable {
    private readonly BuildFunction<T> _func;
    private readonly T _arg;

    public ParameterizedFunctionBuildable(BuildFunction<T> func, T arg) {
      _func = func;
      _arg = arg;
    }

    public Widget Build(BuildContext context) {
      return _func(context, _arg);
    }
  }

  public readonly struct ParameterizedFunctionBuildable<T1, T2> : IBuildable {
    private readonly BuildFunction<T1, T2> _func;
    private readonly T1 _arg0;
    private readonly T2 _arg1;

    public ParameterizedFunctionBuildable(BuildFunction<T1, T2> func, T1 arg0, T2 arg1) {
      _func = func;
      _arg0 = arg0;
      _arg1 = arg1;
    }

    public Widget Build(BuildContext context) {
      return _func(context, _arg0, _arg1);
    }
  }
}