using System;
using System.Xml.Serialization;
using System.Xml;
namespace AccessDataLib
{
    public class Comment
    {
        [XmlAttribute()]
        public long id;
        [XmlElement("pin")]
        public string pinned;
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
            this.pinned = "";
            this.createdAt = DateTime.Parse(createdAt);
        }
        public override string ToString()
        {
            return $"{this.id}|{this.pinned}| - {this.comment} \n\tcreated at: [{this.createdAt.ToString()}]";
        }
    }
}