using Godot;
using System;


public partial class InputManager : Node
{
    public enum InputSource { Keyboard, Gamepad }

    private InputSource activeInputSource = InputSource.Keyboard;

    [Signal]
    public delegate void InputSourceChangeEventHandler(InputSource source);

    public Vector2 GetMovementVector()
    {
        if (activeInputSource == InputSource.Keyboard)
        {
            return Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        }
        if (activeInputSource == InputSource.Gamepad)
        {
            return Vector2.Zero;
        }

        return Vector2.Zero;
    }

    public bool GetActionJustPressed(string actionName)
    {
        return Input.IsActionJustPressed(actionName);
    }


    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey || @event is InputEventMouse)
        {
            setInputSource(InputSource.Keyboard);
        }
        if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
        {
            setInputSource(InputSource.Gamepad);
        }
    }

    private void setInputSource(InputSource source)
    {
        if (activeInputSource != source)
        {
            activeInputSource = source;
            GD.Print($"Input Source has changed to: {source}");
            EmitSignal(nameof(InputSourceChange), source.ToString());
        }
    }

}
