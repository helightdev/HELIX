# Widget Previews (/docs/previews)



This page groups the full public widget surface by purpose. For constructor signatures and
parameter details, see the generated API reference under `/reference`. This page focuses on *what
each widget is for* and how they fit together.

***

## Layout [#layout]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=layout" width="100%" height="360px" title="Layout widget preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    var container = context.GetThemed(PrimitiveTheme.Container);
    var containerLow = context.GetThemed(PrimitiveTheme.ContainerLow);
    var border = Border.All(1, context.GetThemed(PrimitiveTheme.TextVariant));

    return new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
    new HText("Layout primitives").Heading(context),
    new HRow(gap: 12) {
      PreviewTile(context, "HColumn", new HColumn(gap: 6, crossAxisAlign: Align.Stretch) {
        new HBox(background: containerLow, borderRadius: 8).Size(height: 18),
        new HBox(background: containerLow, borderRadius: 8).Size(height: 18),
        new HBox(background: containerLow, borderRadius: 8).Size(height: 18)
      }).Expand(),
      PreviewTile(context, "HRow", new HRow(gap: 6, crossAxisAlign: Align.Stretch) {
        new HBox(background: containerLow, borderRadius: 8).Size(width: 36),
        new HBox(background: containerLow, borderRadius: 8).Size(width: 36),
        new HBox(background: containerLow, borderRadius: 8).Size(width: 36)
      }).Expand(),
      PreviewTile(context, "HCenter", new HCenter {
        new HBox(background: containerLow, borderRadius: 8).Size(88, 36)
      }).Expand()
    },
    new HRow(gap: 12, crossAxisAlign: Align.Stretch) {
      new HBox(background: container, border: border, borderRadius: 16) {
        new HAlign(Alignment.BottomRight) {
          new HBox(background: containerLow, borderRadius: 8) {
              new HText("HAlign").Body(context)
          }.Size(112, 40)
        }
      }.Padding(16).Expand(),
      new HBoxShadow {
        new HBox(background: container, borderRadius: 16) {
          new HCenter { new HText("HBoxShadow").Body(context) }
        }
      }.Expand()
    }.Size(height: 104)
    }.Margin(16);
    ```
  </Tab>
</Tabs>

## Text [#text]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=text-icons" width="100%" height="360px" title="Text preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    var typography = context.GetThemed(PrimitiveBaseTheme.Typography);
    var colors = context.GetThemed(PrimitiveBaseTheme.Colors);

    return new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
    new HText("Text and icon styles").Heading(context),
    new HText("Display text").Display(context),
    new HText("Body copy follows the active theme typography and color tokens.").Body(context),
    new HText("Caption text is intended for secondary metadata.").Caption(context),
    new HRow(gap: 18, crossAxisAlign: Align.Center) {
      new HIcon(FaSolidIcons.AddressBook, FaSolidIcons.FontDefinition, size: typography.FontSize6, color: colors.primary.main),
      new HIcon(FaSolidIcons.MagnifyingGlass, FaSolidIcons.FontDefinition, size: typography.FontSize6, color: colors.secondary.main),
      new HIcon(FaSolidIcons.FloppyDisk, FaSolidIcons.FontDefinition, size: typography.FontSize6, color: colors.tertiary.main)
    }
    }.Margin(16);
    ```
  </Tab>
</Tabs>

***

## Buttons [#buttons]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=buttons" width="100%" height="360px" title="Button preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    new HButton(
      HButtonVariant.Soft,
      onClick: Save,
      child: new HRow(gap: 8) {
        new HIcon(FaSolidIcons.FloppyDisk, FaSolidIcons.FontDefinition),
        new HText("Save")
      }
    );
    ```
  </Tab>
</Tabs>

***

## TextField [#textfield]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=text-input" width="100%" height="360px" title="TextField preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    new HTextField(
      initialValue: playerName,
      onChanged: value => playerName = value,
      onSubmitted: SubmitName
    );
    ```
  </Tab>
</Tabs>

## Slider [#slider]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=sliders" width="100%" height="360px" title="Sliders preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    var horizontal = 0.65f;
    var vertical = 0.35f;

    return new HStatefulBuilder((context, state) =>
      new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
        new HText("Sliders").Heading(context),
        new HText($"Horizontal value: {Mathf.RoundToInt(horizontal * 100)}%").Body(context),
        new HSlider(
          initialValue: horizontal,
          onChanged: value => {
            horizontal = value;
            state.SetState();
          }
        ).TightStretch(),
        new HRow(gap: 16) {
          new HSlider(
            axis: Axis.Vertical,
            initialValue: vertical,
            onChanged: value => {
              vertical = value;
              state.SetState();
            }
          ).TightStretch(),
          new HText($"Vertical value: {Mathf.RoundToInt(vertical * 100)}%").Body(context)
        }.Fill()
      }.Margin(16)
    ).Stretch();
    ```
  </Tab>
</Tabs>

***

## Scrolling and virtualization [#scrolling-and-virtualization]

### HScrollView [#hscrollview]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=scroll-controller" width="100%" height="360px" title="Scroll controller preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    private readonly ScrollController _scroll = new();

    new HRow(gap: 16) {
      new HScrollView(controller: _scroll) { content }.Fill(),
      new HSlider(_scroll).TightStretch()
    }
    ```
  </Tab>
</Tabs>

### HListView (virtualized) [#hlistview-virtualized]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=list-view" width="100%" height="360px" title="List view preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    new HListView(
      (context, index) => new HBox {
        new HText($"Item {index + 1}")
      }.Padding(12),
      count: 1000,
      fixedItemHeight: 48f
    );
    ```
  </Tab>
</Tabs>

## Prompts and input glyphs [#prompts-and-input-glyphs]

<Tabs items="[&#x22;Preview&#x22;, &#x22;Code&#x22;]">
  <Tab className="&#x22;p-0&#x22;">
    <iframe src="/Preview/index.html?preview=prompts" width="100%" height="360px" title="Prompts preview" style="{borderRadius: '8px', border: 0, }" />
  </Tab>

  <Tab>
    ```csharp
    new HRow(gap: 8) {
      new HPrompt("Player/Move"),
      new HPrompt("Player/Look"),
      new HPrompt("Player/Attack")
    }
    ```
  </Tab>
</Tabs>
