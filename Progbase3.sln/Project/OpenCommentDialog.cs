using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;

public class OpenCommentDialog: Dialog
{
    public bool deleted;
    public bool updated;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    protected Comment comment;
    protected User user;
    private TextView commentInput;
    public OpenCommentDialog(Comment comment, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.user = userReposytory.GetByID(comment.userId);

        this.Title = "Post";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        Button editProfileBtn = new Button("Edit");
        editProfileBtn.Clicked += OnEditPost;
        this.AddButton(editProfileBtn);

        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editProfileBtn),
            Y = Pos.Top(editProfileBtn),           
        };
        deleteBtn.Clicked += OnPostDelete;
        this.AddButton(deleteBtn);

        int rightColumnX = 20;
        
        Label postLbl = new Label(2,2,"Comment:");
        commentInput = new TextView()
        {
            X = rightColumnX,
            Y = Pos.Top(postLbl),
            Width = Dim.Fill(5),  // margin width
            Height = Dim.Percent(50),
            Text = this.comment.comment,
            ReadOnly = true,
        };
        this.Add(postLbl, commentInput);
    
        Label usernameLbl = new Label(2,6,$"Created by: {this.user.username}"); //мб створити кнопку з переходом на профіль
        this.Add(usernameLbl);
        Label createdAt = new Label(2,8,$"Created at: {this.comment.createdAt.ToString()}");
        this.Add(createdAt);
    }    
    public void SetComment(Comment comment)
    {
        if(comment != null)
        {
            this.comment = comment;
            this.user = userReposytory.GetByID(comment.userId);
        }
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
    private void OnEditPost()
    {
        dialog = new EditPostDialog();
        dialog.SetPost(this.comment, this.user);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            this.updated = true;
            Post updatedpost = dialog.GetPost();
            //postReposytory.Update(this.post.id, updatedpost);
            this.SetComment(updatedpost);
            //MessageBox.ErrorQuery("Update concert", updatedpost.post, "Ok");
            bool result = postReposytory.Update(this.post.id, updatedpost);
            if(!result)
            {
                //this.userPosts = GetListOfPosts(user.posts);
                this.post.post = updatedpost.post;
                this.SetComment(this.post);
                commentInput.Text = this.post.post;
                allCommentsToPostListView.SetSource(this.postComments);
            }
            else
            {
                MessageBox.ErrorQuery("Update concert", "Can't update concert", "Ok");
            }
            return;
        }
        this.updated = false;
    }
    private void OnPostDelete()
    {
        int index = MessageBox.Query("Delete concert", "Are you sure?", "No", "Yes");
        if(index == 1)
        {
            this.deleted = true;
            Application.RequestStop();
            return;
        }
        this.deleted = false;
    }
}