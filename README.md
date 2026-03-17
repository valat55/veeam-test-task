# Folder Synchronization Tool

A one-way folder synchronization tool written in C# for the Veeam SDET test task. The program maintains an identical copy of a source folder at a replica (destination) folder, performing periodic synchronization with full logging.

## How It Works

The program builds an in-memory model of the folder tree on first run, copying the entire source to the replica. On each subsequent sync cycle it compares source and replica, then applies changes in this order:

1. **Delete** files and folders that no longer exist in source
2. **Update** files whose size or MD5 hash differs
3. **Recurse** into existing subfolders
4. **Add** new files and folders that appeared in source

File changes are detected using a size check first (fast), falling back to MD5 hash comparison only when sizes match. All file operations use best-effort error handling — if a single file fails (e.g. locked by another process), the sync continues and errors are reported at the end.

## Usage

```
VeeamTestTask.exe -s <source> -d <replica> -i <interval> -l <logfile>
```

| Argument | Description |
|---|---|
| `-s`, `--src` | Path to the source folder |
| `-d`, `--dest` | Path to the replica folder |
| `-i`, `--interval` | Synchronization interval in seconds |
| `-l`, `--log` | Path to the log file |

### Example

```
VeeamTestTask.exe -s "C:\Data\Source" -d "C:\Data\Replica" -i 30 -l "C:\Logs\sync.log"
```

This will synchronize `Source` → `Replica` every 30 seconds, logging all operations to both console and `sync.log`.

## Project Structure

```
VeeamTestTask/
├── Program.cs          # Entry point, argument parsing, sync loop
├── Config.cs           # Static configuration from CLI arguments
├── FolderModel.cs      # Recursive folder tree model with sync logic
├── SyncPlan.cs         # Computes file/folder diffs between source and replica
├── CommandParser.cs    # Handles filling the configuration and hase some side effects(creating and checking files/folders)
TestProject/
├── BaseTestClass.cs     # Generic test class that implements common methods and properties for another tests
├── ConstructorTests.cs  # Unit tests for FolderModel constructor
├── SyncPlanTests.cs     # Unit tests for SyncPlan class
├── SyncTests.cs         # Unit tests for Synchronization Algorithm
├── ToStringTests        # Unit tests for FoldeModel to string method
VeeamTestTask.sln
```

## Design Decisions

- **No third-party sync libraries** — as per the task requirements, synchronization logic is implemented from scratch.
- **CommandLineParser** — used for argument parsing (allowed by task guidelines as a well-known utility library).
- **MD5 via `System.Security.Cryptography`** — built-in .NET library for hash comparison. MD5 is chosen for speed since this is change detection, not security.
- **Best-effort error handling** — locked or inaccessible files are logged as errors but don't halt synchronization. The model only updates after a successful IO operation, keeping it consistent with the actual disk state.
- **Reparse point filtering** — symlinks and NTFS junction points are skipped to avoid following links outside the source tree.
- **SyncPlan separation** — set operations (what to delete/update/add) are computed once per sync cycle in a dedicated class, avoiding redundant filesystem reads.

## Requirements

- .NET 9.0
- Windows (tested on Windows 11 / NTFS)

## Running Tests

```
cd TestProject
dotnet test
```
