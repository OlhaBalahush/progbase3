using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
[XmlType(TypeName = "post")]
public class Post
{
    [XmlElement("id")]
    public long id;
    [XmlElement("text")]
    public string post;
    [XmlElement("createdAt")]
    public DateTime createdAt;
    //[XmlElement("userId")]
    public long userId;
    [XmlElement("comments")]
    public List<Comment> comments;
    public Post(string post, string createdAt)
    {
        this.post = post;
        this.createdAt = DateTime.Parse(createdAt);
        comments = new List<Comment>();
    }
    public override string ToString()
    {
        return $"post id = {id}|| {post} ||created at: [{createdAt.ToString()}]";
    }
}