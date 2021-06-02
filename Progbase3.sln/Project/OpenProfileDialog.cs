using System; 
using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;
public class OpenProfileDialog: Dialog
{
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private List<Post> userPosts;
    private int pageLength = 5;
    private int currentpage = 1;
    protected User user;
    protected User currentuser;
    public bool deleted;
    public bool updated;
    private TextField usernameInput;
    private ListView allPostOfUserListView;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private Label pageLbl;
    private Label totalPagesLbl;
    public OpenProfileDialog(User currentuser, User user, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.user = user;
        this.currentuser = currentuser;
        this.user.posts = userReposytory.UserPosts(this.user.id);
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
        if(this.GetSearchPage().Count == 0)
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
        Button deleteBtn = new Button("Delete")
        {
            X = Pos.Right(editProfileBtn),
            Y = Pos.Top(editProfileBtn),           
        };
        Button statisticBtn = new Button("Statistic");
        Button moderatorBtn = new Button("create moderator");
        if(currentuser.username == user.username)
        {
            editProfileBtn.Clicked += OnEditProfile;
            this.AddButton(editProfileBtn);

            deleteBtn.Clicked += OnPostDelete;
            this.AddButton(deleteBtn);

            statisticBtn.Clicked += OnStatisticBtn;
            this.AddButton(statisticBtn);
        }
        else if(currentuser.moderator == true)
        {
            //create moderator
            moderatorBtn.Clicked += OnCreateModeratorBtn;
            this.AddButton(moderatorBtn);
        }

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
    private void OnCreateModeratorBtn()
    {
        if(this.user.moderator == true)
        {
            MessageBox.ErrorQuery("Message",$"{this.user.username} is already a moderator","ok");
            return;
        }
        User newuser = this.user;
        newuser.moderator = true;
        MessageBox.Query("Message",$"Are you sure that you want to create a moderator from the {this.user.username}?","no","yes");
        bool result = userReposytory.Update(this.user.id, newuser);
        if(!result)
        {
            MessageBox.ErrorQuery("Error",$"{this.user.username} cannot be a moderator","ok");
            return;
        }
        MessageBox.ErrorQuery("Message",$"{this.user.username} is moderator","ok");
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
    private void OnStatisticBtn()
    {
        GenerationImageWindow dialog = new GenerationImageWindow(this.user, this.userReposytory, this.postReposytory);
        Application.Run(dialog);
    }
    private void OnOpenPost(ListViewItemEventArgs args)
    {
        Post post = (Post)args.Value;
        User user = post.user;
        OpenPostDialog dialog = new OpenPostDialog(this.user, post, this.userReposytory, this.postReposytory, this.commentReposytory);
        //dialog.SetPost(post);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = postReposytory.Delete(post, user);
            if(result)
            {
                int pages = this.NumberOfPages();
                if(this.currentpage > pages && pageLength > 1)
                {
                    pages--;
                    this.UpdateCurrentPage();
                }
                this.UpdateCurrentPage();
                this.user.posts = userReposytory.UserPosts(this.user.id);
                this.userPosts = this.user.posts;
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
                    this.user.posts = userReposytory.UserPosts(this.user.id);
                    this.userPosts = this.user.posts;
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
        if(this.currentpage > totalPages && this.currentpage > 1)
        {
            this.currentpage = totalPages;
        }
        this.pageLbl.Text = currentpage.ToString();
        this.totalPagesLbl.Text = totalPages.ToString();
        this.user.posts = userReposytory.UserPosts(this.user.id);
        this.userPosts = user.posts;
        allPostOfUserListView.SetSource(GetSearchPage());

        prevPageBtn.Visible = (this.currentpage != 1);
        nextPageBtn.Visible = (this.currentpage != totalPages);
        this.Add(prevPageBtn, nextPageBtn);
    }
    private void OnPreviousPage()
    {
        if(this.currentpage == 1)
        {
            return;
        }
        this.currentpage--;
        this.UpdateCurrentPage();
    }
    private void OnNextPage()
    {
        int totalPages = this.NumberOfPages();
        if(this.currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        this.UpdateCurrentPage();

    }
    private void OnEditProfile()
    {
        EditProfileDialog dialog = new EditProfileDialog();
        dialog.SetUser(this.user);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            this.updated = true;
            User updateduser = dialog.GetUser();
            updateduser.moderator = this.user.moderator;
            //updateduser = dialog.GetUser();
            //MessageBox.ErrorQuery("Error", $"{updateduser.username}", "ok");
            bool result = userReposytory.Update(this.user.id, updateduser);
            if(result)
            {
                //this.userPosts = GetListOfPosts(user.posts);
                this.user.username = updateduser.username;
                this.SetUser(this.user);
                this.usernameInput.Text = user.username;
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
            this.user = user;
            this.userPosts = user.posts;
        }
    }
    public User GetUser()
    {
        return this.user;
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
            //long userId = this.user.id;
            this.user.posts = this.userReposytory.UserPosts(this.user.id);
            this.user.comments = this.userReposytory.UserComments(this.user.id);
            List<Post> postsOfuser = this.user.posts;
            foreach (Post item in postsOfuser)
            {
                item.comments = this.postReposytory.CommentsOfPost(item.id, this.commentReposytory);
                foreach (Comment comment in item.comments)
                {
                    comment.userId = this.commentReposytory.UserID(comment.id);
                    comment.postId = this.commentReposytory.PostID(comment.id);
                    User userc = userReposytory.GetByID(comment.userId);
                    bool deltecomment = this.commentReposytory.Delete(comment, userc, item);
                    MessageBox.ErrorQuery($"Delete commet to post{item.id}", "Can't delete post", "Ok");
                    
                }
                bool deltepost = this.postReposytory.Delete(item, this.user);
                MessageBox.ErrorQuery("Delete post of user", "Can't delete post", "Ok");
            }
            List<Comment> commentsOfuser = this.user.comments;
            foreach (Comment item in commentsOfuser)
            {
                item.userId = user.id; //this.commentReposytory.UserID(item.id);
                item.postId = this.commentReposytory.PostID(item.id);
                Post post = this.postReposytory.GetByID(item.postId);
                bool deltecomment = this.commentReposytory.Delete(item, this.user, post);
                if(!deltecomment)
                {
                    MessageBox.ErrorQuery("Delete comment of user", "Can't delete comment", "Ok");
                }
            }
            bool delete = userReposytory.Delete(this.user.id); // checkitem
            if(!delete)
            {
                MessageBox.ErrorQuery("Delete user", "Can't delete user", "Ok");
            }
            Application.RequestStop();
            return;
        }
        this.deleted = false;
    }
}