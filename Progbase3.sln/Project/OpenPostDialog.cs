using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;

public class OpenPostDialog: Dialog
{
    public bool deleted;
    public bool updated = false;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private List<Comment> postComments;
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
    public OpenPostDialog(User currentUser, Post post, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.currentUser = currentUser;
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.post = post;
        this.post.userId = postReposytory.UserID(post.id);
        this.user = userReposytory.GetByID(post.userId);
        this.post.commentIds = postReposytory.CommentsOfPostID(this.post.id);
        this.postComments = this.GetListOfComments(this.post.commentIds);

        this.Title = "Post";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        if(this.user.id == this.currentUser.id)
        {
            editProfileBtn = new Button("Edit");
            editProfileBtn.Clicked += OnEditPost;
            this.AddButton(editProfileBtn);
        }

        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editProfileBtn),
            Y = Pos.Top(editProfileBtn),           
        };
        deleteBtn.Clicked += OnPostDelete;
        this.AddButton(deleteBtn);

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
        if(this.GetSearchPage() == null)
        {
            frameView.Add(noCommentLbl);
        }
        else
        {
            frameView.Add(allCommentsToPostListView);
        }
        this.Add(frameView);

        Button createNewCommentBtn = new Button("Create new comment") // мб змінити розташування
        {
            X = rightColumnX,
            Y = Pos.Bottom(frameView),
        };
        createNewCommentBtn.Clicked += OnCreateNewComment;
        this.AddButton(createNewCommentBtn);

        UpdateCurrentPage();
    }
    private void OnCreateNewComment()
    {
        CreateCommentDialog dialog = new CreateCommentDialog();
        dialog.SetAutor_Post(this.user, this.post);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            if(dialog.GetComment() != null)
            {
                Comment commnet = dialog.GetComment();
                // Post post = postReposytory.GetByID(commnet.postId);
                // User user = userReposytory.GetByID(commnet.userId);
                this.commentReposytory.Insert(commnet, this.post, this.user);
                // user.comments.Add(commnet.id);
                // post.commentIds.Add(commnet.id);
                this.user.comments = userReposytory.UserComments(user.id);
                this.post.commentIds = postReposytory.CommentsOfPostID(post.id);
                this.postComments = GetListOfComments(this.post.commentIds);
                allCommentsToPostListView.SetSource(this.postComments);
                UpdateCurrentPage();
            }
        }
    }
    private int NumberOfPages()
    {
        if(this.postComments != null)
        {
            if(this.postComments.Count % pageLength == 0)
            {
                return this.postComments.Count / pageLength;
            }
            return this.postComments.Count / pageLength + 1;
        }
        return 1;
    }
    private void OnOpenComment(ListViewItemEventArgs args)
    {
        Comment comment = (Comment)args.Value;
        comment.userId = commentReposytory.UserID(comment.id);
        User userCC = userReposytory.GetByID(comment.userId);
        //MessageBox.ErrorQuery("", userCC.id.ToString(),"ok");
        comment.postId = commentReposytory.PostID(comment.id);
        Post postC = postReposytory.GetByID(comment.postId);
        //MessageBox.ErrorQuery("", postC.id.ToString(),"ok");
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
        int totalPages = this.NumberOfPages();
        if(currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        UpdateCurrentPage();
    }
    private void UpdateCurrentPage()
    {
        int totalPages = this.NumberOfPages();
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
        this.post.commentIds = postReposytory.CommentsOfPostID(postId);
        this.postComments = GetListOfComments(this.post.commentIds);
        allCommentsToPostListView.SetSource(this.postComments);
        allCommentsToPostListView.SetSource(GetSearchPage());
        
        prevPageBtn.Visible = (currentpage != 1);
        nextPageBtn.Visible = (currentpage != totalPages);
    }
    public void SetPost(Post post)
    {
        if(post != null)
        {
            this.post = post;
            this.user = userReposytory.GetByID(post.userId);
            this.postComments = GetListOfComments(post.commentIds);
        }
    }
    private List<Comment> GetListOfComments(List<long> commentIds)
    {
        if(commentIds.Count != 0)
        {
            List<Comment> allcomments = new List<Comment>();
            foreach (long item in commentIds)
            {
                allcomments.Add(commentReposytory.GetByID(item));
            }
            return allcomments;
        }
        return null;
    }
    private List<Comment> GetSearchPage()
    {
        if(this.postComments != null)
        {
            int index = 0;
            int counter = 0;
            List<Comment> page = new List<Comment>();
            foreach (Comment item in this.postComments)
            {
                if(index >= (currentpage - 1) * pageLength)
                {
                    page.Add(item);
                    counter++;
                    if(counter == pageLength)
                    {
                        break;
                    }
                }
                index++;
            }
            return page;
        }
        return null;
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