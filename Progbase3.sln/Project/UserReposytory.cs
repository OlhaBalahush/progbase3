using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
public class UserReposytory
{
    private SqliteConnection connection;
    private int numberOfElementsOnPage = 5;
    public UserReposytory(SqliteConnection connection)
    {
        this.connection = connection;
    }
    public List<Comment> UserComments(long userID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        // command.CommandText = @"SELECT * FROM userComment WHERE idUser = $idUser";
        command.CommandText = @"SELECT userComment.idUser, comments.id, comments.comment, comments.createdAt 
        FROM userComment, comments WHERE userComment.idUser = $idUser
        AND comments.id = userComment.idComment";
        command.Parameters.AddWithValue("$idUser", userID);
        SqliteDataReader reader = command.ExecuteReader();

        List<Comment> comments = new List<Comment>();
        while (reader.Read())
        {
            string commenttext = reader.GetString(2);
            string createdAt = reader.GetString(3);
            Comment newcomment = new Comment(commenttext, createdAt);
            newcomment.id = int.Parse(reader.GetString(1));
            comments.Add(newcomment);
        }

        reader.Close();
        connection.Close();
        User user = GetByID(userID);
        user.comments = comments;
        return comments;
    }
    public List<Post> UserPosts(long userID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT userPost.idUser, posts.id, posts.post, posts.createdAt 
        FROM userPost, posts WHERE userPost.idUser = $idUser
        AND posts.id = userPost.idPost";
        command.Parameters.AddWithValue("$idUser", userID);
        SqliteDataReader reader = command.ExecuteReader();

        List<Post> posts = new List<Post>();
        while (reader.Read())
        {
            string postText = reader.GetString(2);
            string createdAt = reader.GetString(3);
            Post newpost = new Post(postText, createdAt);
            newpost.id = int.Parse(reader.GetString(1));
            posts.Add(newpost);
        }

        reader.Close();
        connection.Close();
        User user = GetByID(userID);
        user.posts = posts;
        return posts;
    }
    
    private long GetCount()
        {
            connection.Open();
        
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM users";
            
            long count = (long)command.ExecuteScalar();
            return count;
        }
    public int NumberOfPages(string searchValue, int numberOfElementsOnPage)
        {
            this.numberOfElementsOnPage = numberOfElementsOnPage;
            int count = 0;
            if(searchValue != "")
            {
                List<User> allusers = this.GetSearchValue(searchValue, numberOfElementsOnPage);
                foreach (User item in allusers)
                {
                    count++;
                }
            }
            else
            {
                count = (int)GetCount();
            }

            if(count % numberOfElementsOnPage == 0)
            {
                return count / numberOfElementsOnPage;
            }
            else
            {
                return count / numberOfElementsOnPage + 1;
            }
        }
    public List<User> UserssOnPage(int numberOfPage)
    {
        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM users LIMIT $numberOfElementsOnPage OFFSET $fromElement";
        command.Parameters.AddWithValue("$numberOfElementsOnPage", this.numberOfElementsOnPage);
        command.Parameters.AddWithValue("$fromElement", (numberOfPage - 1) * this.numberOfElementsOnPage);

        SqliteDataReader reader = command.ExecuteReader();
        List<User> users = new List<User>();

        while(reader.Read())
        {
            string username = reader.GetString(1);
            int moderator = int.Parse(reader.GetString(2));
            string password = reader.GetString(4);
            DateTime createdAt = DateTime.Parse(reader.GetString(3));
            User user = new User(username, moderator, password, createdAt.ToString());
            user.id = int.Parse(reader.GetString(0));
            users.Add(user);
        }

        reader.Close();
        connection.Close();
        return users;
    }
    public List<User> GetSearchPage(string searchValue, int page, int numberOfElementsOnPage)
    {
        if(searchValue == "")
        {
            return this.UserssOnPage(page);
        }
        return GetSearchValue(searchValue, numberOfElementsOnPage);
    }
    private List<User> GetSearchValue(string searchValue, int numberOfElementsOnPage)
    {
        List<User> concerts = new List<User>();
        for(int i = 1; i <= NumberOfPages("", numberOfElementsOnPage); i++)
        {
            List<User> currentConcerts = UserssOnPage(i);
            foreach (User item in currentConcerts)
            {
                if(item.username.Contains(searchValue))
                {
                    concerts.Add(item);
                }
            }
        }
        return concerts;
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