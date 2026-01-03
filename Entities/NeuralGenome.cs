namespace KubikDungeon.Entities;

public class NeuralGenome
{
    public double[] Weights { get; private set; }
    public const int GenomeSize = 10;

    public NeuralGenome()
    {
        Weights = new double[GenomeSize];
        Random rnd = new Random();
        for (int i = 0; i < GenomeSize; i++)
            Weights[i] = (rnd.NextDouble() * 2) - 1.0;
    }

    public NeuralGenome Mutate()
    {
        NeuralGenome child = new NeuralGenome();
        Array.Copy(this.Weights, child.Weights, GenomeSize);

        Random rnd = new Random();
        
        // Шанс мутации каждого гена - 30%
        for (int i = 0; i < GenomeSize; i++)
        {
            if (rnd.NextDouble() < 0.3)
            {
                // Либо сдвигаем значение
                child.Weights[i] += (rnd.NextDouble() * 0.8) - 0.4;
                
                // Либо (редко) полностью меняем ген (инновация)
                if (rnd.NextDouble() < 0.1) 
                    child.Weights[i] = (rnd.NextDouble() * 2) - 1.0;

                // Ограничиваем
                if (child.Weights[i] > 1.0) child.Weights[i] = 1.0;
                if (child.Weights[i] < -1.0) child.Weights[i] = -1.0;
            }
        }
        
        return child;
    }
}