using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;

public class OpenPostDialog: Dialog
{
    public  bool pinnedcomment;
    public bool deleted;
    public bool updated = false;
    private UserRepository userRepository;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
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
    public OpenPostDialog(User currentUser, Post post, UserRepository userRepository, PostRepository postRepository, CommentRepository commentRepository)
    {
        this.currentUser = currentUser;
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.post = post;
        this.post.user = postRepository.User(post.id);
        this.user = post.user;
        this.post.comments = postRepository.CommentsOfPost(this.post.id, this.commentRepository);
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
    
        Label usernameLbl = new Label(2,6,$"Created by:");
        TextField usernameField = new TextField(this.user.username)
        {
            X = rightColumnX,
            Y = Pos.Top(usernameLbl),
            Width = 15,
            ReadOnly = true,
        };
        this.Add(usernameLbl, usernameField);

        Label createdAtTimeLbl = new Label(2, 8, "Created at:");
        DateField createdAtTimeField = new DateField(this.post.createdAt)
        {
            X = rightColumnX,
            Y = Pos.Top(createdAtTimeLbl),
            Width = 15,
            ReadOnly = true,
        };
        this.Add(createdAtTimeLbl, createdAtTimeField);

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
        if(this.commentRepository.GetSearchPage(this.postComments, searchValue, this.currentpage - 1, pageLength).Count == 0)
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
        bool updatepost = this.postRepository.Update(this.post.id, this.post);
        bool update = this.commentRepository.Update(comment.id, comment);
        this.post.comments = this.postRepository.CommentsOfPost(this.post.id, this.commentRepository);
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
                this.commentRepository.Insert(commnet, this.post, this.currentUser);
                this.user.comments = userRepository.UserComments(user.id);
                this.post.comments = postRepository.CommentsOfPost(post.id, this.commentRepository);
                this.postComments = this.post.comments;
                allCommentsToPostListView.SetSource(this.postComments);
                UpdateCurrentPage();
            }
        }
    }
    private void OnOpenComment(ListViewItemEventArgs args)
    {
        Comment comment = (Comment)args.Value;
        comment.userId = commentRepository.UserID(comment.id);
        User userCC = userRepository.GetByID(comment.userId);
        comment.postId = commentRepository.PostID(comment.id);
        Post postC = postRepository.GetByID(comment.postId);
        OpenCommentDialog dialog = new OpenCommentDialog(this.user, comment, this.userRepository, this.postRepository, this.commentRepository);
        dialog.SetComment(comment);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = commentRepository.Delete(comment, userCC, postC);
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
                bool result = commentRepository.Update(comment.id, dialog.GetComment());
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
            bool update = this.postRepository.Update(this.post.id, this.post);
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
        int totalPages = this.commentRepository.NumberOfPages(this.postComments, searchValue, pageLength);
        if(currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        UpdateCurrentPage();
    }
    private void UpdateCurrentPage()
    {
        int totalPages = this.commentRepository.NumberOfPages(this.postComments, searchValue, pageLength);
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
        this.user.comments = userRepository.UserComments(user.id);
        this.post.comments = postRepository.CommentsOfPost(postId, this.commentRepository);
        List<Comment> comments = postRepository.CommentsOfPost(postId, this.commentRepository);
        this.postComments = Pinned(comments);
        if(this.pinnedcomment == true || this.commentRepository.GetSearchPage(this.postComments, searchValue, this.currentpage - 1, pageLength).Count == 0)
        {
            pinBtn.Visible = false;// зробити кнопку прикріпити коментар невидимою
        }
        else
        {
            pinBtn.Visible = true;
        }
        allCommentsToPostListView.SetSource(this.commentRepository.GetSearchPage(this.postComments, searchValue, this.currentpage - 1, pageLength));
        
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
            bool result = postRepository.Update(this.post.id, updatedpost);
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