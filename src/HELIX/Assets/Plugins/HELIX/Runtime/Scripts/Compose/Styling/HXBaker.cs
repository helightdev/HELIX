using System.Collections.Generic;
using HELIX.Types;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public static class HXBaker {
    public static Composable Flex(
      IReadOnlyList<Composable> children,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false,
      bool clear = false
    ) => (ref Composition cx) => {
      using (cx.Flex(mainAxis: mainAxis, main: main, cross: cross, reverse: reverse, clear: clear)) {
        for (var i = 0; i < children.Count; i++) {
          if (i > 0 && gap >= math.EPSILON) cx.Gap(gap);
          children[i](ref cx);
        }
      }
    };

    public static Composable Flex(
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false,
      bool clear = false,
      params Composable[] children
    ) => Flex(children, mainAxis, main, cross, gap, reverse, clear);

    public static Composable<T> Flex<T>(
      IReadOnlyList<Composable<T>> children,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false,
      bool clear = false
    ) => (ref Composition cx, T arg) => {
      using (cx.Flex(mainAxis: mainAxis, main: main, cross: cross, reverse: reverse, clear: clear)) {
        for (var i = 0; i < children.Count; i++) {
          if (i > 0 && gap >= math.EPSILON) cx.Gap(gap);
          children[i](ref cx, arg);
        }
      }
    };

    public static Composable<T> Flex<T>(
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false,
      bool clear = false,
      params Composable<T>[] children
    ) => Flex(children, mainAxis, main, cross, gap, reverse, clear);
  }
}