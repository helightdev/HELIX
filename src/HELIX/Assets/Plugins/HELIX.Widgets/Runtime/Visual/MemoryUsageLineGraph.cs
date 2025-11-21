using HELIX.Extensions;
using HELIX.Widgets.Visual.PathDrawers;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    [UxmlElement]
    public partial class MemoryUsageLineGraph : BaseWidget {
        public float pollingInterval = 0.5f;
        public float timeframe = 30f;

        public Color maxColor = HelixConvert.ParseColor("#0030a9");
        public Color residentColor = HelixConvert.ParseColor("#ff4264");
        public Color gcUsedColor = HelixConvert.ParseColor("#fcff35");

        private ProfilerRecorder _maxProfiler;
        private ProfilerRecorder _usedProfiler;
        private ProfilerRecorder _residentProfiler;
        private ProfilerRecorder _gcUsedProfiler;
        private readonly Label _memoryLabel;

        public MemoryUsageLineGraph() {
            _memoryLabel = new Label {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = new Color(0f, 0f, 0f, 0.5f),
                    color = Color.white,
                    fontSize = 10
                },
                enableRichText = true
            }.Positioned(left: 0, top: 0);
            schedule.Execute(() => {
                var usedMb = _usedProfiler.LastValue / (1024f * 1024f);
                var maxMb = _maxProfiler.LastValue / (1024f * 1024f);
                var residentMb = _residentProfiler.LastValue / (1024f * 1024f);
                var gcUsedMb = _gcUsedProfiler.LastValue / (1024f * 1024f);
                _memoryLabel.text =
                    $"<color={maxColor.ToUssColor()}>●</color> Used: {usedMb:F1} MB / {maxMb:F1} MB\n" +
                    $"<color={residentColor.ToUssColor()}>●</color> Resident: {residentMb:F1} MB\n" +
                    $"<color={gcUsedColor.ToUssColor()}>●</color> GC Used: {gcUsedMb:F1} MB";
            }).Every((long)(pollingInterval * 1000));
        }

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            _maxProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            _usedProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            _residentProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "App Resident Memory");
            _gcUsedProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");

            Clear();

            var rangeTime = new Vector2(0, timeframe);
            var rangeMemory = new Vector2(0, 1);
            Add(new TimePollingLineGraph(() => _gcUsedProfiler.LastValue / (float)_maxProfiler.LastValue,
                pollingInterval, timeframe,
                new FixedRangeLineGraphDataSource(rangeMemory, rangeTime)) {
                LineDrawer = new SolidStrokePathDrawer { Color = gcUsedColor },
                FillDrawer =
                    new GradientFillPathDrawer {
                        GradientGenerator = new DirectionalLinearGradientGenerator {
                            Angle = 90f,
                            StartColor = gcUsedColor.AlphaMultiplied(0.6f),
                            EndColor = gcUsedColor.AlphaMultiplied(0.1f)
                        }
                    }
            }.Position(0).MakeAbsolute());
            Add(new TimePollingLineGraph(() => _usedProfiler.LastValue / (float)_maxProfiler.LastValue, pollingInterval,
                timeframe,
                new FixedRangeLineGraphDataSource(rangeMemory, rangeTime)) {
                LineDrawer = new SolidStrokePathDrawer { Color = maxColor },
                FillDrawer =
                    new GradientFillPathDrawer {
                        GradientGenerator = new DirectionalLinearGradientGenerator {
                            Angle = 90f,
                            StartColor = maxColor.AlphaMultiplied(0.6f),
                            EndColor = maxColor.AlphaMultiplied(0.1f)
                        }
                    }
            }.Position(0).MakeAbsolute());
            Add(new TimePollingLineGraph(() => _residentProfiler.LastValue / (float)_maxProfiler.LastValue,
                pollingInterval,
                timeframe, new FixedRangeLineGraphDataSource(rangeMemory, rangeTime)) {
                LineDrawer = new SolidStrokePathDrawer { Color = residentColor },
            }.Position(0).MakeAbsolute());
            Add(_memoryLabel);
        }

        protected override void OnDetached(DetachFromPanelEvent evt) {
            base.OnDetached(evt);
            _maxProfiler.Stop();
            _usedProfiler.Stop();
            _residentProfiler.Stop();
            _gcUsedProfiler.Stop();
        }
    }
}