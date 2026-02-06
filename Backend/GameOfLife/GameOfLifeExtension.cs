namespace Backend;

public static class GameOfLifeExtension
{
    public static async Task SimulateCoreAsync(this GameOfLife? game, int generations, TextWriter? writer, char aliveCell, char deadCell)
    {
        int rows = game!.CurrentGeneration.Data.Length;
        int columns = game.CurrentGeneration.Data[0].Length;

        for (int g = 0; g < generations; g++)
        {
            var currentPopulation = game.CurrentGeneration;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (currentPopulation.Data[i][j])
                    {
                        await writer!.WriteAsync(aliveCell);
                    }
                    else
                    {
                        await writer!.WriteAsync(deadCell);
                    }
                }

                await writer!.WriteLineAsync();
            }

            _ = Console.ReadKey();
            game.NextGeneration();
            await writer!.WriteLineAsync();
        }
    }

    public static Task SimulateAsync(this GameOfLife? game, int generations, TextWriter? writer, char aliveCell, char deadCell)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generations);

        return SimulateCoreAsync(game!, generations, writer!, aliveCell, deadCell);
    }
}
