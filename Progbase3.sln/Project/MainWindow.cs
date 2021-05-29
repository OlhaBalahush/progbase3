using System.Collections;
using Terminal.Gui;
public class MainWindow: Window
{
    private User user;
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private Label welcome;
    private Toplevel top;
    private Rect frame;
    private int pos = 4;
    public MainWindow(Toplevel top, User user, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.user = user;
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.top = top;
        this.Title = "Social Network";
        frame = new Rect(0, 0, top.Frame.Width, top.Frame.Height);

        MenuBar winMenu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_Export", "", OnExportClicked),
                new MenuItem ("_Import", "", OnImportClicked),
                new MenuItem ("_Exit", "", OnExit)
            }),
            new MenuBarItem ("_Help", new MenuItem [] {
                new MenuItem ("_About", "", OnAbout),
            }),
        });
        this.Add(winMenu);
        
        welcome = new Label(pos,4, $"Welcome, {user.username}");
        this.Add(welcome);

        Button ProfileBtn = new Button(pos,6,"My profile");
        ProfileBtn.Clicked += OnProfileBtnClicked;
        this.Add(ProfileBtn);

        Button viewAllUsersBtn = new Button(pos,8,"View users");
        viewAllUsersBtn.Clicked += OnViewUsersBtnClicked;
        this.Add(viewAllUsersBtn);

        Button viewAllPostsBtn = new Button(pos,10,"View posts");
        viewAllPostsBtn.Clicked += OnViewPostsBtnClicked;
        this.Add(viewAllPostsBtn);

        Button createNewPostBtn = new Button(pos,12,"Create post");
        createNewPostBtn.Clicked += OnCreateNewPostBtnClicked;
        this.Add(createNewPostBtn);
    }
    private void OnProfileBtnClicked()
    {
        OpenProfileDialog dialog = new OpenProfileDialog(this.user, this.userReposytory, this.postReposytory, this.commentReposytory);
        //dialog.SetUser(this.user, this.user.posts);
        Application.Run(dialog);
        this.user = dialog.GetUser();
        welcome.Text = $"Welcome, {this.user.username}";
        if(dialog.deleted)
        {
            Authentication registration = new Authentication(this.top, userReposytory, postReposytory, commentReposytory);
            Application.Top.Add(registration);
            Application.Run();
        }
    }
    private void OnViewUsersBtnClicked()
    {
        ViewUsersDialog dialog = new ViewUsersDialog(this.user, this.userReposytory, this.postReposytory, this.commentReposytory);
        //dialog.SetUser(this.user, this.user.posts);
        Application.Run(dialog);
    }
    private void OnViewPostsBtnClicked()
    {
        ViewPostsDialog dialog = new ViewPostsDialog(this.user, this.userReposytory, this.postReposytory, this.commentReposytory);
        //dialog.SetUser(this.user, this.user.posts);
        Application.Run(dialog);
    }
    private void OnCreateNewPostBtnClicked()
    {
        CreatePostDialog dialog = new CreatePostDialog();
        dialog.SetAutor(this.user);
        Application.Run(dialog);

        if(!dialog.canceled)
        {
            if(dialog.GetPost() != null)
            {
                Post post = dialog.GetPost();
                this.postReposytory.Insert(post, this.user);
                user.posts.Add(post);
            }
        }
    }
    private void OnExportClicked()
    {
        ExportWindow dialog  = new ExportWindow(this.postReposytory);
        Application.Run(dialog);
    }
    private void OnImportClicked(){}
    private void OnAbout(){}
    private void OnExit()
    {
        Application.RequestStop();
    }
}