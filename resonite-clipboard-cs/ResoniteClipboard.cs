using System.Runtime.InteropServices;

namespace resonite_clipboard_cs;

public static partial class ResoniteClipboard
{
    private const string ClipboardLib = "libresonite_clipboard_rs";

    [LibraryImport(ClipboardLib, EntryPoint = "copy_text", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void CopyText(string data);

    [LibraryImport(ClipboardLib, EntryPoint = "copy_auto")]
    public static partial void CopyAuto(byte[] data, uint data_length);

    [LibraryImport(ClipboardLib, EntryPoint = "copy_with_type", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void CopyWithType(byte[] data, uint data_length, string mime_type);

    [LibraryImport(ClipboardLib, EntryPoint = "available_mime_types")]
    public static unsafe partial IntPtr AvailableMimeTypes(IntPtr sizePtr);

    [LibraryImport(ClipboardLib, EntryPoint = "paste_text")]
    public static unsafe partial IntPtr PasteText(IntPtr sizePtr);

    [LibraryImport(ClipboardLib, EntryPoint = "paste_auto")]
    public static unsafe partial IntPtr PasteAuto(IntPtr sizePtr);

    [LibraryImport(ClipboardLib, EntryPoint = "paste_with_type", StringMarshalling = StringMarshalling.Utf8)]
    public static unsafe partial IntPtr PasteWithType(string mime_type, IntPtr sizePtr);
}
