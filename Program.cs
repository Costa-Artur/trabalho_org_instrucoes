
internal class Program
{
     private static void Main(string[] args)
    {
        string arquivoEntrada = "teste.txt";

        PipelineSimulator.ProcessarInstrucoes(arquivoEntrada, "saida_sem_forwarding.txt", forwarding: false, incluirNops: false);
        PipelineSimulator.ProcessarInstrucoes(arquivoEntrada, "saida_com_forwarding.txt", forwarding: true, incluirNops: false);
        PipelineSimulator.ProcessarInstrucoes(arquivoEntrada, "saida_sem_forwarding_com_nops.txt", forwarding: false, incluirNops: true);
        PipelineSimulator.ProcessarInstrucoes(arquivoEntrada, "saida_com_forwarding_com_nops.txt", forwarding: true, incluirNops: true);
        PipelineSimulator.GerarArquivosReordenados("teste.txt");

        Console.WriteLine("Simulações finalizadas. Verifique os arquivos de saída.");
    }
}