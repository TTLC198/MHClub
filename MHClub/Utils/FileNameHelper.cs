namespace MHClub.Utils;

public static class FileNameHelper
{
    public static string GetUniqueFileName(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        
        return string.Concat(Path.GetFileNameWithoutExtension(fileName),
            "_",
            Guid.NewGuid().ToString().AsSpan(0, 4),
            Path.GetExtension(fileName));
    }

    public static string GetLocalPath(this string fileName) => 
        string.Join(@"/", fileName.Split(Path.DirectorySeparatorChar).SkipWhile(s => s != "wwwroot").Skip(1));
}