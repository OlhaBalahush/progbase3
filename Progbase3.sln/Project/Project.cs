using AccessDataLib;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using static System.Console;
using System.Collections.Generic;
using Terminal.Gui;
class Program
{
    static void Main(string[] args)
    {
        string databaseFileName = "C:/Users/Olha/Desktop/progbase3/data/socialNetwork.db";
        SqliteConnection connection = new SqliteConnection($"Data Source={databaseFileName}");

        UserRepository userRepository = new UserRepository(connection);
        PostRepository postRepository = new PostRepository(connection);
        CommentRepository commentRepository = new CommentRepository(connection);

        Application.Init();
        Toplevel top = Application.Top;

        AuthenticationWindow registration = new AuthenticationWindow(top, userRepository, postRepository, commentRepository);
        top.Add(registration);

        Application.Run();
    }
}