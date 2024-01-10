namespace Loupedeck.AdvancedCommandsPlugin
{
    using System;
    using System.Collections.Generic;

    using WindowsInput;

    // This class implements an example command that counts button presses.

    public class AdvancedKeyboardShortcut : ActionEditorCommand
    {
        private static readonly String keyParamName = "Key";
        private static readonly String keyUseRightModifier = "RightModifier";
        private static readonly String keyUseRightAlt = "RightAlt";
        private static readonly String keypressDurationParamName = "KeypressDuration";
        private static readonly String repeatParamName = "Repeat";
        private static readonly String repeatIntervalParamName = "RepeatInterval";

        private Dictionary<String, System.Timers.Timer> timers = new Dictionary<String, System.Timers.Timer>();

        // Initializes the command class.
        public AdvancedKeyboardShortcut()
        {
            this.DisplayName = "Advanced Keyboard Shortcut";
            this.Description = "Define a keyboard shortcut with advanced options like, keypress duration, auto - repeat in interval, ...";

            this.ActionEditor.AddControlEx(
                               new ActionEditorKeyboardKey(name: keyParamName, labelText: "Key").SetRequired());

            this.ActionEditor.AddControlEx(
                               new ActionEditorCheckbox(name: keyUseRightModifier, labelText: "Send right modifier", description: "Send right Control, right Shift, right windows instead of the left buttons"));

            this.ActionEditor.AddControlEx(
                               new ActionEditorCheckbox(name: keyUseRightAlt, labelText: "Add right alt", description: "Send rigt alt instead of Alt+Control. Right alt will be added to the list of modifiers if others are present in keybind"));

            this.ActionEditor.AddControlEx(new ActionEditorSlider(name: keypressDurationParamName, labelText: "Keypress duration", description: "Defines how long the key should be pressed down. A value of 0 means a default keypress").SetValues(0, 10000, 0, 100));

            this.ActionEditor.AddControlEx(
                               new ActionEditorCheckbox(name: repeatParamName, labelText: "Repeat", description: "Repeats the configured keypress in an interval"));

            this.ActionEditor.AddControlEx(new ActionEditorSlider(name: repeatIntervalParamName, labelText: "Interval for repeat", description: "Interval for repeating keypress (in seconds)").SetValues(0, 10, 1, 1));

            this.ActionEditor.ControlValueChanged += this.OnActionEditorControlValueChanged;
        }

        private void OnActionEditorControlValueChanged(Object sender, ActionEditorControlValueChangedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(repeatParamName))
            {
                var repeat = Boolean.Parse(e.ActionEditorState.GetControlValue(repeatParamName));
                
                e.ActionEditorState.SetEnabled(repeatIntervalParamName, repeat);
                e.ActionEditorState.SetValue(repeatIntervalParamName, "1");
            }
        }

        private void RunKeyboardCommand(ActionEditorActionParameters actionParameters, Int32 duration)
        {
            var rightModifier = GetUseRightModifierParam(actionParameters);
            var rightAlt = GetUseRightAltParam(actionParameters);
            var keys = actionParameters.Parameters[keyParamName];

            var inputSimulator = new InputSimulator();
            var modifierKeys = Helpers.KeyMapper.MapModifiers(keys, rightModifier);
            var virtualKey = Helpers.KeyMapper.MapKeys(keys);

            if (rightAlt)
            {
                modifierKeys.Add(WindowsInput.Native.VirtualKeyCode.RMENU);
            }

            if (duration == 0)
            {
                inputSimulator.Keyboard.ModifiedKeyStroke(modifierKeys, virtualKey);
            }
            else
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    // Press the key
                    foreach (var modifierKey in modifierKeys)
                    {
                        inputSimulator.Keyboard.KeyDown(modifierKey);
                    }

                    inputSimulator.Keyboard
                    .KeyDown(virtualKey)
                    .Sleep(duration);

                    // Release the key
                    foreach (var modifierKey in modifierKeys)
                    {
                        inputSimulator.Keyboard.KeyUp(modifierKey);
                    }

                    inputSimulator.Keyboard
                    .KeyUp(virtualKey);
                });
            }
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight) 
        {
            var timer = this.GetTimer(actionParameters);

            if (timer == null || !timer.Enabled)
            {
                using (var bitmapBuilder = new BitmapBuilder(imageWidth, imageHeight))
                {
                    bitmapBuilder.DrawText(actionParameters.Parameters[keyParamName].Split("___")[0] + " inactive");             

                    return bitmapBuilder.ToImage();  
                }
            }
            else
            {
                using (var bitmapBuilder = new BitmapBuilder(imageWidth, imageHeight))
                {
                    bitmapBuilder.DrawText(actionParameters.Parameters[keyParamName].Split("___")[0] + " active");

                    return bitmapBuilder.ToImage();
                }
            }
        }
       
        // This method is called when the user executes the command.
        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var duration = GetKeypressDurationParam(actionParameters);
            var repeat = GetRepeatKeypressParam(actionParameters);

            if (!repeat)
            {
                this.RunKeyboardCommand(actionParameters, duration);
            }
            else
            {

                var timer = this.GetTimer(actionParameters);
                if (timer == null || !timer.Enabled)
                {
                    actionParameters.Parameters.TryGetValue(repeatIntervalParamName, out var intervalString);
                    if (String.IsNullOrEmpty(intervalString))
                    {
                        intervalString = "1";
                    }

                    var interval = Int32.Parse(intervalString) * 1000;

                    timer = new System.Timers.Timer(interval);
                    timer.Elapsed += (sender, e) => this.RunKeyboardCommand(actionParameters, duration);
                    timer.AutoReset = true;
                    timer.Start();
                    this.timers[Helpers.Helpers.GetId(actionParameters)] = timer;

                    this.ActionImageChanged(); // Notify the Loupedeck service that the command display name and/or image has changed.
                }
                else
                {
                    timer.Stop();
                    this.ActionImageChanged(); // Notify the Loupedeck service that the command display name and/or image has changed.
                }
            }

            return true;
        }

        private System.Timers.Timer GetTimer(ActionEditorActionParameters actionParameters)
        {
            var id = Helpers.Helpers.GetId(actionParameters);

            return this.timers.ContainsKey(id) ? this.timers[id] : null;
        }

        private static Boolean GetRepeatKeypressParam(ActionEditorActionParameters actionParameters)
        {
            return Helpers.Helpers.GetBooleanParam(actionParameters, repeatParamName);
        }

        private static Int32 GetKeypressDurationParam(ActionEditorActionParameters actionParameters)
        {
            return Helpers.Helpers.GetIntParam(actionParameters, keypressDurationParamName);
        }

        private static Boolean GetUseRightModifierParam(ActionEditorActionParameters actionParameters)
        {
            return Helpers.Helpers.GetBooleanParam(actionParameters, keyUseRightModifier);
        }

        private static Boolean GetUseRightAltParam(ActionEditorActionParameters actionParameters)
        {
            return Helpers.Helpers.GetBooleanParam(actionParameters, keyUseRightAlt);
        }
    }
}
