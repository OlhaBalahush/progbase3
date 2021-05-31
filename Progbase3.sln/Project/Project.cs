using AccessDataLib;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using static System.Console;
using System.Collections.Generic;
using Terminal.Gui;
class Program
{
    //ВИДАЛЯТИ УСЕ ЗАПИСАНЕ КОРИСТУВАЧЕМ(КОМЕНТИ ТА ПОСТИ) ЧИ РОБИТИ МАРКЕР ВИДАЛЕНОГО КОРИСТУВАЧА?????
    // static void Clean(UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory) // delete
    // {
    //     for(int i = 0; i < 20; i++)
    //     {
    //         Post post = postReposytory.GetByID(i);
    //         WriteLine(postReposytory.Delete(post, post.user/*, commentReposytory*/ ));
    //         Comment comment = commentReposytory.GetByID(i);
    //         WriteLine(commentReposytory.Delete(comment, userReposytory.GetByID(comment.userId), postReposytory.GetByID(comment.postId)));
            
    //         WriteLine(userReposytory.Delete(i));
    //     }
    // }
    // static bool IsNumber(string value)
    // {
    //     char[] charArr = value.ToCharArray();
    //     int i = 0;
    //     if(charArr[0] == 45)
    //     {
    //         i = 1;
    //     }
    //     while (i < charArr.Length)
    //     {
    //         if(char.IsDigit(charArr[i]) == false)
    //         {
    //             return false;
    //         }
    //         i++;
    //     }
    //     return true;
    // }
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
    }
}