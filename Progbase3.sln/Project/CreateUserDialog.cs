using System;
using Terminal.Gui;
using AccessDataLib;
public class CreateUserDialog: Dialog
{
    protected bool update = false;
    public bool canceled;
    protected TextField usernameInput;
    protected TextField passwordInput;
    public CreateUserDialog()
    {
        this.Title = "Create user";
        
        Button okBtn = new Button("Ok");
        okBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(okBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);

        int rightColumnX = 20;

        Label usernameLbl = new Label(2,2,"Username");
        usernameInput = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(usernameLbl),
            Width = 40,
        };
        this.Add(usernameLbl, usernameInput);

        Label passwordLbl = new Label(2,4,"Password");
        passwordInput = new TextField ("")
        {
            X = rightColumnX,
            Y = Pos.Top(passwordLbl),
            Width = 40,
        };
        this.Add(passwordLbl, passwordInput);
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
    private void OnCreateDialogSubmit()
    {
        if(usernameInput.Text.ToString() == "" || passwordInput.Text.ToString() == "")
        {
            MessageBox.ErrorQuery("Error","String can't be null","ok");
            return;
        }
        this.canceled = false;
        Application.RequestStop();
    }
    public User GetUser()
    {
        string username = usernameInput.Text.ToString();
        string password = passwordInput.Text.ToString();
        if(username != "" && password != "")
        {
            User user = new User(username, password, DateTime.Now.ToString());
            user.moderator = false;
            return user;
        }
        return null;
    }
}