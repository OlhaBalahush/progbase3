using System;
using Microsoft.Data.Sqlite;
using AccessDataLib;
using static System.Console;
namespace GenerationDataProject
{
    class Program
    {
        static void Main(string[] args) //????????
        {
            string databaseGeneratorFileName = "C:/Users/Olha/Desktop/progbase3/data/generator/generator.db";
            SqliteConnection generatorconnection = new SqliteConnection($"Data Source={databaseGeneratorFileName}");

            Generator g = new Generator(generatorconnection);
            ForegroundColor = ConsoleColor.Blue;
            WriteLine("generation command:\n\tgenerate {number of users} users\n\tgenerate {number of posts} posts by user {user id}\n\tgenerate {number of commants} comments to post {post id} by user {user id}");
            ForegroundColor = ConsoleColor.Gray;
            Generator generator = new Generator(generatorconnection);

            string databaseFileName = "C:/Users/Olha/Desktop/progbase3/data/socialNetwork.db";
            SqliteConnection connection = new SqliteConnection($"Data Source={databaseFileName}");

            UserReposytory userReposytory = new UserReposytory(connection);
            PostReposytory postReposytory = new PostReposytory(connection);
            CommentReposytory commentReposytory = new CommentReposytory(connection);

            while(true)
            {
                Write("do you want to generate random data?(y/n) ");
                string answer = ReadLine();
                if(answer == "y")
                {
                    string command;
                    do
                    {
                        Write("enter generation command: ");
                        command = ReadLine();
                        string[] com = command.Split(" ");
                        //generate 10 users
                        if(command == "")
                        {
                            break;
                        }
                        else if(com.Length == 3 && com[0] == "generate" && IsNumber(com[1]) && com[2] == "users")
                        {
                            for(int i = 0; i < int.Parse(com[1]);i++)
                            {
                                User user = generator.GeneratorUser();
                                Authentication.SignUp(userReposytory, user);
                            }
                        }
                        // generate 5 posts by user 3
                        else if(com.Length == 6 && com[0] == "generate" && IsNumber(com[1]) && com[2] == "posts" && com[3] == "by" && com[4] == "user" && IsNumber(com[5]))
                        {
                            User user = userReposytory.GetByID(int.Parse(com[5]));
                            if(user == null)
                            {
                                Console.Error.WriteLine("incoorect entered user id");
                                return;
                            }
                            for(int i = 0; i < int.Parse(com[1]); i++)
                            {
                                Post post = generator.GeneratorPost();;
                                long postId = postReposytory.Insert(post, user);
                            }
                        }
                        // generate 5 comments to post 2 by user 3
                        else if(com.Length == 9 && com[0] == "generate" && IsNumber(com[1]) && com[2] == "comments" && com[3] == "to" && com[4] == "post" && IsNumber(com[5]) && com[6] == "by" && com[7] == "user" && IsNumber(com[8]))
                        {
                            Post post = postReposytory.GetByID(int.Parse(com[5]));
                            User user = userReposytory.GetByID(int.Parse(com[8]));
                            if(post == null)
                            {
                                Console.Error.WriteLine("incoorect entered post id");
                                return;
                            }
                            if(user == null)
                            {
                                Console.Error.WriteLine("incoorect entered user id");
                                return;
                            }
                            for(int i = 0; i < int.Parse(com[1]); i++)
                            {
                                Comment comment = generator.GeneratorComment();
                                long commentId = commentReposytory.Insert(comment, post, user);
                            }
                        }
                        else
                        {
                            Error.WriteLine("generation command was entered incorrectly");
                        }
                    }while(true);
                    break;
                }
                else if(answer == "n")
                {
                    break;
                }
                else
                {
                    Error.WriteLine("enter 'y' or 'n'");
                }
            }
        }
        static bool IsNumber(string value)
        {
            char[] charArr = value.ToCharArray();
            int i = 0;
            if(charArr[0] == 45)
            {
                i = 1;
            }
            while (i < charArr.Length)
            {
                if(char.IsDigit(charArr[i]) == false)
                {
                    return false;
                }
                i++;
            }
            return true;
        }
    }
    public class Generator
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
            string password = GeneratePassword();
            string createdtAt = GeneratorCreatedtAt();
            User user = new User(username, password, createdtAt);
            user.moderator = false;
            return user;
        }
        private string GeneratePassword()
        {
            Random rand = new Random();
            string password = "";
            int length = rand.Next(5,12);
            for(int i = 0; i < length;i++)
            {
                char c = (char)rand.Next(48,122);
                password += c.ToString();
            }
            return password;
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