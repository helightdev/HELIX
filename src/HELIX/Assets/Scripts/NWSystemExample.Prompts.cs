using HELIX.Coloring;
using HELIX.Compose;
using HELIX.Theming;
using HELIX.Types;
using HELIX.UI.Prompts;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  public partial class HomeComposable {
    private InputAction _promptSubmit;
    private InputAction _promptCancel;
    private InputAction _promptNavigate;
    private InputAction _promptClick;
    private HelixInputController _promptInputController;

    private void InitializePromptsExample() {
      _promptInputController = new HelixInputController(InputConfiguration.Default);
      _promptSubmit = new InputAction("Submit");
      _promptSubmit.AddBinding("<Keyboard>/space", groups: "Keyboard&Mouse");
      _promptSubmit.AddBinding("<Gamepad>/buttonSouth", groups: "Gamepad");
      _promptSubmit.AddBinding("<Touchscreen>/press", groups: "Touch");

      _promptCancel = new InputAction("Cancel");
      _promptCancel.AddBinding("<Keyboard>/escape", groups: "Keyboard&Mouse");
      _promptCancel.AddBinding("<Gamepad>/buttonEast", groups: "Gamepad");

      _promptNavigate = new InputAction("Navigate");
      _promptNavigate.AddBinding("<Keyboard>/upArrow", groups: "Keyboard&Mouse");
      _promptNavigate.AddBinding("<Gamepad>/dpad/up", groups: "Gamepad");

      _promptClick = new InputAction("Click");
      _promptClick.AddBinding("<Mouse>/leftButton", groups: "Keyboard&Mouse");
      _promptClick.AddBinding("<Gamepad>/rightTrigger", groups: "Gamepad");
    }

    private void DisposePromptsExample() {
      _promptSubmit?.Dispose();
      _promptCancel?.Dispose();
      _promptNavigate?.Dispose();
      _promptClick?.Dispose();
      _promptSubmit = null;
      _promptCancel = null;
      _promptNavigate = null;
      _promptClick = null;
      _promptInputController?.Dispose();
      _promptInputController = null;
    }

    private void ComposePromptsTab(ref Composition cx) {
      cx.SubscribeTo(_promptInputController);
      var configuration = _promptInputController.PeekValue();

      using (cx.ScrollView()) {
        cx.Text("Compose input prompts", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(
          "The first row follows the HelixInputController supplied by HXGuiHost. Change its device " +
          "configuration below to test live prompt resolution.",
          TextRole.BodySmall
        );
        cx.Spacing(2);

        cx.Text(
          $"Active input: {configuration.deviceType} • {configuration.gamepadVariant}",
          TextRole.LabelLarge
        );
        cx.Spacing(1);
        using (cx.Row(cross: Align.Center)) {
          ComposePromptAction(ref cx, _promptSubmit, "Submit");
          cx.Spacing(2);
          ComposePromptAction(ref cx, _promptCancel, "Cancel");
          cx.Spacing(2);
          ComposePromptAction(ref cx, _promptNavigate, "Navigate up");
          cx.Spacing(2);
          ComposePromptAction(ref cx, _promptClick, "Click");
        }

        cx.Spacing(3);
        cx.Text("Controller configuration", TextRole.LabelLarge);
        cx.Spacing(1);
        using (cx.Row(cross: Align.Stretch)) {
          PromptConfigurationButton(ref cx, "Keyboard & mouse", SetPromptKeyboardMouse);
          cx.Spacing(1);
          PromptConfigurationButton(ref cx, "Xbox", SetPromptXbox);
          cx.Spacing(1);
          PromptConfigurationButton(ref cx, "PlayStation", SetPromptPlayStation);
          cx.Spacing(1);
          PromptConfigurationButton(ref cx, "Switch", SetPromptSwitch);
          cx.Spacing(1);
          PromptConfigurationButton(ref cx, "Steam Deck", SetPromptSteamDeck);
          cx.Spacing(1);
          PromptConfigurationButton(ref cx, "Steam Controller", SetPromptSteamController);
        }

        cx.Spacing(3);
        cx.Text("Explicit configurations", TextRole.LabelLarge);
        cx.Text(
          "These prompts bypass the controller, demonstrating per-prompt configuration overrides.",
          TextRole.BodySmall
        );
        cx.Spacing(1);
        using (cx.Row(cross: Align.Center)) {
          ComposeConfiguredPrompt(ref cx, "Keyboard", InputDeviceType.KeyboardMouse, GamepadVariant.Generic);
          cx.Spacing(2);
          ComposeConfiguredPrompt(ref cx, "Xbox", InputDeviceType.Gamepad, GamepadVariant.Xbox);
          cx.Spacing(2);
          ComposeConfiguredPrompt(ref cx, "PlayStation", InputDeviceType.Gamepad, GamepadVariant.PlayStation);
          cx.Spacing(2);
          ComposeConfiguredPrompt(ref cx, "Switch", InputDeviceType.Gamepad, GamepadVariant.NintendoSwitch);
          cx.Spacing(2);
          ComposeConfiguredPrompt(ref cx, "Steam Deck", InputDeviceType.Gamepad, GamepadVariant.SteamDeck);
          cx.Spacing(2);
          ComposeConfiguredPrompt(ref cx, "Steam Controller", InputDeviceType.Gamepad, GamepadVariant.SteamController);
        }
      }
    }

    private static void ComposePromptAction(ref Composition cx, InputAction action, string label) {
      using (cx.Row(cross: Align.Center)) {
        cx.Spec(new PromptSpec(action, constraints: BoxConstraints.Tight(40f, 40f)));
        cx.Spacing(1);
        cx.Text(label, TextRole.LabelMedium);
      }
    }

    private void ComposeConfiguredPrompt(
      ref Composition cx, string label, InputDeviceType device, GamepadVariant gamepad
    ) {
      using (cx.Column(cross: Align.Center)) {
        cx.Spec(new PromptSpec(
          _promptSubmit,
          configuration: new InputConfiguration(device, gamepad),
          constraints: BoxConstraints.Tight(48f, 48f),
          tint: Colors.White
        ));
        cx.Spacing(1);
        cx.Text(label, TextRole.BodySmall);
      }
    }

    private static void PromptConfigurationButton(
      ref Composition cx, string label, CompositionAction action
    ) => cx.Button(
      (ref Composition child) => child.Text(label),
      style: ThemeProperties.ButtonOutlined[in cx],
      action: action
    );

    private static void SetPromptKeyboardMouse(CompositionContext context) =>
      SetPromptConfiguration(context, InputDeviceType.KeyboardMouse, GamepadVariant.Generic);
    private static void SetPromptXbox(CompositionContext context) =>
      SetPromptConfiguration(context, InputDeviceType.Gamepad, GamepadVariant.Xbox);
    private static void SetPromptPlayStation(CompositionContext context) =>
      SetPromptConfiguration(context, InputDeviceType.Gamepad, GamepadVariant.PlayStation);
    private static void SetPromptSwitch(CompositionContext context) =>
      SetPromptConfiguration(context, InputDeviceType.Gamepad, GamepadVariant.NintendoSwitch);
    private static void SetPromptSteamDeck(CompositionContext context) =>
      SetPromptConfiguration(context, InputDeviceType.Gamepad, GamepadVariant.SteamDeck);
    private static void SetPromptSteamController(CompositionContext context) =>
      SetPromptConfiguration(context, InputDeviceType.Gamepad, GamepadVariant.SteamController);

    private static void SetPromptConfiguration(
      CompositionContext context, InputDeviceType device, GamepadVariant gamepad
    ) => context.Lookup<HomeComposable>()?._promptInputController?.SetValue(new InputConfiguration(device, gamepad));
  }
}
