namespace DeepData.Stego.Utils;

public static class DirectionWorker
{
    /// <summary>
    /// Идёт вверх от текущей директории, пока не найдёт .sln-файл.
    /// </summary>
    /// <returns>Путь к директории, где лежит решение.</returns>
    /// <exception cref="FileNotFoundException">Если .sln не найден ни в одной из родительских директорий.</exception>
    public static string FindSolutionDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (dir != null)
        {
            var slnFile = dir.GetFiles("*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (slnFile != null)
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("Не удалось найти .sln-файл в текущей или вышестоящих директориях.");
    }
    
    public static string InitSolutionRoot()
    {
        return FindSolutionDirectory();
    }

    public static string GetProjectRoot(string projectName)
    {
        return Path.Combine(Constants.SolutionRoot.Value, projectName);
    }
}