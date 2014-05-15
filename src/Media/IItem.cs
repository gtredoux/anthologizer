using System;
namespace com.renoster.Anthologizer.Media
{
    public enum ItemTypeEnum { atomic, composite };

    public interface IItem
    {
        ItemTypeEnum ItemType { get; }
        
        string Id { get; set; }
        string Mimetype { get; set; }
        string Name { get; set; }
        long Size { get; set; }

        DateTime LastModified { get; set; }

    }
}
