using System;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace Examples {
    public class ListVirtualizationExample : StatefulWidget<ListVirtualizationExample> {
        public override State<ListVirtualizationExample> CreateState() {
            return new ListVirtualizationExampleState();
        }
    }

    public class ListVirtualizationExampleState : State<ListVirtualizationExample> {
        private bool _fixedHeight = true;
        private int _itemCount = 72;

        public override Widget Build(BuildContext context) {
            return new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
                new HText("List Views").Heading(context),
                new HText("Switch between fixed and dynamic item heights to see how the list adapts.").Caption(context),
                new HRow(gap: 8) {
                    new HButton(
                        HButtonVariant.Soft,
                        onClick: () => {
                            _itemCount += 12;
                            SetState();
                        },
                        child: new HText("+ 12 Rows")
                    ),
                    new HButton(
                        HButtonVariant.Soft,
                        onClick: () => {
                            _itemCount = Math.Max(12, _itemCount - 12);
                            SetState();
                        },
                        child: new HText("- 12 Rows")
                    ),
                    new HButton(
                        HButtonVariant.TwoState,
                        selected: _fixedHeight,
                        child: new HText("Fixed Height"),
                        onClick: () => {
                            _fixedHeight = !_fixedHeight;
                            SetState();
                        }
                    )
                },
                new HText($"Rows: {_itemCount} · Mode: {(_fixedHeight ? "fixed" : "dynamic")}").Body(context),
                new HBox(borderRadius: 16) {
                    new HListView {
                        itemCount = _itemCount,
                        fixedItemHeight = _fixedHeight ? 72f : -1f,
                        itemBuilder = BuildRow
                    }
                }.Clip().Fill()
            }.Margin(16);
        }

        private Widget BuildRow(BuildContext context, int index) {
            var isFeatured = index % 6 == 0;
            var isExpanded = index % 4 == 0;
            var background = isFeatured
                ? context.GetThemed(PrimitiveTheme.Background)
                : context.GetThemed(PrimitiveTheme.BackgroundSubtle);

            var children = isExpanded
                ? new Widget[] {
                    new HText($"Item {index + 1:000}").Body(context),
                    new HText(
                        "Featured rows include a second line of copy to demonstrate dynamic height virtualization."
                    ).Caption(context),
                    new HText("When fixed item height is disabled, the list can size itself to this taller content.")
                        .Caption(context)
                }
                : new Widget[] {
                    new HText($"Item {index + 1:000}").Body(context), new HText("Compact row.").Caption(context)
                };

            return new HBox(
                background: background,
                borderRadius: BorderRadius.All(12)
            ) { new HColumn(gap: 4, children: children) }.Padding(12).Fill();
        }
    }
}