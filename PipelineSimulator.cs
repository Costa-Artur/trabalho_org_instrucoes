public class PipelineSimulator {
    static Dictionary<char, string> hexCharacterToBinary = new Dictionary<char, string> {
        { '0', "0000" }, { '1', "0001" }, { '2', "0010" }, { '3', "0011" },
        { '4', "0100" }, { '5', "0101" }, { '6', "0110" }, { '7', "0111" },
        { '8', "1000" }, { '9', "1001" }, { 'a', "1010" }, { 'b', "1011" },
        { 'c', "1100" }, { 'd', "1101" }, { 'e', "1110" }, { 'f', "1111" }
    };

    static Dictionary<int, string> nomeRegistradores = new Dictionary<int, string> {
        { 0, "x0" },  { 1, "ra" },  { 2, "sp" },  { 3, "gp" },  { 4, "tp" },
        { 5, "t0" },  { 6, "t1" },  { 7, "t2" },  { 8, "s0" },  { 9, "s1" },
        { 10, "a0" }, { 11, "a1" }, { 12, "a2" }, { 13, "a3" }, { 14, "a4" },
        { 15, "a5" }, { 16, "a6" }, { 17, "a7" }, { 18, "s2" }, { 19, "s3" },
        { 20, "s4" }, { 21, "s5" }, { 22, "s6" }, { 23, "s7" }, { 24, "s8" },
        { 25, "s9" }, { 26, "s10" },{ 27, "s11" },{ 28, "t3" }, { 29, "t4" },
        { 30, "t5" }, { 31, "t6" }
    };

    static Dictionary<string, string> instructionDictionary = new Dictionary<string, string> {
        { "0110111", "U" }, { "0010111", "U" }, { "1101111", "J" },
        { "1110011", "I" }, { "0001111", "I" }, { "0110011", "R" },
        { "0010011", "I" }, { "0100011", "S" }, { "0000011", "I" },
        { "1100011", "B" }
    };

    public static void ProcessarInstrucoes(string arquivoEntrada, string arquivoSaida, bool forwarding)
    {
        Queue<int> ultimosRD = new Queue<int>();
        int stalls = 0;
        string tipoInstrucaoAnterior = "";
        int rdAnterior = -1;

        using StreamWriter writer = new StreamWriter(arquivoSaida);
        foreach (string linha in File.ReadLines(arquivoEntrada))
        {
            string binarioCombinado = "";
            foreach (char c in linha)
            {
                if (hexCharacterToBinary.TryGetValue(char.ToLower(c), out string? valor))
                {
                    binarioCombinado += valor;
                }
            }

            if (binarioCombinado.Length < 32)
            {
                writer.WriteLine("Instrução incompleta.");
                continue;
            }

            string opcode = binarioCombinado.Substring(25, 7);
            string tipoInstrucao = instructionDictionary.ContainsKey(opcode) ? instructionDictionary[opcode] : "desconhecido";

            string rdBin = binarioCombinado.Substring(20, 5);
            string rs1Bin = binarioCombinado.Substring(12, 5);
            string rs2Bin = binarioCombinado.Substring(7, 5);

            int rd = Convert.ToInt32(rdBin, 2);
            int rs1 = Convert.ToInt32(rs1Bin, 2);
            int rs2 = Convert.ToInt32(rs2Bin, 2);

            string nomeRd = nomeRegistradores.ContainsKey(rd) ? nomeRegistradores[rd] : $"x{rd}";
            string nomeRs1 = nomeRegistradores.ContainsKey(rs1) ? nomeRegistradores[rs1] : $"x{rs1}";
            string nomeRs2 = nomeRegistradores.ContainsKey(rs2) ? nomeRegistradores[rs2] : $"x{rs2}";

            writer.WriteLine($"Instrução: Tipo {tipoInstrucao} | rd: {nomeRd}, rs1: {nomeRs1}, rs2: {nomeRs2}");

            if (tipoInstrucao == "B" || tipoInstrucao == "J")
            {
                writer.WriteLine("Conflito de controle detectado.");
                writer.WriteLine("Stall de 1 ciclo necessário.");
                stalls++;
            }

            if (!forwarding)
            {
                foreach (int regAnterior in ultimosRD)
                {
                    if (rs1 == regAnterior || rs2 == regAnterior)
                    {
                        string nomeRegAnterior = nomeRegistradores.ContainsKey(regAnterior) ? nomeRegistradores[regAnterior] : $"x{regAnterior}";
                        writer.WriteLine($"Conflito de dados: registrador {nomeRegAnterior} usado nas próximas duas instruções.");
                        writer.WriteLine("Stall necessário (sem forwarding).");
                        stalls++;
                        break;
                    }
                }
            }
            else
            {
                if (tipoInstrucaoAnterior == "I" && opcode == "0110011") // se anterior foi load e atual R
                {
                    if (rs1 == rdAnterior || rs2 == rdAnterior)
                    {
                        string nomeRegAnterior = nomeRegistradores.ContainsKey(rdAnterior) ? nomeRegistradores[rdAnterior] : $"x{rdAnterior}";
                        writer.WriteLine($"Conflito de dados com load: registrador {nomeRegAnterior} usado logo após.");
                        writer.WriteLine("Stall necessário (mesmo com forwarding).");
                        stalls++;
                    }
                }
            }

            ultimosRD.Enqueue(rd);
            if (ultimosRD.Count > 2)
                ultimosRD.Dequeue();

            tipoInstrucaoAnterior = tipoInstrucao;
            rdAnterior = rd;

            writer.WriteLine();
        }

        writer.WriteLine($"Total de stalls: {stalls}");
        Console.WriteLine($"{(forwarding ? "[COM FORWARDING]" : "[SEM FORWARDING]")} Total de stalls: {stalls}");
    }
}