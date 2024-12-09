# C# Data Recovery Tool

## Project Description

This project is a C# application capable of scanning and recovering deleted files from a disk. The application detects deleted files in the NTFS file system and recovers their data blocks to reconstruct them. The project consists of three main classes: `DiskReader`, `MFTParser`, and `FileRecovery`.

## Features

- Detects deleted files from the NTFS file system.
- Reads and recovers the data blocks of deleted files.
- Writes the recovered files to a specified location.

## Requirements

- .NET Core SDK
- Windows operating system (compatible with NTFS file system)

## Installation

1. **Clone the repository:**

    ```sh
    git clone https://github.com/FurkanYazbahar/RecoveryDeletedFiles
    cd RecoveryDeletedFiles
    ```

2. **Install the necessary dependencies:**

    Ensure that the .NET Core SDK is installed. Visit the [official site](https://dotnet.microsoft.com/download) to download and install it.

3. **Build the project:**

    ```sh
    dotnet build
    ```

## Usage

1. **Run the application:**

    ```sh
    dotnet run
    ```

2. **Read the raw data from the disk:**

    The `DiskReader` class opens the specified drive and reads the raw data.

3. **Parse MFT records:**

    The `MFTParser` class analyzes MFT records and identifies deleted files.

4. **Recover data blocks:**

    The `FileRecovery` class reads and recovers the data blocks of deleted files.

### Example Usage Code

In the `Main` function of the program, you can run it as follows:

```csharp
class Program
{
    static void Main(string[] args)
    {
        try
        {
            DiskReader diskReader = new DiskReader();
            diskReader.OpenDisk("C"); // Change to the appropriate drive letter

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
}
```

## License

This project is licensed under the Apache License 2.0. See the `LICENSE` file for more information.
