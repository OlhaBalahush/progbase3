using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
public class EditCommentDialog: CreateCommentDialog
{
    public EditCommentDialog()
    {
        this.Title = "Edit comment";
    }
    public void SetComment(Comment comment,Post post, User user)
    {
        this.commentInput.Text = comment.comment;
        this.user = user;
        this.post = post;
    }
}