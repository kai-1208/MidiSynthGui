// NativeMethods.cs
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MidiSynthGui
{
    internal static class NativeMethods
    {
        private const string DllName = "midi_synth_core.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Synth_Init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Synth_Shutdown();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Synth_LoadSoundFont(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Synth_SetMasterVolume(float volume);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Synth_SetPedalEnabled([MarshalAs(UnmanagedType.I1)] bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Synth_AllNotesOff();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Synth_GetMidiPortCount();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Synth_GetMidiPortName(int index, StringBuilder outBuffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Synth_OpenMidiPort(int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Synth_GetAudioDeviceCount();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Synth_GetAudioDeviceName(int index, StringBuilder outBuffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Synth_StartAudio(int deviceIndex);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Synth_StopAudio();
    }
}