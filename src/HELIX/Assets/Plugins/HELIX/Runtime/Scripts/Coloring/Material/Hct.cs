using System;
using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///     HCT, hue, chroma, and tone.
    /// </summary>
    public sealed class Hct : IEquatable<Hct> {
        private int _argb;
        private double _chroma;
        private double _hue;
        private double _tone;

        private Hct(int argb) {
            _argb = argb;
            var cam16 = Cam16.FromInt(argb);
            _hue = cam16.Hue;
            _chroma = cam16.Chroma;
            _tone = MaterialColorUtils.LstarFromArgb(_argb);
        }

        public double Hue {
            get => _hue;
            set {
                _argb = HctSolver.SolveToInt(value, Chroma, Tone);
                var cam16 = Cam16.FromInt(_argb);
                _hue = cam16.Hue;
                _chroma = cam16.Chroma;
                _tone = MaterialColorUtils.LstarFromArgb(_argb);
            }
        }

        public double Chroma {
            get => _chroma;
            set {
                _argb = HctSolver.SolveToInt(Hue, value, Tone);
                var cam16 = Cam16.FromInt(_argb);
                _hue = cam16.Hue;
                _chroma = cam16.Chroma;
                _tone = MaterialColorUtils.LstarFromArgb(_argb);
            }
        }

        public double Tone {
            get => _tone;
            set {
                _argb = HctSolver.SolveToInt(Hue, Chroma, value);
                var cam16 = Cam16.FromInt(_argb);
                _hue = cam16.Hue;
                _chroma = cam16.Chroma;
                _tone = MaterialColorUtils.LstarFromArgb(_argb);
            }
        }

        public bool Equals(Hct other) {
            return !(other is null) && other._argb == _argb;
        }

        public static Hct From(double hue, double chroma, double tone) {
            var argb = HctSolver.SolveToInt(hue, chroma, tone);
            return new Hct(argb);
        }

        public static Hct FromInt(int argb) {
            return new Hct(argb);
        }

        public int ToInt() {
            return _argb;
        }

        public Hct InViewingConditions(ViewingConditions vc) {
            var cam16 = Cam16.FromInt(ToInt());
            var viewedInVc = cam16.XyzInViewingConditions(vc);

            var recastInVc = Cam16.FromXyzInViewingConditions(
                viewedInVc.x,
                viewedInVc.y,
                viewedInVc.z,
                ViewingConditions.Make()
            );

            return From(
                recastInVc.Hue,
                recastInVc.Chroma,
                MaterialColorUtils.LstarFromY(viewedInVc.y)
            );
        }

        public override bool Equals(object obj) {
            return obj is Hct other && Equals(other);
        }

        public override int GetHashCode() {
            return _argb;
        }

        public override string ToString() {
            return $"H{math.round(_hue)} C{math.round(_chroma)} T{math.round(_tone)}";
        }
    }
}