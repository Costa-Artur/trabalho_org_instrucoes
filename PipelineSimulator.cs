public class PipelineSimulator
{
    static Dictionary<char, string> hexCharacterToBinary = new()
    {
        { '0', "0000" }, { '1', "0001" }, { '2', "0010" }, { '3', "0011" },
        { '4', "0100" }, { '5', "0101" }, { '6', "0110" }, { '7', "0111" },
        { '8', "1000" }, { '9', "1001" }, { 'a', "1010" }, { 'b', "1011" },
        { 'c', "1100" }, { 'd', "1101" }, { 'e', "1110" }, { 'f', "1111" }
    };

    static Dictionary<int, string> nomeRegistradores = new()
    {
        { 0, "x0" },  { 1, "ra" },  { 2, "sp" },  { 3, "gp" },  { 4, "tp" },
        { 5, "t0" },  { 6, "t1" },  { 7, "t2" },  { 8, "s0" },  { 9, "s1" },
        { 10, "a0" }, { 11, "a1" }, { 12, "a2" }, { 13, "a3" }, { 14, "a4" },
        { 15, "a5" }, { 16, "a6" }, { 17, "a7" }, { 18, "s2" }, { 19, "s3" },
        { 20, "s4" }, { 21, "s5" }, { 22, "s6" }, { 23, "s7" }, { 24, "s8" },
        { 25, "s9" }, { 26, "s10" },{ 27, "s11" },{ 28, "t3" }, { 29, "t4" },
        { 30, "t5" }, { 31, "t6" }
    };

    static Dictionary<string, string> instructionDictionary = new()
    {
        { "0110111", "U" }, { "0010111", "U" }, { "1101111", "J" },
        { "1110011", "I" }, { "0001111", "I" }, { "0110011", "R" },
        { "0010011", "I" }, { "0100011", "S" }, { "0000011", "I" },
        { "1100011", "B" }
    };

    const string NOP_HEX = "00000013";

public static void ProcessarInstrucoes(string arquivoEntrada, string arquivoSaida, bool forwarding, bool incluirNops)
{
    Queue<int> ultimosRD = new();
    int stalls = 0;
    string tipoInstrucaoAnterior = "";
    int rdAnterior = -1;
    int numNops = 0;

    List<string> instrucoesComNopsHex = new();

    using StreamWriter writer = new(arquivoSaida);
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
            if (!incluirNops)
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

        bool precisaStall = false;

        if (!forwarding)
        {
            foreach (int regAnterior in ultimosRD)
            {
                if (rs1 == regAnterior || rs2 == regAnterior)
                {
                    precisaStall = true;
                    stalls++;
                    break;
                }
            }
        }
        else
        {
            if (tipoInstrucaoAnterior == "I" && opcode == "0110011")
            {
                if (rs1 == rdAnterior || rs2 == rdAnterior)
                {
                    precisaStall = true;
                    stalls++;
                }
            }
        }

        if (incluirNops)
        {
            if (precisaStall)
            {
                instrucoesComNopsHex.Add(NOP_HEX);
                numNops++;
            }

            instrucoesComNopsHex.Add(linha.Trim().ToLower()); 
        }
        else
        {
            writer.WriteLine($"Instrução: Tipo {tipoInstrucao} | rd: {nomeRd}, rs1: {nomeRs1}, rs2: {nomeRs2}");

            if (precisaStall)
            {
                string nomeRegConflito = nomeRegistradores.ContainsKey(rdAnterior) ? nomeRegistradores[rdAnterior] : $"x{rdAnterior}";
                writer.WriteLine($"Conflito de dados: registrador {nomeRegConflito} usado logo após.");
            }

            writer.WriteLine();
        }

        ultimosRD.Enqueue(rd);
        if (ultimosRD.Count > 2)
            ultimosRD.Dequeue();

        tipoInstrucaoAnterior = tipoInstrucao;
        rdAnterior = rd;
    }

    if (incluirNops)
    {
        foreach (var instr in instrucoesComNopsHex)
        {
            writer.WriteLine(instr);
        }
        writer.WriteLine($"Total de NOPS: {numNops}");
        Console.WriteLine($"{(forwarding ? "[COM FORWARDING]" : "[SEM FORWARDING]")} Total de NOPS: {numNops}");
    }
    else
    {
        writer.WriteLine($"Total de stalls: {stalls}");
        Console.WriteLine($"{(forwarding ? "[COM FORWARDING]" : "[SEM FORWARDING]")} Total de stalls: {stalls}");
    }
}

    public static void GerarArquivosReordenados(string entrada)
    {
        ReordenarInstrucoesParaReducaoDeStallsComForwarding(entrada, "saida_com_forwarding_reordenado.txt");
        
        ReordenarInstrucoesParaReducaoDeStallsComForwarding(entrada, "saida_sem_forwarding_reordenado.txt");
    }
    
    public static void ReordenarInstrucoesParaReducaoDeStallsComForwarding(string entrada, string saida)
{
    var instrucoes = File.ReadAllLines(entrada).ToList();
    List<string> resultado = new();
    int i = 0;

    while (i < instrucoes.Count)
    {
        string atual = instrucoes[i];
        string atualBin = string.Join("", atual.Select(c => hexCharacterToBinary[char.ToLower(c)]));
        if (atualBin.Length < 32)
        {
            resultado.Add(atual);
            i++;
            continue;
        }

        string opcodeAtual = atualBin.Substring(25, 7);
        string rdAtual = atualBin.Substring(20, 5);

        bool ehLoad = opcodeAtual == "0000011"; // opcode de LOAD

        if (ehLoad && i + 1 < instrucoes.Count)
        {
            string proxima = instrucoes[i + 1];
            string proximaBin = string.Join("", proxima.Select(c => hexCharacterToBinary[char.ToLower(c)]));
            if (proximaBin.Length < 32)
            {
                resultado.Add(atual);
                i++;
                continue;
            }

            string rs1Prox = proximaBin.Substring(12, 5);
            string rs2Prox = proximaBin.Substring(7, 5);

            if (rs1Prox == rdAtual || rs2Prox == rdAtual)
            {
                bool encontrada = false;
                for (int j = i + 2; j < instrucoes.Count; j++)
                {
                    string candidata = instrucoes[j];
                    string candidataBin = string.Join("", candidata.Select(c => hexCharacterToBinary[char.ToLower(c)]));
                    if (candidataBin.Length < 32) continue;

                    string rs1Cand = candidataBin.Substring(12, 5);
                    string rs2Cand = candidataBin.Substring(7, 5);
                    string rdCand = candidataBin.Substring(20, 5);

                    if (rs1Cand != rdAtual && rs2Cand != rdAtual && rdCand != rdAtual)
                    {
                        resultado.Add(atual);
                        resultado.Add(candidata);
                        instrucoes.RemoveAt(j);
                        resultado.Add(proxima);
                        encontrada = true;
                        break;
                    }
                }

                if (!encontrada)
                {
                    resultado.Add(atual);
                    resultado.Add("00000013"); // NOP necessário
                    resultado.Add(proxima);
                }
                i += 2;
                continue;
            }
        }

        resultado.Add(atual);
        i++;
    }

    File.WriteAllLines(saida, resultado);
}
    
    public static void InserirNopsParaDesvios(string entrada, string saida)
    {
        var instrucoes = File.ReadAllLines(entrada).ToList();
        List<string> resultado = new();

        foreach (var instr in instrucoes)
        {
            string binario = string.Join("", instr.Select(c => hexCharacterToBinary[char.ToLower(c)]));
            if (binario.Length < 32)
            {
                resultado.Add(instr);
                continue;
            }

            string opcode = binario.Substring(25, 7);

            resultado.Add(instr);

            if (opcode == "1100011")
            {
                resultado.Add("00000013");
            }
        }

        File.WriteAllLines(saida, resultado);
        
        int nopsInseridos = resultado.Count(x => x == NOP_HEX);
        Console.WriteLine($"[DESVIO SIMPLES] NOPs inseridos após desvios: {nopsInseridos}");
    }
    
    public static void InserirInstrucaoUtilOuNopAposDesvio(string entrada, string saida)
{
    var instrucoes = File.ReadAllLines(entrada).ToList();
    List<string> resultado = new();

    int i = 0;
    while (i < instrucoes.Count)
    {
        string instrAtual = instrucoes[i];
        string binAtual = string.Join("", instrAtual.Select(c => hexCharacterToBinary[char.ToLower(c)]));

        if (binAtual.Length < 32)
        {
            resultado.Add(instrAtual);
            i++;
            continue;
        }

        string opcodeAtual = binAtual.Substring(25, 7);
        resultado.Add(instrAtual);

        if (opcodeAtual == "1100011")
        {
            if (i + 1 < instrucoes.Count)
            {
                string proximaInstr = instrucoes[i + 1];
                string binProx = string.Join("", proximaInstr.Select(c => hexCharacterToBinary[char.ToLower(c)]));
                if (binProx.Length >= 32)
                {
                    string rs1Desvio = binAtual.Substring(12, 5);
                    string rs2Desvio = binAtual.Substring(7, 5);

                    string rs1Prox = binProx.Substring(12, 5);
                    string rs2Prox = binProx.Substring(7, 5);
                    string rdProx  = binProx.Substring(20, 5);

                    if (rdProx != rs1Desvio && rdProx != rs2Desvio &&
                        rs1Prox != rs1Desvio && rs1Prox != rs2Desvio &&
                        rs2Prox != rs1Desvio && rs2Prox != rs2Desvio)
                    {
                        resultado.Add(proximaInstr);
                        i += 2;
                        continue;
                    }
                }
            }

            resultado.Add("00000013");
        }

        i++;
    }

    File.WriteAllLines(saida, resultado);
    int nopsInseridos = resultado.Count(x => x == NOP_HEX);
    Console.WriteLine($"[DELAYED BRANCH] NOPs inseridos: {nopsInseridos}");
}

}
