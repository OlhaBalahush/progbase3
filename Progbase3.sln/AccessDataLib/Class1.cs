using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Data.Sqlite;
using System.Xml;
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
        public User(string username/*, int moderator*/, string password, string createdAt)
        {
            this.username = username;
            this.password = password;
            // if(moderator == 0)
            // {
            //     this.moderator = false;
            // }
            // else
            // {
            //     this.moderator = true;
            // }
            this.createdAt = DateTime.Parse(createdAt);
            comments = new List<Comment>();
            posts = new List<Post>();
        }
        public override string ToString()
        {
            return $"{id} - '{username}' \n\tcreated at: [{createdAt.ToString()}]";
        }
    }
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
            command.CommandText = @"SELECT userComment.idUser, comments.id, comments.pinned, comments.comment, comments.createdAt 
            FROM userComment, comments WHERE userComment.idUser = $idUser
            AND comments.id = userComment.idComment";
            command.Parameters.AddWithValue("$idUser", userID);
            SqliteDataReader reader = command.ExecuteReader();

            List<Comment> comments = new List<Comment>();
            while (reader.Read())
            {
                string commenttext = reader.GetString(2);
                string createdAt = reader.GetString(4);
                Comment newcomment = new Comment(commenttext, createdAt);
                newcomment.id = int.Parse(reader.GetString(1));
                newcomment.pinned = reader.GetString(3);
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
        public List<Post> NumberOfPostsFromTo(long userId, DateTime startDate, DateTime endDate)
        {
            List<Post> currentposts = this.UserPosts(userId);
            List<Post> posts = new List<Post>();
            foreach (Post item in currentposts)
            {
                if(item.createdAt >= startDate && item.createdAt < endDate)
                {
                    posts.Add(item);
                }
            }
            return posts;
        }
        public List<Comment> NumberOfCommentsFromTo(long userId, DateTime startDate, DateTime endDate)
        {
            List<Comment> currentcomments = this.UserComments(userId);
            List<Comment> comments = new List<Comment>();
            foreach (Comment item in currentcomments)
            {
                if(item.createdAt >= startDate && item.createdAt < endDate)
                {
                    comments.Add(item);
                }
            }
            return comments;
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
                // int moderator = int.Parse(reader.GetString(2));
                string password = reader.GetString(4);
                DateTime createdAt = DateTime.Parse(reader.GetString(3));
                User user = new User(username, password, createdAt.ToString());
                user.id = int.Parse(reader.GetString(0));
                if(int.Parse(reader.GetString(2)) == 0)
                {
                    user.moderator = false;
                }
                else
                {
                    user.moderator = true;
                }
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
                // /int moderator = int.Parse(reader.GetString(2));
                string createdAt = reader.GetString(3);
                string password = reader.GetString(4);
                User user = new User(username, password, createdAt);
                user.id = int.Parse(reader.GetString(0));
                if(int.Parse(reader.GetString(2)) == 0)
                {
                    user.moderator = false;
                }
                else
                {
                    user.moderator = true;
                }
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
                User user = new User(username, password, createdAt);
                user.id = int.Parse(reader.GetString(0));
                if(int.Parse(reader.GetString(2)) == 0)
                {
                    user.moderator = false;
                }
                else
                {
                    user.moderator = true;
                }
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
            command.CommandText = @"UPDATE users SET username = $username, moderator = $moderator WHERE id = $id";
            command.Parameters.AddWithValue("$id", userID);
            command.Parameters.AddWithValue("$username", newuser.username);
            if(newuser.moderator == true)
            {
                command.Parameters.AddWithValue("$moderator", 1);
            }
            else
            {
                command.Parameters.AddWithValue("$moderator", 0);
            }
            
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
    [XmlType(TypeName = "post")]
    public class Post
    {
        [XmlAttribute()]
        public long id;
        [XmlElement("text")]
        public string post;
        [XmlElement("pinned comment")]
        public long pinnedCommetId;
        [XmlElement("createdAt")]
        public DateTime createdAt;
        //[XmlElement("userId")]
        public User user;
        //[XmlElement("comments")]
        public List<Comment> comments;
        public Post(){}
        public Post(string post, string createdAt)
        {
            this.post = post;
            this.createdAt = DateTime.Parse(createdAt);
            this.pinnedCommetId = -1;
            this.comments = new List<Comment>();
        }
        public override string ToString()
        {
            return $"post id = {id}|| {post} ||created at: [{createdAt.ToString()}]";
        }
    }
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
                //int moderator = int.Parse(reader.GetString(3));
                string createdAt = reader.GetString(4);
                string password = reader.GetString(5);
                User user = new User(username, password, createdAt);
                user.id = int.Parse(reader.GetString(1));
                if(int.Parse(reader.GetString(3)) == 0)
                {
                    user.moderator = false;
                }
                else
                {
                    user.moderator = true;
                }
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
            command.CommandText = @"SELECT postComment.idPost, comments.id, comments.comment, comments.pinned, comments.createdAt 
            FROM postComment, comments WHERE postComment.idPost = $idPost 
            AND comments.id = postComment.idComment";
            command.Parameters.AddWithValue("$idPost", postID);
            SqliteDataReader reader = command.ExecuteReader();

            List<Comment> comments = new List<Comment>();
            while (reader.Read())
            {
                string commenttext = reader.GetString(2);
                string createdAt = reader.GetString(4);
                Comment newcomment = new Comment(commenttext, createdAt);
                newcomment.id= long.Parse(reader.GetString(1));
                newcomment.postId = postID;
                newcomment.pinned = reader.GetString(3);
                comments.Add(newcomment);
            }

            reader.Close();
            connection.Close();
            Post post = GetByID(postID);
            if(comments.Count != 0)
            {
                foreach (Comment item in comments)
                {
                    long userId = commentReposytory.UserID(item.id);
                    if(userId == -1)
                    {
                        comments.Remove(item);
                    }
                    else
                    {
                        item.userId = commentReposytory.UserID(item.id);
                    }
                }
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
                DateTime createdAt = DateTime.Parse(reader.GetString(3));
                Post post = new Post(postText, createdAt.ToString());
                post.id = int.Parse(reader.GetString(0));
                post.pinnedCommetId = int.Parse(reader.GetString(2));
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
        public Post PostWithTheMostCommentsDuringThisPeriod(List<Post> posts, CommentReposytory commentReposytory)
        {
            int maxValue = 0;
            Post post = null;
            foreach (Post item in posts)
            {
                List<Comment> comments = this.CommentsOfPost(item.id, commentReposytory);
                int numOfComments = comments.Count;
                if(numOfComments != 0 && numOfComments >= maxValue)
                {
                    maxValue = numOfComments;
                    post = item;
                }
            }
            return post;
        }

        //CRUD
        public long Insert(Post post, User user)
        {
            connection.Open();
    
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = 
            @"
                INSERT INTO posts (post, pinned_commet, createdAt) 
                VALUES ($post, $pinned_commet, $createdAt);
            
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$post", post.post);
            command.Parameters.AddWithValue("$createdAt", post.createdAt);
            command.Parameters.AddWithValue("$pinned_commet", post.pinnedCommetId);
            
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
                string createdAt = reader.GetString(3);
                Post post = new Post(postText, createdAt);
                post.id = int.Parse(reader.GetString(0));
                post.pinnedCommetId = int.Parse(reader.GetString(2));
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
            command.CommandText = @"UPDATE posts SET post = $post, pinned_commet = $pinned_commet WHERE id = $id";
            command.Parameters.AddWithValue("$id", postID);
            command.Parameters.AddWithValue("$post", newPost.post);
            command.Parameters.AddWithValue("$pinned_commet", newPost.pinnedCommetId);
            
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
        // public List<Post> NumberOfPostsFromTo(DateTime startDate, DateTime endDate)
        // {
        //     connection.Open();
                
        //     SqliteCommand command = connection.CreateCommand();
        //     command.CommandText = @"SELECT * FROM posts";

        //     SqliteDataReader reader = command.ExecuteReader();
        //     List<Post> posts = new List<Post>();
        //     while (reader.Read())
        //     {
        //         DateTime createdat = DateTime.Parse(reader.GetString(2));
        //         if(createdat >= startDate && createdat <= endDate)
        //         {
        //             string postText = reader.GetString(1);
        //             string createdAt = reader.GetString(2);
        //             Post post = new Post(postText, createdAt);
        //             long id = long.Parse(reader.GetString(0));
        //             post.id = id;
        //             posts.Add(post);
        //         }
        //     }
        //     connection.Close();
        //     return posts;
        // }
        // public Post PostWithTheMostCommentsDuringThisPeriod(DateTime startDate, DateTime endDate)
        // {
        //     List<Post> posts = this.NumberOfPostsFromTo(startDate, endDate);
        //     int maxValue = 0;
        //     Post post = null;
        //     foreach (Post item in posts)
        //     {
        //         List<Comment> comments = this.CommentsOfPost(item.id, this.commentReposytory);
        //         int numOfComments = comments.Count;
        //         if(numOfComments != 0 && numOfComments >= maxValue)
        //         {
        //             maxValue = numOfComments;
        //             post = item;
        //         }
        //     }
        //     return post;
        // }

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
    // [XmlType(TypeName = "comment")]
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
                reader.Close();
                connection.Close();
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
                INSERT INTO comments (comment, pinned, createdAt) 
                VALUES ($comment, $pinned, $createdAt);
            
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$comment", comment.comment);
            command.Parameters.AddWithValue("$pinned", comment.pinned);
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
            
            user.comments.Add(comment);
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
            post.comments.Add(comment);
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
                string createdAt = reader.GetString(3);
                Comment comment = new Comment(commentText, createdAt);
                comment.id = int.Parse(reader.GetString(0));
                comment.pinned = reader.GetString(2);
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
            command.CommandText = @"UPDATE comments SET comment = $comment, pinned = $pinned WHERE id = $id";
            command.Parameters.AddWithValue("$id", commentID);
            command.Parameters.AddWithValue("$comment", newComment.comment);
            command.Parameters.AddWithValue("$pinned", newComment.pinned);
            
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
            user.comments.Remove(comment);
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
            post.comments.Remove(comment);
            return true;
        }
        // public int NumberOfCommentsFromTo(DateTime startDate, DateTime endDate)
        // {
        //     connection.Open();
                
        //     SqliteCommand command = connection.CreateCommand();
        //     command.CommandText = @"SELECT * FROM comments";

        //     SqliteDataReader reader = command.ExecuteReader();
        //     int counter = 0;
        //     while (reader.Read())
        //     {
        //         DateTime createdAt = DateTime.Parse(reader.GetString(2));
        //         if(createdAt >= startDate && createdAt <= endDate)
        //         {
        //             counter++;
        //         }
        //     }
        //     connection.Close();
        //     return counter;
        // }
    }
}
//dsfkjhsd