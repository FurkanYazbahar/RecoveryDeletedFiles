using RecoveryDeletedFiles;

static void Main(string[] args)
{
    try
    {
        DiskReader diskReader = new DiskReader();
        diskReader.OpenDisk("C"); // Change to appropriate drive letter

        byte[] mftData = diskReader.ReadData(0, 1024 * 1024); // Read 1MB of MFT data

        MFTParser mftParser = new MFTParser();
        var deletedFiles = mftParser.IdentifyDeletedFiles(mftData);

        FileRecovery fileRecovery = new FileRecovery(diskReader);
        foreach (var deletedFile in deletedFiles)
        {
            Console.WriteLine($"Deleted File Found: {deletedFile.FileName}");
            fileRecovery.RecoverFileDataBlocks(deletedFile.MFTRecord, deletedFile.FileName);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}