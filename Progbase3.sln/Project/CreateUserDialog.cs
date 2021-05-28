using System;
using Terminal.Gui;
public class CreateUserDialog: Dialog
{
    protected bool update = false;
    public bool canceled;
    protected TextField usernameInput;
    protected CheckBox moderatorCheck;
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

        Label moderatorLbl = new Label(2,4,"Moderator");
        moderatorCheck = new CheckBox ("")
        {
            X = rightColumnX,
            Y = Pos.Top(moderatorLbl),
            Width = 40,
        };
        this.Add(moderatorLbl, moderatorCheck);

        Label passwordLbl = new Label(2,6,"Password");
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
        this.canceled = false;
        Application.RequestStop();
    }
    public User GetUser()
    {
        string username = usernameInput.Text.ToString();
        int moderator = 0;
        if(moderatorCheck.Checked == true)
        {
            moderator = 1;
        }
        string password = passwordInput.Text.ToString();
        if(username != "" && password != "")
        {
            return new User(username, moderator, password, DateTime.Now.ToString());
        }
        return null;
    }
}