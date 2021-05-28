using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;

public class OpenCommentDialog: Dialog
{
    public bool deleted;
    public bool updated = false;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    protected Comment comment;
    protected User user;
    protected Post post;
    protected User currentUser;
    private TextView commentInput;
    private Button editCommentBtn;
    public OpenCommentDialog(User current, Comment comment, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.comment = comment;
        this.user = userReposytory.GetByID(comment.userId);
        this.currentUser = current;

        this.Title = "Post";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        if(currentUser.id == user.id)
        {
            this.editCommentBtn = new Button("Edit");
            editCommentBtn.Clicked += OnEditComment;
            this.AddButton(editCommentBtn);
        }

        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editCommentBtn),
            Y = Pos.Top(editCommentBtn),       
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
            this.post = postReposytory.GetByID(comment.postId);
            this.user = userReposytory.GetByID(comment.userId);
        }
    }
    public Comment GetComment()
    {
        return this.comment;
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
    private void OnEditComment()
    {
        EditCommentDialog dialog = new EditCommentDialog();
        //dialog.SetComment();
        MessageBox.ErrorQuery("", comment.id.ToString(),"ok");
        if(this.post == null)
        {
            MessageBox.ErrorQuery("","where is post??","ok");
        }
        dialog.SetComment(this.comment, this.post, this.user);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            this.updated = true;
            Comment updatedcomment = dialog.GetComment();
            //postReposytory.Update(this.post.id, updatedpost);
            this.SetComment(updatedcomment);
            //MessageBox.ErrorQuery("Update concert", updatedpost.post, "Ok");
            bool result = commentReposytory.Update(this.post.id, updatedcomment);
            if(result)
            {
                //this.userPosts = GetListOfPosts(user.posts);
                this.comment.comment = updatedcomment.comment;
                this.SetComment(this.comment);
                commentInput.Text = this.comment.comment;
                this.updated = true;
                // allCommentsToPostListView.SetSource(this.postComments);
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