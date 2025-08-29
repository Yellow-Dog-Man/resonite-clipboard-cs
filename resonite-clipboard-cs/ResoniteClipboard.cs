using System.Runtime.InteropServices;
using System.Text;

namespace resonite_clipboard_cs;

public static partial class ResoniteClipboard
{
    private const string ClipboardLib = "libresonite_clipboard_rs";


    /// <summary>
    /// Copies text to the wayland clipboard.
    /// </summary>
    /// <param name="data">Text to be held in the clipboard.</param>
    [LibraryImport(ClipboardLib, EntryPoint = "copy_text", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void CopyText(string data);


    /// <summary>
    /// Copies arbitrary binary data to the wayland clipboard, inferring the mime type from the data provided.
    /// </summary>
    /// <param name="data">Binary data to be held in the clipboard.</param>
    /// <param name="data_length">Length of the binary data.</param>
    [LibraryImport(ClipboardLib, EntryPoint = "copy_auto")]
    public static partial void CopyAuto(byte[] data, uint data_length);


    /// <summary>
    /// Copies arbitrary binary data to the wayland clipboard with the specified mime type.
    /// </summary>
    /// <param name="data">Binary data to be held in the clipboard.</param>
    /// <param name="data_length">Length of the binary data.</param>
    /// <param name="mime_type">Mime type that the binary data should be identified as.</param>
    [LibraryImport(ClipboardLib, EntryPoint = "copy_with_type", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void CopyWithType(byte[] data, uint data_length, string mime_type);

    [LibraryImport(ClipboardLib, EntryPoint = "available_mime_types")]
    private static unsafe partial byte* AvailableMimeTypes_Raw(out uint size);

    [LibraryImport(ClipboardLib, EntryPoint = "paste_text")]
    private static unsafe partial byte* PasteText_Raw(out uint size);

    [LibraryImport(ClipboardLib, EntryPoint = "paste_auto")]
    private static unsafe partial byte* PasteAuto_Raw(out uint size);

    [LibraryImport(ClipboardLib, EntryPoint = "paste_with_type", StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial byte* PasteWithType_Raw(string mime_type, out uint size);


    /// <summary>
    /// Queries available mime types currently in the wayland clipboard.
    /// </summary>
    /// <returns>An array of strings where each string is a mime type currently in the clipboard, or null if no mime types exist.</returns>
    public static unsafe string[]? AvailableMimeTypes()
    {
        byte* mimesPtr = null;

        try
        {
            mimesPtr = AvailableMimeTypes_Raw(out uint size);
            return Encoding.UTF8.GetString(mimesPtr, (int)size)?.Split('\n').ToArray();
        }
        finally
        {
            NativeMemory.Free(mimesPtr);
        }
    }


    /// <summary>
    /// Pastes text from the wayland clipboard.
    /// </summary>
    /// <returns>Text that currently exists in the wayland clipboard, or null if no text exists.</returns>
    public static unsafe string? PasteText()
    {
        byte* strPtr = null;

        try
        {
            strPtr = PasteText_Raw(out uint size);
            return Encoding.UTF8.GetString(strPtr, (int)size);
        }
        finally
        {
            NativeMemory.Free(strPtr);
        }
    }


    /// <summary>
    /// Pastes arbitrary binary data from the wayland clipboard, automatically choosing a mime type to format the data as.
    /// </summary>
    /// <returns>Arbitrary binary data that currently exists in the wayland clipboard formatted to an automatically-chosen mime type, or null if no data exists.</returns>
    public static unsafe byte[]? PasteAuto()
    {
        byte* pastedPtr = null;
        byte[]? pasted = null;

        try
        {
            pastedPtr = PasteAuto_Raw(out uint size);

            if (size == 0)
                return null;

            pasted = new byte[size];

            uint i = size;
            while (i-- > 0) // Copy manually since Marshal.Copy only takes an int for the length to copy, not a uint.
                pasted[i] = pastedPtr[i];
        }
        finally
        {
            NativeMemory.Free(pastedPtr);
        }

        return pasted;
    }


    /// <summary>
    /// Pastes arbitrary binary data from the wayland clipboard, formatted to the specified mime type.
    /// </summary>
    /// <param name="mime_type">The mime type to format the binary data as.</param>
    /// <returns>Arbitary binary data that currently exists in the wayland clipboard formatted to the specified mime type, or null if no data exists.</returns>
    public static unsafe byte[]? PasteWithType(string mime_type)
    {
        byte* pastedPtr = null;
        byte[]? pasted = null;

        try
        {
            pastedPtr = PasteWithType_Raw(mime_type, out uint size);

            if (size == 0)
                return null;

            pasted = new byte[size];

            uint i = size;
            while (i-- > 0) // Copy manually since Marshal.Copy only takes an int for the length to copy, not a uint.
                pasted[i] = pastedPtr[i];
        }
        finally
        {
            NativeMemory.Free(pastedPtr);
        }

        return pasted;
    }
}
