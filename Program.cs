using System.Runtime.InteropServices;

namespace Lab1
{
    class Program
    {
        private const int ChunkSize = 100 * 1024 * 1024;
        private const int AverageNumberSize = 11;

        static void Main(string[] args)
        {
            MemoryController.LimitMemoryUsage(512);
            
            string inputFile = "unsorted_numbers.txt";
            string outputFile = "sorted_numbers.txt";

            Console.Write("Enter file size (In MB): ");
            int fileSizeInMB = int.Parse(Console.ReadLine());
            long numNumbers = CalculateNumberCount(fileSizeInMB);

            Console.WriteLine("Generating random numbers and filling the file...");
            GenerateRandomNumbersFile(inputFile, numNumbers);

            var startSort = DateTime.Now;

            Console.WriteLine("Splitting the file...");
            List<string> chunkFiles = SplitFile(inputFile);

            Console.WriteLine("Sorting part of the file...");
            foreach (var chunkFile in chunkFiles)
            {
                SortChunk(chunkFile);
            }

            Console.WriteLine("Merging parts of the file...");
            MergeSortedFiles(chunkFiles, outputFile);

            var endSort = DateTime.Now;
            Console.WriteLine("Time taken: " + (endSort - startSort).TotalSeconds + " sec.\n");
        }

        private static void GenerateRandomNumbersFile(string fileName, long numNumbers)
        {
            Random random = new Random();
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                for (long i = 0; i < numNumbers; i++)
                {
                    writer.WriteLine(random.Next(int.MaxValue));
                }
            }
        }

        private static long CalculateNumberCount(int fileSizeInMB)
        {
            long fileSizeInBytes = fileSizeInMB * 1024L * 1024L;
            return fileSizeInBytes / AverageNumberSize;
        }

        private static List<string> SplitFile(string inputFile)
        {
            List<string> chunkFiles = new List<string>();
            using (StreamReader reader = new StreamReader(inputFile))
            {
                string line;
                int counter = 0;
                List<int> numbers = new List<int>();
                while ((line = reader.ReadLine()) != null)
                {
                    numbers.Add(int.Parse(line));
                    if (numbers.Count * sizeof(int) >= ChunkSize)
                    {
                        string chunkFile = $"chunk_{counter++}.txt";
                        WriteChunkToFile(numbers, chunkFile);
                        chunkFiles.Add(chunkFile);
                        numbers.Clear();
                    }
                }
                if (numbers.Any())
                {
                    string chunkFile = $"chunk_{counter}.txt";
                    WriteChunkToFile(numbers, chunkFile);
                    chunkFiles.Add(chunkFile);
                }
            }
            return chunkFiles;
        }

        private static void WriteChunkToFile(List<int> numbers, string chunkFile)
        {
            using (StreamWriter writer = new StreamWriter(chunkFile))
            {
                foreach (int num in numbers)
                {
                    writer.WriteLine(num);
                }
            }
        }

        private static void SortChunk(string chunkFile)
        {
            List<int> numbers = new List<int>();
            using (StreamReader reader = new StreamReader(chunkFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    numbers.Add(int.Parse(line));
                }
            }

            numbers.Sort();

            using (StreamWriter writer = new StreamWriter(chunkFile))
            {
                foreach (int num in numbers)
                {
                    writer.WriteLine(num);
                }
            }
        }

        private static void MergeSortedFiles(List<string> chunkFiles, string outputFile)
        {
            var queue = new PriorityQueue<FileEntry, int>();
            List<StreamReader> readers = new List<StreamReader>();

            try
            {
                foreach (var chunkFile in chunkFiles)
                {
                    var reader = new StreamReader(chunkFile);
                    readers.Add(reader);
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        queue.Enqueue(new FileEntry(int.Parse(line), reader), int.Parse(line));
                    }
                }

                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    while (queue.Count > 0)
                    {
                        FileEntry smallest = queue.Dequeue();
                        writer.WriteLine(smallest.Value);
                        string nextLine = smallest.Reader.ReadLine();
                        if (nextLine != null)
                        {
                            queue.Enqueue(new FileEntry(int.Parse(nextLine), smallest.Reader), int.Parse(nextLine));
                        }
                    }
                }
            }
            finally
            {
                foreach (var reader in readers)
                {
                    reader.Close();
                }
            }
        }

        private class FileEntry
        {
            public int Value { get; }
            public StreamReader Reader { get; }

            public FileEntry(int value, StreamReader reader)
            {
                Value = value;
                Reader = reader;
            }
        }
    }
}