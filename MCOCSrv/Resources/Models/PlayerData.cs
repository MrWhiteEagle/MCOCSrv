namespace MCOCSrv.Resources.Models
{
    public class PlayerData
    {
        public string UUID { get; set; }
        public string Name { get; set; }

        public PlayerData(string uuid, string name)
        {
            UUID = uuid;
            Name = name;
        }
    }
}
