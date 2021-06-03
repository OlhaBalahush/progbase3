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

        UserReposytory userReposytory = new UserReposytory(connection);
        PostReposytory postReposytory = new PostReposytory(connection);
        CommentReposytory commentReposytory = new CommentReposytory(connection);

        Application.Init();
        Toplevel top = Application.Top;

        AuthenticationWindow registration = new AuthenticationWindow(top, userReposytory, postReposytory, commentReposytory);
        top.Add(registration);

        Application.Run();
    }
}