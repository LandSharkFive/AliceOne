namespace AliceOne
{
    internal class Program
    {

        /// <summary>
        /// The application entry point that drives a Read-Eval-Print Loop (REPL) for the CSV editor.
        /// </summary>
        static void Main(string[] args)
        {
            var alice = new AliceOne();

            if (args.Length > 0)
            {
                alice.Load(args[0]);
            }

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                // Exit on null (Ctrl+Z) or empty string
                if (string.IsNullOrEmpty(line))
                    break;

                // Trim spaces.
                line = line.Trim();

                // Use the first character as the command, case-insensitive.
                char command = char.ToUpper(line[0]);

                switch (command)
                {
                    case 'A': alice.ShowStatus(); break;
                    case 'C': alice.Clear(); break;
                    case 'D': alice.HandleDelete(line); break;
                    case 'I': alice.HandleInsert(line); break;
                    case 'L': alice.HandleLoad(line); break;
                    case 'O': alice.HandleSort(line); break;
                    case 'Q': return;
                    case 'S': alice.HandleSelect(line); break;
                    case 'W': alice.HandleWrite(line); break;
                    case 'X': alice.FixDuplicates(); break;
                    case '?': alice.ShowHelp(); break;
                    default: Console.WriteLine("Unknown command. Type '?' for help."); break;
                }
            }
        }
    }

}
