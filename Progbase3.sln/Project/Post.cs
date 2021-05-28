using System;
using System.Collections.Generic;
public class Post
{
    public long id;
    public string post;
    public DateTime createdAt;
    public long userId;
    public List<long> commentIds;
    public Post(string post, string createdAt)
    {
        this.post = post;
        this.createdAt = DateTime.Parse(createdAt);
        commentIds = new List<long>();
    }
    public override string ToString()
    {
        return $"post id = {id}|| {post} ||created at: [{createdAt.ToString()}]";
    }
}