using System;
using Terminal.Gui;
using AccessDataLib;
public class CreateCommentDialog: Dialog
{
    public bool canceled;
    protected User user;
    protected Post post;
    protected TextView commentInput;
    public void SetAutor_Comment(User user, Post post)
    {
        this.user = user;
        this.post = post;
    }
    public CreateCommentDialog()
    {
        this.Title = "Create comment";

        Button okBtn = new Button("Ok");
        okBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(okBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);

        int rightColumnX = 20;

        Label postLbl = new Label(2,2,"Comment:");
        commentInput = new TextView()
        {
            X = rightColumnX,
            Y = Pos.Bottom(postLbl),
            Width = Dim.Fill(5),  // margin width
            Height = Dim.Percent(50),
            Text = "Somomething",
        };
        this.Add(postLbl, commentInput);
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
    public Comment GetComment()
    {
        string commentText = commentInput.Text.ToString();
        if(commentText != null)
        {
            Comment comment = new Comment(commentText, DateTime.Now.ToString());
            comment.userId = this.user.id;
            comment.postId = this.post.id;
            return comment;
        }
        return null;
    }
}