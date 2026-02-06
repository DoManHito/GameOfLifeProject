namespace Backend;

public class GameOfLife
{
    private readonly bool[][] initialGeneration;
    public Bool2X2 CurrentGeneration { get; set; }
    public int Generation { get; private set; }

    public GameOfLife(int rows, int columns)
    {
        Random rand = new Random();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        bool[][] newGeneration = new bool[rows][];
        for (int i = 0; i < rows; i++) newGeneration[i] = new bool[columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                newGeneration[i][j] = Convert.ToBoolean(rand.Next(2));
            }
        }

        this.CurrentGeneration = new Bool2X2 { Data = newGeneration };
        this.initialGeneration = newGeneration;
    }

    public GameOfLife(Bool2X2 pattern)
    {
        this.CurrentGeneration!.Data = pattern.Data;
        this.initialGeneration = pattern.Data;
        this.Generation = 0;
    }

    public void Restart()
    {
        this.CurrentGeneration.Data = (bool[][])this.initialGeneration.Clone();
    }

    public void NextGeneration()
    {
        int rows = this.CurrentGeneration.Data.Length;
        int columns = this.CurrentGeneration.Data[0].Length;
        bool[][] newGeneration = new bool[rows][];
        for (int i = 0; i < rows; i++) newGeneration[i] = new bool[columns];

        _ = Parallel.For(0, rows, i =>
        {
            for (int j = 0; j < columns - 1; j++)
            {
                switch (this.CurrentGeneration.Data[i][j])
                {
                    case true:
                        switch (this.CountAliveNeighbors(i, j))
                        {
                            case < 2:
                                newGeneration[i][j] = false;
                                break;
                            case 2:
                            case 3:
                                newGeneration[i][j] = true;
                                break;
                            case > 3:
                                newGeneration[i][j] = false;
                                break;
                            default:
                        }

                        break;
                    case false:
                        if (this.CountAliveNeighbors(i, j) == 3)
                        {
                            newGeneration[i][j] = true;
                        }

                        break;
                }
            }
        });

        this.Generation++;
        this.CurrentGeneration.Data = newGeneration;
    }

    private int CountAliveNeighbors(int row, int column)
    {
        int count = 0;
        int rows = this.CurrentGeneration.Data.Length;
        int columns = this.CurrentGeneration.Data[0].Length;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                int r = row + i;
                int c = column + j;
                if (r <= 0 || r >= rows || c <= 0 || c >= columns)
                {
                    continue;
                }

                if (this.CurrentGeneration.Data[row + i][column + j])
                {
                    count++;
                }
            }
        }

        return count;
    }
}
