using Microsoft.Data.Sqlite;
using System;
public class CommentReposytory
{
    private SqliteConnection connection;
    public CommentReposytory(SqliteConnection connection)
    {
        this.connection = connection;
    }
    
    public long UserID(long commentID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM userComment WHERE idComment = $idComment";
        command.Parameters.AddWithValue("$idComment", commentID);
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            int userId = int.Parse(reader.GetString(1));
            reader.Close();
            connection.Close();
            Comment comment = GetByID(commentID);
            comment.userId = userId;
            return userId;
        }

        reader.Close();
        connection.Close();
        return -1;
    }
    public long PostID(long commentID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM postComment WHERE idComment = $idComment";
        command.Parameters.AddWithValue("$idComment", commentID);
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            int postId = int.Parse(reader.GetString(1));
            reader.Close();
            connection.Close();
            Comment comment = GetByID(commentID);
            comment.postId = postId;
            return postId;
        }

        reader.Close();
        connection.Close();
        return -1;
    }
    // public void DeleteAllCommentsToPost(long postId){}
    // public void DeleteAllCommentsByUser(long userId, User user)
    // {
    //     user.comments.Remove();
    // }
    //CRUD
    public long Insert(Comment comment, Post post, User user)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = 
        @"
            INSERT INTO comments (comment, createdAt) 
            VALUES ($comment, $createdAt);
        
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("$comment", comment.comment);
        command.Parameters.AddWithValue("$createdAt", comment.createdAt);
        
        long newId = (long)command.ExecuteScalar();
        comment.id = newId;
        
        connection.Close();
        InsertSubscriptionWithUser(comment, user);
        InsertSubscriptionWithPost(comment, post);
        return newId;
    }
    private long InsertSubscriptionWithUser(Comment comment, User user)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = 
        @"
            INSERT INTO userComment (idUser, idComment) 
            VALUES ($idUser, $idComment);
        
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("$idUser", user.id);
        command.Parameters.AddWithValue("$idComment", comment.id);
        
        long newId = (long)command.ExecuteScalar();
        
        user.comments.Add(comment.id);
        comment.userId = user.id;
        
        connection.Close();
        return newId;
    }
    private long InsertSubscriptionWithPost(Comment comment, Post post)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = 
        @"
            INSERT INTO postComment (idPost, idComment) 
            VALUES ($idPost, $idComment);
        
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("$idPost", post.id);
        command.Parameters.AddWithValue("$idComment", comment.id);
        
        long newId = (long)command.ExecuteScalar();
        post.commentIds.Add(comment.id);
        comment.postId = post.id;
        
        connection.Close();
        return newId;
    }
    public Comment GetByID(long commentID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM comments WHERE id = $id";
        command.Parameters.AddWithValue("$id", commentID);
        
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            string commentText = reader.GetString(1);
            string createdAt = reader.GetString(2);
            Comment comment = new Comment(commentText, createdAt);
            comment.id = int.Parse(reader.GetString(0));
            reader.Close();
        
            connection.Close();
            return comment;
        }
        reader.Close();
        
        connection.Close();
        return null;
    }
    public bool Update(long commentID, Comment newComment)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE comments SET comment = $comment WHERE id = $id";
        command.Parameters.AddWithValue("$id", commentID);
        command.Parameters.AddWithValue("$comment", newComment.comment);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
    public bool Delete(Comment comment, User user, Post post/*UserReposytory userReposytory, PostReposytory postReposytory*/)
    {
        //Comment comment = GetByID(commentID);
        //User user = userReposytory.GetByID(comment.userId);
        DeleteSubscriptionWithUser(comment, user);
        //Post post = postReposytory.GetByID(comment.postId);
        DeleteSubscriptionWithPost(comment, post);
        //
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM comments WHERE id = $id";
        command.Parameters.AddWithValue("$id", comment.id);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
    private bool DeleteSubscriptionWithUser(Comment comment, User user)
    {
        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM userComment WHERE id = $idComment";
        command.Parameters.AddWithValue("$idComment", comment.id);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        user.comments.Remove(comment.id);
        return true;
    }
    private bool DeleteSubscriptionWithPost(Comment comment, Post post)
    {
        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM postComment WHERE id = $idComment";
        command.Parameters.AddWithValue("$idComment", comment.id);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        post.commentIds.Remove(comment.id);
        return true;
    }
    public int NumberOfCommentsFromTo(DateTime startDate, DateTime endDate)
    {
        connection.Open();
            
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM comments";

        SqliteDataReader reader = command.ExecuteReader();
        int counter = 0;
        while (reader.Read())
        {
            DateTime createdAt = DateTime.Parse(reader.GetString(2));
            if(createdAt >= startDate && createdAt <= endDate)
            {
                counter++;
            }
        }
        connection.Close();
        return counter;
    }
}