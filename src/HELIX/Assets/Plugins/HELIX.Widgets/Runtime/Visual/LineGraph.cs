using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Painting;
using HELIX.Painting.Paths;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

namespace HELIX.Widgets.Visual {
    [UxmlElement]
    public partial class LineGraph : PaintingWidget {
        public ILineGraphDataSource datasource = new DynamicRangeLineGraphDataSource();

        [UxmlAttribute] public bool smoothLines = true;
        [UxmlAttribute] public float oversampleFactor = 0f;

        private ScriptablePathDrawer _fillDrawer;
        private ScriptablePathDrawer _lineDrawer;

        [UxmlObjectReference("stroke")]
        public ScriptablePathDrawer LineDrawer {
            get => _lineDrawer;
            set {
                _lineDrawer = value;
                MarkDirtyRepaint();
            }
        }

        [UxmlObjectReference("fill")]
        public ScriptablePathDrawer FillDrawer {
            get => _fillDrawer;
            set {
                _fillDrawer = value;
                MarkDirtyRepaint();
            }
        }

        public ILineGraphDataSource Datasource {
            get => datasource;
            set {
                datasource = value;
                MarkDirtyRepaint();
            }
        }

        public void Refresh() {
            MarkDirtyRepaint();
        }

        public LineGraph() {
            style.overflow = Overflow.Hidden;
        }

        public override void Paint(PaintCanvas canvas, Rect bounds) {
            if (!Datasource.HasData) return;
            var painter = canvas.painter;
            var width = bounds.width;
            var height = bounds.height;

            // Convert all normalized points to pixel coords up front
            var src = Datasource.GetNormalizedPoints();
            if (src.Length < 2) return;
            var pts = new Vector2[src.Length];
            var n = 0;
            var sampleXStart = 0 - oversampleFactor - float.Epsilon;
            var sampleXEnd = 1 + oversampleFactor + float.Epsilon;
            foreach (var it in src) {
                var p = it;
                if (p.x < sampleXStart || p.x > sampleXEnd) continue;
                p.x = p.x;
                p.y = Mathf.Clamp01(p.y);
                pts[n++] = new Vector2(p.x * width, (1 - p.y) * height);
            }

            var pathBuilder = new PathBuilder();
            pathBuilder.MoveTo(pts[0]);
            if (smoothLines) {
                for (var i = 0; i < n - 1; i++) {
                    var p0 = i == 0 ? pts[i] : pts[i - 1];
                    var p1 = pts[i];
                    var p2 = pts[i + 1];
                    var p3 = (i + 2 < n) ? pts[i + 2] : pts[n - 1];

                    const float t0 = 0f;
                    var t1 = t0 + Mathf.Sqrt((p1 - p0).magnitude);
                    var t2 = t1 + Mathf.Sqrt((p2 - p1).magnitude);
                    var t3 = t2 + Mathf.Sqrt((p3 - p2).magnitude);

                    var m1 = (p2 - p0) / (t2 - t0);
                    var m2 = (p3 - p1) / (t3 - t1);

                    var dt = t2 - t1;

                    var c1 = p1 + m1 * (dt / 3f);
                    var c2 = p2 - m2 * (dt / 3f);

                    pathBuilder.BezierCurveTo(c1, c2, p2);
                }
            } else {
                for (var i = 1; i < n; i++) pathBuilder.LineTo(pts[i]);
            }

            var path = pathBuilder.Build();

            if (FillDrawer != null) {
                path.Apply(painter);
                painter.LineTo(new Vector2(pts[n - 1].x, height));
                painter.LineTo(new Vector2(pts[0].x, height));
                painter.ClosePath();
                FillDrawer.Draw(canvas);
            }

            if (LineDrawer != null) {
                path.Apply(painter);
                LineDrawer.Draw(canvas);
            }
        }
    }

    public class TimePollingLineGraph : LineGraph {
        public float pollingInterval;
        public float timeframe;
        private RingBuffer<Vector2> _dataBuffer;
        private FixedTimeframeDynamicRangeLineGraphDataSource _dataSource;
        private Func<float> _sampleFunction;

        public TimePollingLineGraph(Func<float> sampleFunction, float pollingInterval, float timeframe,
            ILineGraphDataSource source = null) {
            this.pollingInterval = pollingInterval;
            this.timeframe = timeframe;
            _sampleFunction = sampleFunction;
            _dataBuffer = new RingBuffer<Vector2>(Mathf.CeilToInt((timeframe + 1) / pollingInterval));
            oversampleFactor = 0.1f;
            datasource = source ?? new FixedTimeframeDynamicRangeLineGraphDataSource(timeframe);
            schedule.Execute(UpdateEvent).Every((long)(pollingInterval * 1000));
        }

        private void UpdateEvent() {
            var time = Time.realtimeSinceStartup;
            var value = _sampleFunction.Invoke();
            _dataBuffer.Add(new Vector2(time, value));
            datasource.Update(_dataBuffer, new Vector2(-time + timeframe, 0));
            MarkDirtyRepaint();
        }
    }

    [UxmlObject]
    public partial class LineGraphStroke {
        [UxmlObjectReference] public ScriptablePathDrawer Drawer { get; set; }
    }

    [UxmlObject]
    public partial class LineGraphFill {
        [UxmlObjectReference] public ScriptablePathDrawer Drawer { get; set; }
    }


    public interface ILineGraphDataSource {
        bool HasData { get; }
        Vector2[] GetNormalizedPoints();

        void Update(IReadOnlyCollection<Vector2> points, Vector2 offset = default) { }
    }

    public class FixedRangeLineGraphDataSource : ILineGraphDataSource {
        private Vector2[] _samples;
        public bool HasData => _samples?.Length > 1;
        public Vector2[] GetNormalizedPoints() => _samples;
        private readonly Vector2 _valueRange;
        private readonly Vector2 _timeRange;

        public FixedRangeLineGraphDataSource(Vector2 valueRange, Vector2 timeRange) {
            _valueRange = valueRange;
            _timeRange = timeRange;
        }

        public FixedRangeLineGraphDataSource(IReadOnlyCollection<Vector2> initialPoints, Vector2 valueRange,
            Vector2 timeRange, Vector2 offset = default) {
            _valueRange = valueRange;
            _timeRange = timeRange;
            Update(initialPoints, offset);
        }

        public void Update(IReadOnlyCollection<Vector2> points, Vector2 offset = default) {
            if (points == null || points.Count == 0) {
                _samples = null;
                return;
            }

            var data = points.ToArray();
            var rangeX = _timeRange.y - _timeRange.x;
            var rangeY = _valueRange.y - _valueRange.x;
            if (rangeX <= 0) rangeX = 1;
            if (rangeY <= 0) rangeY = 1;
            for (var i = 0; i < data.Length; i++) {
                var p = data[i] + offset;
                var nx = (p.x - _timeRange.x) / rangeX;
                var ny = (p.y - _valueRange.x) / rangeY;
                data[i] = new Vector2(nx, ny);
            }

            _samples = data;
        }
    }

    public class DynamicRangeLineGraphDataSource : ILineGraphDataSource {
        private Vector2[] _samples;
        public bool HasData => _samples?.Length > 1;
        public Vector2[] GetNormalizedPoints() => _samples;
        public DynamicRangeLineGraphDataSource() { }

        public DynamicRangeLineGraphDataSource(IReadOnlyCollection<Vector2> initialPoints, Vector2 offset = default) {
            Update(initialPoints, offset);
        }

        public void Update(IReadOnlyCollection<Vector2> points, Vector2 offset = default) {
            if (points == null || points.Count == 0) {
                _samples = null;
                return;
            }

            var data = points.ToArray();
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            foreach (var p in data) {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            var rangeX = maxX - minX;
            var rangeY = maxY - minY;
            if (rangeX <= 0) rangeX = 1;
            if (rangeY <= 0) rangeY = 1;
            for (var i = 0; i < data.Length; i++) {
                var p = data[i] + offset;
                var nx = (p.x - minX) / rangeX;
                var ny = (p.y - minY) / rangeY;
                data[i] = new Vector2(nx, ny);
            }

            _samples = data;
        }
    }

    public class FixedTimeframeDynamicRangeLineGraphDataSource : ILineGraphDataSource {
        private Vector2[] _samples;
        public bool HasData => _samples?.Length > 1;
        public Vector2[] GetNormalizedPoints() => _samples;
        private readonly float _timeframe;

        public FixedTimeframeDynamicRangeLineGraphDataSource(float timeframe) {
            _timeframe = timeframe;
        }

        public FixedTimeframeDynamicRangeLineGraphDataSource(IReadOnlyCollection<Vector2> initialPoints,
            float timeframe, Vector2 offset = default) {
            _timeframe = timeframe;
            Update(initialPoints, offset);
        }

        public void Update(IReadOnlyCollection<Vector2> points, Vector2 offset = default) {
            if (points == null || points.Count == 0) {
                _samples = null;
                return;
            }

            var data = points.ToArray();
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            foreach (var p in data) {
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            var rangeY = maxY - minY;
            if (rangeY <= 0) rangeY = 1;
            for (var i = 0; i < data.Length; i++) {
                var p = data[i] + offset;
                var nx = (p.x) / _timeframe;
                var ny = (p.y - minY) / rangeY;
                data[i] = new Vector2(nx, ny);
            }

            _samples = data;
        }
    }
}