using System;
using System.Xml.Serialization;
using System.Xml;
// [XmlType(TypeName = "comment")]
public class Comment
{
    [XmlAttribute()]
    public long id;
    [XmlElement("comment")]
    public string comment;
    [XmlElement("createdAt")]
    public DateTime createdAt;
    public long userId;
    public long postId;
    public Comment(){}
    public Comment(string comment, string createdAt)
    {
        this.comment = comment;
        this.createdAt = DateTime.Parse(createdAt);
    }
    public override string ToString()
    {
        return $"{id} - {comment} \n\tcreated at: [{createdAt.ToString()}]";
    }
}