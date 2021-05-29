using System;
using System.Collections.Generic;
public class Post
{
    public long id;
    public string post;
    public DateTime createdAt;
    public long userId;
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