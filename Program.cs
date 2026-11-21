using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new LoginForm());
    }
}

// Database helpers
static class Db
{
    public static SqlConnection Open()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["LoginDBConnectionString"].ConnectionString;
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public static DataTable Table(string sql, params SqlParameter[] parameters)
    {
        using (var connection = Open())
        {
            var adapter = new SqlDataAdapter(sql, connection);
            adapter.SelectCommand.Parameters.AddRange(parameters);
            var data = new DataTable();
            adapter.Fill(data);
            return data;
        }
    }

    public static void Run(string sql, params SqlParameter[] parameters)
    {
        using (var connection = Open())
        {
            var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddRange(parameters);
            cmd.ExecuteNonQuery();
        }
    }

    public static void Try(Action action)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
    }
}

class LoginForm : Form
{
    TextBox txtUsername = new TextBox { Location = new Point(330, 200), Width = 160 };
    TextBox txtPassword = new TextBox { Location = new Point(330, 240), Width = 160, UseSystemPasswordChar = true };

    public LoginForm()
    {
        Text = "Login - Developed by NGUYEN GIA HUY";
        ClientSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        var btnLogin = new Button { Text = "Login", Location = new Point(330, 290), Size = new Size(75, 30) };
        var btnCancel = new Button { Text = "Cancel", Location = new Point(415, 290), Size = new Size(75, 30) };
        btnLogin.Click += btnLogin_Click;
        btnCancel.Click += (s, e) => Close();
        AcceptButton = btnLogin;

        Controls.AddRange(new Control[]
        {
            new Label { Text = "Username", AutoSize = true, Location = new Point(250, 203) },
            new Label { Text = "Password", AutoSize = true, Location = new Point(250, 243) },
            txtUsername, txtPassword, btnLogin, btnCancel
        });
    }

    void btnLogin_Click(object sender, EventArgs e)
    {
        bool success = false;
        try
        {
            using (var connection = Db.Open())
            {
                string query = "SELECT COUNT(1) FROM tblUsers WHERE Username=@Username AND Password=@Password";
                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                success = count == 1;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            return;
        }

        if (success)
        {
            MessageBox.Show("Login Successful!");
            Hide();
            using (var main = new MainForm()) main.ShowDialog();
            Close();
        }
        else
        {
            MessageBox.Show("Login failed. Please check your username and password.");
        }
    }
}

class MainForm : Form
{
    public MainForm()
    {
        Text = "MainForm - Developed by NGUYEN GIA HUY";
        ClientSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        var menu = new MenuStrip();
        menu.Items.Add(Menu("File", "Exit", () => Close()));
        menu.Items.Add(Menu("Students", "View Students", () => Pages.Students().Show()));
        menu.Items.Add(Menu("Courses", "View Courses", () => Pages.Courses().Show()));
        menu.Items.Add(Menu("Instructors", "View Instructors", () => Pages.Instructors().Show()));
        menu.Items.Add(Menu("Departments", "View Departments", () => Pages.Departments().Show()));

        var students = (ToolStripMenuItem)menu.Items[1];
        var enrollments = new ToolStripMenuItem("Student Enrollments");
        enrollments.Click += (s, e) => new EnrollmentsForm().Show();
        students.DropDownItems.Add(enrollments);

        MainMenuStrip = menu;
        Controls.Add(menu);
    }

    static ToolStripMenuItem Menu(string title, string itemText, Action onClick)
    {
        var top = new ToolStripMenuItem(title);
        var item = new ToolStripMenuItem(itemText);
        item.Click += (s, e) => onClick();
        top.DropDownItems.Add(item);
        return top;
    }
}
