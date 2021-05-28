using Microsoft.Data.Sqlite;
using System.Collections.Generic;
public class UserReposytory
{
    private SqliteConnection connection;
    public UserReposytory(SqliteConnection connection)
    {
        this.connection = connection;
    }
    public List<long> UserComments(long userID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM userComment WHERE idUser = $idUser";
        command.Parameters.AddWithValue("$idUser", userID);
        SqliteDataReader reader = command.ExecuteReader();

        List<long> commentIDS = new List<long>();
        while (reader.Read())
        {
            int commentId = int.Parse(reader.GetString(2));
            commentIDS.Add(commentId);
        }

        reader.Close();
        connection.Close();
        User user = GetByID(userID);
        user.comments = commentIDS;
        return commentIDS;
    }
    public List<long> UserPosts(long userID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM userPost WHERE idUser = $idUser";
        command.Parameters.AddWithValue("$idUser", userID);
        SqliteDataReader reader = command.ExecuteReader();

        List<long> postIDS = new List<long>();
        while (reader.Read())
        {
            int postId = int.Parse(reader.GetString(2));
            postIDS.Add(postId);
        }

        reader.Close();
        connection.Close();
        User user = GetByID(userID);
        user.posts = postIDS;
        return postIDS;
    }
    
    public User GetByUsername(string username)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM users WHERE username = $username";
        command.Parameters.AddWithValue("$username", username);
        
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            int moderator = int.Parse(reader.GetString(2));
            string createdAt = reader.GetString(3);
            string password = reader.GetString(4);
            User user = new User(username, moderator, password, createdAt);
            user.id = int.Parse(reader.GetString(0));
            reader.Close();
        
            connection.Close();
            return user;
        }
        reader.Close();
        
        connection.Close();
        return null;
    }    
    //CRUD
    public long Insert(User user)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = 
        @"
            INSERT INTO users (username, moderator, createdAt, password) 
            VALUES ($username, $moderator, $createdAt, $password);
        
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("$username", user.username);
        if(user.moderator == true)
        {
            command.Parameters.AddWithValue("$moderator", 1);
        }
        else
        {
            command.Parameters.AddWithValue("$moderator", 0);
        }
        command.Parameters.AddWithValue("$createdAt", user.createdAt);
        command.Parameters.AddWithValue("$password", user.password);
        
        long newId = (long)command.ExecuteScalar();
        user.id = newId;
        
        connection.Close();
        return newId;
    }
    public User GetByID(long userID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM users WHERE id = $id";
        command.Parameters.AddWithValue("$id", userID);
        
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            string username = reader.GetString(1);
            int moderator = int.Parse(reader.GetString(2));
            string createdAt = reader.GetString(3);
            string password = reader.GetString(4);
            User user = new User(username, moderator, password, createdAt);
            user.id = int.Parse(reader.GetString(0));
            reader.Close();
        
            connection.Close();
            return user;
        }
        reader.Close();
        
        connection.Close();
        return null;
    }
    public bool Update(long userID, User newuser)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE users SET username = $username WHERE id = $id";
        command.Parameters.AddWithValue("$id", userID);
        command.Parameters.AddWithValue("$username", newuser.username);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
    public bool Delete(long userID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM users WHERE id = $id";
        command.Parameters.AddWithValue("$id", userID);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
}