using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace resonite_clipboard_cs;

public static partial class ResoniteClipboard
{
    private static readonly string WlCopyPath = Path.Combine(AppContext.BaseDirectory, "runtimes",
        RuntimeInformation.RuntimeIdentifier, "native", "wl-copy.bin");

    private static readonly string WlPastePath = Path.Combine(AppContext.BaseDirectory, "runtimes",
        RuntimeInformation.RuntimeIdentifier, "native", "wl-paste.bin");

    /// <summary>
    /// Copies text to the wayland clipboard.
    /// </summary>
    /// <param name="data">Text to be held in the clipboard.</param>
    public static void CopyText(string data)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlCopyPath,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();

        using (var writer = new StreamWriter(process.StandardInput.BaseStream, new UTF8Encoding(false)))
        {
            writer.Write(data);
            writer.Flush();
        }

        EnsureProcessSuccess(process);
    }


    /// <summary>
    /// Copies arbitrary binary data to the wayland clipboard, inferring the mime type from the data provided.
    /// </summary>
    /// <param name="data">Binary data to be held in the clipboard.</param>
    /// <param name="data_length">Length of the binary data.</param>
    public static void CopyAuto(byte[] data, uint data_length)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlCopyPath,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();

        process.StandardInput.BaseStream.Write(data, 0, (int)data_length);
        process.StandardInput.BaseStream.Flush();
        process.StandardInput.Close();

        EnsureProcessSuccess(process);
    }


    /// <summary>
    /// Copies arbitrary binary data to the wayland clipboard with the specified mime type.
    /// </summary>
    /// <param name="data">Binary data to be held in the clipboard.</param>
    /// <param name="data_length">Length of the binary data.</param>
    /// <param name="mime_type">Mime type that the binary data should be identified as.</param>
    public static void CopyWithType(byte[] data, uint data_length, string mime_type)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlCopyPath,
                RedirectStandardInput = true,
                UseShellExecute = false,
                ArgumentList =
                {
                    "--type",
                    mime_type
                }
            }
        };

        process.Start();

        process.StandardInput.BaseStream.Write(data, 0, (int)data_length);
        process.StandardInput.BaseStream.Flush();

        process.StandardInput.Close();

        EnsureProcessSuccess(process);
    }

    /// <summary>
    /// Queries available mime types currently in the wayland clipboard.
    /// </summary>
    /// <returns>An array of strings where each string is a mime type currently in the clipboard, or null if no mime types exist.</returns>
    public static string[]? AvailableMimeTypes()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlPastePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                ArgumentList = { "-l" }
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();

        EnsureProcessSuccess(process);

        var lines = output.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        return lines.Length == 0 ? null : lines;
    }


    /// <summary>
    /// Pastes text from the wayland clipboard.
    /// </summary>
    /// <returns>Text that currently exists in the wayland clipboard, or null if no text exists.</returns>
    public static string? PasteText()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlPastePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                ArgumentList =
                {
                    "-n"
                }
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        EnsureProcessSuccess(process);
        return output.Length == 0 ? null : output;
    }


    /// <summary>
    /// Pastes arbitrary binary data from the wayland clipboard, automatically choosing a mime type to format the data as.
    /// </summary>
    /// <returns>Arbitrary binary data that currently exists in the wayland clipboard formatted to an automatically-chosen mime type, or null if no data exists.</returns>
    public static byte[]? PasteAuto()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlPastePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                ArgumentList =
                {
                    "-n"
                }
            }
        };

        process.Start();
        using var ms = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(ms);
        EnsureProcessSuccess(process);

        var result = ms.ToArray();
        return result.Length == 0 ? null : result;
    }


    /// <summary>
    /// Pastes arbitrary binary data from the wayland clipboard, formatted to the specified mime type.
    /// </summary>
    /// <param name="mime_type">The mime type to format the binary data as.</param>
    /// <returns>Arbitary binary data that currently exists in the wayland clipboard formatted to the specified mime type, or null if no data exists.</returns>
    public static byte[]? PasteWithType(string mime_type)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WlPastePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                ArgumentList =
                {
                    "--type",
                    mime_type,
                    "-n"
                }
            }
        };

        process.Start();
        using var ms = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(ms);
        EnsureProcessSuccess(process);

        var result = ms.ToArray();
        return result.Length == 0 ? null : result;
    }

    private static void EnsureProcessSuccess(Process process)
    {
        if (!process.WaitForExit(10000))
        {
            process.Kill();
            throw new InvalidOperationException("wl-clipboard process did not exit after 5 seconds.");
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"wl-clipboard failed. Process exited with code {process.ExitCode}.");
    }
}