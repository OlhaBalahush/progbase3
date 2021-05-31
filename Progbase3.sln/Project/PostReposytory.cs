using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
public class PostReposytory
{
    private SqliteConnection connection;
    private CommentReposytory commentReposytory;
    private int numberOfElementsOnPage = 5;
    public PostReposytory(SqliteConnection connection)
    {
        this.connection = connection;
    }
    public User User(long postID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT userPost.idPost, users.id, users.username, users.moderator, users.createdAt , users.password
        FROM userPost, users WHERE userPost.idPost = $idPost
        AND userPost.idUser = users.id";
        command.Parameters.AddWithValue("$idPost", postID);
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            string username = reader.GetString(2);
            int moderator = int.Parse(reader.GetString(3));
            string createdAt = reader.GetString(4);
            string password = reader.GetString(5);
            User user = new User(username, moderator, password, createdAt);
            user.id = int.Parse(reader.GetString(1));
            reader.Close();
            connection.Close();
            Post post = GetByID(postID);
            post.user = user;
            return user;
        }

        reader.Close();
        connection.Close();
        return null;
    }
    public List<Comment> CommentsOfPost(long postID, CommentReposytory commentReposytory)
    {
        this.commentReposytory = commentReposytory;
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT postComment.idPost, comments.id, comments.comment, comments.createdAt 
        FROM postComment, comments WHERE postComment.idPost = $idPost 
        AND comments.id = postComment.idComment";
        command.Parameters.AddWithValue("$idPost", postID);
        SqliteDataReader reader = command.ExecuteReader();

        List<Comment> comments = new List<Comment>();
        while (reader.Read())
        {
            string commenttext = reader.GetString(2);
            string createdAt = reader.GetString(3);
            Comment newcomment = new Comment(commenttext, createdAt);
            newcomment.id= long.Parse(reader.GetString(1));
            newcomment.postId = postID;
            comments.Add(newcomment);
        }

        reader.Close();
        connection.Close();
        Post post = GetByID(postID);
        foreach (Comment item in comments)
        {
            item.userId = commentReposytory.UserID(item.id);
        }
        post.comments = comments;
        return comments;
    }
    private long GetCount()
        {
            connection.Open();
        
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM posts";
            
            long count = (long)command.ExecuteScalar();
            return count;
        }
    public int NumberOfPages(string searchValue, int numberOfElementsOnPage)
        {
            this.numberOfElementsOnPage = numberOfElementsOnPage;
            int count = 0;
            if(searchValue != "")
            {
                List<Post> allusers = this.GetSearchValue(searchValue, numberOfElementsOnPage);
                foreach (Post item in allusers)
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
    public List<Post> PostsOnPage(int numberOfPage)
    {
        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM posts LIMIT $numberOfElementsOnPage OFFSET $fromElement";
        command.Parameters.AddWithValue("$numberOfElementsOnPage", this.numberOfElementsOnPage);
        command.Parameters.AddWithValue("$fromElement", (numberOfPage - 1) * this.numberOfElementsOnPage);

        SqliteDataReader reader = command.ExecuteReader();
        List<Post> posts = new List<Post>();

        while(reader.Read())
        {
            string postText = reader.GetString(1);
            DateTime createdAt = DateTime.Parse(reader.GetString(2));
            Post post = new Post(postText, createdAt.ToString());
            post.id = int.Parse(reader.GetString(0));
            posts.Add(post);
        }

        reader.Close();
        connection.Close();
        return posts;
    }
    public List<Post> GetSearchPage(string searchValue, int page, int numberOfElementsOnPage)
    {
        if(searchValue == "")
        {
            return this.PostsOnPage(page);
        }
        return GetSearchValue(searchValue, numberOfElementsOnPage);
    }
    public List<Post> GetSearchValue(string searchValue, int numberOfElementsOnPage)
    {
        List<Post> concerts = new List<Post>();
        for(int i = 1; i <= NumberOfPages("", numberOfElementsOnPage); i++)
        {
            List<Post> currentConcerts = PostsOnPage(i);
            foreach (Post item in currentConcerts)
            {
                if(item.post.Contains(searchValue))
                {
                    concerts.Add(item);
                }
            }
        }
        return concerts;
    }
    //треба щось змінити, хз ще що
    public List<Post> GetSearchValue_1(string searchValue)
    {
        connection.Open();
        List<Post> posts = new List<Post>();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM posts";
        SqliteDataReader reader = command.ExecuteReader();
        while(reader.Read())
        {
            string postText = reader.GetString(1);
            DateTime createdAt = DateTime.Parse(reader.GetString(2));
            Post post = new Post(postText, createdAt.ToString());
            post.id = int.Parse(reader.GetString(0));
            if(post.post.Contains(searchValue))
            {
                posts.Add(post);
            }
        }

        reader.Close();
        connection.Close();
        return posts;
    }

    //CRUD
    public long Insert(Post post, User user)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = 
        @"
            INSERT INTO posts (post, createdAt) 
            VALUES ($post, $createdAt);
        
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("$post", post.post);
        command.Parameters.AddWithValue("$createdAt", post.createdAt);
        
        long newId = (long)command.ExecuteScalar();
        post.id = newId;        
        connection.Close();

        InsertSubscription(post, user);

        return newId;
    }
    private long InsertSubscription(Post post, User user)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = 
        @"
            INSERT INTO userPost (idUser, idPost) 
            VALUES ($idUser, $idPost);
        
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("$idUser", user.id);
        command.Parameters.AddWithValue("$idPost", post.id);
        
        long newId = (long)command.ExecuteScalar();
        post.user = user;
        user.posts.Add(post);
        
        connection.Close();
        return newId;
    }
    public Post GetByID(long postID)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM posts WHERE id = $id";
        command.Parameters.AddWithValue("$id", postID);
        
        SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            string postText = reader.GetString(1);
            string createdAt = reader.GetString(2);
            Post post = new Post(postText, createdAt);
            post.id = int.Parse(reader.GetString(0));
            reader.Close();
        
            connection.Close();
            return post;
        }
        reader.Close();
        
        connection.Close();
        return null;
    }
    public bool Update(long postID, Post newPost)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE posts SET post = $post WHERE id = $id";
        command.Parameters.AddWithValue("$id", postID);
        command.Parameters.AddWithValue("$post", newPost.post);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
    public bool Delete(Post post, User user/*,CommentReposytory commentReposytory*/)
    {
        // delete all comments:
        //Post post = GetByID(postID);
        // List<long> comments = post.commentIds;
        // foreach (long item in comments)
        // {
        //     Comment comment = commentReposytory.GetByID(item);
        //     commentReposytory.Delete(comment, user, post);
        // }
        DeleteSubscription(post, user);
        //DeleteAllCommentsToPost(post);
        //
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM posts WHERE id = $id";
        command.Parameters.AddWithValue("$id", post.id);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
    private bool DeleteSubscription(Post post, User user)
    {
        long postId = post.id;
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM userPost WHERE idPost = $idPost";
        command.Parameters.AddWithValue("$idPost", post.id);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        //user.posts.Remove(postId);
        return true;
    }
    public List<Post> NumberOfPostsFromTo(DateTime startDate, DateTime endDate)
    {
        connection.Open();
            
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM posts";

        SqliteDataReader reader = command.ExecuteReader();
        List<Post> posts = new List<Post>();
        while (reader.Read())
        {
            DateTime createdat = DateTime.Parse(reader.GetString(2));
            if(createdat >= startDate && createdat <= endDate)
            {
                string postText = reader.GetString(1);
                string createdAt = reader.GetString(2);
                Post post = new Post(postText, createdAt);
                long id = long.Parse(reader.GetString(0));
                post.id = id;
                posts.Add(post);
            }
        }
        connection.Close();
        return posts;
    }
    public Post PostWithTheMostCommentsDuringThisPeriod(DateTime startDate, DateTime endDate)
    {
        List<Post> posts = this.NumberOfPostsFromTo(startDate, endDate);
        int maxValue = 0;
        Post post = null;
        foreach (Post item in posts)
        {
            List<Comment> comments = this.CommentsOfPost(item.id, this.commentReposytory);
            int numOfComments = comments.Count;
            if(numOfComments != 0 && numOfComments >= maxValue)
            {
                maxValue = numOfComments;
                post = item;
            }
        }
        return post;
    }

    /*private bool DeleteAllCommentsToPost(Post post)
    {
        connection.Open();
 
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM postComment WHERE idPost = $idPost";
        command.Parameters.AddWithValue("$idPost", post.id);
        
        int nChanged = command.ExecuteNonQuery();
        
        if (nChanged == 0)
        {
            connection.Close();
            return false;
        }
        connection.Close();
        return true;
    }
    */
}