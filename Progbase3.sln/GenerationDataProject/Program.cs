using System;
using Microsoft.Data.Sqlite;
using AccessDataLib;
namespace GenerationDataProject
{
    class Program
    {
        static void Main(string[] args) //????????
        {
        }
    }
    class Generator
    {
        private Random rand = new Random();
        private SqliteConnection connection;
        public void Main(){}
        public Generator(SqliteConnection connection)
        {
            this.connection = connection;
        }
        public Post GeneratorPost()
        {
            string postText = GeneratorText();
            string createdtAt = GeneratorCreatedtAt();
            Post post = new Post(postText, createdtAt);
            return post;
        }
        public Comment GeneratorComment()
        {
            string commentText = GeneratorText();
            string createdtAt = GeneratorCreatedtAt();
            Comment comment = new Comment(commentText, createdtAt);
            return comment;
        }
        public User GeneratorUser()
        {
            string username = GeneratorUsername();
            int moderator = rand.Next(0,2);
            string password = "hfdgjfg"; //генерувати паролі???
            string createdtAt = GeneratorCreatedtAt();
            User user = new User(username, moderator, password, createdtAt);
            return user;
        }
        private string GeneratorCreatedtAt()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;           
            return start.AddDays(rand.Next(range)).ToString();
        }
        private string GeneratorUsername()
        {
            connection.Open();

            int numOfWords = rand.Next(10,50); // задаються користувачем??
            string username = "";
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM usernames WHERE id = $id";
            command.Parameters.AddWithValue("$id", rand.Next(1,159)); //
            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                username = reader.GetString(1);
            }
            reader.Close();
            connection.Close();
            if(username == "") //??
            {
                username = GeneratorUsername();
            }
            return username;
        }
        private string GeneratorText()
        {
            connection.Open();

            int numOfWords = rand.Next(10,50); // задаються користувачем??
            string text = "";
            for(int i = 0; i < numOfWords; i++)
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM words WHERE id = $id";
                command.Parameters.AddWithValue("$id", rand.Next(1,159)); //
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    text += reader.GetString(1);
                }
                if(i != numOfWords - 1)
                {
                    text += " ";
                }
                reader.Close();
            }
            connection.Close();
            return text;
        }
    }
}