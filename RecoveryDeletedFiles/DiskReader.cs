using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RecoveryDeletedFiles
{
    public class DiskReader
    {
        const uint GENERIC_READ = 0x80000000;
        const uint OPEN_EXISTING = 3;
        const int BUFFER_SIZE = 1024 * 1024; // 1MB buffer size

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        private IntPtr _diskHandle;

        public void OpenDisk(string driveLetter)
        {
            string path = @"\\.\" + driveLetter + ":";
            _diskHandle = CreateFile(path, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (_diskHandle == IntPtr.Zero)
            {
                throw new IOException($"Unable to open disk {driveLetter} for reading.");
            }
        }

        public byte[] ReadData(long offset, int size)
        {
            byte[] buffer = new byte[size];
            IntPtr ptr = _diskHandle;
            long fileOffset = offset;
            int bytesReadTotal = 0;

            try
            {
                while (size > 0)
                {
                    uint bytesToRead = (uint)Math.Min(BUFFER_SIZE, size);
                    byte[] tempBuffer = new byte[bytesToRead];

                    bool success = ReadFile(ptr, tempBuffer, bytesToRead, out uint bytesRead, IntPtr.Zero);
                    if (!success)
                    {
                        throw new IOException("Failed to read from disk.");
                    }

                    Array.Copy(tempBuffer, 0, buffer, bytesReadTotal, bytesRead);
                    bytesReadTotal += (int)bytesRead;
                    size -= (int)bytesRead;
                    fileOffset += bytesRead;
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading data from disk: {ex.Message}", ex);
            }

            return buffer;
        }
    }
}
