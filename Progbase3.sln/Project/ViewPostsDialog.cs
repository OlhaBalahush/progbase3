using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;

public class ViewPostsDialog: Dialog
{
    // public bool deleted;
    // public bool updated = false;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    protected User currentUser;
    private ListView allPostListView;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private Label pageLbl;
    private Label totalPagesLbl;
    private FrameView frameView;
    private Label noPostLbl;
    private int pageLength = 5;
    private int currentpage = 1;
    private string searchValue = "";
    private TextField searchInput;
    public ViewPostsDialog(User current, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        //this.users;
        this.currentUser = current;

        this.Title = "Post";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        //int rightColumnX = 20;

        noPostLbl = new Label("No post")
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        allPostListView = new ListView((IList)null)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        allPostListView.OpenSelectedItem += OnOpenPost;
        
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

        frameView = new FrameView("Posts:")
        {
            X = 2,
            Y = 12,
            Width = Dim.Fill() - 4,
            Height = pageLength + 3,
        };
        if(postReposytory.GetSearchPage(searchValue, currentpage, pageLength).Count == 0)
        {
            frameView.Add(noPostLbl);
        }
        else
        {
            frameView.Add(allPostListView);
        }
        this.Add(frameView);

        searchInput = new TextField(2,4,20,"");
        searchInput.KeyPress += OnSearchEnter;
        this.Add(searchInput);

        UpdateCurrentPage();
    }
    private void OnSearchEnter(KeyEventEventArgs args)
    {
        if(args.KeyEvent.Key == Key.Enter)
        {
            this.searchValue = this.searchInput.Text.ToString();
            UpdateCurrentPage();
        }
    }
    private void UpdateCurrentPage()
    {
        int totalPages = postReposytory.NumberOfPages(this.searchValue, this.pageLength);
        if(totalPages == 0)
        {
            totalPages = 1;
        }
        if(currentpage > totalPages && currentpage > 1)
        {
            currentpage = totalPages;
        }
        this.pageLbl.Text = currentpage.ToString();
        this.totalPagesLbl.Text = totalPages.ToString();

        // this.user.comments = userReposytory.UserComments(user.id);
        // this.post.commentIds = postReposytory.CommentsOfPostID(postId);
        // this.postComments = GetListOfComments(this.post.commentIds);
        // allCommentsToPostListView.SetSource(this.postComments);
        allPostListView.SetSource(postReposytory.GetSearchPage(searchValue,  currentpage, this.pageLength));
        
        prevPageBtn.Visible = (currentpage != 1);
        nextPageBtn.Visible = (currentpage != totalPages);
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
        int totalPages = postReposytory.NumberOfPages(this.searchValue, this.pageLength);
        if(currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        UpdateCurrentPage();
    }
    private void OnOpenPost(ListViewItemEventArgs args)
    {
        Post post = (Post)args.Value;
        User user = post.user;
        OpenPostDialog dialog = new OpenPostDialog(this.currentUser , post, this.userReposytory, this.postReposytory, this.commentReposytory);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = postReposytory.Delete(post, user);
            if(result)
            {
                int pages = postReposytory.NumberOfPages(searchValue, pageLength);
                if(currentpage > pages && pageLength > 1)
                {
                    pages--;
                    this.UpdateCurrentPage();
                }
                this.UpdateCurrentPage();
                //this.users = 
                allPostListView.SetSource(postReposytory.PostsOnPage(currentpage));
            }
            else
            {
                MessageBox.ErrorQuery("Delete post", "Can't delete post", "Ok");
            }
        }
        if(dialog.updated)
        {
            if(dialog.GetPost() != null)
            {
                bool result = postReposytory.Update(post.id, dialog.GetPost());
                if(result)
                {
                    // this.currentUser.posts = userReposytory.UserPosts(this.currentUser.id);
                    // this.userPosts = GetListOfPosts(this.currentUser.posts);
                    allPostListView.SetSource(postReposytory.PostsOnPage(currentpage));
                    this.UpdateCurrentPage();
                }
                else
                {
                    MessageBox.ErrorQuery("Update concert", "Can't update concert", "Ok");
                }
            }
        }
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
}