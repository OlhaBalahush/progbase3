using System;
public class Comment
{
    public long id;
    public string comment;
    public DateTime createdAt;
    public long userId;
    public long postId;
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