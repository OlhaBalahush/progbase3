using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
namespace AccessDataLib
{
    [XmlType(TypeName = "post")]
    public class Post
    {
        [XmlAttribute()]
        public long id;
        [XmlElement("text")]
        public string post;
        [XmlElement("pinned comment")]
        public long pinnedCommetId;
        [XmlElement("createdAt")]
        public DateTime createdAt;
        //[XmlElement("userId")]
        public User user;
        //[XmlElement("comments")]
        public List<Comment> comments;
        public Post(){}
        public Post(string post, string createdAt)
        {
            this.post = post;
            this.createdAt = DateTime.Parse(createdAt);
            this.pinnedCommetId = -1;
            this.comments = new List<Comment>();
        }
        public override string ToString()
        {
            return $"post id = {id}|| {post} ||created at: [{createdAt.ToString()}]";
        }
    }
}