namespace HELIX.Widgets {
  public delegate Widget BuildFunction(BuildContext context);

  public delegate Widget BuildFunction<in T>(BuildContext context, T parameter);

  public delegate Widget BuildFunction<in T1, in T2>(BuildContext context, T1 param1, T2 param2);

  public interface IBuildable {
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
    private readonly T _param;

    public ParameterizedFunctionBuildable(BuildFunction<T> func, T param) {
      _func = func;
      _param = param;
    }

    public Widget Build(BuildContext context) {
      return _func(context, _param);
    }
  }

  public readonly struct ParameterizedFunctionBuildable<T1, T2> : IBuildable {
    private readonly BuildFunction<T1, T2> _func;
    private readonly T1 _param1;
    private readonly T2 _param2;

    public ParameterizedFunctionBuildable(BuildFunction<T1, T2> func, T1 param1, T2 param2) {
      _func = func;
      _param1 = param1;
      _param2 = param2;
    }

    public Widget Build(BuildContext context) {
      return _func(context, _param1, _param2);
    }
  }
}