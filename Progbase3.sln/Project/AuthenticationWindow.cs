using System.Collections;
using Terminal.Gui;
using System;
using System.Security.Cryptography;
using System.Text;
using AccessDataLib;
public class AuthenticationWindow: Window
{
    private UserRepository userRepository;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
    protected TextField usernameInput;
    protected TextField passwordInput;
    private Toplevel top;
    private User user;
    public AuthenticationWindow(Toplevel top, UserRepository userRepository, PostRepository postRepository, CommentRepository commentRepository)
    {
        this.top = top;
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.Title = "";
        Rect frame = new Rect(0, 0, top.Frame.Width, top.Frame.Height);

        MenuBar menu = new MenuBar(new MenuBarItem[] {
           new MenuBarItem ("_File", new MenuItem [] {
               new MenuItem ("_Exit", "", OnExit)
           }),
        });
       
        int rightColumnX = 20;

        Label usernameLbl = new Label(2,2,"Username:");
        usernameInput = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(usernameLbl),
            Width = 40,
        };
        this.Add(usernameLbl, usernameInput);

        Label passwordLbl = new Label(2,4,"Password:");
        passwordInput = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(passwordLbl),
            Width = 40,
            Secret = true,
        };

        this.Add(passwordLbl, passwordInput);

        Button LogInBtn = new Button(2,6,"Log in")
        {
            X = rightColumnX,
            Y = Pos.Bottom(passwordLbl),
        };
        LogInBtn.Clicked += OnLogInBtnClicked;
        this.Add(LogInBtn);

        Button SignUpBtn = new Button(2,8,"Sign up")
        {
            X = rightColumnX,
            Y = Pos.Bottom(LogInBtn),
        };
        SignUpBtn.Clicked += OnSignUpBtnClicked;
        this.Add(SignUpBtn);
    }
    private void OnLogInBtnClicked()
    {
        User user = Authentication.LogIn(this.userRepository, usernameInput.Text.ToString(), passwordInput.Text.ToString());
        if(user == null)
        {
            MessageBox.ErrorQuery("Incorrect entered data","try again or sign up","Ok");
            usernameInput.Text = "";
            passwordInput.Text = "";
            return;
        }
        this.user = user;
        GoToMainWindow();
        OnExit();
    }
    private void OnSignUpBtnClicked()
    {
        CreateUserDialog dialog = new CreateUserDialog();
        Application.Run(dialog);
        if(!dialog.canceled)
        {
            if(dialog.GetUser() != null)
            {
                this.user = dialog.GetUser();
                if(user == null)
                {
                    MessageBox.ErrorQuery("Error","User with such a username already exists, please try again","Ok");
                    return;
                }
                //sign up
                if(Authentication.SignUp(this.userRepository, this.user) == null)
                {
                    MessageBox.ErrorQuery("Error","User with such a username already exists, please try again","Ok");
                    return;
                }
            }
            GoToMainWindow();
        }
        else if(dialog.canceled)
        {
            return;
        }
        OnExit();
    }
    private void GoToMainWindow()
    {
        Application.Init();

        MainWindow window = new MainWindow(this.top, this.user, this.userRepository, this.postRepository, this.commentRepository);
        Application.Top.Add(window);

        Application.Run();
    }
    private void OnExit()
    {
        Application.RequestStop();
    }
}