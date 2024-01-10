namespace Loupedeck.AdvancedCommandsPlugin
{
    using System;
    using System.Collections.Generic;
    using WindowsInput;

    // This class implements an example command that counts button presses.

    public class AdvancedMouseWheel : ActionEditorCommand
    {
        private static readonly String keyParamName = "Key";
        private static readonly String keyUseRightModifier = "RightModifier";
        private static readonly String keyMouseWheelDirection = "MouseWheelDirection";
        private static readonly String keyDelayAfterKeyPress = "DelayAfterKeypress";
        private static readonly String keyMouseWheelClicks = "MousWheelClicks";
        private static readonly String keyMaxRampUpMultiplier = "MaxRampUpMultiplier";

        private Dictionary<String, RampUpData> rampUpData = new Dictionary<String, RampUpData>();

        // Initializes the command class.
        public AdvancedMouseWheel()
        {
            this.DisplayName = "Advanced Mouse Wheel";
            this.Description = "Define a mouse wheel movement combined with modifier keys.";

            this.ActionEditor.AddControlEx(
                               new ActionEditorKeyboardKey(name: keyParamName, labelText: "Key").SetRequired());

            this.ActionEditor.AddControlEx(
                               new ActionEditorCheckbox(name: keyUseRightModifier, labelText: "Send right modifier", description: "Send right Control, right Shift, right windows instead of the left buttons"));

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(name: keyMouseWheelDirection, labelText: "Wheel direction").SetRequired());

            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(name: keyDelayAfterKeyPress, labelText: "Delay after keypress", description: "Defines the delay (ms) betwenn the modifier key is pressed down and the wheel turn.")
                    .SetValues(0, 1000, 0, 10));

            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(name: keyMouseWheelClicks, labelText: "Mouse wheel clicks", description: "Defines how far the mouse wheel travels in clicks.")
                    .SetValues(1, 100, 1, 1));

            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(name: keyMaxRampUpMultiplier, labelText: "Max. ramp-up multiplier", description: "If the command is executed in quick succession, a multiplier can be specified for the number of mouse wheel clicks. The multiplier is increased every 100ms up to the value specified here and automatically falls back to 1 if no input is made for longer than 100ms.")
                    .SetValues(1, 50, 1, 1));

            this.ActionEditor.ListboxItemsRequested += this.OnActionEditorListboxItemsRequested;
        }

        private void OnActionEditorListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            e.AddItem("Up", "Up", "Scroll up");
            e.AddItem("Down", "Down", "Scroll down");
        }

        private RampUpData GetRampUpData(ActionEditorActionParameters actionParameters)
        {
            var id = Helpers.Helpers.GetId(actionParameters);
            if (!this.rampUpData.ContainsKey(id))
            {
                this.rampUpData.Add(id, new RampUpData() 
                { 
                    LastTurn = DateTime.MinValue 
                });
            }

            return this.rampUpData[id];
        }

        // This method is called when the user executes the command.
        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var rightModifier = Helpers.Helpers.GetBooleanParam(actionParameters, keyUseRightModifier);
            var keys = actionParameters.Parameters[keyParamName];
            var direction = actionParameters.Parameters[keyMouseWheelDirection];
            var delay = Helpers.Helpers.GetIntParam(actionParameters, keyDelayAfterKeyPress);
            var clicks = Helpers.Helpers.GetIntParam(actionParameters, keyMouseWheelClicks);
            var rampUpData = this.GetRampUpData(actionParameters);
            var maxRampUpMultiplier = Helpers.Helpers.GetIntParam(actionParameters, keyMaxRampUpMultiplier);

            var inputSimulator = new InputSimulator();
            var modifierKeys = Helpers.KeyMapper.MapModifiers(keys, rightModifier);
            var virtualKey = Helpers.KeyMapper.MapKeys(keys);

            if (DateTime.Now - rampUpData.LastTurn < TimeSpan.FromMilliseconds(100) && rampUpData.Direction == direction)
            {
                if (rampUpData.Multiplier < maxRampUpMultiplier)
                {
                    rampUpData.Multiplier++;
                }
            }
            else
            {
                rampUpData.Multiplier = 1;
            }

            rampUpData.Direction = direction;

            System.Threading.Tasks.Task.Run(() =>
            {
                // Press the key
                foreach (var modifierKey in modifierKeys)
                {
                    inputSimulator.Keyboard.KeyDown(modifierKey);
                }

                if (virtualKey != 0)
                {
                    inputSimulator.Keyboard
                    .KeyDown(virtualKey);
                }

                if (delay > 0)
                {
                    System.Threading.Thread.Sleep(delay);
                }
                
                var actualClicks = clicks * rampUpData.Multiplier;

                for (var i = 0; i < actualClicks; i++)
                {
                    System.Threading.Thread.Sleep(10);
                    if (direction == "Down")
                    {
                        inputSimulator.Mouse.VerticalScroll(-1);
                    }
                    else
                    {
                        inputSimulator.Mouse.VerticalScroll(1);
                    }
                }

                rampUpData.LastTurn = DateTime.Now;

                if (delay > 0)
                {
                    System.Threading.Thread.Sleep(delay);
                }

                // Release the key
                foreach (var modifierKey in modifierKeys)
                {
                    inputSimulator.Keyboard.KeyUp(modifierKey);
                }

                if (virtualKey != 0)
                {
                    inputSimulator.Keyboard
                    .KeyUp(virtualKey);
                }
            });

            return true;
        }
    }

    public class RampUpData
    {
        public DateTime LastTurn { get; set; }
        public Int32 Multiplier { get; set; }
        public String Direction { get; set; }
    }
}
