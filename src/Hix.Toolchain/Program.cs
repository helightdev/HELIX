if (args.Length > 0 && args[0] is "analysis-server" or "graph-server") return AnalysisHost.Run(args);
return AotReflectionRoots.Run(args);
