using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
// We receive data as raw HID input reports. This struct
// describes the raw binary format of such a report.

[StructLayout(LayoutKind.Explicit, Size = 32)]
struct NSWWiredControllerInputReport : IInputStateTypeInfo {
    // Because all HID input reports are tagged with the 'HID ' FourCC,
    // this is the format we need to use for this state struct.
    public FourCC format => new FourCC('H', 'I', 'D');

    // HID input reports can start with an 8-bit report ID. It depends on the device
    // whether this is present or not. On the PS4 DualShock controller, it is
    // present. We don't really need to add the field, but let's do so for the sake of
    // completeness. This can also help with debugging.
    [FieldOffset(0)] public byte reportId;


    /*  
    byte reportId;               // #0
    byte leftStickX;             // #4
    byte leftStickY;             // #5
    byte rightStickX;            // #6
    byte rightStickY;            // #7
    byte dpad : 4;               // #3 bit #0 (0=up, 2=right, 4=down, 6=left)
    byte YButton : 1;            // #1 bit #0
    byte BButton : 1;            // #1 bit #1
    byte AButton : 1;            // #1 bit #2
    byte XButton : 1;            // #1 bit #3
    byte leftShoulder : 1;       // #1 bit #4
    byte rightShoulder : 1;      // #1 bit #5
    byte leftTrigger;            // #1 bit #6
    byte rightTrigger;           // #1 bit #7
    byte minusButton : 1;        // #2 bit #0
    byte plusButton : 1;         // #2 bit #1
    byte leftStickPress : 1;     // #2 bit #2
    byte rightStickPress : 1;    // #2 bit #3
    byte systemButton : 1;       // #2 bit #4
    byte screenshotPress : 1;    // #2 bit #5
     */


    [InputControl(name = "buttonWest", displayName = "Y", offset = 1, bit = 0)]
    [InputControl(name = "buttonSouth", displayName = "B", offset = 1, bit = 1)]
    [InputControl(name = "buttonEast", displayName = "A", offset = 1, bit = 2)]
    [InputControl(name = "buttonNorth", displayName = "X", offset = 1, bit = 3)]
    [InputControl(name = "leftTriggerButton", layout = "Button", offset = 1, bit = 4)]
    [InputControl(name = "rightTriggerButton", layout = "Button", offset = 1, bit = 5)]
    [InputControl(name = "leftShoulder", offset = 1, bit = 6)]
    [InputControl(name = "rightShoulder", offset = 1, bit = 7)]
    [FieldOffset(1)] public byte buttons1;

    [InputControl(name = "select", displayName = "-", offset = 2, bit = 0)]
    [InputControl(name = "start", displayName = "+", offset = 2, bit = 1)]
    [InputControl(name = "leftStickPress", offset = 2, bit = 2)]
    [InputControl(name = "rightStickPress", offset = 2, bit = 3)]
    [InputControl(name = "systemButton", layout = "Button", displayName = "System", offset = 2, bit = 4)]
    [InputControl(name = "screenshotButton", layout = "Button", displayName = "Touchpad Press", offset = 2, bit = 5)]
    [FieldOffset(2)] public byte buttons2;

    [InputControl(name = "dpad", format = "BIT", layout = "Dpad", sizeInBits = 4, defaultState = 8)]
    [InputControl(name = "dpad/up", format = "BIT", layout = "DiscreteButton", parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7", bit = 0, sizeInBits = 4)]
    [InputControl(name = "dpad/right", format = "BIT", layout = "DiscreteButton", parameters = "minValue=1,maxValue=3", bit = 0, sizeInBits = 4)]
    [InputControl(name = "dpad/down", format = "BIT", layout = "DiscreteButton", parameters = "minValue=3,maxValue=5", bit = 0, sizeInBits = 4)]
    [InputControl(name = "dpad/left", format = "BIT", layout = "DiscreteButton", parameters = "minValue=5, maxValue=7", bit = 0, sizeInBits = 4)]
    [FieldOffset(3)] public byte buttons3;
    
    [InputControl(name = "leftStick", layout = "Stick", sizeInBits = 16, format = "VEC2")]
    [InputControl(name = "leftStick/x", format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "leftStick/left", format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "leftStick/right", format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
    [InputControl(name = "leftStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "leftStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "leftStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
    [FieldOffset(4)] public byte leftStickX;
    [FieldOffset(5)] public byte leftStickY;

    [InputControl(name = "rightStick", layout = "Stick", sizeInBits = 16, format = "VEC2")]
    [InputControl(name = "rightStick/x", format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "rightStick/left", format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "rightStick/right", format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
    [InputControl(name = "rightStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "rightStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "rightStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
    [FieldOffset(6)] public byte rightStickX;
    [FieldOffset(7)] public byte rightStickY;

    [InputControl(name = "leftTrigger", bit = 4, format = "BIT")]
    [FieldOffset(1)] public byte leftTrigger;

    [InputControl(name = "rightTrigger", bit = 5, format = "BIT")]
    [FieldOffset(1)] public byte rightTrigger;

}
