namespace DocToMd;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var form = new MainForm();

        // Support drag-drop of a file directly onto the exe
        if (args.Length == 1 && File.Exists(args[0]))
        {
            form.LoadDocument(args[0], ExtractionLevel.Default);
        }

        Application.Run(form);
    }
}
