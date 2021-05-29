using System; 
using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
public class OpenProfileDialog: Dialog
{
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private List<Post> userPosts;
    private int pageLength = 5;
    private int currentpage = 1;
    protected User currentUser;
    public bool deleted;
    public bool updated;
    private TextField usernameInput;
    private ListView allPostOfUserListView;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private Label pageLbl;
    private Label totalPagesLbl;
    public OpenProfileDialog(User user, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.currentUser = user;
        this.currentUser.posts = userReposytory.UserPosts(this.currentUser.id);
        this.userPosts = user.posts;
        this.Title = "My profile";
        allPostOfUserListView = new ListView((IList)null)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        allPostOfUserListView.OpenSelectedItem += OnOpenPost;

        prevPageBtn = new Button(2,6,"Prev");
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

        Label noPostLbl = new Label("No post")
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        FrameView frameView = new FrameView("User posts:")
        {
            X = 2,
            Y = 8,
            Width = Dim.Fill() - 4,
            Height = pageLength + 3,
        };
        if(this.GetSearchPage() == null)
        {
            frameView.Add(noPostLbl);
        }
        else
        {
            frameView.Add(allPostOfUserListView);
        }
        this.Add(frameView);
        
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        Button editProfileBtn = new Button("Edit");
        editProfileBtn.Clicked += OnEditProfile;
        this.AddButton(editProfileBtn);

        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editProfileBtn),
            Y = Pos.Top(editProfileBtn),           
        };
        deleteBtn.Clicked += OnPostDelete;
        this.AddButton(deleteBtn);

        int rightColumnX = 20;

        Label usernameLbl = new Label(2,2,"Username");
        usernameInput = new TextField(user.username)
        {
            X = rightColumnX,
            Y = Pos.Top(usernameLbl),
            Width = 40,
            ReadOnly = true,
        };
        this.Add(usernameLbl, usernameInput);

        this.UpdateCurrentPage();
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
    private void OnOpenPost(ListViewItemEventArgs args)
    {
        Post post = (Post)args.Value;
        User user = userReposytory.GetByID(post.userId);
        OpenPostDialog dialog = new OpenPostDialog(this.currentUser, post, this.userReposytory, this.postReposytory, this.commentReposytory);
        //dialog.SetPost(post);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = postReposytory.Delete(post, user);
            if(result)
            {
                int pages = this.NumberOfPages();
                if(currentpage > pages && pageLength > 1)
                {
                    pages--;
                    this.UpdateCurrentPage();
                }
                this.UpdateCurrentPage();
                this.currentUser.posts = userReposytory.UserPosts(this.currentUser.id);
                this.userPosts = currentUser.posts;
                allPostOfUserListView.SetSource(this.userPosts);
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
                    this.currentUser.posts = userReposytory.UserPosts(this.currentUser.id);
                    this.userPosts = this.currentUser.posts;
                    allPostOfUserListView.SetSource(this.userPosts);
                    this.UpdateCurrentPage();
                }
                else
                {
                    MessageBox.ErrorQuery("Update profile", "Can't update profile", "Ok");
                }
            }
        }
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
        this.currentUser.posts = userReposytory.UserPosts(this.currentUser.id);
        this.userPosts = currentUser.posts;
        allPostOfUserListView.SetSource(GetSearchPage());

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
        this.UpdateCurrentPage();
    }
    private void OnNextPage()
    {
        int totalPages = this.NumberOfPages();
        if(currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        this.UpdateCurrentPage();
    }
    private void OnEditProfile()
    {
        EditProfileDialog dialog = new EditProfileDialog();
        dialog.SetUser(this.currentUser);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            this.updated = true;
            User updateduser = dialog.GetUser();
            //updateduser = dialog.GetUser();
            //MessageBox.ErrorQuery("Error", $"{updateduser.username}", "ok");
            bool result = userReposytory.Update(this.currentUser.id, updateduser);
            if(result)
            {
                //this.userPosts = GetListOfPosts(user.posts);
                this.currentUser.username = updateduser.username;
                this.SetUser(this.currentUser);
                this.usernameInput.Text = currentUser.username;
                allPostOfUserListView.SetSource(this.userPosts);
            }
            else
            {
                MessageBox.ErrorQuery("Update concert", "Can't update concert", "Ok");
            }
            return;
        }
        this.updated = false;
    }
    public void SetUser(User user)
    {
        if(user != null)
        {
            this.currentUser = user;
            this.userPosts = user.posts;
        }
    }
    public User GetUser()
    {
        return this.currentUser;
    }
    private List<Post> GetListOfPosts(List<long> postidS)
    {
        List<Post> posts = new List<Post>();
        foreach (long item in postidS)
        {
            posts.Add(postReposytory.GetByID(item));
        }
        return posts;
    }
    private int NumberOfPages()
    {
        if(this.userPosts.Count % pageLength == 0)
        {
            return this.userPosts.Count / pageLength;
        }
        return this.userPosts.Count / pageLength + 1;
    }
    private List<Post> GetSearchPage()
    {
        //MessageBox.ErrorQuery("", this.userPosts.Count.ToString(), "ok");
        if(this.userPosts != null)
        {
            int index = 0;
            int counter = 0;
            List<Post> page = new List<Post>();
            foreach (Post item in this.userPosts)
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
    private void OnPostDelete()
    {
        int index = MessageBox.Query("Delete your profile", "Are you sure?", "No", "Yes");
        if(index == 1)
        {
            this.deleted = true;
            Application.RequestStop();
            bool delete = userReposytory.Delete(currentUser.id); // check
            if(!delete)
            {
                MessageBox.ErrorQuery("Delete post", "Can't delete post", "Ok");
            }
            Application.RequestStop();
            return;
        }
        this.deleted = false;
    }
}