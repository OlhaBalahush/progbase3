using System; 
using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;
public class OpenProfileDialog: Dialog
{
    private UserRepository userRepository;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
    private List<Post> userPosts;
    private string searchValue = "";
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
    public OpenProfileDialog(User currentuser, User user, UserRepository userRepository, PostRepository postRepository, CommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.user = user;
        this.currentuser = currentuser;
        this.user.posts = userRepository.UserPosts(this.user.id);
        this.userPosts = user.posts;
        this.Title = "Profile";
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
        if(this.postRepository.GetSearchPage(this.userPosts, searchValue, currentpage - 1, pageLength).Count == 0)
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
            this.Title = "My profile";
            editProfileBtn.Clicked += OnEditProfile;
            this.AddButton(editProfileBtn);

            deleteBtn.Clicked += OnPostDelete;
            this.AddButton(deleteBtn);

            statisticBtn.Clicked += OnStatisticBtn;
            this.AddButton(statisticBtn);
        }
        else if(currentuser.moderator == true)
        {
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

        Label createdAtTimeLbl = new Label(2, 4, "Created at:");
        DateField createdAtTimeField = new DateField(this.user.createdAt)
        {
            X = rightColumnX,
            Y = Pos.Top(createdAtTimeLbl),
            Width = 15,
            ReadOnly = true,
        };
        this.Add(createdAtTimeLbl, createdAtTimeField);

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
        bool result = userRepository.Update(this.user.id, newuser);
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
        GenerationImageWindow dialog = new GenerationImageWindow(this.user, this.userRepository, this.postRepository, this.commentRepository);
        Application.Run(dialog);
    }
    private void OnOpenPost(ListViewItemEventArgs args)
    {
        Post post = (Post)args.Value;
        User user = post.user;
        OpenPostDialog dialog = new OpenPostDialog(this.user, post, this.userRepository, this.postRepository, this.commentRepository);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = postRepository.Delete(post, user);
            if(result)
            {
                int pages = this.postRepository.NumberOfPages(this.userPosts, searchValue, pageLength);
                if(this.currentpage > pages && pageLength > 1)
                {
                    pages--;
                    this.UpdateCurrentPage();
                }
                this.UpdateCurrentPage();
                this.user.posts = userRepository.UserPosts(this.user.id);
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
                bool result = postRepository.Update(post.id, dialog.GetPost());
                if(result)
                {
                    this.user.posts = userRepository.UserPosts(this.user.id);
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
        int totalPages = this.postRepository.NumberOfPages(this.userPosts, searchValue, pageLength);
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
        this.user.posts = userRepository.UserPosts(this.user.id);
        this.userPosts = user.posts;
        allPostOfUserListView.SetSource(this.postRepository.GetSearchPage(this.userPosts, searchValue, currentpage - 1, pageLength));

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
        int totalPages = this.postRepository.NumberOfPages(this.userPosts, searchValue, pageLength);
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
            bool result = userRepository.Update(this.user.id, updateduser);
            if(result)
            {
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
    private void OnPostDelete()
    {
        int index = MessageBox.Query("Delete your profile", "Are you sure?", "No", "Yes");
        if(index == 1)
        {
            this.deleted = true;
            Application.RequestStop();
            this.user.posts = this.userRepository.UserPosts(this.user.id);
            this.user.comments = this.userRepository.UserComments(this.user.id);
            List<Post> postsOfuser = this.user.posts;
            foreach (Post item in postsOfuser)
            {
                item.comments = this.postRepository.CommentsOfPost(item.id, this.commentRepository);
                foreach (Comment comment in item.comments)
                {
                    comment.userId = this.commentRepository.UserID(comment.id);
                    comment.postId = this.commentRepository.PostID(comment.id);
                    User userc = userRepository.GetByID(comment.userId);
                    bool deltecomment = this.commentRepository.Delete(comment, userc, item);
                    MessageBox.ErrorQuery($"Delete commet to post{item.id}", "Can't delete post", "Ok");
                    
                }
                bool deltepost = this.postRepository.Delete(item, this.user);
                MessageBox.ErrorQuery("Delete post of user", "Can't delete post", "Ok");
            }
            List<Comment> commentsOfuser = this.user.comments;
            foreach (Comment item in commentsOfuser)
            {
                item.userId = user.id; 
                item.postId = this.commentRepository.PostID(item.id);
                Post post = this.postRepository.GetByID(item.postId);
                bool deltecomment = this.commentRepository.Delete(item, this.user, post);
                if(!deltecomment)
                {
                    MessageBox.ErrorQuery("Delete comment of user", "Can't delete comment", "Ok");
                }
            }
            bool delete = userRepository.Delete(this.user.id);
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