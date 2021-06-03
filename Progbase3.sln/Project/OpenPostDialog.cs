using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;

public class OpenPostDialog: Dialog
{
    public  bool pinnedcomment;
    public bool deleted;
    public bool updated = false;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private List<Comment> postComments;
    private string searchValue = "";
    private int pageLength = 5;
    private int currentpage = 1;
    protected Post post;
    protected User user;
    protected User currentUser;
    private TextView postInput;
    private ListView allCommentsToPostListView;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private Button editProfileBtn;
    private Label pageLbl;
    private Label totalPagesLbl;
    private FrameView frameView;
    private Label noCommentLbl;
    private Button pinBtn;
    public OpenPostDialog(User currentUser, Post post, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.currentUser = currentUser;
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.post = post;
        this.post.user = postReposytory.User(post.id);
        this.user = post.user;
        this.post.comments = postReposytory.CommentsOfPost(this.post.id, this.commentReposytory);
        this.postComments = this.post.comments;
        if(this.post.pinnedCommetId == -1)
        {
            this.pinnedcomment = false;
        }
        else
        {
            this.pinnedcomment = true;
        }

        this.Title = "Post";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        editProfileBtn = new Button("Edit");
        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editProfileBtn),
            Y = Pos.Top(editProfileBtn),           
        };
        pinBtn = new Button(2,22,"Pin comment");
        if(this.user.id == this.currentUser.id)
        {
            editProfileBtn.Clicked += OnEditPost;
            this.AddButton(editProfileBtn);

            deleteBtn.Clicked += OnPostDelete;
            this.AddButton(deleteBtn);

            pinBtn.Clicked += OnPinComment;
            this.AddButton(pinBtn);
            if(this.pinnedcomment == false)
            {
                pinBtn.Visible = false;
            }
        }
        else if(this.currentUser.moderator == true)
        {
            deleteBtn.Clicked += OnPostDelete;
            this.AddButton(deleteBtn);
        }

        int rightColumnX = 20;
        
        Label postLbl = new Label(2,2,"Post:");
        postInput = new TextView()
        {
            X = rightColumnX,
            Y = Pos.Top(postLbl),
            Width = Dim.Fill(5),  // margin width
            Height = Dim.Percent(50),
            Text = this.post.post,
            ReadOnly = true,
        };
        this.Add(postLbl, postInput);
    
        Label usernameLbl = new Label(2,6,$"Created by: {this.user.username}"); //мб створити кнопку з переходом на профіль
        this.Add(usernameLbl);
        
        Label createdAt = new Label(2,8,$"Created at: {this.post.createdAt.ToString()}");
        this.Add(createdAt);

        noCommentLbl = new Label("No comment")
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        allCommentsToPostListView = new ListView((IList)null)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        allCommentsToPostListView.OpenSelectedItem += OnOpenComment;
        
        prevPageBtn = new Button(2,10,"Prev");
        prevPageBtn.Clicked += OnPreviousPage;
        pageLbl = new Label("?")
        {
            X = Pos.Right(prevPageBtn) + 2,
            Y = Pos.Top(prevPageBtn),
            Width = 5,
        };
        totalPagesLbl = new Label("?")
        {
            X = Pos.Right(pageLbl) + 2,
            Y = Pos.Top(prevPageBtn),
            Width = 5,
        };
        nextPageBtn = new Button("Next")
        {
            X = Pos.Right(totalPagesLbl) + 2,
            Y = Pos.Top(prevPageBtn),
        };
        nextPageBtn.Clicked += OnNextPage;
        this.Add(prevPageBtn, pageLbl, totalPagesLbl, nextPageBtn);

        frameView = new FrameView("User posts:")
        {
            X = 2,
            Y = 12,
            Width = Dim.Fill() - 4,
            Height = pageLength + 3,
        };
        if(this.commentReposytory.GetSearchPage(this.postComments, searchValue, this.currentpage - 1, pageLength).Count == 0)
        {
            frameView.Add(noCommentLbl);
        }
        else
        {
            frameView.Add(allCommentsToPostListView);
        }
        this.Add(frameView);

        Button createNewCommentBtn = new Button("Create new comment")
        {
            X = Pos.Left(frameView),
            Y = Pos.Bottom(frameView) + 2,
        };
        createNewCommentBtn.Clicked += OnCreateNewComment;
        this.AddButton(createNewCommentBtn);

        UpdateCurrentPage();
    }
    private void OnPinComment()
    {
        if(pinnedcomment)
        {
            MessageBox.ErrorQuery("Error","Other comment pinned","ok");
            return;
        }
        int commentindex = this.allCommentsToPostListView.SelectedItem;
        if(commentindex == -1)
        {
            return;
        }
        if(this.post.pinnedCommetId != -1)
        {
            MessageBox.ErrorQuery("","Comment pinned","ok");
            return;
        }
        Comment comment = (Comment)this.allCommentsToPostListView.Source.ToList()[commentindex];
        comment.pinned = "pinned";
        this.post.pinnedCommetId = comment.id;
        bool updatepost = this.postReposytory.Update(this.post.id, this.post);
        bool update = this.commentReposytory.Update(comment.id, comment);
        this.post.comments = this.postReposytory.CommentsOfPost(this.post.id, this.commentReposytory);
        this.post.comments.Remove(comment);
        this.post.comments.Insert(0, comment);
        this.pinnedcomment = true;
        pinBtn.Visible = false;
    }
    private void OnCreateNewComment()
    {
        CreateCommentDialog dialog = new CreateCommentDialog();
        dialog.SetAutor_Comment(this.currentUser, this.post);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            if(dialog.GetComment() != null)
            {
                Comment commnet = dialog.GetComment();
                this.commentReposytory.Insert(commnet, this.post, this.currentUser);
                this.user.comments = userReposytory.UserComments(user.id);
                this.post.comments = postReposytory.CommentsOfPost(post.id, this.commentReposytory);
                this.postComments = this.post.comments;
                allCommentsToPostListView.SetSource(this.postComments);
                UpdateCurrentPage();
            }
        }
    }
    private void OnOpenComment(ListViewItemEventArgs args)
    {
        Comment comment = (Comment)args.Value;
        comment.userId = commentReposytory.UserID(comment.id);
        User userCC = userReposytory.GetByID(comment.userId);
        comment.postId = commentReposytory.PostID(comment.id);
        Post postC = postReposytory.GetByID(comment.postId);
        OpenCommentDialog dialog = new OpenCommentDialog(this.user, comment, this.userReposytory, this.postReposytory, this.commentReposytory);
        dialog.SetComment(comment);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = commentReposytory.Delete(comment, userCC, postC);
            if(result)
            {
                OnCreateDialogSubmit();
            }
            else
            {
                MessageBox.ErrorQuery("Delete comment", "Can't delete comment", "Ok");
            }
        }
        if(dialog.updated)
        {
            if(dialog.GetComment() != null)
            {
                bool result = commentReposytory.Update(comment.id, dialog.GetComment());
                if(result)
                {
                    UpdateCurrentPage();
                }
                else
                {
                    MessageBox.ErrorQuery("Update concert", "Can't update concert", "Ok");
                }
            }
            
        }
        if(dialog.unpinned)
        {
            this.post.pinnedCommetId = -1;
            bool update = this.postReposytory.Update(this.post.id, this.post);
            UpdateCurrentPage();
        }
    }
    private void OnPreviousPage()
    {
        if(currentpage == 1)
        {
            return;
        }
        this.currentpage--;
        UpdateCurrentPage();
    }
    private void OnNextPage()
    {
        int totalPages = this.commentReposytory.NumberOfPages(this.postComments, searchValue, pageLength);
        if(currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        UpdateCurrentPage();
    }
    private void UpdateCurrentPage()
    {
        int totalPages = this.commentReposytory.NumberOfPages(this.postComments, searchValue, pageLength);
        if(totalPages == 0)
        {
            totalPages = 1;
        }
        if(currentpage > totalPages && currentpage > 1)
        {
            currentpage = totalPages;
        }
        long postId = post.id;
        this.pageLbl.Text = currentpage.ToString();
        this.totalPagesLbl.Text = totalPages.ToString();
        this.user.comments = userReposytory.UserComments(user.id);
        this.post.comments = postReposytory.CommentsOfPost(postId, this.commentReposytory);
        List<Comment> comments = postReposytory.CommentsOfPost(postId, this.commentReposytory);
        this.postComments = Pinned(comments);
        if(this.pinnedcomment == true || this.commentReposytory.GetSearchPage(this.postComments, searchValue, this.currentpage - 1, pageLength).Count == 0)
        {
            pinBtn.Visible = false;// зробити кнопку прикріпити коментар невидимою
        }
        else
        {
            pinBtn.Visible = true;
        }
        allCommentsToPostListView.SetSource(this.commentReposytory.GetSearchPage(this.postComments, searchValue, this.currentpage - 1, pageLength));
        
        prevPageBtn.Visible = (currentpage != 1);
        nextPageBtn.Visible = (currentpage != totalPages);
    }
    private List<Comment> Pinned(List<Comment> comments)
    {
        foreach (Comment item in comments)
        {
            if(item.pinned == "pinned")
            {
                comments.Remove(item);
                this.pinnedcomment = true;
                comments.Insert(0, item);
                break;
            }
        }
        return comments;
    }
    public void SetPost(Post post)
    {
        if(post != null)
        {
            this.post = post;
            this.user = post.user;
            this.postComments = this.post.comments;
        }
    }
    public Post GetPost()
    {
        return this.post;
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
    private void OnEditPost()
    {
        EditPostDialog dialog = new EditPostDialog();
        dialog.SetPost(this.post, this.user);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            this.updated = true;
            Post updatedpost = dialog.GetPost();
            bool result = postReposytory.Update(this.post.id, updatedpost);
            if(result)
            {
                this.post.post = updatedpost.post;
                this.SetPost(this.post);
                this.postInput.Text = this.post.post;
                UpdateCurrentPage();
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