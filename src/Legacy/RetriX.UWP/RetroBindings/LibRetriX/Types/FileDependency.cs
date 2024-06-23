namespace LibRetriX
{
    public class FileDependency
    {
        public string Name = "";
        public string Description = "";
        public string MD5 = "";
        public bool Optional = false;
        public bool IsFolder = false;

        public FileDependency(string name, string description, string md5, bool optional = false, bool folder = false)
        {
            Name = name;
            Description = description;
            MD5 = md5;
            Optional = optional;
            IsFolder = folder;
        }
    }
}
