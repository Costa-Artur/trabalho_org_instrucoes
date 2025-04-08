namespace trabalho_instrucoes;
class Program
{
    private static readonly Dictionary<char, string> hexCharacterToBinary = new Dictionary<char, string> {
        { '0', "0000" },
        { '1', "0001" },
        { '2', "0010" },
        { '3', "0011" },
        { '4', "0100" },
        { '5', "0101" },
        { '6', "0110" },
        { '7', "0111" },
        { '8', "1000" },
        { '9', "1001" },
        { 'a', "1010" },
        { 'b', "1011" },
        { 'c', "1100" },
        { 'd', "1101" },
        { 'e', "1110" },
        { 'f', "1111" }
    };

        private static readonly Dictionary<string, string> instructionDictionary = new Dictionary<string, string> {
            { "0110111", "U" },  // U
            { "0010111", "U" },  // U
            { "1101111", "J" },  // J
            { "1110011", "I" },  // I
            { "0001111", "I" },  // I
            { "0110011", "R" },  // R
            { "0010011", "I" },  // I
            { "0100011", "S" },  // S
            { "0000011", "I" },  // I
            { "1100011", "B" }   // B
        };


    static void Main(string[] args)
    {
        string caminhoArquivo = "teste.txt";

        foreach (string linha in File.ReadLines(caminhoArquivo))
        {
            string binarioCombinado = "";
            foreach (char caractere in linha)
            {
                if (hexCharacterToBinary.TryGetValue(char.ToLower(caractere), out string valorBinario))
                {
                    binarioCombinado += valorBinario;
                }
                else
                {
                    Console.WriteLine($"Caractere '{caractere}' não encontrado no dicionário.");
                }
            }

            if(binarioCombinado.Length >= 7) {
                string seteBits = binarioCombinado.Substring(binarioCombinado.Length - 7);

                if(instructionDictionary.TryGetValue(seteBits, out string tipoInstrucao))
                {
                    Console.WriteLine($"{seteBits} - Tipo: {tipoInstrucao}");
                } else {
                    Console.WriteLine("Instrução não reconhecida");
                }
            } else {
                Console.WriteLine("Não Reconhecido");
            }

            Console.WriteLine();

        }
    }
}
