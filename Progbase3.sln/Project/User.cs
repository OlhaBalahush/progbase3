using System;
using System.Collections.Generic;
public class User
{
    public long id;
    public string username;
    public bool moderator; // 1 - true, 0 - false
    public List<long> comments;
    public List<long> posts;
    public string password;
    public DateTime createdAt;
    public User(string username, int moderator, string password, string createdAt)
    {
        this.username = username;
        this.password = password;
        if(moderator == 0)
        {
            this.moderator = false;
        }
        else
        {
            this.moderator = true;
        }
        this.createdAt = DateTime.Parse(createdAt);
        comments = new List<long>();
        posts = new List<long>();
    }
    public override string ToString()
    {
        return $"{id} - '{username}' \n\tcreated at: [{createdAt.ToString()}]";
    }
}