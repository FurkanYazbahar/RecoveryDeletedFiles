using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecoveryDeletedFiles
{
    public class FileRecovery
    {
        const int BUFFER_SIZE = 1024 * 1024; // 1MB buffer size
        const int DATA_ATTRIBUTE_TYPE = 0x80; // NTFS data attribute type code

        private DiskReader _diskReader;

        public FileRecovery(DiskReader diskReader)
        {
            _diskReader = diskReader;
        }

        public void RecoverFileDataBlocks(byte[] mftRecord, string fileName)
        {
            try
            {
                int offset = 0;
                while (offset < mftRecord.Length)
                {
                    int attributeType = BitConverter.ToInt32(mftRecord, offset);
                    if (attributeType == DATA_ATTRIBUTE_TYPE)
                    {
                        int length = BitConverter.ToInt32(mftRecord, offset + 4);
                        int nonResidentFlag = mftRecord[offset + 8];
                        if (nonResidentFlag != 0)
                        {
                            int dataRunOffset = offset + 64; // Example offset for data run (needs proper calculation)
                            ReadDataRuns(mftRecord, dataRunOffset, fileName);
                        }
                    }
                    offset += BitConverter.ToInt32(mftRecord, offset + 4);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Error recovering data blocks for file {fileName}: {ex.Message}", ex);
            }
        }

        private void ReadDataRuns(byte[] mftRecord, int dataRunOffset, string fileName)
        {
            try
            {
                List<(long Offset, long Length)> dataRuns = ParseDataRuns(mftRecord, dataRunOffset);
                using (FileStream fs = new FileStream(fileName + "_recovered", FileMode.Create, FileAccess.Write))
                {
                    foreach (var dataRun in dataRuns)
                    {
                        try
                        {
                            byte[] dataBlock = _diskReader.ReadData(dataRun.Offset, (int)dataRun.Length);
                            fs.Write(dataBlock, 0, dataBlock.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading data run at offset {dataRun.Offset}: {ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"Recovered data for file: {fileName}");
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading data runs for file {fileName}: {ex.Message}", ex);
            }
        }

        private List<(long Offset, long Length)> ParseDataRuns(byte[] mftRecord, int dataRunOffset)
        {
            List<(long Offset, long Length)> dataRuns = new List<(long Offset, long Length)>();
            long currentOffset = 0;

            try
            {
                while (dataRunOffset < mftRecord.Length && mftRecord[dataRunOffset] != 0)
                {
                    byte header = mftRecord[dataRunOffset];
                    int lengthSize = header & 0x0F;
                    int offsetSize = (header >> 4) & 0x0F;

                    long runLength = 0;
                    for (int i = 0; i < lengthSize; i++)
                    {
                        runLength |= (long)mftRecord[dataRunOffset + 1 + i] << (i * 8);
                    }

                    long runOffset = 0;
                    for (int i = 0; i < offsetSize; i++)
                    {
                        runOffset |= (long)mftRecord[dataRunOffset + 1 + lengthSize + i] << (i * 8);
                    }

                    if ((mftRecord[dataRunOffset + 1 + lengthSize + offsetSize - 1] & 0x80) != 0)
                    {
                        runOffset |= -1L << (offsetSize * 8);
                    }

                    currentOffset += runOffset;
                    dataRuns.Add((currentOffset, runLength));

                    dataRunOffset += 1 + lengthSize + offsetSize;
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Error parsing data runs: {ex.Message}", ex);
            }

            return dataRuns;
        }
    }
}
