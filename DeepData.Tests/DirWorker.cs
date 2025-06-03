namespace DeepData.Test;

public static class DirectionWorker
{
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

        throw new FileNotFoundException("There is something wrong with the solution directory");
    }
    
    public static string GetProjectRoot(string projectName)
    {
        return Path.Combine(TestConstants.SolutionRoot.Value, projectName);
    }
}