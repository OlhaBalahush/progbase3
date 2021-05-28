using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;

public class OpenPostDialog: Dialog
{
    public bool deleted;
    public bool updated;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private List<Comment> postComments;
    private int pageLength = 5;
    private int currentpage = 1;
    protected Post post;
    protected User user;
    private TextView postInput;
    private ListView allCommentsToPostListView;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private Label pageLbl;
    private Label totalPagesLbl;
    private FrameView frameView;
    private Label noCommentLbl;
    public OpenPostDialog(Post post, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
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
        UpdateCurrentPage();
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
        // Post post = (Post)args.Value;
        // OpenPostDialog dialog = new OpenPostDialog();
        // dialog.SetPost(post);

        // Application.Run(dialog);

        // if(dialog.deleted)
        // {
        //     // bool result = concertReposytory.Delete(concert.id);
        //     // if(result)
        //     // {
        //     //     int pages = concertReposytory.NumberOfPages(searchValue,pageLength);
        //     //     if(currentpage > pages && pageLength > 1)
        //     //     {
        //     //         pages--;
        //     //         this.UpdateCurrentPage();
        //     //     }
        //     //     allConcertListView.SetSource(concertReposytory.ConcertsOnPage(currentpage));
        //     // }
        //     // else
        //     // {
        //     //     MessageBox.ErrorQuery("Delete concert", "Can't delete concert", "Ok");
        //     // }
        // }
        // if(dialog.updated)
        // {
        //     // if(dialog.GetConcert() != null)
        //     // {
        //     //     bool result = concertReposytory.Update(concert.id, dialog.GetConcert());
        //     //     if(result)
        //     //     {
        //     //         allConcertListView.SetSource(concertReposytory.ConcertsOnPage(currentpage));
        //     //     }
        //     //     else
        //     //     {
        //     //         MessageBox.ErrorQuery("Update concert", "Can't update concert", "Ok");
        //     //     }
        //     // }
        // }
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
        this.pageLbl.Text = currentpage.ToString();
        this.totalPagesLbl.Text = totalPages.ToString();
        user.posts = userReposytory.UserPosts(this.user.id);
        allCommentsToPostListView.SetSource(GetSearchPage());
        
        //MessageBox.ErrorQuery("", currentpage.ToString(), "ok");
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
            //postReposytory.Update(this.post.id, updatedpost);
            this.SetPost(updatedpost);
            //MessageBox.ErrorQuery("Update concert", updatedpost.post, "Ok");
            bool result = postReposytory.Update(this.post.id, updatedpost);
            if(!result)
            {
                //this.userPosts = GetListOfPosts(user.posts);
                this.post.post = updatedpost.post;
                this.SetPost(this.post);
                postInput.Text = this.post.post;
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