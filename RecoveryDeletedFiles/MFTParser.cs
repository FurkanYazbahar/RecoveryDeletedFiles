using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecoveryDeletedFiles
{
    public class MFTParser
    {
        const int RECORD_SIZE = 1024; // Typical MFT record size
        const int FILE_ATTRIBUTE_DELETED = 0x00000001; // Hypothetical value for deleted files (needs proper value)
        const int DATA_ATTRIBUTE_TYPE = 0x80; // NTFS data attribute type code

        public class DeletedFile
        {
            public string FileName { get; set; }
            public byte[] MFTRecord { get; set; }
        }

        public DeletedFile[] IdentifyDeletedFiles(byte[] mftData)
        {
            var deletedFiles = new List<DeletedFile>();

            try
            {
                for (int offset = 0; offset < mftData.Length; offset += RECORD_SIZE)
                {
                    byte[] record = new byte[RECORD_SIZE];
                    Array.Copy(mftData, offset, record, 0, RECORD_SIZE);

                    if (Encoding.ASCII.GetString(record, 0, 4) != "FILE")
                        continue;

                    int flags = BitConverter.ToInt16(record, 22);

                    if ((flags & FILE_ATTRIBUTE_DELETED) == FILE_ATTRIBUTE_DELETED)
                    {
                        int fileNameLength = record[56];
                        string fileName = Encoding.Unicode.GetString(record, 58, fileNameLength * 2);

                        deletedFiles.Add(new DeletedFile
                        {
                            FileName = fileName,
                            MFTRecord = record
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Error parsing MFT data: {ex.Message}", ex);
            }

            return deletedFiles.ToArray();
        }
    }
}
