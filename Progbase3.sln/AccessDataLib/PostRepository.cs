using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
namespace AccessDataLib
{
    public class PostRepository
    {
        private SqliteConnection connection;
        private int numberOfElementsOnPage = 5;
        public PostRepository(SqliteConnection connection)
        {
            this.connection = connection;
        }
        // методи знаходження айді юзера, що створив пост, та коментарів
        public User User(long postID)
        {
            connection.Open();
    
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT user_post.id_post, users.id, users.username, users.moderator, users.created_at , users.password
            FROM user_post, users WHERE user_post.id_post = $idPost
            AND user_post.id_user = users.id";
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
        public List<Comment> CommentsOfPost(long postID, CommentRepository commentReposytory)
        {
            connection.Open();
    
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT post_comment.id_post, comments.id, comments.comment, comments.pinned, comments.created_at 
            FROM post_comment, comments WHERE post_comment.id_post = $idPost 
            AND comments.id = post_comment.id_comment";
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
        // методи для створення пагінованого списку
        private long GetCount()
        {
            connection.Open();
        
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM posts";
            
            long count = (long)command.ExecuteScalar();
            return count;
        }
        public int NumberOfPages(List<Post> postsOfUser, string searchValue, int numberOfElementsOnPage)
        {
            this.numberOfElementsOnPage = numberOfElementsOnPage;
            int count = 0;
            if(searchValue != "")
            {
                List<Post> allusers = this.GetSearchValue(postsOfUser, searchValue, numberOfElementsOnPage);
                foreach (Post item in allusers)
                {
                    count++;
                }
            }
            else
            {
                if(postsOfUser == null)
                {
                    count = (int)GetCount();
                }
                else
                {
                    count = postsOfUser.Count;
                }
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
        public List<Post> PostsOnPage(List<Post> postsOfUser, int numberOfPage)
        {
            if(postsOfUser == null)
            {
                if(numberOfPage != -1)
                {
                    connection.Open();

                    SqliteCommand command_1 = connection.CreateCommand();
                    command_1.CommandText = @"SELECT * FROM posts LIMIT $numberOfElementsOnPage OFFSET $fromElement";
                    command_1.Parameters.AddWithValue("$numberOfElementsOnPage", this.numberOfElementsOnPage);
                    command_1.Parameters.AddWithValue("$fromElement", (numberOfPage - 1) * this.numberOfElementsOnPage);

                    SqliteDataReader reader_1 = command_1.ExecuteReader();
                    List<Post> posts_1 = new List<Post>();

                    while(reader_1.Read())
                    {
                        string postText = reader_1.GetString(1);
                        DateTime createdAt = DateTime.Parse(reader_1.GetString(3));
                        Post post = new Post(postText, createdAt.ToString());
                        post.id = int.Parse(reader_1.GetString(0));
                        post.pinnedCommetId = int.Parse(reader_1.GetString(2));
                        posts_1.Add(post);
                    }

                    reader_1.Close();
                    connection.Close();
                    return posts_1;
                }
                connection.Open();
                List<Post> posts = new List<Post>();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM posts";
                SqliteDataReader reader = command.ExecuteReader();
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
            int index = 0;
            int counter = 0;
            List<Post> page = new List<Post>();
            foreach (Post item in postsOfUser)
            {
                if(index >= numberOfPage * numberOfElementsOnPage)
                {
                    page.Add(item);
                    counter++;
                    if(counter == numberOfElementsOnPage)
                    {
                        break;
                    }
                }
                index++;
            }
            return page;
        }
        public List<Post> GetSearchPage(List<Post> postsOfUser, string searchValue, int page, int numberOfElementsOnPage)
        {
            if(searchValue == "")
            {
                return this.PostsOnPage(postsOfUser, page);
            }
            return GetSearchValue(postsOfUser, searchValue, numberOfElementsOnPage);
        }
        public List<Post> GetSearchValue(List<Post> postsOfUser, string searchValue, int numberOfElementsOnPage) // якщо numberOfElementsOnPage = -1 потрібні усі елементи
        {
            List<Post> posts = new List<Post>();
            List<Post> currentPosts;
            if(numberOfElementsOnPage != -1)
            {
                for(int i = 1; i <= NumberOfPages(postsOfUser, "", numberOfElementsOnPage); i++)
                {
                    currentPosts = PostsOnPage(postsOfUser, i);
                    foreach (Post item in currentPosts)
                    {
                        if(item.post.Contains(searchValue))
                        {
                            posts.Add(item);
                        }
                    }
                }
                return posts;
            }
            for(int i = 1; i <= NumberOfPages(postsOfUser, "", numberOfElementsOnPage); i++)
            {
                currentPosts = PostsOnPage(postsOfUser, -1);
                foreach (Post item in currentPosts)
                {
                    if(item.post.Contains(searchValue))
                    {
                        posts.Add(item);
                    }
                }
            }
            return posts;
        }
        // метод для генерації звіту
        public Post PostWithTheMostCommentsDuringThisPeriod(List<Post> posts, CommentRepository commentReposytory)
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
                INSERT INTO posts (post, pinned_commet, created_at) 
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
                INSERT INTO user_post (id_user, id_post) 
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
        public bool Delete(Post post, User user)
        {
            DeleteSubscription(post, user);
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
            command.CommandText = @"DELETE FROM user_post WHERE id_post = $idPost";
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
    }
}