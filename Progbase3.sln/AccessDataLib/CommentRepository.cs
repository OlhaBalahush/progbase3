using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;
namespace AccessDataLib
{
    public class CommentRepository
    {
        private SqliteConnection connection;
        private int numberOfElementsOnPage = 5;
        public CommentRepository(SqliteConnection connection)
        {
            this.connection = connection;
        }
        // методи знаходження айді юзера, що створив коментар, та айді поста, до якого створено коментар
        public long UserID(long commentID)
        {
            connection.Open();
    
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM user_comment WHERE id_comment = $idComment";
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
            command.CommandText = @"SELECT * FROM post_comment WHERE id_comment = $idComment";
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
        // методи для створення пагінованого списку
        private long GetCount()
        {
            connection.Open();
        
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM comments";
            
            long count = (long)command.ExecuteScalar();
            return count;
        }
        public int NumberOfPages(List<Comment> commentsToPost, string searchValue, int numberOfElementsOnPage)
        {
            this.numberOfElementsOnPage = numberOfElementsOnPage;
            int count = 0;
            if(searchValue != "")
            {
                List<Comment> allusers = this.GetSearchValue(commentsToPost, searchValue, numberOfElementsOnPage);
                foreach (Comment item in allusers)
                {
                    count++;
                }
            }
            else
            {
                if(commentsToPost == null)
                {
                    count = (int)GetCount();
                }
                else
                {
                    count = commentsToPost.Count;
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
        public List<Comment> PostsOnPage(List<Comment> commentsToPost, int numberOfPage)
        {
            if(commentsToPost == null)
            {
                if(numberOfPage != -1)
                {
                    connection.Open();

                    SqliteCommand command_1 = connection.CreateCommand();
                    command_1.CommandText = @"SELECT * FROM posts LIMIT $numberOfElementsOnPage OFFSET $fromElement";
                    command_1.Parameters.AddWithValue("$numberOfElementsOnPage", this.numberOfElementsOnPage);
                    command_1.Parameters.AddWithValue("$fromElement", (numberOfPage - 1) * this.numberOfElementsOnPage);

                    SqliteDataReader reader_1 = command_1.ExecuteReader();
                    List<Comment> comments_1 = new List<Comment>();

                    while(reader_1.Read())
                    {
                        string postText = reader_1.GetString(1);
                        DateTime createdAt = DateTime.Parse(reader_1.GetString(3));
                        Comment comment = new Comment(postText, createdAt.ToString());
                        comment.id = int.Parse(reader_1.GetString(0));
                        comment.pinned = reader_1.GetString(2);
                        comments_1.Add(comment);
                    }

                    reader_1.Close();
                    connection.Close();
                    return comments_1;
                }
                connection.Open();
                List<Comment> comments = new List<Comment>();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM posts";
                SqliteDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    string commentText = reader.GetString(1);
                    DateTime createdAt = DateTime.Parse(reader.GetString(3));
                    Comment comment = new Comment(commentText, createdAt.ToString());
                    comment.id = int.Parse(reader.GetString(0));
                    comment.pinned = reader.GetString(2);
                    comments.Add(comment);
                }

                reader.Close();
                connection.Close();
                return comments;
            }
            int index = 0;
            int counter = 0;
            List<Comment> page = new List<Comment>();
            foreach (Comment item in commentsToPost)
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
        public List<Comment> GetSearchPage(List<Comment> commentsToPost, string searchValue, int page, int numberOfElementsOnPage)
        {
            if(searchValue == "")
            {
                return this.PostsOnPage(commentsToPost, page);
            }
            return GetSearchValue(commentsToPost, searchValue, numberOfElementsOnPage);
        }
        public List<Comment> GetSearchValue(List<Comment> commentsToPost, string searchValue, int numberOfElementsOnPage) // якщо numberOfElementsOnPage = -1 потрібні усі елементи
        {
            List<Comment> comments = new List<Comment>();
            List<Comment> currentComments;
            if(numberOfElementsOnPage != -1)
            {
                for(int i = 1; i <= NumberOfPages(commentsToPost, "", numberOfElementsOnPage); i++)
                {
                    currentComments = PostsOnPage(commentsToPost, i);
                    foreach (Comment item in currentComments)
                    {
                        if(item.comment.Contains(searchValue))
                        {
                            comments.Add(item);
                        }
                    }
                }
                return comments;
            }
            for(int i = 1; i <= NumberOfPages(commentsToPost, "", numberOfElementsOnPage); i++)
            {
                currentComments = PostsOnPage(commentsToPost, -1);
                foreach (Comment item in currentComments)
                {
                    if(item.comment.Contains(searchValue))
                    {
                        comments.Add(item);
                    }
                }
            }
            return comments;
        }
        //CRUD
        public long Insert(Comment comment, Post post, User user)
        {
            connection.Open();
    
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = 
            @"
                INSERT INTO comments (comment, pinned, created_at) 
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
                INSERT INTO user_comment (id_user, id_comment) 
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
                INSERT INTO post_comment (id_post, id_comment) 
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
        public bool Delete(Comment comment, User user, Post post)
        {
            DeleteSubscriptionWithUser(comment, user);
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
            command.CommandText = @"DELETE FROM user_comment WHERE id_comment = $idComment";
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
            command.CommandText = @"DELETE FROM post_comment WHERE id_comment = $idComment";
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
    }
}