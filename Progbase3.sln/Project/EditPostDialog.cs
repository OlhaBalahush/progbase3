using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
public class EditPostDialog: CreatePostDialog
{
    public EditPostDialog()
    {
        this.Title = "Edit post";
    }
    public void SetPost(Post post, User user)
    {
        this.postInput.Text = post.post;
        this.user = user;
    }
}