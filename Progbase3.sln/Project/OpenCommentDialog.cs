using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;

public class OpenCommentDialog: Dialog
{
    public bool unpinned = false;
    public bool deleted;
    public bool updated = false;
    private UserRepository userRepository;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
    protected Comment comment;
    protected User user;
    protected Post post;
    protected User currentUser;
    private TextView commentInput;
    private Button editCommentBtn;
    private Button unpinnedBtn;
    public OpenCommentDialog(User current, Comment comment, UserRepository userRepository, PostRepository postRepository, CommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.comment = comment;
        this.user = userRepository.GetByID(comment.userId);
        this.currentUser = current;

        this.Title = "Comment";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        this.editCommentBtn = new Button("Edit");
        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editCommentBtn),
            Y = Pos.Top(editCommentBtn),       
        };
        unpinnedBtn = new Button("Unpinned comment");
        if(currentUser.id == user.id)
        {
            editCommentBtn.Clicked += OnEditComment;
            this.AddButton(editCommentBtn);

            deleteBtn.Clicked += OnPostDelete;
            this.AddButton(deleteBtn);

            if(this.comment.pinned == "pinned")
            {
                unpinnedBtn.Clicked += OnUnpinnedBtnDelete;
                this.AddButton(unpinnedBtn);
            }
        }
        else if(this.currentUser.moderator == true)
        {
            deleteBtn.Clicked += OnPostDelete;
            this.AddButton(deleteBtn);
        }

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
        
        Label usernameLbl = new Label(2,6,$"Created by:");
        TextField usernameField = new TextField(this.user.username)
        {
            X = rightColumnX,
            Y = Pos.Top(usernameLbl),
            Width = 15,
            ReadOnly = true,
        };
        this.Add(usernameLbl, usernameField);

        Label createdAtTimeLbl = new Label(2, 4, "Created at:");
        DateField createdAtTimeField = new DateField(this.comment.createdAt)
        {
            X = rightColumnX,
            Y = Pos.Top(createdAtTimeLbl),
            Width = 15,
            ReadOnly = true,
        };
        this.Add(createdAtTimeLbl, createdAtTimeField);
    }
    private void OnUnpinnedBtnDelete()
    {
        this.unpinned = true;
        this.comment.pinned = "";
        bool update = this.commentRepository.Update(this.comment.id, this.comment);
        if(!update)
        {
            MessageBox.ErrorQuery("Error","Comment wasn't unpinned","ok");
            return;
        }
        MessageBox.ErrorQuery("Message","Comment was unpinned","ok");
        this.unpinnedBtn.Visible = false;
    }
    public void SetComment(Comment comment)
    {
        if(comment != null)
        {
            this.comment = comment;
            this.post = postRepository.GetByID(comment.postId);
            this.user = userRepository.GetByID(comment.userId);
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
        dialog.SetComment(this.comment, this.post, this.user);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            this.updated = true;
            Comment updatedcomment = dialog.GetComment();
            this.SetComment(updatedcomment);
            bool result = commentRepository.Update(this.post.id, updatedcomment);
            if(result)
            {
                this.comment.comment = updatedcomment.comment;
                this.SetComment(this.comment);
                commentInput.Text = this.comment.comment;
                this.updated = true;
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