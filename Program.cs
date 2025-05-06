
internal class Program
{
    private static void Main(string[] args)
    {
        string arquivoEntrada = "teste.txt";
        string saidaSemForwarding = "saida_sem_forwarding.txt";
        string saidaComForwarding = "saida_com_forwarding.txt";

        Console.WriteLine("Simulando sem forwarding...");
        PipelineSimulator.ProcessarInstrucoes(arquivoEntrada, saidaSemForwarding, false);

        Console.WriteLine("Simulando com forwarding...");
        PipelineSimulator.ProcessarInstrucoes(arquivoEntrada, saidaComForwarding, true);

        Console.WriteLine("Simulação finalizada. Verifique os arquivos de saída.");
    }
}