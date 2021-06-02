using System;
using Terminal.Gui;
using AccessDataLib;
public class CreatePostDialog: Dialog
{
    public bool canceled;
    protected User user;
    protected TextView postInput;
    public void SetAutor(User user)
    {
        this.user = user;
    }
    public CreatePostDialog()
    {
        this.Title = "Create post";
        Button okBtn = new Button("Ok");
        okBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(okBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);

        int rightColumnX = 20;

        Label postLbl = new Label(2,2,"Text of Post:");
        postInput = new TextView()
        {
            X = rightColumnX,
            Y = Pos.Bottom(postLbl),
            Width = Dim.Fill(5),  // margin width
            Height = Dim.Percent(50),
            Text = "Some\r\ntest\r\nLast line!",
            //TabWidth = 5,
        };
        this.Add(postLbl, postInput);
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
    private void OnCreateDialogSubmit()
    {
        this.canceled = false;
        Application.RequestStop();
    }
    public Post GetPost()
    {
        string postText = postInput.Text.ToString();
        // int maxLenght = 20; // кількість символів у рядку
        // char [] ch = postText.ToCharArray();
        // int j = 1;
        // for(int i = 0; i < maxLenght; i++)
        // {
        //     if(ch[maxLenght * j] != 32)
        //     {}
        // }
        if(postText != null)
        {
            Post post = new Post(postText, DateTime.Now.ToString());
            post.user = this.user;
            return post;
        }
        return null;
    }
}