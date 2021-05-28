public class EditProfileDialog: CreateUserDialog
{
    public EditProfileDialog()
    {
        this.Title = "Edit profile";
        passwordInput.Text = "...";
        passwordInput.Visible = false;
        moderatorCheck.Visible = false;
        update = true;
    }
    public void SetUser(User user)
    {
        this.usernameInput.Text = user.username;
    }
}