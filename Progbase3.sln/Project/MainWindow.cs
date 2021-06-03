using System.Collections;
using Terminal.Gui;
using AccessDataLib;
public class MainWindow: Window
{
    private User user;
    private UserRepository userRepository;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
    private Label welcome;
    private Toplevel top;
    private Rect frame;
    private int pos = 4;
    public MainWindow(Toplevel top, User user, UserRepository userRepository, PostRepository postRepository, CommentRepository commentRepository)
    {
        this.user = user;
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
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
        OpenProfileDialog dialog = new OpenProfileDialog(this.user, this.user, this.userRepository, this.postRepository, this.commentRepository);
        //dialog.SetUser(this.user, this.user.posts);
        Application.Run(dialog);
        this.user = dialog.GetUser();
        welcome.Text = $"Welcome, {this.user.username}";
        if(dialog.deleted)
        {
            AuthenticationWindow registration = new AuthenticationWindow(this.top, userRepository, postRepository, commentRepository);
            Application.Top.Add(registration);
            Application.Run();
        }
    }
    private void OnViewUsersBtnClicked()
    {
        ViewUsersDialog dialog = new ViewUsersDialog(this.user, this.userRepository, this.postRepository, this.commentRepository);
        //dialog.SetUser(this.user, this.user.posts);
        Application.Run(dialog);
    }
    private void OnViewPostsBtnClicked()
    {
        ViewPostsDialog dialog = new ViewPostsDialog(this.user, this.userRepository, this.postRepository, this.commentRepository);
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
                this.postRepository.Insert(post, this.user);
                user.posts.Add(post);
            }
        }
    }
    private void OnExportClicked()
    {
        ExportWindow dialog  = new ExportWindow(this.postRepository, this.commentRepository);
        Application.Run(dialog);
    }
    private void OnImportClicked()
    {
        ImportWindow dialog = new ImportWindow(this.postRepository, this.commentRepository, this.userRepository);
        Application.Run(dialog);
    }
    private void OnAbout(){}
    private void OnExit()
    {
        Application.RequestStop();
    }
}