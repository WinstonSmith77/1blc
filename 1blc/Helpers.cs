using System.Numerics;

namespace _1blc;

public static class Helpers
{
    public static List<DataItem<T>> Merge<T>(this IEnumerable<Dictionary<int, DataItem<T>>> dicts)
        where T : IFloatingPoint<T>, IMinMaxValue<T>
    {
        var listDicts = dicts.ToList();
        var allKeys = listDicts.SelectMany(i => i.Keys).Distinct();
        var result = new Dictionary<int, DataItem<T>>();

        foreach (var key in allKeys)
        {
            var combined = new DataItem<T> { Sum = T.Zero, Min = T.MaxValue, Max = T.MinValue, Count = 0 };
            foreach (var dict in listDicts)
            {
                if (dict.TryGetValue(key, out var data))
                {
                    combined.Name = data.Name;
                    combined.Sum += data.Sum;
                    combined.Min = T.Min(data.Min, combined.Min);
                    combined.Max = T.Max(data.Max, combined.Max);
                    combined.Count += data.Count;
                }
            }

            combined.Mean = combined.Sum / T.CreateChecked(combined.Count);
            result.Add(key, combined);
        }

        return result.Select(r => r.Value).OrderBy(i => i.Name).ToList();
    }

    public static byte[] ReadBuffer(this FilePart part, int index)
    {
#if OUTPUT
		$"Start read {index}".Dump();
#endif
        using var fs = new FileStream(part.FileName, FileMode.Open, FileAccess.Read);
        fs.Seek(part.Start, SeekOrigin.Begin);
        var buffer = new byte[part.Lenght];
        fs.Read(buffer, 0, (int)part.Lenght);
#if OUTPUT
			$"End read {index}".Dump();
#endif
        return buffer;
    }

    /// <returns>List of FilePart objects representing the boundaries of each part</returns>
    public static List<FilePart> SplitFile(string filePath, int numberOfParts)
    {
        if (numberOfParts <= 0)
            throw new ArgumentException("Number of parts must be greater than 0", nameof(numberOfParts));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var fileSize = new FileInfo(filePath).Length;

        if (fileSize == 0)
        {
            return new List<FilePart>();
        }

        var parts = new List<FilePart>();
        long approximatePartSize = fileSize / numberOfParts;

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            var currentPosition = 0L;

            for (int i = 0; i < numberOfParts; i++)
            {
                long startPosition = currentPosition;
                long targetPosition = (i == numberOfParts - 1) ? fileSize : startPosition + approximatePartSize;

                // For the last part, just go to the end of the file
                if (i == numberOfParts - 1)
                {
                    parts.Add(new FilePart
                    {
                        FileName = filePath,
                        Start = startPosition,
                        End = fileSize
                    });
                    break;
                }

                // Seek to the approximate position
                fileStream.Seek(targetPosition, SeekOrigin.Begin);

                // Find the next line break
                var endPosition = FindNextLineBreak(fileStream, targetPosition, fileSize);

                parts.Add(new FilePart
                {
                    FileName = filePath,
                    Start = startPosition,
                    End = endPosition
                });

                currentPosition = endPosition + 1;
            }
        }

        return parts;
    }


    public static void OpenAndClose(string path)
    {
        const FileOptions FILE_FLAG_NO_BUFFERING = (FileOptions)0x20000000;
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                   FileOptions.WriteThrough | FILE_FLAG_NO_BUFFERING))
        {
        }
    }

    /// <summary>
    /// Finds the next line break starting from the current position in the stream.
    /// </summary>
    /// <param name="stream">The file stream to search in</param>
    /// <param name="startPosition">Position to start searching from</param>
    /// <param name="fileSize">Total size of the file</param>
    /// <returns>Position after the line break, or file size if no line break found</returns>
    private static long FindNextLineBreak(FileStream stream, long startPosition, long fileSize)
    {
        const int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];
        long currentPosition = startPosition;

        while (currentPosition < fileSize)
        {
            var bytesToRead = (int)Math.Min(bufferSize, fileSize - currentPosition);
            var bytesRead = stream.Read(buffer, 0, bytesToRead);

            if (bytesRead == 0)
            {
                break;
            }

            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == '\n')
                {
                    return currentPosition + i + 1; // Position after the line break
                }
                else if (buffer[i] == '\r')
                {
                    // Handle Windows line endings (\r\n)
                    if (i + 1 < bytesRead && buffer[i + 1] == '\n')
                    {
                        return currentPosition + i + 2; // Position after \r\n
                    }
                    else if (i + 1 == bytesRead && currentPosition + i + 1 < fileSize)
                    {
                        // \r is at the end of buffer, need to check next byte
                        stream.Seek(currentPosition + i + 1, SeekOrigin.Begin);
                        int nextByte = stream.ReadByte();
                        if (nextByte == '\n')
                        {
                            return currentPosition + i + 2; // Position after \r\n
                        }
                        else
                        {
                            return currentPosition + i + 1; // Position after \r
                        }
                    }
                    else
                    {
                        return currentPosition + i + 1; // Position after \r
                    }
                }
            }

            currentPosition += bytesRead;
        }

        return fileSize; // No line break found, return end of file
    }
}