using System.Collections;
using Terminal.Gui;
using System;
using System.Security.Cryptography;
using System.Text;
public class Authentication: Dialog
{
    private UserReposytory userReposytory;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    protected TextField usernameInput;
    protected TextField passwordInput;
    private Toplevel top;
    private User user;
    public Authentication(Toplevel top, UserReposytory userReposytory, PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.top = top;
        this.userReposytory = userReposytory;
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
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
        if(usernameInput.Text.ToString() == "" || passwordInput.Text.ToString() == "")
        {
            MessageBox.ErrorQuery("Error","Enter data","Ok");
            return;
        }
        this.user = userReposytory.GetByUsername(usernameInput.Text.ToString());
        if(user == null)
        {
            MessageBox.ErrorQuery("Error","The user with the name entered does not exist, please try again","Ok");
            usernameInput.Text = "";
            passwordInput.Text = "";
            return;
        }
        if(user.password != this.GetHashPassword(passwordInput.Text.ToString()))
        {
            MessageBox.ErrorQuery("Error","The password is entered incorrectly, please try again","Ok");
            passwordInput.Text = "";
            return;
        }
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
                if(userReposytory.GetByUsername(this.user.username) != null)
                {
                    MessageBox.ErrorQuery("Error","User with such a username already exists, please try again","Ok");
                    return;
                }
                user.password = this.GetHashPassword(user.password);
                long userId = userReposytory.Insert(this.user);
                this.user.id = userId;
            }
        }
        GoToMainWindow();
        OnExit();
    }
    // public User GetCurrentUser()
    // {
    //     return this.user;
    // }
    private string GetHashPassword(string source)
    {
        SHA256 sha256Hash = SHA256.Create();
        string hash = GetHash(sha256Hash, source);

        // Console.WriteLine($"The SHA256 hash of {source} is: {hash}.");

        // Console.WriteLine("Verifying the hash...");

        // if (VerifyHash(sha256Hash, source, hash))
        // {
        //     //Console.WriteLine("The hashes are the same.");
        // }
        // else
        // {
        //     //Console.WriteLine("The hashes are not same.");
        // }
 
        // sha256Hash.Dispose();
        return hash;
    }
    private string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    private bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
    {
        var hashOfInput = GetHash(hashAlgorithm, input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
    
        return comparer.Compare(hashOfInput, hash) == 0;
    }
    private void GoToMainWindow()
    {
        Application.Init();

        MainWindow window = new MainWindow(this.top, this.user, this.userReposytory, this.postReposytory, this.commentReposytory);
        Application.Top.Add(window);

        Application.Run();
    }
    private void OnExit()
    {
        Application.RequestStop();
    }
}