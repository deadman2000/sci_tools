namespace SCI_Lib.Resources.Scripts
{
    public static class KernelFunctions
    {
        public static readonly string[] Names = new[]
        {
            /*0x00*/ "Load",
            /*0x01*/ "UnLoad",
            /*0x02*/ "ScriptID",
            /*0x03*/ "DisposeScript",
            /*0x04*/ "Clone",
            /*0x05*/ "DisposeClone",
            /*0x06*/ "IsObject",
            /*0x07*/ "RespondsTo",
            /*0x08*/ "DrawPic",
            /*0x09*/ "Show",
            /*0x0a*/ "PicNotValid",
            /*0x0b*/ "Animate",
            /*0x0c*/ "SetNowSeen",
            /*0x0d*/ "NumLoops",
            /*0x0e*/ "NumCels",
            /*0x0f*/ "CelWide",
            /*0x10*/ "CelHigh",
            /*0x11*/ "DrawCel",
            /*0x12*/ "AddToPic",
            /*0x13*/ "NewWindow",
            /*0x14*/ "GetPort",
            /*0x15*/ "SetPort",
            /*0x16*/ "DisposeWindow",
            /*0x17*/ "DrawControl",
            /*0x18*/ "HiliteControl",
            /*0x19*/ "EditControl",
            /*0x1a*/ "TextSize",
            /*0x1b*/ "Display",
            /*0x1c*/ "GetEvent",
            /*0x1d*/ "GlobalToLocal",
            /*0x1e*/ "LocalToGlobal",
            /*0x1f*/ "MapKeyToDir",
            /*0x20*/ "DrawMenuBar",
            /*0x21*/ "MenuSelect",
            /*0x22*/ "AddMenu",
            /*0x23*/ "DrawStatus",
            /*0x24*/ "Parse",
            /*0x25*/ "Said",
            /*0x26*/ "SetSynonyms",	// Portrait (KQ6 hires)
            /*0x27*/ "HaveMouse",
            /*0x28*/ "SetCursor",
            // FOpen (SCI0)
            // FPuts (SCI0)
            // FGets (SCI0)
            // FClose (SCI0)
            /*0x29*/ "SaveGame",
            /*0x2a*/ "RestoreGame",
            /*0x2b*/ "RestartGame",
            /*0x2c*/ "GameIsRestarting",
            /*0x2d*/ "DoSound",
            /*0x2e*/ "NewList",
            /*0x2f*/ "DisposeList",
            /*0x30*/ "NewNode",
            /*0x31*/ "FirstNode",
            /*0x32*/ "LastNode",
            /*0x33*/ "EmptyList",
            /*0x34*/ "NextNode",
            /*0x35*/ "PrevNode",
            /*0x36*/ "NodeValue",
            /*0x37*/ "AddAfter",
            /*0x38*/ "AddToFront",
            /*0x39*/ "AddToEnd",
            /*0x3a*/ "FindKey",
            /*0x3b*/ "DeleteKey",
            /*0x3c*/ "Random",
            /*0x3d*/ "Abs",
            /*0x3e*/ "Sqrt",
            /*0x3f*/ "GetAngle",
            /*0x40*/ "GetDistance",
            /*0x41*/ "Wait",
            /*0x42*/ "GetTime",
            /*0x43*/ "StrEnd",
            /*0x44*/ "StrCat",
            /*0x45*/ "StrCmp",
            /*0x46*/ "StrLen",
            /*0x47*/ "StrCpy",
            /*0x48*/ "Format",
            /*0x49*/ "GetFarText",
            /*0x4a*/ "ReadNumber",
            /*0x4b*/ "BaseSetter",
            /*0x4c*/ "DirLoop",
            /*0x4d*/ "CantBeHere",       // CanBeHere in older SCI versions
            /*0x4e*/ "OnControl",
            /*0x4f*/ "InitBresen",
            /*0x50*/ "DoBresen",
            /*0x51*/ "Platform",        // DoAvoider (SCI0)
            /*0x52*/ "SetJump",
            /*0x53*/ "SetDebug",        // for debugging
            /*0x54*/ "InspectObj",      // for debugging
            /*0x55*/ "ShowSends",       // for debugging
            /*0x56*/ "ShowObjs",        // for debugging
            /*0x57*/ "ShowFree",        // for debugging
            /*0x58*/ "MemoryInfo",
            /*0x59*/ "StackUsage",      // for debugging
            /*0x5a*/ "Profiler",        // for debugging
            /*0x5b*/ "GetMenu",
            /*0x5c*/ "SetMenu",
            /*0x5d*/ "GetSaveFiles",
            /*0x5e*/ "GetCWD",
            /*0x5f*/ "CheckFreeSpace",
            /*0x60*/ "ValidPath",
            /*0x61*/ "CoordPri",
            /*0x62*/ "StrAt",
            /*0x63*/ "DeviceInfo",
            /*0x64*/ "GetSaveDir",
            /*0x65*/ "CheckSaveGame",
            /*0x66*/ "ShakeScreen",
            /*0x67*/ "FlushResources",
            /*0x68*/ "SinMult",
            /*0x69*/ "CosMult",
            /*0x6a*/ "SinDiv",
            /*0x6b*/ "CosDiv",
            /*0x6c*/ "Graph",
            /*0x6d*/ "Joystick",
            // End of kernel function table for SCI0
            /*0x6e*/ "ShiftScreen",     // never called?
            /*0x6f*/ "Palette",
            /*0x70*/ "MemorySegment",
            ///*0x71*/ "Intersections",	// MoveCursor (SCI1 late), PalVary (SCI1.1)
            /*0x71*/ "PalVary",	        // MoveCursor (SCI1 late), PalVary (SCI1.1)
            /*0x72*/ "Memory",
            /*0x73*/ "ListOps",         // never called?
            /*0x74*/ "FileIO",
            /*0x75*/ "DoAudio",
            /*0x76*/ "DoSync",
            /*0x77*/ "AvoidPath",
            /*0x78*/ "Sort",            // StrSplit (SCI01)
            /*0x79*/ "ATan",            // never called?
            /*0x7a*/ "Lock",
            /*0x7b*/ "StrSplit",
            /*0x7c*/ "Message",         // GetMessage (before SCI1.1)
            /*0x7d*/ "IsItSkip",
            /*0x7e*/ "MergePoly",
            /*0x7f*/ "ResCheck",
            /*0x80*/ "AssertPalette",
            /*0x81*/ "TextColors",
            /*0x82*/ "TextFonts",
            /*0x83*/ "Record",          // for debugging
            /*0x84*/ "PlayBack",        // for debugging
            /*0x85*/ "ShowMovie",
            /*0x86*/ "SetVideoMode",
            /*0x87*/ "SetQuitStr",
            /*0x88*/ "DbugStr"          // for debugging
        };
    }
}
