using System;
using System.Collections.Generic;
namespace AccessDataLib
{
    public class User
    {
        public long id;
        public string username;
        public bool moderator; // 1 - true, 0 - false
        public List<Comment> comments;
        public List<Post> posts;
        public string password;
        public DateTime createdAt;
        public User(){}
        public User(string username, string password, string createdAt)
        {
            this.username = username;
            this.password = password;
            this.createdAt = DateTime.Parse(createdAt);
            comments = new List<Comment>();
            posts = new List<Post>();
        }
        public override string ToString()
        {
            return $"{id} - '{username}' \n\tcreated at: [{createdAt.ToString()}]";
        }
    }
}