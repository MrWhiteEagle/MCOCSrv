namespace MCOCSrv.Resources.Classes
{
    public static class Global
    {

        public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MCOCSrv");
        public static readonly string AppDataSourcesPath = Path.Combine(AppDataPath, "sources");
        public static readonly string AppDataInstancesFilePath = Path.Combine(AppDataPath, "instances.json");
        public static readonly string AppDataInstancesPath = Path.Combine(AppDataPath, "instances");



        public static void EnsurePathsExist()
        {
            Directory.CreateDirectory(AppDataPath);
            Directory.CreateDirectory(AppDataSourcesPath);
            Directory.CreateDirectory(AppDataInstancesPath);
        }
    }
}
