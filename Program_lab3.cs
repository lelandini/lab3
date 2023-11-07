using System.Diagnostics;

int count = 0;
object obj = new();

int[,] CalculateAdamsMatrix(int[,] a, int[,] b)
{
    var result = new int[a.GetLength(0), b.GetLength(1)];

    for (int i = 0; i < a.GetLength(0); i++)
    {
        for (int j = 0; j < a.GetLength(1); j++)
        {
            result[i, j] = a[i, j] * b[i, j];
        }
    }

    return result;
}


int[,] GetRandomMatrix(int n)
{
    var rnd = new Random();
    var result = new int[n, n];

    for (int i = 0; i < n; i++)
    {
        for (int j = 0; j < n; j++)
        {
            result[i, j] = rnd.Next(-100, 100);
        }
    }

    return result;
}

void CalculateMatrices(List<int[,]> matrixList, int aStart, int aEnd, int bStart, int bEnd)
{

    for (int i = aStart; i < aEnd; i++)
    {
        for (int j = i + 1; j < matrixList.Count; j++)
        {
            CalculateAdamsMatrix(matrixList[i], matrixList[j]);
            lock (obj)
            {
                count++;
            }
        }
    }


    for (int i = bStart; i < bEnd; i++)
    {
        for (int j = i + 1; j < matrixList.Count; j++)
        {
            CalculateAdamsMatrix(matrixList[i], matrixList[j]);
            lock (obj)
            {
                count++;
            }
        }
    }
}

int matrixSize = 10000;

var matrices = new List<int[,]>(matrixSize);

for (int i = 0; i < matrices.Capacity; i++)
{
    matrices.Add(GetRandomMatrix(10));
}

int[] numberOfThreads = new int[] { 1, 2, 4, 9 };

for (int i = 0; i < numberOfThreads.Length; i++)
{
    var maxThreads = numberOfThreads[i];
    int k = 0;
    count = 0;

    var sw = Stopwatch.StartNew();

    Parallel.For(
        0,
        maxThreads,
        new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
        (i) =>
        {
            int offset = matrixSize / (2 * maxThreads); 
                
            if (i == maxThreads - 1 && 2 * offset * maxThreads != matrixSize)
            {
                offset += (matrixSize - 2 * offset * maxThreads) / 2;
            }

            int aStart = k * offset;
            int aEnd = k * offset + offset;
            int bStart = matrixSize - offset * (k + 1);
            int bEnd = bStart + offset;
            CalculateMatrices(matrices, aStart, aEnd, bStart, bEnd);
            k++;
        });

    sw.Stop();

    Console.WriteLine($"Number of threads: {maxThreads}");
    Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
    Console.WriteLine(count);

}