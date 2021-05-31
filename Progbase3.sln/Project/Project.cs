using System;
using System.IO;
using Microsoft.Data.Sqlite;
using static System.Console;
using System.Collections.Generic;
using Terminal.Gui;
class Program
{
    //ВИДАЛЯТИ УСЕ ЗАПИСАНЕ КОРИСТУВАЧЕМ(КОМЕНТИ ТА ПОСТИ) ЧИ РОБИТИ МАРКЕР ВИДАЛЕНОГО КОРИСТУВАЧА?????
    static void Clean(UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory) // delete
    {
        for(int i = 0; i < 20; i++)
        {
            Post post = postReposytory.GetByID(i);
            WriteLine(postReposytory.Delete(post, post.user/*, commentReposytory*/ ));
            Comment comment = commentReposytory.GetByID(i);
            WriteLine(commentReposytory.Delete(comment, userReposytory.GetByID(comment.userId), postReposytory.GetByID(comment.postId)));
            
            WriteLine(userReposytory.Delete(i));
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
    static void Main(string[] args)
    {
        string databaseFileName = "C:/Users/Olha/Desktop/progbase3/data/socialNetwork.db";
        SqliteConnection connection = new SqliteConnection($"Data Source={databaseFileName}");

        UserReposytory userReposytory = new UserReposytory(connection);
        PostReposytory postReposytory = new PostReposytory(connection);
        CommentReposytory commentReposytory = new CommentReposytory(connection);

        Application.Init();
        Toplevel top = Application.Top;

        Authentication registration = new Authentication(top, userReposytory, postReposytory, commentReposytory);
        top.Add(registration);

        Application.Run();

        // Application.Init();

        // MainWindow window = new MainWindow(top);
        // window.SetReposytory(userReposytory, postReposytory, commentReposytory);
        // top.Add(window);

        // Application.Run();
        // DateTime startDate = new DateTime(2008, 3, 1, 7, 0, 0);  
        // DateTime endDate = DateTime.Now;
        // List<Post> posts = postReposytory.NumberOfPostsFromTo(startDate, endDate);
        // int num = posts.Count;
        // Console.WriteLine(num);
        // Post post = postReposytory.PostWithTheMostCommentsDuringThisPeriod(startDate, endDate);
        // if(post == null)
        // {
        //     Console.Error.WriteLine("all post hasn't comments");
        // }
        // else
        // {
        //     Console.WriteLine(post);
        // }
        // num = commentReposytory.NumberOfCommentsFromTo(startDate, endDate);
        // Console.WriteLine(num);

        // while(true)
        // {
        //     Write("do you want to generate random data?(y/n) ");
        //     string answer = ReadLine();
        //     if(answer == "y")
        //     {
        //         Generation(userReposytory, postReposytory, commentReposytory);
        //         break;
        //     }
        //     else if(answer == "n")
        //     {
        //         break;
        //     }
        //     else
        //     {
        //         Error.WriteLine("enter 'y' or 'n'");
        //     }
        // }
        //Clean(userReposytory, postReposytory, commentReposytory);
    }
    static void Generation(UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        string databaseGeneratorFileName = "C:/Users/Olha/Desktop/progbase3/data/generator/generator.db";
        SqliteConnection generatorconnection = new SqliteConnection($"Data Source={databaseGeneratorFileName}");
        
        Generator g = new Generator(generatorconnection);
        ForegroundColor = ConsoleColor.Blue;
        WriteLine("generation command:\n\tgenerate {number of users} users\n\tgenerate {number of posts} posts by user {user id}\n\tgenerate {number of commants} comments to post {post id} by user {user id}");
        ForegroundColor = ConsoleColor.Gray;

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
                GeneratorUsers(com, g, userReposytory);
            }
            // generate 5 posts by user 3
            else if(com.Length == 6 && com[0] == "generate" && IsNumber(com[1]) && com[2] == "posts" && com[3] == "by" && com[4] == "user" && IsNumber(com[5]))
            {
                GeneratorPostsByUser(com, g, userReposytory, postReposytory);
            }
            // generate 5 comments to post 2 by user 3
            else if(com.Length == 9 && com[0] == "generate" && IsNumber(com[1]) && com[2] == "comments" && com[3] == "to" && com[4] == "post" && IsNumber(com[5]) && com[6] == "by" && com[7] == "user" && IsNumber(com[8]))
            {
                GeneratorCommentsForPostsByUser(com, g, userReposytory, postReposytory, commentReposytory);
            }
            else
            {
                Error.WriteLine("generation command was entered incorrectly");
            }
        }while(true);
    }
    static void GeneratorUsers(string[] com, Generator g, UserReposytory userReposytory)
    {
        for(int i = 0; i < int.Parse(com[1]); i++)
        {
            User user = g.GeneratorUser();
            userReposytory.Insert(user);
            //WriteLine(user);
        }
    }
    static void GeneratorPostsByUser(string[] com, Generator g, UserReposytory userReposytory, PostReposytory postReposytory)
    {
        User user = userReposytory.GetByID(int.Parse(com[5]));
        if(user == null)
        {
            Console.Error.WriteLine("incoorect entered user id");
            return;
        }
        for(int i = 0; i < int.Parse(com[1]); i++)
        {
            Post post = g.GeneratorPost();
            long postId = postReposytory.Insert(post, user);
            //WriteLine(post);
        }
    }
    static void GeneratorCommentsForPostsByUser(string[] com, Generator g, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
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
            Comment comment = g.GeneratorComment();
            long commentId = commentReposytory.Insert(comment, post, user);
            //WriteLine(comment);
        }
    }
}