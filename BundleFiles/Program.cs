using System.CommandLine;

var rootCommand = new RootCommand("root command for file bundle CLI");


var bundleCommand = new Command("bundle", "Bundle code files into a single file");

var createRspCommand = new Command("create-rsp", "Creating respond file with your requirements, for use next time bundle command");


var outputOption = new Option<FileInfo>("--output", "File path and name") { IsRequired = true };
outputOption.AddAlias("-o");

var languageOption = new Option<string>("--language", "List of programming languages") { IsRequired = true };
languageOption.AddAlias("-l");

var noteOption = new Option<bool>("--note", "Note about the source code");
noteOption.AddAlias("-n");
noteOption.SetDefaultValue(false);

var sortOption = new Option<string>("--sort", "Sort order (name or type)");
sortOption.AddAlias("-s");
sortOption.SetDefaultValue("name");

var emptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines");
emptyLinesOption.AddAlias("-re");
emptyLinesOption.SetDefaultValue(false);

var authorOption = new Option<string>("--author", "Author's name");
authorOption.AddAlias("-a");


bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(emptyLinesOption);
bundleCommand.AddOption(authorOption);

string currentPath = Directory.GetCurrentDirectory();
List<string> allFolders = Directory.GetFiles(currentPath, "", SearchOption.AllDirectories).Where(file => !file.Contains("bin") && !file.Contains("Debug") && !file.Contains("node_modules") && !file.Contains(".git") && !file.Contains(".vscode")).ToList();

string[] allLanguages = { "c", "c++", "java", "c#", "javascript", "html", "pyton", "asembler", "SQL" };
string[] allExtensions = { ".c", ".cpp", ".java", ".cs", ".js", ".html", ".py", ".asm", ".sql" };


bundleCommand.SetHandler((filePath, languages, note, sort, removeEmptyLines, author) =>
{
    string[] currentLanguages = GetLanguages(languages, allExtensions, allLanguages);
    List<string> files = allFolders.Where(file => currentLanguages.Contains(Path.GetExtension(file))).ToList();

    files.Sort(); // alphabetic sort - Default
    if (sort == "type")
        files = files.OrderBy(f => Path.GetExtension(f)).ToList();
    else if (sort != "name")
        Console.WriteLine("The sort type is incorrect, default sort is used");

    WriteToFile(filePath.FullName, author, note, files, removeEmptyLines, currentPath);

}, outputOption, languageOption, noteOption, sortOption, emptyLinesOption, authorOption);


createRspCommand.SetHandler(() =>
{
    Console.WriteLine("We make a response file, What will it be called?");
    string fileName = Console.ReadLine();
    if (fileName == "")
        fileName = "response";
    string filePath, languages, sort, author = "";
    char note, removeEmptyLines, t;
    StreamWriter file = new StreamWriter($"{fileName}.rsp");
    Console.WriteLine("Enter file name (you may add all path)");
    filePath = Console.ReadLine();
    while (filePath.Length == 0)
    {
        Console.WriteLine("This field is required!");
        filePath = Console.ReadLine();
    }
    Console.WriteLine("Which languages you want to include? (you may choose to include all by 'all')");
    languages = Console.ReadLine();
    while (languages.Length == 0)
    {
        Console.WriteLine("This field is reqired!");
        languages = Console.ReadLine();
    }
    Console.WriteLine("How to sort (name / type)?");
    sort = Console.ReadLine();
    Console.WriteLine("Do you want to write the source file? (y/n)");
    note = char.Parse(Console.ReadLine());
    Console.WriteLine("Do you want to write author name? (y/n)");
    t = char.Parse(Console.ReadLine());
    if (t == 'y' || t == 'Y')
    {
        Console.WriteLine("What is author name?");
        author = Console.ReadLine();
    }
    Console.WriteLine("Do you want to remove empty lines? (y/n)");
    removeEmptyLines = char.Parse(Console.ReadLine());

    file.Write("bundle");
    file.Write(" --output " + filePath);
    file.Write($" -l \"{languages}\" ");
    if (author != "")
        file.Write(" -a " + author);
    if (sort.Length > 0)
        file.Write(" -s " + sort);
    if (note.Equals("y") || note.Equals("Y"))
        file.Write(" -n ");
    if (removeEmptyLines.Equals("y") || removeEmptyLines.Equals("Y"))
        file.Write(" -re ");
    file.Close();
    Console.WriteLine($"Response file added succesfully called {fileName}.rsp, now you can use it.");
    Console.WriteLine("Good Luck"); ;
});

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

rootCommand.InvokeAsync(args);


void WriteToFile(string fullName, string author, bool note, List<string> files, bool removeEmptyLines, string currentPath)
{
    try
    {
        StreamWriter file = new StreamWriter(fullName);
        Console.WriteLine("file was created successfully");

        if (author != null)
            file.WriteLine("// author: " + author);
        if (note)
            file.WriteLine("// source file: " + currentPath);

        for (int i = 0; i < files.Count; i++)
        {
            file.WriteLine("\n//===== " + Path.GetFileName(files[i]) + " =====\n");
            StreamReader reader = new StreamReader(files[i]);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if ((removeEmptyLines && line.Length > 0) || !removeEmptyLines)
                    file.WriteLine(line);
            }
            reader.Close();
        }

        file.WriteLine("\n// Have a Good Day!");
        file.Close();
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: file path is invalid");
    }
}

static string[] GetLanguages(string currentLanguages, string[] allExtensions, string[] allLanguages)
{
    if (currentLanguages.Contains("all"))
        return allExtensions;
    string[] languages = currentLanguages.Split(' ');
    for (int i = 0; i < languages.Length; i++)
        if (allLanguages.Contains(languages[i]))
            languages[i] = allExtensions[Array.IndexOf(allLanguages, languages[i])];
        else Console.WriteLine($"Language {languages[i]} is not recognized, It is ignored");
    return languages.Where(l => l[0] == '.').ToArray();
}
